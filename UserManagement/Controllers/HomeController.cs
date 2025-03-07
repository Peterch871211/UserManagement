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
        var userRole = HttpContext.Session.GetString("UserRole") ?? "Viewer"; // �w�] Viewer �קK null

        Console.WriteLine($"[DEBUG] �i�J Home/Index - UserId: {userId}, UserRole: {userRole}");


        if (string.IsNullOrEmpty(userId))
        {
            Console.WriteLine("[DEBUG] �ϥΪ̥��n�J�A����� Login ����");
            return RedirectToAction("Login", "Account");
        }

        //  �T�O `Viewer` ���d�b `Home`�A���|���ɨ� `Users/Index`
        if (userRole == "Viewer")
        {
            Console.WriteLine("[DEBUG] Viewer �w�n�J�A���d�b Home ����");
            return View(); //���n Redirect�A�� Viewer ���d�b Home
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
