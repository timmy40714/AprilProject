using Microsoft.EntityFrameworkCore;
using prjCatChaOnlineShop.Models;
using prjCatChaOnlineShop.Models.CModels;
using prjCatChaOnlineShop.Models.ViewModels;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

//���Usession
builder.Services.AddSession();

//�X�ݷ�e HTTP �n�D��������T�A�Ҧp HTTP �W�U��BSession�BCookies
builder.Services.AddHttpContextAccessor();

// ���U CheckoutService �A��
builder.Services.AddScoped<CheckoutService>();

//�������i�H�ѪRDB��Ʈw

builder.Services.AddDbContext<cachaContext>(
 options => options.UseSqlServer(builder.Configuration.GetConnectionString("CachaConnection")));

// �ͦ��@�ӷs���H�����_
string randomKey = CKeyGenerator.GenerateRandomKey();

// �N�H�����_�]�m�� IConfiguration ��
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
