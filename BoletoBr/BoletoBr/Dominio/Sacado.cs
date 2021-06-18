using BoletoBr.Dominio;
using System;

namespace BoletoBr
{
    /// <summary>
    /// #Contas a pagar - Favorecido - Fornecedor - quem recebe
    /// #Contas a receber - Pagador - Cliente
    /// </summary>
    public class Sacado
    {
        public string Nome { get; set; }
        public string CpfCnpj { get; set; }
        public Endereco EnderecoSacado { get; set; }
        public string NomeAvalista { get; set; }
        public string CpfCnpjAvalista { get; set; }
        public string CpfCnpjFormatado
        {
            get
            {
                var valorBaseTratado = CpfCnpj;
                valorBaseTratado = valorBaseTratado.Replace(".", "").Replace("-", "").Replace("/", "");

                if (valorBaseTratado.Length > 11)
                    return valorBaseTratado.BoletoBrSetMascara("##.###.###/####-##");

                return valorBaseTratado.BoletoBrSetMascara("###.###.###-##");
            }
        }

        public Sacado(string nome, string cpfCnpj, Endereco endereco)
        {
            this.Nome = nome;
            this.CpfCnpj = cpfCnpj;
            this.EnderecoSacado = endereco;
        }

        public Sacado( string cpfCnpj, string codigo, Endereco endereco, ContaBancaria contaBancaria )
        { 
            this.CpfCnpj = cpfCnpj;
            this.EnderecoSacado = endereco;
            this.ContaBancariaSacado = contaBancaria;
            this.CodigoSacado = codigo;
        }

        public string IdentificacaoClienteBanco { get; set; }

        public ContaBancaria ContaBancariaSacado { get; set; }
        public string CodigoSacado { get; set; }


    }
}
