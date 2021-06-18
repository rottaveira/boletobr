using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using BoletoBr.Arquivo.CNAB240.Remessa;
using BoletoBr.Arquivo.CNAB400.Remessa;
using BoletoBr.Arquivo.Generico.Retorno;
using BoletoBr.Dominio;
using BoletoBr.Dominio.Instrucao;
using BoletoBr.Enums;
using BoletoBr.Interfaces;

namespace BoletoBr.Bancos.Bradesco
{
    public class BancoBradesco : IBanco
    {
        public string CodigoBanco { get; set; }
        public string DigitoBanco { get; set; }
        public string NomeBanco { get; set; }
        public Image LogotipoBancoParaExibicao { get; set; }

        public BancoBradesco()
        {
            CodigoBanco = "237";
            DigitoBanco = "2";
            NomeBanco = "Bradesco";
            LocalDePagamento = "Pagável preferencialmente nas Agências Bradesco.";
            MoedaBanco = "9";
        }

        public string CalcularDigitoNossoNumero(Boleto boleto)
        {
            if(boleto.CarteiraCobranca.Codigo.Equals("16"))
                return Common.Mod11Base7Bradesco(boleto.CarteiraCobranca.Codigo + boleto.IdentificadorInternoBoleto, 7, boleto.CarteiraCobranca.Codigo);
            return Common.Mod11Base7Bradesco(boleto.CarteiraCobranca.Codigo + boleto.IdentificadorInternoBoleto, 7);
        }

        private int _digitoAutoConferenciaBoleto;
        private string _digitoAutoConferenciaNossoNumero;

        public string LocalDePagamento { get; private set; }
        public string MoedaBanco { get; private set; }

        public void ValidaBoletoComNormasBanco(Boleto boleto)
        {
            if (boleto.CarteiraCobranca.Codigo != "02" && boleto.CarteiraCobranca.Codigo != "03" &&
                boleto.CarteiraCobranca.Codigo != "04" && boleto.CarteiraCobranca.Codigo != "06" && 
                boleto.CarteiraCobranca.Codigo != "09" && boleto.CarteiraCobranca.Codigo != "19" &&
                boleto.CarteiraCobranca.Codigo != "16")
                throw new ValidacaoBoletoException(
                    "Carteira não implementada. Carteiras implementadas 02, 03, 04, 06, 09, 16 e 19.");

            //O valor � obrigat�rio para a carteira 03
            if (boleto.CarteiraCobranca.Codigo == "03")
            {
                if (boleto.ValorBoleto == 0)
                    throw new ValidacaoBoletoException("Para a carteira 03, o valor do boleto n�o pode ser igual a zero");
            }

            //O valor � obrigat�rio para a carteira 09
            if (boleto.CarteiraCobranca.Codigo == "09")
            {
                if (boleto.ValorBoleto == 0)
                    throw new ValidacaoBoletoException("Para a carteira 09, o valor do boleto não pode ser igual a zero");
            }

            //Verifica se o nosso n�mero � v�lido
            //if (boleto.NossoNumeroFormatado.Length > 16)
            //    throw new ValidacaoBoletoException("A quantidade de dígitos do nosso número deve ser 16 números."
            //        + Environment.NewLine + "02->Carteira/" + Environment.NewLine + "11->Nosso Número-" + Environment.NewLine + "01->DV");

            //Verifica se a Agencia esta correta
            if (boleto.CedenteBoleto.ContaBancariaCedente.Agencia.Length > 4)
                throw new ValidacaoBoletoException("A quantidade de dígitos da Agência " +
                                                   boleto.CedenteBoleto.ContaBancariaCedente.Agencia +
                                                   ", deve ser de 4 números.");
            if (boleto.CedenteBoleto.ContaBancariaCedente.Agencia.Length < 4)
                boleto.CedenteBoleto.ContaBancariaCedente.Agencia =
                    boleto.CedenteBoleto.ContaBancariaCedente.Agencia.PadLeft(4, '0');

            //Verifica se a Conta esta correta
            if (boleto.CedenteBoleto.ContaBancariaCedente.Conta.Length > 7)
                throw new ValidacaoBoletoException("A quantidade de dígitos da Conta " +
                                                   boleto.CedenteBoleto.ContaBancariaCedente.Conta +
                                                   ", deve ser de 07 números.");
            if (boleto.CedenteBoleto.ContaBancariaCedente.Conta.Length < 7)
                boleto.CedenteBoleto.ContaBancariaCedente.Conta =
                    boleto.CedenteBoleto.ContaBancariaCedente.Conta.PadLeft(7, '0');

            //Verifica se data do processamento � valida
            //if (boleto.DataProcessamento.ToString("dd/MM/yyyy") == "01/01/0001")
            if (boleto.DataProcessamento == DateTime.MinValue) // diegomodolo (diego.ribeiro@nectarnet.com.br)
                boleto.DataProcessamento = DateTime.Now;


            //Verifica se data do documento � valida
            //if (boleto.DataDocumento.ToString("dd/MM/yyyy") == "01/01/0001")
            if (boleto.DataDocumento == DateTime.MinValue) // diegomodolo (diego.ribeiro@nectarnet.com.br)
                boleto.DataDocumento = DateTime.Now;
        }

        public void FormataMoeda(Boleto boleto)
        {
            boleto.Moeda = MoedaBanco;

            if (String.IsNullOrEmpty(boleto.Moeda))
                throw new Exception("Espécie/Moeda para o boleto não foi informada.");

            if ((boleto.Moeda == "9") || (boleto.Moeda == "REAL") || (boleto.Moeda == "R$"))
                boleto.Moeda = "R$";
            else
                boleto.Moeda = "0";
        }

        public void FormatarBoleto(Boleto boleto)
        {
            //N. Número com 11(onze) caracteres
            //Identificador interno usado como nosso número para calculo do DAC tem que estar com 11 caracteres
            //No codigo de barras no campo livre tambem considera-se 11 caracteres
            if (boleto.IdentificadorInternoBoleto.Length > 11)
                throw new ValidacaoBoletoException(
                    "Tamanho máximo para o campo Identificador interno do boleto é de 11.");

            if (boleto.IdentificadorInternoBoleto.Length < 11)
                boleto.IdentificadorInternoBoleto = boleto.IdentificadorInternoBoleto.PadLeft(11, '0');

            //Atribui o local de pagamento
            boleto.LocalPagamento = LocalDePagamento;

            boleto.ValidaDadosEssenciaisDoBoleto();

            // Calcula o DAC do Nosso Número
            _digitoAutoConferenciaNossoNumero = CalcularDigitoNossoNumero(boleto);
            boleto.DigitoNossoNumero = _digitoAutoConferenciaNossoNumero;

            FormataNumeroDocumento(boleto);
            FormataNossoNumero(boleto);
            FormataCodigoBarra(boleto);
            FormataLinhaDigitavel(boleto);

            FormataMoeda(boleto);

            ValidaBoletoComNormasBanco(boleto);

            boleto.CedenteBoleto.CodigoCedenteFormatado = String.Format("{0}-{1}/{2}-{3}",
                boleto.CedenteBoleto.ContaBancariaCedente.Agencia.PadLeft(4, '0'),
                boleto.CedenteBoleto.ContaBancariaCedente.DigitoAgencia,
                boleto.CedenteBoleto.ContaBancariaCedente.Conta.PadLeft(7, '0'),
                boleto.CedenteBoleto.ContaBancariaCedente.DigitoConta);
        }

        /// <summary>
        /// 
        ///   *******
        /// 
        ///	O c�digo de barra para cobran�a cont�m 44 posi��es dispostas da seguinte forma:
        ///    01 a 03 - 3 - Identifica��o  do  Banco
        ///    04 a 04 - 1 - C�digo da Moeda
        ///    05 a 05 � 1 - D�gito verificador do C�digo de Barras
        ///    06 a 09 - 4 - Fator de vencimento
        ///    10 a 19 - 10 - Valor
        ///    20 a 44 � 25 - Campo Livre
        /// 
        ///   *******
        /// 
        /// </summary>

        public void FormataNossoNumero(Boleto boleto)
        {
            boleto.SetNossoNumeroFormatado(boleto.IdentificadorInternoBoleto);

            boleto.SetNossoNumeroFormatado(
                string.Format("{0}/{1}-{2}",
                    boleto.CarteiraCobranca.Codigo,
                    boleto.NossoNumeroFormatado.PadLeft(11, '0'),
                    boleto.DigitoNossoNumero));
        }

        public void FormataCodigoBarra(Boleto boleto)
        {
            var valorBoleto = boleto.ValorBoleto.ToString("f").Replace(",", "").Replace(".", "");
            valorBoleto = valorBoleto.PadLeft(10, '0');

            if (boleto.CarteiraCobranca.Codigo == "02" || boleto.CarteiraCobranca.Codigo == "03" ||
                boleto.CarteiraCobranca.Codigo == "04" || boleto.CarteiraCobranca.Codigo == "09" ||
                boleto.CarteiraCobranca.Codigo == "19" || boleto.CarteiraCobranca.Codigo == "16")
            {
                boleto.CodigoBarraBoleto = string.Format("{0}{1}{2}{3}{4}", CodigoBanco, boleto.Moeda,
                    Common.FatorVencimento(boleto.DataVencimento), valorBoleto, FormataCampoLivre(boleto));
            }
            else if (boleto.CarteiraCobranca.Codigo == "06")
            {
                if (boleto.ValorBoleto == 0)
                {
                    boleto.CodigoBarraBoleto = string.Format("{0}{1}0000{2}{3}", CodigoBanco, boleto.Moeda,
                        valorBoleto, FormataCampoLivre(boleto));
                }
                else
                {
                    boleto.CodigoBarraBoleto = string.Format("{0}{1}{2}{3}{4}", CodigoBanco, boleto.Moeda,
                        Common.FatorVencimento(boleto.DataVencimento), valorBoleto, FormataCampoLivre(boleto));
                }
            }
            else
            {
                throw new Exception("Carteira ainda não implementada.");
            }

            _digitoAutoConferenciaBoleto = Common.Mod11(boleto.CodigoBarraBoleto, 9);

            boleto.CodigoBarraBoleto = Common.Left(boleto.CodigoBarraBoleto, 4) + _digitoAutoConferenciaBoleto +
                                       Common.Right(boleto.CodigoBarraBoleto, 39);
        }

        ///<summary>
        /// Campo Livre
        ///    20 a 23 -  4 - Ag�ncia Cedente (Sem o digito verificador,completar com zeros a esquerda quandonecess�rio)
        ///    24 a 25 -  2 - Carteira
        ///    26 a 36 - 11 - N�mero do Nosso N�mero(Sem o digito verificador)
        ///    37 a 43 -  7 - Conta do Cedente (Sem o digito verificador,completar com zeros a esquerda quando necess�rio)
        ///    44 a 44	- 1 - Zero            
        ///</summary>
        public string FormataCampoLivre(Boleto boleto)
        {
            string formataCampoLivre = string.Format("{0}{1}{2}{3}{4}",
                boleto.CedenteBoleto.ContaBancariaCedente.Agencia.PadLeft(4, '0'), boleto.CarteiraCobranca.Codigo,
                boleto.IdentificadorInternoBoleto.PadLeft(11, '0'),
                boleto.CedenteBoleto.ContaBancariaCedente.Conta.PadLeft(7, '0'), "0");

            return formataCampoLivre;
        }

        public void FormataLinhaDigitavel(Boleto boleto)
        {

            //BBBMC.CCCCD1 CCCCC.CCCCCD2 CCCCC.CCCCCD3 D4 FFFFVVVVVVVVVV

            #region Campo 1

            var BBB = boleto.CodigoBarraBoleto.Substring(0, 3);
            var M = boleto.CodigoBarraBoleto.Substring(3, 1);
            var CCCCC = boleto.CodigoBarraBoleto.Substring(19, 5);
            var D1 = Common.Mod10(BBB + M + CCCCC).ToString(CultureInfo.InvariantCulture);

            var Grupo1 = string.Format("{0}{1}{2}.{3}{4} ", BBB, M, CCCCC.Substring(0, 1), CCCCC.Substring(1, 4), D1);


            #endregion Campo 1

            #region Campo 2

            var CCCCCCCCCC2 = boleto.CodigoBarraBoleto.Substring(24, 10);
            var D2 = Common.Mod10(CCCCCCCCCC2).ToString(CultureInfo.InvariantCulture);

            var Grupo2 = string.Format("{0}.{1}{2} ", CCCCCCCCCC2.Substring(0, 5), CCCCCCCCCC2.Substring(5, 5), D2);

            #endregion Campo 2

            #region Campo 3

            var CCCCCCCCCC3 = boleto.CodigoBarraBoleto.Substring(34, 10);
            var D3 = Common.Mod10(CCCCCCCCCC3).ToString(CultureInfo.InvariantCulture);

            var Grupo3 = string.Format("{0}.{1}{2} ", CCCCCCCCCC3.Substring(0, 5), CCCCCCCCCC3.Substring(5, 5), D3);

            #endregion Campo 3

            #region Campo 4

            var D4 = _digitoAutoConferenciaBoleto.ToString(CultureInfo.InvariantCulture);

            var Grupo4 = string.Format("{0} ", D4);

            #endregion Campo 4

            #region Campo 5

            //string FFFF = boleto.CodigoBarra.Codigo.Substring(5, 4);//FatorVencimento(boleto).ToString() ;
            var FFFF = Common.FatorVencimento(boleto.DataVencimento).ToString(CultureInfo.InvariantCulture);

            //if (boleto.CarteiraCobranca.Codigo == "06" && boleto.DataVencimento == DateTime.MinValue)
            //    FFFF = "0000";

            var VVVVVVVVVV = boleto.ValorBoleto.ToString("f").Replace(",", "").Replace(".", "");
            VVVVVVVVVV = VVVVVVVVVV.PadLeft(10, '0');

            //if (Convert.ToInt64(VVVVVVVVVV) == 0)
            //    VVVVVVVVVV = "000";

            var Grupo5 = string.Format("{0}{1}", FFFF, VVVVVVVVVV);

            #endregion Campo 5

            boleto.LinhaDigitavelBoleto = Grupo1 + Grupo2 + Grupo3 + Grupo4 + Grupo5;

        }

        public void FormataNumeroDocumento(Boleto boleto)
        {
            if (String.IsNullOrEmpty(boleto.NumeroDocumento) ||
                String.IsNullOrEmpty(boleto.NumeroDocumento.TrimStart('0')))
                throw new Exception("Número do Documento não foi informado.");

            boleto.NumeroDocumento = boleto.NumeroDocumento.PadLeft(10, '0');
        }

        public ICodigoOcorrencia ObtemCodigoOcorrenciaByInt(int numeroOcorrencia)
        {
            switch (numeroOcorrencia)
            {
                case 02:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 02,
                        Descricao = "Entrada Confirmada".ToUpper()
                    };
                case 03:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 03,
                        Descricao = "ENTRADA REJEITADA".ToUpper()
                    };
                case 06:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 06,
                        Descricao = "Liquidação normal".ToUpper()
                    };
                case 09:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 09,
                        Descricao = "Baixado Automat. via Arquivo".ToUpper()
                    };
                case 10:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 10,
                        Descricao = "Baixado conforme instruções da Agência".ToUpper()
                    };
                case 11:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 11,
                        Descricao = "Em Ser - Arquivo de Títulos pendentes".ToUpper()
                    };
                case 12:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 12,
                        Descricao = "Abatimento Concedido".ToUpper()
                    };
                case 13:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 13,
                        Descricao = "Abatimento Cancelado".ToUpper()
                    };
                case 14:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 14,
                        Descricao = "Vencimento Alterado".ToUpper()
                    };
                case 15:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 15,
                        Descricao = "Liquidação em Cartório".ToUpper()
                    };
                case 16:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 16,
                        Descricao = "Título Pago em Cheque – Vinculado".ToUpper()
                    };
                case 17:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 17,
                        Descricao = "Liquidação após baixa ou Título não registrado".ToUpper()
                    };
                case 18:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 18,
                        Descricao = "Acerto de Depositária".ToUpper()
                    };
                case 19:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 19,
                        Descricao = "Confirmação Receb. Inst. de Protesto".ToUpper()
                    };
                case 20:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 20,
                        Descricao = "Confirmação Recebimento Instrução Sustação de Protesto".ToUpper()
                    };
                case 21:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 21,
                        Descricao = "Acerto do Controle do Participante".ToUpper()
                    };
                case 22:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 22,
                        Descricao = "Título Com Pagamento Cancelado".ToUpper()
                    };
                case 23:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 23,
                        Descricao = "Entrada do Título em Cartório".ToUpper()
                    };
                case 24:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 24,
                        Descricao = "Entrada rejeitada por CEP Irregular".ToUpper()
                    };
                case 27:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 27,
                        Descricao = "Baixa Rejeitada".ToUpper()
                    };
                case 28:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 28,
                        Descricao = "Débito de tarifas/custas".ToUpper()
                    };
                case 30:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 30,
                        Descricao = "Alteração de Outros Dados Rejeitados".ToUpper()
                    };
                case 32:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 32,
                        Descricao = "Instrução Rejeitada".ToUpper()
                    };
                case 33:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 33,
                        Descricao = "Confirmação Pedido Alteração Outros Dados".ToUpper()
                    };
                case 34:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 34,
                        Descricao = "Retirado de Cartório e Manutenção Carteira".ToUpper()
                    };
                case 35:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 35,
                        Descricao = "Desagendamento do débito automático".ToUpper()
                    };
                case 40:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 40,
                        Descricao = "Estorno de pagamento".ToUpper()
                    };
                case 55:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 55,
                        Descricao = "Sustado judicial".ToUpper()
                    };
                case 68:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 68,
                        Descricao = "Acerto dos dados do rateio de Crédito".ToUpper()
                    };
                case 69:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 69,
                        Descricao = "Cancelamento dos dados do rateio".ToUpper()
                    };
                default:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = numeroOcorrencia,
                        Descricao = "Código de ocorrência não encontrado, n° ".ToUpper() + numeroOcorrencia
                    };
            }
            /* Impede que um titulo comprometa a leitura de todos os titulos
            throw new Exception(
                String.Format(
                    "Não foi possível obter Código de Comando/Movimento/Ocorrência. Banco: {0} Código: {1}",
                    CodigoBanco, numeroOcorrencia.ToString()));*/
        }

        public ICodigoOcorrencia ObtemCodigoOcorrencia(EnumCodigoOcorrenciaRetorno ocorrenciaRetorno)
        {
            throw new NotImplementedException();
        }

        public IEspecieDocumento ObtemEspecieDocumento(EnumEspecieDocumento especie)
        {
            switch (especie)
            {
                case EnumEspecieDocumento.DuplicataMercantil:
                {
                    return new EspecieDocumento((int) especie)
                    {
                        Codigo = 01,
                        Descricao = "Duplicata mercantil",
                        Sigla = "DM"
                    };
                }
                case EnumEspecieDocumento.NotaPromissoria:
                {
                    return new EspecieDocumento((int) especie)
                    {
                        Codigo = 02,
                        Descricao = "Nota promissória",
                        Sigla = "NP"
                    };
                }
                case EnumEspecieDocumento.NotaDeSeguro:
                {
                    return new EspecieDocumento((int) especie)
                    {
                        Codigo = 03,
                        Descricao = "Nota de seguro",
                        Sigla = "NS"
                    };
                }
                case EnumEspecieDocumento.CobrancaSeriada:
                {
                    return new EspecieDocumento((int) especie)
                    {
                        Codigo = 04,
                        Descricao = "Cobrança seriada",
                        Sigla = "CS"
                    };
                }
                case EnumEspecieDocumento.Recibo:
                {
                    return new EspecieDocumento((int) especie)
                    {
                        Codigo = 05,
                        Descricao = "Recibo",
                        Sigla = "RC"
                    };
                }
                case EnumEspecieDocumento.LetraCambio:
                {
                    return new EspecieDocumento((int) especie)
                    {
                        Codigo = 10,
                        Descricao = "Letra de câmbio",
                        Sigla = "LC"
                    };
                }
                case EnumEspecieDocumento.NotaDebito:
                {
                    return new EspecieDocumento((int) especie)
                    {
                        Codigo = 11,
                        Descricao = "Nota de débito",
                        Sigla = "ND"
                    };
                }
                case EnumEspecieDocumento.DuplicataServico:
                {
                    return new EspecieDocumento((int) especie)
                    {
                        Codigo = 98,
                        Descricao = "Duplicata de Serv.",
                        Sigla = "DS"
                    };
                }
                case EnumEspecieDocumento.Outros:
                {
                    return new EspecieDocumento((int) especie)
                    {
                        Codigo = 99,
                        Descricao = "Outros",
                        Sigla = "OU"
                    };
                }
            }

            throw new Exception(
                String.Format("Não foi possível obter instrução padronizada. Banco: {0} Código Espécie: {1}",
                    CodigoBanco, especie.ToString()));
        }

        public IInstrucao ObtemInstrucaoPadronizada(EnumTipoInstrucao tipoInstrucao, double valorInstrucao,
            DateTime dataInstrucao, int diasInstrucao)
        {
            switch (tipoInstrucao)
            {
                case EnumTipoInstrucao.NaoCobrarJurosDeMora:
                {
                    return new InstrucaoPadronizada
                    {
                        Codigo = 08,
                        QtdDias = diasInstrucao,
                        Valor = valorInstrucao,
                        TextoInstrucao = "Não cobrar juros de mora."
                    };
                }
                case EnumTipoInstrucao.NaoReceberAposOVencimento:
                {
                    return new InstrucaoPadronizada
                    {
                        Codigo = 09,
                        QtdDias = diasInstrucao,
                        Valor = valorInstrucao,
                        TextoInstrucao = "Não receber após o vencimento."
                    };
                }
                case EnumTipoInstrucao.MultaPercentualVencimento:
                {
                    return new InstrucaoPadronizada
                    {
                        Codigo = 10,
                        QtdDias = diasInstrucao,
                        Valor = valorInstrucao,
                        TextoInstrucao = "Multa de 10 % após o 4º dia do vencimento."
                    };
                }
                case EnumTipoInstrucao.NaoReceberAposNDiasCorridos:
                {
                    return new InstrucaoPadronizada
                    {
                        Codigo = 11,
                        QtdDias = diasInstrucao,
                        Valor = valorInstrucao,
                        TextoInstrucao = "Não receber após o 8º dia do vencimento."
                    };
                }
                case EnumTipoInstrucao.CobrarEncargosApos5DiaVencimento:
                {
                    return new InstrucaoPadronizada
                    {
                        Codigo = 12,
                        QtdDias = diasInstrucao,
                        Valor = valorInstrucao,
                        TextoInstrucao = "Cobrar encargos após o 5º dia do vencimento."
                    };
                }
                case EnumTipoInstrucao.CobrarEncargosApos10DiaVencimento:
                {
                    return new InstrucaoPadronizada
                    {
                        Codigo = 13,
                        QtdDias = diasInstrucao,
                        Valor = valorInstrucao,
                        TextoInstrucao = "Cobrar encargos após o 10º dia do vencimento."
                    };
                }
                case EnumTipoInstrucao.CobrarEncargosApos15DiaVencimento:
                {
                    return new InstrucaoPadronizada
                    {
                        Codigo = 14,
                        QtdDias = diasInstrucao,
                        Valor = valorInstrucao,
                        TextoInstrucao = "Cobrar encargos após o 15º dia do vencimento."
                    };
                }
                case EnumTipoInstrucao.ConcederDescontoPagoAposVencimento:
                {
                    return new InstrucaoPadronizada
                    {
                        Codigo = 15,
                        QtdDias = diasInstrucao,
                        Valor = valorInstrucao,
                        TextoInstrucao = "Conceder desconto mesmo se pago após o vencimento."
                    };
                }
            }
            throw new Exception(
                String.Format(
                    "Não foi possível obter instrução padronizada. Banco: {0} Código Instrução: {1} Qtd Dias/Valor: {2}",
                    CodigoBanco, tipoInstrucao.ToString(), valorInstrucao));
        }

        public RetornoGenerico LerArquivoRetorno(List<string> linhasArquivo)
        {
            if (linhasArquivo == null || linhasArquivo.Any() == false)
                throw new ApplicationException("Arquivo informado é inválido/Não existem títulos no retorno.");

            /* Identifica o layout: 240 ou 400 */
            if (linhasArquivo.First().Length == 240)
            {
                var leitor = new LeitorRetornoCnab240Bradesco(linhasArquivo);
                var retornoProcessado = leitor.ProcessarRetorno();

                var objRetornar = new RetornoGenerico(retornoProcessado);
                return objRetornar;
            }
            if (linhasArquivo.First().Length == 400)
            {
                var leitor = new LeitorRetornoCnab400Bradesco(linhasArquivo);
                var retornoProcessado = leitor.ProcessarRetorno();

                var objRetornar = new RetornoGenerico(retornoProcessado);
                return objRetornar;
            }

            throw new Exception("Arquivo de RETORNO com " + linhasArquivo.First().Length + " posições, não é suportado.");
        }

        public RetornoGenericoPagamento LerArquivoRetornoPagamento(List<string> linhasArquivo)
        {
            if (linhasArquivo == null || linhasArquivo.Any() == false)
                throw new ApplicationException("Arquivo informado é inválido/Não existem títulos no retorno.");
             
            if (linhasArquivo.First().Length == 240)
            {
                var leitor = new LeitorRetornoPagamentoCnab240Bradesco(linhasArquivo);
                var retornoProcessado = leitor.ProcessarRetorno();

                var objRetornar = new RetornoGenericoPagamento(retornoProcessado);
                return objRetornar;
            }
             
            throw new Exception("Arquivo de RETORNO com " + linhasArquivo.First().Length + " posições, não é suportado.");
        }

        public RemessaCnab240 GerarArquivoRemessaCnab240(List<Boleto> boletos)
        {
            throw new NotImplementedException();
        }

        public RemessaCnab400 GerarArquivoRemessaCnab400(List<Boleto> boletos)
        {
            throw new NotImplementedException();
        }
        public ICodigoOcorrencia ObtemCodigoOcorrenciaPagamento(string numeroOcorrencia)
        {
            switch (numeroOcorrencia)
            {
                case "00": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "00", Descricao = "Pagamento confirmado".ToUpper() }; }
                case "01": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "01", Descricao = "Insuficiência de Fundos - Débito Não Efetuado".ToUpper() }; }
                case "02": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "02", Descricao = "Crédito ou Débito Cancelado pelo Pagador/Credor".ToUpper() }; }
                case "03": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "03", Descricao = "Débito Autorizado pela Agência - Efetuado".ToUpper() }; }
                case "AA": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "AA", Descricao = "Controle Inválido".ToUpper() }; }
                case "AB": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "AB", Descricao = "Tipo de Operação Inválido".ToUpper() }; }
                case "AC": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "AC", Descricao = "Tipo de Serviço Inválido".ToUpper() }; }
                case "AD": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "AD", Descricao = "Forma de Lançamento Inválida".ToUpper() }; }
                case "AE": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "AE", Descricao = "Tipo/Número de Inscrição Inválido".ToUpper() }; }
                case "AF": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "AF", Descricao = "Código de Convênio Inválido".ToUpper() }; }
                case "AG": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "AG", Descricao = "Agência/Conta Corrente/DV Inválido".ToUpper() }; }
                case "AH": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "AH", Descricao = "Nº Seqüencial do Registro no Lote Inválido".ToUpper() }; }
                case "AI": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "AI", Descricao = "Código de Segmento de Detalhe Inválido".ToUpper() }; }
                case "AJ": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "AJ", Descricao = "Tipo de Movimento Inválido".ToUpper() }; }
                case "AK": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "AK", Descricao = "Código da Câmara de Compensação do Banco Favorecido/Depositário Inválido".ToUpper() }; }
                case "AL": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "AL", Descricao = "Código do Banco Favorecido ou Depositário Inválido".ToUpper() }; }
                case "AM": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "AM", Descricao = "Agência Mantenedora da Conta Corrente do Favorecido Inválida".ToUpper() }; }
                case "AN": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "AN", Descricao = "Conta Corrente/DV do Favorecido Inválido".ToUpper() }; }
                case "AO": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "AO", Descricao = "Nome do Favorecido Não Informado".ToUpper() }; }
                case "AP": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "AP", Descricao = "Data Lançamento Inválido".ToUpper() }; }
                case "AQ": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "AQ", Descricao = "Tipo/Quantidade da Moeda Inválido".ToUpper() }; }
                case "AR": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "AR", Descricao = "Valor do Lançamento Inválido".ToUpper() }; }
                case "AS": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "AS", Descricao = "Aviso ao Favorecido - Identificação Inválida".ToUpper() }; }
                case "AT": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "AT", Descricao = "Tipo/Número de Inscrição do Favorecido Inválido".ToUpper() }; }
                case "AU": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "AU", Descricao = "Logradouro do Favorecido Não Informado".ToUpper() }; }
                case "AV": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "AV", Descricao = "Nº do Local do Favorecido Não Informado ".ToUpper() }; }
                case "AW": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "AW", Descricao = "Cidade do Favorecido Não Informada".ToUpper() }; }
                case "AX": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "AX", Descricao = "CEP/Complemento do Favorecido Inválido".ToUpper() }; }
                case "AY": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "AY", Descricao = "Sigla do Estado do Favorecido Inválida".ToUpper() }; }
                case "AZ": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "AZ", Descricao = "Código/Nome do Banco Depositário Inválido".ToUpper() }; }
                case "BA": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "BA", Descricao = "Código/Nome da Agência Depositária Não Informado".ToUpper() }; }
                case "BB": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "BB", Descricao = "Seu Número Inválido".ToUpper() }; }
                case "BC": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "BC", Descricao = "Nosso Número Inválido".ToUpper() }; }
                case "BD": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "BD", Descricao = "Inclusão Efetuada com Sucesso".ToUpper() }; }
                case "BE": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "BE", Descricao = "Alteração Efetuada com Sucesso".ToUpper() }; }
                case "BF": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "BF", Descricao = "Exclusão Efetuada com Sucesso".ToUpper() }; }
                case "BG": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "BG", Descricao = "Agência/Conta Impedida Legalmente".ToUpper() }; }
                case "BH": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "BH", Descricao = "Empresa não pagou salário".ToUpper() }; }
                case "BI": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "BI", Descricao = "Falecimento do mutuário".ToUpper() }; }
                case "BJ": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "BJ", Descricao = "Empresa não enviou remessa do mutuário".ToUpper() }; }
                case "BK": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "BK", Descricao = "Empresa não enviou remessa no vencimento".ToUpper() }; }
                case "BL": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "BL", Descricao = "Valor da parcela inválida".ToUpper() }; }
                case "BM": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "BM", Descricao = "Identificação do contrato inválida".ToUpper() }; }
                case "BN": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "BN", Descricao = "Operação de Consignação Incluída com Sucesso".ToUpper() }; }
                case "BO": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "BO", Descricao = "Operação de Consignação Alterada com Sucesso".ToUpper() }; }
                case "BP": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "BP", Descricao = "Operação de Consignação Excluída com Sucesso".ToUpper() }; }
                case "BQ": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "BQ", Descricao = "Operação de Consignação Liquidada com Sucesso".ToUpper() }; }
                case "BR": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "BR", Descricao = "Reativação Efetuada com Sucesso".ToUpper() }; }
                case "BS": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "BS", Descricao = "Suspensão Efetuada com Sucesso".ToUpper() }; }
                case "CA": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "CA", Descricao = "Código de Barras - Código do Banco Inválido".ToUpper() }; }
                case "CB": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "CB", Descricao = "Código de Barras - Código da Moeda Inválido".ToUpper() }; }
                case "CC": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "CC", Descricao = "Código de Barras - Dígito Verificador Geral Inválido".ToUpper() }; }
                case "CD": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "CD", Descricao = "Código de Barras - Valor do Título Inválido".ToUpper() }; }
                case "CE": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "CE", Descricao = "Código de Barras - Campo Livre Inválido".ToUpper() }; }
                case "CF": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "CF", Descricao = "Valor do Documento Inválido".ToUpper() }; }
                case "CG": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "CG", Descricao = "Valor do Abatimento Inválido".ToUpper() }; }
                case "CH": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "CH", Descricao = "Valor do Desconto Inválido".ToUpper() }; }
                case "CI": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "CI", Descricao = "Valor de Mora Inválido".ToUpper() }; }
                case "CJ": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "CJ", Descricao = "Valor da Multa Inválido ".ToUpper() }; }
                case "CK": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "CK", Descricao = "Valor do IR Inválido".ToUpper() }; }
                case "CL": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "CL", Descricao = "Valor do ISS Inválido".ToUpper() }; }
                case "CM": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "CM", Descricao = "Valor do IOF Inválido".ToUpper() }; }
                case "CN": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "CN", Descricao = "Valor de Outras Deduções Inválido".ToUpper() }; }
                case "CO": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "CO", Descricao = "Valor de Outros Acréscimos Inválido".ToUpper() }; }
                case "CP": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "CP", Descricao = "Valor do INSS Inválido".ToUpper() }; }
                case "HA": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "HA", Descricao = "Lote Não Aceito".ToUpper() }; }
                case "HB": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "HB", Descricao = "Inscrição da Empresa Inválida para o Contrato".ToUpper() }; }
                case "HC": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "HC", Descricao = "Convênio com a Empresa Inexistente/Inválido para o Contrato".ToUpper() }; }
                case "HD": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "HD", Descricao = "Agência/Conta Corrente da Empresa Inexistente/Inválido para o Contrato".ToUpper() }; }
                case "HE": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "HE", Descricao = "Tipo de Serviço Inválido para o Contrato".ToUpper() }; }
                case "HF": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "HF", Descricao = "Conta Corrente da Empresa com Saldo Insuficiente".ToUpper() }; }
                case "HG": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "HG", Descricao = "Lote de Serviço Fora de Seqüência".ToUpper() }; }
                case "HH": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "HH", Descricao = "Lote de Serviço Inválido".ToUpper() }; }
                case "HI": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "HI", Descricao = "Arquivo não aceito".ToUpper() }; }
                case "HJ": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "HJ", Descricao = "Tipo de Registro Inválido".ToUpper() }; }
                case "HK": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "HK", Descricao = "Código Remessa / Retorno Inválido".ToUpper() }; }
                case "HL": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "HL", Descricao = "Versão de layout inválida".ToUpper() }; }
                case "HM": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "HM", Descricao = "Mutuário não identificado".ToUpper() }; }
                case "HN": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "HN", Descricao = "Tipo do beneficio não permite empréstimo".ToUpper() }; }
                case "HO": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "HO", Descricao = "Beneficio cessado/suspenso".ToUpper() }; }
                case "HP": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "HP", Descricao = "Beneficio possui representante legal".ToUpper() }; }
                case "HQ": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "HQ", Descricao = "Beneficio é do tipo PA (Pensão alimentícia)".ToUpper() }; }
                case "HR": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "HR", Descricao = "Quantidade de contratos permitida excedida".ToUpper() }; }
                case "HS": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "HS", Descricao = "Beneficio não pertence ao Banco informado".ToUpper() }; }
                case "HT": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "HT", Descricao = "Início do desconto informado já ultrapassado".ToUpper() }; }
                case "HU": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "HU", Descricao = "Número da parcela inválida".ToUpper() }; }
                case "HV": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "HV", Descricao = "Quantidade de parcela inválida".ToUpper() }; }
                case "HW": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "HW", Descricao = "Margem consignável excedida para o mutuário dentro do prazo do contrato".ToUpper() }; }
                case "HX": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "HX", Descricao = "Empréstimo já cadastrado".ToUpper() }; }
                case "HY": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "HY", Descricao = "Empréstimo inexistente".ToUpper() }; }
                case "HZ": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "HZ", Descricao = "Empréstimo já encerrado".ToUpper() }; }
                case "H1": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "H1", Descricao = "Arquivo sem trailer ".ToUpper() }; }
                case "H2": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "H2", Descricao = "Mutuário sem crédito na competência".ToUpper() }; }
                case "H3": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "H3", Descricao = "Não descontado – outros motivos".ToUpper() }; }
                case "H4": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "H4", Descricao = "Retorno de Crédito não pago".ToUpper() }; }
                case "H5": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "H5", Descricao = "Cancelamento de empréstimo retroativo".ToUpper() }; }
                case "H6": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "H6", Descricao = "Outros Motivos de Glosa".ToUpper() }; }
                case "H7": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "H7", Descricao = "Margem consignável excedida para o mutuário acima do prazo do contrato".ToUpper() }; }
                case "H8": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "H8", Descricao = "Mutuário desligado do empregador".ToUpper() }; }
                case "H9": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "H9", Descricao = "Mutuário afastado por licença".ToUpper() }; }
                case "IA": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "IA", Descricao = "Primeiro nome do mutuário diferente do primeiro nome do movimento do censo ou diferente da base de Titular do Benefício".ToUpper() }; }
                case "IB": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "IB", Descricao = "Benefício suspenso/cessado pela APS ou Sisobi".ToUpper() }; }
                case "IC": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "IC", Descricao = "Benefício suspenso por dependência de cálculo".ToUpper() }; }
                case "ID": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "ID", Descricao = "Benefício suspenso/cessado pela inspetoria/auditoria".ToUpper() }; }
                case "IE": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "IE", Descricao = "Benefício bloqueado para empréstimo pelo beneficiário".ToUpper() }; }
                case "IF": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "IF", Descricao = "Benefício bloqueado para empréstimo por TBM".ToUpper() }; }
                case "IG": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "IG", Descricao = "Benefício está em fase de concessão de PA ou desdobramento".ToUpper() }; }
                case "IH": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "IH", Descricao = "Benefício cessado por óbito".ToUpper() }; }
                case "II": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "II", Descricao = "Benefício cessado por fraude".ToUpper() }; }
                case "IJ": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "IJ", Descricao = "Benefício cessado por concessão de outro benefício".ToUpper() }; }
                case "IK": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "IK", Descricao = "Benefício cessado: estatutário transferido para órgão de origem".ToUpper() }; }
                case "IL": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "IL", Descricao = "Empréstimo suspenso pela APS".ToUpper() }; }
                case "IM": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "IM", Descricao = "Empréstimo cancelado pelo banco".ToUpper() }; }
                case "IN": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "IN", Descricao = "Crédito transformado em PAB".ToUpper() }; }
                case "IO": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "IO", Descricao = "Término da consignação foi alterado".ToUpper() }; }
                case "IP": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "IP", Descricao = "Fim do empréstimo ocorreu durante período de suspensão ou concessão".ToUpper() }; }
                case "IQ": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "IQ", Descricao = "Empréstimo suspenso pelo banco".ToUpper() }; }
                case "IR": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "IR", Descricao = "Não averbação de contrato – quantidade de parcelas/competências informadas ultrapassou a data limite da extinção de cota do dependente titular de benefícios".ToUpper() }; }
                case "TA": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "TA", Descricao = "Lote Não Aceito - Totais do Lote com Diferença".ToUpper() }; }
                case "YA": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "YA", Descricao = "Título Não Encontrado".ToUpper() }; }
                case "YB": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "YB", Descricao = "Identificador Registro Opcional Inválido".ToUpper() }; }
                case "YC": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "YC", Descricao = "Código Padrão Inválido".ToUpper() }; }
                case "YD": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "YD", Descricao = "Código de Ocorrência Inválido".ToUpper() }; }
                case "YE": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "YE", Descricao = "Complemento de Ocorrência Inválido".ToUpper() }; }
                case "YF": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "YF", Descricao = "Alegação já Informada".ToUpper() }; }
                case "ZA": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "ZA", Descricao = "Agência / Conta do Favorecido Substituída Observação: As ocorrências iniciadas com ZA tem caráter informativo para o cliente".ToUpper() }; }
                case "ZB": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "ZB", Descricao = "Divergência entre o primeiro e último nome do beneficiário versus primeiro e último nome na Receita Federal".ToUpper() }; }
                case "ZC": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "ZC", Descricao = "Confirmação de Antecipação de Valor".ToUpper() }; }
                case "ZD": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "ZD", Descricao = "Antecipação parcial de valor".ToUpper() }; }
                case "ZE": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "ZE", Descricao = "Título bloqueado na base".ToUpper() }; }
                case "ZF": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "ZF", Descricao = "Sistema em contingência – título valor maior que referência".ToUpper() }; }
                case "ZG": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "ZG", Descricao = "Sistema em contingência – título vencido".ToUpper() }; }
                case "ZH": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "ZH", Descricao = "Sistema em contingência – título indexado".ToUpper() }; }
                case "ZI": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "ZI", Descricao = "Beneficiário divergente".ToUpper() }; }
                case "ZJ": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "ZJ", Descricao = "Limite de pagamentos parciais excedido".ToUpper() }; }
                case "ZK": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "ZK", Descricao = "Boleto já liquidado".ToUpper() }; }
                                                                          
                /*Exclusivo Bradesco*/                                    
                case "5A": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "5A", Descricao = "Agendado sob lista de debito".ToUpper() }; }
                case "5B": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "5B", Descricao = "Pagamento não autoriza sob lista de debito".ToUpper() }; }
                case "5C": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "5C", Descricao = "Lista com mais de uma modalidade".ToUpper() }; }
                case "5D": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "5D", Descricao = "Lista com mais de uma data de pagamento".ToUpper() }; }
                case "5E": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "5E", Descricao = "Número de lista duplicado".ToUpper() }; }
                case "5F": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "5F", Descricao = "Lista de debito vencida e não autorizada".ToUpper() }; }
                case "5I": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "5I", Descricao = "Ordem de Pagamento emitida".ToUpper() }; }
                case "5M": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "5M", Descricao = "Número de lista de debito invalida".ToUpper() }; }
                case "5T": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "5T", Descricao = "Pagamento realizado em contrato na condição de TESTE".ToUpper() }; }
            }
            throw new Exception(
                String.Format(
                    "Não foi possível obter Código de Comando/Movimento/Ocorrência. Banco: {0} Código: {1}",
                    CodigoBanco, numeroOcorrencia.ToString()));
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
                        Descricao = "Cancelamento de abatimento concedido"
                    };
                }
                case EnumCodigoOcorrenciaRemessa.AlteracaoDeVencimento:
                {
                    return new CodigoOcorrencia((int) ocorrencia)
                    {
                        Codigo = 06,
                        Descricao = "Alteração de vencimento"
                    };
                }
                case EnumCodigoOcorrenciaRemessa.AlteracaoDoControleDoParticipante:
                {
                    return new CodigoOcorrencia((int) ocorrencia)
                    {
                        Codigo = 07,
                        Descricao = "Alteração do controle do participante"
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
                        Descricao = "Pedido de Protesto"
                    };
                }
                case EnumCodigoOcorrenciaRemessa.SustarProtestoEBaixarTitulo:
                {
                    return new CodigoOcorrencia((int) ocorrencia)
                    {
                        Codigo = 18,
                        Descricao = "Sustar protesto e baixar título"
                    };
                }
                case EnumCodigoOcorrenciaRemessa.SustarProtestoEManterEmCarteira:
                {
                    return new CodigoOcorrencia((int) ocorrencia)
                    {
                        Codigo = 19,
                        Descricao = "Sustar protesto e manter em carteira"
                    };
                }
                case EnumCodigoOcorrenciaRemessa.TransferenciaCessaoCredito:
                {
                    return new CodigoOcorrencia((int) ocorrencia)
                    {
                        Codigo = 22,
                        Descricao = "Transferência cessão crédito ID. Prod. 10"
                    };
                }
                case EnumCodigoOcorrenciaRemessa.TransferenciaEntreCarteiras:
                {
                    return new CodigoOcorrencia((int) ocorrencia)
                    {
                        Codigo = 23,
                        Descricao = "Transferência entre carteiras"
                    };
                }
                case EnumCodigoOcorrenciaRemessa.DevTransferenciaEntreCarteiras:
                {
                    return new CodigoOcorrencia((int) ocorrencia)
                    {
                        Codigo = 24,
                        Descricao = "Dev. Transferência entre carteiras"
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
                case EnumCodigoOcorrenciaRemessa.DesagendamentoDoDebitoAutomatico:
                {
                    return new CodigoOcorrencia((int) ocorrencia)
                    {
                        Codigo = 35,
                        Descricao = "Desagendamento do débito automático"
                    };
                }
                case EnumCodigoOcorrenciaRemessa.AcertoNosDadosDoRateioDeCredito:
                {
                    return new CodigoOcorrencia((int) ocorrencia)
                    {
                        Codigo = 68,
                        Descricao = "Acerto nos dados do rateio de crédito"
                    };
                }
                case EnumCodigoOcorrenciaRemessa.CancelamentoDoRateioDeCredito:
                {
                    return new CodigoOcorrencia((int) ocorrencia)
                    {
                        Codigo = 69,
                        Descricao = "Cancelamento do rateio do crédito"
                    };
                }
            }
            throw new Exception(
                String.Format(
                    "Não foi possível obter Código de Comando/Movimento/Ocorrência. Banco: {0} Código: {1}",
                    CodigoBanco, ocorrencia.ToString()));
        }
        public int CodigoJurosMora(CodigoJurosMora codigoJurosMora)
        {
            return 0;
        }

        public int CodigoProteso(bool protestar = true)
        {
            return 0;
        }
    }
}
