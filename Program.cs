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

using (var scope = app.Services.CreateScope())
{
    var contexto = scope.ServiceProvider.GetRequiredService<ContextoBanco>();
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

    var emailAdmin = config["AdminInicial:Email"];
    var senhaAdmin = config["AdminInicial:Senha"];

    if (!contexto.Usuarios.Any(u => u.Email == emailAdmin))
    {
        contexto.Usuarios.Add(new LH_PET_WEB.Models.Usuario
        {
            Nome = "Administrador",
            Email = emailAdmin!,
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(senhaAdmin),
            Perfil = "Admin",
            Ativo = true,
            SenhaTemporaria = false
        });
        contexto.SaveChanges();
    }
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication(); // 
app.UseAuthorization();
app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();