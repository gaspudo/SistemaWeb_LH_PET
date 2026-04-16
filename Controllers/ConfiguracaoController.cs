using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LH_PET_WEB.Data;
using LH_PET_WEB.Models;

namespace LH_PET_WEB.Controllers
{
    [Authorize] // Permite Administrador e Funcionário
    public class ConfiguracaoController : Controller
    {
        private readonly ContextoBanco _contexto;

        public ConfiguracaoController(ContextoBanco contexto)
        {
            _contexto = contexto;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Busca a configuração única da clinica (Sempre o ID 1)
            var config = await _contexto.Configuracoes.FirstOrDefaultAsync(c => c.Id == 1);

            // Medida de segurança: se alguém apagar do banco sem querer, cria uma em branco na tela
            if (config == null)
            {
                config = new ConfiguracaoClinica();
            }

            return View(config);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Salvar(ConfiguracaoClinica model, List<string> DiasSelecionados)
        {
            // Junta a lista de dias que vieram do formulário (ex: "1", "2") numa string "1,2"
            model.DiasTrabalho = string.Join(",", DiasSelecionados);

            // Ignoramos a validação da string vazia de Dias Trabalho pois montamos ela manualmente acima
            ModelState.Remove("DiasTrabalho");

            if (ModelState.IsValid)
            {
                var configExistente = await _contexto.Configuracoes.FirstOrDefaultAsync(c => c.Id == 1);

                if (configExistente != null)
                {
                    // Atualiza a existente
                    configExistente.HoraAbertura = model.HoraAbertura;
                    configExistente.HoraFechamento = model.HoraFechamento;
                    configExistente.DiasTrabalho = model.DiasTrabalho;
                    configExistente.MinutosConsulta = model.MinutosConsulta;
                    configExistente.MinutosBanho = model.MinutosBanho;
                    configExistente.MinutosTosa = model.MinutosTosa;

                    _contexto.Configuracoes.Update(configExistente);
                }
                else
                {
                    // Se não existir, insere como ID 1
                    model.Id = 1;
                    _contexto.Configuracoes.Add(model);
                }

                await _contexto.SaveChangesAsync();
                TempData["Sucesso"] = "Configurações da agenda atualizadas com sucesso!";
                return RedirectToAction(nameof(Index));
            }

            TempData["Erro"] = "Verifique os campos preenchidos.";
            return View("Index", model);
        }
    }
}