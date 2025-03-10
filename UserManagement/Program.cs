using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserManagement.Data;

var builder = WebApplication.CreateBuilder(args);

// 讀取 appsettings.json，確保 Connection String 存在
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new Exception("Connection string 'DefaultConnection' not found in appsettings.json");

// 註冊 DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 啟用 Session 服務
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // 設定 Session 過期時間
    options.Cookie.HttpOnly = true; // 防止 JavaScript 存取，提高安全性
    options.Cookie.IsEssential = true; // 確保 Session 在隱私模式可用
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // 只允許 HTTPS
    options.Cookie.SameSite = SameSiteMode.Lax; // 允許跨站存取
});

// 註冊 MVC 控制器
builder.Services.AddControllersWithViews();

// 設定 CSRF 防護的 Cookie
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // 只允許 HTTPS
    options.Cookie.SameSite = SameSiteMode.Lax; // 允許跨站請求存取
    options.Cookie.HttpOnly = true; // 防止 JavaScript 存取
});

var app = builder.Build();

// 啟用 `HTTPS`、靜態檔案、Session
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession(); // 直接啟用 `Session`（不需要重複設置 Cookie）

// 啟用身份驗證 & 授權
app.UseAuthentication();
app.UseAuthorization();

// 設定 Cookie 原則
app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Lax, // 允許跨站存取 Cookie
    Secure = CookieSecurePolicy.Always // 只允許 HTTPS
});

// 設定 MVC 路由
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
