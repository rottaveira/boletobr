using System;

namespace BoletoBr.Arquivo.CNAB240.Retorno
{
    public class DetalheSegmentoJRetornoCnab240
    {

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
        public string NomeCedente { get; set; } 
        public string Ocorrencia { get; set; }
        public string NossoNumero { get; set; }
        public decimal ValorAcrescimo { get; set; }
        public string CodigoSegmento { get; set; }
        public string CodigoRegistro { get; set; }
        public DateTime? DataVencimento { get; set; } 
    }
}
