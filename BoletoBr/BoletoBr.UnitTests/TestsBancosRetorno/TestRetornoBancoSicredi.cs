using System;
using System.Collections.Generic;
using BoletoBr.Bancos.BancoSicredi; 
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BoletoBr.UnitTests.TestsBancosRetorno
{
    [TestClass]
    public class TestRetornoBancoSicredi
    {
        #region CNAB400

        [TestMethod]
        public void TestHeaderArquivoRetornoCnab400Sicredi()
        {
            LeitorRetornoCnab400Sicredi leitor = new LeitorRetornoCnab400Sicredi(null);

            string valorTesteRegistro =
                "02RETORNO01COBRANCA       8111109999999999999                               748BANSICREDI     20170203        0000471                                                                                                                                                                                                                                                                                 1.00000001";

            var resultado = leitor.ObterHeader(valorTesteRegistro);

            Assert.AreEqual(resultado.LiteralRetorno, "RETORNO");
        }

        [TestMethod]
        public void TestDetalheArquivoRetornoCnab400Sicredi()
        {
            LeitorRetornoCnab400Sicredi leitor = new LeitorRetornoCnab400Sicredi(null);

            string valorTesteRegistro =
                "1            A2AAAA000002                      162000001                                                    06020217000000001 081206              1502170000000079242         A000000000000000000000000000000000000000000000000000000000000000000000000000000000000007924200000000000000000000000000                          000000000020170203                                                          000002";

            var resultado = leitor.ObterRegistrosDetalhe(valorTesteRegistro);

            Assert.AreEqual(resultado.DataDeVencimento.Date, new DateTime(2017,2,15).Date);
            Assert.AreEqual(resultado.ValorDoTituloParcela, 792.42M);
        }

        [TestMethod]
        public void TestTrailerArquivoRetornoCnab400Sicredi()
        {
            LeitorRetornoCnab400Sicredi leitor = new LeitorRetornoCnab400Sicredi(null);

            string valorTesteRegistro =
                "9274881111                                                                                                                                                                                                                                                                                                                                                                                                000003";

            var resultado = leitor.ObterTrailer(valorTesteRegistro);

            Assert.AreEqual(resultado.CodigoBeneficiario, 81111);
        }

        [TestMethod]
        public void TestArquivoRetornoCnab400Sicredi()
        {

            List<string> list = new List<string>();
            list.Add("02RETORNO01COBRANCA       2055313562237000108                               748BANSICREDI     20210415        0000003                                                                                                                                                                                                                                                                                 1.00000001");
            list.Add("1            A06P67000002                      212000180                                                    0214042118                            2704210000000000200         A000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000                          0000000000                                                                  000002");
            list.Add("1            A06P68000002                      212000198                                                    0214042119                            2704210000000000200         A000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000                          0000000000                                                                  000003");
            list.Add("1            A06P5C000002                      211000011                                                    1414042101                            1604210000000049000         A000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000                          0000000000                                                                  000004");
            list.Add("1            A06P67000002                      212000180                                                    0614042118        010117              2704210000000000200         A000000000000000000000000000000000000000000000000000000000000000000000000000000000000000020000000000000000000000000000                          000000000020210415                                                          000005");
            list.Add("1            A06P67000002                      212000180                                                    0714042118                            2704210000000000200         A000000000000000000000000000000000000000000000000000000000000000000000000000000000000000020000000000000000000000000000                          0000000000                                                                  000006");
            list.Add("1            A06P67000002                      212000180                                                    2814042118                            2704210000000000200         A000000000015000000000000000000000000000000000000000000000000000000000000000000000000000015000000000000000000000000000                          B30000000020210414                                                          000007");
            list.Add("1            A06P68000002                      212000198                                                    2814042119                            2704210000000000200         A000000000015000000000000000000000000000000000000000000000000000000000000000000000000000015000000000000000000000000000                          B30000000020210414                                                          000008");
            list.Add("9274820553                                                                                                                                                                                                                                                                                                                                                                                                000009");


            LeitorRetornoCnab400Sicredi leitor = new LeitorRetornoCnab400Sicredi(list);
            var resultado = leitor.ProcessarRetorno();
             
        }
  
        #endregion
    }
}
