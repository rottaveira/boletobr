using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using BoletoBr.Arquivo.CNAB240.Remessa;
using BoletoBr.Arquivo.CNAB400.Remessa;
using BoletoBr.Arquivo.Generico.Retorno;
using BoletoBr.Dominio;
using BoletoBr.Dominio.Instrucao;
using BoletoBr.Enums;
using BoletoBr.Interfaces;

namespace BoletoBr.Bancos.Brasil
{
    public class BancoBrasil : IBanco
    {
        #region Campos
        public string CodigoBanco { get; set; }
        public string DigitoBanco { get; set; }
        public string NomeBanco { get; set; }
        public Image LogotipoBancoParaExibicao { get; set; }

        private string _digitoAutoConferenciaNossoNumero = String.Empty;
        private int _digitoAutoConferenciaBoleto = 0;
        #endregion

        public BancoBrasil()
        {
            CodigoBanco = "001";
            DigitoBanco = "9";
            NomeBanco = "Banco do Brasil";
            LocalDePagamento = "Pagável em qualquer banco até o vencimento.";
            MoedaBanco = "9";
        }

        public string LocalDePagamento { get; private set; }
        public string MoedaBanco { get; private set; }

        public void ValidaBoletoComNormasBanco(Boleto boleto)
        {
            if (string.IsNullOrEmpty(boleto.CarteiraCobranca.Codigo))
                throw new Exception("Carteira não informada. Utilize a carteira 11, 16, 17, 18, 18-019, 18-027, 18-035, 18-140 ou 31.");

            //Verifica as carteiras implementadas
            if (!boleto.CarteiraCobranca.Codigo.Equals("11") &
                !boleto.CarteiraCobranca.Codigo.Equals("16") &
                !boleto.CarteiraCobranca.Codigo.Equals("17") &
                !boleto.CarteiraCobranca.Codigo.Equals("17-019") &
                !boleto.CarteiraCobranca.Codigo.Equals("18") &
                !boleto.CarteiraCobranca.Codigo.Equals("18-019") &
                !boleto.CarteiraCobranca.Codigo.Equals("18-027") &
                !boleto.CarteiraCobranca.Codigo.Equals("18-035") &
                !boleto.CarteiraCobranca.Codigo.Equals("18-140") &
                !boleto.CarteiraCobranca.Codigo.Equals("31"))

                throw new Exception("Carteira não informada. Utilize a carteira 11, 16, 17, 17-019, 18, 18-019, 18-027, 18-035, 18-140 ou 31.");

            //Verifica se o nosso número é válido
            if (boleto.NossoNumeroFormatado == String.Empty)
                throw new Exception("Nosso número inválido");

            #region Carteira 11

            //Carteira 11 com nosso número de 17 posições
            if (boleto.CarteiraCobranca.Codigo.Equals("11"))
            {
                if (!boleto.TipoModalidade.BoletoBrToStringSafe().Equals("21"))
                {
                    if (boleto.NossoNumeroFormatado.Length > 17)
                        throw new ValidacaoBoletoException(String.Format("Para a carteira {0}, a quantidade máxima são de 17 de posições para o nosso número", boleto.CarteiraCobranca.Codigo));

                    if (boleto.CedenteBoleto.Convenio.Length == 6)
                        boleto.SetNossoNumeroFormatado(String.Format("{0}{1}", boleto.CedenteBoleto.Convenio, boleto.NossoNumeroFormatado.PadLeft(11, '0')));
                    else
                        boleto.SetNossoNumeroFormatado(boleto.NossoNumeroFormatado.PadLeft(17, '0'));
                }
                else
                {
                    if (boleto.CedenteBoleto.Convenio.Length != 6)
                        throw new ValidacaoBoletoException(String.Format("Para a carteira {0} e o tipo da modalidade 21, o número do convênio são de 6 posições", boleto.CarteiraCobranca.Codigo));

                    boleto.SetNossoNumeroFormatado(boleto.NossoNumeroFormatado.PadLeft(17, '0'));
                }
            }

            #endregion Carteira 11

            #region Carteira 16
            //Carteira 18 com nosso número de 11 posições
            if (boleto.CarteiraCobranca.Codigo.Equals("16"))
            {
                if (!boleto.TipoModalidade.BoletoBrToStringSafe().Equals("21"))
                {
                    if (boleto.NossoNumeroFormatado.Length > 11)
                        throw new ValidacaoBoletoException(string.Format("Para a carteira {0}, a quantidade máxima são de 11 de posições para o nosso número", boleto.CarteiraCobranca.Codigo));

                    if (boleto.CedenteBoleto.Convenio.Length == 6)
                        boleto.SetNossoNumeroFormatado(string.Format("{0}{1}", boleto.CedenteBoleto.Convenio, boleto.NossoNumeroFormatado.PadLeft(11, '0')));
                    else
                        boleto.SetNossoNumeroFormatado(boleto.NossoNumeroFormatado.PadLeft(11, '0'));
                }
                else
                {
                    if (boleto.CedenteBoleto.Convenio.Length != 6)
                        throw new ValidacaoBoletoException(string.Format("Para a carteira {0} e o tipo da modalidade 21, o número do convênio são de 6 posições", boleto.CarteiraCobranca.Codigo));

                    boleto.SetNossoNumeroFormatado(boleto.NossoNumeroFormatado.PadLeft(17, '0'));
                }
            }
            #endregion Carteira 16

            #region Carteira 17

            //Carteira 17
            if (boleto.CarteiraCobranca.Codigo.Equals("17"))
            {
                switch (boleto.CedenteBoleto.Convenio.Length)
                {
                    //O BB manda como padrão 7 posições, mas é possível solicitar um convênio com 6 posições na carteira 17
                    case 6:
                        if (boleto.NossoNumeroFormatado.Length > 12)
                            throw new ValidacaoBoletoException(string.Format("Para a carteira {0}, a quantidade máxima são de 12 de posições para o nosso número", boleto.CarteiraCobranca.Codigo));
                        boleto.SetNossoNumeroFormatado(boleto.NossoNumeroFormatado.PadLeft(12, '0'));
                        break;
                    case 7:
                        if (boleto.NossoNumeroFormatado.Length > 17)
                            throw new ValidacaoBoletoException(string.Format("Para a carteira {0}, a quantidade máxima são de 10 de posições para o nosso número", boleto.CarteiraCobranca.Codigo));
                        break;
                    default:
                        throw new ValidacaoBoletoException(string.Format("Para a carteira {0}, o número do convênio deve ter 6 ou 7 posições", boleto.CarteiraCobranca.Codigo));
                }
            }

            #endregion Carteira 17

            #region Carteira 17-019
            //Carteira 17, com variação 019
            if (boleto.CarteiraCobranca.Codigo.Equals("17-019"))
            {
                /*
                 * Convênio de 7 posições
                 * Nosso Número com 17 posições
                 */
                if (boleto.CedenteBoleto.Convenio.Length == 7)
                {
                    if (boleto.NossoNumeroFormatado.Length > 10)
                        throw new ValidacaoBoletoException(string.Format("Para a carteira {0}, a quantidade máxima são de 10 de posições para o nosso número", boleto.CarteiraCobranca.Codigo));

                    boleto.SetNossoNumeroFormatado(string.Format("{0}{1}", boleto.CedenteBoleto.Convenio, boleto.NossoNumeroFormatado.PadLeft(10, '0')));
                }
                /*
                 * Convênio de 6 posições
                 * Nosso Número com 11 posições
                 */
                else if (boleto.CedenteBoleto.Convenio.Length == 6)
                {
                    //Nosso Número com 17 posições
                    if ((boleto.CedenteBoleto.CodigoCedente.Length + boleto.NossoNumeroFormatado.Length) > 11)
                        throw new ValidacaoBoletoException(string.Format("Para a carteira {0}, a quantidade máxima são de 11 de posições para o nosso número. Onde o nosso número é formado por CCCCCCNNNNN-X: C -> número do convênio fornecido pelo Banco, N -> seqüencial atribuído pelo cliente e X -> dígito verificador do “Nosso-Número”.", boleto.CarteiraCobranca.Codigo));

                    boleto.SetNossoNumeroFormatado(string.Format("{0}{1}", boleto.CedenteBoleto.Convenio, boleto.NossoNumeroFormatado.PadLeft(5, '0')));
                }
                /*
                  * Convênio de 4 posições
                  * Nosso Número com 11 posições
                  */
                else if (boleto.CedenteBoleto.Convenio.Length == 4)
                {
                    if (boleto.NossoNumeroFormatado.Length > 7)
                        throw new ValidacaoBoletoException(string.Format("Para a carteira {0}, a quantidade máxima são de 7 de posições para o nosso número [{1}]", boleto.CarteiraCobranca.Codigo, boleto.NossoNumeroFormatado));

                    boleto.SetNossoNumeroFormatado(string.Format("{0}{1}", boleto.CedenteBoleto.Convenio, boleto.NossoNumeroFormatado.PadLeft(7, '0')));
                }
                else
                    boleto.SetNossoNumeroFormatado(boleto.NossoNumeroFormatado.PadLeft(11, '0'));
            }
            #endregion Carteira 17-019

            #region Carteira 18
            //Carteira 18 com nosso número de 11 posições
            if (boleto.CarteiraCobranca.Codigo.Equals("18"))
                boleto.SetNossoNumeroFormatado(boleto.NossoNumeroFormatado.PadLeft(11, '0'));
            #endregion Carteira 18

            #region Carteira 18-019
            //Carteira 18, com variação 019
            if (boleto.CarteiraCobranca.Codigo.Equals("18-019"))
            {
                /*
                 * Convênio de 7 posições
                 * Nosso Número com 17 posições
                 */
                if (boleto.CedenteBoleto.Convenio.Length == 7)
                {
                    if (boleto.NossoNumeroFormatado.Length > 10)
                        throw new ValidacaoBoletoException(string.Format("Para a carteira {0}, a quantidade máxima são de 10 de posições para o nosso número", boleto.CarteiraCobranca.Codigo));

                    boleto.SetNossoNumeroFormatado(string.Format("{0}{1}", boleto.CedenteBoleto.Convenio, boleto.NossoNumeroFormatado.PadLeft(10, '0')));
                }
                /*
                 * Convênio de 6 posições
                 * Nosso Número com 11 posições
                 */
                else if (boleto.CedenteBoleto.Convenio.Length == 6)
                {
                    //Modalidades de Cobrança Sem Registro – Carteira 16 e 18
                    //Nosso Número com 17 posições
                    if (!boleto.TipoModalidade.Equals("21"))
                    {
                        if ((boleto.CedenteBoleto.CodigoCedente.ToString().Length + boleto.NossoNumeroFormatado.Length) > 11)
                            throw new ValidacaoBoletoException(string.Format("Para a carteira {0}, a quantidade máxima são de 11 de posições para o nosso número. Onde o nosso número é formado por CCCCCCNNNNN-X: C -> número do convênio fornecido pelo Banco, N -> seqüencial atribuído pelo cliente e X -> dígito verificador do “Nosso-Número”.", boleto.CarteiraCobranca.Codigo));

                        boleto.SetNossoNumeroFormatado(string.Format("{0}{1}", boleto.CedenteBoleto.Convenio,
                            boleto.NossoNumeroFormatado.PadLeft(5, '0')));
                    }
                    else
                    {
                        if (boleto.CedenteBoleto.Convenio.ToString().Length != 6)
                            throw new ValidacaoBoletoException(string.Format("Para a carteira {0} e o tipo da modalidade 21, o número do convênio são de 6 posições", boleto.CarteiraCobranca.Codigo));

                        boleto.SetNossoNumeroFormatado(boleto.NossoNumeroFormatado.PadLeft(17, '0'));
                    }
                }
                /*
                  * Convênio de 4 posições
                  * Nosso Número com 11 posições
                  */
                else if (boleto.CedenteBoleto.Convenio.ToString().Length == 4)
                {
                    if (boleto.NossoNumeroFormatado.Length > 7)
                        throw new NotImplementedException(string.Format("Para a carteira {0}, a quantidade máxima são de 7 de posições para o nosso número [{1}]", boleto.CarteiraCobranca.Codigo, boleto.NossoNumeroFormatado));

                    boleto.SetNossoNumeroFormatado(string.Format("{0}{1}", boleto.CedenteBoleto.Convenio, boleto.NossoNumeroFormatado.PadLeft(7, '0')));
                }
                else
                    boleto.SetNossoNumeroFormatado(boleto.NossoNumeroFormatado.PadLeft(11, '0'));
            }
            #endregion Carteira 18-019

            //Para atender o cliente Fiemg foi adaptado no código na variação 18-027 as variações 18-035 e 18-140
            #region Carteira 18-027
            //Carteira 18, com variação 019
            if (boleto.CarteiraCobranca.Codigo.Equals("18-027"))
            {
                /*
                 * Convênio de 7 posições
                 * Nosso Número com 17 posições
                 */
                if (boleto.CedenteBoleto.Convenio.ToString().Length == 7)
                {
                    if (boleto.NossoNumeroFormatado.Length > 10)
                        throw new NotImplementedException(string.Format("Para a carteira {0}, a quantidade máxima são de 10 de posições para o nosso número", boleto.CarteiraCobranca.Codigo));

                    boleto.SetNossoNumeroFormatado(string.Format("{0}{1}", boleto.CedenteBoleto.Convenio, boleto.NossoNumeroFormatado.PadLeft(10, '0')));
                }
                /*
                 * Convênio de 6 posições
                 * Nosso Número com 11 posições
                 */
                else if (boleto.CedenteBoleto.Convenio.ToString().Length == 6)
                {
                    //Modalidades de Cobrança Sem Registro – Carteira 16 e 18
                    //Nosso Número com 17 posições
                    if (!boleto.TipoModalidade.Equals("21"))
                    {
                        if ((boleto.CedenteBoleto.CodigoCedente.ToString().Length + boleto.NossoNumeroFormatado.Length) > 11)
                            throw new NotImplementedException(string.Format("Para a carteira {0}, a quantidade máxima são de 11 de posições para o nosso número. Onde o nosso número é formado por CCCCCCNNNNN-X: C -> número do convênio fornecido pelo Banco, N -> seqüencial atribuído pelo cliente e X -> dígito verificador do “Nosso-Número”.", boleto.CarteiraCobranca.Codigo));

                        boleto.SetNossoNumeroFormatado(string.Format("{0}{1}", boleto.CedenteBoleto.Convenio, boleto.NossoNumeroFormatado.PadLeft(5, '0')));
                    }
                    else
                    {
                        if (boleto.CedenteBoleto.Convenio.ToString().Length != 6)
                            throw new NotImplementedException(string.Format("Para a carteira {0} e o tipo da modalidade 21, o número do convênio são de 6 posições", boleto.CarteiraCobranca.Codigo));

                        boleto.SetNossoNumeroFormatado(boleto.NossoNumeroFormatado.PadLeft(17, '0'));
                    }
                }
                /*
                  * Convênio de 4 posições
                  * Nosso Número com 11 posições
                  */
                else if (boleto.CedenteBoleto.Convenio.ToString().Length == 4)
                {
                    if (boleto.NossoNumeroFormatado.Length > 7)
                        throw new NotImplementedException(string.Format("Para a carteira {0}, a quantidade máxima são de 7 de posições para o nosso número [{1}]", boleto.CarteiraCobranca.Codigo, boleto.NossoNumeroFormatado));

                    boleto.SetNossoNumeroFormatado(string.Format("{0}{1}", boleto.CedenteBoleto.Convenio, boleto.NossoNumeroFormatado.PadLeft(7, '0')));
                }
                else
                    boleto.SetNossoNumeroFormatado(boleto.NossoNumeroFormatado.PadLeft(11, '0'));
            }
            #endregion Carteira 18-027

            #region Carteira 18-035
            //Carteira 18, com variação 019
            if (boleto.CarteiraCobranca.Codigo.Equals("18-035"))
            {
                /*
                 * Convênio de 7 posições
                 * Nosso Número com 17 posições
                 */
                if (boleto.CedenteBoleto.Convenio.ToString().Length == 7)
                {
                    if (boleto.NossoNumeroFormatado.Length > 10)
                        throw new ValidacaoBoletoException(string.Format("Para a carteira {0}, a quantidade máxima são de 10 de posições para o nosso número", boleto.CarteiraCobranca.Codigo));

                    boleto.SetNossoNumeroFormatado(string.Format("{0}{1}", boleto.CedenteBoleto.Convenio, boleto.NossoNumeroFormatado.PadLeft(10, '0')));
                }
                /*
                 * Convênio de 6 posições
                 * Nosso Número com 11 posições
                 */
                else if (boleto.CedenteBoleto.Convenio.ToString().Length == 6)
                {
                    //Modalidades de Cobrança Sem Registro – Carteira 16 e 18
                    //Nosso Número com 17 posições
                    if (!boleto.TipoModalidade.Equals("21"))
                    {
                        if ((boleto.CedenteBoleto.CodigoCedente.ToString().Length + boleto.NossoNumeroFormatado.Length) > 11)
                            throw new NotImplementedException(string.Format("Para a carteira {0}, a quantidade máxima são de 11 de posições para o nosso número. Onde o nosso número é formado por CCCCCCNNNNN-X: C -> número do convênio fornecido pelo Banco, N -> seqüencial atribuído pelo cliente e X -> dígito verificador do “Nosso-Número”.", boleto.CarteiraCobranca.Codigo));

                        boleto.SetNossoNumeroFormatado(string.Format("{0}{1}", boleto.CedenteBoleto.Convenio, boleto.NossoNumeroFormatado.PadLeft(5, '0')));
                    }
                    else
                    {
                        if (boleto.CedenteBoleto.Convenio.ToString().Length != 6)
                            throw new NotImplementedException(string.Format("Para a carteira {0} e o tipo da modalidade 21, o número do convênio são de 6 posições", boleto.CarteiraCobranca.Codigo));

                        boleto.SetNossoNumeroFormatado(boleto.NossoNumeroFormatado.PadLeft(17, '0'));
                    }
                }
                /*
                  * Convênio de 4 posições
                  * Nosso Número com 11 posições
                  */
                else if (boleto.CedenteBoleto.Convenio.ToString().Length == 4)
                {
                    if (boleto.NossoNumeroFormatado.Length > 7)
                        throw new ValidacaoBoletoException(string.Format("Para a carteira {0}, a quantidade máxima são de 7 de posições para o nosso número [{1}]", boleto.CarteiraCobranca.Codigo, boleto.NossoNumeroFormatado));

                    boleto.SetNossoNumeroFormatado(string.Format("{0}{1}", boleto.CedenteBoleto.Convenio, boleto.NossoNumeroFormatado.PadLeft(7, '0')));
                }
                else
                    boleto.SetNossoNumeroFormatado(boleto.NossoNumeroFormatado.PadLeft(11, '0'));
            }
            #endregion Carteira 18-035

            #region Carteira 18-140
            //Carteira 18, com variação 019
            if (boleto.CarteiraCobranca.Codigo.Equals("18-140"))
            {
                /*
                 * Convênio de 7 posições
                 * Nosso Número com 17 posições
                 */
                if (boleto.CedenteBoleto.Convenio.ToString().Length == 7)
                {
                    if (boleto.NossoNumeroFormatado.Length > 10)
                        throw new ValidacaoBoletoException(string.Format("Para a carteira {0}, a quantidade máxima são de 10 de posições para o nosso número", boleto.CarteiraCobranca.Codigo));

                    boleto.SetNossoNumeroFormatado(string.Format("{0}{1}", boleto.CedenteBoleto.Convenio, boleto.NossoNumeroFormatado.PadLeft(10, '0')));
                }
                /*
                 * Convênio de 6 posições
                 * Nosso Número com 11 posições
                 */
                else if (boleto.CedenteBoleto.Convenio.ToString().Length == 6)
                {
                    //Modalidades de Cobrança Sem Registro – Carteira 16 e 18
                    //Nosso Número com 17 posições
                    if (!boleto.TipoModalidade.Equals("21"))
                    {
                        if ((boleto.CedenteBoleto.CodigoCedente.ToString().Length + boleto.NossoNumeroFormatado.Length) > 11)
                            throw new ValidacaoBoletoException(string.Format("Para a carteira {0}, a quantidade máxima são de 11 de posições para o nosso número. Onde o nosso número é formado por CCCCCCNNNNN-X: C -> número do convênio fornecido pelo Banco, N -> seqüencial atribuído pelo cliente e X -> dígito verificador do “Nosso-Número”.", boleto.CarteiraCobranca.Codigo));

                        boleto.SetNossoNumeroFormatado(string.Format("{0}{1}", boleto.CedenteBoleto.Convenio, boleto.NossoNumeroFormatado.PadLeft(5, '0')));
                    }
                    else
                    {
                        if (boleto.CedenteBoleto.Convenio.ToString().Length != 6)
                            throw new ValidacaoBoletoException(string.Format("Para a carteira {0} e o tipo da modalidade 21, o número do convênio são de 6 posições", boleto.CarteiraCobranca.Codigo));

                        boleto.SetNossoNumeroFormatado(boleto.NossoNumeroFormatado.PadLeft(17, '0'));
                    }
                }
                /*
                  * Convênio de 4 posições
                  * Nosso Número com 11 posições
                  */
                else if (boleto.CedenteBoleto.Convenio.ToString().Length == 4)
                {
                    if (boleto.NossoNumeroFormatado.Length > 7)
                        throw new ValidacaoBoletoException(string.Format("Para a carteira {0}, a quantidade máxima são de 7 de posições para o nosso número [{1}]", boleto.CarteiraCobranca.Codigo, boleto.NossoNumeroFormatado));

                    boleto.SetNossoNumeroFormatado(string.Format("{0}{1}", boleto.CedenteBoleto.Convenio, boleto.NossoNumeroFormatado.PadLeft(7, '0')));
                }
                else
                    boleto.SetNossoNumeroFormatado(boleto.NossoNumeroFormatado.PadLeft(11, '0'));
            }
            #endregion Carteira 18-140

            #region Carteira 31
            //Carteira 31
            if (boleto.CarteiraCobranca.Codigo.Equals("31"))
            {
                switch (boleto.CedenteBoleto.Convenio.ToString().Length)
                {
                    //O BB manda como padrão 7 posições, mas é possível solicitar um convênio com 6 posições na carteira 31
                    case 5:
                        if (boleto.NossoNumeroFormatado.Length > 10)
                            throw new ValidacaoBoletoException(string.Format("Para a carteira {0}, a quantidade máxima são de 12 de posições para o nosso número", boleto.CarteiraCobranca));
                        boleto.SetNossoNumeroFormatado(boleto.NossoNumeroFormatado.PadLeft(10, '0'));
                        break;
                    case 6:
                        if (boleto.NossoNumeroFormatado.Length > 10)
                            throw new ValidacaoBoletoException(string.Format("Para a carteira {0}, a quantidade máxima são de 12 de posições para o nosso número", boleto.CarteiraCobranca));
                        boleto.SetNossoNumeroFormatado(boleto.NossoNumeroFormatado.PadLeft(10, '0'));
                        break;
                    case 7:
                        if (boleto.NossoNumeroFormatado.Length > 17)
                            throw new ValidacaoBoletoException(string.Format("Para a carteira {0}, a quantidade máxima são de 10 de posições para o nosso número", boleto.CarteiraCobranca));
                        boleto.SetNossoNumeroFormatado(string.Format("{0}{1}", boleto.CedenteBoleto.Convenio, boleto.NossoNumeroFormatado.PadLeft(10, '0')));
                        break;
                    default:
                        throw new ValidacaoBoletoException(string.Format("Para a carteira {0}, o número do convênio deve ter 6 ou 7 posições", boleto.CarteiraCobranca));
                }
            }
            #endregion Carteira 31

            #region Agência e Conta Corrente
            //Verificar se a Agencia esta correta
            if (boleto.CedenteBoleto.ContaBancariaCedente.Agencia.Length > 4)
                throw new NotImplementedException("A quantidade de dígitos da Agência " + boleto.CedenteBoleto.ContaBancariaCedente.Agencia + ", são de 4 números.");
            else if (boleto.CedenteBoleto.ContaBancariaCedente.Agencia.Length < 4)
                boleto.CedenteBoleto.ContaBancariaCedente.Agencia = boleto.CedenteBoleto.ContaBancariaCedente.Agencia.PadLeft(4, '0');

            //Verificar se a Conta esta correta
            if (boleto.CedenteBoleto.ContaBancariaCedente.Conta.Length > 8)
                throw new NotImplementedException("A quantidade de dígitos da Conta " + boleto.CedenteBoleto.ContaBancariaCedente.Conta + ", são de 8 números.");
            else if (boleto.CedenteBoleto.ContaBancariaCedente.Conta.Length < 8)
                boleto.CedenteBoleto.ContaBancariaCedente.Conta = boleto.CedenteBoleto.ContaBancariaCedente.Conta.PadLeft(8, '0');
            #endregion Agência e Conta Corrente
        }

        public void FormataMoeda(Boleto boleto)
        {
            boleto.Moeda = this.MoedaBanco;

            if (string.IsNullOrEmpty(boleto.Moeda))
                throw new Exception("Espécie/Moeda para o boleto não foi informada.");

            if ((boleto.Moeda == "9") || (boleto.Moeda == "REAL") || (boleto.Moeda == "R$"))
                boleto.Moeda = "R$";
            else
                boleto.Moeda = "0";
        }

        public void FormatarBoleto(Boleto boleto)
        {
            //Atribui o local de pagamento
            boleto.LocalPagamento = this.LocalDePagamento;

            boleto.ValidaDadosEssenciaisDoBoleto();

            FormataNumeroDocumento(boleto);
            FormataNossoNumero(boleto);
            FormataCodigoBarra(boleto);
            FormataLinhaDigitavel(boleto);
            FormataMoeda(boleto);

            ValidaBoletoComNormasBanco(boleto);

            boleto.CedenteBoleto.CodigoCedenteFormatado = String.Format("{0}-{1}/{2}-{3}",
                boleto.CedenteBoleto.ContaBancariaCedente.Agencia,
                boleto.CedenteBoleto.ContaBancariaCedente.DigitoAgencia,
                boleto.CedenteBoleto.ContaBancariaCedente.Conta,
                boleto.CedenteBoleto.ContaBancariaCedente.DigitoConta);
        }

        private string LimparCarteira(string carteira)
        {
            return carteira.Split('-')[0];
        }

        internal static string Mod11BancoBrasil(string value)
        {
            #region Trecho do manual DVMD11.doc
            /* 
            Multiplicar cada algarismo que compõe o número pelo seu respectivo multiplicador (PESO).
            Os multiplicadores(PESOS) variam de 9 a 2.
            O primeiro dígito da direita para a esquerda deverá ser multiplicado por 9, o segundo por 8 e assim sucessivamente.
            O resultados das multiplicações devem ser somados:
            72+35+24+27+4+9+8=179
            O total da soma deverá ser dividido por 11:
            179 / 11=16
            RESTO=3

            Se o resto da divisão for igual a 10 o D.V. será igual a X. 
            Se o resto da divisão for igual a 0 o D.V. será igual a 0.
            Se o resto for menor que 10, o D.V.  será igual ao resto.

            No exemplo acima, o dígito verificador será igual a 3
            */
            #endregion

            /* d - Dígito
             * s - Soma
             * p - Peso
             * b - Base
             * r - Resto
             */

            string d;
            int s = 0, p = 9, b = 2;

            for (int i = value.Length - 1; i >= 0; i--)
            {
                s += (int.Parse(value[i].ToString()) * p);
                if (p == b)
                    p = 9;
                else
                    p--;
            }

            int r = (s % 11);
            if (r == 10)
                d = "X";
            else if (r == 0)
                d = "0";
            else
                d = r.ToString();

            return d;
        }

        public void FormataCodigoBarra(Boleto boleto)
        {
            boleto.Moeda = this.MoedaBanco;
            boleto.SetNossoNumeroFormatado(boleto.NossoNumeroFormatado.Replace("-", ""));

            string valorBoleto = boleto.ValorBoleto.ToString("f").Replace(",", "").Replace(".", "");
            valorBoleto = valorBoleto.PadLeft(10, '0');

            //Criada por AFK

            #region Carteira 11

            if (boleto.CarteiraCobranca.Codigo.Equals("11"))
            {
                if (boleto.CedenteBoleto.Convenio.ToString().Length == 6)
                {
                    if (boleto.TipoModalidade.Equals("21"))
                        boleto.CodigoBarraBoleto = string.Format("{0}{1}{2}{3}{4}{5}{6}",
                            this.CodigoBanco.PadLeft(3, '0'),
                            boleto.Moeda,
                            Common.FatorVencimento(boleto.DataVencimento),
                            valorBoleto,
                            boleto.CedenteBoleto.Convenio,
                            boleto.NossoNumeroFormatado,
                            "21");
                }
                else
                {
                    boleto.CodigoBarraBoleto = string.Format("{0}{1}{2}{3}{4}{5}{6}",
                        CodigoBanco.PadLeft(3, '0'),
                        boleto.Moeda,
                        Common.FatorVencimento(boleto.DataVencimento),
                        valorBoleto,
                        "000000",
                        boleto.NossoNumeroFormatado,
                        boleto.CarteiraCobranca.Codigo);
                }
            }

            #endregion Carteira 11

            #region Carteira 16

            if (boleto.CarteiraCobranca.Codigo.Equals("16"))
            {
                if (boleto.CedenteBoleto.Convenio.ToString().Length == 6)
                {
                    if (boleto.TipoModalidade.Equals("21"))
                        boleto.CodigoBarraBoleto = string.Format("{0}{1}{2}{3}{4}{5}{6}",
                            this.CodigoBanco.PadLeft(3, '0'),
                            boleto.Moeda,
                            Common.FatorVencimento(boleto.DataVencimento),
                            valorBoleto,
                            boleto.CedenteBoleto.Convenio,
                            boleto.NossoNumeroFormatado,
                            "21");
                }
                else
                {
                    boleto.CodigoBarraBoleto = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}",
                        this.CodigoBanco.PadLeft(3, '0'),
                        boleto.Moeda,
                        Common.FatorVencimento(boleto.DataVencimento),
                        valorBoleto,
                        boleto.NossoNumeroFormatado,
                        boleto.CedenteBoleto.ContaBancariaCedente.Agencia,
                        boleto.CedenteBoleto.ContaBancariaCedente.Conta,
                        boleto.CarteiraCobranca.Codigo);
                }
            }

            #endregion Carteira 16

            #region Carteira 17

            if (boleto.CarteiraCobranca.Codigo.Equals("17"))
            {
                if (boleto.CedenteBoleto.Convenio.ToString().Length == 7)
                {
                    boleto.CodigoBarraBoleto = string.Format("{0}{1}{2}{3}{4}{5}{6}",
                        this.CodigoBanco.PadLeft(3, '0'),
                        boleto.Moeda,
                        Common.FatorVencimento(boleto.DataVencimento),
                        valorBoleto,
                        "000000",
                        boleto.NossoNumeroFormatado,
                        LimparCarteira(boleto.CarteiraCobranca.Codigo).PadLeft(2, '0'));
                }
                else if (boleto.CedenteBoleto.Convenio.ToString().Length == 6)
                {
                    boleto.CodigoBarraBoleto = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}",
                        this.CodigoBanco.PadLeft(3, '0'),
                        boleto.Moeda,
                        Common.FatorVencimento(boleto.DataVencimento),
                        valorBoleto,
                        Common.Mid(boleto.NossoNumeroFormatado, 1, 11),
                        boleto.CedenteBoleto.ContaBancariaCedente.Agencia,
                        boleto.CedenteBoleto.ContaBancariaCedente.Conta,
                        boleto.CarteiraCobranca.Codigo);
                }
                else
                {
                    boleto.CodigoBarraBoleto = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}",
                        this.CodigoBanco.PadLeft(3, '0'),
                        boleto.Moeda,
                        Common.FatorVencimento(boleto.DataVencimento),
                        valorBoleto,
                        boleto.NossoNumeroFormatado,
                        boleto.CedenteBoleto.ContaBancariaCedente.Agencia,
                        boleto.CedenteBoleto.ContaBancariaCedente.Conta,
                        boleto.CarteiraCobranca.Codigo);
                }
            }

            #endregion Carteira 17

            #region Carteira 17-019

            if (boleto.CarteiraCobranca.Codigo.Equals("17-019"))
            {
                if (boleto.CedenteBoleto.Convenio.ToString().Length == 7)
                {
                    #region Especificação Convênio 7 posições

                    /*
                    Posição     Tamanho     Picture     Conteúdo
                    01 a 03         03      9(3)            Código do Banco na Câmara de Compensação = ‘001’
                    04 a 04         01      9(1)            Código da Moeda = '9'
                    05 a 05         01      9(1)            DV do Código de Barras (Anexo 10)
                    06 a 09         04      9(04)           Fator de Vencimento (Anexo 8)
                    10 a 19         10      9(08)           V(2) Valor
                    20 a 25         06      9(6)            Zeros
                    26 a 42         17      9(17)           Nosso-Número, sem o DV
                    26 a 32         9       (7)             Número do Convênio fornecido pelo Banco (CCCCCCC)
                    33 a 42         9       (10)            Complemento do Nosso-Número, sem DV (NNNNNNNNNN)
                    43 a 44         02      9(2)            Tipo de Carteira/Modalidade de Cobrança
                     */

                    #endregion Especificação Convênio 7 posições

                    boleto.CodigoBarraBoleto = string.Format("{0}{1}{2}{3}{4}{5}{6}",
                        this.CodigoBanco.PadLeft(3, '0'),
                        boleto.Moeda,
                        Common.FatorVencimento(boleto.DataVencimento),
                        valorBoleto,
                        "000000",
                        boleto.NossoNumeroFormatado,
                        LimparCarteira(boleto.CarteiraCobranca.Codigo).PadLeft(2, '0'));
                }
                else if (boleto.CedenteBoleto.Convenio.ToString().Length == 6)
                {
                    boleto.CodigoBarraBoleto = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}",
                        this.CodigoBanco.PadLeft(3, '0'),
                        boleto.Moeda,
                        Common.FatorVencimento(boleto.DataVencimento),
                        valorBoleto,
                        boleto.NossoNumeroFormatado,
                        boleto.CedenteBoleto.ContaBancariaCedente.Agencia,
                        boleto.CedenteBoleto.ContaBancariaCedente.Conta,
                        LimparCarteira(boleto.CarteiraCobranca.Codigo));
                }
                else if (boleto.CedenteBoleto.Convenio.ToString().Length == 4)
                {
                    boleto.CodigoBarraBoleto = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}",
                        this.CodigoBanco.PadLeft(3, '0'),
                        boleto.Moeda,
                        Common.FatorVencimento(boleto.DataVencimento),
                        valorBoleto,
                        boleto.NossoNumeroFormatado,
                        boleto.CedenteBoleto.ContaBancariaCedente.Agencia,
                        boleto.CedenteBoleto.ContaBancariaCedente.Conta,
                        LimparCarteira(boleto.CarteiraCobranca.Codigo));
                }
            }

            #endregion Carteira 17-019

            #region Carteira 18

            if (boleto.CarteiraCobranca.Codigo.Equals("18"))
            {
                boleto.CodigoBarraBoleto = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}",
                    this.CodigoBanco.PadLeft(3, '0'),
                    boleto.Moeda,
                    Common.FatorVencimento(boleto.DataVencimento),
                    valorBoleto,
                    boleto.NossoNumeroFormatado,
                    boleto.CedenteBoleto.ContaBancariaCedente.Agencia,
                    boleto.CedenteBoleto.ContaBancariaCedente.Conta,
                    boleto.CarteiraCobranca.Codigo);
            }

            #endregion Carteira 18

            #region Carteira 18-019

            if (boleto.CarteiraCobranca.Codigo.Equals("18-019"))
            {
                if (boleto.CedenteBoleto.Convenio.ToString().Length == 7)
                {
                    #region Especificação Convênio 7 posições

                    /*
                    Posição     Tamanho     Picture     Conteúdo
                    01 a 03         03      9(3)            Código do Banco na Câmara de Compensação = ‘001’
                    04 a 04         01      9(1)            Código da Moeda = '9'
                    05 a 05         01      9(1)            DV do Código de Barras (Anexo 10)
                    06 a 09         04      9(04)           Fator de Vencimento (Anexo 8)
                    10 a 19         10      9(08)           V(2) Valor
                    20 a 25         06      9(6)            Zeros
                    26 a 42         17      9(17)           Nosso-Número, sem o DV
                    26 a 32         9       (7)             Número do Convênio fornecido pelo Banco (CCCCCCC)
                    33 a 42         9       (10)            Complemento do Nosso-Número, sem DV (NNNNNNNNNN)
                    43 a 44         02      9(2)            Tipo de Carteira/Modalidade de Cobrança
                     */

                    #endregion Especificação Convênio 7 posições

                    boleto.CodigoBarraBoleto = string.Format("{0}{1}{2}{3}{4}{5}{6}",
                        CodigoBanco.PadLeft(3, '0'),
                        boleto.Moeda,
                        Common.FatorVencimento(boleto.DataVencimento),
                        valorBoleto,
                        "000000",
                        boleto.NossoNumeroFormatado,
                        LimparCarteira(boleto.CarteiraCobranca.Codigo).PadLeft(2, '0'));
                }
                else if (boleto.CedenteBoleto.Convenio.ToString().Length == 6)
                {
                    if (boleto.TipoModalidade.Equals("21"))
                        boleto.CodigoBarraBoleto = string.Format("{0}{1}{2}{3}{4}{5}{6}",
                            this.CodigoBanco.PadLeft(3, '0'),
                            boleto.Moeda,
                            Common.FatorVencimento(boleto.DataVencimento),
                            valorBoleto,
                            boleto.CedenteBoleto.Convenio,
                            boleto.NossoNumeroFormatado,
                            "21");
                    else
                        boleto.CodigoBarraBoleto = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}",
                            this.CodigoBanco.PadLeft(3, '0'),
                            boleto.Moeda,
                            Common.FatorVencimento(boleto.DataVencimento),
                            valorBoleto,
                            boleto.NossoNumeroFormatado,
                            boleto.CedenteBoleto.ContaBancariaCedente.Agencia,
                            boleto.CedenteBoleto.ContaBancariaCedente.Conta,
                            LimparCarteira(boleto.CarteiraCobranca.Codigo));
                }
                else if (boleto.CedenteBoleto.Convenio.ToString().Length == 4)
                {
                    boleto.CodigoBarraBoleto = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}",
                        this.CodigoBanco.PadLeft(3, '0'),
                        boleto.Moeda,
                        Common.FatorVencimento(boleto.DataVencimento),
                        valorBoleto,
                        boleto.NossoNumeroFormatado,
                        boleto.CedenteBoleto.ContaBancariaCedente.Agencia,
                        boleto.CedenteBoleto.ContaBancariaCedente.Conta,
                        LimparCarteira(boleto.CarteiraCobranca.Codigo));
                }
            }

            #endregion Carteira 18-019

            //Para atender o cliente Fiemg foi adptado no código na variação 18-027 as variações 18-035 e 18-140

            #region Carteira 18-027

            if (boleto.CarteiraCobranca.Codigo.Equals("18-027"))
            {
                if (boleto.CedenteBoleto.Convenio.ToString().Length == 7)
                {
                    #region Especificação Convênio 7 posições

                    /*
                    Posição     Tamanho     Picture     Conteúdo
                    01 a 03         03      9(3)            Código do Banco na Câmara de Compensação = ‘001’
                    04 a 04         01      9(1)            Código da Moeda = '9'
                    05 a 05         01      9(1)            DV do Código de Barras (Anexo 10)
                    06 a 09         04      9(04)           Fator de Vencimento (Anexo 8)
                    10 a 19         10      9(08)           V(2) Valor
                    20 a 25         06      9(6)            Zeros
                    26 a 42         17      9(17)           Nosso-Número, sem o DV
                    26 a 32         9       (7)             Número do Convênio fornecido pelo Banco (CCCCCCC)
                    33 a 42         9       (10)            Complemento do Nosso-Número, sem DV (NNNNNNNNNN)
                    43 a 44         02      9(2)            Tipo de Carteira/Modalidade de Cobrança
                     */

                    #endregion Especificação Convênio 7 posições

                    boleto.CodigoBarraBoleto = string.Format("{0}{1}{2}{3}{4}{5}{6}",
                        this.CodigoBanco.PadLeft(3, '0'),
                        boleto.Moeda,
                        Common.FatorVencimento(boleto.DataVencimento).ToString("0000"),
                        valorBoleto,
                        "000000",
                        boleto.NossoNumeroFormatado,
                        LimparCarteira(boleto.CarteiraCobranca.Codigo).PadLeft(2, '0'));
                }
                else if (boleto.CedenteBoleto.Convenio.ToString().Length == 6)
                {
                    if (boleto.TipoModalidade.Equals("21"))
                        boleto.CodigoBarraBoleto = string.Format("{0}{1}{2}{3}{4}{5}{6}",
                            this.CodigoBanco.PadLeft(3, '0'),
                            boleto.Moeda,
                            Common.FatorVencimento(boleto.DataVencimento),
                            valorBoleto,
                            boleto.CedenteBoleto.Convenio,
                            boleto.NossoNumeroFormatado,
                            "21");
                    else
                        boleto.CodigoBarraBoleto = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}",
                            this.CodigoBanco.PadLeft(3, '0'),
                            boleto.Moeda,
                            Common.FatorVencimento(boleto.DataVencimento),
                            valorBoleto,
                            boleto.NossoNumeroFormatado,
                            boleto.CedenteBoleto.ContaBancariaCedente.Agencia,
                            boleto.CedenteBoleto.ContaBancariaCedente.Conta,
                            LimparCarteira(boleto.CarteiraCobranca.Codigo));
                }
                else if (boleto.CedenteBoleto.Convenio.ToString().Length == 4)
                {
                    boleto.CodigoBarraBoleto = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}",
                        this.CodigoBanco.PadLeft(3, '0'),
                        boleto.Moeda,
                        Common.FatorVencimento(boleto.DataVencimento),
                        valorBoleto,
                        boleto.NossoNumeroFormatado,
                        boleto.CedenteBoleto.ContaBancariaCedente.Agencia,
                        boleto.CedenteBoleto.ContaBancariaCedente.Conta,
                        LimparCarteira(boleto.CarteiraCobranca.Codigo));
                }
            }

            #endregion Carteira 18-027

            #region Carteira 18-035

            if (boleto.CarteiraCobranca.Codigo.Equals("18-035"))
            {
                if (boleto.CedenteBoleto.Convenio.ToString().Length == 7)
                {
                    #region Especificação Convênio 7 posições

                    /*
                    Posição     Tamanho     Picture     Conteúdo
                    01 a 03         03      9(3)            Código do Banco na Câmara de Compensação = ‘001’
                    04 a 04         01      9(1)            Código da Moeda = '9'
                    05 a 05         01      9(1)            DV do Código de Barras (Anexo 10)
                    06 a 09         04      9(04)           Fator de Vencimento (Anexo 8)
                    10 a 19         10      9(08)           V(2) Valor
                    20 a 25         06      9(6)            Zeros
                    26 a 42         17      9(17)           Nosso-Número, sem o DV
                    26 a 32         9       (7)             Número do Convênio fornecido pelo Banco (CCCCCCC)
                    33 a 42         9       (10)            Complemento do Nosso-Número, sem DV (NNNNNNNNNN)
                    43 a 44         02      9(2)            Tipo de Carteira/Modalidade de Cobrança
                     */

                    #endregion Especificação Convênio 7 posições

                    boleto.CodigoBarraBoleto = string.Format("{0}{1}{2}{3}{4}{5}{6}",
                        this.CodigoBanco.PadLeft(3, '0'),
                        boleto.Moeda,
                        Common.FatorVencimento(boleto.DataVencimento).ToString("0000"),
                        valorBoleto,
                        "000000",
                        boleto.NossoNumeroFormatado,
                        LimparCarteira(boleto.CarteiraCobranca.Codigo).PadLeft(2, '0'));
                }
                else if (boleto.CedenteBoleto.Convenio.ToString().Length == 6)
                {
                    if (boleto.TipoModalidade.Equals("21"))
                        boleto.CodigoBarraBoleto = string.Format("{0}{1}{2}{3}{4}{5}{6}",
                            this.CodigoBanco.PadLeft(3, '0'),
                            boleto.Moeda,
                            Common.FatorVencimento(boleto.DataVencimento),
                            valorBoleto,
                            boleto.CedenteBoleto.Convenio,
                            boleto.NossoNumeroFormatado,
                            "21");
                    else
                        boleto.CodigoBarraBoleto = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}",
                            this.CodigoBanco.PadLeft(3, '0'),
                            boleto.Moeda,
                            Common.FatorVencimento(boleto.DataVencimento),
                            valorBoleto,
                            boleto.NossoNumeroFormatado,
                            boleto.CedenteBoleto.ContaBancariaCedente.Agencia,
                            boleto.CedenteBoleto.ContaBancariaCedente.Conta,
                            LimparCarteira(boleto.CarteiraCobranca.Codigo));
                }
                else if (boleto.CedenteBoleto.Convenio.ToString().Length == 4)
                {
                    boleto.CodigoBarraBoleto = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}",
                        this.CodigoBanco.PadLeft(3, '0'),
                        boleto.Moeda,
                        Common.FatorVencimento(boleto.DataVencimento),
                        valorBoleto,
                        boleto.NossoNumeroFormatado,
                        boleto.CedenteBoleto.ContaBancariaCedente.Agencia,
                        boleto.CedenteBoleto.ContaBancariaCedente.Conta,
                        LimparCarteira(boleto.CarteiraCobranca.Codigo));
                }
            }

            #endregion Carteira 18-035

            #region Carteira 18-140

            if (boleto.CarteiraCobranca.Codigo.Equals("18-140"))
            {
                if (boleto.CedenteBoleto.Convenio.ToString().Length == 7)
                {
                    #region Especificação Convênio 7 posições

                    /*
                    Posição     Tamanho     Picture     Conteúdo
                    01 a 03         03      9(3)            Código do Banco na Câmara de Compensação = ‘001’
                    04 a 04         01      9(1)            Código da Moeda = '9'
                    05 a 05         01      9(1)            DV do Código de Barras (Anexo 10)
                    06 a 09         04      9(04)           Fator de Vencimento (Anexo 8)
                    10 a 19         10      9(08)           V(2) Valor
                    20 a 25         06      9(6)            Zeros
                    26 a 42         17      9(17)           Nosso-Número, sem o DV
                    26 a 32         9       (7)             Número do Convênio fornecido pelo Banco (CCCCCCC)
                    33 a 42         9       (10)            Complemento do Nosso-Número, sem DV (NNNNNNNNNN)
                    43 a 44         02      9(2)            Tipo de Carteira/Modalidade de Cobrança
                     */

                    #endregion Especificação Convênio 7 posições

                    boleto.CodigoBarraBoleto = string.Format("{0}{1}{2}{3}{4}{5}{6}",
                        this.CodigoBanco.PadLeft(3, '0'),
                        boleto.Moeda,
                        Common.FatorVencimento(boleto.DataVencimento).ToString("0000"),
                        valorBoleto,
                        "000000",
                        boleto.NossoNumeroFormatado,
                        LimparCarteira(boleto.CarteiraCobranca.Codigo).PadLeft(2, '0'));
                }
                else if (boleto.CedenteBoleto.Convenio.ToString().Length == 6)
                {
                    if (boleto.TipoModalidade.Equals("21"))
                        boleto.CodigoBarraBoleto = string.Format("{0}{1}{2}{3}{4}{5}{6}",
                            this.CodigoBanco.PadLeft(3, '0'),
                            boleto.Moeda,
                            Common.FatorVencimento(boleto.DataVencimento),
                            valorBoleto,
                            boleto.CedenteBoleto.Convenio,
                            boleto.NossoNumeroFormatado,
                            "21");
                    else
                        boleto.CodigoBarraBoleto = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}",
                            this.CodigoBanco.PadLeft(3, '0'),
                            boleto.Moeda,
                            Common.FatorVencimento(boleto.DataVencimento),
                            valorBoleto,
                            boleto.NossoNumeroFormatado,
                            boleto.CedenteBoleto.ContaBancariaCedente.Agencia,
                            boleto.CedenteBoleto.ContaBancariaCedente.Conta,
                            LimparCarteira(boleto.CarteiraCobranca.Codigo));
                }
                else if (boleto.CedenteBoleto.Convenio.ToString().Length == 4)
                {
                    boleto.CodigoBarraBoleto = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}",
                        this.CodigoBanco.PadLeft(3, '0'),
                        boleto.Moeda,
                        Common.FatorVencimento(boleto.DataVencimento),
                        valorBoleto,
                        boleto.NossoNumeroFormatado,
                        boleto.CedenteBoleto.ContaBancariaCedente.Agencia,
                        boleto.CedenteBoleto.ContaBancariaCedente.Conta,
                        LimparCarteira(boleto.CarteiraCobranca.Codigo));
                }
            }

            #endregion Carteira 18-140

            #region Carteira 31

            if (boleto.CarteiraCobranca.Codigo.Equals("31"))
            {
                boleto.CodigoBarraBoleto = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}",
                    this.CodigoBanco.PadLeft(3, '0'),
                    boleto.Moeda,
                    Common.FatorVencimento(boleto.DataVencimento),
                    valorBoleto,
                    boleto.NossoNumeroFormatado,
                    boleto.CedenteBoleto.ContaBancariaCedente.Agencia,
                    boleto.CedenteBoleto.ContaBancariaCedente.Conta,
                    boleto.CarteiraCobranca.Codigo);
            }

            #endregion Carteira 31

            _digitoAutoConferenciaBoleto = Common.Mod11(boleto.CodigoBarraBoleto, 9);

            boleto.CodigoBarraBoleto = Common.Left(boleto.CodigoBarraBoleto, 4) + _digitoAutoConferenciaBoleto +
                                       Common.Right(boleto.CodigoBarraBoleto, 39);
        }

        public void FormataLinhaDigitavel(Boleto boleto)
        {
            boleto.Moeda = MoedaBanco;

            string campo1 = string.Empty;
            string campo2 = string.Empty;
            string campo3 = string.Empty;
            string campo4 = string.Empty;
            string campo5 = string.Empty;
            long icampo5 = 0;
            int digitoMod = 0;

            /*
            Campos 1 (AAABC.CCCCX):
            A = Código do Banco na Câmara de Compensação “001”
            B = Código da moeda "9" (*)
            C = Posição 20 a 24 do código de barras
            X = DV que amarra o campo 1 (Módulo 10, contido no Anexo 7)
             */
            campo1 = CodigoBanco.PadLeft(3, '0') + boleto.Moeda + Common.Mid(boleto.CodigoBarraBoleto, 20, 5);
            digitoMod = Common.Mod10(campo1);
            campo1 = campo1 + digitoMod;
            campo1 = Common.Mid(campo1, 1, 5) + "." + Common.Mid(campo1, 6, 5);

            /*
            Campo 2 (DDDDD.DDDDDY)
            D = Posição 25 a 34 do código de barras
            Y = DV que amarra o campo 2 (Módulo 10, contido no Anexo 7)
             */
            campo2 = Common.Mid(boleto.CodigoBarraBoleto, 25, 10);
            digitoMod = Common.Mod10(campo2);
            campo2 = campo2 + digitoMod;
            campo2 = Common.Mid(campo2, 1, 5) + "." + Common.Mid(campo2, 6, 6);

            /*
            Campo 3 (EEEEE.EEEEEZ)
            E = Posição 35 a 44 do código de barras
            Z = DV que amarra o campo 3 (Módulo 10, contido no Anexo 7)
             */
            campo3 = Common.Mid(boleto.CodigoBarraBoleto, 35, 10);
            digitoMod = Common.Mod10(campo3);
            campo3 = campo3 + digitoMod;
            campo3 = Common.Mid(campo3, 1, 5) + "." + Common.Mid(campo3, 6, 6);

            /*
            Campo 4 (K)
            K = DV do Código de Barras (Módulo 11, contido no Anexo 10)
             */
            campo4 = Common.Mid(boleto.CodigoBarraBoleto, 5, 1);

            /*
            Campo 5 (UUUUVVVVVVVVVV)
            U = Fator de Vencimento ( Anexo 10)
            V = Valor do Título (*)
             */
            icampo5 = Convert.ToInt64(Common.Mid(boleto.CodigoBarraBoleto, 6, 14));

            if (icampo5 == 0)
                campo5 = "000";
            else
                campo5 = icampo5.ToString();

            boleto.LinhaDigitavelBoleto = campo1 + " " + campo2 + " " + campo3 + " " + campo4 + " " + campo5;
        }

        public void FormataNossoNumero(Boleto boleto)
        {
            if (boleto.CedenteBoleto.Convenio.ToString().Length == 4)
            {
                boleto.SetNossoNumeroFormatado(string.Format("{0}{1}-{2}", boleto.CedenteBoleto.Convenio,
                    boleto.IdentificadorInternoBoleto.PadLeft(7, '0'),
                    Mod11BancoBrasil(boleto.IdentificadorInternoBoleto)));
                return;
            }

            //somente monta o digito verificador no nosso numero se o convenio tiver 6 posições
            if (boleto.CedenteBoleto.Convenio.ToString().Length == 6)
            {
                boleto.SetNossoNumeroFormatado(string.Format("{0}{1}-{2}", boleto.CedenteBoleto.Convenio,
                    boleto.IdentificadorInternoBoleto.PadLeft(7, '0'),
                    Mod11BancoBrasil(boleto.IdentificadorInternoBoleto)));
                return;
                //switch (boleto.CarteiraCobranca.Codigo)
                //{
                //    case "18-019":
                //        boleto.SetNossoNumeroFormatado(string.Format("{0}/{1}-{2}", LimparCarteira(boleto.CarteiraCobranca.Codigo), boleto.SequencialNossoNumero, Mod11BancoBrasil(boleto.SequencialNossoNumero)));
                //        return;
                //}
            }

            if (boleto.CedenteBoleto.Convenio.ToString().Length == 7)
            {
                boleto.SetNossoNumeroFormatado(string.Format("{0}{1}", boleto.CedenteBoleto.Convenio,
                    boleto.IdentificadorInternoBoleto.PadLeft(10, '0')));
                return;
            }

            switch (boleto.CarteiraCobranca.Codigo)
            {
                case "17-019":
                case "18-019":
                    boleto.SetNossoNumeroFormatado(string.Format("{0}/{1}",
                        LimparCarteira(boleto.CarteiraCobranca.Codigo), boleto.IdentificadorInternoBoleto));
                    return;
                case "31":
                    boleto.SetNossoNumeroFormatado(string.Format("{0}{1}", boleto.CedenteBoleto.Convenio.PadLeft(7, '0'),
                        boleto.IdentificadorInternoBoleto));
                    return;
            }

            throw new Exception("Não foi possível formatar o nosso número para o boleto " + boleto.NumeroDocumento);
        }

        public void FormataNumeroDocumento(Boleto boleto)
        {
            if (String.IsNullOrEmpty(boleto.NumeroDocumento) || String.IsNullOrEmpty(boleto.NumeroDocumento.TrimStart('0')))
                throw new Exception("Número do Documento não foi informado.");

            boleto.NumeroDocumento = boleto.NumeroDocumento.PadLeft(10, '0');
        }

        public ICodigoOcorrencia ObtemCodigoOcorrenciaByInt(int numeroOcorrencia)
        {
            switch (numeroOcorrencia)
            {
                case 02:
                    {
                        return new CodigoOcorrencia(numeroOcorrencia)
                        {
                            Codigo = 02,
                            Descricao = "Confirmação de Entrada de Título".ToUpper()
                        };
                    }
                case 03:
                    {
                        return new CodigoOcorrencia(numeroOcorrencia)
                        {
                            Codigo = 03,
                            Descricao = "ENTRADA REJEITADA".ToUpper()
                        };
                    }
                case 05:
                    {
                        return new CodigoOcorrencia(numeroOcorrencia)
                        {
                            Codigo = 05,
                            Descricao = "Liquidado sem registro (carteira 17-tipo4)".ToUpper()
                        };
                    }
                case 06:
                    {
                        return new CodigoOcorrencia(numeroOcorrencia)
                        {
                            Codigo = 06,
                            Descricao = "Liquidação Normal".ToUpper()
                        };
                    }
                case 07:
                    {
                        return new CodigoOcorrencia(numeroOcorrencia)
                        {
                            Codigo = 07,
                            Descricao = "Liquidação por Conta/Parcial".ToUpper()
                        };
                    }
                case 08:
                    {
                        return new CodigoOcorrencia(numeroOcorrencia)
                        {
                            Codigo = 08,
                            Descricao = "Liquidação por Saldo".ToUpper()
                        };
                    }
                case 09:
                    {
                        return new CodigoOcorrencia(numeroOcorrencia)
                        {
                            Codigo = 09,
                            Descricao = "Baixa de Titulo".ToUpper()
                        };
                    }
                case 10:
                    {
                        return new CodigoOcorrencia(numeroOcorrencia)
                        {
                            Codigo = 10,
                            Descricao = "Baixa Solicitada".ToUpper()
                        };
                    }
                case 11:
                    {
                        return new CodigoOcorrencia(numeroOcorrencia)
                        {
                            Codigo = 11,
                            Descricao =
                                "Títulos em Ser (constara somente do arquivo de existência de cobrança, fornecido mediante solicitação do cliente)"
                                    .ToUpper()
                        };
                    }
                case 12:
                    {
                        return new CodigoOcorrencia(numeroOcorrencia)
                        {
                            Codigo = 12,
                            Descricao = "Abatimento Concedido".ToUpper()
                        };
                    }
                case 13:
                    {
                        return new CodigoOcorrencia(numeroOcorrencia)
                        {
                            Codigo = 13,
                            Descricao = "Abatimento Cancelado".ToUpper()
                        };
                    }
                case 14:
                    {
                        return new CodigoOcorrencia(numeroOcorrencia)
                        {
                            Codigo = 14,
                            Descricao = "Alteração de Vencimento do título".ToUpper()
                        };
                    }
                case 15:
                    {
                        return new CodigoOcorrencia(numeroOcorrencia)
                        {
                            Codigo = 15,
                            Descricao = "Liquidação em Cartório".ToUpper()
                        };
                    }
                case 16:
                    {
                        return new CodigoOcorrencia(numeroOcorrencia)
                        {
                            Codigo = 16,
                            Descricao = "Confirmação de alteração de juros de mora".ToUpper()
                        };
                    }
                case 19:
                    {
                        return new CodigoOcorrencia(numeroOcorrencia)
                        {
                            Codigo = 19,
                            Descricao = "Confirmação de recebimento de instruções para protesto".ToUpper()
                        };
                    }
                case 20:
                    {
                        return new CodigoOcorrencia(numeroOcorrencia)
                        {
                            Codigo = 20,
                            Descricao = "Débito em Conta".ToUpper()
                        };
                    }
                case 21:
                    {
                        return new CodigoOcorrencia(numeroOcorrencia)
                        {
                            Codigo = 21,
                            Descricao = "Alteração do Nome do Sacado".ToUpper()
                        };
                    }
                case 22:
                    {
                        return new CodigoOcorrencia(numeroOcorrencia)
                        {
                            Codigo = 22,
                            Descricao = "Alteração do Endereço do Sacado".ToUpper()
                        };
                    }
                case 23:
                    {
                        return new CodigoOcorrencia(numeroOcorrencia)
                        {
                            Codigo = 23,
                            Descricao = "Indicação de encaminhamento a cartório".ToUpper()
                        };
                    }
                case 24:
                    {
                        return new CodigoOcorrencia(numeroOcorrencia)
                        {
                            Codigo = 24,
                            Descricao = "Sustar Protesto".ToUpper()
                        };
                    }
                case 25:
                    {
                        return new CodigoOcorrencia(numeroOcorrencia)
                        {
                            Codigo = 25,
                            Descricao = "Dispensar Juros de mora".ToUpper()
                        };
                    }
                case 26:
                    {
                        return new CodigoOcorrencia(numeroOcorrencia)
                        {
                            Codigo = 26,
                            Descricao =
                                "Alteração do número do título dado pelo Cedente (Seu número) – 10 e 15 posições".ToUpper()
                        };
                    }
                case 28:
                    {
                        return new CodigoOcorrencia(numeroOcorrencia)
                        {
                            Codigo = 28,
                            Descricao = "Manutenção de titulo vencido".ToUpper()
                        };
                    }
                case 31:
                    {
                        return new CodigoOcorrencia(numeroOcorrencia)
                        {
                            Codigo = 31,
                            Descricao = "Conceder desconto".ToUpper()
                        };
                    }
                case 32:
                    {
                        return new CodigoOcorrencia(numeroOcorrencia)
                        {
                            Codigo = 32,
                            Descricao = "Não conceder desconto".ToUpper()
                        };
                    }
                case 33:
                    {
                        return new CodigoOcorrencia(numeroOcorrencia)
                        {
                            Codigo = 33,
                            Descricao = "Retificar desconto".ToUpper()
                        };
                    }
                case 34:
                    {
                        return new CodigoOcorrencia(numeroOcorrencia)
                        {
                            Codigo = 34,
                            Descricao = "Alterar data para desconto".ToUpper()
                        };
                    }
                case 35:
                    {
                        return new CodigoOcorrencia(numeroOcorrencia)
                        {
                            Codigo = 35,
                            Descricao = "Cobrar Multa".ToUpper()
                        };
                    }
                case 36:
                    {
                        return new CodigoOcorrencia(numeroOcorrencia)
                        {
                            Codigo = 36,
                            Descricao = "Dispensar Multa".ToUpper()
                        };
                    }
                case 37:
                    {
                        return new CodigoOcorrencia(numeroOcorrencia)
                        {
                            Codigo = 37,
                            Descricao = "Dispensar Indexador".ToUpper()
                        };
                    }
                case 38:
                    {
                        return new CodigoOcorrencia(numeroOcorrencia)
                        {
                            Codigo = 38,
                            Descricao = "Dispensar prazo limite para recebimento".ToUpper()
                        };
                    }
                case 39:
                    {
                        return new CodigoOcorrencia(numeroOcorrencia)
                        {
                            Codigo = 39,
                            Descricao = "Alterar prazo limite para recebimento".ToUpper()
                        };
                    }
                case 41:
                    {
                        return new CodigoOcorrencia(numeroOcorrencia)
                        {
                            Codigo = 41,
                            Descricao = "Alteração do número do controle do participante (25 posições)".ToUpper()
                        };
                    }
                case 42:
                    {
                        return new CodigoOcorrencia(numeroOcorrencia)
                        {
                            Codigo = 42,
                            Descricao = "Alteração do número do documento do sacado (CNPJ/CPF)".ToUpper()
                        };
                    }
                case 44:
                    {
                        return new CodigoOcorrencia(numeroOcorrencia)
                        {
                            Codigo = 44,
                            Descricao = "Título pago com cheque devolvido".ToUpper()
                        };
                    }
                case 46:
                    {
                        return new CodigoOcorrencia(numeroOcorrencia)
                        {
                            Codigo = 46,
                            Descricao = "Título pago com cheque, aguardando compensação".ToUpper()
                        };
                    }
                case 72:
                    {
                        return new CodigoOcorrencia(numeroOcorrencia)
                        {
                            Codigo = 72,
                            Descricao =
                                "Alteração de tipo de cobrança (específico para títulos das carteiras 11 e 17)".ToUpper()
                        };
                    }
                case 73:
                    {
                        return new CodigoOcorrencia(numeroOcorrencia)
                        {
                            Codigo = 73,
                            Descricao = "Confirmação de Instrução de Parâmetro de Pagamento Parcial".ToUpper()
                        };
                    }
                case 96:
                    {
                        return new CodigoOcorrencia(numeroOcorrencia)
                        {
                            Codigo = 96,
                            Descricao = "Despesas de Protesto".ToUpper()
                        };
                    }
                case 97:
                    {
                        return new CodigoOcorrencia(numeroOcorrencia)
                        {
                            Codigo = 97,
                            Descricao = "Despesas de Sustação de Protesto".ToUpper()
                        };
                    }
                case 98:
                    {
                        return new CodigoOcorrencia(numeroOcorrencia)
                        {
                            Codigo = 98,
                            Descricao = "Débito de Custas Antecipadas".ToUpper()
                        };
                    }
            }
            throw new Exception(
                String.Format(
                    "Não foi possível obter Código de Comando/Movimento/Ocorrência. Banco: {0} Código: {1}",
                    CodigoBanco, numeroOcorrencia.ToString()));
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
            }
            throw new Exception(
                String.Format(
                    "Não foi possível obter Código de Comando/Movimento/Ocorrência. Banco: {0} Código: {1}",
                    CodigoBanco, numeroOcorrencia.ToString()));
        }

        public ICodigoOcorrencia ObtemCodigoOcorrencia(EnumCodigoOcorrenciaRemessa comando, double valorOcorrencia,
            DateTime dataOcorrencia)
        {
            switch (comando)
            {
                case EnumCodigoOcorrenciaRemessa.Registro:
                    {
                        return new CodigoOcorrencia((int)comando)
                        {
                            Codigo = 01,
                            Descricao = "Registro de títulos"
                        };
                    }
                case EnumCodigoOcorrenciaRemessa.Baixa:
                    {
                        return new CodigoOcorrencia((int)comando)
                        {
                            Codigo = 02,
                            Descricao = "Solicitação de baixa"
                        };
                    }
                case EnumCodigoOcorrenciaRemessa.PedidoDeDebitoEmConta:
                    {
                        return new CodigoOcorrencia((int)comando)
                        {
                            Codigo = 03,
                            Descricao = "Pedido de débito em conta"
                        };
                    }
                case EnumCodigoOcorrenciaRemessa.ConcessaoDeAbatimento:
                    {
                        return new CodigoOcorrencia((int)comando)
                        {
                            Codigo = 04,
                            Descricao = "Concessão de abatimento"
                        };
                    }
                case EnumCodigoOcorrenciaRemessa.CancelamentoDeAbatimento:
                    {
                        return new CodigoOcorrencia((int)comando)
                        {
                            Codigo = 05,
                            Descricao = "Cancelamento de abatimento"
                        };
                    }
                case EnumCodigoOcorrenciaRemessa.AlteracaoDeVencimento:
                    {
                        return new CodigoOcorrencia((int)comando)
                        {
                            Codigo = 06,
                            Descricao = "Alteração de vencimento de título"
                        };
                    }
                case EnumCodigoOcorrenciaRemessa.AlteracaoDoControleDoParticipante:
                    {
                        return new CodigoOcorrencia((int)comando)
                        {
                            Codigo = 07,
                            Descricao = "Alteração do número de controle do participante"
                        };
                    }
                case EnumCodigoOcorrenciaRemessa.AlteracaoSeuNumero:
                    {
                        return new CodigoOcorrencia((int)comando)
                        {
                            Codigo = 08,
                            Descricao = "Alteração do número do título dado pelo cedente"
                        };
                    }
                case EnumCodigoOcorrenciaRemessa.Protesto:
                    {
                        return new CodigoOcorrencia((int)comando)
                        {
                            Codigo = 09,
                            Descricao = "Protestar"
                        };
                    }
                case EnumCodigoOcorrenciaRemessa.SustarProtesto:
                    {
                        return new CodigoOcorrencia((int)comando)
                        {
                            Codigo = 10,
                            Descricao = "Sustar protesto"
                        };
                    }
                case EnumCodigoOcorrenciaRemessa.DispensarCobrancaDeJurosDeMora:
                    {
                        return new CodigoOcorrencia((int)comando)
                        {
                            Codigo = 11,
                            Descricao = "Dispensar juros"
                        };
                    }
                case EnumCodigoOcorrenciaRemessa.AlteracaoNomeEEnderecoSacado:
                    {
                        return new CodigoOcorrencia((int)comando)
                        {
                            Codigo = 12,
                            Descricao = "Alteração de nome e endereço do sacado"
                        };
                    }
                case EnumCodigoOcorrenciaRemessa.AlteracaoDeJurosDeMora:
                    {
                        return new CodigoOcorrencia((int)comando)
                        {
                            Codigo = 16,
                            Descricao = "Alteração de juros de mora"
                        };
                    }
                case EnumCodigoOcorrenciaRemessa.ConcessaoDeDesconto:
                    {
                        return new CodigoOcorrencia((int)comando)
                        {
                            Codigo = 31,
                            Descricao = "Conceder desconto"
                        };
                    }
                case EnumCodigoOcorrenciaRemessa.NaoConcederDesconto:
                    {
                        return new CodigoOcorrencia((int)comando)
                        {
                            Codigo = 32,
                            Descricao = "Não conceder desconto"
                        };
                    }
                case EnumCodigoOcorrenciaRemessa.AlteracaoDoValorDeDesconto:
                    {
                        return new CodigoOcorrencia((int)comando)
                        {
                            Codigo = 33,
                            Descricao = "Retificar dados da concessão de desconto"
                        };
                    }
                case EnumCodigoOcorrenciaRemessa.AlteracaoDeDataConcessaoDesconto:
                    {
                        return new CodigoOcorrencia((int)comando)
                        {
                            Codigo = 34,
                            Descricao = "Alterar data para concessão de desconto"
                        };
                    }
                case EnumCodigoOcorrenciaRemessa.CobrarMulta:
                    {
                        return new CodigoOcorrencia((int)comando)
                        {
                            Codigo = 35,
                            Descricao = "Cobrar multa"
                        };
                    }
                case EnumCodigoOcorrenciaRemessa.DispensarMulta:
                    {
                        return new CodigoOcorrencia((int)comando)
                        {
                            Codigo = 36,
                            Descricao = "Dispensar multa"
                        };
                    }
                case EnumCodigoOcorrenciaRemessa.DispensarIndexador:
                    {
                        return new CodigoOcorrencia((int)comando)
                        {
                            Codigo = 37,
                            Descricao = "Dispensar indexador"
                        };
                    }
                case EnumCodigoOcorrenciaRemessa.DispensarPrazoLimiteRecebimento:
                    {
                        return new CodigoOcorrencia((int)comando)
                        {
                            Codigo = 38,
                            Descricao = "Dispensar prazo limite para recebimento"
                        };
                    }
                case EnumCodigoOcorrenciaRemessa.AlteracaoPrazoLimiteRecebimento:
                    {
                        return new CodigoOcorrencia((int)comando)
                        {
                            Codigo = 39,
                            Descricao = "Alterar prazo limite de recebimento"
                        };
                    }
                case EnumCodigoOcorrenciaRemessa.AlteracaoModalidade:
                    {
                        return new CodigoOcorrencia((int)comando)
                        {
                            Codigo = 40,
                            Descricao = "Alterar modalidade"
                        };
                    }
            }
            throw new Exception(
                String.Format(
                    "Não foi possível obter Código de Comando/Movimento/Ocorrência. Banco: {0} Código: {1}",
                    CodigoBanco, comando.ToString()));
        }

        public ICodigoOcorrencia ObtemCodigoOcorrencia(EnumCodigoOcorrenciaRetorno ocorrenciaRetorno)
        {
            throw new NotImplementedException();
        }

        public IEspecieDocumento ObtemEspecieDocumento(EnumEspecieDocumento especie)
        {
            #region ESPÉCIE DE TÍTULO

            /*
             * 01 - Duplicata Mercantil
             * 02 - Nota Promissória
             * 03 - Nota de Seguro
             * 05 - Recibo
             * 08 - Letra de Câmbio
             * 09 - Warrant
             * 10 - Cheque
             * 12 - Duplicata de Serviço
             * 13 - Nota de Débito
             * 15 - Apólice de Seguro
             * 25 - Dívida Ativa da União
             * 26 - Dívida Ativa de Estado
             * 27 - Dívida Ativa de Município
             */

            /* Observações:
             * As espécies 25 - DAU, 26 - DAE, 27 - DAM, somente são admissíveis nas Carteiras 11 e 17, como Cobrança Simples.
             * Na modalidade de Cobrança Descontada somente são permitidas as espécies: 01 - Duplicata Mercantil (DM), 12 - Duplicata de Prestação de Serviço (DS) e 08 - Letra de Câmbio (LC).
             * Para a modalidade Vendor somente são permitidas as espécies: 01 - Duplicata Mercantil (DM) e 12 - Duplicata de Prestação de Serviço (DS).
             */

            #endregion

            switch (especie)
            {
                case EnumEspecieDocumento.DuplicataMercantil:
                    {
                        return new EspecieDocumento((int)especie)
                        {
                            Codigo = 01,
                            Descricao = "Duplicata Mercantil",
                            Sigla = "DM"
                        };
                    }
                case EnumEspecieDocumento.NotaPromissoria:
                    {
                        return new EspecieDocumento((int)especie)
                        {
                            Codigo = 02,
                            Descricao = "Nota Promissória",
                            Sigla = "NP"
                        };
                    }
                case EnumEspecieDocumento.NotaSeguro:
                    {
                        return new EspecieDocumento((int)especie)
                        {
                            Codigo = 03,
                            Descricao = "Nota de Seguro",
                            Sigla = "NS"
                        };
                    }
                case EnumEspecieDocumento.Recibo:
                    {
                        return new EspecieDocumento((int)especie)
                        {
                            Codigo = 05,
                            Descricao = "Recibo",
                            Sigla = "RC"
                        };
                    }
                case EnumEspecieDocumento.LetraCambio:
                    {
                        return new EspecieDocumento((int)especie)
                        {
                            Codigo = 08,
                            Descricao = "Letra de Câmbio",
                            Sigla = "LC"
                        };
                    }
                case EnumEspecieDocumento.Warrant:
                    {
                        return new EspecieDocumento((int)especie)
                        {
                            Codigo = 09,
                            Descricao = "Warrant",
                            Sigla = "WR"
                        };
                    }
                case EnumEspecieDocumento.Cheque:
                    {
                        return new EspecieDocumento((int)especie)
                        {
                            Codigo = 10,
                            Descricao = "Cheque",
                            Sigla = "CH"
                        };
                    }
                case EnumEspecieDocumento.DuplicataServico:
                    {
                        return new EspecieDocumento((int)especie)
                        {
                            Codigo = 12,
                            Descricao = "Duplicata de Serviço",
                            Sigla = "DS"
                        };
                    }
                case EnumEspecieDocumento.NotaDebito:
                    {
                        return new EspecieDocumento((int)especie)
                        {
                            Codigo = 13,
                            Descricao = "Nota de Débitos",
                            Sigla = "ND"
                        };
                    }
                case EnumEspecieDocumento.ApoliceSeguro:
                    {
                        return new EspecieDocumento((int)especie)
                        {
                            Codigo = 15,
                            Descricao = "Apólice de Seguro",
                            Sigla = "AS"
                        };
                    }
                case EnumEspecieDocumento.DAU:
                    {
                        return new EspecieDocumento((int)especie)
                        {
                            Codigo = 25,
                            Descricao = "Dívida Ativa da União",
                            Sigla = "DAU"
                        };
                    }
                case EnumEspecieDocumento.DAE:
                    {
                        return new EspecieDocumento((int)especie)
                        {
                            Codigo = 26,
                            Descricao = "Dívida ativa de Estado",
                            Sigla = "DAE"
                        };
                    }
                case EnumEspecieDocumento.DAM:
                    {
                        return new EspecieDocumento((int)especie)
                        {
                            Codigo = 27,
                            Descricao = "Dívida Ativa de Município",
                            Sigla = "DAM"
                        };
                    }
            }
            throw new Exception(
                String.Format("Não foi possível obter espécie. Banco: {0} Código Espécie: {1}",
                    CodigoBanco, especie.ToString()));
        }

        public IInstrucao ObtemInstrucaoPadronizada(EnumTipoInstrucao tipoInstrucao, double valorInstrucao, DateTime dataInstrucao,
            int diasInstrucao)
        {
            switch (tipoInstrucao)
            {
                #region Instruções para COMANDO 01 - REGISTRO DE TÍTULO

                // Para Comando 01 - Registro de Título (posição 109-110)

                /* Instrução 00
                 * Se o COMANDO = 09 - Protestar, o Sistema de Cobrança do Banco assumirá o prazo de protesto de 5 (cinco) dias úteis
                 */
                case EnumTipoInstrucao.SemInstrucoes:
                    {
                        return new InstrucaoPadronizada()
                        {
                            Codigo = 00,
                            QtdDias = diasInstrucao,
                            Valor = valorInstrucao,
                            TextoInstrucao = string.Empty
                        };
                    }
                case EnumTipoInstrucao.JurosdeMora:
                    {
                        return new InstrucaoPadronizada()
                        {
                            Codigo = 01,
                            QtdDias = diasInstrucao,
                            Valor = valorInstrucao,
                            TextoInstrucao = "Cobrar juros"
                        };
                    }
                case EnumTipoInstrucao.ProtestarApos3DiasUteis:
                    {
                        return new InstrucaoPadronizada()
                        {
                            Codigo = 03,
                            QtdDias = diasInstrucao,
                            Valor = valorInstrucao,
                            TextoInstrucao = "Protestar no 3º dia útil após o vencido."
                        };
                    }
                case EnumTipoInstrucao.ProtestarApos4DiasUteis:
                    {
                        return new InstrucaoPadronizada()
                        {
                            Codigo = 04,
                            QtdDias = diasInstrucao,
                            Valor = valorInstrucao,
                            TextoInstrucao = "Protestar no 4º dia útil após o vencido."
                        };
                    }
                case EnumTipoInstrucao.ProtestarApos5DiasUteis:
                    {
                        return new InstrucaoPadronizada()
                        {
                            Codigo = 05,
                            QtdDias = diasInstrucao,
                            Valor = valorInstrucao,
                            TextoInstrucao = "Protestar no 5º dia útil após o vencido."
                        };
                    }
                // Indica protesto com prazo de 6 a 29, 35 ou 40 dias corridos.
                case EnumTipoInstrucao.ProtestarAposNDiasCorridos:
                    {
                        return new InstrucaoPadronizada()
                        {
                            Codigo = 06,
                            QtdDias = diasInstrucao,
                            Valor = valorInstrucao,
                            TextoInstrucao = "Protestar após " + diasInstrucao + " dias."
                        };
                    }
                case EnumTipoInstrucao.NaoProtestar:
                    {
                        return new InstrucaoPadronizada()
                        {
                            Codigo = 07,
                            QtdDias = diasInstrucao,
                            Valor = valorInstrucao,
                            TextoInstrucao = "Não protestar"
                        };
                    }
                case EnumTipoInstrucao.ConcederDescontoAte:
                    {
                        return new InstrucaoPadronizada()
                        {
                            Codigo = 22,
                            QtdDias = diasInstrucao,
                            Valor = valorInstrucao,
                            TextoInstrucao = "Conceder desconto até " + dataInstrucao.ToString("ddMMyyyy")
                        };
                    }

                #endregion

                #region Instruções para COMANDO 02 - SOLICITAÇÃO DE BAIXA

                // Para Comando 02 - Solicitação de Baixa (posição 109-110)

                case EnumTipoInstrucao.Devolver:
                    {
                        return new InstrucaoPadronizada()
                        {
                            Codigo = 42,
                            QtdDias = diasInstrucao,
                            Valor = valorInstrucao,
                            TextoInstrucao = "Devolver"
                        };
                    }
                case EnumTipoInstrucao.Baixar:
                    {
                        return new InstrucaoPadronizada()
                        {
                            Codigo = 44,
                            QtdDias = diasInstrucao,
                            Valor = valorInstrucao,
                            TextoInstrucao = "Baixar"
                        };
                    }
                case EnumTipoInstrucao.EntregarAoSacado:
                    {
                        return new InstrucaoPadronizada()
                        {
                            Codigo = 46,
                            QtdDias = diasInstrucao,
                            Valor = valorInstrucao,
                            TextoInstrucao = "Entregar ao sacado franco de pagamento."
                        };
                    }

                    #endregion

            }
            throw new Exception(String.Format("Não foi possível obter instrução padronizada. Banco: {0} Código Instrução: {1} Qtd Dias/Valor: {2}",
                CodigoBanco, tipoInstrucao.ToString(), valorInstrucao));
        }

        public RetornoGenerico LerArquivoRetorno(List<string> linhasArquivo)
        {
            if (linhasArquivo == null || linhasArquivo.Any() == false)
                throw new ApplicationException("Arquivo informado é inválido/Não existem títulos no retorno.");

            /* Identifica o layout: 240 */
            if (linhasArquivo.First().Length == 240)
            {
                var leitor = new LeitorRetornoCnab240BancoDoBrasil(linhasArquivo);
                var retornoProcessado = leitor.ProcessarRetorno();

                var objRetornar = new RetornoGenerico(retornoProcessado);
                return objRetornar;
            }
            /* Identifica o layout: 400 */
            if (linhasArquivo.First().Length == 400)
            {
                var leitor = new LeitorRetornoCnab400BancoDoBrasil(linhasArquivo);
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
                var leitor = new LeitorRetornoPagamentoCnab240BancoDoBrasil(linhasArquivo);
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

        public RemessaCnab240 GerarArquivoRemessaCnab240(RemessaCnab240 remessaCnab240, List<Boleto> boletos)
        {
            throw new NotImplementedException();
        }

        public RemessaCnab400 GerarArquivoRemessaCnab400(RemessaCnab400 remessaCnab400, List<Boleto> boletos)
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
    }
}
