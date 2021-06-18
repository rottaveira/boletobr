using System; 

namespace BoletoBr.Arquivo.CNAB240.Remessa
{
    public class DetalheSegmentoARemessaCnab240
    {
        public DetalheSegmentoARemessaCnab240(Pagamento pagamento, int numeroRegistroNoLote)
        {
            this.CodigoBanco = pagamento.BancoEmpresa.CodigoBanco;
            this.NumeroRegistro = numeroRegistroNoLote;
  
            this.CodigoCamaraCentralizadora = pagamento.CodigoCamaraCentralizadora;
            this.CodigoBancoFavorecido = pagamento?.BancoFavorecido?.CodigoBanco;
            this.AgenciaMantenedoraFavorecido = pagamento?.Favorecido?.ContaBancariaSacado?.Agencia;
            this.DigitoAgenciaMantenedoraFavorecido = pagamento?.Favorecido?.ContaBancariaSacado?.DigitoAgencia;
            this.ContaFavorecido = pagamento?.Favorecido?.ContaBancariaSacado?.Conta;
            this.DigitoContaFavorecido = pagamento?.Favorecido?.ContaBancariaSacado?.DigitoConta;
            this.NomeFavorecido = pagamento?.Favorecido?.Nome;

            this.DataPagamento = pagamento.DataPagamento;
            this.SeuNumero = pagamento.SeuNumero;
            this.ValorPagamento = pagamento.ValorPagamento;
            this.FinalidadePagamento = pagamento.FinalidadePagamento;
            this.CodigoFinalidadeTed = pagamento.CodigoFinalidadeTed; 
            this.CodigoFinalidadeDoc = pagamento.CodigoFinalidadeDoc;
             
        }

        public string CodigoBanco { get; set; }
        public int LoteServico { get; set; }
        

        /// <summary> 
        /// Código da Câmara Centralizadora
        /// Código adotado pela FEBRABAN, para identificar qual Câmara de Centralizadora será responsável pelo processamento dos pagamentos.
        /// Preencher com o código da Câmara Centralizadora para envio do DOC.
        /// Domínio:
        /// '018' = TED(STR, CIP)
        /// '700' = DOC(COMPE)
        /// '888' = TED(STR/CIP) – Utilizado quando for necessário o envio de TED utilizando o código ISPB da
        /// Instituição Financeira Destinatária.Neste caso é obrigatório o preenchimento do campo “Código ISPB” –
        /// Campo 26.3B, do Segmento de Pagamento, conforme descrito na Nota P015.
        /// </summary>
        public string CodigoCamaraCentralizadora { get; set; }
        public int NumeroRegistro { get; set; }
        public string CodigoBancoFavorecido { get; set; }
        public string AgenciaMantenedoraFavorecido { get; set; }
        public DateTime? DataPagamento { get; set; }
        public string SeuNumero { get; set; }
        public string NomeFavorecido { get; set; }
        public string DigitoContaFavorecido { get; set; }
        public string ContaFavorecido { get; set; }
        public string DigitoAgenciaMantenedoraFavorecido { get; set; }
        public decimal ValorPagamento { get; set; }

        /// <summary>    
        /// Código Finalidade Complementar
        /// Código adotado para complemento da finalidade pagamento.
        /// Para finalidade TED deverá ser formatada com o seguinte domínio: 
        ///
        /// CC – para destino com tipo de conta corrente
        /// PP – para destino com tipo de conta poupança
        /// Observação: para modalidade DOC esta informação é definida a partir do código de finalidade DOC
        ///     
        /// </summary>
        public string FinalidadePagamento { get; set; }
        public string CodigoFinalidadeTed { get; set; }
        public string CodigoFinalidadeDoc { get; set; }


    }
}
