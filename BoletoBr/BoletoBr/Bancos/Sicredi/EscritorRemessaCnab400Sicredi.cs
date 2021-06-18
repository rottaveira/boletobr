using System;
using System.Collections.Generic;
using BoletoBr.Arquivo.CNAB400.Remessa;
using BoletoBr.Fabricas;
using BoletoBr.Interfaces;

namespace BoletoBr.Bancos.Sicredi
{
    public class EscritorRemessaCnab400Sicredi : IEscritorArquivoRemessaCnab400
    {
        private RemessaCnab400 _remessaEscrever;

        public EscritorRemessaCnab400Sicredi(RemessaCnab400 remessaEscrever)
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

                /*Opcional*/
                //listaRetornar.AddRange(new[] { EscreverRegistroMensagem(detalheAdicionar, sequencial) });
                //sequencial++;          
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
                header = header.PreencherValorNaLinha(27, 31, infoHeader.ContaCorrente.PadLeft(4, '0'));
                header = header.PreencherValorNaLinha(32, 45, infoHeader.CpfCnpjCedente.Replace(".", "").Replace("/", "").Replace("-", "").PadLeft(14, '0'));
                header = header.PreencherValorNaLinha(46, 76, string.Empty.PadLeft(31, ' '));
                header = header.PreencherValorNaLinha(77, 79, "748");
                header = header.PreencherValorNaLinha(80, 94, "SICREDI".PadRight(15, ' '));
                header = header.PreencherValorNaLinha(95, 102, DateTime.Now.ToString("yyyyMMdd").Replace("/", ""));
                header = header.PreencherValorNaLinha(103, 110, string.Empty.PadLeft(8, ' '));
                header = header.PreencherValorNaLinha(111, 117, infoHeader.NumeroSequencialRemessa.ToString().PadLeft(7, '0'));
                header = header.PreencherValorNaLinha(118, 390, string.Empty.PadRight(273, ' '));
                header = header.PreencherValorNaLinha(391, 394, "2.00");
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

            if ((infoDetalhe.DataVencimento - infoDetalhe.DataEmissao).Days < 1)
                throw new Exception("A data de vencimento deve ser no minimo um dia MAIOR que a data de emissão.");

            if (infoDetalhe.Especie == null)
                throw new Exception("Espécie do documento não informada! São aceitas:{A,B,C,D,E,F,H,I,J,O,K}");

            if (!infoDetalhe.Especie.Sigla.Equals("A") && //A - Duplicata Mercantil por Indicação
                !infoDetalhe.Especie.Sigla.Equals("B") && //B - Duplicata Rural;
                !infoDetalhe.Especie.Sigla.Equals("C") && //C - Nota Promissória;
                !infoDetalhe.Especie.Sigla.Equals("D") && //D - Nota Promissória Rural;
                !infoDetalhe.Especie.Sigla.Equals("E") && //E - Nota de Seguros;
                !infoDetalhe.Especie.Sigla.Equals("F") && //G – Recibo;
                !infoDetalhe.Especie.Sigla.Equals("H") && //H - Letra de Câmbio;
                !infoDetalhe.Especie.Sigla.Equals("I") && //I - Nota de Débito;
                !infoDetalhe.Especie.Sigla.Equals("J") && //J - Duplicata de Serviço por Indicação;
                !infoDetalhe.Especie.Sigla.Equals("O") && //O – Boleto Proposta
                !infoDetalhe.Especie.Sigla.Equals("K") //K – Outros.
               )
                throw new Exception("Informe o código da espécie documento! São aceitas:{A,B,C,D,E,F,H,I,J,O,K}");

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

                detalhe = detalhe.PreencherValorNaLinha(1, 1, "1"); // Identificação do Registro Transação
                detalhe = detalhe.PreencherValorNaLinha(2, 2, "A"); // Tipo de Inscrição da Empresa
                detalhe = detalhe.PreencherValorNaLinha(3, 3, "A");
                detalhe = detalhe.PreencherValorNaLinha(4, 4, "A"); /*Tipo Impressao A - Normal B - Carnê */
                detalhe = detalhe.PreencherValorNaLinha(5, 16, string.Empty.PadLeft(12, ' '));
                detalhe = detalhe.PreencherValorNaLinha(17, 17, "A");
                detalhe = detalhe.PreencherValorNaLinha(18, 18, "A"); /* TipoDesconto A - Valor monetário B - Percentual */
                detalhe = detalhe.PreencherValorNaLinha(19, 19, "A"); /* TipoJuros A - Valor monetário B - Percentual */
                detalhe = detalhe.PreencherValorNaLinha(20, 47, string.Empty.PadLeft(28, ' '));
                detalhe = detalhe.PreencherValorNaLinha(48, 56, nossoNumeroSequencial);
                detalhe = detalhe.PreencherValorNaLinha(57, 62, string.Empty.PadLeft(6, ' '));
                detalhe = detalhe.PreencherValorNaLinha(63, 70, DateTime.Now.ToString("yyyyMMdd").Replace("/", ""));
                detalhe = detalhe.PreencherValorNaLinha(71, 71, " ");
                detalhe = detalhe.PreencherValorNaLinha(72, 72, "N");
                detalhe = detalhe.PreencherValorNaLinha(73, 73, " ");
                detalhe = detalhe.PreencherValorNaLinha(74, 74, "B");
                detalhe = detalhe.PreencherValorNaLinha(75, 76, "00"); /*parcela carne*/
                detalhe = detalhe.PreencherValorNaLinha(77, 78, "00"); /*total parcela carne*/
                detalhe = detalhe.PreencherValorNaLinha(79, 82, string.Empty.PadLeft(4, ' '));
                detalhe = detalhe.PreencherValorNaLinha(83, 92, infoDetalhe.ValorDescontoDia.ToStringParaValoresDecimais().PadLeft(10, '0'));
                detalhe = detalhe.PreencherValorNaLinha(93, 96, infoDetalhe.PercentualMulta.ToStringParaValoresDecimais().PadLeft(4, '0'));
                detalhe = detalhe.PreencherValorNaLinha(97, 108, string.Empty.PadLeft(12, ' '));
                detalhe = detalhe.PreencherValorNaLinha(109, 110, infoDetalhe.Instrucao1.PadLeft(2, '0'));
                detalhe = detalhe.PreencherValorNaLinha(111, 120, infoDetalhe.NumeroDocumento.PadRight(10, ' '));
                detalhe = detalhe.PreencherValorNaLinha(121, 126, infoDetalhe.DataVencimento.ToString("ddMMyy").Replace("/", ""));/*A data de vencimento deve ser sete dias MAIOR que a data de emissão (campos 151-156). Formato: DDMMAA*/
                detalhe = detalhe.PreencherValorNaLinha(127, 139, infoDetalhe.ValorBoleto.ToStringParaValoresDecimais().PadLeft(13, '0'));
                detalhe = detalhe.PreencherValorNaLinha(149, 149, infoDetalhe.Especie.Sigla.BoletoBrToStringSafe());
                detalhe = detalhe.PreencherValorNaLinha(150, 150, infoDetalhe.Aceite);
                detalhe = detalhe.PreencherValorNaLinha(151, 156, infoDetalhe.DataEmissao.ToString("ddMMyy").Replace("/", ""));
                detalhe = detalhe.PreencherValorNaLinha(157, 158, "00"); /*00 - Não protestar automaticamente 06 - Protestar automaticamente*/
                detalhe = detalhe.PreencherValorNaLinha(159, 160, "00"); /*00 - Não protestar automaticamente 06 - Protestar automaticamente*/
                detalhe = detalhe.PreencherValorNaLinha(161, 173, infoDetalhe.ValorJuros.ToStringParaValoresDecimais().PadLeft(13, '0'));

                if (infoDetalhe.DataLimiteConcessaoDesconto > DateTime.MinValue)
                    detalhe = detalhe.PreencherValorNaLinha(174, 179, infoDetalhe.DataLimiteConcessaoDesconto.ToString("ddMMyy").Replace("/", ""));
                else
                    detalhe = detalhe.PreencherValorNaLinha(174, 179, "000000");

                detalhe = detalhe.PreencherValorNaLinha(180, 192, infoDetalhe.ValorDesconto.ToStringParaValoresDecimais().PadLeft(13, '0'));
                detalhe = detalhe.PreencherValorNaLinha(193, 194, "00");
                detalhe = detalhe.PreencherValorNaLinha(195, 196, "00");
                detalhe = detalhe.PreencherValorNaLinha(197, 205, string.Empty.PadLeft(9, '0'));
                detalhe = detalhe.PreencherValorNaLinha(206, 218, infoDetalhe.ValorAbatimento.ToStringParaValoresDecimais().PadLeft(13, '0'));
                detalhe = detalhe.PreencherValorNaLinha(219, 219, infoDetalhe.InscricaoPagador.Replace(".", "").Replace("/", "").Replace("-", "").Length == 11 ? "1" : "2");
                detalhe = detalhe.PreencherValorNaLinha(220, 220, "0");
                detalhe = detalhe.PreencherValorNaLinha(221, 234, infoDetalhe.InscricaoPagador.Replace(".", "").Replace("/", "").Replace("-", "").PadLeft(14, '0'));
                detalhe = detalhe.PreencherValorNaLinha(235, 274, infoDetalhe.NomePagador.PadLeft(40, ' '));

                string endereco = infoDetalhe.EnderecoPagador.Replace(".", "").Replace("/", "").Replace(",", "").Replace("-", "");
                if (endereco.Length > 40)
                    endereco = endereco.Substring(0, 40);

                detalhe = detalhe.PreencherValorNaLinha(275, 314, endereco.PadRight(40, ' '));
                detalhe = detalhe.PreencherValorNaLinha(315, 319, string.Empty.PadLeft(5, '0'));
                detalhe = detalhe.PreencherValorNaLinha(320, 325, string.Empty.PadLeft(6, '0'));
                detalhe = detalhe.PreencherValorNaLinha(326, 326, " ");
                detalhe = detalhe.PreencherValorNaLinha(327, 334, infoDetalhe.CepPagador.Replace(".", "").Replace("/", "").Replace("-", ""));
                detalhe = detalhe.PreencherValorNaLinha(335, 339, string.Empty.PadLeft(5, '0'));
                detalhe = detalhe.PreencherValorNaLinha(340, 353, string.Empty.PadLeft(14, ' '));
                detalhe = detalhe.PreencherValorNaLinha(354, 394, string.Empty.PadLeft(41, ' '));
                detalhe = detalhe.PreencherValorNaLinha(395, 400, sequenciaDetalhe.ToString().PadLeft(6, '0'));

                //switch (infoDetalhe.Instrucao1)
                //{
                //    case "31": 
                //        infoDetalhe.Variacao
                //        detalhe = detalhe.PreencherValorNaLinha(71, 71, infoDetalhe.TipoRegistro);
                //        break;
                //    default:
                //        detalhe = detalhe.PreencherValorNaLinha(71, 71, " ");
                //        break;
                //} 

                return detalhe;
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("<BoletoBr>{0}Falha na geração do DETALHE do arquivo de REMESSA.",
                    Environment.NewLine), e);
            }
        }

        public string EscreverRegistroMensagem(DetalheRemessaCnab400 infoDetalhe, int sequenciaDetalhe)
        {
            var registroMensagem = new string(' ', 400);
            try
            {
                string nossoNumeroSequencial =
               infoDetalhe.NossoNumeroFormatado.Replace(".", "").Replace("/", "").Replace("-", "").Substring(3, 8);

                registroMensagem = registroMensagem.PreencherValorNaLinha(1, 1, "2");
                registroMensagem = registroMensagem.PreencherValorNaLinha(2, 12, string.Empty.PadLeft(11, ' '));
                registroMensagem = registroMensagem.PreencherValorNaLinha(13, 21, nossoNumeroSequencial);
                registroMensagem = registroMensagem.PreencherValorNaLinha(22, 101, infoDetalhe.MensagemLinha1.PadRight(80, ' '));
                registroMensagem = registroMensagem.PreencherValorNaLinha(102, 181, infoDetalhe.MensagemLinha2.PadRight(80, ' '));
                registroMensagem = registroMensagem.PreencherValorNaLinha(182, 261, infoDetalhe.MensagemLinha3.PadRight(80, ' '));
                registroMensagem = registroMensagem.PreencherValorNaLinha(262, 341, infoDetalhe.MensagemLinha4.PadRight(80, ' '));
                registroMensagem = registroMensagem.PreencherValorNaLinha(342, 351, infoDetalhe.NumeroDocumento.PadRight(10, ' '));
                registroMensagem = registroMensagem.PreencherValorNaLinha(352, 394, string.Empty.PadRight(43, ' '));
                registroMensagem = registroMensagem.PreencherValorNaLinha(395, 400, sequenciaDetalhe.ToString().PadLeft(6, '0'));

                return registroMensagem;
            }
            catch (Exception e)
            {
                throw new Exception(
                    string.Format("<BoletoBr>{0}Falha na geração do REGISTRO DE MENSAGENS do arquivo de REMESSA.",
                        Environment.NewLine), e);
            }
        }



        public string EscreverTrailer(TrailerRemessaCnab400 infoTrailer, int sequenciaTrailer, string contaCedente)
        {
            var trailer = new string(' ', 400);
            try
            {
                trailer = trailer.PreencherValorNaLinha(1, 1, "9");
                trailer = trailer.PreencherValorNaLinha(2, 2, "1");
                trailer = trailer.PreencherValorNaLinha(3, 5, "748");
                trailer = trailer.PreencherValorNaLinha(6, 10, contaCedente);
                trailer = trailer.PreencherValorNaLinha(11, 394, string.Empty.PadLeft(384, ' '));
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
