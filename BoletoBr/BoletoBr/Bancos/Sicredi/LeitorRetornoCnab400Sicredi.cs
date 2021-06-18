using System;
using System.Collections.Generic;
using System.Linq; 
using BoletoBr.Dominio;
using BoletoBr.Interfaces;

namespace BoletoBr.Bancos.BancoSicredi
{
    public class LeitorRetornoCnab400Sicredi : ILeitorArquivoRetornoCnab400
    {
        private readonly List<string> _linhasArquivo;

        public LeitorRetornoCnab400Sicredi(List<string> linhasArquivo)
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
            objRetornar.LiteralServico = linha.ExtrairValorDaLinha(12, 26);
            objRetornar.ContaCorrente = linha.ExtrairValorDaLinha(27, 31);
            objRetornar.CpfCnpjBeneficiario = linha.ExtrairValorDaLinha(32, 45);
            /* Brancos */
            objRetornar.CodigoDoBanco = linha.ExtrairValorDaLinha(77, 79);
            objRetornar.NomeDoBanco = linha.ExtrairValorDaLinha(80, 94);
            objRetornar.DataGeracaoGravacao = (DateTime)linha.ExtrairValorDaLinha(95, 102).ToString().ToDateTimeFromAaaaMmDd();
            /* Brancos */
            objRetornar.SequencialRetorno = linha.ExtrairValorDaLinha(111, 117);
            /* Brancos */
            objRetornar.VersaoSicredi = linha.ExtrairValorDaLinha(390, 394);
            /* Brancos */
            objRetornar.NumeroSequencial = linha.ExtrairValorDaLinha(395, 400);

            return objRetornar;
        }

        public DetalheRetornoCnab400 ObterRegistrosDetalhe(string linha)
        {
            var objRetornar = new DetalheRetornoCnab400();

            objRetornar.CodigoDoRegistro = linha.ExtrairValorDaLinha(1, 1).BoletoBrToInt();
            objRetornar.TipoCarteiraSicredi = linha.ExtrairValorDaLinha(2, 2);
            // Brancos
            objRetornar.TipoCobrancaSicredi = linha.ExtrairValorDaLinha(14, 14);
            objRetornar.CodigoDoPagadorNaCooperativa = linha.ExtrairValorDaLinha(15, 19);
            objRetornar.CodigoDoPagadorNoAssociado = linha.ExtrairValorDaLinha(20, 24);
            objRetornar.IndicadorBoletoDDA = linha.ExtrairValorDaLinha(25, 25);
            // Brancos
            objRetornar.NossoNumero = linha.ExtrairValorDaLinha(48, 62);
            // Brancos
            objRetornar.CodigoDeOcorrencia = linha.ExtrairValorDaLinha(109, 110).ToString().BoletoBrToInt();
            objRetornar.MotivoCodigoOcorrencia = linha.ExtrairValorDaLinha(109, 110).ToString();
            objRetornar.DataDaOcorrencia = (DateTime)linha.ExtrairValorDaLinha(111, 116).ToString().ToDateTimeFromDdMmAa();
            objRetornar.SeuNumero = linha.ExtrairValorDaLinha(117, 126);
            // Brancos 
            objRetornar.DataDeVencimento = linha.ExtrairValorDaLinha(147, 152).BoletoBrToStringSafe().ToDateTimeFromDdMmAa() ?? DateTime.MinValue;
            objRetornar.ValorDoTituloParcela = linha.ExtrairValorDaLinha(153, 165).BoletoBrToDecimal() / 100;
            // Brancos 
            objRetornar.Especie= linha.ExtrairValorDaLinha(175, 175);
            objRetornar.ValorDespesas= linha.ExtrairValorDaLinha(176, 188).BoletoBrToDecimal()/100; /*Despesas de cobrança*/
            objRetornar.ValorOutrasDespesas= linha.ExtrairValorDaLinha(189, 201).BoletoBrToDecimal()/100;/* Despesas de custas de protesto*/
            // Brancos 
            objRetornar.ValorAbatimento = linha.ExtrairValorDaLinha(228, 240).BoletoBrToDecimal()/100;
            objRetornar.ValorDesconto = linha.ExtrairValorDaLinha(241, 253).BoletoBrToDecimal()/100;
            objRetornar.ValorLiquidoRecebido = linha.ExtrairValorDaLinha(254, 266).BoletoBrToDecimal()/100;
            objRetornar.ValorJurosDeMora = linha.ExtrairValorDaLinha(267, 279).BoletoBrToDecimal()/100;
            objRetornar.ValorMulta= linha.ExtrairValorDaLinha(280, 292).BoletoBrToDecimal()/100;
            // Brancos 
            objRetornar.CodigoOcorrencia1 = linha.ExtrairValorDaLinha(295, 295);
            // Brancos 
            objRetornar.MotivoDaOcorrencia = linha.ExtrairValorDaLinha(319, 328).BoletoBrToInt();
            objRetornar.DataPrevistaLancamentoConta = linha.ExtrairValorDaLinha(329, 336).BoletoBrToStringSafe().ToDateTimeFromAaaaMmDd();
            objRetornar.DataDeCredito = objRetornar.DataPrevistaLancamentoConta.GetValueOrDefault();
            // Brancos 
            objRetornar.NumeroSequencial = linha.ExtrairValorDaLinha(395, 400).BoletoBrToInt();

            return objRetornar;
        }

        public TrailerRetornoCnab400 ObterTrailer(string linha)
        {
            var objRetornar = new TrailerRetornoCnab400();

            objRetornar.CodigoDoRegistro = linha.ExtrairValorDaLinha(1, 1).BoletoBrToInt();
            objRetornar.CodigoDeRetorno = linha.ExtrairValorDaLinha(2, 2).BoletoBrToInt();
            objRetornar.CodigoDoBanco = linha.ExtrairValorDaLinha(3, 5);
            objRetornar.CodigoBeneficiario = linha.ExtrairValorDaLinha(6, 10).BoletoBrToInt();
            // Brancos
            objRetornar.NumeroSequencial = linha.ExtrairValorDaLinha(395, 400).BoletoBrToInt();

            return objRetornar;
        }
    }
}
