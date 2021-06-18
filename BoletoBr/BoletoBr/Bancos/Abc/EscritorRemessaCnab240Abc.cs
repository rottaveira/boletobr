using BoletoBr.Arquivo.CNAB240.Remessa;
using BoletoBr.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoletoBr.Bancos.Abc
{
    public class EscritorRemessaCnab240Abc : IEscritorArquivoRemessaCnab240
    {
        private RemessaCnab240 _remessaEscrever;

        public EscritorRemessaCnab240Abc(RemessaCnab240 remessaEscrever)
        {
            _remessaEscrever = remessaEscrever;
        }

        public string EscreverHeader(HeaderRemessaCnab240 infoHeader)
        {
            var nomeEmpresa = "";
            if (infoHeader.NomeEmpresa.Length > 30)
                nomeEmpresa = infoHeader.NomeEmpresa.Substring(0, 30);
            else
                nomeEmpresa = infoHeader.NomeEmpresa.ToUpper();
            var header = new string(' ', 240);
            try
            {
                header = header.PreencherValorNaLinha(1, 3, "246");
                header = header.PreencherValorNaLinha(4, 7, "0000");
                header = header.PreencherValorNaLinha(8, 8, "0");
                header = header.PreencherValorNaLinha(9, 17, string.Empty.PadLeft(9, ' '));
                header = header.PreencherValorNaLinha(18, 18, infoHeader.NumeroInscricao.Replace(".", "").Replace("/", "").Replace("-", "").Length == 11 ? "1" : "2");
                header = header.PreencherValorNaLinha(19, 32, infoHeader.NumeroInscricao.Replace(".", "").Replace("/", "").Replace("-", "").PadLeft(14, '0'));
                header = header.PreencherValorNaLinha(33, 52, infoHeader.CodigoConvenio.PadRight(20, ' '));
                header = header.PreencherValorNaLinha(53, 72, infoHeader.CodigoConvenio.PadRight(20, ' '));
                header = header.PreencherValorNaLinha(73, 102, nomeEmpresa.PadRight(30, ' '));
                header = header.PreencherValorNaLinha(103, 132, "BANCO ABC BRASIL".PadRight(30, ' '));
                header = header.PreencherValorNaLinha(133, 142, string.Empty.PadLeft(10, ' '));
                header = header.PreencherValorNaLinha(143, 143, "1");
                header = header.PreencherValorNaLinha(144, 151, DateTime.Now.ToString("ddMMyyyy"));
                header = header.PreencherValorNaLinha(152, 157, DateTime.Now.ToString("HHmmss"));
                header = header.PreencherValorNaLinha(158, 163, infoHeader.SequencialNsa.ToString().PadLeft(6, '0'));
                header = header.PreencherValorNaLinha(164, 166, "040");
                header = header.PreencherValorNaLinha(167, 171, string.Empty.PadLeft(5, '0'));
                header = header.PreencherValorNaLinha(172, 191, string.Empty.PadRight(20, ' '));
                header = header.PreencherValorNaLinha(192, 211, string.Empty.PadRight(20, ' '));
                header = header.PreencherValorNaLinha(212, 240, string.Empty.PadRight(29, ' '));
                return header;
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("BoletoBr{0}Falha na geração do HEADER do arquivo de REMESSA.",
                    Environment.NewLine), e);
            }
        }
        public string EscreverHeaderDeLote(HeaderLoteRemessaCnab240 infoHeaderLote, string convenio)
        {
            #region NOTAS EXPLICATIVAS HEADER DE LOTE


            #endregion
             
            var nomeEmpresa = "";
            if (infoHeaderLote.NomeEmpresa.Length > 30)
                nomeEmpresa = infoHeaderLote.NomeEmpresa.Substring(0, 30);
            else
                nomeEmpresa = infoHeaderLote.NomeEmpresa.ToUpper();

            var headerLote = new string(' ', 240);
            try
            {
                headerLote = headerLote.PreencherValorNaLinha(1, 3, "246"); // Código do Banco na Compensação
                headerLote = headerLote.PreencherValorNaLinha(4, 7, infoHeaderLote.LoteServico.ToString().PadLeft(4, '0')); // Lote de Serviço
                headerLote = headerLote.PreencherValorNaLinha(8, 8, "1"); // Tipo de Registro
                headerLote = headerLote.PreencherValorNaLinha(9, 9, "R"); // Tipo de Operação
                headerLote = headerLote.PreencherValorNaLinha(10, 11, "01"); // Tipo de Serviço
                headerLote = headerLote.PreencherValorNaLinha(12, 13, string.Empty.PadRight(2, ' ')); // Uso Exclusivo FREBRABAN/CNAB
                headerLote = headerLote.PreencherValorNaLinha(14, 16, "030"); // Nº da versão do Layout do Lote
                headerLote = headerLote.PreencherValorNaLinha(17, 17, " "); // Uso Exclusivo FREBRABAN/CNAB
                headerLote = headerLote.PreencherValorNaLinha(18, 18, infoHeaderLote.NumeroInscricao.Replace(".", "").Replace("/", "").Replace("-", "").Length == 11 ? "1" : "2");
                headerLote = headerLote.PreencherValorNaLinha(19, 33, infoHeaderLote.NumeroInscricao.Replace(".", "").Replace("/", "").Replace("-", "").PadLeft(15, '0'));
                headerLote = headerLote.PreencherValorNaLinha(34, 53, convenio.PadRight(20, ' '));
                headerLote = headerLote.PreencherValorNaLinha(54, 73, convenio.PadRight(20, ' '));
                headerLote = headerLote.PreencherValorNaLinha(74, 103, nomeEmpresa.PadRight(30, ' ')); // Nome Empresa
                headerLote = headerLote.PreencherValorNaLinha(104, 143, infoHeaderLote.Mensagem1.BoletoBrToStringSafe().PadRight(40, ' ')); // Mensagem 1
                headerLote = headerLote.PreencherValorNaLinha(144, 183, infoHeaderLote.Mensagem2.BoletoBrToStringSafe().PadRight(40, ' ')); // Mensagem 2
                headerLote = headerLote.PreencherValorNaLinha(184, 191, infoHeaderLote.NumeroRemessaRetorno.PadLeft(8, '0'));// numero remessa
                headerLote = headerLote.PreencherValorNaLinha(192, 199, DateTime.Now.ToString("ddMMyyyy"));
                headerLote = headerLote.PreencherValorNaLinha(200, 207, string.Empty.PadLeft(8, '0'));
                headerLote = headerLote.PreencherValorNaLinha(208, 240, string.Empty.PadRight(33, ' '));

                return headerLote;
            }
            catch (Exception e)
            {
                throw new Exception(
                    String.Format("<BoletoBr>{0}Falha na geração do HEADER DE LOTE do arquivo de REMESSA.",
                        Environment.NewLine), e);
            }
        }

        public string EscreverDetalheSegmentoP(DetalheSegmentoPRemessaCnab240 infoSegmentoP)
        {
            var segmentoP = new string(' ', 240);
            try
            {
                segmentoP = segmentoP.PreencherValorNaLinha(1, 3, "246"); // Código do Banco na Compensação
                segmentoP = segmentoP.PreencherValorNaLinha(4, 7, infoSegmentoP.LoteServico.ToString().PadLeft(4, '0')); // Lote De Serviço
                segmentoP = segmentoP.PreencherValorNaLinha(8, 8, "3"); // Tipo de Registro
                segmentoP = segmentoP.PreencherValorNaLinha(9, 13, infoSegmentoP.NumeroRegistro.ToString().PadLeft(5, '0'));
                segmentoP = segmentoP.PreencherValorNaLinha(14, 14, "P"); // Cód. Segmento do Registro Detalhe
                segmentoP = segmentoP.PreencherValorNaLinha(15, 15, " ");
                segmentoP = segmentoP.PreencherValorNaLinha(16, 17, "01"); // Código de Movimento Remessa
                segmentoP = segmentoP.PreencherValorNaLinha(18, 37, infoSegmentoP.Convenio.PadRight(20, ' '));
                segmentoP = segmentoP.PreencherValorNaLinha(38, 38, "5");  
                segmentoP = segmentoP.PreencherValorNaLinha(39, 41, "000");  
                segmentoP = segmentoP.PreencherValorNaLinha(42, 43, "00");
                segmentoP = segmentoP.PreencherValorNaLinha(44, 46, "110"); 

                segmentoP = segmentoP.PreencherValorNaLinha(47, 57, infoSegmentoP.NossoNumero.Replace("-", "").Replace(".", "").PadLeft(11, '0'));

                /* Código da Carteira
                 * '1' = Cobrança Simples Com Registro
                 * '3' = Cobrança Caucionada
                 */
                segmentoP = segmentoP.PreencherValorNaLinha(58, 58, "1");
                segmentoP = segmentoP.PreencherValorNaLinha(59, 59, "1");
                segmentoP = segmentoP.PreencherValorNaLinha(60, 60, "1"); // Tipo de Documento
                // '1' = Banco Emite
                // '2' = Emissão pelo beneficiario
                segmentoP = segmentoP.PreencherValorNaLinha(61, 61, infoSegmentoP.BancoEmiteBoleto ? "1" : "2"); // Identificação da Emissão do Bloqueto
                // '1' = Sacado Via Correios
                // '2' = Envio pelo beneficiario
                segmentoP = segmentoP.PreencherValorNaLinha(62, 62, infoSegmentoP.BancoEmiteBoleto ? "1" : "2"); // Identificação da Entrega do Bloqueto
                segmentoP = segmentoP.PreencherValorNaLinha(63, 77, infoSegmentoP.NumeroDocumento.PadLeft(15, '0'));
                segmentoP = segmentoP.PreencherValorNaLinha(78, 85, infoSegmentoP.DataVencimento.ToString("ddMMyyyy"));

                segmentoP = segmentoP.PreencherValorNaLinha(86, 100, infoSegmentoP.ValorBoleto.ToStringParaValoresDecimais().PadLeft(15, '0'));
                segmentoP = segmentoP.PreencherValorNaLinha(101, 105, string.Empty.PadLeft(5, '0'));
                segmentoP = segmentoP.PreencherValorNaLinha(106, 106, " "); // Dígito Verificador da Agência
                segmentoP = segmentoP.PreencherValorNaLinha(107, 108, infoSegmentoP.Especie.Sigla.Equals("DM") ? "02" : infoSegmentoP.Especie.Codigo.ToString().PadLeft(2, '0')); // Espécia do Título

                if (String.IsNullOrEmpty(infoSegmentoP.Aceite))
                    segmentoP = segmentoP.PreencherValorNaLinha(109, 109, "N");
                else
                    segmentoP = segmentoP.PreencherValorNaLinha(109, 109, infoSegmentoP.Aceite.Equals("A") ? "A" : "N");

                if (infoSegmentoP.DataEmissao == DateTime.MinValue)
                    segmentoP = segmentoP.PreencherValorNaLinha(110, 117, DateTime.Now.ToString("ddMMyyyy"));
                else
                    segmentoP = segmentoP.PreencherValorNaLinha(110, 117, infoSegmentoP.DataEmissao.ToString("ddMMyyyy"));
               
                /* Modalidade de cobrança de juros de mora
                 * 0 - Isento
                 * 1 - Valor por dia
                 * 2 - Taxa Mensal
                 */
                segmentoP = segmentoP.PreencherValorNaLinha(118, 118, infoSegmentoP.CodigoJurosMora.ToString().Length > 1 ? infoSegmentoP.CodigoJurosMora.ToString().Substring(infoSegmentoP.NumeroDocumento.Length - 1, 1) : infoSegmentoP.CodigoJurosMora.ToString().PadLeft(1, '0')); // Código do Juros de Mora

                if (infoSegmentoP.DataJurosMora == DateTime.MinValue)
                    segmentoP = segmentoP.PreencherValorNaLinha(119, 126, infoSegmentoP.DataVencimento.AddDays(1).ToString("ddMMyyyy"));
                else
                {
                    /*DATA DO JUROS/MORA – CAMPOS 119 A 126 = A data deve ser no mínimo D+1 do vencimento.*/
                    infoSegmentoP.DataJurosMora = infoSegmentoP.DataJurosMora <= infoSegmentoP.DataVencimento ? infoSegmentoP.DataVencimento.AddDays(1) : infoSegmentoP.DataJurosMora;
                    segmentoP = segmentoP.PreencherValorNaLinha(119, 126, infoSegmentoP.DataJurosMora.ToString("ddMMyyyy"));
                }

                /*JUROS/ MORA – CAMPOS 127 A 141 (descrição C020 do layout) = Valor ou porcentagem a ser cobrado de juros de mora. Quando valor em Reais, utilizar 2 decimais, quando taxa utilizar 4 decimais.*/
                segmentoP = segmentoP.PreencherValorNaLinha(127, 141, infoSegmentoP.ValorJurosMora.GetValueOrDefault().ToStringParaValoresDecimais(infoSegmentoP.CasasDecimaisJuros).PadLeft(15, '0'));
               
                /* Código do Desconto
                 * 0 - Sem desconto
                 * 1 - Valor fixo até a data informada
                 * 2 - Percentual até a data informada
                 * Obs.: Para os códigos '1' e '2' será obrigatório a informação da data.
                 */

                if (infoSegmentoP.ValorDesconto1 > 0)
                    segmentoP = segmentoP.PreencherValorNaLinha(142, 142, "1"); // Código do Desconto 1
                else
                    segmentoP = segmentoP.PreencherValorNaLinha(142, 142, "0"); // Código do Desconto 0

                if (infoSegmentoP.DataDesconto1 == DateTime.MinValue)
                    segmentoP = segmentoP.PreencherValorNaLinha(143, 150, string.Empty.PadLeft(8, '0'));
                else
                    segmentoP = segmentoP.PreencherValorNaLinha(143, 150, infoSegmentoP.DataDesconto1.ToString("ddMMyyyy"));

                segmentoP = segmentoP.PreencherValorNaLinha(151, 165, infoSegmentoP.ValorDesconto1.GetValueOrDefault().ToStringParaValoresDecimais().PadLeft(15, '0'));
                segmentoP = segmentoP.PreencherValorNaLinha(166, 180, infoSegmentoP.ValorIof.GetValueOrDefault().ToStringParaValoresDecimais().PadLeft(15, '0'));
                segmentoP = segmentoP.PreencherValorNaLinha(181, 195, infoSegmentoP.ValorAbatimento.GetValueOrDefault().ToStringParaValoresDecimais().PadLeft(15, '0'));

                var usoEmpresa = string.IsNullOrEmpty(infoSegmentoP.UsoEmpresa) ? "" : infoSegmentoP.UsoEmpresa.Length > 25 ? infoSegmentoP.UsoEmpresa.Substring(0, 25) : infoSegmentoP.UsoEmpresa;

                segmentoP = segmentoP.PreencherValorNaLinha(196, 220, usoEmpresa.PadRight(25, ' '));

                segmentoP = segmentoP.PreencherValorNaLinha(221, 221, infoSegmentoP.CodigoProtesto.ToString().PadLeft(1, '0')); // Código para Protesto
                segmentoP = segmentoP.PreencherValorNaLinha(222, 223, infoSegmentoP.PrazoProtesto.ToString().PadLeft(2, '0')); // Número de Dias para Protesto
                segmentoP = segmentoP.PreencherValorNaLinha(224, 224, "2"/*infoSegmentoP.CodigoBaixaDevolucao.ToString().PadLeft(1, '0')*/); // Código para Baixa/Devolução
                segmentoP = segmentoP.PreencherValorNaLinha(225, 227, "000"); // Número de Dias para Baixa/Devolução

                // Fixo 09 - REAL
                segmentoP = segmentoP.PreencherValorNaLinha(228, 229, "09"); // Código da Moeda
                segmentoP = segmentoP.PreencherValorNaLinha(230, 239, string.Empty.PadLeft(10, '0')); // Uso Exclusivo 
                segmentoP = segmentoP.PreencherValorNaLinha(240, 240, string.Empty.PadLeft(1, ' '));

                return segmentoP;
            }
            catch (Exception e)
            {
                throw new Exception(String.Format("<BoletoBr>{0}Falha na geração do DETALHE - Segmento P do arquivo de REMESSA.",
                    Environment.NewLine), e);
            }
        }

        public string EscreverDetalheSegmentoQ(DetalheSegmentoQRemessaCnab240 infoSegmentoQ)
        {
            string enderecoSacado = string.Empty;
            string bairroSacado = string.Empty;
            string cidadeSacado = string.Empty;

            string nomeSacado = string.Empty;

            if (String.IsNullOrEmpty(infoSegmentoQ.EnderecoSacado))
                enderecoSacado.PadRight(40, ' ');
            else
                if (infoSegmentoQ.EnderecoSacado.Length > 40)
                enderecoSacado = infoSegmentoQ.EnderecoSacado.Substring(0, 40).ToUpper();
            else
                enderecoSacado = infoSegmentoQ.EnderecoSacado.PadRight(40, ' ').ToUpper();

            if (String.IsNullOrEmpty(infoSegmentoQ.BairroSacado))
                bairroSacado.PadRight(15, ' ');
            else
                if (infoSegmentoQ.BairroSacado.Length > 15)
                bairroSacado = infoSegmentoQ.BairroSacado.Substring(0, 15).ToUpper();
            else
                bairroSacado = infoSegmentoQ.BairroSacado.PadRight(15, ' ').ToUpper();

            if (String.IsNullOrEmpty(infoSegmentoQ.CidadeSacado))
                cidadeSacado.PadRight(15, ' ');
            else
                if (infoSegmentoQ.CidadeSacado.Length > 15)
                cidadeSacado = infoSegmentoQ.CidadeSacado.Substring(0, 15).ToUpper();
            else
                cidadeSacado = infoSegmentoQ.CidadeSacado.PadRight(15, ' ').ToUpper();

            if (String.IsNullOrEmpty(infoSegmentoQ.NomeSacado))
                nomeSacado.PadRight(40, ' ');
            else
                if (infoSegmentoQ.NomeSacado.Length > 40)
                nomeSacado = infoSegmentoQ.NomeSacado.Substring(0, 40).ToUpper();
            else
                nomeSacado = infoSegmentoQ.NomeSacado.PadRight(40, ' ').ToUpper();


            var segmentoQ = new string(' ', 240);

            try
            {
                segmentoQ = segmentoQ.PreencherValorNaLinha(1, 3, "246");
                segmentoQ = segmentoQ.PreencherValorNaLinha(4, 7, infoSegmentoQ.LoteServico.ToString().PadLeft(4, '0'));
                segmentoQ = segmentoQ.PreencherValorNaLinha(8, 8, "3");
                segmentoQ = segmentoQ.PreencherValorNaLinha(9, 13, infoSegmentoQ.NumeroRegistro.ToString().PadLeft(5, '0'));
                segmentoQ = segmentoQ.PreencherValorNaLinha(14, 14, "Q");
                segmentoQ = segmentoQ.PreencherValorNaLinha(15, 15, " ");
                segmentoQ = segmentoQ.PreencherValorNaLinha(16, 17, "01");
                segmentoQ = segmentoQ.PreencherValorNaLinha(18, 18, infoSegmentoQ.NumeroInscricaoSacado.Replace(".", "").Replace("-", "").Replace("-", "").Length == 11 ? "1" : "2");
                segmentoQ = segmentoQ.PreencherValorNaLinha(19, 33, infoSegmentoQ.NumeroInscricaoSacado.Replace(".", "").Replace("/", "").Replace("-", "").PadLeft(15, '0'));
                segmentoQ = segmentoQ.PreencherValorNaLinha(34, 73, nomeSacado.PadRight(40, ' '));
                segmentoQ = segmentoQ.PreencherValorNaLinha(74, 113, enderecoSacado.PadRight(40, ' '));
                segmentoQ = segmentoQ.PreencherValorNaLinha(114, 128, bairroSacado.PadRight(15, ' '));

                var Cep = infoSegmentoQ.CepSacado;

                if (Cep.Contains(".") && Cep.Contains("-"))
                    Cep = Cep.Replace(".", "").Replace("-", "");
                if (Cep.Contains("."))
                    Cep = Cep.Replace(".", "");
                if (Cep.Contains("-"))
                    Cep = Cep.Replace("-", "");

                segmentoQ = segmentoQ.PreencherValorNaLinha(129, 136, Cep.PadLeft(8, '0'));
                segmentoQ = segmentoQ.PreencherValorNaLinha(137, 151, cidadeSacado.PadRight(15, ' '));
                segmentoQ = segmentoQ.PreencherValorNaLinha(152, 153, infoSegmentoQ.UfSacado.PadRight(2, ' '));

                if (String.IsNullOrEmpty(infoSegmentoQ.NumeroInscricaoAvalista))
                {
                    segmentoQ = segmentoQ.PreencherValorNaLinha(154, 154, "0");
                    segmentoQ = segmentoQ.PreencherValorNaLinha(155, 169, string.Empty.PadLeft(15, '0'));
                }
                else
                {
                    segmentoQ = segmentoQ.PreencherValorNaLinha(154, 154,
                        infoSegmentoQ.NumeroInscricaoAvalista.Replace(".", "").Replace("/", "").Replace("-", "").Length ==
                        11
                            ? "1"
                            : "2");
                    segmentoQ = segmentoQ.PreencherValorNaLinha(155, 169,
                        infoSegmentoQ.NumeroInscricaoSacado.Replace(".", "")
                            .Replace("/", "")
                            .Replace("-", "")
                            .PadLeft(15, '0'));
                }

                if (String.IsNullOrEmpty(infoSegmentoQ.NomeAvalista))
                    segmentoQ = segmentoQ.PreencherValorNaLinha(170, 209, string.Empty.PadRight(40, ' '));
                else
                    segmentoQ = segmentoQ.PreencherValorNaLinha(170, 209, infoSegmentoQ.NomeAvalista.PadRight(40, ' '));

                segmentoQ = segmentoQ.PreencherValorNaLinha(210, 212, string.Empty.PadLeft(3, '0'));
                segmentoQ = segmentoQ.PreencherValorNaLinha(213, 232, string.Empty.PadLeft(20, ' '));
                segmentoQ = segmentoQ.PreencherValorNaLinha(233, 240, string.Empty.PadLeft(8, ' '));

                return segmentoQ;
            }
            catch (Exception e)
            {
                throw new Exception(
                    String.Format("<BoletoBr>{0}Falha na geração do DETALHE - Segmento Q do arquivo de REMESSA.",
                        Environment.NewLine), e);
            }
        }

        public string EscreverTrailerDeLote(TrailerLoteRemessaCnab240 infoTrailerLote)
        {
            if (infoTrailerLote.QtdRegistrosLote == 0)
                throw new Exception("Sequencial do registro no lote não foi informado na geração do TRAILER DE LOTE.");

            var trailerLote = new string(' ', 240);
            try
            {
                trailerLote = trailerLote.PreencherValorNaLinha(1, 3, "246");
                trailerLote = trailerLote.PreencherValorNaLinha(4, 7, infoTrailerLote.LoteServico.ToString().PadLeft(4, '0'));
                trailerLote = trailerLote.PreencherValorNaLinha(8, 8, "5");
                trailerLote = trailerLote.PreencherValorNaLinha(9, 17, string.Empty.PadLeft(9, ' '));
                trailerLote = trailerLote.PreencherValorNaLinha(18, 23, infoTrailerLote.QtdRegistrosLote.ToString().PadLeft(6, '0'));
                trailerLote = trailerLote.PreencherValorNaLinha(24, 29, infoTrailerLote.QtdTitulosCobrancaSimples.ToString().PadLeft(6, '0'));
                trailerLote = trailerLote.PreencherValorNaLinha(30, 46, infoTrailerLote.ValorTitulosCobrancaSimples.ToStringParaValoresDecimais().PadLeft(17, '0'));
                trailerLote = trailerLote.PreencherValorNaLinha(47, 52, string.Empty.PadLeft(6, '0'));
                trailerLote = trailerLote.PreencherValorNaLinha(53, 69, string.Empty.PadLeft(17, '0'));
                trailerLote = trailerLote.PreencherValorNaLinha(70, 75, infoTrailerLote.QtdTitulosCobrancaCaucionada.ToString().PadLeft(6, '0'));
                trailerLote = trailerLote.PreencherValorNaLinha(76, 92, infoTrailerLote.ValorTitulosCobrancaCaucionada.ToStringParaValoresDecimais().PadLeft(17, '0'));
                trailerLote = trailerLote.PreencherValorNaLinha(93, 98, infoTrailerLote.QtdTitulosCobrancaDescontada.ToString().PadLeft(6, '0'));
                trailerLote = trailerLote.PreencherValorNaLinha(99, 115, infoTrailerLote.ValorTitulosCobrancaDescontada.ToStringParaValoresDecimais().PadLeft(17, '0'));
                trailerLote = trailerLote.PreencherValorNaLinha(116, 123, string.Empty.PadLeft(8, ' '));
                trailerLote = trailerLote.PreencherValorNaLinha(124, 240, string.Empty.PadLeft(117, ' '));

                return trailerLote;
            }
            catch (Exception e)
            {
                throw new Exception(String.Format("<BoletoBr>{0}Falha na geração do TRAILER DE LOTE do arquivo de REMESSA.",
                   Environment.NewLine), e);
            }
        }

        public string EscreverTrailer(TrailerRemessaCnab240 infoTrailer)
        {
            if (infoTrailer.QtdRegistrosArquivo == 0)
                throw new Exception("Não foi informada a quantidade de registros do arquivo.");

            var trailer = new string(' ', 240);
            try
            {
                trailer = trailer.PreencherValorNaLinha(1, 3, "246");
                trailer = trailer.PreencherValorNaLinha(4, 7, "9999");
                trailer = trailer.PreencherValorNaLinha(8, 8, "9");
                trailer = trailer.PreencherValorNaLinha(9, 17, string.Empty.PadLeft(9, ' '));
                trailer = trailer.PreencherValorNaLinha(18, 23, infoTrailer.QtdLotesArquivo.ToString().PadLeft(6, '0'));
                trailer = trailer.PreencherValorNaLinha(24, 29, infoTrailer.QtdRegistrosArquivo.ToString().PadLeft(6, '0'));
                trailer = trailer.PreencherValorNaLinha(30, 35, string.Empty.PadLeft(6, '0'));
                trailer = trailer.PreencherValorNaLinha(36, 240, string.Empty.PadLeft(205, ' '));

                return trailer;
            }
            catch (Exception e)
            {
                throw new Exception(String.Format("<BoletoBr>{0}Falha na geração do TRAILER do arquivo de REMESSA.",
                    Environment.NewLine), e);
            }
        }

        public List<string> EscreverTexto(RemessaCnab240 remessaEscrever)
        {
            ValidarRemessa(remessaEscrever);

            var listaRet = new List<string>();

            /* Header */
            listaRet.Add(EscreverHeader(remessaEscrever.Header));
            var convenio = remessaEscrever.Header.CodigoConvenio;


            /* Detalhes */
            /* Caso não venha a sequencia do lote informada no header criar a mesma */
            var sequenciaLote = 1;
            foreach (var loteEscrever in remessaEscrever.Lotes)
            {
                if (loteEscrever.HeaderLote.LoteServico.BoletoBrToStringSafe().BoletoBrToInt() == 0)
                    loteEscrever.HeaderLote.LoteServico = sequenciaLote;

                if (loteEscrever.TrailerLote.LoteServico.BoletoBrToStringSafe().BoletoBrToInt() == 0)
                    loteEscrever.TrailerLote.LoteServico = sequenciaLote;

                listaRet.AddRange(EscreverLote(loteEscrever, convenio));

                sequenciaLote++;
            }

            /* Trailer */
            listaRet.Add(EscreverTrailer(remessaEscrever.Trailer));

            return listaRet;
        }

        private List<string> EscreverLote(LoteRemessaCnab240 loteEscrever, string convenio)
        {
            var listaRet = new List<string>();

            listaRet.Add(EscreverHeaderDeLote(loteEscrever.HeaderLote, convenio));

            foreach (var detalhe in loteEscrever.RegistrosDetalheSegmentos)
            {
                if (detalhe.SegmentoP.LoteServico.BoletoBrToStringSafe().BoletoBrToInt() == 0)
                    detalhe.SegmentoP.LoteServico = loteEscrever.HeaderLote.LoteServico;

                if (detalhe.SegmentoQ.LoteServico.BoletoBrToStringSafe().BoletoBrToInt() == 0)
                    detalhe.SegmentoQ.LoteServico = loteEscrever.HeaderLote.LoteServico;

                listaRet.Add(EscreverDetalheSegmentoP(detalhe.SegmentoP));
                listaRet.Add(EscreverDetalheSegmentoQ(detalhe.SegmentoQ));
            }

            listaRet.Add(EscreverTrailerDeLote(loteEscrever.TrailerLote));

            return listaRet;
        }

        public void ValidarRemessa(RemessaCnab240 remessaValidar)
        {
            if (remessaValidar == null)
                throw new Exception("Não há informações para geração do arquivo de remessa.");

            if (remessaValidar.Header == null)
                throw new Exception("Não há informações para geração do HEADER no arquivo de remessa.");

            if (remessaValidar.Lotes == null)
                throw new Exception("Não há informações para geração dos LOTES no arquivo de remessa.");

            if (remessaValidar.Trailer == null)
                throw new Exception("Não há informações para geração do TRAILER no arquivo de remessa.");

            #region #HEADER

            if (String.IsNullOrEmpty(remessaValidar.Header.NumeroInscricao))
                throw new Exception("CPF/CNPJ do Cedente não foi informado.");

            if (String.IsNullOrEmpty(remessaValidar.Header.AgenciaMantenedora))
                throw new Exception("Agência do Cedente não foi informada.");

            if (String.IsNullOrEmpty(remessaValidar.Header.DigitoAgenciaMantenedora))
                throw new Exception("Dígito da agência do Cedente não foi informada.");

            if (String.IsNullOrEmpty(remessaValidar.Header.CodigoCedente))
                throw new Exception("Código do Cedente não foi informado.");

            if (String.IsNullOrEmpty(remessaValidar.Header.NomeEmpresa))
                throw new Exception("Nome do Cedente não foi informado.");

            #endregion #HEADER

            #region #TRAILER

            if (remessaValidar.Trailer.QtdLotesArquivo <= 0)
                throw new Exception("O número de lotes do arquivo não foi informado no registro TRAILER.");

            if (remessaValidar.Trailer.QtdRegistrosArquivo <= 0)
                throw new Exception("O número de registros do arquivo não foi informado no registro TRAILER");

            #endregion #TRAILER
        }
    }
}
