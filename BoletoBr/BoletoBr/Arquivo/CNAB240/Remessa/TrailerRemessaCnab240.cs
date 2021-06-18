
namespace BoletoBr.Arquivo.CNAB240.Remessa
{
    public class TrailerRemessaCnab240
    {
        public TrailerRemessaCnab240(int qtdLotes, int qtdRegistros)
        {
            this.QtdLotesArquivo = qtdLotes;
            this.QtdRegistrosArquivo = qtdRegistros;
        }

        public int CodigoBanco { get; set; }
        public int LoteServico { get; set; }
        public int TipoRegistro { get; set; }
        public int QtdLotesArquivo { get; set; }
        public int QtdRegistrosArquivo { get; set; }

        /// <summary>
        /// Quantidade de Contas para Conciliação (Lotes)
        /// Número indicativo de lotes de Conciliação Bancária enviados no arquivo.Somatória dos registros de tipo 1 
        /// e Tipo de Operação = 'E'.
        /// Campo específico para o serviço de Conciliação Bancária.
        /// </summary>
        public int QtdContasConciliacao { get; set; }
    }
}
