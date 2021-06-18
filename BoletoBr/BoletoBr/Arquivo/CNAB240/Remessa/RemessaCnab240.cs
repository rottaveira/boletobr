using System.Collections.Generic;

namespace BoletoBr.Arquivo.CNAB240.Remessa
{
    public class RemessaCnab240
    {
        public RemessaCnab240()
        {
            Lotes = new List<LoteRemessaCnab240>();
        }
        public HeaderRemessaCnab240 Header { get; set; }
        public List<LoteRemessaCnab240> Lotes { get; set; }
        public TrailerRemessaCnab240 Trailer { get; set; }

        public LoteRemessaCnab240 AdicionarLote(HeaderLoteRemessaCnab240 headerLote, TrailerLoteRemessaCnab240 trailerLote)
        {
            var loteAdd = new LoteRemessaCnab240();
            loteAdd.HeaderLote = headerLote;
            loteAdd.TrailerLote = trailerLote;

            Lotes.Add(loteAdd);
            return loteAdd;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loteContainer">Lote onde serão adicionados os lançamentos</param>
        /// <param name="boletoAdicionar">Boleto que será detalhado nos detalhes de segmento</param>
        /// <param name="reg1">Primeiro registro detalhe do boleto - SEGMENTO P</param>
        /// <param name="reg2">Segundo registro detalhe do boleto - SEGMENTO Q</param>
        /// <returns></returns>
        public DetalheRemessaCnab240 AdicionarBoletoAoLote(LoteRemessaCnab240 loteContainer, Boleto boletoAdicionar, int contador, int reg1, int reg2, int reg3)
        {
            var detalheRemessaAdd = new DetalheRemessaCnab240(boletoAdicionar, contador);
            var numeroRegistroNoLote = 0;

            numeroRegistroNoLote = reg1;
            detalheRemessaAdd.SegmentoP = new DetalheSegmentoPRemessaCnab240(boletoAdicionar, numeroRegistroNoLote);
            numeroRegistroNoLote = reg2;
            detalheRemessaAdd.SegmentoQ = new DetalheSegmentoQRemessaCnab240(boletoAdicionar, numeroRegistroNoLote);
            if (reg3 > 0)
            {
                numeroRegistroNoLote = reg3;
                detalheRemessaAdd.SegmentoR = new DetalheSegmentoRRemessaCnab240(boletoAdicionar, numeroRegistroNoLote);
            }
            loteContainer.RegistrosDetalheSegmentos.Add(detalheRemessaAdd);

            return detalheRemessaAdd;
        }

        public DetalheRemessaCnab240 AdicionarBoletoAoLote(LoteRemessaCnab240 loteContainer, Pagamento pagamentoAdicionar, int contador, int reg1, int reg2)
        {
            var detalheRemessaAdd = new DetalheRemessaCnab240(pagamentoAdicionar, contador);
            var numeroRegistroNoLote = 0;

            if (string.IsNullOrEmpty(pagamentoAdicionar.CodBarras))
            {
                numeroRegistroNoLote = reg1;
                detalheRemessaAdd.SegmentoA = new DetalheSegmentoARemessaCnab240(pagamentoAdicionar, numeroRegistroNoLote);
                numeroRegistroNoLote = reg2;
                detalheRemessaAdd.SegmentoB = new DetalheSegmentoBRemessaCnab240(pagamentoAdicionar, numeroRegistroNoLote);
            }
            else
            {
                /*é adicionado em lote separado dos registros AB*/
                numeroRegistroNoLote = reg1;
                detalheRemessaAdd.SegmentoJ = new DetalheSegmentoJRemessaCnab240(pagamentoAdicionar, numeroRegistroNoLote);
            }

            loteContainer.RegistrosDetalheSegmentos.Add(detalheRemessaAdd);

            return detalheRemessaAdd;
        }
    }
}
