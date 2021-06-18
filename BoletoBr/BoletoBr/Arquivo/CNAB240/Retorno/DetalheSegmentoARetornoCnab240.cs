using System; 

namespace BoletoBr.Arquivo.CNAB240.Retorno
{
    public class DetalheSegmentoARetornoCnab240
    { 
        public string CodigoBanco { get; set; }
        public int LoteServico { get; set; }
        public string CodigoSegmento { get; set; }

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
        public DateTime? DataEfetivacaoPagamento { get; set; }
        public string SeuNumero { get; set; }
        public string NomeFavorecido { get; set; }
        public string DigitoContaFavorecido { get; set; }
        public string ContaFavorecido { get; set; }
        public string DigitoAgenciaMantenedoraFavorecido { get; set; }
        public decimal ValorPagamento { get; set; }
        public decimal ValorRealEfetivacaoPagamento { get; set; }

 
        public string CodigoFinalidadeTed { get; set; }
        public string CodigoFinalidadeDoc { get; set; }
        public string CodigoFinalidadeComplementar { get; set; }
        public string NossoNumero { get; set; }
        public string Mensagem { get; set; }
        public string AvisoFavorecido { get; set; }
        public string Ocorrencia { get; set; }

        public int CodigoMovimento { get; set; }


    }
}
