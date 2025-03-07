using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Models;

namespace UserManagement.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        var userId = HttpContext.Session.GetString("UserId");
        var userRole = HttpContext.Session.GetString("UserRole") ?? "Viewer"; // 預設 Viewer 避免 null

        Console.WriteLine($"[DEBUG] 進入 Home/Index - UserId: {userId}, UserRole: {userRole}");


        if (string.IsNullOrEmpty(userId))
        {
            Console.WriteLine("[DEBUG] 使用者未登入，跳轉到 Login 頁面");
            return RedirectToAction("Login", "Account");
        }

        //  確保 `Viewer` 停留在 `Home`，不會重導到 `Users/Index`
        if (userRole == "Viewer")
        {
            Console.WriteLine("[DEBUG] Viewer 已登入，停留在 Home 頁面");
            return View(); //不要 Redirect，讓 Viewer 停留在 Home
        }

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
}
