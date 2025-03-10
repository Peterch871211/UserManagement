using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using UserManagement.Data;
using UserManagement.Models;

namespace UserManagement.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");

            Console.WriteLine($"[DEBUG] 目前登入的使用者 ID：{userId}");
            Console.WriteLine($"[DEBUG] 目前登入的使用者角色：{userRole}");

            if (string.IsNullOrEmpty(userId))
            {
                Console.WriteLine("[DEBUG] 使用者未登入，跳轉回 Login 頁面");
                return RedirectToAction("Login", "Account");
            }

            if (userRole != "Admin" && userRole != "User")
            {
                Console.WriteLine("[DEBUG] 權限不足，跳轉到 Home 頁面");
                return RedirectToAction("Index", "Home");
            }

            Console.WriteLine("[DEBUG] 使用者已登入，顯示使用者列表");
            return View(await _context.Users.ToListAsync());
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            var userRole = HttpContext.Session.GetString("UserRole");

            if (userRole != "Admin")
            {
                Console.WriteLine("[DEBUG] 權限不足，跳轉到 Home 頁面");
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Email,Password,Phone,Role")] User user)
        {

            try
            {
                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                // 這裡假設是 Email 重複造成的錯誤
                ModelState.AddModelError("Email", "此 Email 已被使用，請輸入其他 Email。");
            }
        
            return View(user);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Email,Password,Phone,Role")] User user)
        {
            var currentUserId = HttpContext.Session.GetString("UserId"); // 取得目前登入的使用者 ID
            var currentUserRole = HttpContext.Session.GetString("UserRole"); // 取得目前登入的使用者角色

            if (id != user.Id)
            {
                return NotFound();
            }

            //  如果 `Admin` 嘗試更改自己的 `Role`，阻止修改
            if (currentUserId == user.Id.ToString() && currentUserRole == "Admin" && user.Role != "Admin")
            {
                Console.WriteLine("[DEBUG] Admin 嘗試修改自己的 Role，被拒絕！");
                ModelState.AddModelError("", "你不能更改自己的角色！");
                return View(user);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // 確保 `Password` 沒有變成明文
                    var existingUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
                    if (existingUser == null)
                    {
                        return NotFound();
                    }

                    if (string.IsNullOrEmpty(user.Password))
                    {
                        Console.WriteLine($"[DEBUG] 編輯時密碼未變更，保留原密碼：{existingUser.Password}");
                        user.Password = existingUser.Password;
                    }
                    else
                    {
                        Console.WriteLine($"[DEBUG] 編輯時密碼已變更");
                        Console.WriteLine($"[DEBUG] 原密碼哈希值: {existingUser.Password}");

                        // ✅ 直接存入明文密碼，讓 `User.cs` 自動加密
                        user.Password = user.Password;

                        Console.WriteLine($"[DEBUG] 存入的密碼（未加密）: {user.Password}");
                    }

                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }


        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
