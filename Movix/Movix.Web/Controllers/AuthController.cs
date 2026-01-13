//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Movix.Infrastructure.Entities;   // ✅ ApplicationUser
//using Movix.Web.Models;               // ✅ LoginVm
//using System.Threading.Tasks;

//namespace Movix.Web.Controllers
//{
//    public class AuthController : Controller
//    {
//        private readonly SignInManager<ApplicationUser> _signInManager;
//        private readonly UserManager<ApplicationUser> _userManager;

//        public AuthController(SignInManager<ApplicationUser> signInManager,
//                              UserManager<ApplicationUser> userManager)
//        {
//            _signInManager = signInManager;
//            _userManager = userManager;
//        }

//        // ------------------------------------------------------
//        // GET: /Auth/Login
//        // ------------------------------------------------------
//        [HttpGet]
//        public IActionResult Login(string? returnUrl = null)
//        {
//            ViewData["ReturnUrl"] = returnUrl;
//            return View();
//        }

//        // ------------------------------------------------------
//        // POST: /Auth/Login
//        // ------------------------------------------------------
//        [HttpPost]
//        public async Task<IActionResult> Login(LoginVm model, string? returnUrl = null)
//        {
//            ViewData["ReturnUrl"] = returnUrl;

//            if (!ModelState.IsValid)
//                return View(model);

//            var user = await _userManager.FindByEmailAsync(model.Email);
//            if (user == null)
//            {
//                ModelState.AddModelError("", "Usuário não encontrado.");
//                return View(model);
//            }

//            var result = await _signInManager.PasswordSignInAsync(
//                user, model.Password, isPersistent: false, lockoutOnFailure: false);

//            if (result.Succeeded)
//            {
//                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
//                    return Redirect(returnUrl);

//                return RedirectToAction("Index", "Home");
//            }

//            ModelState.AddModelError("", "Credenciais inválidas.");
//            return View(model);
//        }

//        // ------------------------------------------------------
//        // POST: /Auth/Logout
//        // ------------------------------------------------------
//        [HttpPost]
//        public async Task<IActionResult> Logout()
//        {
//            await _signInManager.SignOutAsync();
//            return RedirectToAction("Index", "Home");
//        }

//        // ------------------------------------------------------
//        // GET: /Auth/Denied (403)
//        // ------------------------------------------------------
//        [HttpGet]
//        public IActionResult Denied()
//        {
//            return View();
//        }
//    }
//}







//criar



using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Movix.Infrastructure.Entities;   // ApplicationUser
using Movix.Web.Models;               // LoginVm
using System.Threading.Tasks;

namespace Movix.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthController(SignInManager<ApplicationUser> signInManager,
                              UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        // ------------------------------------------------------
        // GET: /Auth/Login
        // ------------------------------------------------------
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // ------------------------------------------------------
        // POST: /Auth/Login
        // ------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> Login(LoginVm model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "Usuário não encontrado.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user, model.Password, isPersistent: false, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Credenciais inválidas.");
            return View(model);
        }

        // ------------------------------------------------------
        // GET: /Auth/Registrar
        // ------------------------------------------------------
        [HttpGet]
        public IActionResult Registrar()
        {
            return View();
        }

        // ------------------------------------------------------
        // POST: /Auth/Registrar
        // ------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> Registrar(string Email, string Password, string ConfirmPassword)
        {
            if (Password != ConfirmPassword)
            {
                ViewBag.ErrorMessage = "As senhas não coincidem.";
                return View();
            }

            var user = new ApplicationUser
            {
                UserName = Email,
                Email = Email
            };

            var result = await _userManager.CreateAsync(user, Password);

            if (result.Succeeded)
            {
                // login automático
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View();
        }

       
                // POST: /Auth/Logout
                // ------------------------------------------------------
                [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // ------------------------------------------------------
        // GET: /Auth/Denied (403)
        // ------------------------------------------------------
        [HttpGet]
        public IActionResult Denied()
        {
            return View();
        }
    }
}

