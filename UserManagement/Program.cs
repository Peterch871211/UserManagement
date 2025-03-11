using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserManagement.Data;

var builder = WebApplication.CreateBuilder(args);

//  確保 `logs/` 目錄存在，避免 `stdout.log` 無法寫入
var logsPath = Path.Combine(Directory.GetCurrentDirectory(), "logs");
if (!Directory.Exists(logsPath))
{
    Directory.CreateDirectory(logsPath);
    Console.WriteLine(" logs 目錄已建立！");
}

//  判斷是否為正式環境（Production）
bool isProduction = builder.Environment.IsProduction();

//  讀取 `appsettings.json`，確保 `Connection String` 存在
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new Exception("Connection string 'DefaultConnection' not found in appsettings.json");

//  註冊 `DbContext`
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

//  啟用 `Session` 服務
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = isProduction ? CookieSecurePolicy.Always : CookieSecurePolicy.None; // 允許 HTTP 開發，正式環境強制 HTTPS
    options.Cookie.SameSite = SameSiteMode.Lax;
});

//  註冊 `MVC` 控制器
builder.Services.AddControllersWithViews();

//  設定 `CSRF` 防護（避免 `HTTPS` 限制導致 `500`）
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.SecurePolicy = isProduction && builder.Configuration["Environment"] == "Production"
        ? CookieSecurePolicy.Always  // 只有在有 HTTPS 環境時強制 HTTPS
        : CookieSecurePolicy.None;   // 允許 HTTP，避免 Antiforgery 問題
});

var app = builder.Build();

//  錯誤處理（`Development` 模式顯示錯誤，`Production` 導向錯誤頁面）
if (!isProduction)
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
}

//  確保 `Production` 模式使用 `HTTPS`
if (isProduction)
{
    app.UseHttpsRedirection();
}

//  啟用靜態檔案、Session、路由
app.UseStaticFiles();
app.UseRouting();
app.UseSession();

//  啟用身份驗證 & 授權
app.UseAuthentication();
app.UseAuthorization();

//  設定 `Cookie` 原則
app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Lax,
    Secure = isProduction ? CookieSecurePolicy.Always : CookieSecurePolicy.None
});

//  記錄 `IIS` 請求，方便 Debug
app.Use(async (context, next) =>
{
    Console.WriteLine($"[{DateTime.UtcNow}] {context.Request.Method} {context.Request.Path}");
    await next();
});

//  記錄 `Exception`，確保 `stdout.log` 可記錄錯誤
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        Console.WriteLine($" 例外錯誤: {ex.Message}");
        throw;
    }
});
var logFilePath = Path.Combine(logsPath, "stdout.log");

// 強制在應用程式啟動時記錄
File.AppendAllText(logFilePath, $"[{DateTime.UtcNow}] 應用程式已啟動\n");

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
        Console.WriteLine($" 記錄錯誤: {ex.Message}");
        throw;
    }
});

//  設定 `MVC` 路由
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

//  顯示應用程式啟動訊息
Console.WriteLine(" 應用程式已啟動");
app.Run();
