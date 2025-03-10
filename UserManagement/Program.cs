using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserManagement.Data;

var builder = WebApplication.CreateBuilder(args);

// Ū�� appsettings.json�A�T�O Connection String �s�b
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new Exception("Connection string 'DefaultConnection' not found in appsettings.json");

// ���U DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// �ҥ� Session �A��
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // �]�w Session �L���ɶ�
    options.Cookie.HttpOnly = true; // ���� JavaScript �s���A�����w����
    options.Cookie.IsEssential = true; // �T�O Session �b���p�Ҧ��i��
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // �u���\ HTTPS
    options.Cookie.SameSite = SameSiteMode.Lax; // ���\�󯸦s��
});

// ���U MVC ���
builder.Services.AddControllersWithViews();

// �]�w CSRF ���@�� Cookie
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // �u���\ HTTPS
    options.Cookie.SameSite = SameSiteMode.Lax; // ���\�󯸽ШD�s��
    options.Cookie.HttpOnly = true; // ���� JavaScript �s��
});

var app = builder.Build();

// �ҥ� `HTTPS`�B�R�A�ɮסBSession
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession(); // �����ҥ� `Session`�]���ݭn���Ƴ]�m Cookie�^

// �ҥΨ������� & ���v
app.UseAuthentication();
app.UseAuthorization();

// �]�w Cookie ��h
app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Lax, // ���\�󯸦s�� Cookie
    Secure = CookieSecurePolicy.Always // �u���\ HTTPS
});

// �]�w MVC ����
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
