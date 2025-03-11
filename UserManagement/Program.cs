using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserManagement.Data;

var builder = WebApplication.CreateBuilder(args);

//  �T�O `logs/` �ؿ��s�b�A�קK `stdout.log` �L�k�g�J
var logsPath = Path.Combine(Directory.GetCurrentDirectory(), "logs");
if (!Directory.Exists(logsPath))
{
    Directory.CreateDirectory(logsPath);
    Console.WriteLine(" logs �ؿ��w�إߡI");
}

//  �P�_�O�_���������ҡ]Production�^
bool isProduction = builder.Environment.IsProduction();

//  Ū�� `appsettings.json`�A�T�O `Connection String` �s�b
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new Exception("Connection string 'DefaultConnection' not found in appsettings.json");

//  ���U `DbContext`
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

//  �ҥ� `Session` �A��
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = isProduction ? CookieSecurePolicy.Always : CookieSecurePolicy.None; // ���\ HTTP �}�o�A�������ұj�� HTTPS
    options.Cookie.SameSite = SameSiteMode.Lax;
});

//  ���U `MVC` ���
builder.Services.AddControllersWithViews();

//  �]�w `CSRF` ���@�]�קK `HTTPS` ����ɭP `500`�^
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.SecurePolicy = isProduction && builder.Configuration["Environment"] == "Production"
        ? CookieSecurePolicy.Always  // �u���b�� HTTPS ���Үɱj�� HTTPS
        : CookieSecurePolicy.None;   // ���\ HTTP�A�קK Antiforgery ���D
});

var app = builder.Build();

//  ���~�B�z�]`Development` �Ҧ���ܿ��~�A`Production` �ɦV���~�����^
if (!isProduction)
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
}

//  �T�O `Production` �Ҧ��ϥ� `HTTPS`
if (isProduction)
{
    app.UseHttpsRedirection();
}

//  �ҥ��R�A�ɮסBSession�B����
app.UseStaticFiles();
app.UseRouting();
app.UseSession();

//  �ҥΨ������� & ���v
app.UseAuthentication();
app.UseAuthorization();

//  �]�w `Cookie` ��h
app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Lax,
    Secure = isProduction ? CookieSecurePolicy.Always : CookieSecurePolicy.None
});

//  �O�� `IIS` �ШD�A��K Debug
app.Use(async (context, next) =>
{
    Console.WriteLine($"[{DateTime.UtcNow}] {context.Request.Method} {context.Request.Path}");
    await next();
});

//  �O�� `Exception`�A�T�O `stdout.log` �i�O�����~
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        Console.WriteLine($" �ҥ~���~: {ex.Message}");
        throw;
    }
});
var logFilePath = Path.Combine(logsPath, "stdout.log");

// �j��b���ε{���ҰʮɰO��
File.AppendAllText(logFilePath, $"[{DateTime.UtcNow}] ���ε{���w�Ұ�\n");

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        var errorLog = $"[{DateTime.UtcNow}] �ҥ~���~: {ex.Message}\n{ex.StackTrace}\n";
        File.AppendAllText(logFilePath, errorLog);
        Console.WriteLine($" �O�����~: {ex.Message}");
        throw;
    }
});

//  �]�w `MVC` ����
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

//  ������ε{���ҰʰT��
Console.WriteLine(" ���ε{���w�Ұ�");
app.Run();
