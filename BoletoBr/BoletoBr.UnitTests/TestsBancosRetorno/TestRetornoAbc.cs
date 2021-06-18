 
using BoletoBr.Bancos;
using BoletoBr.Bancos.Abc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BoletoBr.UnitTests.Tests.BancosRetorno
{
    [TestClass]
    public class TestRetornoAbc
    {
        #region CNAB 240

        [TestMethod]
        public void TestHeaderArquivoRetornoCnab240BancoAbc()
        {
            LeitorRetornoCnab240Abc leitor = new LeitorRetornoCnab240Abc(null);

            string valorTesteRegistro =
                "24600000         2106230130001700000000000000001234000000000000000012340RMEX CONSTRUTORA E INCORPORADOBANCO ABC BRASIL                        22507201400562700036204000000                    RETORNO-PRODUCAO                                 ";
            
            var resultado = leitor.ObterHeader(valorTesteRegistro);

            Assert.AreEqual(resultado.NomeDoBeneficiario, "RMEX CONSTRUTORA E INCORPORADO");
        }

        [TestMethod]
        public void TestHeaderLoteArquivoRetornoCnab240BancoAbc()
        {
            LeitorRetornoCnab240Abc leitor = new LeitorRetornoCnab240Abc(null);

            string valorTesteRegistro =
                "24600011T0100030 20106230130001700000000000000000000001839220301600000000RMEX CONSTRUTORA E INCORPORADO                                                                                00000362250720140000000000                          00   ";

            var resultado = leitor.ObterHeaderLote(valorTesteRegistro);

            Assert.AreEqual(resultado.TipoOperacao, "T");
        }

        [TestMethod]
        public void TestDetalheSegmentoTArquivoRetornoCnab240BancoAbc()
        {
            LeitorRetornoCnab240Abc leitor = new LeitorRetornoCnab240Abc(null);

            string valorTesteRegistro =
                "2460001300001T 060000002030160000000   110000000588426997100002055010    1007201400000000003818100004222000002055010              090000000000000000NATANNI SANTANA PINHEIRO                          000000000000320030100                     ";

            var resultado = leitor.ObterRegistrosDetalheT(valorTesteRegistro);

            Assert.AreEqual(resultado.CodigoSegmento, "T");
        }

        [TestMethod]
        public void TestDetalheSegmentoUArquivoRetornoCnab240BancoAbc()
        {
            LeitorRetornoCnab240Abc leitor = new LeitorRetornoCnab240Abc(null);

            string valorTesteRegistro =
                "2460001300790U 09000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000250720140000000000000000000000000000000000000000000000000000000000000000000000000000000000000000       ";

            var resultado = leitor.ObterRegistrosDetalheU(valorTesteRegistro);

            Assert.AreEqual(resultado.CodigoSegmento, "U");
        }

        [TestMethod]
        public void TestTrailerLoteArquivoRetornoCnab240BancoAbc()
        {
            LeitorRetornoCnab240Abc leitor = new LeitorRetornoCnab240Abc(null);

            string valorTesteRegistro =
                "24600015         00079600000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000                                                                                                                             ";

            var resultado = leitor.ObterTrailerLote(valorTesteRegistro);

            Assert.AreEqual(resultado.QtdRegistrosLote, 796);
        }

        [TestMethod]
        public void TestTrailerArquivoRetornoCnab240BancoAbc()
        {
            LeitorRetornoCnab240Abc leitor = new LeitorRetornoCnab240Abc(null);

            string valorTesteRegistro =
                "24699999         000001000798                                                                                                                                                                                                                ";

            var resultado = leitor.ObterTrailer(valorTesteRegistro);

            Assert.AreEqual(resultado.LoteServico, "9999");
        }

        #endregion
         
    }
}

