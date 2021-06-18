using BoletoBr.Interfaces;

namespace BoletoBr.Dominio
{
    public class CodigoOcorrencia : ICodigoOcorrencia
    {
        public int Codigo { get; set; }
        public string CodigoText { get; set; }
        public int QtdDias { get; set; }
        public double Valor { get; set; }
        public string Descricao { get; set; }

        public CodigoOcorrencia( )
        { 
        }
        public CodigoOcorrencia(int codigo)
        {
            this.Codigo = codigo;
        }

        public CodigoOcorrencia(string OcorrenciaPagamento)
        {
            this.CodigoText = OcorrenciaPagamento;
        }

        public static string ValidaCodigo(ICodigoOcorrencia ocorrencia)
        {
            try
            {
                return ocorrencia.Codigo.ToString();
            }
            catch
            {
                return "0";
            }
        }
    }
}
