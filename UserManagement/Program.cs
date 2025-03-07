using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UserManagement.Data; // �T�O�����T�� namespace

var builder = WebApplication.CreateBuilder(args);

// Ū�� appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new Exception("Connection string 'DefaultConnection' not found in appsettings.json");
}


// ���U DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// �ҥ� Session �A��
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // �]�w Session �L���ɶ� (30 ����)
    options.Cookie.HttpOnly = true; // �� Cookie �u��z�L HTTP �s���A�����w����
    options.Cookie.IsEssential = true; // �T�O Session �b���p�v�Ҧ��U���M�i��
});


builder.Services.AddControllersWithViews();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();//Session
app.UseAuthentication();  // �ҥΨ�������
app.UseAuthorization();   // �ҥα��v

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
