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
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return Unauthorized("User is not authenticated.");

                var roles = await _userManager.GetRolesAsync(user);
                var roleName = roles.FirstOrDefault()?.ToLower();

                if (application == null)
                {
                    return BadRequest("Invalid application data.");
                }

                if (application.ApplicationId == 0)
                {
                    // Create
                    var applicationexits = await _iapplication.GetApplicationByName(application.ApplicationName);
                    if (applicationexits != null)
                    {
                        return BadRequest("Application with this name already exists.");
                    }
                   
                    var mApplicationDetails = new MApplicationDetails
                    {
                        ApplicationKey = Guid.NewGuid().ToString(), // Generate unique key
                        ApplicationName = application.ApplicationName,
                        Description = application.Description,
                        CreatedOn = DateTime.Now,
                        UpdatedOn = DateTime.Now,
                        CreatedBy = Convert.ToInt32(userId), // TODO: Replace with current user id/claim
                        UpdatedBy = Convert.ToInt32(userId), // TODO: Replace with current user id/claim
                        IsActive = true,
                        IsDeleted = false,
                        origin = application.origin,
                        ColorCode = ColorHelper.GetRandomHexColor()
                    };
                    await _iapplication.AddWithReturn(mApplicationDetails);
                    return Json(new DTOGenericResponse<object>(ConnKeyConstants.Success, ConnKeyConstants.SaveMessage, null));
                    
                }
                else
                {
                    // Update
                    var Data = await _iapplication.Get(application.ApplicationId);
                    Data.ApplicationName = application.ApplicationName;
                    Data.Description = application.Description;
                    Data.UpdatedOn = DateTime.Now;
                    Data.origin = application.origin;
                    Data.UpdatedBy = Convert.ToInt32(userId);

                    if(Convert.ToInt32(userId)==Data.CreatedBy || roleName=="admin")
                    {
                        await _iapplication.UpdateWithReturn(Data);
                        return Json(new DTOGenericResponse<object>(ConnKeyConstants.Success, ConnKeyConstants.UpdateMessage, null));
                    }
                    else
                    {
                        return BadRequest("Your Not Authorize user for this application.");
                    }

                   
                   
                }
            }
            else
            {
                var error = ModelState.Where(x => x.Value?.Errors?.Count > 0)
                                               .SelectMany(x => x.Value!.Errors)
                                               .Select(e => e.ErrorMessage)
                                               .ToList();

                return Json(new DTOGenericResponse<object>(ConnKeyConstants.IncorrectData, ConnKeyConstants.IncorrectDataMessage, error));
            }
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
