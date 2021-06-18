using System.Collections.Generic; 
using BoletoBr.Bancos.Brasil;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BoletoBr.UnitTests.TestsBancosRetorno
{
    [TestClass]
    public class TestRetornoBancoDoBrasil
    {
        #region CNAB240

        [TestMethod]
        public void TestSegmentosRetornoPagamentoCnab240BancoDoBrasil()
        {
            LeitorRetornoPagamentoCnab240BancoDoBrasil leitor = new LeitorRetornoPagamentoCnab240BancoDoBrasil(null);

            /*Retorno é mesmo formato da remessa. Utilizando remessa para validar*/
            string valorTesteRegistroHeader =       "00100000         2999999999999997980171  0126     TS01234800000001234560RAZAO SOCIAL X                BANCO DO BRASIL                         13003202115271300000100100000                    REMESSA-PAGAMENTO                 000000000000000";
            string valorTesteRegistroHeaderLote =   "00100011C0003001 2999999999999990079801710126     TS01234800000001234560Razao Social X                                                        RUA X                         00025EDF EXECUTIVO  GOIABALÂNDIA        75690000GO        0000000000";
            string valorTesteRegistroDetalheA =     "0010001300001A0000182370434300000000354322 2                             1015                00000000BRL000000000000000000000000000500                    00000000000000000000000                                          5    CC   00000000000";
            string valorTesteRegistroDetalheB =     "0010001300002B   100001236548901RUA 3                         00005GALHO ESQUERDO TRENZIM        MANGALANDIA         75690000GO210420210000000000005000000000000000000000000000000000000000000000000000000000000001              0000000        ";
            string valorTesteRegistroTrailerLote =  "00100015         000004000000000000000010000000000000000000000000                                                                                                                                                                     0000000000";
            string valorTesteRegistroTrailer =      "00199999         000001000006000000                                                                                                                                                                                                             ";

            var resultadoHeader = leitor.ObterHeader(valorTesteRegistroHeader);
            var resultadoHeaderLote = leitor.ObterHeaderLote(valorTesteRegistroHeaderLote);
            var resultadoDetalheA = leitor.ObterRegistrosDetalheA(valorTesteRegistroDetalheA);
            var resultadoDetalheB = leitor.ObterRegistrosDetalheB(valorTesteRegistroDetalheB);
            var resultadoTrailerLote = leitor.ObterTrailerLote(valorTesteRegistroTrailerLote);
            var resultadoTrailer = leitor.ObterTrailer(valorTesteRegistroTrailer);

            Assert.AreEqual(resultadoHeader.CodigoBanco, 1);
        }

        [TestMethod]
        public void TestRetornoPagamentoCnab240BancoDoBrasil()
        { 
            var linhas = new List<string>
            {
                "00100000         2999999999999997980171  0126     TS01234800000001234560RAZAO SOCIAL X                BANCO DO BRASIL                         13003202115271300000100100000                    REMESSA-PAGAMENTO                 000000000000000",
                "00100011C0003001 2999999999999990079801710126     TS01234800000001234560Razao Social X                                                        RUA X                         00025EDF EXECUTIVO  GOIABALÂNDIA        75690000GO        0000000000",
                "0010001300001A0000182370434300000000354322 2                             1015                00000000BRL000000000000000000000000000500                    00000000000000000000000                                          5    CC   00000000002",
                "0010001300002B   100001236548901RUA 3                         00005GALHO ESQUERDO TRENZIM        MANGALANDIA         75690000GO210420210000000000005000000000000000000000000000000000000000000000000000000000000001              0000000        ",
                "00100015         000004000000000000000010000000000000000000000000                                                                                                                                                                     0000000002",
                "00199999         000001000006000000                                                                                                                                                                                                             "

            };

            /*Retorno é mesmo formato da remessa. Utilizando remessa para validar*/
            var banco = Fabricas.BancoFactory.ObterBanco("001");
            var objReturn = banco.LerArquivoRetornoPagamento(linhas);
             
        }
        #endregion

        #region CNAB400

        [TestMethod]
        public void TestHeaderArquivoRetornoCnab400BancoDoBrasil()
        {
            LeitorRetornoCnab400BancoDoBrasil leitor = new LeitorRetornoCnab400BancoDoBrasil(null);

            string valorTesteRegistro =
                "02RETORNO01COBRANCA       00201300369513003695MARIA AUGUSTA SOARES PASCHOAL 033SANTANDER      13081500000000006432794                                                                                                                                                                                                                                                                                  370000001";

            var resultado = leitor.ObterHeader(valorTesteRegistro);

            Assert.AreEqual(resultado.LiteralRetorno, "RETORNO");
        }

        [TestMethod]
        public void TestArquivoRetornoCnab400BancoDoBrasil()
        {
            var linhas = new List<string>
            {
                "02RETORNO01COBRANCA       01651000324736000000HOT BEACH SUITES OLIMPIA - EMP001BANCO DO BRASIL2808150000059                      000000060447579594  2749885                                                                                                                                                                                                                                              000001",
                "70000000000000000016510003247362749885DOC27498850000005356     2749885465588530110002800AI 01900000000000 11022808150000005356                    250915000000003400000104278010000000000350000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000035010000000000000          0000000000000000000000000000000000000000000000001000000002",
                "9201001          000000020000000006807500000002          000000000000000000000000000000          000000000000000000000000000000          000000000000000000000000000000                                                  000000000000000000000000000000                                                                                                                                                   000003"
            };

            var banco = Fabricas.BancoFactory.ObterBanco("001");
            var objReturn = banco.LerArquivoRetorno(linhas);

            Assert.AreEqual(objReturn.RegistrosDetalhe[0].NumeroDocumento, "0000005356");
        }

        #endregion
    }
}
