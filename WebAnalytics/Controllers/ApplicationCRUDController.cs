using Domain.Constants;
using Domain.Identitytable;
using Domain.interfaces;
using Domain.Model;
using Domain.Requests;
using Domain.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebAnalytics.Helpers;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WebAnalytics.Controllers
{
    [Authorize]
    public class ApplicationCRUDController : Controller
    {
        private readonly IApplication _iapplication;
        private readonly UserManager<ApplicationUser> _userManager;
        public ApplicationCRUDController(IApplication application, UserManager<ApplicationUser> userManager)
        {
            _iapplication = application;
            _userManager = userManager;

        }
        public IActionResult Index()
        {
           
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> GetAll() => Ok(await _iapplication.GetAll());
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddApplication(DTORequestApplicationDetails application)
        {
            if (!ModelState.IsValid)
                return InvalidModelStateResponse();

            if (application == null)
                return BadRequest("Invalid application data.");

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized("User is not authenticated.");

            var userId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var roleName = await GetUserRoleAsync(user);

            if (application.ApplicationId == 0)
                return await CreateApplicationAsync(application, userId);

            return await UpdateApplicationAsync(application, userId, roleName);
        }
        private IActionResult InvalidModelStateResponse()
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors?.Count > 0)
                .SelectMany(x => x.Value!.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return Json(new DTOGenericResponse<object>(
                ConnKeyConstants.IncorrectData,
                ConnKeyConstants.IncorrectDataMessage,
                errors));
        }
        private async Task<string?> GetUserRoleAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            return roles.FirstOrDefault()?.ToLower();
        }
        private async Task<IActionResult> CreateApplicationAsync(
    DTORequestApplicationDetails application,
    int userId)
        {
            var existing = await _iapplication
                .GetApplicationByName(application.ApplicationName);

            if (existing != null)
                return BadRequest("Application with this name already exists.");

            var entity = new MApplicationDetails
            {
                ApplicationKey = Guid.NewGuid().ToString(),
                ApplicationName = application.ApplicationName,
                Description = application.Description,
                CreatedOn = DateTime.Now,
                UpdatedOn = DateTime.Now,
                CreatedBy = userId,
                UpdatedBy = userId,
                IsActive = true,
                IsDeleted = false,
                origin = application.origin,
                ColorCode = ColorHelper.GetRandomHexColor()
            };

            await _iapplication.AddWithReturn(entity);

            return Json(new DTOGenericResponse<object>(
                ConnKeyConstants.Success,
                ConnKeyConstants.SaveMessage,
                null));
        }
        private async Task<IActionResult> UpdateApplicationAsync(
    DTORequestApplicationDetails application,
    int userId,
    string? roleName)
        {
            var data = await _iapplication.Get(application.ApplicationId);

            data.ApplicationName = application.ApplicationName;
            data.Description = application.Description;
            data.UpdatedOn = DateTime.Now;
            data.origin = application.origin;
            data.UpdatedBy = userId;

            if (!IsAuthorizedToUpdate(data, userId, roleName))
                return BadRequest("Your Not Authorize user for this application.");

            await _iapplication.UpdateWithReturn(data);

            return Json(new DTOGenericResponse<object>(
                ConnKeyConstants.Success,
                ConnKeyConstants.UpdateMessage,
                null));
        }
        private bool IsAuthorizedToUpdate(
            MApplicationDetails data,
            int userId,
            string? roleName)
        {
            return data.CreatedBy == userId || roleName == "admin";
        }


        [HttpPost]
        public async Task<IActionResult> GetApplication()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized("User is not authenticated.");

            var roles = await _userManager.GetRolesAsync(user);
            var roleName = roles.FirstOrDefault()?.ToLower();
            if(roleName=="admin")
                return Ok(await _iapplication.GetAll());

            return Ok(await _iapplication.GetApplication(Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier))));
        }

    }
}
