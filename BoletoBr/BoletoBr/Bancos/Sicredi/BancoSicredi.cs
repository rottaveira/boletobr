using System;
using System.Collections.Generic;
using System.Drawing; 
using System.Linq; 
using BoletoBr.Arquivo.CNAB240.Remessa;
using BoletoBr.Arquivo.CNAB400.Remessa;
using BoletoBr.Arquivo.Generico.Retorno;
using BoletoBr.Bancos.BancoSicredi;
using BoletoBr.Dominio;
using BoletoBr.Dominio.Instrucao;
using BoletoBr.Enums;
using BoletoBr.Interfaces;

namespace BoletoBr.Bancos.Sicredi
{
    public class BancoSicredi : IBanco
    { 
        private int _dacNossoNumero = 0;

        public BancoSicredi()
        {
            CodigoBanco = "748";
            DigitoBanco = "X";
            NomeBanco = "Sicredi"; 
            MoedaBanco = "9";
            LocalDePagamento = "PAGAVEL PREFERENCIALMENTE EM CANAIS ELETRONICOS DA SUA INSTITUICAO FINANCEIRA";
        }

        public string CodigoBanco { get; set; }
        public string DigitoBanco { get; set; }
        public string NomeBanco { get; set; }
        public Image LogotipoBancoParaExibicao { get; set; }

        public string LocalDePagamento { get; private set; }
        public string MoedaBanco { get; private set; }

        public void ValidaBoletoComNormasBanco(Boleto boleto)
        {
            try
            {
                //Formata o tamanho do número da agência
                if (boleto.CedenteBoleto.ContaBancariaCedente.Agencia.Length < 4)
                    boleto.CedenteBoleto.ContaBancariaCedente.Agencia = boleto.CedenteBoleto.ContaBancariaCedente.Agencia.PadLeft(4, '0');

                //Formata o tamanho do número da conta corrente
                if (boleto.CedenteBoleto.ContaBancariaCedente.Conta.Length < 5)
                    boleto.CedenteBoleto.ContaBancariaCedente.Conta = boleto.CedenteBoleto.ContaBancariaCedente.Conta.PadLeft(5, '0'); 
                 
                //Verifica se data do processamento é valida
                if (boleto.DataProcessamento == DateTime.MinValue)  
                    boleto.DataProcessamento = DateTime.Now;

                //Verifica se data do documento é valida
                if (boleto.DataDocumento == DateTime.MinValue) 
                    boleto.DataDocumento = DateTime.Now;
                  
                var codigoCedente = boleto.CedenteBoleto.CodigoCedente.PadLeft(11, '0');  

                if (string.IsNullOrEmpty(codigoCedente))
                    throw new Exception("Código do cedente deve ser informado, formato AAAAPPCCCCC, onde: AAAA = Número da agência, PP = Posto do beneficiário, CCCCC = Código do beneficiário");

                var conta = boleto.CedenteBoleto.ContaBancariaCedente.Conta;
                if (boleto.CedenteBoleto.ContaBancariaCedente != null &&
                    (!codigoCedente.StartsWith(boleto.CedenteBoleto.ContaBancariaCedente.Agencia) ||
                     !(codigoCedente.EndsWith(conta) || codigoCedente.EndsWith(conta.Substring(0, conta.Length - 1)))))
                    boleto.CedenteBoleto.CodigoCedenteFormatado = string.Format("{0}.{1}.{2}", boleto.CedenteBoleto.ContaBancariaCedente.Agencia, boleto.CedenteBoleto.ContaBancariaCedente.DigitoAgencia, $@"{boleto.CedenteBoleto.CodigoCedente}".PadLeft(5, '0'));
  
                if (boleto.CodigoBarraBoleto.Length != 44)
                    throw new Exception("Código de barras é inválido"); 
            }
            catch (Exception ex)
            {
                throw new Exception("Falha ao validar boleto (s).", ex);
            }
        }
         
        public void FormataMoeda(Boleto boleto)
        {
            try
            {
                boleto.Moeda = MoedaBanco;

                if (String.IsNullOrEmpty(boleto.Moeda))
                    throw new Exception("Espécie/Moeda para o boleto não foi informada.");

                if ((boleto.Moeda == "9") || (boleto.Moeda == "REAL") || (boleto.Moeda == "R$"))
                    boleto.Moeda = "REAL";
                else
                    boleto.Moeda = "1";
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("<BoletoBr>" +
                                                  "{0}Mensagem: Falha ao formatar moeda para o Banco: " +
                                                  CodigoBanco + " - " + NomeBanco, Environment.NewLine), ex);
            }
        }

        public void FormatarBoleto(Boleto boleto)
        {
            //Atribui o local de pagamento
            boleto.LocalPagamento = LocalDePagamento;

            boleto.ValidaDadosEssenciaisDoBoleto();
             
            FormataNossoNumero(boleto);
            FormataCodigoBarra(boleto);
            FormataLinhaDigitavel(boleto);
            FormataMoeda(boleto);

            ValidaBoletoComNormasBanco(boleto); 
        }

        public void FormataCodigoBarra(Boleto boleto)
        {
            try
            {
                string valorBoleto = boleto.ValorBoleto.ToString("f").Replace(",", "").Replace(".", "");
                valorBoleto = valorBoleto.PadLeft(10, '0');

                //var codigoCobranca = 1; //Código de cobrança com registro
                ////string cmp_livre =
                ////    codigoCobranca +
                ////    boleto.CarteiraCobranca.Codigo +
                ////     boleto.NossoNumeroFormatado.PadLeft(9, '0').Replace("-", "").Replace("/", "") +
                ////    boleto.CedenteBoleto.CodigoCedente.PadLeft(11, '0') + "10";

                string campoLivre = new string(' ', 24);
                campoLivre = campoLivre.PreencherValorNaLinha(1,1, "1");
                campoLivre = campoLivre.PreencherValorNaLinha(2,2, "1");
                campoLivre = campoLivre.PreencherValorNaLinha(3,11, boleto.NossoNumeroFormatado.PadLeft(9, '0').Replace("-", "").Replace("/", ""));
                campoLivre = campoLivre.PreencherValorNaLinha(12,15, boleto.CedenteBoleto.ContaBancariaCedente.Agencia);
                campoLivre = campoLivre.PreencherValorNaLinha(16,17, boleto.CedenteBoleto.ContaBancariaCedente.DigitoAgencia);
                campoLivre = campoLivre.PreencherValorNaLinha(18,22, boleto.CedenteBoleto.CodigoCedente.PadLeft(5, '0'));
                campoLivre = campoLivre.PreencherValorNaLinha(23,24, "10");

                string dv_cmpLivre = Common.digSicredi(campoLivre).ToString();

                var codigoTemp = GerarCodigoDeBarras(boleto, valorBoleto, campoLivre, dv_cmpLivre);
 
                boleto.Moeda = MoedaBanco; 

                int _dacBoleto = Common.digSicredi(codigoTemp);

                if (_dacBoleto == 0 || _dacBoleto > 9)
                    _dacBoleto = 1;

                boleto.CodigoBarraBoleto = GerarCodigoDeBarras(boleto, valorBoleto, campoLivre, dv_cmpLivre, _dacBoleto);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("<BoletoBr>" +"{0}Mensagem: Falha ao formatar código de barras.",Environment.NewLine), ex);
            }
        }
         
        private string GerarCodigoDeBarras(Boleto boleto, string valorBoleto, string cmp_livre, string dv_cmpLivre, int? dv_geral = null)
        {
            return string.Format("{0}{1}{2}{3}{4}{5}{6}",
                CodigoBanco.PadLeft(3, '0'),
                boleto.Moeda,
                dv_geral.HasValue ? dv_geral.Value.ToString() : string.Empty,
                Common.FatorVencimento(boleto.DataVencimento),
                valorBoleto,
                cmp_livre,
                dv_cmpLivre);
        }
        
        public void FormataLinhaDigitavel(Boleto boleto)
        {
            try
            {
                //041M2.1AAAd1  CCCCC.CCNNNd2  NNNNN.041XXd3  V FFFF9999999999

                string campo1 = CodigoBanco + MoedaBanco + boleto.CodigoBarraBoleto.Substring(19, 5);
                int d1 = Common.Mod10Sicredi(campo1);
                campo1 = string.Format("{0}.{1}", campo1.Substring(0, 5), campo1.Substring(5)) + d1.ToString();

                string campo2 = boleto.CodigoBarraBoleto.Substring(24, 10);
                int d2 = Common.Mod10Sicredi(campo2);
                campo2 = string.Format("{0}.{1}", campo2.Substring(0, 5), campo2.Substring(5)) + d2.ToString();

                string campo3 = boleto.CodigoBarraBoleto.Substring(34, 10);
                int d3 = Common.Mod10Sicredi(campo3);
                campo3 = string.Format("{0}.{1}", campo3.Substring(0, 5), campo3.Substring(5)) + d3.ToString();

                string campo4 = boleto.CodigoBarraBoleto.Substring(4, 1);

                string campo5 = boleto.CodigoBarraBoleto.Substring(5, 14);

                boleto.LinhaDigitavelBoleto = campo1 + "  " + campo2 + "  " + campo3 + "  " + campo4 + "  " + campo5;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("<BoletoBr>" +
                                                  "{0}Mensagem: Falha ao formatar linha digitável." +
                                                  "{0}Carteira: " + boleto.CarteiraCobranca.Codigo +
                                                  "{0}Documento: " + boleto.NumeroDocumento, Environment.NewLine), ex);
            }
        }

        public void FormataNossoNumero(Boleto boleto)
        {
            if (String.IsNullOrEmpty(boleto.IdentificadorInternoBoleto) ||
                String.IsNullOrEmpty(boleto.IdentificadorInternoBoleto.TrimStart('0')))
                throw new Exception("Sequencial Nosso Número não foi informado.");
            
            try
            {
                var identificadorInterno = boleto.IdentificadorInternoBoleto.PadLeft(5, '0');
                var prefix = DateTime.Now.Year.ToString().Substring(2) ; 

                var nossoNumero = prefix + "2" + identificadorInterno;
                 
                string seq =$@"{boleto.CedenteBoleto.ContaBancariaCedente.Agencia}{boleto.CedenteBoleto.ContaBancariaCedente.DigitoAgencia}{boleto.CedenteBoleto.CodigoCedente}{nossoNumero}"; // = aaaappcccccyybnnnnn

                // Usando Método e Geração do DAC do Nosso Número
                _dacNossoNumero = digNossoNroSicredi(seq);  //GerarDacNossoNumero(seq);
                 
                /*  
                 *  AA/BXXXXX-D, onde:
                 *  AA = Ano (pode ser diferente do ano corrente)
                 *  B = Byte de geração (0 a 9). O Byte 1 só poderá ser informado pela Cooperativa
                 *  XXXXX = Número livre de 00000 a 99999
                 *  D = Dígito verificador pelo módulo 11                 */
                boleto.SetNossoNumeroFormatado(string.Format("{0}/{1}{2}-{3}", prefix, "2", identificadorInterno, _dacNossoNumero));
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("<BoletoBr>" +
                                                  "{0}Mensagem: Falha ao formatar nosso número." +
                                                  "{0}Carteira: " + boleto.CarteiraCobranca.Codigo +
                                                  "{0}Numeração Sequencial: " + boleto.NossoNumeroFormatado + " - " +
                                                  "DAC: " + _dacNossoNumero, Environment.NewLine), ex);
            }
        }

        public int digNossoNroSicredi(string seq)
        {
            /* Variáveis
           * -------------
           * d - Dígito
           * s - Soma
           * p - Peso
           * b - Base
           * r - Resto
           */

            int d, s = 0, p = 2, b = 9;
            //Atribui os pesos de {2..9}
            for (int i = seq.Length - 1; i >= 0; i--)
            {
                s = s + (Convert.ToInt32(seq.Substring(i, 1)) * p);
                if (p < b)
                    p = p + 1;
                else
                    p = 2;
            }
            d = 11 - (s % 11);//Calcula o Módulo 11;
            if (d > 9)
                d = 0;
            return d;
        }

        public void FormataNumeroDocumento(Boleto boleto)
        {
            //string numeroDoDocumento;
            //string digitoNumeroDoDocumento;
            //string numeroDoDocumentoFormatado;

            //if (String.IsNullOrEmpty(boleto.NumeroDocumento))
            //    throw new Exception("O número do documento não foi informado.");

            //if (boleto.NumeroDocumento.Length > 8)
            //    numeroDoDocumento = boleto.NumeroDocumento.Substring(0, 8);
            //else
            //    numeroDoDocumento = boleto.NumeroDocumento.PadLeft(8, '0');

            //digitoNumeroDoDocumento = Common.Mod10(numeroDoDocumento).ToString();
            //numeroDoDocumentoFormatado = String.Format("{0}-{1}", numeroDoDocumento, digitoNumeroDoDocumento);

            //boleto.NumeroDocumento = numeroDoDocumentoFormatado;
        }

        public ICodigoOcorrencia ObtemCodigoOcorrenciaByInt(int numeroOcorrencia)
        {
            switch (numeroOcorrencia)
            {
                case 02:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 02,
                        Descricao = "ENTRADA CONFIRMADA"
                    };
                case 03:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 03,
                        Descricao = "ENTRADA REJEITADA"
                    };
                case 06:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 06,
                        Descricao = "LIQUIDAÇÃO NORMAL"
                    };  
                case 07:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 07,
                        Descricao = "INTENÇÃO DE PAGAMENTO"
                    }; 
                case 09:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 09,
                        Descricao = "BAIXADO AUTOMATICAMENTE VIA ARQUIVO"
                    };
                case 10:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 10,
                        Descricao = "BAIXADO CONFORME INSTRUÇÕES DA COOPERATIVA"
                    }; 
                case 12:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 12,
                        Descricao = "ABATIMENTO CONCEDIDO"
                    };
                case 13:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 13,
                        Descricao = "ABATIMENTO CANCELADO"
                    };
                case 14:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 14,
                        Descricao = "VENCIMENTO ALTERADO"
                    };
                case 15:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 15,
                        Descricao = "LIQUIDAÇÃO EM CARTÓRIO"
                    }; 
                case 17:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 17,
                        Descricao = "LIQUIDAÇÃO APÓS BAIXA"
                    }; 
                case 19:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 19,
                        Descricao = "CONFIRMAÇÃO DE RECEBIMENTO DE INSTRUÇÃO DE PROTESTO"
                    };
                case 20:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 20,
                        Descricao = "CONFIRMAÇÃO DE RECEBIMENTO DE INSTRUÇÃO DE SUSTAÇÃO DE PROTESTO"
                    }; 
                case 23:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 23,
                        Descricao = "ENTRADA DE TÍTULO EM CARTÓRIO"
                    };
                case 24:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 24,
                        Descricao = "ENTRADA REJEITADA POR CEP IRREGULAR"
                    }; 
                case 27:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 27,
                        Descricao = "BAIXA REJEITADA"
                    };
                case 28:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 28,
                        Descricao = "TARIFAS"
                    };
                case 29:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 29,
                        Descricao = "REJEIÇÃO DO PAGADOR"
                    };
                case 30:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 30,
                        Descricao = "ALTERAÇÃO REJEITADA"
                    };
                case 32:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 32,
                        Descricao = "INSTRUÇÃO REJEITADA"
                    };
                case 33:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 33,
                        Descricao = "CONFIRMAÇÃO DE PEDIDO DE ALTERAÇÃO DE OUTROS DADOS"
                    };
                case 34:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 34,
                        Descricao = "RETIRADO DE CARTÓRIO E MANUTENÇÃO EM CARTEIRA"
                    };
                case 35:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 35,
                        Descricao = "ACEITE DO PAGADOR"
                    }; 
                case 78:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 78,
                        Descricao = "CONFIRMAÇÃO DE RECEBIMENTO DE PEDIDO DE NEGATIVAÇÃO"
                    };
                case 79:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 79,
                        Descricao = "CONFIRMAÇÃO DE RECEBIMENTO DE PEDIDO DE EXCLUSÃO DE NEGATIVAÇÃO"
                    };
                case 80:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 80,
                        Descricao = "CONFIRMAÇÃO DE ENTRADA DE NEGATIVAÇÃO"
                    };
                case 81:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 81,
                        Descricao = "ENTRADA DE NEGATIVAÇÃO REJEITADA"
                    };
                case 82:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 82,
                        Descricao = "CONFIRMAÇÃO DE EXCLUSÃO DE NEGATIVAÇÃO"
                    };
                case 83:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 83,
                        Descricao = "EXCLUSÃO DE NEGATIVAÇÃO REJEITADA"
                    };
                case 84:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 84,
                        Descricao = "EXCLUSÃO DE NEGATIVAÇÃO POR OUTROS MOTIVOS"
                    };
                case 85:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 85,
                        Descricao = "OCORRÊNCIA INFORMACIONAL POR OUTROS MOTIVOS"
                    }; 
            }
            throw new Exception(
                String.Format(
                    "Não foi possível obter Código de Comando/Movimento/Ocorrência. Banco: {0} Código: {1}",
                    CodigoBanco, numeroOcorrencia.ToString()));
        }

        public ICodigoOcorrencia ObtemCodigoOcorrencia(EnumCodigoOcorrenciaRetorno ocorrenciaRetorno)
        {
            throw new NotImplementedException();
        }

        public IEspecieDocumento ObtemEspecieDocumento(EnumEspecieDocumento especie)
        {
            #region Código Espécie

            /** Este campo só permite usar os seguintes códigos:
             *   A - Duplicata Mercantil por Indicação
             *   B - Duplicata Rural
             *   C - Nota Promissória 
             *   D - Nota Promissória Rural
             *   E - Nota de Seguros 
             *   G - Recibo
             *   H - Letra de Câmbio
             *   I - Nota de Débito
             *   J - Duplicata de Serviço por Indicação
             *   K - Outros
             *   O - Boleto Proposta 
             **/

            #endregion

            switch (especie)
            {
                case EnumEspecieDocumento.DuplicataMercantilIndicacao:
                {
                    return new EspecieDocumento((int) especie)
                    {
                        Codigo = 01,
                        Descricao = "Duplicata Mercantil por Indicação",
                        Sigla = "A"
                    };
                }
                case EnumEspecieDocumento.DuplicataRural:
                {
                    return new EspecieDocumento((int) especie)
                    {
                        Codigo = 01,
                        Descricao = "Duplicata Rural",
                        Sigla = "B"
                    };
                }
                case EnumEspecieDocumento.NotaPromissoria:
                {
                    return new EspecieDocumento((int) especie)
                    {
                        Codigo = 01,
                        Descricao = "Nota Promissoria",
                        Sigla = "C"
                    };
                }
                case EnumEspecieDocumento.NotaPromissoriaRural:
                {
                    return new EspecieDocumento((int) especie)
                    {
                        Codigo = 01,
                        Descricao = "Nota Promissoria Rural",
                        Sigla = "D"
                    };
                }
                case EnumEspecieDocumento.NotaSeguro:
                {
                    return new EspecieDocumento((int) especie)
                    {
                        Codigo = 01,
                        Descricao = "Nota de Seguro",
                        Sigla = "E"
                    };
                }
                case EnumEspecieDocumento.Recibo:
                {
                    return new EspecieDocumento((int) especie)
                    {
                        Codigo = 01,
                        Descricao = "Recibo",
                        Sigla = "G"
                    };
                }
                case EnumEspecieDocumento.LetraCambio:
                {
                    return new EspecieDocumento((int) especie)
                    {
                        Codigo = 01,
                        Descricao = "Letra de Câmbio",
                        Sigla = "H"
                    };
                }
                case EnumEspecieDocumento.NotaDebito:
                {
                    return new EspecieDocumento((int) especie)
                    {
                        Codigo = 01,
                        Descricao = "Nota de Débito",
                        Sigla = "I"
                    };
                }
                case EnumEspecieDocumento.DuplicataServicoIndicacao:
                {
                    return new EspecieDocumento((int) especie)
                    {
                        Codigo = 01,
                        Descricao = "Duplicata de Serviço por Indicação",
                        Sigla = "J"
                    };
                }
                case EnumEspecieDocumento.Outros:
                {
                    return new EspecieDocumento((int) especie)
                    {
                        Codigo = 01,
                        Descricao = "Outros",
                        Sigla = "K"
                    };
                }
                case EnumEspecieDocumento.BoletoProposto:
                {
                    return new EspecieDocumento((int) especie)
                    {
                        Codigo = 01,
                        Descricao = "Boleto Proposto",
                        Sigla = "O"
                    };
                } 
            }
            throw new Exception(
                String.Format("Não foi possível obter espécie. Banco: {0} Código Espécie: {1}",
                    CodigoBanco, especie.ToString()));
        }

        public IInstrucao ObtemInstrucaoPadronizada(EnumTipoInstrucao tipoInstrucao, double valorInstrucao,
            DateTime dataInstrucao, int diasInstrucao)
        {
            switch (tipoInstrucao)
            {
                case EnumTipoInstrucao.Protestar:
                {
                    return new InstrucaoPadronizada()
                    {
                        Codigo = 9,
                        QtdDias = diasInstrucao,
                        Valor = valorInstrucao,
                        TextoInstrucao = "Protestar após " + valorInstrucao + " dias úteis."
                    };
                }
                case EnumTipoInstrucao.NaoProtestar:
                {
                    return new InstrucaoPadronizada()
                    {
                        Codigo = 10,
                        QtdDias = diasInstrucao,
                        Valor = valorInstrucao,
                        TextoInstrucao = "Não protestar."
                    };
                }
                case EnumTipoInstrucao.DevolverApos90Dias:
                {
                    return new InstrucaoPadronizada()
                    {
                        Codigo = 18,
                        QtdDias = diasInstrucao,
                        Valor = valorInstrucao,
                        TextoInstrucao = "Devolver após 90 dias do vencimento."
                    };
                }
                case EnumTipoInstrucao.ProtestarAposNDiasCorridos:
                {
                    return new InstrucaoPadronizada()
                    {
                        Codigo = 34,
                        QtdDias = diasInstrucao,
                        Valor = valorInstrucao,
                        TextoInstrucao = "Protestar após " + diasInstrucao + " dias corridos do vencimento."
                    };
                }
                case EnumTipoInstrucao.ProtestarAposNDiasUteis:
                {
                    return new InstrucaoPadronizada()
                    {
                        Codigo = 35,
                        QtdDias = diasInstrucao,
                        Valor = valorInstrucao,
                        TextoInstrucao = "Protestar após " + diasInstrucao + " dias úteis do vencimento."
                    };
                }
                case EnumTipoInstrucao.NaoReceberAposOVencimento:
                {
                    return new InstrucaoPadronizada()
                    {
                        Codigo = 39,
                        QtdDias = diasInstrucao,
                        Valor = valorInstrucao,
                        TextoInstrucao = "Não receber após o vencimento."
                    };
                }
                case EnumTipoInstrucao.ImportanciaPorDiaDeAtrasoAPartirDeDDMMAA:
                {
                    return new InstrucaoPadronizada()
                    {
                        Codigo = 44,
                        QtdDias = diasInstrucao,
                        Valor = valorInstrucao,
                        TextoInstrucao = "Importância por dia de atraso a partir de " + dataInstrucao.ToString("ddmmyy")
                    };
                }
                case EnumTipoInstrucao.NoVencimentoPagavelEmQualquerAgencia:
                {
                    return new InstrucaoPadronizada()
                    {
                        Codigo = 90,
                        QtdDias = diasInstrucao,
                        Valor = valorInstrucao,
                        TextoInstrucao = "No vencimento pagável em qualquer agência bancária."
                    };
                }
                case EnumTipoInstrucao.NaoReceberAposNDiasCorridos:
                {
                    return new InstrucaoPadronizada()
                    {
                        Codigo = 91,
                        QtdDias = diasInstrucao,
                        Valor = valorInstrucao,
                        TextoInstrucao = "Não receber após " + diasInstrucao + " dias do vencimento."
                    };
                }
                case EnumTipoInstrucao.DevolverAposNDias:
                {
                    return new InstrucaoPadronizada()
                    {
                        Codigo = 92,
                        QtdDias = diasInstrucao,
                        Valor = valorInstrucao,
                        TextoInstrucao = "Devolver após " + diasInstrucao + " dias do vencimento."
                    };
                }
                case EnumTipoInstrucao.MultaVencimento:
                {
                    return new InstrucaoPadronizada()
                    {
                        Codigo = 997,
                        QtdDias = diasInstrucao,
                        Valor = valorInstrucao,
                        TextoInstrucao = "Cobrar multa após o vencimento."
                    };
                }
                case EnumTipoInstrucao.JurosdeMora:
                {
                    return new InstrucaoPadronizada()
                    {
                        Codigo = 998,
                        QtdDias = diasInstrucao,
                        Valor = valorInstrucao,
                        TextoInstrucao = "Cobrar juros após o vencimento."
                    };
                }
            }
            throw new Exception(
                String.Format(
                    "Não foi possível obter instrução padronizada. Banco: {0} Código Instrução: {1} Qtd Dias/Valor: {2}",
                    CodigoBanco, tipoInstrucao.ToString(), valorInstrucao));
        }

        public ICodigoOcorrencia ObtemCodigoOcorrencia(EnumCodigoOcorrenciaRemessa ocorrencia, double valorOcorrencia,
            DateTime dataOcorrencia)
        {
            switch (ocorrencia)
            {
                case EnumCodigoOcorrenciaRemessa.Registro:
                {
                    return new CodigoOcorrencia((int) ocorrencia)
                    {
                        Codigo = 01,
                        Descricao = "Remessa"
                    };
                }
                case EnumCodigoOcorrenciaRemessa.Baixa:
                {
                    return new CodigoOcorrencia((int) ocorrencia)
                    {
                        Codigo = 02,
                        Descricao = "Pedido de baixa"
                    };
                }
                case EnumCodigoOcorrenciaRemessa.ConcessaoDeAbatimento:
                {
                    return new CodigoOcorrencia((int) ocorrencia)
                    {
                        Codigo = 04,
                        Descricao = "Concessão de abatimento"
                    };
                }
                case EnumCodigoOcorrenciaRemessa.CancelamentoDeAbatimento:
                {
                    return new CodigoOcorrencia((int) ocorrencia)
                    {
                        Codigo = 05,
                        Descricao = "Cancelamento de abatimento"
                    };
                }
                case EnumCodigoOcorrenciaRemessa.AlteracaoDeVencimento:
                {
                    return new CodigoOcorrencia((int) ocorrencia)
                    {
                        Codigo = 06,
                        Descricao = "Alteração do vencimento"
                    };
                }
                case EnumCodigoOcorrenciaRemessa.AlteracaoDoControleDoParticipante:
                {
                    return new CodigoOcorrencia((int) ocorrencia)
                    {
                        Codigo = 07,
                        Descricao = "Alteração do uso da empresa"
                    };
                }
                case EnumCodigoOcorrenciaRemessa.AlteracaoSeuNumero:
                {
                    return new CodigoOcorrencia((int) ocorrencia)
                    {
                        Codigo = 08,
                        Descricao = "Alteração de seu número"
                    };
                }
                case EnumCodigoOcorrenciaRemessa.Protesto:
                {
                    return new CodigoOcorrencia((int) ocorrencia)
                    {
                        Codigo = 09,
                        Descricao = "Protestar"
                    };
                }
                case EnumCodigoOcorrenciaRemessa.NaoProtestar:
                {
                    return new CodigoOcorrencia((int) ocorrencia)
                    {
                        Codigo = 10,
                        Descricao = "Não protestar"
                    };
                }
                case EnumCodigoOcorrenciaRemessa.ProtestoParaFinsFalimentares:
                {
                    return new CodigoOcorrencia((int) ocorrencia)
                    {
                        Codigo = 11,
                        Descricao = "Protesto para fins falimentares"
                    };
                }
                case EnumCodigoOcorrenciaRemessa.SustarProtesto:
                {
                    return new CodigoOcorrencia((int) ocorrencia)
                    {
                        Codigo = 18,
                        Descricao = "Sustar o protesto"
                    };
                }
                case EnumCodigoOcorrenciaRemessa.ExclusaoDeSacadorAvalista:
                {
                    return new CodigoOcorrencia((int) ocorrencia)
                    {
                        Codigo = 30,
                        Descricao = "Exclusão de sacador avalista"
                    };
                }
                case EnumCodigoOcorrenciaRemessa.AlteracaoDeOutrosDados:
                {
                    return new CodigoOcorrencia((int) ocorrencia)
                    {
                        Codigo = 31,
                        Descricao = "Alteração de outros dados"
                    };
                }
                case EnumCodigoOcorrenciaRemessa.BaixaPorTerSidoPagoDiretamenteAoCedente:
                {
                    return new CodigoOcorrencia((int) ocorrencia)
                    {
                        Codigo = 34,
                        Descricao = "Baixa por ter sido pago diretamente ao cedente"
                    };
                }
                case EnumCodigoOcorrenciaRemessa.CancelamentoDeInstrucao:
                {
                    return new CodigoOcorrencia((int) ocorrencia)
                    {
                        Codigo = 35,
                        Descricao = "Cancelamento de instrução"
                    };
                }
                case EnumCodigoOcorrenciaRemessa.AlteracaoDoVencimentoESustarProtesto:
                {
                    return new CodigoOcorrencia((int) ocorrencia)
                    {
                        Codigo = 37,
                        Descricao = "Alteração do vencimento e sustar protesto"
                    };
                }
                case EnumCodigoOcorrenciaRemessa.CedenteNaoConcordaComAlegacaoDoSacado:
                {
                    return new CodigoOcorrencia((int) ocorrencia)
                    {
                        Codigo = 38,
                        Descricao = "Cedente não concorda com alegação do sacado"
                    };
                }
                case EnumCodigoOcorrenciaRemessa.CedenteSolicitaDispensaDeJuros:
                {
                    return new CodigoOcorrencia((int) ocorrencia)
                    {
                        Codigo = 47,
                        Descricao = "Cedente solicita dispensa de juros"
                    };
                }
            }
            throw new Exception(
                String.Format(
                    "Não foi possível obter Código de Comando/Movimento/Ocorrência. Banco: {0} Código: {1}",
                    CodigoBanco, ocorrencia.ToString()));
        }

        public RetornoGenerico LerArquivoRetorno(List<string> linhasArquivo)
        {
            if (linhasArquivo == null || linhasArquivo.Any() == false)
                throw new ApplicationException("Arquivo informado é inválido/Não existem títulos no retorno.");

            /* Identifica o layout: 240 ou 400 */
            if (linhasArquivo.First().Length == 240)
            { 
            }
            if (linhasArquivo.First().Length == 400)
            {
                var leitor = new LeitorRetornoCnab400Sicredi(linhasArquivo);
                var retornoProcessado = leitor.ProcessarRetorno();

                var objRetornar = new RetornoGenerico(retornoProcessado);
                return objRetornar;
            }

            throw new Exception("Arquivo de RETORNO com " + linhasArquivo.First().Length + " posições, não é suportado.");
        }

        public RemessaCnab240 GerarArquivoRemessaCnab240(RemessaCnab240 remessaCnab240, List<Boleto> boletos)
        {
            throw new NotImplementedException();
        }

        public RemessaCnab400 GerarArquivoRemessaCnab400(RemessaCnab400 remessaCnab400, List<Boleto> boletos)
        {
            throw new NotImplementedException();
        }

        public RemessaCnab240 GerarArquivoRemessaCnab240(List<Boleto> boletos)
        {
            throw new NotImplementedException();
        }

        public RemessaCnab400 GerarArquivoRemessaCnab400(List<Boleto> boletos)
        {
            throw new NotImplementedException();
        }
        public int CodigoJurosMora(CodigoJurosMora codigoJurosMora)
        {
            return 0;
        }

        public int CodigoProteso(bool protestar = true)
        {
            return 0;
        }

        public RetornoGenericoPagamento LerArquivoRetornoPagamento(List<string> linhasArquivo)
        {
            throw new NotImplementedException();
        }

        public ICodigoOcorrencia ObtemCodigoOcorrenciaPagamento(string numeroOcorrencia)
        {
            throw new NotImplementedException();
        }
    }
}
