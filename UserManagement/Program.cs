using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UserManagement.Data; // 確保有正確的 namespace

var builder = WebApplication.CreateBuilder(args);

// 讀取 appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new Exception("Connection string 'DefaultConnection' not found in appsettings.json");
}


// 註冊 DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 啟用 Session 服務
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // 設定 Session 過期時間 (30 分鐘)
    options.Cookie.HttpOnly = true; // 讓 Cookie 只能透過 HTTP 存取，提高安全性
    options.Cookie.IsEssential = true; // 確保 Session 在隱私權模式下仍然可用
});


builder.Services.AddControllersWithViews();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();//Session
app.UseAuthentication();  // 啟用身份驗證
app.UseAuthorization();   // 啟用授權

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
