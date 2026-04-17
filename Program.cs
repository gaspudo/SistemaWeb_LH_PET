using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LH_PET_WEB.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using LH_PET_WEB.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ContextoBanco>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    ));

builder.Services.AddScoped<IEmailService, EmailService>();

// Add services to the container.
builder.Services.AddControllersWithViews();
// 1. SERVIÇOS - antes do Build()
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Autenticacao/Login";
        options.AccessDeniedPath = "/Autenticacao/AcessoNegado";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

var app = builder.Build();

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication(); // 
app.UseAuthorization();
app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();