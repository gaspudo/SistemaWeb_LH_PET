using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using LH_PET_WEB.Data;
using LH_PET_WEB.Models.ViewModels;
using LH_PET_WEB.Services;
using Microsoft.EntityFrameworkCore;

namespace LH_PET_WEB.Controllers
{
    public class AutenticacaoController : Controller
    {
        private readonly ContextoBanco _contexto;
        private readonly IEmailService _emailService;

        public AutenticacaoController(ContextoBanco contexto, IEmailService emailService)
        {
            _contexto = contexto;
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity is { IsAuthenticated: true }) return RedirectToAction("Index", "Painel");
            return View(new Login ViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(Login ViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var usuario = await _contexto.Usuarios.FirstOrDefaultAsync(u => u.Email == model.Email);

            if (usuario == null || !BCrypt.Net.BCrypt.Verify(model.Senha, usuario.SenhaHash))
            {
                TempData["Erro"] = "E-mail ou senha inválidos.";
                return View(model);
            }

            if (!usuario.Ativo)
            {
                TempData["Erro"] = "Seu usuário está desativado. Contate o administrador.";
                return View(model);
            }

            if (usuario.Senha Temporaria)
            {
                TempData["ResetUsuariold"] = usuario.Id;
                TempData["Aviso Temporario"] = "Sua senha é temporária. Por favor, defina uma nova senha segura para continuar.";
                return RedirectToAction(nameof(RedefinirSenha));
            }

            // Realiza o Login Efetivo passando o NOME agora
            await FazerLoginNoCookie(usuario.Id, usuario.Nome, usuario.Email, usuario.Perfil);
            return RedirectToAction("Index", "Painel");
        }

        [HttpGet]
        public IActionResult RedefinirSenha()
        {
            if (TempData["ResetUsuariold"] == null) return RedirectToAction("Login");
            TempData.Keep("ResetUsuariold");
            return View(new RedefinirSenha ViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RedefinirSenha(RedefinirSenha ViewModel model)
        {
            if (TempData["ResetUsuariold"] == null) return RedirectToAction("Login");

            if (!ModelState.IsValid)
            {
                TempData.Keep("ResetUsuariold");
                return View(model);
            }

            int usuariold = (int)TempData["ResetUsuariold"]!;
            var usuario = await _contexto.Usuarios.FindAsync(usuariold);

            if (usuario != null)
            {
                usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(model.NovaSenha);
                usuario.Senha Temporaria = false;
                _contexto.Usuarios.Update(usuario);
                await _contexto.SaveChangesAsync();

                // Faz o login automaticamente passando o NOME
                await FazerLoginNoCookie(usuario.Id, usuario.Nome, usuario.Email, usuario.Perfil);
                TempData["Sucesso"] = "Senha redefinida com sucesso! Bem-vindo(a).";
                return RedirectToAction("Index", "Painel");
            }

            return RedirectToAction("Login");
        }

        // Método auxiliar atualizado para receber o parâmetro 'nome'
        private async Task FazerLoginNoCookie(int id, string nome, string email, string perfil)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, id.ToString()),
                new Claim(ClaimTypes.Name, nome ?? "Usuário"), // <--- O NOME ENTRA AQUI!
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, perfil)
            };

            var identidade = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identidade);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        }

        [HttpGet]
        public async Task<IActionResult> Sair()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult EsqueciSenha()
        {
            return View(new EsqueciSenha ViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EsqueciSenha(EsqueciSenha ViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var usuario = await _contexto.Usuarios.FirstOrDefaultAsync(u => u.Email == model.Email);

            if (usuario != null)
            {
                string senha Temporaria = Guid.NewGuid().ToString().Substring(0, 8);
                usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(senha Temporaria);
                usuario.Senha Temporaria = true;

                _contexto.Usuarios.Update(usuario);
                await _contexto.SaveChangesAsync();

                string mensagem = $"Olá {usuario.Nome}!\n\nUma redefinição de senha foi solicitada.\nSua nova senha temporária é: {senha Temporaria}\n\nVocê será solicitado a alterá-la no próximo acesso.";
                bool emailEnviado = await _emailService.EnviarEmailAsync(usuario.Email, "Recuperação de Senha - VetPlus Care", mensagem);

                if (!emailEnviado)
                {
                    TempData["Erro"] = "Serviço de e-mail indisponível. Contate o suporte para redefinir sua senha.";
                }

                return RedirectToAction("Login");
            }

            TempData["Sucesso"] = "Se o e-mail estiver cadastrado, você receberá as instruções em breve.";
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult AcessoNegado()
        {
            return View();
        }
    }
}