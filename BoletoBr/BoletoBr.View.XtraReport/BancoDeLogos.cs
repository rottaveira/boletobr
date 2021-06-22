﻿using System; 
using System.Drawing; 

namespace BoletoBr.View.XtraReport
{
    public class BancoDeLogos
    {
        public static Image ObterLogoBanco(string codigoBanco)
        {
            var objGenerico =
                LogotiposDeBancos.ResourceManager.GetObject("_" + codigoBanco.PadLeft(3, '0'));

            if (objGenerico == null)
                throw new Exception("Logotipo do banco de código: " + codigoBanco + " não pode ser obtido.");

            var imgRetornar =
                objGenerico as Image;

            return imgRetornar;
        }
    }
}
