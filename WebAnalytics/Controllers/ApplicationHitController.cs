using Domain.CommonModel;
using Domain.interfaces;
using Domain.Requests;
using Domain.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebAnalytics.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationHitController : ControllerBase
    {
        private readonly ICentralAnalyticsService _analyticsService;
        private readonly ICentralAnalyticsServiceSummary _centralAnalyticsServiceSummary;
        private readonly IApplication _iapplication;
        private readonly IApplicationHitsUserTrack _applicationHitsUserTrack;
        private readonly IApplicationSessions _applicationSessions;
       
       
        public ApplicationHitController(ICentralAnalyticsService analyticsService, IApplication application, IApplicationHitsUserTrack applicationHitsUserTrack, IApplicationSessions applicationSessions, ICentralAnalyticsServiceSummary centralAnalyticsServiceSummary)
        {
            _analyticsService = analyticsService;
            _iapplication = application;
            _applicationHitsUserTrack = applicationHitsUserTrack;
            _applicationSessions = applicationSessions;
            _centralAnalyticsServiceSummary = centralAnalyticsServiceSummary;
           
            
        }

        #region For User

        /// <summary>
        /// Increments the hit counter for a given ApplicationKey.
        /// Prevents duplicate increments by checking if the user already hit today.
        /// Tracks UserId (DomainId or IP) and IP address.
        /// </summary>
        [HttpPost("IncrementHits")]
        public async Task<IActionResult> IncrementHits([FromHeader(Name = "X-API-KEY")] string ApplicationKey, string? DomainId)
        {
            if (string.IsNullOrEmpty(ApplicationKey))
                return BadRequest("X-API-KEY header missing");

            var origin = Request.Headers["Origin"].FirstOrDefault();   // e.g. https://clientapp.com
            // Get the IP address of the caller
            var IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            // Get the IP address of the caller
            if (string.IsNullOrEmpty(DomainId))
            {
                DomainId = IpAddress;
            }

            var Application = await _iapplication.CheckApplicationKey(ApplicationKey, origin);
            if (Application != null && Application.ApplicationId > 0)
            { // Optionally, you can track the user hit if needed
                MApplicationHitsUserTrack applicationHitsUserTrack = new MApplicationHitsUserTrack
                {
                    ApplicationId = Application.ApplicationId,
                    UserId = DomainId,
                    HitDate = DateTime.Now,
                    IpAddress = IpAddress // Store the IP address of the user
                };
                var ishitexits = await _applicationHitsUserTrack.CheckHitIsToday(applicationHitsUserTrack);
                if (!ishitexits)
                {
                    await _analyticsService.IncrementHitCounter(Application.ApplicationId);
                    await _centralAnalyticsServiceSummary.IncrementHitCounter(Application.ApplicationId);
                    await _applicationHitsUserTrack.Add(applicationHitsUserTrack);

                    return Ok(new { message = "Hits incremented successfully" });
                }
                else
                {
                    return Ok("Duplicate request");
                }
            }
            else
            {
                return BadRequest("ApplicationKey Not Found");
            }
        }
        /// <summary>
        /// Gets hit summary (today, month, total) for a specific application.
        /// </summary>
        [HttpPost("Hits")]
        public async Task<IActionResult> Hits([FromHeader(Name = "X-API-KEY")]  string ApplicationKey)
        {
            if (string.IsNullOrEmpty(ApplicationKey))
                return BadRequest("X-API-KEY header missing");
            var origin = Request.Headers["Origin"].FirstOrDefault();   // e.g. https://clientapp.com
            var Application = await _iapplication.CheckApplicationKey(ApplicationKey, origin);
            if (Application != null && Application.ApplicationId > 0)
            {

                var res = await _analyticsService.HitCounter(Application.ApplicationId);
                return Ok(res);
            }
            else
            {
                return BadRequest("ApplicationKey Not Found");
            }
        }
        /// <summary>
        /// Gets hit summary (today, month, total) + concurrent users for a specific app.
        /// </summary>
        // Endpoint to increment hit counter for a specific app
        [HttpPost("HitswithConcurrentuser")]
        public async Task<IActionResult> HitswithConcurrentuser([FromHeader(Name = "X-API-KEY")]  string ApplicationKey)
        {
            if (string.IsNullOrEmpty(ApplicationKey))
                return BadRequest("X-API-KEY header missing");
            var origin = Request.Headers["Origin"].FirstOrDefault();   // e.g. https://clientapp.com
            DTOHitsWithActiveUserResponse dTOHitsWithActiveUserResponse = new DTOHitsWithActiveUserResponse();
            var Application = await _iapplication.CheckApplicationKey(ApplicationKey, origin);
            if (Application != null && Application.ApplicationId > 0)
            {
                // Increment the hit counter for the application
                var activeUser = await _applicationSessions.Concurrentuser(Application.ApplicationId);
                var res = await _analyticsService.HitCounter(Application.ApplicationId);
                if (res != null)
                {
                    dTOHitsWithActiveUserResponse.MonthlyHits = res.MonthlyHits;
                    dTOHitsWithActiveUserResponse.TodayHits = res.TodayHits;
                    dTOHitsWithActiveUserResponse.TotalHits = res.TotalHits;
                    dTOHitsWithActiveUserResponse.Concurrentuser = activeUser.Concurrentuser;
                }
                return Ok(dTOHitsWithActiveUserResponse);
            }
            else
            {
                return BadRequest("ApplicationKey Not Found");
            }
        }
        #endregion


        
       
    }
}
