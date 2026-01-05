using DataAccessLayerEF;
using Domain.Identitytable;
using Domain.interfaces;
using Domain.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebAnalytics.Models;
using static System.Net.Mime.MediaTypeNames;

namespace WebAnalytics.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IApplicationSessions _applicationSessions;
        private readonly IApplication _application;
        private readonly IConfiguration _configuration;
        private readonly ICentralAnalyticsService _analyticsService;
        private readonly ICentralAnalyticsServiceSummary _centralAnalyticsServiceSummary;
        private readonly IApplicationHitsUserTrack _applicationHitsUserTrack;
        private readonly IAnalyticsSummary _analyticsSummary;
        private readonly UserManager<ApplicationUser> _userManager;
        public HomeController(ICentralAnalyticsService analyticsService, IApplication application, IApplicationHitsUserTrack applicationHitsUserTrack, IApplicationSessions applicationSessions, ICentralAnalyticsServiceSummary centralAnalyticsServiceSummary, IAnalyticsSummary analyticsSummary, IConfiguration configuration, UserManager<ApplicationUser> userManager)
        {
            _analyticsService = analyticsService;
            _application = application;
            _applicationHitsUserTrack = applicationHitsUserTrack;
            _applicationSessions = applicationSessions;
            _centralAnalyticsServiceSummary = centralAnalyticsServiceSummary;
            _analyticsSummary = analyticsSummary;
            _configuration = configuration;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
           
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        /// <summary>
        /// Retrieves a list of concurrent active users for a given application.
        /// </summary>
        /// <param name="ApplicationId">The unique application identifier.</param>
        /// <returns>
        /// JSON result with concurrent user list for the application.
        /// </returns>
        public async Task<IActionResult> ConcurrentuserList(int ApplicationId)
        {
            var ret = await _applicationSessions.ConcurrentuserList(ApplicationId);
            return Json(ret);
        }

        /// <summary>
        /// Retrieves a date-specific concurrent user list for a given application.
        /// </summary>
        /// <param name="ApplicationName">The name of the application.</param>
        /// <param name="date">Date to filter the concurrent user list by.</param>
        /// <returns>
        /// - JSON result with concurrent user list if the application is found.  
        /// - JSON with message <c>"Application not found"</c> if lookup fails.
        /// </returns>
        public async Task<IActionResult> ConcurrentuserListDatewise(string ApplicationName, DateTime date)
        {
            var data = await _application.GetApplicationByName(ApplicationName);
            if (data != null)
            {
                var ret = await _applicationSessions.ConcurrentuserListDatewise(data.ApplicationId, date);
                return Json(ret);
            }
            else
            {
                return Json(new { message = "Application not found" });
            }
        }
        public async Task<IActionResult> GetDataSummary([FromBody]  ApplicationKeyRequest Data)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized("User is not authenticated.");

            var roles = await _userManager.GetRolesAsync(user);
            var roleName = roles.FirstOrDefault()?.ToLower();

            if (string.IsNullOrEmpty(roleName))
                return Forbid("User role is not assigned.");

            // 🔹 ADMIN → can view ALL data
            if (roleName == "admin" && Data.ApplicationKey=="0")
            {
                var adminResult = await _analyticsSummary.GetDataSummary(0,0,Data.Month,Data.Years);
                return Ok(adminResult);
            }
            else if(Data.ApplicationKey == "0")
            {
                var adminResult = await _analyticsSummary.GetDataSummary(0,user.Id, Data.Month, Data.Years);
                return Ok(adminResult);
            }

                // 🔹 USER → must own the application
                var application = await _application.CheckApplicationKey(Data.ApplicationKey);

            if (application == null)
                return NotFound("Application key does not exist.");

            if (application.CreatedBy != user.Id && roleName != "admin")
                return Forbid("You are not authorized to access this application data.");
            if (roleName == "admin")
            {
                var result = await _analyticsSummary.GetDataSummary(application.ApplicationId, 0, Data.Month, Data.Years);
                return Ok(result);
            }
            else
            {
                var result = await _analyticsSummary.GetDataSummary(application.ApplicationId, user.Id, Data.Month, Data.Years);
                return Ok(result);
            }



        }
       
        public async Task<IActionResult> AllConcurrentuser([FromBody] ApplicationKeyRequest Data)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized("User is not authenticated.");

            var roles = await _userManager.GetRolesAsync(user);
            var roleName = roles.FirstOrDefault()?.ToLower();
            if (string.IsNullOrEmpty(roleName))
                return Forbid("User role is not assigned.");
            if (roleName == "admin" && Data.ApplicationKey == "0")
            {
              
                return Ok(await _applicationSessions.Concurrentuser());
            }
            else if (roleName == "user" && Data.ApplicationKey == "0")
            {
                return Ok(await _applicationSessions.ConcurrentuserByUserId(user.Id));
                
            }
                var application = await _application.CheckApplicationKey(Data.ApplicationKey);
            if (application == null)
                return NotFound("Application key does not exist.");
            if (application.CreatedBy != user.Id && roleName != "admin")
                return Forbid("You are not authorized to access this application data.");
            var All = await _applicationSessions.ConcurrentuserForDashboardByApplicationId(application.ApplicationId);

            return Ok(All);

            
            
        }
    }
}
