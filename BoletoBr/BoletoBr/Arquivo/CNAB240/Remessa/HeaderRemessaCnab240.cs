using System;

namespace BoletoBr.Arquivo.CNAB240.Remessa
{
    public class HeaderRemessaCnab240
    {
        public HeaderRemessaCnab240(Boleto boleto, int sequencialArquivo)
        {
            this.CodigoBanco = boleto.BancoBoleto.CodigoBanco;
            this.NumeroInscricao = boleto.CedenteBoleto.CpfCnpj;
            this.AgenciaMantenedora = boleto.CedenteBoleto.ContaBancariaCedente.Agencia;
            this.DigitoAgenciaMantenedora = boleto.CedenteBoleto.ContaBancariaCedente.DigitoAgencia;
            this.CodigoCedente = boleto.CedenteBoleto.CodigoCedente;
            this.DigitoCedente = boleto.CedenteBoleto.DigitoCedente;
            this.NomeEmpresa = boleto.CedenteBoleto.Nome;
            this.SequencialNsa = sequencialArquivo;

            this.NumeroContaCorrente = boleto.CedenteBoleto.ContaBancariaCedente.Conta;
            this.DigitoContaCorrente = boleto.CedenteBoleto.ContaBancariaCedente.DigitoConta;
            this.CodigoConvenio = boleto.CedenteBoleto.Convenio;
        }

        public HeaderRemessaCnab240(Pagamento pagamento, int sequencialArquivo)
        {
            var banco = Fabricas.BancoFactory.ObterBanco(pagamento.CodigoBanco); 
            this.CodigoBanco = pagamento.CodigoBanco;
            this.NumeroInscricao = pagamento.Empresa.CpfCnpj;
            this.AgenciaMantenedora = pagamento.Empresa.ContaBancariaCedente.Agencia;
            this.NomeBanco = banco.NomeBanco;
            this.DigitoAgenciaMantenedora = pagamento.Empresa.ContaBancariaCedente.DigitoAgencia;
            this.CodigoCedente = pagamento.Empresa.CodigoCedente;
            this.DigitoCedente = pagamento.Empresa.DigitoCedente;
            this.NomeEmpresa = pagamento.Empresa.Nome;
            this.SequencialNsa = sequencialArquivo;

            this.NumeroContaCorrente = pagamento.Empresa.ContaBancariaCedente.Conta;
            this.DigitoContaCorrente = pagamento.Empresa.ContaBancariaCedente.DigitoConta;
            this.CodigoConvenio = pagamento.CodigoConvenio;
        }

        public HeaderRemessaCnab240(Pagamento pagamento, int sequencialArquivo, bool teste)
        {
            var banco = Fabricas.BancoFactory.ObterBanco(pagamento.CodigoBanco);
            this.CodigoBanco = pagamento.CodigoBanco;
            this.NumeroInscricao = pagamento.Empresa.CpfCnpj;
            this.AgenciaMantenedora = pagamento.Empresa.ContaBancariaCedente.Agencia;
            this.NomeBanco = banco.NomeBanco;
            this.DigitoAgenciaMantenedora = pagamento.Empresa.ContaBancariaCedente.DigitoAgencia;
            this.CodigoCedente = pagamento.Empresa.CodigoCedente;
            this.DigitoCedente = pagamento.Empresa.DigitoCedente;
            this.NomeEmpresa = pagamento.Empresa.Nome;
            this.SequencialNsa = sequencialArquivo;

            this.NumeroContaCorrente = pagamento.Empresa.ContaBancariaCedente.Conta;
            this.DigitoContaCorrente = pagamento.Empresa.ContaBancariaCedente.DigitoConta;
            this.CodigoConvenio = pagamento.CodigoConvenio;

            this.Teste = teste && pagamento.CodigoBanco == "001" ? "TS" : string.Empty; /*Banco Brasil*/
        }

        public string DigitoContaCorrente { get; set; }

        public string NumeroContaCorrente { get; set; }

        public string CodigoBanco { get; set; }
        public int LoteServico { get; set; }
        public int TipoRegistro { get; set; }
        public int TipoInscricao { get; set; }
        public string NumeroInscricao { get; set; }
        public string AgenciaMantenedora { get; set; }
        public string DigitoAgenciaMantenedora { get; set; }
        public string CodigoCedente { get; set; }
        public int DigitoCedente { get; set; }
        public string NomeEmpresa { get; set; }
        public string NomeBanco { get; set; }
        public string CodigoRemessa { get; set; }
        public DateTime DataGeracao { get; set; }
        public DateTime HoraGeracao { get; set; }
        public int SequencialNsa { get; set; }
        public string VersaoLayout { get; set; }
        public string Densidade { get; set; }
        public string ReservadoBanco { get; set; }
        public string ReservadoEmpresa { get; set; }
        public string VersaoAplicativo { get; set; }
        public string CodigoConvenio { get; set; }
        public string Teste{ get; set; }
    }
}
