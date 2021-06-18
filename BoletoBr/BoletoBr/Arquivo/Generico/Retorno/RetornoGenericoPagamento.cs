using System;
using System.Collections.Generic;
using System.Globalization;
using BoletoBr.Arquivo.CNAB240.Retorno;
using BoletoBr.Dominio;
using BoletoBr.Fabricas;

namespace BoletoBr.Arquivo.Generico.Retorno
{
    public class RetornoGenericoPagamento
    {
        public void Inicializa()
        {
            Header = new RetornoHeaderGenerico();
            RegistrosDetalhe = new List<RetornoDetalheGenerico>();
            Trailer = new RetornoTrailerGenerico();
        }

        public RetornoGenericoPagamento(RetornoCnab240 retornoCnab240)
        {
            Inicializa();
            RetornoCnab240Especifico = retornoCnab240;
            /* Transformar de CNAB240 para formato genérico */

            foreach (var loteAtual in retornoCnab240.Lotes)
            {
                foreach (var d in loteAtual.RegistrosDetalheSegmentos)
                {
                    var detalheGenericoAdd = new RetornoDetalheGenerico();

                    if (d.SegmentoJ != null)
                    {
                        detalheGenericoAdd.NossoNumero = d.SegmentoJ.NossoNumero;
                        detalheGenericoAdd.SeuNumero = d.SegmentoJ.SeuNumero;
                        detalheGenericoAdd.NomeFavorecido = d.SegmentoJ.NomeCedente;

                        detalheGenericoAdd.DataPagamento = d.SegmentoJ.DataPagamento.GetValueOrDefault();
                        detalheGenericoAdd.DataEfetivacaoPagamento= detalheGenericoAdd.DataPagamento;
                        detalheGenericoAdd.DataVencimento = Convert.ToDateTime(d.SegmentoJ.DataVencimento);
                        
                        detalheGenericoAdd.ValorPagamento = d.SegmentoJ.ValorPagamento;
                        detalheGenericoAdd.ValorDocumento = d.SegmentoJ.ValorTitulo;
                        detalheGenericoAdd.ValorRealEfetivacaoPagamento = d.SegmentoJ.ValorPagamento;
                        detalheGenericoAdd.ValorAcrescimos = d.SegmentoJ.ValorAcrescimo;
                        detalheGenericoAdd.ValorDesconto = d.SegmentoJ.ValorDesconto;
                        
                        var banco = BancoFactory.ObterBanco(d.SegmentoJ?.CodigoBanco.ToString());
                        var ocorrencia = banco.ObtemCodigoOcorrenciaPagamento(d.SegmentoJ.Ocorrencia.Substring(8, 2));

                        detalheGenericoAdd.CodigoOcorrencia = ocorrencia?.CodigoText.ToString();
                        detalheGenericoAdd.MensagemOcorrenciaRetornoBancario = ocorrencia?.Descricao;
                        detalheGenericoAdd.Ocorrencia = ocorrencia;

                        RegistrosDetalhe.Add(detalheGenericoAdd);
                    }
                    else
                    {

                        detalheGenericoAdd.NossoNumero = d.SegmentoA.NossoNumero;
                        detalheGenericoAdd.SeuNumero = d.SegmentoA.SeuNumero;
                        detalheGenericoAdd.NomeFavorecido = d.SegmentoA.NomeFavorecido;

                        detalheGenericoAdd.DataPagamento = d.SegmentoA.DataPagamento.GetValueOrDefault();
                        detalheGenericoAdd.DataVencimento = Convert.ToDateTime(d.SegmentoB.DataVencimento);
                        detalheGenericoAdd.DataEfetivacaoPagamento = d.SegmentoA.DataEfetivacaoPagamento.GetValueOrDefault();

                        detalheGenericoAdd.ValorPagamento = d.SegmentoA.ValorPagamento;
                        detalheGenericoAdd.ValorRealEfetivacaoPagamento = d.SegmentoA.ValorRealEfetivacaoPagamento;
                        detalheGenericoAdd.ValorAcrescimos = d.SegmentoB.ValorJurosMora.GetValueOrDefault();
                        detalheGenericoAdd.ValorDesconto = d.SegmentoB.ValorDesconto.GetValueOrDefault();
                        detalheGenericoAdd.ValorMulta = d.SegmentoB.ValorMulta.GetValueOrDefault();

                        var banco = BancoFactory.ObterBanco(d.SegmentoB?.CodigoBanco.ToString());
                        var ocorrencia = banco.ObtemCodigoOcorrenciaPagamento(d.SegmentoA.Ocorrencia.Substring(8, 2));

                        detalheGenericoAdd.CodigoOcorrencia = ocorrencia?.CodigoText.ToString();
                        detalheGenericoAdd.MensagemOcorrenciaRetornoBancario = ocorrencia?.Descricao;
                        detalheGenericoAdd.Ocorrencia = ocorrencia;

                        RegistrosDetalhe.Add(detalheGenericoAdd);
                    }

                }
            }

            Trailer.QtdRegistrosArquivo = retornoCnab240.Trailer.QtdRegistrosArquivo.ToString(CultureInfo.InvariantCulture);
        }

        public RetornoHeaderGenerico Header { get; set; }
        public List<RetornoDetalheGenerico> RegistrosDetalhe { get; set; }
        public RetornoTrailerGenerico Trailer { get; set; }
        public RetornoCnab240 RetornoCnab240Especifico { get; set; }
    }
}
