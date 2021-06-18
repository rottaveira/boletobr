using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraPrinting.BarCode;
using DevExpress.XtraReports.UI;

namespace BoletoBr.View.XtraReport
{
    public partial class BoletoGenericoRpt : DevExpress.XtraReports.UI.XtraReport
    {
        public BoletoGenericoRpt()
        {
            InitializeComponent();
        }
        public BoletoGenericoRpt(string tiponossonumero, bool BancoCaixa = false): this()
        {
            if (BancoCaixa)
            {
                labelSacCaixa1.Visible = true;
                labelSacCaixa2.Visible = true;
                MoraMulta1.Text = MoraMulta2.Text = "( + ) Mora/Multa/Juros"; 
            }

            if (!string.IsNullOrEmpty(tiponossonumero))
                xrLabel48.Text = tiponossonumero;
            else
                xrLabel48.Text = "Agência/ Código do Cedente";
            xrLabel89.Text = xrLabel48.Text;

           
        }
    }
}
