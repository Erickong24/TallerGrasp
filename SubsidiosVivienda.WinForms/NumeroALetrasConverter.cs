using System.Globalization;
using System.Text;

namespace SubsidiosVivienda.WinForms
{
    internal static class NumeroALetrasConverter
    {
        private static readonly string[] Especiales =
        {
            "cero", "uno", "dos", "tres", "cuatro", "cinco", "seis", "siete", "ocho", "nueve",
            "diez", "once", "doce", "trece", "catorce", "quince", "dieciséis", "diecisiete",
            "dieciocho", "diecinueve", "veinte", "veintiuno", "veintidós", "veintitrés", "veinticuatro",
            "veinticinco", "veintiséis", "veintisiete", "veintiocho", "veintinueve"
        };

        private static readonly string[] Decenas =
        {
            string.Empty, string.Empty, "veinte", "treinta", "cuarenta", "cincuenta",
            "sesenta", "setenta", "ochenta", "noventa"
        };

        private static readonly string[] Centenas =
        {
            string.Empty, "ciento", "doscientos", "trescientos", "cuatrocientos",
            "quinientos", "seiscientos", "setecientos", "ochocientos", "novecientos"
        };

        public static string Convertir(decimal valor)
        {
            long entero = (long)decimal.Truncate(valor);
            if (entero == 0)
            {
                return "CERO PESOS";
            }

            var letras = ConvertirEntero(entero).Trim();
            var cultura = new CultureInfo("es-CO");

            if (entero == 1)
            {
                return "UN PESO";
            }

            var sufijo = entero >= 1_000_000 ? " de pesos" : " pesos";
            return (letras + sufijo).ToUpper(cultura);
        }

        private static string ConvertirEntero(long numero)
        {
            if (numero < 0)
            {
                return "menos " + ConvertirEntero(-numero);
            }

            if (numero < 30)
            {
                return Especiales[numero];
            }

            if (numero < 100)
            {
                int decena = (int)numero / 10;
                int resto = (int)numero % 10;

                if (numero < 30)
                {
                    return Especiales[numero];
                }

                if (resto == 0)
                {
                    return Decenas[decena];
                }

                if (decena == 2)
                {
                    return Especiales[20 + resto];
                }

                return Decenas[decena] + " y " + Especiales[resto];
            }

            if (numero < 1000)
            {
                int centena = (int)numero / 100;
                int resto = (int)numero % 100;

                if (numero == 100)
                {
                    return "cien";
                }

                var sb = new StringBuilder();
                sb.Append(Centenas[centena]);
                if (resto > 0)
                {
                    sb.Append(' ');
                    sb.Append(ConvertirEntero(resto));
                }

                return sb.ToString();
            }

            if (numero < 1_000_000)
            {
                long miles = numero / 1000;
                int resto = (int)(numero % 1000);
                var sb = new StringBuilder();

                if (miles == 1)
                {
                    sb.Append("mil");
                }
                else
                {
                    sb.Append(ConvertirEntero(miles));
                    sb.Append(" mil");
                }

                if (resto > 0)
                {
                    sb.Append(' ');
                    sb.Append(ConvertirEntero(resto));
                }

                return sb.ToString();
            }

            if (numero < 1_000_000_000)
            {
                long millones = numero / 1_000_000;
                long resto = numero % 1_000_000;
                var sb = new StringBuilder();

                if (millones == 1)
                {
                    sb.Append("un millón");
                }
                else
                {
                    sb.Append(ConvertirEntero(millones));
                    sb.Append(" millones");
                }

                if (resto > 0)
                {
                    sb.Append(' ');
                    sb.Append(ConvertirEntero(resto));
                }

                return sb.ToString();
            }

            long milesDeMillones = numero / 1_000_000_000;
            long restoMiles = numero % 1_000_000_000;
            var resultado = new StringBuilder();

            if (milesDeMillones == 1)
            {
                resultado.Append("mil millones");
            }
            else
            {
                resultado.Append(ConvertirEntero(milesDeMillones));
                resultado.Append(" mil millones");
            }

            if (restoMiles > 0)
            {
                resultado.Append(' ');
                resultado.Append(ConvertirEntero(restoMiles));
            }

            return resultado.ToString();
        }
    }
}
