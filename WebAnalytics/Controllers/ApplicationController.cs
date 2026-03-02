using DataAccessLayerEF;
using Domain.CommonModel;
using Domain.interfaces;
using Domain.Model;
using Domain.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using WebAnalytics.Helpers;

namespace WebAnalytics.Controllers
{
    /// <summary>
    /// Manages applications (CRUD-like endpoints) and session lifecycle
    /// (start, end, keep-alive/active) per application.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class ApplicationController : ControllerBase
    {
        private readonly IApplication _iapplication;
        private readonly IApplicationSessions _applicationSessions;
        private readonly IApplicationHitsUserTrack _applicationHitsUserTrack;
        private readonly ICentralAnalyticsService _analyticsService;
        private readonly ICentralAnalyticsServiceSummary _centralAnalyticsServiceSummary;
        /// <summary>
        /// DI constructor for <see cref="ApplicationController"/>.
        /// </summary>
        /// <param name="iapplication">Application service for app metadata and keys.</param>
        /// <param name="applicationSessions">Service for managing app user sessions.</param>
        public ApplicationController(IApplication iapplication, IApplicationSessions applicationSessions, IApplicationHitsUserTrack applicationHitsUserTrack, ICentralAnalyticsService centralAnalyticsService, ICentralAnalyticsServiceSummary centralAnalyticsServiceSummary)
        {
            _iapplication = iapplication;
            _applicationSessions = applicationSessions;
            _applicationHitsUserTrack = applicationHitsUserTrack;
            _analyticsService = centralAnalyticsService;
            _centralAnalyticsServiceSummary = centralAnalyticsServiceSummary;
        }
       
        /// <summary>
        /// Starts (or re-activates) a session for a user against the given application key.
        /// Also mirrors the session row into a session mirror table on re-activation.
        /// </summary>
        /// <param name="ApplicationKey">Unique app key to bind the session to.</param>
        /// <param name="DomainId">
        /// Optional caller/user identifier. If null/empty, the remote IP is used.
        /// </param>
        /// <returns>
        /// 200 OK with created/updated session and the resolved <c>UserId</c>;  
        /// 400 BadRequest if the key is invalid or origin mismatch (handled in service).
        /// </returns>
        [HttpPost("ApplicationSessionStart")]
        public async Task<IActionResult> ApplicationSessionStart([FromHeader(Name = "X-API-KEY")] string ApplicationKey, string? DomainId,
    CancellationToken cancellationToken)
        {
            // Resolve user id as DomainId or fallback to remote IP.
            var UserId = HttpContext.Connection.RemoteIpAddress?.ToString();
            if (string.IsNullOrEmpty(DomainId))
            {
                DomainId = UserId;
            }

            // Validate origin-bound key.
            var origin = Request.Headers["Origin"].FirstOrDefault();
            var Application = await _iapplication.CheckApplicationKey(ApplicationKey, origin);
            if (Application != null && Application.ApplicationId > 0)
            {
                var mApplicationSessions = new MApplicationSessions
                {
                    ApplicationId = Application.ApplicationId,
                    IpAddress = UserId,
                    SessionStartTime = DateTime.Now,
                    IsActive = true,
                    LastUpdated = DateTime.Now
                };

                // If a session exists for this (ApplicationKey, DomainId), toggle active & mirror.
                var ret = await _applicationSessions.GetApplicationBySessions(ApplicationKey, UserId);
                if (ret != null && ret.ApplicationSessionsId > 0)
                {
                    ret.IsActive = true;
                    ret.LastUpdated = DateTime.Now;

                    var result = await _applicationSessions.UpdateWithReturn(ret);

                    // Mirror row for audit/history.
                    var mirrorSession = new MApplicationSessionMirror
                    {
                        ApplicationSessionsId = result.ApplicationSessionsId,
                        ApplicationId = result.ApplicationId,
                        SessionStartTime = result.SessionStartTime,
                        SessionEndTime = result.SessionEndTime ?? DateTime.MinValue,
                        IsActive = result.IsActive,
                        IpAddress = UserId,
                        LastUpdated = result.LastUpdated,
                        MirrorOn = DateTime.Now
                    };
                    var mirror = await _applicationSessions.AddWithMirror(mirrorSession);


                    MApplicationHitsUserTrack applicationHitsUserTrack = new MApplicationHitsUserTrack
                    {
                        ApplicationId = Application.ApplicationId,
                        HitDate = DateTime.Now,
                        IpAddress = UserId // Store the IP address of the user
                    };
                    var ishitexits = await _applicationHitsUserTrack.CheckHitIsToday(applicationHitsUserTrack);
                    if (!ishitexits)
                    {
                        await _analyticsService.IncrementHitCounter(Application.ApplicationId);
                        await _centralAnalyticsServiceSummary.IncrementHitCounter(Application.ApplicationId);
                        await _applicationHitsUserTrack.Add(applicationHitsUserTrack);

                        
                    }

                    return Ok(new { message = "SessionStart successfully" });
                }
                else
                {
                    // No existing session — create new.
                    var result = await _applicationSessions.AddWithReturn(mApplicationSessions);
                    return Ok(new { message = "Session Update successfully" });
                }
            }
            else
            {
                return BadRequest("Invalid Application Key Or Orign");
            }
        }

        /// <summary>
        /// Ends the active session for the given application key and user (DomainId/IP).
        /// </summary>
        /// <param name="ApplicationKey">Unique application key.</param>
        /// <param name="DomainId">
        /// Optional user identifier; if omitted, remote IP is used to locate the session.
        /// </param>
        /// <returns>
        /// 200 OK with updated session record,  
        /// 404 NotFound if no session exists,  
        /// 400 BadRequest for invalid key/origin.
        /// </returns>
        [HttpPost("ApplicationSessionEnd")]
        public async Task<IActionResult> ApplicationSessionEnd([FromHeader(Name = "X-API-KEY")] string ApplicationKey, string? DomainId)
        {
            var UserId = HttpContext.Connection.RemoteIpAddress?.ToString();
            if (string.IsNullOrEmpty(DomainId))
            {
                DomainId = UserId;
            }

            var origin = Request.Headers["Origin"].FirstOrDefault();
            var Application = await _iapplication.CheckApplicationKey(ApplicationKey, origin);
            if (Application != null && Application.ApplicationId > 0)
            {
                var ret = await _applicationSessions.GetApplicationBySessions(ApplicationKey, DomainId);
                if (ret != null && ret.ApplicationSessionsId > 0)
                {
                    ret.SessionEndTime = DateTime.Now;
                    ret.IsActive = false;
                    ret.LastUpdated = DateTime.Now;

                    var result = await _applicationSessions.UpdateWithReturn(ret);
                    return Ok(new { message = "Session End successfully" });
                }
                else
                {
                    return NotFound("Session not found for the given Application Key and User ID.");
                }
            }
            else
            {
                return BadRequest("Invalid Application Key Or Orign");
            }
        }

      
    }
}
