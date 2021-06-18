using BoletoBr.Arquivo.CNAB240.Remessa;
using BoletoBr.Arquivo.CNAB400.Remessa;
using BoletoBr.Arquivo.Generico.Retorno;
using BoletoBr.Dominio;
using BoletoBr.Dominio.Instrucao;
using BoletoBr.Enums;
using BoletoBr.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;

namespace BoletoBr.Bancos.Abc
{
    public class BancoAbc : IBanco
    {
        private int _dvCodigoBarras;
        private string _campoLivre;

        public string CodigoBanco { get; set; }
        public string DigitoBanco { get; set; }
        public string NomeBanco { get; set; }
        public Image LogotipoBancoParaExibicao { get; set; }
        public string LocalDePagamento { get; private set; }
        public string MoedaBanco { get; private set; }

        public BancoAbc()
        {
            CodigoBanco = "246";
            DigitoBanco = "0";
            NomeBanco = "ABC";
            LocalDePagamento = "PAGÁVEL EM TODA REDE BANCÁRIA";
            MoedaBanco = "9";
        }

        public void ValidaBoletoComNormasBanco(Boleto boleto)
        {
            if (!(boleto.CarteiraCobranca.Codigo == "110"))
                throw new NotImplementedException("Carteira não implementada. Carteira dísponivel 110");

            if (boleto.CarteiraCobranca.Codigo == "110")
            {
                if (boleto.CedenteBoleto.ContaBancariaCedente.Agencia.BoletoBrToStringSafe().Trim().Length > 4)
                    throw new ValidacaoBoletoException("A agencia deve ter no máximo 4 dígitos.");

                var codigoCliente = boleto.CedenteBoleto.CodigoCedente + boleto.CedenteBoleto.DigitoCedente;
                if (codigoCliente.BoletoBrToStringSafe().Trim().Length > 10)
                    throw new ValidacaoBoletoException("O código do cedente deve ter no máximo 10 dígitos.");

                if (boleto.IdentificadorInternoBoleto.BoletoBrToStringSafe().Trim().Length > 10)
                    throw new ValidacaoBoletoException("Identificador interno do boleto deve ter no máximo 10 dígitos.");
            }
        }

        public void FormataMoeda(Boleto boleto)
        {
            boleto.Moeda = MoedaBanco;

            if (String.IsNullOrEmpty(boleto.Moeda))
                throw new Exception("Espécie/Moeda para o boleto não foi informada.");

            if ((boleto.Moeda == "9") || (boleto.Moeda == "REAL") || (boleto.Moeda == "R$"))
                boleto.Moeda = "R$";
        }

        public void FormatarBoleto(Boleto boleto)
        {
            boleto.LocalPagamento = LocalDePagamento;

            boleto.ValidaDadosEssenciaisDoBoleto();

            FormataNumeroDocumento(boleto);
            FormataNossoNumero(boleto);
            FormataCodigoBarra(boleto);
            FormataLinhaDigitavel(boleto);
            FormataMoeda(boleto);

            ValidaBoletoComNormasBanco(boleto);

            boleto.CedenteBoleto.CodigoCedenteFormatado = String.Format("{0}/{1}{2}",
                boleto.CedenteBoleto.ContaBancariaCedente.Agencia.PadLeft(4, '0'),
                boleto.CedenteBoleto.CodigoCedente.PadLeft(7, '0'),
                boleto.CedenteBoleto.DigitoCedente);
        }

        public void FormataCodigoBarra(Boleto boleto)
        {
            var codigoBanco = CodigoBanco.PadLeft(3, '0'); //3
            var codigoMoeda = MoedaBanco; //1
            var fatorVencimento = Common.FatorVencimento(boleto.DataVencimento).ToString(); //4
            var valorNominal = boleto.ValorBoleto.ToString("f").Replace(",", "").Replace(".", "").PadLeft(10, '0'); //10

            var codigoAgencia = boleto.CedenteBoleto.ContaBancariaCedente.Agencia.PadLeft(4, '0'); //4
            var carteiraCobranca = boleto.CarteiraCobranca.Codigo.PadLeft(3, '0'); //3 

            /*Numero da Operação*/
            var numeroOperacao = boleto.CedenteBoleto.Operacao.PadLeft(7, '0'); //7

            var nossoNumeroBoleto = boleto.NossoNumeroFormatado.Replace("-", "").PadLeft(11, '0'); //11
            var campoLivre = string.Format("{0}{1}{2}{3}", codigoAgencia, carteiraCobranca, numeroOperacao, nossoNumeroBoleto);

            boleto.CodigoBarraBoleto = string.Format("{0}{1}{2}{3}{4}", codigoBanco, codigoMoeda, fatorVencimento, valorNominal, campoLivre);

            _dvCodigoBarras = Common.Mod11Peso2a9(boleto.CodigoBarraBoleto);

            boleto.CodigoBarraBoleto = string.Format("{0}{1}{2}{3}{4}{5}", codigoBanco, codigoMoeda, _dvCodigoBarras, fatorVencimento, valorNominal, campoLivre);

            _campoLivre = campoLivre;
        }

        public void FormataLinhaDigitavel(Boleto boleto)
        {
            var nossoNumero = boleto.NossoNumeroFormatado.Replace("-", "").PadLeft(8, '0');

            #region Campo 1

            var codigoBanco = CodigoBanco.PadLeft(3, '0'); //3
            var codigoMoeda = MoedaBanco; //1
            var livre1 = _campoLivre.Substring(0, 5);

            var calculoDv1 = Common.Mod10(string.Format("{0}{1}{2}", codigoBanco, codigoMoeda, livre1)).ToString(CultureInfo.InvariantCulture); //1
            var campo1 = string.Format("{0}{1}{2}{3}", codigoBanco, codigoMoeda, livre1, calculoDv1);
            var grupo1 = string.Format("{0}.{1}", campo1.Substring(0, 5), campo1.Substring(5, 5));

            #endregion

            #region Campo 2

            var livre2 = _campoLivre.Substring(5, 10);
            var calculoDv2 = Common.Mod10(livre2).ToString(CultureInfo.InvariantCulture); //1
            var campo2 = string.Format("{0}{1}",livre2,calculoDv2); 
            var grupo2 = string.Format("{0}.{1}", campo2.Substring(0, 5), campo2.Substring(5, 6));

            #endregion

            #region Campo 3

            var livre3 = _campoLivre.Substring(15, 10);
            var calculoDv3 = Common.Mod10(livre3).ToString(CultureInfo.InvariantCulture); //1
            var campo3 = string.Format("{0}{1}", livre3, calculoDv3);
            var grupo3 = string.Format("{0}.{1}", campo3.Substring(0, 5), campo3.Substring(5, 6));

            #endregion

            #region Campo 5

            var fatorVencimento = Common.FatorVencimento(boleto.DataVencimento).ToString(); //4
            var valorNominal = boleto.ValorBoleto.ToString("f").Replace(",", "").Replace(".", "").PadLeft(10, '0'); //10

            var grupo5 = string.Format("{0}{1}", fatorVencimento, valorNominal);

            #endregion

            boleto.LinhaDigitavelBoleto = string.Format("{0} {1} {2} {3} {4}", grupo1, grupo2, grupo3, _dvCodigoBarras, grupo5);
        }

        public void FormataNossoNumero(Boleto boleto)
        {
            if (String.IsNullOrEmpty(boleto.IdentificadorInternoBoleto) || String.IsNullOrEmpty(boleto.IdentificadorInternoBoleto.TrimStart('0')))
                throw new Exception("Sequencial Nosso Número não foi informado.");

            if (boleto.CarteiraCobranca.Codigo == "110")
            {
                var numeroAgencia = boleto.CedenteBoleto.ContaBancariaCedente.Agencia.Replace(".", "").Replace("/", "").Replace("-", "").Replace(",", "").BoletoBrToStringSafe().PadLeft(4, '0');
                var identificadorInternoBoleto = boleto.IdentificadorInternoBoleto.BoletoBrToStringSafe().PadLeft(10, '0');

                var nossoNumeroComposto = numeroAgencia + "110" + identificadorInternoBoleto;

                boleto.SetNossoNumeroFormatado(String.Format("{0}-{1}", identificadorInternoBoleto, Common.Mod10(nossoNumeroComposto)));
            }
        }

        public void FormataNumeroDocumento(Boleto boleto)
        {
            if (String.IsNullOrEmpty(boleto.NumeroDocumento) || String.IsNullOrEmpty(boleto.NumeroDocumento.TrimStart('0')))
                throw new Exception("Número do Documento não foi informado.");

            boleto.NumeroDocumento = boleto.NumeroDocumento.PadLeft(7, '0');
        }

        public ICodigoOcorrencia ObtemCodigoOcorrenciaByInt(int numeroOcorrencia)
        {
            switch (numeroOcorrencia)
            {
                case 02:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 02,
                        Descricao = "CONFIRMAÇÃO ENTRADA DE TITULO"
                    };
                case 03:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 03,
                        Descricao = "ENTRADA REJEITADA"
                    };
                case 04:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 04,
                        Descricao = "Transferência de Carteira/Entrada"
                    };
                case 05:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 05,
                        Descricao = "Transferência de Carteira/Baixa"
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
                        Descricao = "CONFIRMAÇÃO DO RECEBIMENTO DA INSTRUÇÃO DE DESCONTO"
                    };
                case 08:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 08,
                        Descricao = "CONFIRMAÇÃO DO RECEBIMENTO DO CANCELAMENTO DO DESCONTO"
                    };
                case 09:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 09,
                        Descricao = "BAIXA SIMPLES"
                    };
                case 10:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 10,
                        Descricao = "BAIXADO CONFORME INSTRUÇÕES DA AGÊNCIA"
                    };
                case 11:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 11,
                        Descricao = "TITULO EM SER"
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
                        Descricao = "CANCELAMENTO ABATIMENTO"
                    };
                case 14:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 14,
                        Descricao = "ALTERAÇÃO DE VENCIMENTO"
                    };
                case 15:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 15,
                        Descricao = "FRANCO DE PAGAMENTO"
                    };
                case 17:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 17,
                        Descricao = "LIQUIDAÇÃO APÓS BAIXA OU LIQUIDAÇÃO TÍTULO NÃO REGISTRADO"
                    };
                case 19:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 19,
                        Descricao = "CONFIRMAÇÃO RECEBIMENTO INSTRUÇÃO DE PROTESTO"
                    };
                case 20:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 20,
                        Descricao = "CONFIRMAÇÃO RECEBIMENTO INSTRUÇÃO DE SUSTAÇÃO/CANCELAMENTO DE PROTESTO"
                    };
                case 23:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 23,
                        Descricao = "REMESSA A CARTÓRIO (APONTE EM CARTÓRIO)"
                    };
                case 24:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 24,
                        Descricao = "RETIRADA DE CARTÓRIO E MANUTENÇÃO EM CARTEIRA"
                    };
                case 25:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 25,
                        Descricao = "PROTESTADO E BAIXADO (BAIXA POR TER SIDO PROTESTADO)"
                    };
                case 26:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 26,
                        Descricao = "INSTRUÇÃO REJEITADA"
                    };
                case 27:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 27,
                        Descricao = "CONFIRMAÇÃO DE ALTERAÇÃO DE DADOS"
                    };
                case 28:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 28,
                        Descricao = "DÉBITO DE TARIFAS/CUSTAS"
                    };
                case 29:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 29,
                        Descricao = "OCORRÊNCIAS DO PAGADOR"
                    };
                case 30:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 30,
                        Descricao = "ALTERAÇÃO DE DADOS REJEITADA"
                    };
                case 33:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 33,
                        Descricao = "CONFIRMAÇÃO DA ALTERAÇÃO DOS DADOS DO RATEIO DE CRÉDITO"
                    };
                case 48:
                    return new CodigoOcorrencia(numeroOcorrencia)
                    {
                        Codigo = 48,
                        Descricao = "CONFIRMAÇÃO DE INSTRUÇÃO DE TRANSFERÊNCIA DE CARTEIRA/MODALIDADE DE COBRANÇA"
                    };
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
                        return new CodigoOcorrencia((int)ocorrencia)
                        {
                            Codigo = 01,
                            Descricao = "Entrada de título"
                        };
                    }
                case EnumCodigoOcorrenciaRemessa.Baixa:
                    {
                        return new CodigoOcorrencia((int)ocorrencia)
                        {
                            Codigo = 02,
                            Descricao = "Pedido de baixa"
                        };
                    }
                case EnumCodigoOcorrenciaRemessa.ConcessaoDeAbatimento:
                    {
                        return new CodigoOcorrencia((int)ocorrencia)
                        {
                            Codigo = 04,
                            Descricao = "Concessão de abatimento"
                        };
                    }
                case EnumCodigoOcorrenciaRemessa.CancelamentoDeAbatimento:
                    {
                        return new CodigoOcorrencia((int)ocorrencia)
                        {
                            Codigo = 05,
                            Descricao = "Cancelamento de abatimento"
                        };
                    }
                case EnumCodigoOcorrenciaRemessa.AlteracaoDeVencimento:
                    {
                        return new CodigoOcorrencia((int)ocorrencia)
                        {
                            Codigo = 06,
                            Descricao = "Alteração de vencimento"
                        };
                    }
                case EnumCodigoOcorrenciaRemessa.CancelamentoDeDesconto:
                    {
                        return new CodigoOcorrencia((int)ocorrencia)
                        {
                            Codigo = 08,
                            Descricao = "Cancelamento de Desconto"
                        };
                    }
                case EnumCodigoOcorrenciaRemessa.Protesto:
                    {
                        return new CodigoOcorrencia((int)ocorrencia)
                        {
                            Codigo = 09,
                            Descricao = "Pedido de Protesto"
                        };
                    }
                case EnumCodigoOcorrenciaRemessa.SustarProtesto:
                    {
                        return new CodigoOcorrencia((int)ocorrencia)
                        {
                            Codigo = 10,
                            Descricao = "Pedido de Sustação de Protesto"
                        };
                    }
                case EnumCodigoOcorrenciaRemessa.SustarProtestoEManterEmCarteira:
                    {
                        return new CodigoOcorrencia((int)ocorrencia)
                        {
                            Codigo = 11,
                            Descricao = "Sustar Protesto e Manter em Carteira"
                        };
                    }
                 
                case EnumCodigoOcorrenciaRemessa.CedenteNaoConcordaComAlegacaoDoSacado:
                    {
                        return new CodigoOcorrencia((int)ocorrencia)
                        {
                            Codigo = 30,
                            Descricao = "Recusa da Alegação do Sacado"
                        };
                    } 
                case EnumCodigoOcorrenciaRemessa.AlteracaoDeOutrosDados:
                    {
                        return new CodigoOcorrencia((int)ocorrencia)
                        {
                            Codigo = 31,
                            Descricao = "Alteração de outros dados"
                        };
                    }
                case EnumCodigoOcorrenciaRemessa.AlteracaoDosDadosDoRateioDeCredito:
                    {
                        return new CodigoOcorrencia((int)ocorrencia)
                        {
                            Codigo = 33,
                            Descricao = "Alteração dos Dados do Rateio de Crédito"
                        };
                    }
                case EnumCodigoOcorrenciaRemessa.CancelamentoDoRateioDeCredito:
                    {
                        return new CodigoOcorrencia((int)ocorrencia)
                        {
                            Codigo = 34,
                            Descricao = "Pedido de Cancelamento dos Dados do Rateio de Crédito"
                        };
                    }
                case EnumCodigoOcorrenciaRemessa.AlteracaoValorTitulo:
                    {
                        return new CodigoOcorrencia((int)ocorrencia)
                        {
                            Codigo = 47,
                            Descricao = "Alteração do Valor Nominal do título (altera vencimento também)"
                        };
                    }
                case EnumCodigoOcorrenciaRemessa.AlteracaoValorMinimoPercentual:
                    {
                        return new CodigoOcorrencia((int)ocorrencia)
                        {
                            Codigo = 48,
                            Descricao = "Alteração do Valor Mínimo/Percentual"
                        };
                    }
                case EnumCodigoOcorrenciaRemessa.AlteracaoValorMaximoPercentual:
                    {
                        return new CodigoOcorrencia((int)ocorrencia)
                        {
                            Codigo = 49,
                            Descricao = "Alteração do Valor Máximo/Percentual"
                        };
                    }
            }

            throw new Exception(
                String.Format(
                    "Não foi possível obter Código de Comando/Movimento/Ocorrência. Banco: {0} Código: {1}",
                    CodigoBanco, ocorrencia.ToString()));
        }

        public ICodigoOcorrencia ObtemCodigoOcorrencia(EnumCodigoOcorrenciaRetorno ocorrenciaRetorno)
        {
            throw new NotImplementedException();
        }

        public IEspecieDocumento ObtemEspecieDocumento(EnumEspecieDocumento especie)
        {
            switch (especie)
            {
                case EnumEspecieDocumento.Cheque:
                    {
                        return new EspecieDocumento((int)especie)
                        {
                            Codigo = 01,
                            Descricao = "Cheque",
                            Sigla = "CH"
                        };
                    }
                case EnumEspecieDocumento.CedulaProdutoRural:
                    {
                        return new EspecieDocumento((int)especie)
                        {
                            Codigo = 25,
                            Descricao = "Cédulo de Produto Rura",
                            Sigla = ""
                        };
                    }
                case EnumEspecieDocumento.DividaAtivaEstado:
                    {
                        return new EspecieDocumento((int)especie)
                        {
                            Codigo = 27,
                            Descricao = "Dívida Ativa de Estado",
                            Sigla = ""
                        };
                    }
                case EnumEspecieDocumento.DividaAtivaMunicipio:
                    {
                        return new EspecieDocumento((int)especie)
                        {
                            Codigo = 28,
                            Descricao = "Dívida Ativa de Municipio",
                            Sigla = ""
                        };
                    }
                case EnumEspecieDocumento.DividaAtivaUniao:
                    {
                        return new EspecieDocumento((int)especie)
                        {
                            Codigo = 29,
                            Descricao = "Dívida Ativa da União",
                            Sigla = ""
                        };
                    }
                case EnumEspecieDocumento.EncargosCondominais:
                    {
                        return new EspecieDocumento((int)especie)
                        {
                            Codigo = 30,
                            Descricao = "Encargos Condominais",
                            Sigla = ""
                        };
                    }
                case EnumEspecieDocumento.CartaoDeCredito:
                    {
                        return new EspecieDocumento((int)especie)
                        {
                            Codigo = 31,
                            Descricao = "Cartao de Crédito",
                            Sigla = "CC"
                        };
                    }
                case EnumEspecieDocumento.DuplicataMercantil:
                    {
                        return new EspecieDocumento((int)especie)
                        {
                            Codigo = 02,
                            Descricao = "Duplicata Mercantil",
                            Sigla = "DM"
                        };
                    }
                case EnumEspecieDocumento.NotaSeguro:
                    {
                        return new EspecieDocumento((int)especie)
                        {
                            Codigo = 16,
                            Descricao = "Nota de Seguro",
                            Sigla = "NS"
                        };
                    }
                case EnumEspecieDocumento.Recibo:
                    {
                        return new EspecieDocumento((int)especie)
                        {
                            Codigo = 17,
                            Descricao = "Recibo",
                            Sigla = "RE"
                        };
                    }
                case EnumEspecieDocumento.DuplicataRural:
                    {
                        return new EspecieDocumento((int)especie)
                        {
                            Codigo = 06,
                            Descricao = "Duplicata Rural",
                            Sigla = "DR"
                        };
                    }
                case EnumEspecieDocumento.LetraCambio:
                    {
                        return new EspecieDocumento((int)especie)
                        {
                            Codigo = 07,
                            Descricao = "Letra Câmbio",
                            Sigla = "LC"
                        };
                    }
                case EnumEspecieDocumento.Warrant:
                    {
                        return new EspecieDocumento((int)especie)
                        {
                            Codigo = 26,
                            Descricao = "Warrant",
                            Sigla = "WA"
                        };
                    } 
                case EnumEspecieDocumento.DuplicataServico:
                    {
                        return new EspecieDocumento((int)especie)
                        {
                            Codigo = 04,
                            Descricao = "Duplicata de Serviço",
                            Sigla = "DS"
                        };
                    }
                case EnumEspecieDocumento.NotaDebito:
                    {
                        return new EspecieDocumento((int)especie)
                        {
                            Codigo = 19,
                            Descricao = "Nota de Débito",
                            Sigla = "ND"
                        };
                    }
                case EnumEspecieDocumento.TriplicataMercantil:
                    {
                        return new EspecieDocumento((int)especie)
                        {
                            Codigo = 14,
                            Descricao = "Triplicata Mercantil",
                            Sigla = "TM"
                        };
                    }
                case EnumEspecieDocumento.TriplicataServico:
                    {
                        return new EspecieDocumento((int)especie)
                        {
                            Codigo = 12,
                            Descricao = "Triplicata de Serviço",
                            Sigla = "NP"
                        };
                    }
                case EnumEspecieDocumento.Fatura:
                    {
                        return new EspecieDocumento((int)especie)
                        {
                            Codigo = 18,
                            Descricao = "Fatura",
                            Sigla = "FAT"
                        };
                    }
                case EnumEspecieDocumento.ApoliceSeguro:
                    {
                        return new EspecieDocumento((int)especie)
                        {
                            Codigo = 20,
                            Descricao = "Apólice de Seguro",
                            Sigla = "AP"
                        };
                    }
                case EnumEspecieDocumento.MensalidadeEscolar:
                    {
                        return new EspecieDocumento((int)especie)
                        {
                            Codigo = 21,
                            Descricao = "Mensalidade Escolar",
                            Sigla = "ME"
                        };
                    }
                case EnumEspecieDocumento.ParcelaConsorcio:
                    {
                        return new EspecieDocumento((int)especie)
                        {
                            Codigo = 22,
                            Descricao = "Parcela de Consórcio",
                            Sigla = "PC"
                        };
                    }
                case EnumEspecieDocumento.Outros:
                    {
                        return new EspecieDocumento((int)especie)
                        {
                            Codigo = 99,
                            Descricao = "Outros",
                            Sigla = "OT"
                        };
                    }
            }

            throw new Exception(
                String.Format("Não foi possível obter instrução padronizada. Banco: {0} Código Espécie: {1}",
                    CodigoBanco, especie.ToString()));
        }

        public IInstrucao ObtemInstrucaoPadronizada(EnumTipoInstrucao tipoInstrucao, double valorInstrucao, DateTime dataInstrucao,
            int diasInstrucao)
        {
            switch (tipoInstrucao)
            {
                case EnumTipoInstrucao.SemInstrucoes:
                    {
                        return new InstrucaoPadronizada()
                        {
                            Codigo = 0,
                            QtdDias = diasInstrucao,
                            Valor = valorInstrucao,
                            TextoInstrucao = "AUSENCIA DE INSTRUCOES"
                        };
                    }
                case EnumTipoInstrucao.JurosdeMora:
                    {
                        return new InstrucaoPadronizada()
                        {
                            Codigo = 1,
                            QtdDias = diasInstrucao,
                            Valor = valorInstrucao,
                            TextoInstrucao = "COBRAR JUROS"
                        };
                    }
                case EnumTipoInstrucao.ProtestarApos3DiasUteis:
                    {
                        return new InstrucaoPadronizada()
                        {
                            Codigo = 3,
                            QtdDias = diasInstrucao,
                            Valor = valorInstrucao,
                            TextoInstrucao = "PROTESTAR APÓS " + diasInstrucao + " DIAS ÚTEIS DO VENCIMENTO"
                        };
                    }
                case EnumTipoInstrucao.ProtestarApos4DiasUteis:
                    {
                        return new InstrucaoPadronizada()
                        {
                            Codigo = 4,
                            QtdDias = diasInstrucao,
                            Valor = valorInstrucao,
                            TextoInstrucao = "PROTESTAR APÓS " + diasInstrucao + " DIAS ÚTEIS DO VENCIMENTO"
                        };
                    }
                case EnumTipoInstrucao.ProtestarApos5DiasUteis:
                    {
                        return new InstrucaoPadronizada()
                        {
                            Codigo = 5,
                            QtdDias = diasInstrucao,
                            Valor = valorInstrucao,
                            TextoInstrucao = "PROTESTAR APÓS " + diasInstrucao + " DIAS ÚTEIS DO VENCIMENTO"
                        };
                    }
                case EnumTipoInstrucao.NaoProtestar:
                    {
                        return new InstrucaoPadronizada()
                        {
                            Codigo = 7,
                            QtdDias = diasInstrucao,
                            Valor = valorInstrucao,
                            TextoInstrucao = "NÃO PROTESTAR"
                        };
                    }
                case EnumTipoInstrucao.ProtestarAposNDiasUteis:
                    {
                        return new InstrucaoPadronizada()
                        {
                            Codigo = 10,
                            QtdDias = diasInstrucao,
                            Valor = valorInstrucao,
                            TextoInstrucao = "PROTESTAR APÓS " + diasInstrucao + " DIAS ÚTEIS DO VENCIMENTO"
                        };
                    }
                case EnumTipoInstrucao.ConcederDescontoAte:
                    {
                        return new InstrucaoPadronizada()
                        {
                            Codigo = 22,
                            QtdDias = diasInstrucao,
                            Valor = valorInstrucao,
                            TextoInstrucao = "CONCEDER DESCONTO SO ATE A DATA ESTIPULADA"
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
                var leitor = new LeitorRetornoCnab240Abc(linhasArquivo);
                var retornoProcessado = leitor.ProcessarRetorno();

                var objRetornar = new RetornoGenerico(retornoProcessado);
                return objRetornar;
            }

            /*Non ecziste*/
            //if (linhasArquivo.First().Length == 400)
            //{
            //    var leitor = new LeitorRetornoCnab400Abc(linhasArquivo);
            //    var retornoProcessado = leitor.ProcessarRetorno();

            //    var objRetornar = new RetornoGenerico(retornoProcessado);
            //    return objRetornar;
            //}

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

         /// <summary>
         /// CODIGO DO JUROS/MORA - CAMPO 118 (descrição C018 do layout) = “1” = Valor ao dia
         /// “2” =taxa mensal e “3”= isento.
         /// </summary>
         /// <param name="codigoJurosMora"></param>
         /// <returns></returns>
        public int CodigoJurosMora(CodigoJurosMora codigoJurosMora)
        {
            switch (codigoJurosMora)
            {
                case Enums.CodigoJurosMora.Isento:
                    return 3;
                case Enums.CodigoJurosMora.Diario:
                    return 1;
                case Enums.CodigoJurosMora.Mensal:
                    return 2;
                default: return 0;
            }
        }

        public int CodigoProteso(bool protestar = true)
        {
            switch (protestar)
            {
                case true:
                    return 1;
                default:
                    return 3;
            }
        }

        public RetornoGenericoPagamento LerArquivoRetornoPagamento(List<string> linhasArquivo)
        {
            throw new NotImplementedException();
        }

        public ICodigoOcorrencia ObtemCodigoOcorrenciaPagamento(string numeroOcorrencia)
        {
            switch (numeroOcorrencia)
            {
                case "00": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "00", Descricao = "Pagamento confirmado".ToUpper() }; }
                case "AJ": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "AJ", Descricao = "Tipo de Movimento Inválido / Erro CNAB".ToUpper() }; }
                case "BD": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "BD", Descricao = "Inclusão Efetuada com Sucesso".ToUpper() }; }
                case "BF": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "BF", Descricao = "Transação Rejeitada".ToUpper() }; }
                case "PD": { return new CodigoOcorrencia(numeroOcorrencia) { CodigoText = "PD", Descricao = "Transação Pendente de Assinatura".ToUpper() }; }

            }
            throw new Exception(
                String.Format(
                    "Não foi possível obter Código de Comando/Movimento/Ocorrência. Banco: {0} Código: {1}",
                    CodigoBanco, numeroOcorrencia.ToString()));
        }
    }
}
