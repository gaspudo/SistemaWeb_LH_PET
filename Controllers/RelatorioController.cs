using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using LH_PET_WEB.Data;
using LH_PET_WEB.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace LH_PET_WEB.Controllers
{
    [Authorize (Roles = "Admin")]
    public class RelatoriosController : Controller 
    {
        private readonly ContextoBanco _contexto;

        public RelatoriosController(ContextoBanco contexto) {
            _contexto = contexto;
        }
    
    public async Task<IActionResult> Index()
    {
        var hoje = DateTime.Today;
        var inicioDoMes = new DateTime(hoje.Year, hoje.Month, 1);

        var vendasMes = await _contexto.Vendas.Where(v => v.DataVenda >= inicioDoMes).ToListAsync();
        
        var vendasHoje = await _contexto.Vendas.Where(v => v.DataVenda.Date == hoje).ToListAsync();

        var faturamentoPagamento = vendasMes
            .GroupBy(v => v.FormaPagamento)
            .ToDictionary(g => g.Key, g => g.Sum(v => v.Total));

        var topProdutos = await _contexto.ItensVenda
            .Include(i => i.Produto)
            .Include(i => i.Venda)
            .Where(i => i.Venda!.DataVenda >= inicioDoMes)
            .GroupBy(i => new {i.ProdutoId, i.Produto!.Nome})
            .Select(g => new TopProdutosViewModel {
                NomeProduto = g.Key.Nome,
                QuantidadeVendida = g.Sum(i => i.Quantidade),
                ValorTotalGerado = g.Sum(i => i.Quantidade * i.PrecoUnitario)
            })
            .OrderByDescending(p => p.QuantidadeVendida)
            .Take(5)
            .ToListAsync();

        var viewModel = new RelatorioDashboardViewModel()
        {
            FaturamentoHoje = vendasHoje.Sum(v => v.Total),
            FaturamentoMes = vendasMes.Sum(v => v.Total),
            QuantidadeVendasMes = vendasMes.Count,
            FaturamentoPorPagamento = faturamentoPagamento,
            TopProdutos = topProdutos
        };
        return View(viewModel);
    }

    }
}