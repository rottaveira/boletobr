using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BoletoBr.Arquivo;
using BoletoBr.Arquivo.CNAB240.Remessa;
using BoletoBr.Enums;
using BoletoBr.Fabricas;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BoletoBr.UnitTests.TestsBancosRemessa
{
    [TestClass]
    public class TestRemessaAbc
    {
        [TestMethod]
        public void TestGeracaoArquivoRemessaSicoob()
        {

            var database = 2010;
            var dataatual = 2012;
            var periodo = 2;
            var resultado = (dataatual - dataatual) % periodo;


            var tiporemessa = new Remessa(Remessa.EnumTipoAmbiemte.Homologacao, EnumCodigoOcorrenciaRemessa.Registro, "2");
            var banco = Fabricas.BancoFactory.ObterBanco("246", "0");
            var contaBancariaCedente = new ContaBancaria("0001", "9", "107228", "5");
            var cedente = new Cedente("28796", "1234", 2, "99.999.999/9999-99", "Razão Social X", contaBancariaCedente,
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

            var boleto = new Boleto(carteira, cedente, sacado, tiporemessa)
            {
                NumeroDocumento = "106",
                ValorBoleto = Convert.ToDecimal(465.82),
                IdentificadorInternoBoleto = "0004309540",
                DataVencimento = new DateTime(2016, 11, 30),
                BancoBoleto = banco,
                Especie = banco.ObtemEspecieDocumento(EnumEspecieDocumento.DuplicataMercantil),
                JurosMora = 2.5M,
                DataJurosMora = DateTime.Now
            };
            boleto.PercentualJurosMora = decimal.Round(boleto.PercentualJurosMora.GetValueOrDefault(), 4);
            var listaBoleto = new List<Boleto>();

            listaBoleto.Add(boleto);

            banco.FormatarBoleto(boleto);

            #region GERAÇÃO 1

            var remessa = new RemessaCnab240();

            //var listaDetalhes = new List<DetalheRemessaCnab240>();

            remessa.Header = new HeaderRemessaCnab240(listaBoleto.FirstOrDefault(), 1);

            //var detalheSegmentoP = new DetalheSegmentoPRemessaCnab240(boleto)
            //{
            //    CodigoCedente = "123456",
            //    NossoNumero = "123456",
            //    NumeroDocumento = "123456",
            //    CodigoOcorrencia = new CodigoOcorrencia(01),
            //    Especie = banco.ObtemEspecieDocumento(EnumEspecieDocumento.DuplicataMercantil),
            //    Aceite = "N",
            //    DataVencimento = new DateTime(2014, 10, 01),
            //    ValorBoleto = (decimal)100.51,
            //};

            remessa.Lotes = new List<LoteRemessaCnab240> { };

            var loteAdd = new LoteRemessaCnab240();
            loteAdd.HeaderLote = new HeaderLoteRemessaCnab240(listaBoleto.FirstOrDefault(), 1);

            loteAdd.TrailerLote = new TrailerLoteRemessaCnab240(1);

            //loteAdd.RegistrosDetalheSegmentos = new List<DetalheRemessaCnab240>();
            //var detalheRemessaAdd = new DetalheRemessaCnab240();
            //detalheRemessaAdd.SegmentoP = detalheSegmentoP;
            //loteAdd.RegistrosDetalheSegmentos.Add(detalheRemessaAdd);

            remessa.Lotes.Add(loteAdd);

            remessa.Trailer = new TrailerRemessaCnab240(1, 1);

            #endregion GERAÇÃO 1

            var fabricaRemessa = new RemessaFactory();

            var remessaPronta = fabricaRemessa.GerarRemessa(remessa.Header, loteAdd.HeaderLote, listaBoleto, loteAdd.TrailerLote, remessa.Trailer);

            var escritor = EscritorArquivoRemessaFactory.ObterEscritorRemessa(remessa);

            var linhasEscrever = escritor.EscreverTexto(remessaPronta);

            var path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            var data = String.Format("{0}_{1}", DateTime.Now.ToString("ddMMyyyy"), DateTime.Now.ToString("HHmmss"));

            var nomeArquivo = string.Format("{0}{1}{2}{3}", banco.CodigoBanco, @"_REMESSA_", data, ".txt");

            StringBuilder sb = new StringBuilder();
            foreach (var linha in linhasEscrever)
            {
                sb.AppendLine(linha);
            }

            File.WriteAllLines($@"{path}\\{nomeArquivo}", linhasEscrever.ToArray());

        }
    }
}
