using eSignPRPO.interfaces;
using eSignPRPO.Models.Login;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace eSignPRPO.Controllers
{
    public class AccountController : Controller
    {
        private IAccountService _accountService;
        private ILogger<AccountController> _logger;
        public AccountController(IAccountService accountService, ILogger<AccountController> logger)
        {
            _accountService = accountService;
            _logger = logger;
        }
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(Credential credential, string returnUrl = "")
        {
            try
            {
                if (!ModelState.IsValid) return View();

                var chkSupLogin = await _accountService.checkSupplierLogin(credential);

                if (chkSupLogin != null)
                {
                    var claims = new List<Claim> {
                 new Claim(ClaimTypes.Sid,$"{chkSupLogin?.SCusUsername}"),
                new Claim(ClaimTypes.Name,$"{chkSupLogin?.SCusName}"),
                new Claim(ClaimTypes.Role,"99"),
                    };
                    var identity = new ClaimsIdentity(claims, "eSignPRPO");
                    ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);

                    await HttpContext.SignInAsync("eSignPRPO", claimsPrincipal);

                    _logger.LogInformation($"[{chkSupLogin?.SCusUsername}] : {chkSupLogin?.SCusName} login success.");

                    if (!String.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        if (returnUrl == "/")
                        {
                            return RedirectToAction("worklist", "supplier");
                        }
                        return Redirect(returnUrl);
                    }
                    else
                        return RedirectToAction("worklist", "supplier");
                }


                var AccessLogin = await _accountService.checkLoginUser(credential);
                if (AccessLogin != null)
                {
                    var claims = new List<Claim> {
                 new Claim(ClaimTypes.Sid,$"{AccessLogin?.SEmpUsername}"),
                new Claim(ClaimTypes.Name,$"{AccessLogin?.SEmpName}"),
                new Claim(ClaimTypes.Role,$"{AccessLogin?.NPositionLevel}"),
                new Claim("Department",$"{AccessLogin?.SDepartment}"),
                 new Claim("Position",$"{AccessLogin?.SPosition}"),
                 new Claim("Title",$"{AccessLogin?.SEmpTitle}"),
                };

                    var identity = new ClaimsIdentity(claims, "eSignPRPO");
                    ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(identity);

                    await HttpContext.SignInAsync("eSignPRPO", claimsPrincipal);

                    _logger.LogInformation($"[{AccessLogin?.SEmpUsername}] : {AccessLogin?.SEmpName} login success.");

                    if (!String.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("worklist", "prpo");
                    }

                }

                ViewBag.State = false;
                ViewBag.Message = "Username หรือ Password ไม่ถูกต้องกรุณาตรวจสอบใหม่อีกครั้ง";
                _logger.LogWarning($"[{AccessLogin?.SEmpUsername}] : {AccessLogin?.SEmpName} login failure. [Username or password incorrects.]");
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message}");
                ViewBag.State = false;
                ViewBag.Message = ex.Message;
                return View();
            }
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("eSignPRPO");
            return RedirectToAction("worklist", "prpo");
        }


    }
}
