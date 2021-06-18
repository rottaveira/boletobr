 
using BoletoBr.Bancos.Bradesco;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BoletoBr.UnitTests.TestsBancosRetorno
{
    [TestClass]
    public class TestRetornoBradesco
    {
        #region CNAB 240

        [TestMethod]
        public void TestHeaderArquivoRetornoCnab240BancoBradesco() { }

        [TestMethod]
        public void TestRegistroArquivoRetornoCnab240BancoBradesco() { }

        [TestMethod]
        public void TestTrailerArquivoRetornoCnab240BancoBradesco() { }

        [TestMethod]
        public void TestRetornoPagamentoCnab240BancoBradesco()
        {
            LeitorRetornoPagamentoCnab240Bradesco leitor = new LeitorRetornoPagamentoCnab240Bradesco(null);

            /*Retorno é mesmo formato da remessa. Utilizando remessa para validar*/
            string valorTesteRegistroHeader =      "23700000         2999999999999997980171             0123480000000123456 RAZAO SOCIAL X0000000000000000BRADESCO                                13003202112083600000108900000                    REMESSA-PAGAMENTO                                ";
            string valorTesteRegistroHeaderLote =  "23700011C0100045 299999999999999000000000000079801710123480000000123456 RAZAO SOCIAL X                                                        RUA X                            25EDF EXECUTIVO  GOIABALÂNDIA        75690000GO01                ";
            string valorTesteRegistroDetalheA =    "2370001300001A0000180010434300000000354322 2                             1015                00000000BRL000000000000000000000000000500                    00000000000000000000000                                          5    CC   6          ";
            string valorTesteRegistroDetalheB =    "2370001300002B   100001236548901RUA 3                         00005GALHO ESQUERDO TRENZIM        MANGALANDIA         75690000GO210420210000000000005000000000000000000000000000000000000000000000000000000000000001              600000000000000";
            string valorTesteRegistroTrailerLote = "10400015         000004000000000000000000000000000000000000000000000000000000000000000000000                                                                                                                                                    ";
            string valorTesteRegistroTrailer =     "00099999         000001000006000000                                                                                                                                                                                                             ";

            var resultadoHeader = leitor.ObterHeader(valorTesteRegistroHeader);
            var resultadoHeaderLote = leitor.ObterHeaderLote(valorTesteRegistroHeaderLote);
            var resultadoDetalheA = leitor.ObterRegistrosDetalheA(valorTesteRegistroDetalheA);
            var resultadoDetalheB = leitor.ObterRegistrosDetalheB(valorTesteRegistroDetalheB);
            var resultadoTrailerLote = leitor.ObterTrailerLote(valorTesteRegistroTrailerLote);
            var resultadoTrailer = leitor.ObterTrailer(valorTesteRegistroTrailer);

            Assert.AreEqual(resultadoHeader.CodigoBanco, 237);
        }
        #endregion

        #region CNAB 400

        [TestMethod]
        public void TestHeaderArquivoRetornoCnab400BancoBradesco()
        {
            LeitorRetornoCnab400Bradesco leitor = new LeitorRetornoCnab400Bradesco(null);

            string valorTesteRegistro =
                "02RETORNO01COBRANCA       00000000000004603020RMEX CONSTRUTORA E INCORPORADO237BRADESCO       0907140160000000131                                                                                                                                                                                                                                                                          100714         000001";

            var resultado = leitor.ObterHeader(valorTesteRegistro);

            Assert.AreEqual(resultado.CodigoDoBanco, "237");
        }

        [TestMethod]
        public void TestRegistroArquivoRetornoCnab400BancoBradesco()
        {
            LeitorRetornoCnab400Bradesco leitor = new LeitorRetornoCnab400Bradesco(null);

            string valorTesteRegistro =
                "10210623013000170000000901923000105100001217010               000000002300000684440000000000000000000000000906090714000121701000000000230000068444100714000000002902410400162  000000000000000000000000000000000000000000000000000000000000000000000000000000000000002902400000000000000000000000000   100714             00000000000000                                                                  000002";

            var resultado = leitor.ObterRegistrosDetalhe(valorTesteRegistro);

            Assert.AreEqual(resultado.NumeroSequencial, 2);
        }

        [TestMethod]
        public void TestTrailerArquivoRetornoCnab400BancoBradesco()
        {
            LeitorRetornoCnab400Bradesco leitor = new LeitorRetornoCnab400Bradesco(null);

            string valorTesteRegistro =
                "9201237          000008850000003044113700000131          00000000000000000000000122998000030000001229980000000000000000000000000000000000000000000000000000000000000000000000000000000000000                                                                                                                                                                              00000000000000000000000         000006";

            var resultado = leitor.ObterTrailer(valorTesteRegistro);

            Assert.AreEqual(resultado.NumeroSequencial, 6);
        }

        #endregion
    }
}
