using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using LH_PET_WEB.Data;
using LH_PET_WEB.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using LH_PET_WEB.Models;
using LH_PET_WEB.Services;

namespace LH_PET_WEB.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsuariosController : Controller
    {
        private readonly ContextoBanco _contexto;
        private readonly IEmailService emailService;

        public UsuariosController(ContextoBanco contexto, IEmailService emailService)
        {
            _contexto = contexto;
            this.emailService = emailService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var usuarios = await _contexto.Usuarios
                .Where(u => u.Perfil != "Cliente")
                .ToListAsync();
            
            return View(usuarios);
        }

        [HttpGet]
        public IActionResult Criar()
        {
            return View(new UsuarioCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Criar(UsuarioCreateViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            
            var existe = await _contexto.Usuarios.AnyAsync(u => u.Email == model.Email);
            if (existe)
            {
                ModelState.AddModelError("Email", "Já existe um usuário com este email.");
                return View(model);
            }

            string senhaGerada = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 10);

            var novoUsuario = new Usuario
            {
                Nome = model.Nome,
                Email = model.Email,
                Perfil = model.Perfil,
                Ativo = true,
                SenhaTemporaria = true
                SenhaHash = BCrypt.Net.BCrypt.HashPassword(senhaGerada)
            };

            _contexto.Usuarios.Add(novoUsuario);
            await _contexto.SaveChangesAsync();

            string mensagem = $"Olá {model.Nome}, bem vindo(a) à VetPlus Care!\n\nSeu acesso como {model.Perfil} foi criado com sucesso.\n\nSua senha temporária é: {senhaGerada}\n\nPor favor, faça login e altere sua senha o mais breve possível.";
            bool emailEnviado = await emailService.EnviarEmailAsync(model.Email, "Bem-vindo à VetPlus Care - Acesso Criado", mensagem);

            if (emailEnviado)
            {
                TempData["Sucesso"] = "Usuário criado com sucesso e email de boas-vindas enviado.";
            }
            else
            {
                TempData["Sucesso"] = "Usuário criado, mas houve um problema ao enviar o email de boas-vindas.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlternarStatus(int id)
        {
            var usuario = await _contexto.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();
            
            var emailLogado = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value;
            if (usuario.Email == emailLogado)
            {
                TempData["Erro"] = "Você não pode desativar sua própria conta.";
                return RedirectToAction(nameof(Index));
            }

            usuario.Ativo = !usuario.Ativo;
            _contexto.Usuarios.Update(usuario);
            await _contexto.SaveChangesAsync();

            TempData["Sucesso"] = $"Status de {usuario.Nome} alterado com sucesso.";
            return RedirectToAction(nameof(Index));
        }
    }
}