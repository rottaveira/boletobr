using System;
using System.Collections.Generic;
using System.Linq;
using BoletoBr.Arquivo.CNAB240.Remessa;
using BoletoBr.Interfaces;

namespace BoletoBr.Bancos.Sicoob
{
    public class EscritorRemessaPagamentoCnab240Sicoob : IEscritorArquivoRemessaCnab240
    {
        private readonly RemessaCnab240 _remessaEscrever;

        public EscritorRemessaPagamentoCnab240Sicoob(RemessaCnab240 remessaEscrever)
        {
            _remessaEscrever = remessaEscrever;
        }

        public string EscreverHeader(HeaderRemessaCnab240 infoHeader)
        {
            string nomeCedente = infoHeader.NomeEmpresa;

            if (nomeCedente.Length > 30)
                nomeCedente = infoHeader.NomeEmpresa.Substring(0, 30);

            var header = new string(' ', 240);
            try
            {

                header = header.PreencherValorNaLinha(1, 3, infoHeader.CodigoBanco.PadLeft(3, '0'));
                header = header.PreencherValorNaLinha(4, 7, "0000");
                header = header.PreencherValorNaLinha(8, 8, "0");
                header = header.PreencherValorNaLinha(9, 17, string.Empty.PadLeft(9, ' '));
                header = header.PreencherValorNaLinha(18, 18, infoHeader.NumeroInscricao.Replace(".", "").Replace("/", "").Replace("-", "").Length == 11 ? "1" : "2");
                header = header.PreencherValorNaLinha(19, 32, infoHeader.NumeroInscricao.Replace(".", "").Replace("/", "").Replace("-", "").PadLeft(14, '0'));
                header = header.PreencherValorNaLinha(33, 52, infoHeader.CodigoConvenio.PadRight(20, ' '));
                header = header.PreencherValorNaLinha(53, 57, infoHeader.AgenciaMantenedora.PadLeft(5, '0'));
                header = header.PreencherValorNaLinha(58, 58, infoHeader.DigitoAgenciaMantenedora.PadLeft(1, '0').ToUpper());
                header = header.PreencherValorNaLinha(59, 70, infoHeader.NumeroContaCorrente.PadLeft(12, '0'));
                header = header.PreencherValorNaLinha(71, 72, infoHeader.DigitoContaCorrente.PadRight(2, '0').ToUpper());
                header = header.PreencherValorNaLinha(73, 102, nomeCedente.ToUpper().PadRight(30, ' '));
                header = header.PreencherValorNaLinha(103, 132, "SICOOB".PadRight(30, ' '));
                header = header.PreencherValorNaLinha(133, 142, string.Empty.PadRight(10, ' '));

                #region CÓDIGO REMESSA / RETORNO

                /* Código adotado pela FEBRABAN para qualificar o envio ou devoução de arquivo entre Empresa Cliente e o Banco prestador de serviços.
                 * Informar:
                 * 1 - Remessa (Cliente -> Banco)
                 * 2 - Retorno (Banco -> Cliente)
                 * 3 - Remessa Processada (Banco -> Cliente - Pré-Crítica)
                 * 4 - Remessa Processada Parcial (Banco -> Cliente - Pré-Crítica)
                 * 5 - Remessa Rejeitada (Banco -> Cliente - Pré-Crítica)
                 */

                #endregion

                // Código Remessa/Retorno padronizado para "1" no envio do arquivo
                header = header.PreencherValorNaLinha(143, 143, "1");
                header = header.PreencherValorNaLinha(144, 151, DateTime.Now.ToString("ddMMyyyy"));
                header = header.PreencherValorNaLinha(152, 157, DateTime.Now.ToString("HHmmss"));
                header = header.PreencherValorNaLinha(158, 163, infoHeader.SequencialNsa.ToString().PadLeft(6, '0'));
                header = header.PreencherValorNaLinha(164, 166, "087");
                header = header.PreencherValorNaLinha(167, 171, string.Empty.PadLeft(5, '0'));
                header = header.PreencherValorNaLinha(172, 191, string.Empty.PadRight(20, ' '));
                header = header.PreencherValorNaLinha(192, 211, "REMESSA-PAGAMENTO".PadRight(20, ' '));
                header = header.PreencherValorNaLinha(212, 240, string.Empty.PadRight(29, ' '));

                return header;
            }
            catch (Exception e)
            {
                throw new Exception(String.Concat("Falha na geração do HEADER do arquivo de REMESSA.",
                    Environment.NewLine, e));
            }
        }


        public string EscreverHeaderDeLote(HeaderLoteRemessaCnab240 infoHeaderLote, bool headerJ)
        {
            string nomeCedente = infoHeaderLote.NomeEmpresa;
            if (nomeCedente.Length > 30)
                nomeCedente = infoHeaderLote.NomeEmpresa.Substring(0, 30);

            var headerLote = new string(' ', 240);
            try
            {
                headerLote = headerLote.PreencherValorNaLinha(1, 3, infoHeaderLote.CodigoBanco.PadLeft(3, '0')); // Código do Banco na Compensação
                headerLote = headerLote.PreencherValorNaLinha(4, 7, infoHeaderLote.LoteServico.ToString().PadLeft(4, '0')); // Lote de Serviço
                headerLote = headerLote.PreencherValorNaLinha(8, 8, "1"); // Tipo de Registro
                headerLote = headerLote.PreencherValorNaLinha(9, 9, "C");
                /*
                    #Tipo de Serviço
                        Código adotado pela FEBRABAN para indicar o tipo de serviço / produto (processo) contido no arquivo /
                        lote. Domínio:
                        '01' = Cobrança
                        '03' = Bloqueto Eletrônico
                        '04' = Conciliação Bancária
                        '05' = Débitos
                        '06' = Custódia de Cheques
                        '07' = Gestão de Caixa
                        '08' = Consulta/Informação Margem
                        '09' = Averbação da Consignação/Retenção
                        '10' = Pagamento Dividendos
                        ‘11’ = Manutenção da Consignação
                
                    #Tipo serviço
                         Pagamento a Fornecedor = '20'
                         Pagamento de Salário = '30'
                         Pagamentos Diversos = '98
                
                
                    #Forma de lancamento
                        Conta Corrente = '01'
                        DOC/TED = '03'*
                        Poupança = '05'
                        TED Outra Titularidade = '41'*
                        TED Mnesma Titularidade = '43'*
                        * Necessidade de complementação de informação do campo 'Código da Câmara de Compensação', posições18 a 20 do
                        Segmento A
                 */

                var versao = headerJ ? "045" : "040";
                var pagamento = headerJ ? infoHeaderLote.FormaDePagamento.PadRight(2, '0') : string.Empty.PadRight(2, ' ');


                if (infoHeaderLote.TipoServico == null)
                    throw new Exception("Tipo de serviço não informado.");

                headerLote = headerLote.PreencherValorNaLinha(10, 11, infoHeaderLote.TipoServico.PadLeft(2, '0')); // Tipo de Serviço
                headerLote = headerLote.PreencherValorNaLinha(12, 13, infoHeaderLote.FormaDeLancamento.PadLeft(2, '0'));  // Forma de lancamento
                headerLote = headerLote.PreencherValorNaLinha(14, 16, versao); // Nº da versão do Layout do Lote
                headerLote = headerLote.PreencherValorNaLinha(17, 17, " "); // Uso Exclusivo FREBRABAN/CNAB
                headerLote = headerLote.PreencherValorNaLinha(18, 18, infoHeaderLote.NumeroInscricao.Replace(".", "").Replace("/", "").Replace("-", "").Length == 11 ? "1" : "2");
                headerLote = headerLote.PreencherValorNaLinha(19, 32, infoHeaderLote.NumeroInscricao.Replace(".", "").Replace("/", "").Replace("-", "").PadLeft(14, '0'));
                headerLote = headerLote.PreencherValorNaLinha(33, 52, infoHeaderLote.Convenio.BoletoBrToStringSafe().PadLeft(20, '0'));
                headerLote = headerLote.PreencherValorNaLinha(53, 57, infoHeaderLote.Agencia.BoletoBrToStringSafe().PadLeft(5, '0'));
                headerLote = headerLote.PreencherValorNaLinha(58, 58, infoHeaderLote.DigitoAgencia.BoletoBrToStringSafe()).PadRight(1, '0').ToUpper();
                headerLote = headerLote.PreencherValorNaLinha(59, 70, infoHeaderLote.NumeroContaCorrente.BoletoBrToStringSafe().PadLeft(12, '0'));
                headerLote = headerLote.PreencherValorNaLinha(71, 72, infoHeaderLote.DigitoContaCorrente.BoletoBrToStringSafe().PadRight(2, '0').ToUpper());
                headerLote = headerLote.PreencherValorNaLinha(73, 102, nomeCedente.BoletoBrToStringSafe().PadRight(30, ' '));
                headerLote = headerLote.PreencherValorNaLinha(103, 142, infoHeaderLote.Mensagem1.BoletoBrToStringSafe().PadRight(40, ' ')); // Mensagem 1

                var complemento = infoHeaderLote.Complemento.Length > 15 ? infoHeaderLote.Complemento.Substring(0, 15).ToUpper() : infoHeaderLote.Complemento.ToUpper();
                var logradouro = infoHeaderLote.Logradouro.Length > 30 ? infoHeaderLote.Logradouro.Substring(0, 30).ToUpper() : infoHeaderLote.Logradouro.ToUpper();

                headerLote = headerLote.PreencherValorNaLinha(143, 172, logradouro.PadRight(30, ' ').ToUpper());
                headerLote = headerLote.PreencherValorNaLinha(173, 177, infoHeaderLote.Numero.PadLeft(5, '0').ToUpper());
                headerLote = headerLote.PreencherValorNaLinha(178, 192, complemento.PadRight(15, ' ').ToUpper());
                headerLote = headerLote.PreencherValorNaLinha(193, 212, infoHeaderLote.Cidade.PadRight(20, ' ').ToUpper());
                headerLote = headerLote.PreencherValorNaLinha(213, 217, infoHeaderLote.Cep.PadLeft(5, '0'));
                headerLote = headerLote.PreencherValorNaLinha(218, 220, infoHeaderLote.ComplementoCep.PadRight(3, ' '));
                headerLote = headerLote.PreencherValorNaLinha(221, 222, infoHeaderLote.Estado.PadRight(2, ' ').ToUpper());

                /*Indicativo de Forma de Pagamento
                    Possibilitar ao Pagador, mediante acordo com o seu Banco de Relacionamento, a forma de pagamento
                    do compromisso.
                    01 - Débito em Conta Corrente
                    02 – Débito Empréstimo/Financiamento
                    03 – Débito Cartão de Crédito                 */
                headerLote = headerLote.PreencherValorNaLinha(223, 224, pagamento);
                headerLote = headerLote.PreencherValorNaLinha(225, 240, string.Empty.PadRight(16, ' '));

                return headerLote;
            }
            catch (Exception e)
            {
                throw new Exception(
                    String.Format("<BoletoBr>{0}Falha na geração do HEADER DE LOTE do arquivo de REMESSA.",
                        Environment.NewLine), e);
            }
        }

        public string EscreverDetalheSegmentoA(DetalheSegmentoARemessaCnab240 infoSegmentoA)
        {
            var segmentoA = new string(' ', 240);
            try
            {
                segmentoA = segmentoA.PreencherValorNaLinha(1, 3, "756"); // Código do Banco na Compensação
                segmentoA = segmentoA.PreencherValorNaLinha(4, 7, infoSegmentoA.LoteServico.ToString().PadLeft(4, '0')); // Lote De Serviço
                segmentoA = segmentoA.PreencherValorNaLinha(8, 8, "3"); // Tipo de Registro
                segmentoA = segmentoA.PreencherValorNaLinha(9, 13, infoSegmentoA.NumeroRegistro.ToString().PadLeft(5, '0'));
                segmentoA = segmentoA.PreencherValorNaLinha(14, 14, "A"); // Cód. Segmento do Registro Detalhe

                #region TipoMovimento
                /*Tipo de Movimento
                    Código adotado pela FEBRABAN, para identificar o tipo de movimentação enviada no arquivo. Domínio:
                    '0' = Indica INCLUSÃO
                    ‘1’ = Indica CONSULTA
                    '3' = Indica ESTORNO (somente para retorno)
                    '5' = Indica ALTERAÇÃO
                    ‘7’ = Indica LIQUIDAÇAO
                    '9' = Indica EXCLUSÃO
                */
                #endregion
                segmentoA = segmentoA.PreencherValorNaLinha(15, 15, "0"); //Tipo movimento

                #region CodMovimento
                /*
                 Código da Instrução para Movimento
                   Código adotado pela FEBRABAN, para identificar a ação a ser realizada com o lançamento enviado no 
                   arquivo. Domínio:
                   '00' = Inclusão de Registro Detalhe Liberado
                   '05' = Alteração de dados de pagamento e desautoriza
                   '06' = Alteração de dados do pagamento e autoriza
                   '09' = Inclusão do Registro Detalhe Bloqueado
                   '10' = Alteração do Pagamento Liberado para Bloqueado (Bloqueio)
                   '11' = Alteração do Pagamento Bloqueado para Liberado (Liberação)
                   '17' = Alteração do Valor do Título
                   '19' = Alteração da Data de Pagamento
                   '23' = Pagamento Direto ao Fornecedor - Baixar
                   '25' = Manutenção em Carteira - Não Pagar
                   '27' = Retirada de Carteira - Não Pagar
                   '33' = Estorno por Devolução da Câmara Centralizadora (somente para Tipo de Movimento = '3')
                   '40' = Alegação do Sacado
                   Os códigos abaixo são exclusivos para o Bradesco:
                   ‘50’ = Inclusão de conta do favorecido
                   ‘51’ = Alteração de conta do favorecido
                   ‘52’ = Bloqueio de conta do favorecido (Altera situação para 2 -Bloqueada)
                   ‘53’= Desbloqueio conta do favorecido (Altera situação para 1 – Ativa)
                   ‘54’= Exclusão de conta do favorecido
                   ‘60’ = Inclusão de Favorecidos
                   ‘61’ = Alteração de Favorecidos
                */
                #endregion
                segmentoA = segmentoA.PreencherValorNaLinha(16, 17, "00"); // Código de Movimento Remessa

                segmentoA = segmentoA.PreencherValorNaLinha(18, 20, infoSegmentoA.CodigoCamaraCentralizadora.PadLeft(3, '0'));
                segmentoA = segmentoA.PreencherValorNaLinha(21, 23, infoSegmentoA.CodigoBancoFavorecido.PadLeft(3, '0'));
                segmentoA = segmentoA.PreencherValorNaLinha(24, 28, infoSegmentoA.AgenciaMantenedoraFavorecido.PadLeft(5, '0'));
                segmentoA = segmentoA.PreencherValorNaLinha(29, 29, infoSegmentoA.DigitoAgenciaMantenedoraFavorecido);
                segmentoA = segmentoA.PreencherValorNaLinha(30, 41, infoSegmentoA.ContaFavorecido.PadLeft(12, '0'));
                segmentoA = segmentoA.PreencherValorNaLinha(42, 43, infoSegmentoA.DigitoContaFavorecido.PadRight(2, ' '));
                segmentoA = segmentoA.PreencherValorNaLinha(44, 73, infoSegmentoA.NomeFavorecido.PadRight(30, ' '));
                segmentoA = segmentoA.PreencherValorNaLinha(74, 93, infoSegmentoA.SeuNumero.BoletoBrToStringSafe().PadRight(20, ' '));

                var data = infoSegmentoA.DataPagamento.HasValue ? infoSegmentoA.DataPagamento.Value.ToString("ddMMyyyy") : string.Empty;

                segmentoA = segmentoA.PreencherValorNaLinha(94, 101, data.PadLeft(8, '0'));
                segmentoA = segmentoA.PreencherValorNaLinha(102, 104, "BRL");//tipoMoeda
                segmentoA = segmentoA.PreencherValorNaLinha(105, 119, "000000000000000");//qtdMoeda

                var valorPagamento = string.Empty;

                if (infoSegmentoA.ValorPagamento.ToString("f").Contains('.') && infoSegmentoA.ValorPagamento.ToString("f").Contains(','))
                    valorPagamento = infoSegmentoA.ValorPagamento.ToString("f").Replace(".", "").Replace(",", "");
                if (infoSegmentoA.ValorPagamento.ToString("f").Contains('.'))
                    valorPagamento = infoSegmentoA.ValorPagamento.ToString("f").Replace(".", "");
                if (infoSegmentoA.ValorPagamento.ToString("f").Contains(','))
                    valorPagamento = infoSegmentoA.ValorPagamento.ToString("f").Replace(",", "");


                segmentoA = segmentoA.PreencherValorNaLinha(120, 134, valorPagamento.PadLeft(15, '0'));
                segmentoA = segmentoA.PreencherValorNaLinha(135, 154, string.Empty.PadLeft(20, ' ')); //nosso numero - usado no retorno
                segmentoA = segmentoA.PreencherValorNaLinha(155, 162, string.Empty.PadLeft(8, '0')); // data efetivaçao pagamento - usado no retorno
                segmentoA = segmentoA.PreencherValorNaLinha(163, 177, string.Empty.PadLeft(15, '0'));  //valor pago- usado no retorno
                segmentoA = segmentoA.PreencherValorNaLinha(178, 217, string.Empty.PadLeft(40, ' '));

                #region Complemento do Tipo de Serviço
                /*
                 Complemento do Tipo de Serviço
                    Código adotado pela FEBRABAN para identificação da finalidade do DOC (Documento de Ordem de
                    Crédito). Domínio:
                    '01' = Crédito em Conta
                    '02' = Pagamento de Aluguel/Condomínio
                    '03' = Pagamento de Duplicata/Títulos
                    '04' = Pagamento de Dividendos
                    '05' = Pagamento de Mensalidade Escolar
                    '06' = Pagamento de Salários
                    '07' = Pagamento a Fornecedores
                    '08' = Operações de Câmbios/Fundos/Bolsa de Valores
                    '09' = Repasse de Arrecadação/Pagamento de Tributos
                    '10' = Transferência Internacional em Real
                    '11' = DOC para Poupança
                    '12' = DOC para Depósito Judicial
                    '13' = Outros
                    ‘16’ = Pagamento de bolsa auxílio
                    ‘17’ = Remuneração à cooperado
                    ‘18’ = Pagamento de honorários
                    ‘19’ = Pagamento de prebenda (Remuneração a padres e sacerdotes)

                Código adotado pela FEBRABAN para identificação da finalidade do TED. Domínio:
                    '1' Pagamento de Impostos, Tributos e Taxas TED
                    '2' Pagamento à Concessionárias de Serviço Público TED
                    '3' Pagamento de Dividendos TED
                    '4' Pagamento de Salários TED
                    '5' Pagamento de Fornecedores TED
                    '6' Pagamento de Honorários TED
                    '7' Pagamento de Aluguéis e Taxas de Condomínio TED
                    '8' Pagamento de Duplicatas e Títulos TED
                    '9' Pagamento de Mensalidade Escolar TED
                    '10' Crédito em Conta TED
                    '11' Pagamento a Corretoras TED
                    '12' Pagamento de Boleto Bancário em Cartório TED
                    '13' Pagamento de Tarifas pela Prestação de Serviços de Arrecadação de Convênios TED
                    '14' Repasse de Valores Referentes a Títulos Liquidados em Cartórios de Protesto TED
                    '15' Liquidação Financeira de Operadora de Cartão TED
                    '16' Crédito em Conta de Investimento Mantida na IF Destinatária TED
                    '17' Crédito em Conta de Investimento Mantida em Cliente da IF Destinatária TED
                    '18' Operações Seguro Habitacional - SFH TED
                    '19' Operações do FDS - Caixa TED
                    '20' Pagamento de Operações de Crédito TED
                    '21' Crédito para Investimento - Conta de Investimento Mantida na IF Origem TED
                    '22' Crédito para Investimento - Conta de Investimento Mantida em Cliente da IF Origem TED
                    '23' Taxa de Administração TED
                    '24' Crédito para Aplicação em Cliente da IF Destinatária - Conta de Investimento Mantida na TED
                    '25' Crédito para Aplicação em Cliente da IF Destinatária - Conta de Investimento Mantida TED
                    '26' Crédito em CI Mantida na IF Destinatária Proveniente de IF sem Reservas Bancárias TED
                    '27' Pagamento de Acordo / Execução Judicial TED
                    '28' Liquidação de Empréstimos Consignados TED
                    '29' Pagamento de Bolsa Auxílio TED
                    '30' Remuneração à Cooperado TED31 Pagamento de Prebenda (Remuneração a Padres e Sacerdotes) TED
                    '32' Retirada de Recursos da Conta de Investimento TED
                    '33' Pagamento de Juros sobre Capital Próprio TED
                    '34' Pagamento de Rendimentos ou Amortização s/ Cotas e/ou Debêntures TED
                    '35' Taxa de Serviço TED
                    '37' Pagamento de Juros e/ou Amortização de Títulos Depositados em Garantia nas Câmaras TED
                    '38' Estorno ou Restituição – Diversos TED
                    '39' Pagamento de Vale Transporte TED
                    '40' Simples Nacional TED
                    '41' Repasse de Valores para o FUNDEB TED
                    '42' Repasse de Valores de Convênio Centralizado TED
                    '43' Patrocínio com Incentivo Fiscal TED
                    '44' Doação com Incentivo Fiscal TED
                    '45' Transferência de Conta Corrente de Instituição Não Bancária para sua Conta de TED
                    '47' Pagamento de Rescisão de Contrato de Trabalho TED
                    '50' Reembolso de Despesas com Estruturação de Operações de Renda Fixa e Variável TED
                    '97' Compra de Moeda Estrangeira pelo FSB - Fundo Soberano do Brasil TED
                    '100' Depósito Judicial TED
                    '101' Pensão Alimentícia TED
                    '103' Cessão de Créditos - Liquidação Principal por Aquisição de Créditos ou Direitos Creditórios ou Fluxo de Caixa Garantido por Créditos, por Ordem de Cliente PJ Financeira TED
                    '104' Cessão de Créditos - Liquidação Principal por Aquisição de Créditos ou Direitos Creditórios, por Ordem de FIDC ou Cia Securitizadora TED
                    '107' Cessão de Créditos - Repasse Contratual de Fluxo de Caixa ou de Recebíveis Pagos, por TED
                    '108' Cessão de Créditos - Repasse Contratual de Fluxo de Caixa ou de Recebíveis Pagos Antecipadamente, por Ordem de Cliente PJ Financeira TED
                    '109' Cessão de Créditos - Ajustes Diversos TED
                    '110' Transferência entre Contas de Mesma Titularidade TED
                    '111' Crédito para Investidor em Cliente da IF Creditada TED
                    '112' Débito de Investidor em Cliente da IF Debitada TED
                    '113' Pagamento de Operações de Crédito por Cliente TED
                    '114' Resgate de Aplicação Financeira de Cliente para Conta de sua Titularidade TED
                    '117' Aplicação Financeira em Nome do Cliente Remetente TED
                    '123' Cessão de Créditos - Liquidação Principal por Recompra de Créditos ou Direitos Creditórios ou Fluxo de Caixa Garantido por Créditos, por Ordem de Cliente PJ Financeira TED
                    '124' Cessão de Créditos - Liquidação Principal por Recompra de Créditos ou Direitos Creditórios, por Ordem de FIDC ou Cia Securitizadora TED
                    '131' FGCoop - Recolhimento Fundo Garantidor do Cooperativismo de Crédito TED
                    '132' FGCoop - Devolução de Recolhimento a Maior TED
                    '139' Crédito ao Consumidor Decorrente de Programa de Incentivo Fiscal TED
                    '200' Transferência Internacional em Reais TED
                    '201' Ajuste Posição Mercado Futuro TED
                    '202' Repasse de Valores do BNDES TED
                    '203' Liquidação de Compromissos Junto ao BNDES TED
                    '204' Operações de Compra e Venda de Ações - Bolsas de Valores e Mercado de Balcão TED
                    '205' Contratos Referenciados em Ações ou Índices de Ações - Bolsas de Valores, de TED
                    '206' Operação de Câmbio - Não Interbancária TED
                    '207' Operações no Mercado de Renda Fixa e de Renda Variável com Utilização de TED
                    '208' Operação de Câmbio - Mercado Interbancário - Instituições sem Conta Reservas TED
                    '209' Pagamento de Operações com Identificação de Destinatário Final TED300 Restituição de Imposto de Renda TED
                    '301' Ordem Bancária do Tesouro – OBT TED
                    '302' Pagamento de Multas ao BACEN por Atrasos de Importação TED
                    '303' Restituição de Tributos – RFB TED
                    '400' TEA - Transferência Eletrônica Agendada TED
                    '500' Restituição de Prêmios de Seguros TED
                    '501' Pagamento de Indenização de Sinistro de Seguro TED
                    '502' Pagamento de Prêmio de Co-Seguro TED
                    '503' Restituição de Prêmio de Co-Seguro TED
                    '504' Pagamento de Indenização de Sinistro de Co-Seguro TED
                    '505' Pagamento de Prêmio de Resseguro TED
                    '506' Restituição de Prêmio de Resseguro TED
                    '507' Pagamento de Indenização de Sinistro de Resseguro TED
                    '508' Restituição de Indenização de Sinistro de Resseguro TED
                    '509' Pagamento de Despesas com Sinistros TED
                    '510' Pagamento de Inspeções/Vistorias Prévias TED
                    '511' Pagamento de Resgate de Título da Capitalização TED
                    '512' Pagamento de Sorteio de Título de Capitalização TED
                    '513' Pagamento de Devolução de Mensalidade de Título de Capitalização TED
                    '514' Restituição de Contribuição de Plano Previdenciário TED
                    '515' Pagamento de Benefício Previdenciário de Pecúlio TED
                    '516' Pagamento de Benefício Previdenciário de Pensão TED
                    '517' Pagamento de Benefício Previdenciário de Aposentadoria TED
                    '518' Pagamento de Resgate Previdenciário TED
                    '519' Pagamento de Comissão de Corretagem TED
                    '520' Pagamento de Transferências/Portabilidade de Reserva de Seguro/Previdência TED
                    '99999' Outros TED
                */
                #endregion


                var compTed = string.Empty;
                var codFinalidadeDoc = string.Empty;
                var codFinalidadeTed = string.Empty;
                if (infoSegmentoA.CodigoCamaraCentralizadora != "700")
                {
                    compTed = infoSegmentoA.FinalidadePagamento;
                    codFinalidadeTed = infoSegmentoA.CodigoFinalidadeTed;

                }
                else
                {
                    codFinalidadeDoc = infoSegmentoA.CodigoFinalidadeDoc;
                }

                segmentoA = segmentoA.PreencherValorNaLinha(218, 219, codFinalidadeDoc.PadRight(2, ' ')); //Cod finalidade DOC
                segmentoA = segmentoA.PreencherValorNaLinha(220, 224, codFinalidadeTed.PadRight(5, ' ')); //Cod Finalidade TED

                segmentoA = segmentoA.PreencherValorNaLinha(225, 226, compTed.PadLeft(2, ' ')); //Compl. Finalidade de Pagamento
                segmentoA = segmentoA.PreencherValorNaLinha(227, 229, string.Empty.PadLeft(3, ' '));
                segmentoA = segmentoA.PreencherValorNaLinha(230, 230, "0");
                segmentoA = segmentoA.PreencherValorNaLinha(231, 240, string.Empty.PadLeft(10, ' '));

                return segmentoA;
            }
            catch (Exception e)
            {
                throw new Exception(String.Format("<BoletoBr>{0}Falha na geração do DETALHE - Segmento A do arquivo de REMESSA.",
                    Environment.NewLine), e);
            }
        }

        public string EscreverDetalheSegmentoB(DetalheSegmentoBRemessaCnab240 infoSegmentoB)
        {
            var segmentoB = new string(' ', 240);

            try
            {
                segmentoB = segmentoB.PreencherValorNaLinha(1, 3, "756");
                segmentoB = segmentoB.PreencherValorNaLinha(4, 7, infoSegmentoB.LoteServico.ToString().PadLeft(4, '0'));
                segmentoB = segmentoB.PreencherValorNaLinha(8, 8, "3");
                segmentoB = segmentoB.PreencherValorNaLinha(9, 13, infoSegmentoB.NumeroRegistro.ToString().PadLeft(5, '0'));
                segmentoB = segmentoB.PreencherValorNaLinha(14, 14, "B");
                segmentoB = segmentoB.PreencherValorNaLinha(15, 17, string.Empty.PadLeft(3, ' '));
                segmentoB = segmentoB.PreencherValorNaLinha(18, 18, infoSegmentoB.NumeroInscricaoFavorecido.Replace(".", "").Replace("-", "").Replace("-", "").Length == 11 ? "1" : "2");
                segmentoB = segmentoB.PreencherValorNaLinha(19, 32, infoSegmentoB.NumeroInscricaoFavorecido.Replace(".", "").Replace("/", "").Replace("-", "").PadLeft(14, '0'));
                segmentoB = segmentoB.PreencherValorNaLinha(33, 62, infoSegmentoB.LogradouroFavorecido.PadRight(30, ' ').Substring(0, 30).ToUpper());
                segmentoB = segmentoB.PreencherValorNaLinha(63, 67, infoSegmentoB.NumeroFavorecido.PadLeft(5, '0'));
                segmentoB = segmentoB.PreencherValorNaLinha(68, 82, infoSegmentoB.ComplementoFavorecido.PadRight(15, ' ').Substring(0, 15).ToUpper());
                segmentoB = segmentoB.PreencherValorNaLinha(83, 97, infoSegmentoB.BairroFavorecido.PadRight(15, ' ').Substring(0, 15).ToUpper());
                segmentoB = segmentoB.PreencherValorNaLinha(98, 117, infoSegmentoB.CidadeFavorecido.PadRight(20, ' ').Substring(0, 20).ToUpper());
                segmentoB = segmentoB.PreencherValorNaLinha(118, 122, infoSegmentoB.CEPFavorecido.PadLeft(5, '0'));
                segmentoB = segmentoB.PreencherValorNaLinha(123, 125, infoSegmentoB.ComplementoCEPFavorecido.PadRight(3, ' '));
                segmentoB = segmentoB.PreencherValorNaLinha(126, 127, infoSegmentoB.EstadoFavorecido.PadRight(2, ' ').ToUpper());

                var data = infoSegmentoB.DataVencimento.HasValue ? infoSegmentoB.DataVencimento.Value.ToString("ddMMyyyy") : "";
                segmentoB = segmentoB.PreencherValorNaLinha(128, 135, data.PadLeft(8, '0'));

                #region Valores
                var valorPagamento = string.Empty;

                if (infoSegmentoB.ValorPagamento.BoletoBrToStringSafe().BoletoBrToDecimal().ToString("f").Contains('.') && infoSegmentoB.ValorPagamento.BoletoBrToStringSafe().BoletoBrToDecimal().ToString("f").Contains(','))
                    valorPagamento = infoSegmentoB.ValorPagamento.BoletoBrToStringSafe().BoletoBrToDecimal().ToString("f").Replace(".", "").Replace(",", "");
                if (infoSegmentoB.ValorPagamento.BoletoBrToStringSafe().BoletoBrToDecimal().ToString("f").Contains('.'))
                    valorPagamento = infoSegmentoB.ValorPagamento.BoletoBrToStringSafe().BoletoBrToDecimal().ToString("f").Replace(".", "");
                if (infoSegmentoB.ValorPagamento.BoletoBrToStringSafe().BoletoBrToDecimal().ToString("f").Contains(','))
                    valorPagamento = infoSegmentoB.ValorPagamento.BoletoBrToStringSafe().BoletoBrToDecimal().ToString("f").Replace(",", "");

                var valorAbatimento = string.Empty;

                if (infoSegmentoB.ValorAbatimento.GetValueOrDefault().ToString("f").Contains('.') && infoSegmentoB.ValorAbatimento.GetValueOrDefault().ToString("f").Contains(','))
                    valorAbatimento = infoSegmentoB.ValorAbatimento.GetValueOrDefault().ToString("f").Replace(".", "").Replace(",", "");
                if (infoSegmentoB.ValorPagamento.BoletoBrToStringSafe().BoletoBrToDecimal().ToString("f").Contains('.'))
                    valorAbatimento = infoSegmentoB.ValorAbatimento.GetValueOrDefault().ToString("f").Replace(".", "");
                if (infoSegmentoB.ValorPagamento.BoletoBrToStringSafe().BoletoBrToDecimal().ToString("f").Contains(','))
                    valorAbatimento = infoSegmentoB.ValorAbatimento.GetValueOrDefault().ToString("f").Replace(",", "");

                var valorDesconto = string.Empty;

                if (infoSegmentoB.ValorDesconto.GetValueOrDefault().ToString("f").Contains('.') && infoSegmentoB.ValorDesconto.GetValueOrDefault().ToString("f").Contains(','))
                    valorDesconto = infoSegmentoB.ValorDesconto.GetValueOrDefault().ToString("f").Replace(".", "").Replace(",", "");
                if (infoSegmentoB.ValorPagamento.BoletoBrToStringSafe().BoletoBrToDecimal().ToString("f").Contains('.'))
                    valorDesconto = infoSegmentoB.ValorDesconto.GetValueOrDefault().ToString("f").Replace(".", "");
                if (infoSegmentoB.ValorPagamento.BoletoBrToStringSafe().BoletoBrToDecimal().ToString("f").Contains(','))
                    valorDesconto = infoSegmentoB.ValorDesconto.GetValueOrDefault().ToString("f").Replace(",", "");

                var valorMora = string.Empty;

                if (infoSegmentoB.ValorJurosMora.GetValueOrDefault().ToString("f").Contains('.') && infoSegmentoB.ValorJurosMora.GetValueOrDefault().ToString("f").Contains(','))
                    valorMora = infoSegmentoB.ValorJurosMora.GetValueOrDefault().ToString("f").Replace(".", "").Replace(",", "");
                if (infoSegmentoB.ValorPagamento.BoletoBrToStringSafe().BoletoBrToDecimal().ToString("f").Contains('.'))
                    valorMora = infoSegmentoB.ValorJurosMora.GetValueOrDefault().ToString("f").Replace(".", "");
                if (infoSegmentoB.ValorPagamento.BoletoBrToStringSafe().BoletoBrToDecimal().ToString("f").Contains(','))
                    valorMora = infoSegmentoB.ValorJurosMora.GetValueOrDefault().ToString("f").Replace(",", "");

                var valorMulta = string.Empty;

                if (infoSegmentoB.ValorMulta.GetValueOrDefault().ToString("f").Contains('.') && infoSegmentoB.ValorMulta.GetValueOrDefault().ToString("f").Contains(','))
                    valorMulta = infoSegmentoB.ValorMulta.GetValueOrDefault().ToString("f").Replace(".", "").Replace(",", "");
                if (infoSegmentoB.ValorPagamento.BoletoBrToStringSafe().BoletoBrToDecimal().ToString("f").Contains('.'))
                    valorMulta = infoSegmentoB.ValorMulta.GetValueOrDefault().ToString("f").Replace(".", "");
                if (infoSegmentoB.ValorPagamento.BoletoBrToStringSafe().BoletoBrToDecimal().ToString("f").Contains(','))
                    valorMulta = infoSegmentoB.ValorMulta.GetValueOrDefault().ToString("f").Replace(",", "");
                #endregion

                segmentoB = segmentoB.PreencherValorNaLinha(136, 150, valorPagamento.PadLeft(15, '0'));
                segmentoB = segmentoB.PreencherValorNaLinha(151, 165, valorAbatimento.PadLeft(15, '0'));
                segmentoB = segmentoB.PreencherValorNaLinha(166, 180, valorDesconto.PadLeft(15, '0'));
                segmentoB = segmentoB.PreencherValorNaLinha(181, 195, valorMora.PadLeft(15, '0'));
                segmentoB = segmentoB.PreencherValorNaLinha(196, 210, valorMulta.PadLeft(15, '0'));
                segmentoB = segmentoB.PreencherValorNaLinha(211, 225, infoSegmentoB.CodigoFavorecido.PadRight(15, ' '));
                segmentoB = segmentoB.PreencherValorNaLinha(226, 226, "0");
                segmentoB = segmentoB.PreencherValorNaLinha(227, 232, "000000");
                segmentoB = segmentoB.PreencherValorNaLinha(233, 240, "        ");

                return segmentoB;
            }
            catch (Exception e)
            {
                throw new Exception(
                    String.Format("<BoletoBr>{0}Falha na geração do DETALHE - Segmento B do arquivo de REMESSA.",
                        Environment.NewLine), e);
            }
        }
        public string EscreverDetalheSegmentoJ(DetalheSegmentoJRemessaCnab240 infoSegmentoJ)
        {
            var segmentoJ = new string(' ', 240);
            try
            {
                segmentoJ = segmentoJ.PreencherValorNaLinha(1, 3, infoSegmentoJ.CodigoBanco.PadLeft(3, '0')); // Código do Banco na Compensação
                segmentoJ = segmentoJ.PreencherValorNaLinha(4, 7, infoSegmentoJ.LoteServico.ToString().PadLeft(4, '0')); // Lote De Serviço
                segmentoJ = segmentoJ.PreencherValorNaLinha(8, 8, "3"); // Tipo de Registro
                segmentoJ = segmentoJ.PreencherValorNaLinha(9, 13, infoSegmentoJ.NumeroRegistro.ToString().PadLeft(5, '0'));
                segmentoJ = segmentoJ.PreencherValorNaLinha(14, 14, "J"); // Cód. Segmento do Registro Detalhe


                segmentoJ = segmentoJ.PreencherValorNaLinha(15, 15, "0"); //Tipo movimento                
                segmentoJ = segmentoJ.PreencherValorNaLinha(16, 17, "00"); // Código de Movimento Remessa

                if (infoSegmentoJ.CodBarras.Length != 44)
                    throw new Exception("Codigo de barras inválido.");

                segmentoJ = segmentoJ.PreencherValorNaLinha(18, 61, infoSegmentoJ.CodBarras.PadLeft(44, '0'));
                segmentoJ = segmentoJ.PreencherValorNaLinha(62, 91, infoSegmentoJ.NomeCedente.PadRight(30, ' '));
                segmentoJ = segmentoJ.PreencherValorNaLinha(92, 99, infoSegmentoJ.DataVencimento.ToString("ddMMyyyy").PadLeft(8, '0'));

                var valorTitulo = string.Empty;
                var valorPagamento = string.Empty;
                var valorDescontoAbatimento = string.Empty;
                var valorMoraMulta = string.Empty;

                if (infoSegmentoJ.ValorTitulo <= 0)
                    throw new Exception("Titulo não possui valor informado.");

                #region Valores
                var calcDesc = infoSegmentoJ.ValorDesconto + infoSegmentoJ.ValorAbatimento;
                var calcMulta = infoSegmentoJ.ValorMora + infoSegmentoJ.ValorMulta;

                if (calcMulta.ToString("f").Contains('.') && calcMulta.ToString("f").Contains(','))
                    valorMoraMulta = calcMulta.ToString("f").Replace(".", "").Replace(",", "");
                if (calcMulta.ToString("f").Contains('.'))
                    valorMoraMulta = calcMulta.ToString("f").Replace(".", "");
                if (calcMulta.ToString("f").Contains(','))
                    valorMoraMulta = calcMulta.ToString("f").Replace(",", "");

                if (calcDesc.ToString("f").Contains('.') && calcDesc.ToString("f").Contains(','))
                    valorDescontoAbatimento = calcDesc.ToString("f").Replace(".", "").Replace(",", "");
                if (calcDesc.ToString("f").Contains('.'))
                    valorDescontoAbatimento = calcDesc.ToString("f").Replace(".", "");
                if (calcDesc.ToString("f").Contains(','))
                    valorDescontoAbatimento = calcDesc.ToString("f").Replace(",", "");


                if (infoSegmentoJ.ValorPagamento.ToString("f").Contains('.') && infoSegmentoJ.ValorPagamento.ToString("f").Contains(','))
                    valorPagamento = infoSegmentoJ.ValorPagamento.ToString("f").Replace(".", "").Replace(",", "");
                if (infoSegmentoJ.ValorPagamento.ToString("f").Contains('.'))
                    valorPagamento = infoSegmentoJ.ValorPagamento.ToString("f").Replace(".", "");
                if (infoSegmentoJ.ValorPagamento.ToString("f").Contains(','))
                    valorPagamento = infoSegmentoJ.ValorPagamento.ToString("f").Replace(",", "");

                if (infoSegmentoJ.ValorTitulo.ToString("f").Contains('.') && infoSegmentoJ.ValorTitulo.ToString("f").Contains(','))
                    valorTitulo = infoSegmentoJ.ValorTitulo.ToString("f").Replace(".", "").Replace(",", "");
                if (infoSegmentoJ.ValorTitulo.ToString("f").Contains('.'))
                    valorTitulo = infoSegmentoJ.ValorTitulo.ToString("f").Replace(".", "");
                if (infoSegmentoJ.ValorTitulo.ToString("f").Contains(','))
                    valorTitulo = infoSegmentoJ.ValorTitulo.ToString("f").Replace(",", "");
                #endregion

                segmentoJ = segmentoJ.PreencherValorNaLinha(100, 114, valorTitulo.PadLeft(15, '0'));
                segmentoJ = segmentoJ.PreencherValorNaLinha(115, 129, valorDescontoAbatimento.PadLeft(15, '0'));
                segmentoJ = segmentoJ.PreencherValorNaLinha(130, 144, valorMoraMulta.PadLeft(15, '0'));
                segmentoJ = segmentoJ.PreencherValorNaLinha(145, 152, string.Empty.PadLeft(8, '0'));
                segmentoJ = segmentoJ.PreencherValorNaLinha(153, 167, valorPagamento.PadLeft(15, '0'));
                segmentoJ = segmentoJ.PreencherValorNaLinha(168, 182, "000000000000000");//qtdMoeda
                segmentoJ = segmentoJ.PreencherValorNaLinha(183, 202, infoSegmentoJ.SeuNumero.PadRight(20, ' '));
                segmentoJ = segmentoJ.PreencherValorNaLinha(203, 222, string.Empty.PadRight(20, ' '));
                segmentoJ = segmentoJ.PreencherValorNaLinha(223, 224, "09");
                segmentoJ = segmentoJ.PreencherValorNaLinha(225, 230, string.Empty.PadLeft(6, ' '));
                segmentoJ = segmentoJ.PreencherValorNaLinha(231, 240, string.Empty.PadLeft(10, ' '));

                return segmentoJ;
            }
            catch (Exception e)
            {
                throw new Exception(String.Format("<BoletoBr>{0}Falha na geração do DETALHE - Segmento J do arquivo de REMESSA.",
                    Environment.NewLine), e);
            }
        }
        public string EscreverTrailerDeLote(TrailerLoteRemessaCnab240 infoTrailerLote, decimal? totalPagamentoLote)
        {
            if (infoTrailerLote.QtdRegistrosLote == 0)
                throw new Exception("Sequencial do registro no lote não foi informado na geração do TRAILER DE LOTE.");

            var trailerLote = new string(' ', 240);
            try
            {
                var total = totalPagamentoLote.ToString().Replace(".", "").Replace(",", "");

                trailerLote = trailerLote.PreencherValorNaLinha(1, 3, "756");
                trailerLote = trailerLote.PreencherValorNaLinha(4, 7, infoTrailerLote.LoteServico.ToString().PadLeft(4, '0'));
                trailerLote = trailerLote.PreencherValorNaLinha(8, 8, "5");
                trailerLote = trailerLote.PreencherValorNaLinha(9, 17, string.Empty.PadLeft(9, ' '));
                trailerLote = trailerLote.PreencherValorNaLinha(18, 23, infoTrailerLote.QtdRegistrosLote.ToString().PadLeft(6, '0'));

                trailerLote = trailerLote.PreencherValorNaLinha(24, 41, total.PadLeft(18, '0'));
                trailerLote = trailerLote.PreencherValorNaLinha(42, 59, "000000000000000000");
                trailerLote = trailerLote.PreencherValorNaLinha(60, 65, "000000");
                trailerLote = trailerLote.PreencherValorNaLinha(66, 230, string.Empty.PadLeft(165, ' '));
                trailerLote = trailerLote.PreencherValorNaLinha(231, 240, string.Empty.PadLeft(10, ' '));

                return trailerLote;
            }
            catch (Exception e)
            {
                throw new Exception(String.Format("<BoletoBr>{0}Falha na geração do TRAILER DE LOTE do arquivo de REMESSA.",
                   Environment.NewLine), e);
            }
        }

        public string EscreverTrailer(TrailerRemessaCnab240 infoTrailer)
        {
            if (infoTrailer.QtdRegistrosArquivo == 0)
                throw new Exception("Não foi informada a quantidade de registros do arquivo.");

            var trailer = new string(' ', 240);
            try
            {
                trailer = trailer.PreencherValorNaLinha(1, 3, "001");
                trailer = trailer.PreencherValorNaLinha(4, 7, "9999");
                trailer = trailer.PreencherValorNaLinha(8, 8, "9");
                trailer = trailer.PreencherValorNaLinha(9, 17, string.Empty.PadLeft(9, ' '));
                trailer = trailer.PreencherValorNaLinha(18, 23, infoTrailer.QtdLotesArquivo.ToString().PadLeft(6, '0'));
                trailer = trailer.PreencherValorNaLinha(24, 29, infoTrailer.QtdRegistrosArquivo.ToString().PadLeft(6, '0'));
                trailer = trailer.PreencherValorNaLinha(30, 35, infoTrailer.QtdContasConciliacao.ToString().PadLeft(6, '0'));
                trailer = trailer.PreencherValorNaLinha(36, 240, string.Empty.PadLeft(205, ' '));

                return trailer;
            }
            catch (Exception e)
            {
                throw new Exception(String.Format("<BoletoBr>{0}Falha na geração do TRAILER do arquivo de REMESSA.",
                    Environment.NewLine), e);
            }
        }

        public List<string> EscreverTexto(RemessaCnab240 remessaEscrever)
        {
            ValidarRemessa(remessaEscrever);

            var listaRet = new List<string>();

            /* Header */
            listaRet.Add(EscreverHeader(remessaEscrever.Header));

            /* Detalhes */
            /* Caso não venha a sequencia do lote informada no header criar a mesma */
            var sequenciaLote = 1;
            foreach (var loteEscrever in remessaEscrever.Lotes)
            {
                if (loteEscrever.HeaderLote.LoteServico.BoletoBrToStringSafe().BoletoBrToInt() == 0)
                    loteEscrever.HeaderLote.LoteServico = sequenciaLote;

                if (loteEscrever.TrailerLote.LoteServico.BoletoBrToStringSafe().BoletoBrToInt() == 0)
                    loteEscrever.TrailerLote.LoteServico = sequenciaLote;

                listaRet.AddRange(EscreverLote(loteEscrever));

                sequenciaLote++;
            }

            /* Trailer */
            listaRet.Add(EscreverTrailer(remessaEscrever.Trailer));

            return listaRet;
        }

        private List<string> EscreverLote(LoteRemessaCnab240 loteEscrever)
        {
            var listaRet = new List<string>();

            var segAB = loteEscrever.RegistrosDetalheSegmentos.Where(w => w.SegmentoA != null || w.SegmentoB != null).ToList();
            var LoteServico = 0;
            if (segAB.Any(a => a.SegmentoA != null))
            {
                LoteServico++;
                loteEscrever.HeaderLote.LoteServico = LoteServico;
                loteEscrever.TrailerLote.LoteServico = LoteServico;
                listaRet.Add(EscreverHeaderDeLote(loteEscrever.HeaderLote, false));

                foreach (var detalhe in segAB)
                {
                    if (detalhe.SegmentoA.LoteServico.BoletoBrToStringSafe().BoletoBrToInt() == 0)
                        detalhe.SegmentoA.LoteServico = loteEscrever.HeaderLote.LoteServico;

                    if (detalhe.SegmentoB.LoteServico.BoletoBrToStringSafe().BoletoBrToInt() == 0)
                        detalhe.SegmentoB.LoteServico = loteEscrever.HeaderLote.LoteServico;

                    listaRet.Add(EscreverDetalheSegmentoA(detalhe.SegmentoA));
                    listaRet.Add(EscreverDetalheSegmentoB(detalhe.SegmentoB));
                }

                var totalLote = segAB.Sum(s => s.SegmentoA.ValorPagamento);
                listaRet.Add(EscreverTrailerDeLote(loteEscrever.TrailerLote, totalLote)); 
            }

            var segJ = loteEscrever.RegistrosDetalheSegmentos.Where(w => w.SegmentoJ != null).ToList();
            if (segJ.Any(a => a.SegmentoJ != null))
            {
                LoteServico++;
                loteEscrever.HeaderLote.LoteServico = LoteServico;
                loteEscrever.TrailerLote.LoteServico = LoteServico;
                listaRet.Add(EscreverHeaderDeLote(loteEscrever.HeaderLote, true));

                foreach (var detalhe in segJ)
                {
                    if (detalhe.SegmentoJ.LoteServico.BoletoBrToStringSafe().BoletoBrToInt() == 0)
                        detalhe.SegmentoJ.LoteServico = loteEscrever.HeaderLote.LoteServico;

                    listaRet.Add(EscreverDetalheSegmentoJ(detalhe.SegmentoJ));
                }

                var totais = segJ.Sum(s => s.SegmentoJ.ValorPagamento);
                listaRet.Add(EscreverTrailerDeLote(loteEscrever.TrailerLote, totais));
            }

            return listaRet;
        }


        public void ValidarRemessa(RemessaCnab240 remessaValidar)
        {
            if (remessaValidar == null)
                throw new Exception("Não há informações para geração do arquivo de remessa.");

            if (remessaValidar.Header == null)
                throw new Exception("Não há informações para geração do HEADER no arquivo de remessa.");

            if (remessaValidar.Lotes == null)
                throw new Exception("Não há informações para geração dos LOTES no arquivo de remessa.");

            if (remessaValidar.Trailer == null)
                throw new Exception("Não há informações para geração do TRAILER no arquivo de remessa.");

            #region #HEADER

            if (String.IsNullOrEmpty(remessaValidar.Header.NumeroInscricao))
                throw new Exception("CPF/CNPJ do Cedente não foi informado.");

            if (String.IsNullOrEmpty(remessaValidar.Header.AgenciaMantenedora))
                throw new Exception("Agência do Cedente não foi informada.");

            if (String.IsNullOrEmpty(remessaValidar.Header.DigitoAgenciaMantenedora))
                throw new Exception("Dígito da agência do Cedente não foi informada.");

            if (String.IsNullOrEmpty(remessaValidar.Header.CodigoCedente))
                throw new Exception("Código do Cedente não foi informado.");

            if (String.IsNullOrEmpty(remessaValidar.Header.NomeEmpresa))
                throw new Exception("Nome do Cedente não foi informado.");

            #endregion #HEADER

            #region #LOTES

            //foreach (var lote in remessaValidar.Lotes)
            //{
            //    if (String.IsNullOrEmpty(lote.HeaderLote.Convenio))
            //        throw new Exception("O número do convênio não foi informado para geração do HEADER DE LOTE no arquivo de remessa.");

            //    if (lote.TrailerLote.QtdRegistrosLote <= 0)
            //        throw new Exception("A quantidade de registros do arquivo não foi informada no TRAILER.");
            //}

            #endregion #LOTES

            #region #TRAILER

            if (remessaValidar.Trailer.QtdLotesArquivo <= 0)
                throw new Exception("O número de lotes do arquivo não foi informado no registro TRAILER.");

            if (remessaValidar.Trailer.QtdRegistrosArquivo <= 0)
                throw new Exception("O número de registros do arquivo não foi informado no registro TRAILER");

            #endregion #TRAILER
        }
    }
}