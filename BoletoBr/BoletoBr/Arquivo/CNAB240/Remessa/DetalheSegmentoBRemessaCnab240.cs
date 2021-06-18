using System; 

namespace BoletoBr.Arquivo.CNAB240.Remessa
{
    public class DetalheSegmentoBRemessaCnab240
    {
        public DetalheSegmentoBRemessaCnab240(Pagamento pagamento, int numeroRegistroNoLote)
        {
            this.CodigoBanco = pagamento.BancoEmpresa.CodigoBanco;
            this.NumeroRegistro = numeroRegistroNoLote;
             
            this.NumeroInscricaoFavorecido = pagamento?.Favorecido?.CpfCnpj; 
            this.LogradouroFavorecido = pagamento?.Favorecido?.EnderecoSacado?.Logradouro;
            this.BairroFavorecido = pagamento?.Favorecido?.EnderecoSacado?.Bairro;
            this.ComplementoFavorecido = pagamento?.Favorecido?.EnderecoSacado?.Complemento;
            this.CidadeFavorecido= pagamento?.Favorecido?.EnderecoSacado?.Cidade;
            this.NumeroFavorecido = pagamento?.Favorecido?.EnderecoSacado?.Numero;
            this.EstadoFavorecido = pagamento?.Favorecido?.EnderecoSacado?.SiglaUf;

            this.CodigoFavorecido = pagamento?.Favorecido?.CodigoSacado;
            this.DataVencimento = pagamento?.DataVencimento; 
            this.ValorMulta = pagamento?.ValorMulta;
            this.ValorJurosMora = pagamento?.ValorJurosMora;
            this.ValorDesconto = pagamento?.ValorDesconto;
            this.ValorAbatimento = pagamento?.ValorAbatimento;
            this.ValorPagamento = pagamento?.ValorPagamento;

            var cep = pagamento?.Favorecido?.EnderecoSacado?.Cep.Replace("-","").Replace(".","");

            this.CEPFavorecido = cep.Substring(0, 5);
            this.ComplementoCEPFavorecido = cep.Substring(5, 3); 
        }
         

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

        public DateTime? DataVencimento { get; set; }
        public string NumeroInscricaoFavorecido { get; set; }
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
