using System;
using BoletoBr.Arquivo;
using BoletoBr.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BoletoBr.UnitTests.TestsBancos
{
    [TestClass]
    public class BancoUnicrediTests
    {
        #region Carteira 21

        [TestMethod]
        public void FomatarNossoNumeroUnicredi()
        {
            /*
               aG-5951
                CC-000077148
                DGcc -0
                NSSONRO - 00000230839
                VALOR - 22200
             */

            var remessa = new Remessa(Remessa.EnumTipoAmbiemte.Homologacao, EnumCodigoOcorrenciaRemessa.Registro, "2");
            var banco = Fabricas.BancoFactory.ObterBanco("136");
            var contaBancariaCedente = new ContaBancaria("5951", "0", "77148", "0");
            var cedente = new Cedente("77148", 0, "13.562.237/0001-08", "NATIONAL - CONSTRUTORA E INCORPORADORA LTDA", contaBancariaCedente, null);

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

            var carteira = new CarteiraCobranca { Codigo = "21" };

            var boleto = new Boleto(carteira, cedente, sacado, remessa)
            {
                NumeroDocumento = "14369",
                ValorBoleto = Convert.ToDecimal(222),
                IdentificadorInternoBoleto = "23083",
                DataVencimento = new DateTime(2019, 02, 28)
            };

            banco.FormatarBoleto(boleto);

            Assert.IsTrue(boleto.NossoNumeroFormatado.Length == 12);
        }

        [TestMethod]
        public void CalcularLinhaDigitavelCarteiraUnicredi()
        {
            /*
              aG-5951
               CC-000077148
               DGcc -0
               NSSONRO - 00000230839
               VALOR - 22200
            */

            var remessa = new Remessa(Remessa.EnumTipoAmbiemte.Homologacao, EnumCodigoOcorrenciaRemessa.Registro, "2");
            var banco = Fabricas.BancoFactory.ObterBanco("136");
            var contaBancariaCedente = new ContaBancaria("5951", "0", "77148", "0");
            var cedente = new Cedente("77148", 0, "13.562.237/0001-08", "NATIONAL - CONSTRUTORA E INCORPORADORA LTDA", contaBancariaCedente, null);

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

            var carteira = new CarteiraCobranca { Codigo = "21" };

            var boleto = new Boleto(carteira, cedente, sacado, remessa)
            {
                NumeroDocumento = "14369",
                ValorBoleto = Convert.ToDecimal(222),
                IdentificadorInternoBoleto = "23083",
                DataVencimento = new DateTime(2019, 02, 28)
            };

            banco.FormatarBoleto(boleto);

            var linha_banco = boleto.LinhaDigitavelBoleto.Substring(0, 3);
            var linha_Moeda = boleto.LinhaDigitavelBoleto.Substring(3, 1);
            var Valor1 = boleto.LinhaDigitavelBoleto.Substring(48, 10);

            Assert.AreEqual(boleto.NossoNumeroFormatado.Replace("/", "").Replace("-", ""), "00000230839");
            Assert.AreEqual(linha_banco, banco.CodigoBanco);
            Assert.AreEqual(Valor1, "0000022200");
            Assert.AreEqual(boleto.LinhaDigitavelBoleto.Replace(".", "").Replace(" ", "").Length, 47);
        }

        #endregion

    }
}
