using System;
using System.Globalization;
using BoletoBr.Arquivo;
using BoletoBr.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BoletoBr.UnitTests.TestsBancos
{
    [TestClass]
    public class BancoAbcTests
    {
        #region Carteira 110

        [TestMethod]
        public void FomatarNumeroDocumentoCarteira110Abc()
        {
            var remessa = new Remessa(Remessa.EnumTipoAmbiemte.Homologacao, EnumCodigoOcorrenciaRemessa.Registro, "2");
            var banco = Fabricas.BancoFactory.ObterBanco("246", "0");
            var contaBancariaCedente = new ContaBancaria("5004", "0", "107228", "5");
            var cedente = new Cedente("28796", 2, "99.999.999/9999-99", "Razão Social X", contaBancariaCedente,
                null);

            var sacado = new Sacado("Sacado Fulano de Tal", "999.999.999-99", new Endereco()
            {
                TipoLogradouro = "R",
                Logradouro = "1",
                Bairro = "Bairro X",
                Cidade = "Cidade X",
                SiglaUf = "XX",
                Cep = "12345-000",
                Complemento = "Comp X",
                Numero = "9"
            });

            var carteira = new CarteiraCobranca {Codigo = "110"};

            var boleto = new Boleto(carteira, cedente, sacado, remessa)
            {
                NumeroDocumento = "106",
                ValorBoleto = Convert.ToDecimal(465.82),
                IdentificadorInternoBoleto = "106",
                DataVencimento = new DateTime(2016, 11, 30)
            };

            banco.FormatarBoleto(boleto);

            Assert.AreEqual(boleto.NumeroDocumento.Length, 7);
            Assert.AreEqual("0000106", boleto.NumeroDocumento);
        }

        [TestMethod]
        public void FormatarNossoNumeroCarteira110Abc()
        {
            var remessa = new Remessa(Remessa.EnumTipoAmbiemte.Homologacao, EnumCodigoOcorrenciaRemessa.Registro, "2");
            var banco = Fabricas.BancoFactory.ObterBanco("246", "0");
            var contaBancariaCedente = new ContaBancaria("0001", "9", "2201048", "5");
            var cedente = new Cedente("22026282", 2, "01540533000129", "Cia Thermas do Rio Quente", contaBancariaCedente,
                null);

            var sacado = new Sacado("ITALO GOULART BECK", "383.469.468-13", new Endereco()
            {
                TipoLogradouro = "R",
                Logradouro = "Rubenildo Ramos Ribeiro",
                Bairro = "Jardim Piratininga II",
                Cidade = "Franca",
                SiglaUf = "SP",
                Cep = "14401816",
                Complemento = "Apt 12",
                Numero = "1555"
            });

            var carteira = new CarteiraCobranca { Codigo = "110" };

            var boleto = new Boleto(carteira, cedente, sacado, remessa)
            {
                NumeroDocumento = "85739675",
                ValorBoleto = Convert.ToDecimal(387),
                IdentificadorInternoBoleto = "85739675",
                DataVencimento = new DateTime(2021, 05, 1)
            };

            banco.FormatarBoleto(boleto);

            const string nossoNumeroFormatado = "0085739675-4";
            Assert.AreEqual(nossoNumeroFormatado.Length, boleto.NossoNumeroFormatado.Length);
            Assert.AreEqual(nossoNumeroFormatado, boleto.NossoNumeroFormatado);
        }

        [TestMethod]
        public void CalculosCarteira110Abc()
        {
            /* Exemplos da documentação */

            var mod10 = Common.Mod10(string.Format("999977721")).ToString(CultureInfo.InvariantCulture);
            Assert.AreEqual(mod10, "3");

            mod10 = Common.Mod10(string.Format("3053015008")).ToString(CultureInfo.InvariantCulture);
            Assert.AreEqual(mod10, "2");

            mod10 = Common.Mod10(string.Format("1897500000")).ToString(CultureInfo.InvariantCulture);
            Assert.AreEqual(mod10, "3");

            var mod11 = Common.Mod11Peso2a9(string.Format("9999101200000350007772130530150081897500000")).ToString(CultureInfo.InvariantCulture);
            Assert.AreEqual(mod11, "1"); 
        }

        [TestMethod]
        public void FormatarCodBarras110Abc()
        {
            var remessa = new Remessa(Remessa.EnumTipoAmbiemte.Homologacao, EnumCodigoOcorrenciaRemessa.Registro, "2");
            var banco = Fabricas.BancoFactory.ObterBanco("246", "0");
            var contaBancariaCedente = new ContaBancaria("0001", "9", "107228", "5");
            var cedente = new Cedente("28796", 2, "99.999.999/9999-99", "Razão Social X", contaBancariaCedente,
                null);

            var sacado = new Sacado("Sacado Fulano de Tal", "999.999.999-99", new Endereco()
            {
                TipoLogradouro = "R",
                Logradouro = "1",
                Bairro = "Bairro X",
                Cidade = "Cidade X",
                SiglaUf = "XX",
                Cep = "12345-000",
                Complemento = "Comp X",
                Numero = "9"
            });

            var carteira = new CarteiraCobranca { Codigo = "110" };

            var boleto = new Boleto(carteira, cedente, sacado, remessa)
            {
                NumeroDocumento = "106",
                ValorBoleto = Convert.ToDecimal(465.82),
                IdentificadorInternoBoleto = "0004309540",
                DataVencimento = new DateTime(2016, 11, 30)
            };

            banco.FormatarBoleto(boleto);
             
            Assert.IsTrue(boleto.CodigoBarraBoleto.Length == 44); 
        }
        #endregion
    }
}
