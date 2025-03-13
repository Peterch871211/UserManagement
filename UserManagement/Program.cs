using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserManagement.Data;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.CookiePolicy;

var builder = WebApplication.CreateBuilder(args);

// 設定 logs 目錄，確保 `stdout.log` 可寫入
var logsPath = Path.Combine(Directory.GetCurrentDirectory(), "logs");
Directory.CreateDirectory(logsPath);
var logFilePath = Path.Combine(logsPath, "stdout.log");

// 設定 ILogger
var loggerFactory = LoggerFactory.Create(logging =>
{
    logging.AddConsole().AddDebug().AddFile(Path.Combine(logsPath, "myapp-{Date}.txt"));
});
var logger = loggerFactory.CreateLogger<Program>();
logger.LogInformation("應用程式啟動中...");

// 讀取 `Connection String`
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new Exception("Connection string 'DefaultConnection' not found in appsettings.json");

// 註冊 `DbContext`
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 設定 Session（確保 HTTP 環境可用）
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // 設定 30 分鐘 Session
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax; //  HTTP 下 `SameSite=Lax`，避免 `Secure` 限制
    options.Cookie.SecurePolicy = CookieSecurePolicy.None; //  允許 HTTP 存取
    options.Cookie.MaxAge = TimeSpan.FromMinutes(30); //  確保 Cookie 存活時間
});

// 註冊 `MVC` 控制器
builder.Services.AddControllersWithViews();

// 設定 `Antiforgery`（確保 CSRF 防護正常）
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.SecurePolicy = CookieSecurePolicy.None; // 允許 HTTP
});

// 設定 `Cookie` 原則
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.Lax; //  HTTP 可用
    options.Secure = CookieSecurePolicy.None; //  避免 `Secure` 限制
    options.HttpOnly = HttpOnlyPolicy.Always;
});

var app = builder.Build();

// 記錄 `應用程式啟動`
File.AppendAllText(logFilePath, $"[{DateTime.UtcNow}] 應用程式已啟動\n");

// 錯誤處理
if (!builder.Environment.IsProduction())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
}

// 啟用靜態檔案、Session、路由
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.UseCookiePolicy();

// 記錄 IIS 請求，方便 Debug
app.Use(async (context, next) =>
{
    Console.WriteLine($"[{DateTime.UtcNow}] {context.Request.Method} {context.Request.Path}");
    await next();
});

// 記錄 Exception
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        var errorLog = $"[{DateTime.UtcNow}] 例外錯誤: {ex.Message}\n{ex.StackTrace}\n";
        File.AppendAllText(logFilePath, errorLog);
        logger.LogError(ex, "發生未處理的例外錯誤");
        Console.WriteLine($"[ERROR] {ex.Message}");
        throw;
    }
});

// 記錄 Session Id
app.Use(async (context, next) =>
{
    Console.WriteLine($"[DEBUG] Session Id: {context.Session.Id}");
    await next();
});

// 設定 MVC 路由
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

logger.LogInformation("應用程式已成功啟動");
app.Run();
