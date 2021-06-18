﻿using System;
using BoletoBr.Interfaces;

namespace BoletoBr.Arquivo.CNAB240.Remessa
{
    public class DetalheSegmentoPRemessaCnab240
    {
        public DetalheSegmentoPRemessaCnab240(Boleto boleto, int numeroRegistroNoLote)
        {
            this.CodigoBanco = boleto.BancoBoleto.CodigoBanco;
            this.NumeroRegistro = numeroRegistroNoLote;
            this.CodigoOcorrencia = boleto.CodigoOcorrenciaRemessa;
            this.AgenciaMantenedora = boleto.CedenteBoleto.ContaBancariaCedente.Agencia;
            this.DvAgenciaMantenedora = boleto.CedenteBoleto.ContaBancariaCedente.DigitoAgencia;
            this.CodigoCedente = boleto.CedenteBoleto.CodigoCedente;
            this.DigitoCedente = boleto.CedenteBoleto.DigitoCedente;
            this.ModalidadeCarteira = boleto.CarteiraCobranca.Codigo;
            this.NossoNumero = boleto.NossoNumeroFormatado;
            this.NumeroDocumento = boleto.NumeroDocumento;
            this.DataVencimento = boleto.DataVencimento;
            this.ValorBoleto = boleto.ValorBoleto;
            this.Especie = boleto.Especie;
            this.Aceite = boleto.Aceite;
            this.DataEmissao = boleto.DataDocumento;

            /*Juros Mora*/
            this.ValorJurosMora = boleto.JurosMora > 0 ? boleto.JurosMora : boleto.PercentualJurosMora > 0 ? boleto.PercentualJurosMora : 0;
            this.CasasDecimaisJuros = boleto.JurosMora > 0 ? 2 : boleto.PercentualJurosMora > 0 ? 4 : 2;

            this.CodigoJurosMora =
                this.CodigoBanco == "246"
                ? boleto.JurosMora > 0 || boleto.PercentualJurosMora > 0
                    ? boleto.BancoBoleto.CodigoJurosMora(Enums.CodigoJurosMora.Mensal)
                    : boleto.BancoBoleto.CodigoJurosMora(Enums.CodigoJurosMora.Isento)
                :
                boleto.JurosMora > 0
                ? boleto.BancoBoleto.CodigoJurosMora(Enums.CodigoJurosMora.Valor)
                : boleto.PercentualJurosMora > 0
                    ? boleto.BancoBoleto.CodigoJurosMora(Enums.CodigoJurosMora.Percentual)
                    : boleto.BancoBoleto.CodigoJurosMora(Enums.CodigoJurosMora.Isento);

            this.DataJurosMora = boleto.DataJurosMora;
            /*Instruções para protesto*/
            this.PrazoProtesto = boleto.CarteiraCobranca.QtdDiasProtesto;
            this.CodigoProtesto = PrazoProtesto > 0 ? boleto.BancoBoleto.CodigoProteso() : boleto.BancoBoleto.CodigoProteso(false);
            /*Periodo devolução baixa*/
            this.PrazoBaixaDevolucao = boleto.PrazoBaixaDevolucao;
            /*-------------------------------------*/
            this.ValorDesconto1 = boleto.ValorDesconto.HasValue ? boleto.ValorDesconto : 0;

            this.DataDesconto1 = ValorDesconto1 > 0 ? boleto.DataLimitDesconto.GetValueOrDefault() : DateTime.MinValue;

            this.ValorIof = boleto.Iof.HasValue ? boleto.Iof : 0;
            this.ValorAbatimento = boleto.ValorAbatimento.HasValue ? boleto.ValorAbatimento : 0;
            this.CodigoMoeda = boleto.Moeda;
            this.BancoEmiteBoleto = boleto.CarteiraCobranca.BancoEmiteBoleto;
            this.NumeroContaCorrente = boleto.CedenteBoleto.ContaBancariaCedente.Conta;
            this.DigitoContaCorrente = boleto.CedenteBoleto.ContaBancariaCedente.DigitoConta;

            this.NossoNumero = boleto.NossoNumeroFormatado;
            this.Convenio = boleto.CedenteBoleto.Convenio;
        }

        public string CodigoBanco { get; set; }
        public int LoteServico { get; set; }
        public int TipoRegistro { get; set; }
        public int NumeroRegistro { get; set; }
        public string Segmento { get; set; }
        public ICodigoOcorrencia CodigoOcorrencia { get; set; }
        public string AgenciaMantenedora { get; set; }
        public string DvAgenciaMantenedora { get; set; }
        public string CodigoCedente { get; set; }
        public int DigitoCedente { get; set; }
        public string ModalidadeCarteira { get; set; }
        public string NossoNumero { get; set; }
        public int CodigoCarteira { get; set; }
        public int FormaCadastramento { get; set; }
        public string TipoDocumento { get; set; }
        public int IdEmissaoBloqueto { get; set; }
        public string IdEntregaBloqueto { get; set; }
        public string NumeroDocumento { get; set; }
        public DateTime DataVencimento { get; set; }
        public decimal ValorBoleto { get; set; }
        public string AgenciaCobradora { get; set; }
        public string DvAgenciaCobradora { get; set; }
        public IEspecieDocumento Especie { get; set; }
        public string Aceite { get; set; }
        public DateTime DataEmissao { get; set; }
        public int CodigoJurosMora { get; set; }
        public DateTime DataJurosMora { get; set; }
        public decimal? ValorJurosMora { get; set; }
        public int CasasDecimaisJuros { get; set; }
        public int CodigoDesconto1 { get; set; }
        public DateTime DataDesconto1 { get; set; }
        public decimal? ValorDesconto1 { get; set; }
        public decimal? ValorIof { get; set; }
        public decimal? ValorAbatimento { get; set; }
        public string UsoEmpresa { get; set; }
        public int CodigoProtesto { get; set; }
        public int PrazoProtesto { get; set; }
        public int CodigoBaixaDevolucao { get; set; }
        public int PrazoBaixaDevolucao { get; set; }
        public string CodigoMoeda { get; set; }
        public bool BancoEmiteBoleto { get; set; }
        public string NumeroContaCorrente { get; set; }
        public string DigitoContaCorrente { get; set; }
        public string Convenio { get; set; }
    }
}
