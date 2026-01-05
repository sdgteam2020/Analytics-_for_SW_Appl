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
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IRank _rank;
        private readonly IUsers _users;
        public const string SessionKeySalt = "_Salt";
        public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager,IRank rank, IUsers users)
        {
            _signInManager = signInManager;
            _userManager = userManager;
           
            _roleManager = roleManager;
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
            if (ModelState.IsValid)
            {
                string? GetSalt = HttpContext.Session.GetString(SessionKeySalt); // Get Salt from Session
                if (GetSalt != null)
                {
                    ViewBag.hdns = GetSalt;
                    string Password = AESEncrytDecry.DecryptAES(request.Password, GetSalt);  //decrypt password
                    request.Password = Password;
                }
                // Find the user by Domain ID (or username)
                var user = await _userManager.FindByNameAsync(request.UserName); // Or use FindByEmailAsync, depending on your model

                if (user != null)
                {

                    // Attempt to sign in the user with the provided password
                    var result = await _signInManager.PasswordSignInAsync(user, request.Password, isPersistent: false, lockoutOnFailure: true);

                    if (result.Succeeded)
                    {
                        HttpContext.Session.Clear();

                        // Delete the session cookie so ASP.NET Core issues a NEW session ID
                        var sessionCookieName = ".AspNetCore.Session"; // or ".MOU.Session" if you renamed it
                        if (Request.Cookies.ContainsKey(sessionCookieName))
                        {
                            Response.Cookies.Delete(sessionCookieName);
                        }


                        await _userManager.UpdateSecurityStampAsync(user);

                        // 2) Re-issue THIS session’s cookie so it contains the fresh stamp
                        await _signInManager.SignOutAsync();
                        await _signInManager.SignInAsync(user, isPersistent: false);

                      
                        var ret = await _rank.GetByshort(user.RankId);

                        var DTOSession = new DTOSession
                        {
                            UserId = user.Id,
                            RoleName = string.Join(",", await _userManager.GetRolesAsync(user)),
                            UserName = user.UserName,
                            Name = user.Name,
                            ArmyNO = user.ArmyNo,
                            RankName = ret.RankAbbreviation,
                        };

                        SessionHeplers.SetObject(HttpContext.Session, "Users", DTOSession);
                        var dto = SessionHeplers.GetObject<DTOSession>(HttpContext.Session, "Users");


                        if (!user.Active)
                        {
                            return RedirectToAction("ContactUs", "Account");
                        }
                        return RedirectToAction("Index", "Home");
                    }
                    else if (result.IsLockedOut)
                    {
                        ModelState.AddModelError(string.Empty, "Account Locked Out Please Try after 10 minutes.");


                    }
                    else if (result.IsNotAllowed)
                    {
                        ModelState.AddModelError(string.Empty, "Already Login \" + user.UserName + \" Please Try Some Time.");


                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Not Valid User / Password. Access Failed Count " + user.AccessFailedCount + " Max Access Attempts 3");


                    }
                    //else
                    //{
                    //    // If login failed (incorrect password), add a model error
                    //    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    //}
                }
                else
                {
                    // User not found, redirect to the Registration page
                    TempData["UserName"] = request.UserName;
                    return RedirectToAction("Register", "Account");
                }
            }

            // If the model is invalid or login failed, return the login view with errors
            return View(request);
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
            bool ActiveStatus = false;
            

            ViewBag.UserName = model.UserName;
            if (ModelState.IsValid)
            {
                string? GetSalt = HttpContext.Session.GetString(SessionKeySalt); // Get Salt from Session
                if (GetSalt != null)
                {
                    ViewBag.hdns = GetSalt;
                    string Password = AESEncrytDecry.DecryptAES(model.ConfirmPassword, GetSalt);  //decrypt password
                    model.ConfirmPassword = Password;
                }
                int userId = Convert.ToInt32(this.User.FindFirstValue(ClaimTypes.NameIdentifier));
                var user = new ApplicationUser
                {
                    ArmyNo = model.ArmyNo,
                    RankId = model.RankId,
                    Name = model.Name,
                    Active = ActiveStatus,
                    Updatedby = userId,
                    UpdatedOn = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")),
                   
                    UserName = model.UserName.ToLower(),
                    Email = model.UserName.ToLower() + "@army.mil",
                   

                };
                // Check if user already exists
                var existingUser = await _userManager.FindByNameAsync(model.UserName);
                if (existingUser != null)
                {
                    ModelState.AddModelError(string.Empty, "User already exists.");
                    return View(model);
                }

                var result = await _userManager.CreateAsync(user, model.ConfirmPassword);

                if (result.Succeeded)
                {

                   

                    await _userManager.AddToRoleAsync(user, "User");

                    await _signInManager.SignInAsync(user, isPersistent: false);


                    var ret = await _rank.GetByshort(user.RankId);
                    if (!user.Active)
                    {

                        return RedirectToAction("ContactUs", "Account");
                    }
                    var DTOSession = new DTOSession
                    {
                        RoleName = string.Join(",", await _userManager.GetRolesAsync(user)),
                        UserName = user.UserName,
                        Name = user.Name,
                        ArmyNO = user.Name,
                        RankName = ret.RankAbbreviation,
                       

                    };
                    SessionHeplers.SetObject(HttpContext.Session, "Users", DTOSession);
                  
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            return View(model);
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
