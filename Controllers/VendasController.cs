using System;
using System.Security.Claims;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using LH_PET_WEB.Data;
using LH_PET_WEB.Models;
using LH_PET_WEB.Services;

namespace LH_PET_WEB.Controllers
{
    [Authorize]
    public class VendasController : Controller
    {
        private readonly ContextoBanco _contexto;
        private readonly IEmailService _emailService;

        public VendasController(ContextoBanco contexto, IEmailService emailService)
        {
            _contexto = contexto;
            _emailService = emailService;
        }

        [HttpGet]
        public async Task<IActionResult> PDV()
        {
            var produtos = await _contexto.Produtos
                .Where(p => p.Estoque > 0)
                .OrderBy(p => p.Nome)
                .ToListAsync();

                return View(produtos);
        }

        [HttpPost]
        public async Task<IActionResult> Finalizar([FromBody] CheckoutRequest request)
        {
            if (request == null || !request.Itens.Any()) return BadRequest("Dados de checkout inválidos. Carrinho está vazio.");

            var usuarioIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(usuarioIdClaim)) return Unauthorized("Usuário não identificado.");
            int usuarioId = int.Parse(usuarioIdClaim);

            using var transaction = await _contexto.Database.BeginTransactionAsync();

            try {
                var novaVenda = new Venda 
                {
                    UsuarioId = usuarioId,
                    DataVenda = DateTime.Now,
                    FormaPagamento = request.FormaPagamento,
                    Total = 0
                };

                _contexto.Vendas.Add(novaVenda);
                await _contexto.SaveChangesAsync();

                decimal totalVenda = 0;

                foreach (var item in request.Itens) 
                {
                    var produtoBanco = await _contexto.Produtos.FindAsync(item.ProdutoId);
                    if (produtoBanco == null) throw new Exception ($"Produto com ID {item.ProdutoId} não encontrado.");
                    if (produtoBanco.Estoque < item.Quantidade) throw new Exception ($"Estoque insuficiente para o produto {produtoBanco.Nome}.\nRestam: {produtoBanco.Estoque} unidades.   ");
                    produtoBanco.Estoque -= item.Quantidade;
                    _contexto.Produtos.Update(produtoBanco);

                    var novoItem = new ItemVenda 
                    {
                        VendaId = novaVenda.Id,
                        ProdutoId = item.ProdutoId,
                        Quantidade = item.Quantidade,
                        PrecoUnitario = produtoBanco.Preco
                    };

                    totalVenda += item.Quantidade * produtoBanco.Preco;
                    _contexto.ItemVendas.Add(novoItem);

                    novaVenda.Total = totalVenda;
                    _contexto.Vendas.Update(novaVenda);

                    await _contexto.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Ok(new { mensagem = "Venda finalizada com sucesso!", vendaId = novaVenda.Id });
                } catch (Exception ex) 
                {
                    await transaction.RollbackAsync();
                    return BadRequest(new { mensagem = "Erro ao finalizar venda: " + ex.Message });
                } 
            }

            [HttpGet]
            public async Task<IActionResult> Historico()
            {
                var vendas = await _contexto.Vendas
                    .Include(v => v.Usuario)
                    .OrderByDescending(v => v.DataVenda)
                    .ToListAsync();

                return View(vendas);
        }

        [HttpGet]
        public async Task<IActionResult> Recibo(int id)
        {
            var venda = await _contexto.Vendas
                .Include(v => v.Usuario)
                .Include(v => v.Itens)
                    .ThenInclude(i => i.Produto)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venda == null) return NotFound("Venda não encontrada.");

            return View(venda);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnviarPorEmail(int id, string emailCliente)
        {
            var venda = await _contexto.Vendas
                .Include(v => v.Itens)
                    .ThenInclude(i => i.Produto)
                .FirstOrDefaultAsync(v => v.Id == id);
                if ( venda == null) return NotFound("Venda não encontrada.");
                if (string.IsNullOrEmpty(emailCliente)) {
                    TempData["Erro"] = "Digite um email válido.";
                    return RedirectToAction(nameof(Recibo), new { id = venda.Id });
                };

            string itensHtml = "";
            foreach (var item in venda.Itens) 
            {
                itensHtml += $"<li>{item.Produto.Nome} - {item.Quantidade} x {item.Produto!.Nome} - R$ {item.PrecoUnitario:N2}</li>";
            }

            string corpoEmail = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #ddd; padding: 20px; border-radius: 10px;'>
            <h2 style='color: #0d6efd; text-align: center;'>VetPlus Care - Recibo Eletrônico</h2>
            <p>Olá! Agradecemos a sua preferência. Abaixo estão os detalhes da sua compra:</p>
            <p><strong>Recibo Nº:</strong> {venda.Id.ToString("D6")}</p>
            <p><strong>Data:</strong> {venda.DataVenda.ToString("dd/MM/yyyyHH:mm")}</p>
            <p><strong>Pagamento:</strong> {venda.FormaPagamento}</p>
            <hr style='border: 1px solid #eee;'/>
            <ul style='list-style-type: none; padding: 0;'>{itensHtml}</ul>
            <hr style='border: 1px solid #eee;'/>
            <h3 style='text-align: right; color: #198754;'>Total Pago: R${venda.Total:N2}</h3>
            <p style='text-align: center; color: #888; font-size: 12px; margin-top: 30px;'>VetPlus Care System - Documento auxiliar sem valor fiscal.</p>
            </div>";

            bool enviado = await _emailService.EnviarEmailAsync(emailCliente, $"Recibo da Compra #{venda.Id:D6}", corpoEmail);

            if (enviado) {
                TempData["Sucesso"] = "Recibo enviado por email com sucesso!";
            } else {
                TempData["Erro"] = "Houve um erro ao enviar o email. Tente novamente mais tarde.";
            }
            
            return RedirectToAction(nameof(Recibo), new { id = venda.Id });
            }
        }
    } 

    public class CheckoutRequest 
    {
        public List<CarrinhoItem> Itens { get; set; } = new List<CarrinhoItem>();
        public string FormaPagamento { get; set; } = string.Empty;
    }

    public class CarrinhoItem 
    {
        public int ProdutoId { get; set; }
        public int Quantidade { get; set; }
        }
    
}