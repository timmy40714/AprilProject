using Microsoft.EntityFrameworkCore;
using prjCatChaOnlineShop.Models;
using prjCatChaOnlineShop.Models.CModels;
using prjCatChaOnlineShop.Models.ViewModels;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

//註冊session
builder.Services.AddSession();

//訪問當前 HTTP 要求的相關資訊，例如 HTTP 上下文、Session、Cookies
builder.Services.AddHttpContextAccessor();

// 註冊 CheckoutService 服務
builder.Services.AddScoped<CheckoutService>();

//讓網頁可以解析DB資料庫

builder.Services.AddDbContext<cachaContext>(
 options => options.UseSqlServer(builder.Configuration.GetConnectionString("CachaConnection")));

// 生成一個新的隨機金鑰
string randomKey = CKeyGenerator.GenerateRandomKey();

// 將隨機金鑰設置到 IConfiguration 裡
builder.Configuration["ForgetPassword:SecretKey"] = randomKey;

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=CMSHome}/{action=Login}/{id?}");

    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Index}/{action=Index}/{id?}");

});

app.Run();
