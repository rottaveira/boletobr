using Microsoft.VisualStudio.TestTools.UnitTesting; 
using System.Collections.Generic; 

namespace BoletoBr.UnitTests.TestsBancosRetorno
{
    [TestClass]
    public class TestRetornoSicoob
    {
        [TestMethod]
        public void TestRetornoPagamentoCnab240BancoSicoob()
        {
            var linhas = new List<string>
            {
                "75600000         2999999999999997980171             03249200000000734150RAZÃO SOCIAL X                SICOOB                                  10704202116043700000108700000                    REMESSA-PAGAMENTO                                ",
                "75600011C0003045 2999999999999990000000000000012345603249200000000734150Razão Social X                                                        RUA X                         00025EDF EXECUTIVO  GOIABALÂNDIA        75690000GO01                ",
                "7560001300001A0000182370434300000000354322 RAFAEL TAVEIRA                1015                00000000BRL000000000000000000000000000500                    00000000000000000000000                                          5    CC   00000000000",
                "7560001300002B   100001236548901RUA 3                         00005GALHO ESQUERDO TRENZIM        MANGALANDIA         75690000GO210420210000000000005000000000000000000000000000000000000000000000000000000000000001              0000000        ",
                "75600015         000004000000000000000010000000000000000000000000                                                                                                                                                                               ",
                "00199999         000001000006000000                                                                                                                                                                                                             "

            };

            var linhas2 = new List<string>
            {
                "75600000         2999999999999997980171             03249200000000734150RAZÃO SOCIAL X                SICOOB                                  12004202114300500000108700000                    REMESSA-PAGAMENTO                                ",
                "75600011C0003045 2999999999999990000000000000012345603249200000000734150Razão Social X                                                        RUA X                         00025EDF EXECUTIVO  GOIABALÂNDIA        75690000GO01      0000000000",
                "7560001300001J00024691859100000299845004110028796200000000150RAFAEL TAVEIRA                21042021000000000000500000000000000000000000000000000000000000000000000005000000000000000001015                                    09      0000000000",
                "75600015         000004000000000000000005000000000000000000000000                                                                                                                                                                               ",
                "00199999         000001000006000000                                                                                                                                                                                                             ",

            };

            /*Retorno é mesmo formato da remessa. Utilizando remessa para validar*/
            var banco = Fabricas.BancoFactory.ObterBanco("756");
            var objReturn = banco.LerArquivoRetornoPagamento(linhas);

        }
    }
}
