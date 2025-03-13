using Microsoft.AspNetCore.Mvc;
using UserManagement.Data;
using UserManagement.Models;
using System.Linq;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Microsoft.AspNetCore.Identity;

namespace UserManagement.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AccountController> _logger;

        public AccountController(ApplicationDbContext context, ILogger<AccountController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (_context.Users.Any(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "此 Email 已經被使用");
                return View(model);
            }

            var passwordHasher = new PasswordHasher<User>(); // ✅ 確保使用相同的 PasswordHasher
            var hashedPassword = passwordHasher.HashPassword(null, model.Password);

            // 確保密碼加密
            var newUser = new User
            {
                Name = model.Name,
                Email = model.Email,
                Password = hashedPassword, // ✅ 這行會自動加密
                Phone = model.Phone,
                Role = model.Role
            };

            HttpContext.Session.SetString("UserId", newUser.Id.ToString());
            HttpContext.Session.SetString("UserName", newUser.Name);
            HttpContext.Session.SetString("UserRole", newUser.Role);

            _context.Users.Add(newUser);
            _context.SaveChanges();

            return RedirectToAction("Login");
        }

        //當使用者進入 /Account/Login 時，顯示登入頁面(Login.cshtml)。
        [HttpGet]
        public IActionResult Login()
        {
            var userId = HttpContext.Session.GetString("UserId");

            if (!string.IsNullOrEmpty(userId))
            {
                Console.WriteLine("[DEBUG] 使用者已登入，轉跳到 Users/Index");
                return RedirectToAction("Index", "Users");
            }

            Console.WriteLine("[DEBUG] 顯示 Login 頁面");
            return View();
        }
        //驗證登入資訊
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult Login(LoginViewModel model)
        {

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);
            if (user == null)
            {
                _logger.LogWarning("[DEBUG] 帳號不存在，Email: {Email}", model.Email);
                ModelState.AddModelError("Email", "帳號不存在");
                return View(model);
            }


            // 驗證密碼
            var passwordHasher = new PasswordHasher<User>();
            var result = passwordHasher.VerifyHashedPassword(user, user.Password, model.Password);

            // Debug: 顯示密碼比對結果
            Console.WriteLine($"密碼比對結果: {result}");

            if (result == PasswordVerificationResult.Failed)
            {
                _logger.LogWarning("[DEBUG] 密碼錯誤，Email: {Email}", model.Email);
                ModelState.AddModelError("Password", "密碼錯誤");
                return View(model);
            }

            HttpContext.Session.SetString("UserId", user.Id.ToString());
            HttpContext.Session.SetString("UserName", user.Name); 
            HttpContext.Session.SetString("UserRole", user.Role);


            // 測試立即讀取
            var checkSession = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(checkSession))
            {
                _logger.LogError("[ERROR] Session 存入失敗，UserId: {UserId}", user.Id);
            }
            else
            {
                _logger.LogInformation("[DEBUG] Session 成功存入，UserId: {UserId}", checkSession);
            }

            var sessionUserId = HttpContext.Session.GetString("UserId");
            _logger.LogInformation("[DEBUG] Session UserId: {SessionUserId}", sessionUserId);
            _logger.LogInformation("[DEBUG] 嘗試轉跳到 Users/Index");
            return RedirectToAction("Index", "Users");
        }

        public IActionResult ClearSession()
        {
            HttpContext.Session.Clear();
            Console.WriteLine("[DEBUG] Session 已清除");
            return RedirectToAction("Login", "Account");
        }
        public IActionResult Logout()
        {
            Console.WriteLine("[DEBUG] 使用者登出，清除 Session");

            HttpContext.Session.Clear(); //清除所有 Session
            return RedirectToAction("Login", "Account"); // 導回 Login 頁面
        }

        [HttpGet]
        public IActionResult TestSession()
        {
            var userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userId))
            {
                var newSessionId = Guid.NewGuid().ToString();
                HttpContext.Session.SetString("UserId", newSessionId);

                var testRead = HttpContext.Session.GetString("UserId"); // 立即讀取 Session
                if (string.IsNullOrEmpty(testRead))
                {
                    return Content("🔴 Session 立刻丟失，IIS 可能有問題！");
                }

                return Content("🔴 Session 之前是空的，現在已寫入新值：" + newSessionId);
            }
            else
            {
                return Content($"🟢 目前的 Session UserId: {userId}");
            }
        }

    }

}
