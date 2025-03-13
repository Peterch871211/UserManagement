using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserManagement.Data;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.CookiePolicy;

var builder = WebApplication.CreateBuilder(args);

// �]�w logs �ؿ��A�T�O `stdout.log` �i�g�J
var logsPath = Path.Combine(Directory.GetCurrentDirectory(), "logs");
Directory.CreateDirectory(logsPath);
var logFilePath = Path.Combine(logsPath, "stdout.log");

// �]�w ILogger
var loggerFactory = LoggerFactory.Create(logging =>
{
    logging.AddConsole().AddDebug().AddFile(Path.Combine(logsPath, "myapp-{Date}.txt"));
});
var logger = loggerFactory.CreateLogger<Program>();
logger.LogInformation("���ε{���Ұʤ�...");

// Ū�� `Connection String`
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new Exception("Connection string 'DefaultConnection' not found in appsettings.json");

// ���U `DbContext`
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// �]�w Session�]�T�O HTTP ���ҥi�Ρ^
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // �]�w 30 ���� Session
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax; //  HTTP �U `SameSite=Lax`�A�קK `Secure` ����
    options.Cookie.SecurePolicy = CookieSecurePolicy.None; //  ���\ HTTP �s��
    options.Cookie.MaxAge = TimeSpan.FromMinutes(30); //  �T�O Cookie �s���ɶ�
});

// ���U `MVC` ���
builder.Services.AddControllersWithViews();

// �]�w `Antiforgery`�]�T�O CSRF ���@���`�^
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.SecurePolicy = CookieSecurePolicy.None; // ���\ HTTP
});

// �]�w `Cookie` ��h
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.Lax; //  HTTP �i��
    options.Secure = CookieSecurePolicy.None; //  �קK `Secure` ����
    options.HttpOnly = HttpOnlyPolicy.Always;
});

var app = builder.Build();

// �O�� `���ε{���Ұ�`
File.AppendAllText(logFilePath, $"[{DateTime.UtcNow}] ���ε{���w�Ұ�\n");

// ���~�B�z
if (!builder.Environment.IsProduction())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
}

// �ҥ��R�A�ɮסBSession�B����
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.UseCookiePolicy();

// �O�� IIS �ШD�A��K Debug
app.Use(async (context, next) =>
{
    Console.WriteLine($"[{DateTime.UtcNow}] {context.Request.Method} {context.Request.Path}");
    await next();
});

// �O�� Exception
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
        logger.LogError(ex, "�o�ͥ��B�z���ҥ~���~");
        Console.WriteLine($"[ERROR] {ex.Message}");
        throw;
    }
});

// �O�� Session Id
app.Use(async (context, next) =>
{
    Console.WriteLine($"[DEBUG] Session Id: {context.Session.Id}");
    await next();
});

// �]�w MVC ����
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

logger.LogInformation("���ε{���w���\�Ұ�");
app.Run();
