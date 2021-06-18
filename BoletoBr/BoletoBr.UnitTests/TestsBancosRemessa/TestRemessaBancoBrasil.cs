using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BoletoBr.Arquivo;
using BoletoBr.Arquivo.CNAB240.Remessa;
using BoletoBr.Arquivo.CNAB400.Remessa;
using BoletoBr.Bancos.Brasil;
using BoletoBr.Dominio;
using BoletoBr.Enums;
using BoletoBr.Fabricas;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BoletoBr.UnitTests.TestsBancosRemessa
{
    [TestClass]
    public class TestRemessaBancoBrasil
    {
        [TestMethod]
        public void TestGerarHeaderArquivoRemessaBancoBrasilCnab400()
        {
            var remessa = new Remessa(Remessa.EnumTipoAmbiemte.Homologacao, EnumCodigoOcorrenciaRemessa.Registro, "2");

            var banco = Fabricas.BancoFactory.ObterBanco("001", "9");

            var contaBancariaCedente = new ContaBancaria("2374", "4", "0165199", "4");

            var cedente = new Cedente("9999999", "000123", 0, "99.999.999/9999-99", "Razão Social X", contaBancariaCedente, null);

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

            var carteira = new CarteiraCobranca { Codigo = "16" };

            var boleto = new Boleto(carteira, cedente, sacado, remessa)
            {
                NumeroDocumento = "1",
                ValorBoleto = Convert.ToDecimal(221.40),
                IdentificadorInternoBoleto = "1",
                DataVencimento = new DateTime(2014, 07, 10),
                Especie = banco.ObtemEspecieDocumento(EnumEspecieDocumento.DuplicataMercantil),
                TipoModalidade = "21"
            };

            banco.FormatarBoleto(boleto);

            //const int numeroRemessa = 1;
            //const int numeroRegistro = 1;

            //var escritor = new EscritorRemessaCnab400BancoDoBrasil();

            //var linhasEscrever = escritor.EscreverHeader(boleto, numeroRemessa, numeroRegistro);

            //var path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            //var data = DateTime.Now.ToString("ddMMyyyy");

            //var nomeArquivo = string.Format("{0}{1}{2}{3}{4}{5}{6}", banco.CodigoBanco, "-", banco.NomeBanco, "_", data, @"_HEADER", ".txt");

            //var arquivo = new System.IO.StreamWriter(path + @"\" + nomeArquivo, true);
            //arquivo.WriteLine(linhasEscrever);

            //arquivo.Close();
        }

        [TestMethod]
        public void TestGerarDetalheArquivoRemessaBancoBrasilCnab400()
        {
            var remessa = new Remessa(Remessa.EnumTipoAmbiemte.Homologacao, EnumCodigoOcorrenciaRemessa.Registro, "2");

            var banco = Fabricas.BancoFactory.ObterBanco("001", "9");

            var contaBancariaCedente = new ContaBancaria("2374", "4", "0165199", "4");

            var cedente = new Cedente("9999999", "000123", 0, "99.999.999/9999-99", "Razão Social X", contaBancariaCedente, null);

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

            var carteira = new CarteiraCobranca { Codigo = "16" };

            var boleto = new Boleto(carteira, cedente, sacado, remessa)
            {
                NumeroDocumento = "1",
                ValorBoleto = Convert.ToDecimal(221.40),
                IdentificadorInternoBoleto = "1",
                DataVencimento = new DateTime(2014, 07, 10),
                Especie = banco.ObtemEspecieDocumento(EnumEspecieDocumento.DuplicataMercantil),
                TipoModalidade = "21",
                CodigoOcorrenciaRemessa = new CodigoOcorrencia(01),
                BancoBoleto = banco
            };

            banco.FormatarBoleto(boleto);

            var remessaEscrever = new RemessaCnab400();

            remessaEscrever.Header = new HeaderRemessaCnab400(boleto, 1, 1, DateTime.Now);
            var detalheIndividual = new DetalheRemessaCnab400(boleto, 1);
            remessaEscrever.RegistrosDetalhe = new List<DetalheRemessaCnab400>
            {
                detalheIndividual
            };
            var escritor = new EscritorRemessaCnab400BancoDoBrasil(remessaEscrever);

            var linhasEscrever = escritor.EscreverTexto(remessaEscrever);

            //const int numeroRegistro = 1;

            //var escritor = new EscritorRemessaCnab400BancoDoBrasil();

            //var linhasEscrever = escritor.EscreverDetalhe(boleto, numeroRegistro);

            //var path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            //var data = DateTime.Now.ToString("ddMMyyyy");

            //var nomeArquivo = string.Format("{0}{1}{2}{3}{4}{5}{6}", banco.CodigoBanco, "-", banco.NomeBanco, "_", data, @"_REGISTRO_DETALHE", ".txt");

            //var arquivo = new System.IO.StreamWriter(path + @"\" + nomeArquivo, true);
            //arquivo.WriteLine(linhasEscrever);

            //arquivo.Close();
        }

        [TestMethod]
        public void TestGerarTrailerArquivoRemessaBancoBrasilCnab400()
        {
            var remessa = new Remessa(Remessa.EnumTipoAmbiemte.Homologacao, EnumCodigoOcorrenciaRemessa.Registro, "2");

            var banco = Fabricas.BancoFactory.ObterBanco("001", "9");

            var contaBancariaCedente = new ContaBancaria("2374", "4", "0165199", "4");

            var cedente = new Cedente("9999999", "000123", 0, "99.999.999/9999-99", "Razão Social X", contaBancariaCedente, null);

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

            var carteira = new CarteiraCobranca { Codigo = "16" };

            var boleto = new Boleto(carteira, cedente, sacado, remessa)
            {
                NumeroDocumento = "1",
                ValorBoleto = Convert.ToDecimal(221.40),
                IdentificadorInternoBoleto = "1",
                DataVencimento = new DateTime(2014, 07, 10),
                Especie = banco.ObtemEspecieDocumento(EnumEspecieDocumento.DuplicataMercantil),
                TipoModalidade = "21"
            };

            banco.FormatarBoleto(boleto);

            //const int numeroRegistro = 1;

            //var escritor = new EscritorRemessaCnab400BancoDoBrasil();

            //var linhasEscrever = escritor.EscreverTrailer(numeroRegistro);

            //var path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            //var data = DateTime.Now.ToString("ddMMyyyy");

            //var nomeArquivo = string.Format("{0}{1}{2}{3}{4}{5}{6}", banco.CodigoBanco, "-", banco.NomeBanco, "_", data, @"_TRAILER", ".txt");

            //var arquivo = new System.IO.StreamWriter(path + @"\" + nomeArquivo, true);
            //arquivo.WriteLine(linhasEscrever);

            //arquivo.Close();
        }


        [TestMethod]
        public void TestGerarRemessaPagamento()
        {
            var contaBancariaCedente = new ContaBancaria("1234", "8", "12345", "6");
            var empresaPagadora = new BoletoBr.Cedente("7980171", "7980171", 0, "99.999.999/9999-99", "Razao Social X", contaBancariaCedente, null);
            empresaPagadora.EnderecoCedente = new Endereco()
            {
                Bairro = "Jardins",
                Cep = "75690000",
                Cidade = "Goiabalândia",
                Complemento = "EDF EXECUTIVO",
                Logradouro = "Rua x",
                Numero = "25",
                SiglaUf = "GO",
            };

            var objEndereco = new Endereco()
            {
                Bairro = "Trenzim",
                Cep = "75690000",
                Cidade = "Mangalandia",
                Complemento = "Galho esquerdo",
                Logradouro = "Rua 3",
                Numero = "5",
                SiglaUf = "GO",
            };
            var contaBancariaFavorecido = new BoletoBr.ContaBancaria("4343", "0", "35432", "2");
            var favorecido = new BoletoBr.Sacado("012.365.489-01", "1", objEndereco, contaBancariaFavorecido);
            favorecido.Nome = "Fulano da silva";
            var bancoEmpresa = BancoFactory.ObterBanco("001", "");
            var bancoFavorecido = BancoFactory.ObterBanco("237", "");
            var pagamento = new BoletoBr.Pagamento()
            {
                BancoEmpresa = bancoEmpresa,
                BancoFavorecido = bancoFavorecido,
                CodigoBanco = bancoEmpresa.CodigoBanco,
                CodigoBancoFavorecido = bancoFavorecido.CodigoBanco,
                CodigoCamaraCentralizadora = "018",
                CodigoFinalidadeDoc = "",
                CodigoFinalidadeTed = "5",
                FinalidadePagamento = "CC",
                DataVencimento = new DateTime(2021, 4, 21),
                Empresa = empresaPagadora,
                Favorecido = favorecido,
                ValorPagamento = 5M,
                ValorDesconto = 0,
                ValorJurosMora = 0,
                ValorMulta = 0,
                CodigoConvenio = "7980171",
                SeuNumero = "1015",
                TipoServico = "0",
                FormaDeLancamento = "03",

                 ValorTitulo = 5M,
                CodBarras = "24691859100000299845004110028796200000000150"
            };

            var remessa = new RemessaCnab240
            {
                Header = new HeaderRemessaCnab240(pagamento, 1)
            };

            var loteRemessa = new LoteRemessaCnab240
            {
                HeaderLote = new HeaderLoteRemessaCnab240(pagamento, 1),
                TrailerLote = new TrailerLoteRemessaCnab240(4)
            };

            remessa.Trailer = new TrailerRemessaCnab240(1, 6);

            var escritor = EscritorArquivoRemessaFactory.ObterEscritorRemessaPagamento(remessa);
            var listaBoletoBrRemessa = new List<Pagamento>() { pagamento };
            var fabricaRemessa = new RemessaFactory();
            var remessaPronta = fabricaRemessa.GerarRemessa(remessa.Header, loteRemessa.HeaderLote, listaBoletoBrRemessa, loteRemessa.TrailerLote, remessa.Trailer);
            var linhasEscrever = escritor.EscreverTexto(remessaPronta);


            StringBuilder sb = new StringBuilder();
            foreach (var linha in linhasEscrever)
            {
                sb.AppendLine(linha);
            }
              
        }
    }
}
