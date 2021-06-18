using System;

namespace BoletoBr.Arquivo.CNAB240.Remessa
{
    public class HeaderLoteRemessaCnab240
    {
        public HeaderLoteRemessaCnab240(Boleto boleto, int sequencialRemessa)
        {
            this.CodigoBanco = boleto.BancoBoleto.CodigoBanco;
            this.NumeroInscricao = boleto.CedenteBoleto.CpfCnpj;
            this.Agencia = boleto.CedenteBoleto.ContaBancariaCedente.Agencia;
            this.DigitoAgencia = boleto.CedenteBoleto.ContaBancariaCedente.DigitoAgencia;
            this.CodigoCedente = boleto.CedenteBoleto.CodigoCedente;
            this.DigitoCedente = boleto.CedenteBoleto.DigitoCedente;
            this.Convenio = boleto.CedenteBoleto.Convenio;
            this.NomeEmpresa = boleto.CedenteBoleto.Nome;
            this.NumeroRemessaRetorno = sequencialRemessa.ToString();
            this.TipoRegistro = boleto.CarteiraCobranca.BoletoBrToBind().Codigo == "RG" ? 1 : 2;
            this.NumeroContaCorrente = boleto.CedenteBoleto.ContaBancariaCedente.Conta;
            this.DigitoContaCorrente = boleto.CedenteBoleto.ContaBancariaCedente.DigitoConta;
        }

        public HeaderLoteRemessaCnab240(Pagamento pagamento, int sequencialRemessa)
        {
            this.CodigoBanco = pagamento.CodigoBanco;
            this.NumeroInscricao = pagamento.Empresa.CpfCnpj;
            this.Agencia = pagamento.Empresa.ContaBancariaCedente.Agencia;
            this.DigitoAgencia = pagamento.Empresa.ContaBancariaCedente.DigitoAgencia;
            this.Convenio = pagamento.Empresa.Convenio;
            this.DigitoCedente = pagamento.Empresa.DigitoCedente;
            this.Convenio = pagamento.Empresa.Convenio;
            this.NomeEmpresa = pagamento.Empresa.Nome;
            this.NumeroRemessaRetorno = sequencialRemessa.ToString();
            this.TipoRegistro = 1;
            this.TipoServico = pagamento.TipoServico;
            this.NumeroContaCorrente = pagamento.Empresa.ContaBancariaCedente.Conta;
            this.DigitoContaCorrente = pagamento.Empresa.ContaBancariaCedente.DigitoConta;

            this.Mensagem1 = pagamento.Mensagem;
            this.LoteServico = pagamento.LoteServico;

            var cep = pagamento?.Empresa?.EnderecoCedente?.Cep.Replace("-", "").Replace(".", "");

            this.Cep = cep.Substring(0, 5);
            this.ComplementoCep = cep.Substring(5, 3);
            this.Estado = pagamento?.Empresa?.EnderecoCedente?.SiglaUf;
            this.Logradouro = pagamento?.Empresa?.EnderecoCedente?.Logradouro;
            this.Numero = pagamento?.Empresa?.EnderecoCedente?.Numero;
            this.Complemento = pagamento?.Empresa?.EnderecoCedente?.Complemento;
            this.Cidade = pagamento?.Empresa?.EnderecoCedente?.Cidade;

            this.FormaDeLancamento = pagamento.FormaDeLancamento;
            this.FormaDePagamento = pagamento.FormaDePagamento;
        }

        public HeaderLoteRemessaCnab240(Pagamento pagamento, int sequencialRemessa, bool teste)
        {
            this.CodigoBanco = pagamento.CodigoBanco;
            this.NumeroInscricao = pagamento.Empresa.CpfCnpj;
            this.Agencia = pagamento.Empresa.ContaBancariaCedente.Agencia;
            this.DigitoAgencia = pagamento.Empresa.ContaBancariaCedente.DigitoAgencia;
            this.Convenio = pagamento.Empresa.Convenio;
            this.DigitoCedente = pagamento.Empresa.DigitoCedente;
            this.Convenio = pagamento.Empresa.Convenio;
            this.NomeEmpresa = pagamento.Empresa.Nome;
            this.NumeroRemessaRetorno = sequencialRemessa.ToString();
            this.TipoRegistro = 1;
            this.TipoServico = pagamento.TipoServico;
            this.NumeroContaCorrente = pagamento.Empresa.ContaBancariaCedente.Conta;
            this.DigitoContaCorrente = pagamento.Empresa.ContaBancariaCedente.DigitoConta;

            this.Mensagem1 = pagamento.Mensagem;
            this.LoteServico = pagamento.LoteServico;
            this.FormaDeLancamento = pagamento.FormaDeLancamento;
            this.FormaDePagamento = pagamento.FormaDePagamento;

            var cep = pagamento?.Empresa?.EnderecoCedente?.Cep.Replace("-", "").Replace(".", "");

            this.Cep = cep.Substring(0, 5);
            this.ComplementoCep = cep.Substring(5, 3);
            this.Estado = pagamento?.Empresa?.EnderecoCedente?.SiglaUf;
            this.Logradouro = pagamento?.Empresa?.EnderecoCedente?.Logradouro;
            this.Numero = pagamento?.Empresa?.EnderecoCedente?.Numero;
            this.Complemento = pagamento?.Empresa?.EnderecoCedente?.Complemento;
            this.Cidade = pagamento?.Empresa?.EnderecoCedente?.Cidade;

            this.Teste = teste && pagamento.CodigoBanco == "001" ? "TS" : string.Empty; /*Banco Brasil*/
        }
        public string Teste { get; set; }
        public string DigitoContaCorrente { get; set; }

        public string NumeroContaCorrente { get; set; }

        public string CodigoBanco { get; set; }
        public int LoteServico { get; set; }
        public int TipoRegistro { get; set; }
        public string TipoOperacao { get; set; }
        public string TipoServico { get; set; }
        public string VersaoLayoutLote { get; set; }
        public string TipoInscricao { get; set; }
        public string NumeroInscricao { get; set; }
        public string Convenio { get; set; }
        public string Agencia { get; set; }
        public string DigitoAgencia { get; set; }
        public string CodigoCedente { get; set; }
        public int DigitoCedente { get; set; }
        public string CodigoModeloPersonalizado { get; set; }
        public string NomeEmpresa { get; set; }
        public string Mensagem1 { get; set; }
        public string Mensagem2 { get; set; }
        public string NumeroRemessaRetorno { get; set; }
        
        public DateTime DataGravacaoRemessaRetorno { get; set; }
        public DateTime DataCredito { get; set; }

        public string Logradouro { get; set; }
        public string Numero { get; set; }
        public string Complemento { get; set; }
        public string Cidade { get; set; }
        public string Cep { get; set; }
        public string ComplementoCep { get; set; }
        public string Estado { get; set; }
        public string FormaDeLancamento { get; set; }
         
        public string FormaDePagamento { get; set; }
    }
}
