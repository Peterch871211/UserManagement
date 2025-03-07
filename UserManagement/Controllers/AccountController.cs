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

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);
            if (user == null)
            {
                Console.WriteLine(" 帳號不存在");
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
                Console.WriteLine(" 密碼錯誤");
                ModelState.AddModelError("Password", "密碼錯誤");
                return View(model);
            }

            Console.WriteLine(" 登入成功");
            HttpContext.Session.SetString("UserId", user.Id.ToString());
            HttpContext.Session.SetString("UserName", user.Name); // 存入 Name
            HttpContext.Session.SetString("UserRole", user.Role);

            //Debug: 顯示 Session 是否成功儲存
            Console.WriteLine($"[DEBUG] 登入成功！UserId: {HttpContext.Session.GetString("UserId")}");
            Console.WriteLine($"[DEBUG] 登入成功！UserName: {HttpContext.Session.GetString("UserName")}");
            Console.WriteLine($"[DEBUG] 登入成功！UserRole: {HttpContext.Session.GetString("UserRole")}");


            return RedirectToAction("Index", "Users"); // 登入成功
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


    }
}
