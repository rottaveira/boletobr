using System; 

namespace BoletoBr.Arquivo.CNAB240.Remessa
{
    public class DetalheSegmentoJRemessaCnab240
    {
        public DetalheSegmentoJRemessaCnab240(Pagamento pagamento, int numeroRegistroNoLote)
        {
            this.CodigoBanco = pagamento.BancoEmpresa.CodigoBanco;
            this.NumeroRegistro = numeroRegistroNoLote;
  
            this.CodBarras = pagamento.CodBarras; 
   
            this.DataPagamento = pagamento.DataPagamento;
            this.SeuNumero = pagamento.SeuNumero;
            this.ValorPagamento = pagamento.ValorPagamento; 

            this.ValorTitulo = pagamento.ValorTitulo; 
            this.ValorAbatimento = pagamento.ValorAbatimento; 
            this.ValorDesconto = pagamento.ValorDesconto; 
            this.ValorMora = pagamento.ValorJurosMora; 
            this.ValorMulta = pagamento.ValorMulta; 
            this.NomeCedente = pagamento.Favorecido.Nome; 
            this.DataVencimento = pagamento.DataVencimento.GetValueOrDefault();

            this.CodigoBancoFavorecido = pagamento?.BancoFavorecido?.CodigoBanco;
        }
         
        public string CodigoBanco { get; set; }
        public int LoteServico { get; set; } 
        public int NumeroRegistro { get; set; } 
        public DateTime? DataPagamento { get; set; }
        
        /// <summary>
        /// Referência Pagador
        /// </summary>
        public string SeuNumero { get; set; }
         
        public string CodBarras { get; set; }
        public decimal ValorPagamento { get; set; }
        public decimal ValorTitulo { get; set; }
        public decimal ValorDesconto { get; set; }
        public decimal ValorAbatimento { get; set; }
        public decimal ValorMulta { get; set; }
        public decimal ValorMora { get; set; } 
        public string NomeCedente{ get; set; } 
        public string CodigoBancoFavorecido { get; set; } 
        public DateTime DataVencimento { get; set; } 
  
    }
}
