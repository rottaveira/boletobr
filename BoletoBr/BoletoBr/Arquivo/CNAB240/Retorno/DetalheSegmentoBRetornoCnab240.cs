using System; 

namespace BoletoBr.Arquivo.CNAB240.Retorno
{
    public class DetalheSegmentoBRetornoCnab240
    { 

        /// <summary>
        /// Nome da Rua, Av, Pça, Etc
        /// </summary>
        public string LogradouroFavorecido { get; set; }
        /// <summary>
        /// Nº do Local
        /// </summary>
        public string NumeroFavorecido { get; set; }
        /// <summary>
        /// Casa, Apto, Etc
        /// </summary>
        public string ComplementoFavorecido { get; set; }
        public string BairroFavorecido { get; set; }
        public string CidadeFavorecido { get; set; }
        /// <summary>
        /// 5 primeiros nros
        /// </summary>
        public string CEPFavorecido { get; set; }
        /// <summary>
        /// 3 ultimos nros
        /// </summary>
        public string ComplementoCEPFavorecido { get; set; }
        public string EstadoFavorecido { get; set; }
        /// <summary>
        /// Código / Documento do Favorecido
        /// Número ou Código de documento para identificar o Favorecido.O conteúdo deste campo não sofrerá nenhum tratamento por parte do Banco.
        /// </summary>
        public string CodigoFavorecido { get; set; }
        public string Aviso { get; set; }
        public string CodigoRegistro { get; set; }
        public string CodigoSegmento { get; set; }

        public DateTime? DataVencimento { get; set; }
        public string NumeroInscricaoFavorecido { get; set; }
        public string TipoInscricaoFavorecido { get; set; }
        public int NumeroRegistro { get; set; }
        public int LoteServico { get; set; }
        public string CodigoBanco { get; set; }
        public decimal? ValorMulta { get; set; }
        public decimal? ValorJurosMora { get; set; }
        public decimal? ValorDesconto { get; set; }
        public decimal? ValorAbatimento { get; set; }
        public decimal? ValorPagamento { get; set; }
    }
}
