using BoletoBr.Interfaces;
using System; 

namespace BoletoBr
{
   public class Pagamento
    {

        public IBanco BancoFavorecido { get; set; }
        public IBanco BancoEmpresa { get; set; }
         
        public Sacado Favorecido { get; set; }
        public Cedente Empresa { get; set; }

        public string CodigoBanco { get; set; }
        public string Mensagem { get; set; }

        /// <summary>
        /// Lote de Serviço
        ///  Número sequencial para identificar univocamente um lote de serviço.Criado e controlado pelo responsável
        ///  pela geração magnética dos dados contidos no arquivo.
        ///  Preencher com '0001' para o primeiro lote do arquivo.Para os demais: número do lote anterior acrescido
        ///  de 1. O número não poderá ser repetido dentro do arquivo.
        ///  Se registro for Header do Arquivo preencher com '0000' 
        ///  Se registro for Trailer do Arquivo preencher com '9999'
        /// </summary>
        public int LoteServico { get; set; }
         
        public string CodigoCamaraCentralizadora { get; set; }
        public int NumeroRegistro { get; set; }
        public string CodigoBancoFavorecido { get; set; } 
        public DateTime? DataPagamento { get; set; }
        public DateTime? DataVencimento { get; set; }
        public string SeuNumero { get; set; } 
        public decimal ValorPagamento { get; set; }

        public decimal ValorMulta { get; set; }
        public decimal ValorJurosMora { get; set; }
        public decimal ValorDesconto { get; set; }
        public decimal ValorAbatimento { get; set; }
        public decimal ValorTitulo { get; set; }
         
        public string FinalidadePagamento { get; set; }
        public string CodigoFinalidadeTed { get; set; }
        public string CodigoFinalidadeDoc { get; set; }
        public string CodigoConvenio { get; set; }
        public string TipoServico { get; set; }

        /// <summary>
        /// Forma de Lançamento
        /// Código adotado pela FEBRABAN para identificar a operação que está contida no lote.
        /// Domínio:
        /// O campo (Forma de Lançamento), aceita os seguintes valores:
        /// '01' = Crédito em Conta Corrente
        /// '03' = DOC
        /// '05' = Crédito em Conta Poupança
        /// '41' = TED – Outra Titularidade(1)
        /// '43' = TED – Mesma Titularidade(1)
        /// '30' = Liquidação de Títulos do Próprio Banco
        /// '31' = Pagamento de Títulos de Outros Bancos
        /// ‘16’ = Tributo - DARF Normal
        /// ‘17’ = Tributo - GPS(Guia da Previdência Social)
        /// ‘18’ = Tributo - DARF Simples
        /// ‘11’ = Pagamento de Contas e Tributos com Código de Barras
        /// </summary>
        public string FormaDeLancamento { get; set; }

        /// <summary>
        /// Indicativo de Forma de Pagamento (SICOOB)
        /// Possibilitar ao Pagador, mediante acordo com o seu Banco de Relacionamento, a forma de pagamento
        /// do compromisso.
        /// 01 - Débito em Conta Corrente
        /// 02 – Débito Empréstimo/Financiamento
        /// 03 – Débito Cartão de Crédito 
        /// </summary>
        public string FormaDePagamento { get; set; }
        public string CodBarras { get; set; }
    }
}
