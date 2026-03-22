using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using TienLuxury.Models;
using TienLuxury.Areas.Admin.Services;
using TienLuxury.Services;
using TienLuxury.Data;
using HairSalonWeb.Services;
using System.Globalization;
using DotNetEnv;
using MongoDB.Driver;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Thiết lập CultureInfo mặc định cho toàn bộ ứng dụng
var cultureInfo = new CultureInfo("vi-VN");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Nếu chưa đăng nhập thì chuyển hướng về đây
        options.AccessDeniedPath = "/Account/AccessDenied"; // Nếu không đủ quyền thì về đây
        options.ExpireTimeSpan = TimeSpan.FromDays(7); // Lưu đăng nhập 7 ngày  
    })

    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
        // Tự động lưu thông tin vào Cookie sau khi Google xác thực xong
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddFacebook(options =>
    {
        options.AppId = builder.Configuration["Authentication:Facebook:AppId"];
        options.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"];
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    });

// Add services to the container.
builder.Services.AddControllersWithViews();
// Add services to the container.
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache(); // Cần thiết để sử dụng Session
// Thêm dịch vụ Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Thời gian hết hạn session
    options.Cookie.HttpOnly = true; // Bảo mật session
    options.Cookie.IsEssential = true;
});

builder.Services.AddControllersWithViews();



//Lower case url
builder.Services.AddRouting(options => options.LowercaseUrls = true);

//Config database
var mongoDBSettings = builder.Configuration.GetSection("MongoDBSettings").Get<MongoDBSettings>();
var mongoPassword = Environment.GetEnvironmentVariable("MONGODB_PASSWORD") ?? "";
var connectionString = mongoDBSettings?.AtlasURI?.Replace("<PASSWORD>", mongoPassword)
    ?? throw new InvalidOperationException("MongoDBSettings or AtlasURI is not configured properly.");
// Đăng ký MongoClient
builder.Services.AddSingleton<MongoDB.Driver.IMongoClient>(sp =>
    new MongoDB.Driver.MongoClient(connectionString));

builder.Services.AddScoped<MongoDB.Driver.IMongoDatabase>(sp =>
    sp.GetRequiredService<MongoDB.Driver.IMongoClient>().GetDatabase(mongoDBSettings.DatabaseName));

builder.Services.Configure<MongoDBSettings>(builder.Configuration.GetSection("MongoDBSettings"));

builder.Services.AddDbContext<DBContext>(options =>
    options.UseMongoDB(connectionString, mongoDBSettings.DatabaseName ?? "")
);
Console.WriteLine("MongoDB password: " + mongoPassword);
if (string.IsNullOrEmpty(mongoPassword))    
{
    throw new Exception("MONGODB_PASSWORD not found");
}




builder.Services.AddScoped<IAdminAccountService, AdminAccountService>();
builder.Services.AddScoped<IServiceService, ServiceService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IReservationDetailService, ReservationDetailService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IInvoiceDetailsService, InvoiceDetailsService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IVoucherService, VoucherService>();
builder.Services.AddScoped<GeminiService>();
builder.Services.AddScoped<ChatDataService>();


var app = builder.Build();

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture(cultureInfo)
});

app.UseSession(); // Kích hoạt Session
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Product}/{action=ProductDetail}/{id?}");

app.Run();
