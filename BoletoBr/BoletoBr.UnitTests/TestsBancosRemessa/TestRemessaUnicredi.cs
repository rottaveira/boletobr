using System;
using System.Collections.Generic;
using BoletoBr.Arquivo;
using BoletoBr.Arquivo.CNAB400.Remessa;
using BoletoBr.Bancos.UniCred; 
using BoletoBr.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BoletoBr.UnitTests.TestsBancosRemessa
{
    [TestClass]
    public class TestRemessaUnicred
    {
        [TestMethod]
        public void TestGerarArquivoRemessaUnicredCnab400()
        {
            var remessa = new Remessa(Remessa.EnumTipoAmbiemte.Homologacao, EnumCodigoOcorrenciaRemessa.Registro, "2");
            var banco = Fabricas.BancoFactory.ObterBanco("136");
            var contaBancariaCedente = new ContaBancaria("0136", "3", "00623", "9");
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
                DataVencimento = DateTime.Now.AddDays(10),
                CodigoDeTransmissao = "00200643279401300369",
                BancoBoleto = banco,
                CedenteBoleto = cedente,
                CarteiraCobranca = carteira,
                Especie = banco.ObtemEspecieDocumento(EnumEspecieDocumento.NotaPromissoria),
                Aceite = "S"
            };

            banco.FormatarBoleto(boleto);

            var cnab400 = new RemessaCnab400()
            {
                Header = new HeaderRemessaCnab400(boleto, 1, 1),
                RegistrosDetalhe = new List<DetalheRemessaCnab400>() { new DetalheRemessaCnab400(boleto,2)},
                Trailer = new TrailerRemessaCnab400(12345,3)

            };

            var escritor = new EscritorRemessaCnab400Unicred(cnab400);

            var linhasEscrever = escritor.EscreverTexto(cnab400);

            var path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            var data = DateTime.Now.ToString("d").Replace("/", "");

            var nomeArquivo = string.Format("{0}{1}{2}{3}{4}{5}{6}", banco.CodigoBanco, "-", banco.NomeBanco, "_", data, @"_REMESSA", ".txt");

            var arquivo = new System.IO.StreamWriter(path + @"\" + nomeArquivo, true);
            arquivo.Write(string.Join(Environment.NewLine,linhasEscrever));

            arquivo.Close();
        }
         
    }
}
