using System;
using BoletoBr.Arquivo; 
using BoletoBr.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BoletoBr.UnitTests.TestsBancos
{
    [TestClass]
    public class BancoSicrediTests
    {
        #region Carteira 101

        [TestMethod]
        public void FomatarNossoNumeroSicredi()
        {
            var remessa = new Remessa(Remessa.EnumTipoAmbiemte.Homologacao, EnumCodigoOcorrenciaRemessa.Registro, "2");
            var banco = Fabricas.BancoFactory.ObterBanco("748");
            var contaBancariaCedente = new ContaBancaria("0136", "30", "20553", "5");
            var cedente = new Cedente("20553", 0, "13.562.237/0001-08", "NATIONAL - CONSTRUTORA E INCORPORADORA LTDA", contaBancariaCedente, null);

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

            var carteira = new CarteiraCobranca { Codigo = "1" };

            var boleto = new Boleto(carteira, cedente, sacado, remessa)
            {
                NumeroDocumento = "14369",
                ValorBoleto = Convert.ToDecimal(337.00),
                IdentificadorInternoBoleto = "00052",
                DataVencimento = new DateTime(2021, 11, 10) 
            };

            banco.FormatarBoleto(boleto);

            Assert.IsTrue(boleto.NossoNumeroFormatado.Length == 11);
            Assert.IsTrue(boleto.NossoNumeroFormatado[3].ToString() != "1");
            Assert.IsTrue(boleto.NossoNumeroFormatado[2].ToString() == "/" 
                       && boleto.NossoNumeroFormatado[9].ToString() == "-");
        }
         
        [TestMethod]
        public void CalcularLinhaDigitavelCarteiraSicredi()
        {
            var remessa = new Remessa(Remessa.EnumTipoAmbiemte.Homologacao, EnumCodigoOcorrenciaRemessa.Registro, "2");
            var banco = Fabricas.BancoFactory.ObterBanco("748");
            var contaBancariaCedente = new ContaBancaria("0165", "02", "00623", "9");
            var cedente = new Cedente("12345", 0, "99.999.999/9999-99", "Razão Social X", contaBancariaCedente, null);

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

            var carteira = new CarteiraCobranca { Codigo = "1" };

            var boleto = new Boleto(carteira, cedente, sacado, remessa)
            {
                NumeroDocumento = "14369",
                ValorBoleto = Convert.ToDecimal(337.00),
                IdentificadorInternoBoleto = "00003",
                DataVencimento = new DateTime(2016, 11, 10),
                CodigoDeTransmissao = "00200643279401300369"
            };

            banco.FormatarBoleto(boleto);

            var linha_banco = boleto.LinhaDigitavelBoleto.Substring(0, 3);
            var linha_Moeda = boleto.LinhaDigitavelBoleto.Substring(3,1);
            var linha_TipoCobranca = boleto.LinhaDigitavelBoleto.Substring(4, 1);
            var linha_Carteira = boleto.LinhaDigitavelBoleto.Substring(6, 1);
            var linha_NossoNro1 = boleto.LinhaDigitavelBoleto.Substring(7,3);
            var linha_NossoNro2 = boleto.LinhaDigitavelBoleto.Substring(13,7);
            var Valor1 = boleto.LinhaDigitavelBoleto.Substring(48,10);
            var nossoNro = (linha_NossoNro1 + linha_NossoNro2).Replace(".", "");
             
            Assert.AreEqual(boleto.NossoNumeroFormatado.Replace("/","").Replace("-",""),nossoNro);
            Assert.AreEqual(linha_banco, banco.CodigoBanco);
            Assert.AreEqual(Valor1, "0000033700");
            Assert.AreEqual(boleto.LinhaDigitavelBoleto.Replace(".", "").Replace(" ", "").Length, 47);
        }
         
        #endregion
         
    }
}
