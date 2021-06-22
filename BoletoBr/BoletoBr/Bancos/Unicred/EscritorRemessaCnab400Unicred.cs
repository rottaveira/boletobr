using System;
using System.Collections.Generic;
using BoletoBr.Arquivo.CNAB400.Remessa;
using BoletoBr.Fabricas;
using BoletoBr.Interfaces;

namespace BoletoBr.Bancos.UniCred
{
    public class EscritorRemessaCnab400Unicred : IEscritorArquivoRemessaCnab400
    {
        private RemessaCnab400 _remessaEscrever;

        public EscritorRemessaCnab400Unicred(RemessaCnab400 remessaEscrever)
        {
            _remessaEscrever = remessaEscrever;
        }

        public List<string> EscreverTexto(RemessaCnab400 remessaEscrever)
        {

            List<string> listaRetornar = new List<string>();

            listaRetornar.Add(EscreverHeader(remessaEscrever.Header));

            var sequencial = 2;
            var codCedente = remessaEscrever.Header.ContaCorrente;

            foreach (var detalheAdicionar in remessaEscrever.RegistrosDetalhe)
            {
                listaRetornar.AddRange(new[] { EscreverDetalhe(detalheAdicionar, sequencial) });
                sequencial++; 
            }

            listaRetornar.Add(EscreverTrailer(remessaEscrever.Trailer, sequencial, codCedente));

            return listaRetornar;
        }

        public string EscreverHeader(HeaderRemessaCnab400 infoHeader)
        {
            var nomeEmpresa = "";
            if (infoHeader.NomeEmpresa.Length > 30)
                nomeEmpresa = infoHeader.NomeEmpresa.Substring(0, 30);
            else
                nomeEmpresa = infoHeader.NomeEmpresa.ToUpper();

            var header = new string(' ', 400);
            try
            {
                header = header.PreencherValorNaLinha(1, 1, "0");
                header = header.PreencherValorNaLinha(2, 2, "1");
                header = header.PreencherValorNaLinha(3, 9, "REMESSA");
                header = header.PreencherValorNaLinha(10, 11, "01");
                header = header.PreencherValorNaLinha(12, 26, "COBRANCA".PadRight(15, ' '));
                header = header.PreencherValorNaLinha(27, 46, infoHeader.CodigoEmpresa.PadLeft(20, '0'));
                header = header.PreencherValorNaLinha(47, 76, infoHeader.NomeEmpresa.PadRight(30, ' '));
                header = header.PreencherValorNaLinha(77, 79, "136");
                header = header.PreencherValorNaLinha(80, 94, "UNICRED".PadRight(15, ' '));
                header = header.PreencherValorNaLinha(95, 100, DateTime.Now.ToString("ddMMyy").Replace("/", ""));
                header = header.PreencherValorNaLinha(101, 107, string.Empty.PadLeft(7, ' '));
                header = header.PreencherValorNaLinha(108, 110, "000");
                header = header.PreencherValorNaLinha(111, 117, infoHeader.NumeroSequencialRemessa.ToString().PadLeft(7, '0'));
                header = header.PreencherValorNaLinha(118, 394, string.Empty.PadRight(277, ' '));
                header = header.PreencherValorNaLinha(395, 400, "000001");

                return header;
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("BoletoBr{0}Falha na geração do HEADER do arquivo de REMESSA.",
                    Environment.NewLine), e);
            }
        }

        public string EscreverDetalhe(DetalheRemessaCnab400 infoDetalhe, int sequenciaDetalhe)
        {
            if (String.IsNullOrEmpty(infoDetalhe.BairroPagador))
                throw new Exception("Não foi informado o bairro do pagador " + infoDetalhe.NomePagador + "(" +
                                    infoDetalhe.InscricaoPagador + ")");

            if (String.IsNullOrEmpty(infoDetalhe.CepPagador) || infoDetalhe.CepPagador.Length < 8)
                throw new Exception("CEP inválido! Verifique o CEP do pagador " + infoDetalhe.NomePagador + "(" +
                                    infoDetalhe.InscricaoPagador + ")");

            if (String.IsNullOrEmpty(infoDetalhe.NumeroDocumento))
                throw new Exception("Informe um número de documento!");
             
            
            #region Variáveis
            var detalhe = new string(' ', 400);
            try
            { 
                var objBanco = BancoFactory.ObterBanco(infoDetalhe.CodigoBanco);

                string nossoNumeroSequencial =
                    infoDetalhe.NossoNumeroFormatado.Replace(".", "").Replace("/", "").Replace("-", "");

                #endregion

                infoDetalhe.Instrucao1 = infoDetalhe.Instrucao1 == null ? "01" : infoDetalhe.Instrucao1;
                #region Instrução1
                /* Este campo só permite usar os seguintes códigos:
                    01 - Cadastro de Títulos
                    02 - Pedido de Baixa
                    04 - Concessão de Abatimento
                    05 - Cancelamento de Abatimento
                    06 - Alteração de Vencimento
                    09 - Pedido de Protesto
                    18 - Sustar protesto e baixar título
                    19 - Sustar protesto e manter em carteira
                    31 - Alteração de outros dados
                    45 - Incluir negativação
                    75 - Excluir negativação e manter na carteira
                    76 - Excluir negativação e baixar títulos             */
                #endregion

                /*Código adotado pela FEBRABAN para identificação do tipo de pagamento de multa.
                    Domínio:
                    ‘1’ = Valor Fixo (R$)
                    ‘2’ = Taxa (%)
                    ‘3’ = Isento                */
                var codigoMulta = infoDetalhe.PercentualMulta > 0 ? "2" : "3";

                /*Código adotado pela FEBRABAN para identificação do tipo de pagamento de mora de juros.
                    Domínio:
                    ‘1’ = Valor Diário (R$)
                    ‘2’ = Taxa Mensal (%)
                    ‘3’= Valor Mensal (R$) *
                    ‘4’ = Taxa diária (%)
                    ‘5’ = Isento
                    *OBSERVAÇÃO:
                    ‘3’ - Valor Mensal (R$): a CIP não acata valor mensal, segundo manual. Cógido mantido
                    para Correspondentes que ainda utilizam.*/
                var codigoMora = infoDetalhe.ValorMoraDia > 0 ? "1" : "5"; ;

                /*Código adotado pela FEBRABAN para identificação do desconto.
                    Domínio:
                    0 = Isento
                    1 = Valor Fixo*/
                var codigoDesconto = infoDetalhe.ValorDesconto > 0 ? "1" : "0";

                /*Código para Protesto*
                    Código adotado pela FEBRABAN para identificar o tipo de prazo a ser considerado para o
                    protesto.
                    Domínio:
                    1 = Protestar Dias Corridos
                    2 = Protestar Dias Úteis
                    3 = Não Protestar
                */
                var protesto = infoDetalhe.NroDiasParaProtesto > 0 ? "2" : "3";

                detalhe = detalhe.PreencherValorNaLinha(1, 1, "1"); // Identificação do Registro Transação
                detalhe = detalhe.PreencherValorNaLinha(2, 6,  infoDetalhe.Agencia.ToString().PadLeft(5,'0')); //Agência do BENEFICIÁRIO na UNICRED  
                detalhe = detalhe.PreencherValorNaLinha(7, 7,  infoDetalhe.DvAgencia.PadRight(1, ' '));
                detalhe = detalhe.PreencherValorNaLinha(8, 19, infoDetalhe.ContaCorrente.ToString().PadLeft(12, '0'));
                detalhe = detalhe.PreencherValorNaLinha(20, 20, infoDetalhe.DvContaCorrente.ToString().PadRight(1, ' '));
                detalhe = detalhe.PreencherValorNaLinha(21, 21, "0");
                detalhe = detalhe.PreencherValorNaLinha(22, 24, "021"); //Código carteira
                detalhe = detalhe.PreencherValorNaLinha(25, 37, string.Empty.PadLeft(13, '0'));  
                detalhe = detalhe.PreencherValorNaLinha(38, 62, string.Empty.PadLeft(25, ' ')); //Uso Empresa 
                detalhe = detalhe.PreencherValorNaLinha(63, 65, "136"); //cod Banco
                detalhe = detalhe.PreencherValorNaLinha(66, 67, "00"); 
                detalhe = detalhe.PreencherValorNaLinha(68, 92, string.Empty.PadLeft(25, ' '));
                detalhe = detalhe.PreencherValorNaLinha(93, 93, "0");
                detalhe = detalhe.PreencherValorNaLinha(94, 94, codigoMulta);
                detalhe = detalhe.PreencherValorNaLinha(95, 104, infoDetalhe.PercentualMulta.ToStringParaValoresDecimais().PadLeft(10, '0'));
                detalhe = detalhe.PreencherValorNaLinha(105, 105, codigoMora);
                detalhe = detalhe.PreencherValorNaLinha(106, 106, "N");
                detalhe = detalhe.PreencherValorNaLinha(107, 108, string.Empty.PadLeft(2, ' '));
                detalhe = detalhe.PreencherValorNaLinha(109, 110, "01"); /*01 = remessa*/
                detalhe = detalhe.PreencherValorNaLinha(111, 120, infoDetalhe.NumeroDocumento.PadRight(10, ' '));
                detalhe = detalhe.PreencherValorNaLinha(121, 126, infoDetalhe.DataVencimento.ToString("ddMMyy").Replace("/", ""));/*A data de vencimento deve ser sete dias MAIOR que a data de emissão (campos 151-156). Formato: DDMMAA*/
                detalhe = detalhe.PreencherValorNaLinha(127, 139, infoDetalhe.ValorBoleto.ToStringParaValoresDecimais().PadLeft(13, '0'));
                detalhe = detalhe.PreencherValorNaLinha(140, 149, string.Empty.PadLeft(10, '0'));
                detalhe = detalhe.PreencherValorNaLinha(150, 150, codigoDesconto);
                detalhe = detalhe.PreencherValorNaLinha(151, 156, infoDetalhe.DataEmissao.ToString("ddMMyy").Replace("/", ""));
                detalhe = detalhe.PreencherValorNaLinha(157, 157, "0"); 
                detalhe = detalhe.PreencherValorNaLinha(158, 158, protesto);  
                detalhe = detalhe.PreencherValorNaLinha(159, 160, infoDetalhe.NroDiasParaProtesto.ToString().PadLeft(2,'0'));  
                detalhe = detalhe.PreencherValorNaLinha(161, 173, infoDetalhe.ValorJuros.ToStringParaValoresDecimais().PadLeft(13, '0'));
               
                if (infoDetalhe.DataLimiteConcessaoDesconto > DateTime.MinValue)
                    detalhe = detalhe.PreencherValorNaLinha(174, 179, infoDetalhe.DataLimiteConcessaoDesconto.ToString("ddMMyy").Replace("/", ""));
                else
                    detalhe = detalhe.PreencherValorNaLinha(174, 179, "000000");

                detalhe = detalhe.PreencherValorNaLinha(180, 192, infoDetalhe.ValorDesconto.ToStringParaValoresDecimais().PadLeft(13, '0'));
                detalhe = detalhe.PreencherValorNaLinha(193, 203, infoDetalhe.NossoNumero.Replace("/", "").Replace("-", "").PadLeft(11,'0'));
                detalhe = detalhe.PreencherValorNaLinha(204, 205, "00");
                detalhe = detalhe.PreencherValorNaLinha(206, 218, infoDetalhe.ValorAbatimento.ToStringParaValoresDecimais().PadLeft(13, '0'));
                detalhe = detalhe.PreencherValorNaLinha(219, 220, infoDetalhe.InscricaoPagador.Replace(".", "").Replace("/", "").Replace("-", "").Length == 11 ? "01" : "02");
                detalhe = detalhe.PreencherValorNaLinha(221, 234, infoDetalhe.InscricaoPagador.Replace(".", "").Replace("/", "").Replace("-", "").PadLeft(14, '0'));
                detalhe = detalhe.PreencherValorNaLinha(235, 274, infoDetalhe.NomePagador.PadLeft(40, ' '));

                string endereco = infoDetalhe.EnderecoPagador.Replace(".", "").Replace("/", "").Replace(",", "").Replace("-", "");
                if (endereco.Length > 40)
                    endereco = endereco.Substring(0, 40);

                string bairro = infoDetalhe.BairroPagador.Replace(".", "").Replace("/", "").Replace(",", "").Replace("-", "");
                if (bairro.Length > 12)
                    bairro = bairro.Substring(0, 12);

                string cidade = infoDetalhe.CidadePagador.Replace(".", "").Replace("/", "").Replace("-", "");
                if (cidade.Length > 20)
                    cidade = cidade.Substring(0, 20);

                detalhe = detalhe.PreencherValorNaLinha(275, 314, endereco.PadRight(40, ' '));
                detalhe = detalhe.PreencherValorNaLinha(315, 326, bairro.PadRight(12, ' ')); 
                detalhe = detalhe.PreencherValorNaLinha(327, 334, infoDetalhe.CepPagador.Replace(".", "").Replace("/", "").Replace("-", "").PadRight(8, ' '));
                detalhe = detalhe.PreencherValorNaLinha(335, 354, cidade.PadRight(20, ' '));
                detalhe = detalhe.PreencherValorNaLinha(355, 356, infoDetalhe.UfPagador.Replace(".", "").Replace("/", "").Replace("-", "").PadRight(2, ' '));
                detalhe = detalhe.PreencherValorNaLinha(357, 394, string.Empty.PadLeft(38, ' ')); 
                detalhe = detalhe.PreencherValorNaLinha(395, 400, sequenciaDetalhe.ToString().PadLeft(6, '0'));
                 
                return detalhe;
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("<BoletoBr>{0}Falha na geração do DETALHE do arquivo de REMESSA.",
                    Environment.NewLine), e);
            }
        }
  
        public string EscreverTrailer(TrailerRemessaCnab400 infoTrailer, int sequenciaTrailer, string contaCedente)
        {
            var trailer = new string(' ', 400);
            try
            {
                trailer = trailer.PreencherValorNaLinha(1, 1, "9");
                trailer = trailer.PreencherValorNaLinha(2, 394, string.Empty.PadLeft(393, ' ')); 
                trailer = trailer.PreencherValorNaLinha(395, 400, sequenciaTrailer.ToString().PadLeft(6, '0'));

                return trailer;
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("<BoletoBr>{0}Falha na geração do TRAILER do arquivo de REMESSA.",
                    Environment.NewLine), e);
            }
        }

        public void ValidarRemessa(RemessaCnab400 remessaValidar)
        {
            throw new NotImplementedException();
        }
    }
}
