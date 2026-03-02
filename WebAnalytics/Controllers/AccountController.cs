using Azure.Core;
using Domain.CommonModel;
using Domain.Constants;
using Domain.Identitytable;
using Domain.interfaces;
using Domain.Requests;
using Domain.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebAnalytics.Helpers;
namespace WebAnalytics.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        
        private readonly IRank _rank;
        private readonly IUsers _users;
        public const string SessionKeySalt = "_Salt";
        public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager,IRank rank, IUsers users)
        {
            _signInManager = signInManager;
            _userManager = userManager;
           
           
            _rank=rank;
            _users=users;
        }
        public IActionResult Login()
        {
            var model = new DTOIMLoginRequest
            {
               // UserName = "admin",
               // Password = "Admin@123"
            };
           
            string GetSalt = AESEncrytDecry.GenerateKey();
            HttpContext.Session.SetString(SessionKeySalt, GetSalt);
            ViewBag.hdns = GetSalt;
            return View(model);

        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(DTOIMLoginRequest request)
        {
          
            if (!ModelState.IsValid)
                return View(request);

            DecryptCredentials(request);

            var user = await _userManager.FindByNameAsync(request.UserName);
            if (user == null)
            {
                TempData["UserName"] = request.UserName;
                return RedirectToAction("Register", "Account");
            }

            var result = await _signInManager.PasswordSignInAsync(
                user, request.Password, false, lockoutOnFailure: true);

            if (!result.Succeeded)
                return HandleLoginFailure(result, user, request);

            await CompleteSuccessfulLoginAsync(user);

            if (!user.Active)
                return RedirectToAction("ContactUs", "Account");

            return RedirectToAction("Index", "Home");
        }

        private void DecryptCredentials(DTOIMLoginRequest request)
        {
            var salt = HttpContext.Session.GetString(SessionKeySalt);
            if (salt == null) return;

            ViewBag.hdns = salt;

            request.Password = AESEncrytDecry.DecryptAES(request.Password, salt);
            request.UserName = AESEncrytDecry.DecryptAES(request.UserName, salt);
        }
        private IActionResult HandleLoginFailure(
    Microsoft.AspNetCore.Identity.SignInResult result,
    ApplicationUser user,
    DTOIMLoginRequest request)
{
    if (result.IsLockedOut)
    {
        ModelState.AddModelError(
            string.Empty,
            "Account Locked Out. Please try after 10 minutes.");
    }
    else if (result.IsNotAllowed)
    {
        ModelState.AddModelError(
            string.Empty,
            $"Already Login \"{user.UserName}\". Please try later.");
    }
    else
    {
        ModelState.AddModelError(
            string.Empty,
            $"Not Valid User / Password. Access Failed Count {user.AccessFailedCount} Max Access Attempts 3");
    }

    return View(request);
}
        private async Task CompleteSuccessfulLoginAsync(ApplicationUser user)
        {
            HttpContext.Session.Clear();

            const string sessionCookieName = ".AspNetCore.Session";
            if (Request.Cookies.ContainsKey(sessionCookieName))
            {
                Response.Cookies.Delete(sessionCookieName);
            }

            await _userManager.UpdateSecurityStampAsync(user);

            await _signInManager.SignOutAsync();
            await _signInManager.SignInAsync(user, isPersistent: false);

            var rank = await _rank.GetByshort(user.RankId);

            var sessionDto = new DTOSession
            {
                UserId = user.Id,
                RoleName = string.Join(",", await _userManager.GetRolesAsync(user)),
                UserName = user.UserName,
                Name = user.Name,
                RankName = rank.RankAbbreviation
            };

            SessionHeplers.SetObject(HttpContext.Session, "Users", sessionDto);
        }

        // Registration Action (GET)
        [AllowAnonymous]

        public IActionResult Register()
        {
            string GetSalt = AESEncrytDecry.GenerateKey();
            HttpContext.Session.SetString(SessionKeySalt, GetSalt);
            ViewBag.hdns = GetSalt;
            ViewBag.UserName = TempData["UserName"] as string;
            //if (string.IsNullOrEmpty(TempData["UserName"] as string))
            //    return RedirectToAction("Login");
            return View(new DTORegistraionRequest());
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(DTORegistraionRequest model)
        {
            if (!IsRankValid(model))
                return View(model);

            ViewBag.UserName = model.UserName;

            if (!ModelState.IsValid)
                return View(model);

            DecryptConfirmPassword(model);

            if (await UserExistsAsync(model.UserName))
            {
                ModelState.AddModelError(string.Empty, "User already exists.");
                return View(model);
            }

            var user = CreateUser(model);

            var result = await _userManager.CreateAsync(user, model.ConfirmPassword);
            if (!result.Succeeded)
            {
                AddErrors(result);
                return View(model);
            }

            await _userManager.AddToRoleAsync(user, "User");

            if (!user.Active)
                return RedirectToAction("ContactUs", "Account");

            await CreateUserSessionAsync(user);

            return RedirectToAction("Index", "Home");
        }
        private bool IsRankValid(DTORegistraionRequest model)
        {
            if (model.RankId == null || model.RankId == 0)
            {
                ModelState.AddModelError(string.Empty, "Select Rank.");
                return false;
            }
            return true;
        }
        private void DecryptConfirmPassword(DTORegistraionRequest model)
        {
            var salt = HttpContext.Session.GetString(SessionKeySalt);
            if (salt == null) return;

            ViewBag.hdns = salt;
            model.ConfirmPassword = AESEncrytDecry
                .DecryptAES(model.ConfirmPassword, salt);
        }
        private async Task<bool> UserExistsAsync(string userName)
        {
            return await _userManager.FindByNameAsync(userName) != null;
        }
        private ApplicationUser CreateUser(DTORegistraionRequest model)
        {
            int userId = Convert.ToInt32(
                User.FindFirstValue(ClaimTypes.NameIdentifier));

            return new ApplicationUser
            {
                RankId = model.RankId,
                Name = model.Name,
                Active = false,
                Updatedby = userId,
                UpdatedOn = TimeZoneInfo.ConvertTimeFromUtc(
                    DateTime.UtcNow,
                    TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")),
                UserName = model.UserName.ToLower(),
                Email = model.UserName.ToLower() + "@army.mil"
            };
        }
        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
        private async Task CreateUserSessionAsync(ApplicationUser user)
        {
            var rank = await _rank.GetByshort(user.RankId);

            var session = new DTOSession
            {
                RoleName = string.Join(",", await _userManager.GetRolesAsync(user)),
                UserName = user.UserName,
                Name = user.Name,
                ArmyNO = user.Name,
                RankName = rank.RankAbbreviation
            };

            SessionHeplers.SetObject(HttpContext.Session, "Users", session);
        }

        public async Task<IActionResult> Logout()
        {
            var user = await _userManager.GetUserAsync(User);

            // Invalidate ALL existing cookies of this user (including curl’s)
            await _userManager.UpdateSecurityStampAsync(user);
            HttpContext.Session.Clear();
            await _signInManager.SignOutAsync();
            return View();
        }

        public IActionResult Error()
        {
            return View(); // Create an AccessDenied.cshtml under Views/Account
        }
        public IActionResult ContactUs()
        {
            return View(); // Create an AccessDenied.cshtml under Views/Account
        }

        [Authorize(Roles = "admin")]
        public IActionResult AllUsers()
        {
            return View();
        }
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetAllUser()
        {
            return Json(await _users.GetAllData());
        }
        [Authorize(Roles = "admin")]
        [ValidateAntiForgeryToken] // add this if not using AJAX; for AJAX use header token
        public async Task<IActionResult> UpdateApprovalStatus([FromBody] DTOUserApprovalRequest dTOUserApprovalRequest)
        {
            if (ModelState.IsValid)
            {
                // Get the user ID from the ClaimsPrincipal (the logged-in user)
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var data = await _users.Get(dTOUserApprovalRequest.Id);
                if(data!=null)
                {
                    data.Active = dTOUserApprovalRequest.Active;
                    var retdata = await _users.UpdateWithReturn(data);
                    return Json(new DTOGenericResponse<object>(ConnKeyConstants.Success, ConnKeyConstants.SuccessMessage, true));
                }
             
            }
            return Json(new DTOGenericResponse<object>(ConnKeyConstants.BadRequest, ConnKeyConstants.IncorrectDataMessage, true));

        }
    }
}
