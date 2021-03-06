﻿using System;
using System.Linq;
using BoletoBr.Arquivo.CNAB240.Remessa;
using BoletoBr.Arquivo.CNAB400.Remessa;
using BoletoBr.Arquivo.DebitoAutomatico.Remessa;
using BoletoBr.Bancos.DebitoAutomatico;
using BoletoBr.Bancos.Itau;
using BoletoBr.Interfaces;

namespace BoletoBr.Fabricas
{
    public static class EscritorArquivoRemessaFactory
    {
        public static IEscritorArquivoRemessaCnab240 ObterEscritorRemessaPagamento(RemessaCnab240 remessaEscrever)
        {
            try
            {
                switch (remessaEscrever.Header.CodigoBanco.PadLeft(3, '0'))
                {
                    case "001": /* 001 - Banco do Brasil */
                        return new Bancos.Brasil.EscritorRemessaPagamentoCnab240BancoDoBrasil(remessaEscrever);
                    case "237": /* 237 - Bradesco */
                        return new Bancos.Bradesco.EscritorRemessaPagamentoCnab240Bradesco(remessaEscrever);
                    case "756": /* 756 - Sicoob */
                        return new Bancos.Sicoob.EscritorRemessaPagamentoCnab240Sicoob(remessaEscrever);
                    default:
                        throw new NotImplementedException("Banco " + remessaEscrever.Header.CodigoBanco + " ainda não foi implementado.");
                }
            }
            catch (Exception ex)
            {

                throw new Exception("Erro durante a execução da transação.", ex);
            }
        }
        public static IEscritorArquivoRemessaCnab240 ObterEscritorRemessa(RemessaCnab240 remessaEscrever)
        {
            try
            {
                switch (remessaEscrever.Header.CodigoBanco.PadLeft(3, '0'))
                {
                    /* 001 - Banco do Brasil */
                    case "001":
                        throw new NotImplementedException("Banco " + remessaEscrever.Header.CodigoBanco + " ainda não foi implementado.");
                    /* 003 - Banco da Amazônia */
                    case "003":
                        throw new NotImplementedException("Banco " + remessaEscrever.Header.CodigoBanco + " ainda não foi implementado.");

                    case "033":
                        throw new NotImplementedException("Banco " + remessaEscrever.Header.CodigoBanco + " ainda não foi implementado.");
                    /* 104 - Caixa */
                    case "104":
                        return new Bancos.Cef.EscritorRemessaCnab240CefSicgb(remessaEscrever);

                    /* 756 - SICOOB */
                    case "756":
                        return new Bancos.Sicoob.EscritorRemessaCnab240Sicoob(remessaEscrever);

                    /* 041 - BANRISUL */
                    case "041":
                        throw new NotImplementedException("Banco " + remessaEscrever.Header.CodigoBanco + " ainda não foi implementado.");

                    /* 237 - Bradesco */
                    case "237":
                        throw new NotImplementedException("Banco " + remessaEscrever.Header.CodigoBanco + " ainda não foi implementado.");

                    /* 237 - ABC */
                    case "246":
                        return new Bancos.Abc.EscritorRemessaCnab240Abc(remessaEscrever);

                    /* 341 - Itaú */
                    case "341":
                        throw new NotImplementedException("Banco " + remessaEscrever.Header.CodigoBanco + " ainda não foi implementado.");

                    /* 399 - HSBC */
                    case "399":
                        throw new NotImplementedException("Banco " + remessaEscrever.Header.CodigoBanco + " ainda não foi implementado.");

                    default:
                        throw new NotImplementedException("Banco " + remessaEscrever.Header.CodigoBanco + " ainda não foi implementado.");
                }
            }
            catch (Exception ex)
            {

                throw new Exception("Erro durante a execução da transação.", ex);
            }
        }

        public static IEscritorArquivoRemessaCnab400 ObterEscritorRemessa(RemessaCnab400 remessaEscrever)
        {
            try
            {
                switch (remessaEscrever.Header.CodigoBanco.PadLeft(3, '0'))
                {
                    /* 001 - Banco do Brasil */
                    case "001":
                        return new Bancos.Brasil.EscritorRemessaCnab400BancoDoBrasil(remessaEscrever);
                    /* 003 - Banco da Amazônia */
                    case "003":
                        throw new NotImplementedException("Banco " + remessaEscrever.Header.CodigoBanco + " ainda não foi implementado.");
                    case "021":
                        return new Bancos.Banestes.EscritorRemessaCnab400Banestes(remessaEscrever);
                    case "033":
                        return new Bancos.Santander.EscritorRemessaCnab400Santander(remessaEscrever);
                    /* 104 - Caixa */
                    case "104":
                        throw new NotImplementedException("Banco " + remessaEscrever.Header.CodigoBanco + " ainda não foi implementado.");
                    /* 136 - Unicred */
                    case "136":
                        return new BoletoBr.Bancos.UniCred.EscritorRemessaCnab400Unicred(remessaEscrever);
                    /* 237 - Bradesco */
                    case "237":
                        return new Bancos.Bradesco.EscritorRemessaCnab400Bradesco(remessaEscrever);
                    /* 341 - Itaú */
                    case "341":
                        return new EscritorRemessaCnab400Itau(remessaEscrever);
                    /* 399 - HSBC */
                    case "399":
                        return new Bancos.Hsbc.EscritorRemessaCnab400Hsbc(remessaEscrever);
                    /* 070 - BRB */
                    case "070":
                        return new Bancos.BRB.EscritorRemessaCnab400BRB(remessaEscrever);
                    /* 748 - Sicredi */
                    case "748":
                        return new Bancos.Sicredi.EscritorRemessaCnab400Sicredi(remessaEscrever);
                    /* 756 - SICOOB */
                    case "756":
                        return new Bancos.Sicoob.EscritorRemessaCnab400Sicoob(remessaEscrever);
                    /* 422 - SAFRA */
                    case "422":
                        return new Bancos.Safra.EscritorRemessaCnab400Safra(remessaEscrever);
                    /* 707 - BANCO DAYCOVAL */
                    case "707":
                        return new Bancos.Daycoval.EscritorRemessaCnab400Daycoval(remessaEscrever);
                    /* 041 - BANCO BANRISUL */
                    case "041":
                        return new EscritorRemessaCnab400Banrisul(remessaEscrever);
                    default:
                        throw new NotImplementedException("Banco " + remessaEscrever.Header.CodigoBanco +
                                                          " ainda não foi implementado.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Erro durante a execução da transação.", ex);
            }
        }

        public static IEscritorArquivoRemessaDebitoAutomatico ObterEscritorRemessa(RemessaDebitoAutomatico remessaEscrever)
        {
            if (remessaEscrever.RegistrosDetalheC != null && remessaEscrever.RegistrosDetalheC.Any()
                || remessaEscrever.RegistrosDetalheD != null && remessaEscrever.RegistrosDetalheD.Any()
                || remessaEscrever.RegistrosDetalheE != null && remessaEscrever.RegistrosDetalheE.Any()
                || remessaEscrever.RegistrosDetalheI != null && remessaEscrever.RegistrosDetalheI.Any()
                || remessaEscrever.RegistrosDetalheL != null && remessaEscrever.RegistrosDetalheL.Any()
                || remessaEscrever.RegistrosDetalheJ != null && remessaEscrever.RegistrosDetalheJ.Any())
            {
                return new EscritorRemessaDebitoAutomatico(remessaEscrever);
            }
            else
            {
                throw new Exception("Não foi identificado registro de DETALHE");
            }
        }
    }
}
