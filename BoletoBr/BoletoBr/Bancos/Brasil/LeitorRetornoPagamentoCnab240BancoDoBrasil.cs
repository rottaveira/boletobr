using System;
using System.Collections.Generic;
using System.Linq;
using BoletoBr.Arquivo.CNAB240.Retorno;
using BoletoBr.Interfaces;

namespace BoletoBr.Bancos.Brasil
{
    public class LeitorRetornoPagamentoCnab240BancoDoBrasil : ILeitorArquivoRetornoCnab240
    {
        private readonly List<string> _linhasArquivo;

        public LeitorRetornoPagamentoCnab240BancoDoBrasil(List<string> linhasArquivo)
        {
            _linhasArquivo = linhasArquivo;
        }

        public RetornoCnab240 ProcessarRetorno()
        {
            /* Validações */
            #region Validações
            ValidaArquivoRetorno();
            #endregion

            var objRetornar = new RetornoCnab240();

            LoteRetornoCnab240 ultimoLoteIdentificado = null;
            DetalheRetornoCnab240 ultimoRegistroIdentificado = null;

            foreach (var linhaAtual in _linhasArquivo)
            {
                /* Header de arquivo */
                if (linhaAtual.ExtrairValorDaLinha(8, 8) == "0")
                    objRetornar.Header = ObterHeader(linhaAtual);

                /* Header de Lote */
                if (linhaAtual.ExtrairValorDaLinha(8, 8) == "1")
                {
                    ultimoLoteIdentificado = new LoteRetornoCnab240();
                    ultimoRegistroIdentificado = new DetalheRetornoCnab240();
                    objRetornar.Lotes.Add(ultimoLoteIdentificado);

                    ultimoLoteIdentificado.HeaderLote = ObterHeaderLote(linhaAtual);
                }
                if (linhaAtual.ExtrairValorDaLinha(8, 8) == "3")
                {
                    if (linhaAtual.ExtrairValorDaLinha(14, 14) == "A")
                    {
                        if (ultimoLoteIdentificado == null)
                            throw new Exception("Não foi encontrado header de lote para o segmento atual.");

                        ultimoRegistroIdentificado = new DetalheRetornoCnab240();
                        ultimoLoteIdentificado.RegistrosDetalheSegmentos.Add(ultimoRegistroIdentificado);

                        ultimoRegistroIdentificado.SegmentoA = ObterRegistrosDetalheA(linhaAtual);
                    }
                    if (linhaAtual.ExtrairValorDaLinha(14, 14) == "B")
                    {
                        if (ultimoLoteIdentificado == null)
                            throw new Exception("Não foi encontrado header de lote para o segmento atual.");

                        ultimoRegistroIdentificado.SegmentoB = ObterRegistrosDetalheB(linhaAtual);
                    }
                    if (linhaAtual.ExtrairValorDaLinha(14, 14) == "J")
                    {
                        if (ultimoLoteIdentificado == null)
                            throw new Exception("Não foi encontrado header de lote para o segmento atual.");

                        ultimoRegistroIdentificado.SegmentoJ = ObterRegistrosDetalheJ(linhaAtual);
                    }
                }

                /* Trailer de Lote */
                if (linhaAtual.ExtrairValorDaLinha(8, 8) == "5")
                    if (ultimoLoteIdentificado != null)
                    {
                        ultimoLoteIdentificado.TrailerLote = ObterTrailerLote(linhaAtual);
                    }

                /* Trailer de arquivo */
                if (linhaAtual.ExtrairValorDaLinha(8, 8) == "9")
                    objRetornar.Trailer = ObterTrailer(linhaAtual);
            }

            return objRetornar;
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
                _linhasArquivo.Count(wh => wh.ExtrairValorDaLinha(8, 8) == "0");

            if (qtdLinhasHeader <= 0)
                throw new Exception("Não foi encontrado HEADER do arquivo de retorno.");

            if (qtdLinhasHeader > 1)
                throw new Exception("Não é permitido mais de um HEADER no arquivo de retorno.");

            var qtdLinhasHeaderLote = _linhasArquivo.Count(wh => wh.ExtrairValorDaLinha(8, 8) == "1");

            if (qtdLinhasHeaderLote <= 0)
                throw new Exception("Não foi encontrado HEADER do arquivo de retorno.");

            //if (qtdLinhasHeaderLote > 1)
            //    throw new Exception("Não é permitido mais de um HEADER no arquivo de retorno.");

            var qtdLinhasDetalheSegmentoA = _linhasArquivo.Count(wh => wh.ExtrairValorDaLinha(14, 14) == "A");
            var qtdLinhasDetalheSegmentoB = _linhasArquivo.Count(wh => wh.ExtrairValorDaLinha(14, 14) == "B");
            var qtdLinhasDetalheSegmentoJ = _linhasArquivo.Count(wh => wh.ExtrairValorDaLinha(14, 14) == "j");

            if (qtdLinhasDetalheSegmentoA <= 0 && qtdLinhasDetalheSegmentoJ <= 0)
                throw new Exception("Não foi encontrado DETALHE para o Segmento A no arquivo de retorno.");

            if (qtdLinhasDetalheSegmentoB <= 0 && qtdLinhasDetalheSegmentoA <= 0 && qtdLinhasDetalheSegmentoJ <= 0)
                throw new Exception("Não foi encontrado DETALHE no arquivo de retorno.");
            var qtdLinhasTrailerLote = _linhasArquivo.Count(wh => wh.ExtrairValorDaLinha(8, 8) == "5");

            if (qtdLinhasTrailerLote <= 0)
                throw new Exception("Não foi encontrado TRAILER do arquivo de retorno.");

            if (qtdLinhasTrailerLote != qtdLinhasHeaderLote)
                throw new Exception("Quantidade de TRAILER de lotes é diferente da quantidade de HEADER  de lotes");

            var qtdLinhasTrailer = _linhasArquivo.Count(wh => wh.ExtrairValorDaLinha(8, 8) == "9");

            if (qtdLinhasTrailer <= 0)
                throw new Exception("Não foi encontrado TRAILER do arquivo de retorno.");

            if (qtdLinhasTrailer > 1)
                throw new Exception("Não é permitido mais de um TRAILER no arquivo de retorno.");
        }

        public HeaderRetornoCnab240 ObterHeader(string linha)
        {
            var objRetornar = new HeaderRetornoCnab240
            {
                CodigoBanco = linha.ExtrairValorDaLinha(1, 3).BoletoBrToInt(),
                LoteServico = linha.ExtrairValorDaLinha(4, 7).Trim(),
                CodigoRegistro = linha.ExtrairValorDaLinha(8, 8).BoletoBrToInt(),
                TipoInscricaoEmpresa = linha.ExtrairValorDaLinha(18, 18).BoletoBrToInt(),
                NumeroInscricaoEmpresa = linha.ExtrairValorDaLinha(19, 32).Trim(),
                Convenio = linha.ExtrairValorDaLinha(33, 41).Trim(),
                CodigoAgencia = linha.ExtrairValorDaLinha(53, 57).BoletoBrToInt(),
                DvCodigoAgencia = linha.ExtrairValorDaLinha(58, 58).Trim(),
                ContaCorrente = linha.ExtrairValorDaLinha(59, 70).Trim(),
                DvContaCorrente = linha.ExtrairValorDaLinha(71, 71).Trim(),
                DvAgenciaConta = linha.ExtrairValorDaLinha(72, 72).Trim(),
                NomeDoBeneficiario = linha.ExtrairValorDaLinha(73, 102).Trim(),
                NomeDoBanco = linha.ExtrairValorDaLinha(103, 132).Trim(),
                CodigoRemessaRetorno = linha.ExtrairValorDaLinha(143, 143).BoletoBrToInt(),
                DataGeracaoGravacao = Convert.ToDateTime(linha.ExtrairValorDaLinha(144, 151).ToDateTimeFromDdMmAa()),
                HoraGeracaoGravacao = linha.ExtrairValorDaLinha(152, 157).BoletoBrToInt(),
                NumeroSequencial = linha.ExtrairValorDaLinha(158, 163).BoletoBrToInt(),
                VersaoLayout = linha.ExtrairValorDaLinha(164, 166).Trim(),
                Densidade = linha.ExtrairValorDaLinha(167, 171).Trim(),
                UsoBanco = linha.ExtrairValorDaLinha(172, 191).Trim(),
                UsoEmpresa = linha.ExtrairValorDaLinha(192, 211).Trim(),
                TipoServico = linha.ExtrairValorDaLinha(229, 230).Trim(),
                Ocorrencia = linha.ExtrairValorDaLinha(231, 240).Trim(),

            };

            return objRetornar;
        }

        public HeaderLoteRetornoCnab240 ObterHeaderLote(string linha)
        {
            var objRetornar = new HeaderLoteRetornoCnab240
            {
                CodigoBanco = linha.ExtrairValorDaLinha(1, 3).BoletoBrToInt(),
                LoteServico = linha.ExtrairValorDaLinha(4, 7).Trim(),
                CodigoRegistro = linha.ExtrairValorDaLinha(8, 8).BoletoBrToInt(),
                TipoOperacao = linha.ExtrairValorDaLinha(9, 9).Trim(),
                TipoServico = linha.ExtrairValorDaLinha(10, 11).BoletoBrToInt(),
                FormaLancamento = linha.ExtrairValorDaLinha(12, 13).Trim(),
                VersaoLayoutLote = linha.ExtrairValorDaLinha(14, 16).BoletoBrToInt(),
                TipoInscricaoEmpresa = linha.ExtrairValorDaLinha(18, 18).BoletoBrToInt(),
                NumeroInscricaoEmpresa = linha.ExtrairValorDaLinha(19, 32).Trim(),
                Convenio = linha.ExtrairValorDaLinha(33, 41).Trim(),
                CodigoAgencia = linha.ExtrairValorDaLinha(53, 57).BoletoBrToInt(),
                DvCodigoAgencia = linha.ExtrairValorDaLinha(58, 58).Trim(),
                ContaCorrente = linha.ExtrairValorDaLinha(59, 70).Trim(),
                DvContaCorrente = linha.ExtrairValorDaLinha(71, 71).Trim(),
                DvAgenciaConta = linha.ExtrairValorDaLinha(72, 72).Trim(),
                NomeDoBeneficiario = linha.ExtrairValorDaLinha(73, 102).Trim(),
                Mensagem1 = linha.ExtrairValorDaLinha(103, 142).Trim(),
                Logradouro = linha.ExtrairValorDaLinha(143, 172).Trim(),
                Numero = linha.ExtrairValorDaLinha(173, 177).Trim(),
                Complemento = linha.ExtrairValorDaLinha(178, 192).Trim(),
                Cidade = linha.ExtrairValorDaLinha(193, 212).Trim(),
                Cep = linha.ExtrairValorDaLinha(213, 217).Trim(),
                ComplementoCep = linha.ExtrairValorDaLinha(218, 220).Trim(),
                Estado = linha.ExtrairValorDaLinha(221, 222).Trim(),
                Ocorrencia = linha.ExtrairValorDaLinha(231, 240).Trim(),
            };

            return objRetornar;
        }

        public DetalheSegmentoARetornoCnab240 ObterRegistrosDetalheA(string linha)
        {
            var objRetornar = new DetalheSegmentoARetornoCnab240
            {
                CodigoBanco = linha.ExtrairValorDaLinha(1, 3).Trim(),
                LoteServico = linha.ExtrairValorDaLinha(4, 7).BoletoBrToInt(),
                NumeroRegistro = linha.ExtrairValorDaLinha(9, 13).BoletoBrToInt(),
                CodigoSegmento = linha.ExtrairValorDaLinha(14, 14).Trim(),
                CodigoMovimento = linha.ExtrairValorDaLinha(15, 17).BoletoBrToInt(),
                CodigoCamaraCentralizadora = linha.ExtrairValorDaLinha(18, 20).Trim(),
                CodigoBancoFavorecido = linha.ExtrairValorDaLinha(21, 23).Trim(),
                AgenciaMantenedoraFavorecido = linha.ExtrairValorDaLinha(24, 28).Trim(),
                DigitoAgenciaMantenedoraFavorecido = linha.ExtrairValorDaLinha(29, 29).Trim(),
                ContaFavorecido = linha.ExtrairValorDaLinha(30, 41).Trim(),
                DigitoContaFavorecido = linha.ExtrairValorDaLinha(42, 43).Trim(),
                NomeFavorecido = linha.ExtrairValorDaLinha(44, 73).Trim(),
                SeuNumero = linha.ExtrairValorDaLinha(74, 93).Trim(),
                DataPagamento = linha.ExtrairValorDaLinha(94, 101).ToDateTimeFromDdMmAaaa(),
                ValorPagamento = linha.ExtrairValorDaLinha(120, 134).BoletoBrToDecimal() / 100,
                NossoNumero = linha.ExtrairValorDaLinha(135, 154).Trim(),
                DataEfetivacaoPagamento = linha.ExtrairValorDaLinha(155, 162).ToDateTimeFromDdMmAaaa(),
                ValorRealEfetivacaoPagamento = linha.ExtrairValorDaLinha(163, 177).BoletoBrToDecimal() / 100,
                Mensagem = linha.ExtrairValorDaLinha(178, 217).Trim(),
                CodigoFinalidadeDoc = linha.ExtrairValorDaLinha(218, 219).Trim(),
                CodigoFinalidadeTed = linha.ExtrairValorDaLinha(220, 224).Trim(),
                CodigoFinalidadeComplementar = linha.ExtrairValorDaLinha(225, 226).Trim(),
                AvisoFavorecido = linha.ExtrairValorDaLinha(230, 230).Trim(),
                Ocorrencia = linha.ExtrairValorDaLinha(231, 240).Trim()
            };

            return objRetornar;
        }

        public DetalheSegmentoBRetornoCnab240 ObterRegistrosDetalheB(string linha)
        {
            var objRetornar = new DetalheSegmentoBRetornoCnab240
            {
                CodigoBanco = linha.ExtrairValorDaLinha(1, 3).Trim(),
                LoteServico = linha.ExtrairValorDaLinha(4, 7).BoletoBrToInt(),
                CodigoRegistro = linha.ExtrairValorDaLinha(8, 8).Trim(),
                NumeroRegistro = linha.ExtrairValorDaLinha(9, 13).BoletoBrToInt(),
                CodigoSegmento = linha.ExtrairValorDaLinha(14, 14).Trim(),
                TipoInscricaoFavorecido = linha.ExtrairValorDaLinha(18, 18).Trim(),
                NumeroInscricaoFavorecido = linha.ExtrairValorDaLinha(19, 32).Trim(),
                LogradouroFavorecido = linha.ExtrairValorDaLinha(33, 62).Trim(),
                NumeroFavorecido = linha.ExtrairValorDaLinha(63, 67).Trim(),
                ComplementoFavorecido = linha.ExtrairValorDaLinha(68, 82).Trim(),
                BairroFavorecido = linha.ExtrairValorDaLinha(83, 97).Trim(),
                CidadeFavorecido = linha.ExtrairValorDaLinha(98, 117).Trim(),
                CEPFavorecido = linha.ExtrairValorDaLinha(118, 122).Trim(),
                ComplementoCEPFavorecido = linha.ExtrairValorDaLinha(123, 125).Trim(),
                EstadoFavorecido = linha.ExtrairValorDaLinha(126, 127).Trim(),
                DataVencimento = linha.ExtrairValorDaLinha(128, 135).ToDateTimeFromDdMmAaaa(),
                ValorPagamento = linha.ExtrairValorDaLinha(136, 150).BoletoBrToDecimal() / 100,
                ValorAbatimento = linha.ExtrairValorDaLinha(151, 165).BoletoBrToDecimal() / 100,
                ValorDesconto = linha.ExtrairValorDaLinha(166, 180).BoletoBrToDecimal() / 100,
                ValorJurosMora = linha.ExtrairValorDaLinha(181, 195).BoletoBrToDecimal() / 100,
                ValorMulta = linha.ExtrairValorDaLinha(196, 210).BoletoBrToDecimal() / 100,
                CodigoFavorecido = linha.ExtrairValorDaLinha(211, 225).Trim(),
                Aviso = linha.ExtrairValorDaLinha(226, 226).Trim(),

            };

            return objRetornar;
        }

        public DetalheSegmentoJRetornoCnab240 ObterRegistrosDetalheJ(string linha)
        {
            var objRetornar = new DetalheSegmentoJRetornoCnab240
            {
                CodigoBanco = linha.ExtrairValorDaLinha(1, 3).Trim(),
                LoteServico = linha.ExtrairValorDaLinha(4, 7).BoletoBrToInt(),
                CodigoRegistro = linha.ExtrairValorDaLinha(8, 8).Trim(),
                NumeroRegistro = linha.ExtrairValorDaLinha(9, 13).BoletoBrToInt(),
                CodigoSegmento = linha.ExtrairValorDaLinha(14, 14).Trim(),

                CodBarras = linha.ExtrairValorDaLinha(18, 61).Trim(),
                NomeCedente = linha.ExtrairValorDaLinha(62, 91).Trim(),
                DataVencimento = linha.ExtrairValorDaLinha(92, 99).ToDateTimeFromDdMmAaaa(),
                ValorTitulo = linha.ExtrairValorDaLinha(100, 114).BoletoBrToDecimal() / 100,
                ValorDesconto = linha.ExtrairValorDaLinha(115, 129).BoletoBrToDecimal() / 100,
                ValorAcrescimo = linha.ExtrairValorDaLinha(130, 144).BoletoBrToDecimal() / 100,
                DataPagamento = linha.ExtrairValorDaLinha(145, 152).ToDateTimeFromDdMmAaaa(),
                ValorPagamento = linha.ExtrairValorDaLinha(153, 167).BoletoBrToDecimal() / 100,

                SeuNumero = linha.ExtrairValorDaLinha(183, 202).Trim(),
                NossoNumero = linha.ExtrairValorDaLinha(203, 222).Trim(),
                Ocorrencia = linha.ExtrairValorDaLinha(231, 240).Trim(),

            };

            return objRetornar;
        }

        public TrailerLoteRetornoCnab240 ObterTrailerLote(string linha)
        {
            var objRetornar = new TrailerLoteRetornoCnab240
            {
                CodigoBanco = linha.ExtrairValorDaLinha(1, 3).BoletoBrToInt(),
                LoteServico = linha.ExtrairValorDaLinha(4, 7).Trim(),
                CodigoRegistro = linha.ExtrairValorDaLinha(8, 8).BoletoBrToInt(),
                QtdRegistrosLote = linha.ExtrairValorDaLinha(18, 23).BoletoBrToLong(),
                Valor = linha.ExtrairValorDaLinha(24, 41).BoletoBrToDecimal() / 100,
                NumeroAvisoLancamento = linha.ExtrairValorDaLinha(60, 65).Trim(),
                Ocorrencia = linha.ExtrairValorDaLinha(231, 240).Trim()
            };

            return objRetornar;
        }

        public TrailerRetornoCnab240 ObterTrailer(string linha)
        {
            var objRetornar = new TrailerRetornoCnab240
            {
                CodigoBanco = linha.ExtrairValorDaLinha(1, 3).BoletoBrToInt(),
                LoteServico = linha.ExtrairValorDaLinha(4, 7).Trim(),
                CodigoRegistro = linha.ExtrairValorDaLinha(8, 8).BoletoBrToInt(),
                QtdLotesArquivo = linha.ExtrairValorDaLinha(18, 23).BoletoBrToInt(),
                QtdRegistrosArquivo = linha.ExtrairValorDaLinha(24, 29).BoletoBrToInt(),
                QtdContasConc = linha.ExtrairValorDaLinha(30, 35).BoletoBrToInt()
            };

            return objRetornar;
        }

        public DetalheSegmentoTRetornoCnab240 ObterRegistrosDetalheT(string linha)
        {
            throw new NotImplementedException();
        }

        public DetalheSegmentoURetornoCnab240 ObterRegistrosDetalheU(string linha)
        {
            throw new NotImplementedException();
        }
    }
}
