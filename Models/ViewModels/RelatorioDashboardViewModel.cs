using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LH_PET_WEB.Models.ViewModels
{
    public class RelatorioDashboardViewModel
    {
        public decimal FaturamentoHoje { get; set; }
        public decimal FaturamentoMes { get; set; }
        public int QuantidadeVendasMes { get; set; }

       public List<TopProdutosViewModel> TopProdutos { get; set; } = new List<TopProdutosViewModel>();
       public Dictionary<string, decimal> FaturamentoPorPagamento { get; set; } = new Dictionary<string, decimal>();

    }

    public class TopProdutosViewModel
    {
        public string NomeProduto { get; set; } = string.Empty;
        public int QuantidadeVendida { get; set; }
        public decimal ValorTotalGerado { get; set; }
    }
}