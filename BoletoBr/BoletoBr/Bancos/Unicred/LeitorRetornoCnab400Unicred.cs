using System;
using System.Collections.Generic;
using System.Linq;
using BoletoBr.Dominio;
using BoletoBr.Interfaces;

namespace BoletoBr.Bancos.BancoUnicred
{
    public class LeitorRetornoCnab400Unicred : ILeitorArquivoRetornoCnab400
    {
        private readonly List<string> _linhasArquivo;

        public LeitorRetornoCnab400Unicred(List<string> linhasArquivo)
        {
            _linhasArquivo = linhasArquivo;
        }

        public RetornoCnab400 ProcessarRetorno()
        {
            /* Validações */
            #region Validações

            ValidaArquivoRetorno();

            #endregion

            var objRetornar = new RetornoCnab400();
            objRetornar.RegistrosDetalhe = new List<DetalheRetornoCnab400>();

            foreach (var linhaAtual in _linhasArquivo)
            {
                if (linhaAtual.ExtrairValorDaLinha(1, 1) == "0")
                {
                    objRetornar.Header = ObterHeader(linhaAtual);
                }
                if (linhaAtual.ExtrairValorDaLinha(1, 1) == "1")
                {
                    var objDetalhe = ObterRegistrosDetalhe(linhaAtual);
                    objRetornar.RegistrosDetalhe.Add(objDetalhe);
                }
                if (linhaAtual.ExtrairValorDaLinha(1, 1) == "9")
                {
                    objRetornar.Trailer = ObterTrailer(linhaAtual);
                }
            }

            return objRetornar;
        }

        public RetornoCnab400 ProcessarRetorno(TipoArquivo tipoArquivo)
        {
            throw new NotImplementedException();
        }

        public void ValidaArquivoRetorno()
        {
            if (_linhasArquivo == null)
                throw new Exception("Dados do arquivo de retorno estão nulos. Impossível processar.");

            if (_linhasArquivo.Count <= 0)
                throw new Exception("Dados do arquivo de retorno não estão corretos. Impossível processar.");

            if (_linhasArquivo.Count < 3)
                throw new Exception("Dados do arquivo de retorno não contém o mínimo de 3 linhas. Impossível processar.");

            var qtdLinhasHeader =
                _linhasArquivo.Count(wh => wh.ExtrairValorDaLinha(1, 1) == "0");

            if (qtdLinhasHeader <= 0)
                throw new Exception("Não foi encontrado HEADER do arquivo de retorno.");

            if (qtdLinhasHeader > 1)
                throw new Exception("Não é permitido mais de um HEADER no arquivo de retorno.");

            var qtdLinhasDetalhe = _linhasArquivo.Count(wh => wh.ExtrairValorDaLinha(1, 1) == "1");

            if (qtdLinhasDetalhe <= 0)
                throw new Exception("Não foi encontrado DETALHE do arquivo de retorno.");

            var qtdLinhasTrailer = _linhasArquivo.Count(wh => wh.ExtrairValorDaLinha(1, 1) == "9");

            if (qtdLinhasTrailer <= 0)
                throw new Exception("Não foi encontrado TRAILER do arquivo de retorno.");

            if (qtdLinhasTrailer > 1)
                throw new Exception("Não é permitido mais de um TRAILER no arquivo de retorno.");
        }

        public HeaderRetornoCnab400 ObterHeader(string linha)
        {
            var objRetornar = new HeaderRetornoCnab400();

            objRetornar.CodigoDoRegistro = linha.ExtrairValorDaLinha(1, 1).BoletoBrToInt();
            objRetornar.TipoRetorno = linha.ExtrairValorDaLinha(2, 2);
            objRetornar.LiteralRetorno = linha.ExtrairValorDaLinha(3, 9);
            objRetornar.CodigoDoServico = linha.ExtrairValorDaLinha(10, 11);
            objRetornar.LiteralServico = linha.ExtrairValorDaLinha(12, 19);
            objRetornar.CodigoAgenciaCedente = linha.ExtrairValorDaLinha(27, 30).BoletoBrToInt();
            objRetornar.DvAgenciaCedente = linha.ExtrairValorDaLinha(31, 31);
            objRetornar.ContaCorrente = linha.ExtrairValorDaLinha(32, 39);
            objRetornar.DvContaCorrente = linha.ExtrairValorDaLinha(40, 40);

            /* Brancos */
            objRetornar.CodigoDoBanco = linha.ExtrairValorDaLinha(77, 79);
            objRetornar.NomeDoBanco = linha.ExtrairValorDaLinha(80, 94);
            objRetornar.DataGeracaoGravacao = (DateTime)linha.ExtrairValorDaLinha(95, 100).ToString().ToDateTimeFromDdMmAa().GetValueOrDefault();

            /* Brancos */
            objRetornar.CodigoDoBeneficiario = linha.ExtrairValorDaLinha(108, 121);
            objRetornar.NumeroSequencial = linha.ExtrairValorDaLinha(395, 400);

            return objRetornar;
        }

        public DetalheRetornoCnab400 ObterRegistrosDetalhe(string linha)
        {
            var objRetornar = new DetalheRetornoCnab400();

            objRetornar.CodigoDoRegistro = linha.ExtrairValorDaLinha(1, 1).BoletoBrToInt();
            objRetornar.CodigoDeInscricao = linha.ExtrairValorDaLinha(2, 3).BoletoBrToInt();
            objRetornar.NumeroInscricao = linha.ExtrairValorDaLinha(4, 17).BoletoBrToInt();
            // Brancos
            objRetornar.AgenciaCobradora = linha.ExtrairValorDaLinha(18, 21).BoletoBrToInt();
            objRetornar.DvAgenciaCobradora = linha.ExtrairValorDaLinha(22, 22);
            objRetornar.ContaCorrente = linha.ExtrairValorDaLinha(23, 30);
            objRetornar.DvContaCorrente = linha.ExtrairValorDaLinha(31, 31);
            objRetornar.CodigoDoBeneficiario = linha.ExtrairValorDaLinha(32, 45).BoletoBrToInt();
            objRetornar.NossoNumero = linha.ExtrairValorDaLinha(46, 62);

            // Brancos
            objRetornar.CodigoMovimento = linha.ExtrairValorDaLinha(109, 110).ToString();
            objRetornar.DataLiquidacao = (DateTime)linha.ExtrairValorDaLinha(111, 116).ToString().ToDateTimeFromDdMmAa();

            // Brancos 
            objRetornar.DataDeVencimento = linha.ExtrairValorDaLinha(147, 152).BoletoBrToStringSafe().ToDateTimeFromDdMmAa() ?? DateTime.MinValue;
            objRetornar.ValorDoTituloParcela = linha.ExtrairValorDaLinha(153, 165).BoletoBrToDecimal() / 100;
            // Brancos 
            objRetornar.DataPrevistaLancamentoConta = linha.ExtrairValorDaLinha(176, 181).BoletoBrToStringSafe().ToDateTimeFromDdMmAa();
            objRetornar.ValorTarifa = linha.ExtrairValorDaLinha(182, 188).BoletoBrToDecimal() / 100; /*Despesas de cobrança*/
            // Brancos 
            objRetornar.ValorAbatimento = linha.ExtrairValorDaLinha(228, 240).BoletoBrToDecimal() / 100;
            objRetornar.ValorDesconto = linha.ExtrairValorDaLinha(241, 253).BoletoBrToDecimal() / 100;
            objRetornar.ValorPrincipal = linha.ExtrairValorDaLinha(254, 266).BoletoBrToDecimal() / 100;
            objRetornar.ValorJurosDeMora = linha.ExtrairValorDaLinha(267, 279).BoletoBrToDecimal() / 100;
            objRetornar.SeuNumero = linha.ExtrairValorDaLinha(280, 305);
            // Brancos 
            objRetornar.ValorLiquidoRecebido = linha.ExtrairValorDaLinha(306, 318).BoletoBrToDecimal() / 100;

            // Brancos 
            objRetornar.ComplementoMovimento = linha.ExtrairValorDaLinha(319, 326);
            objRetornar.CodigoDeOcorrencia = linha.ExtrairValorDaLinha(327, 328).BoletoBrToInt();
            // Brancos 
            objRetornar.NumeroSequencial = linha.ExtrairValorDaLinha(395, 400).BoletoBrToInt();

            return objRetornar;
        }

        public TrailerRetornoCnab400 ObterTrailer(string linha)
        {
            var objRetornar = new TrailerRetornoCnab400();

            objRetornar.CodigoDoRegistro = linha.ExtrairValorDaLinha(1, 1).BoletoBrToInt();
            // Brancos
            objRetornar.NumeroSequencial = linha.ExtrairValorDaLinha(395, 400).BoletoBrToInt();

            return objRetornar;
        }
    }
}
