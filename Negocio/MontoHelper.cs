using System.Globalization;
using System.Text.RegularExpressions;

namespace Negocio
{
    /// <summary>
    /// Parseo de importes y porcentajes ingresados por el usuario en formato es-AR
    /// (punto como separador de miles, coma como separador decimal).
    ///
    /// Motivo: el sitio corre con culture="es-AR". Con decimal.Parse / NumberStyles.Currency,
    /// un texto como "12.5" se interpreta como 125 y "1234.56" como 123456, y el valor
    /// multiplicado se persiste sin aviso. Este helper acepta únicamente formatos no ambiguos
    /// y rechaza todo lo demás para que la UI pueda informar el error.
    /// </summary>
    public static class MontoHelper
    {
        // Acepta: "1234", "1234,5", "1.234,56", "$ 1.234,56", "$1.234". Rechaza: "12.5", "1234.56", "1,234.56".
        private static readonly Regex FormatoEsAr = new Regex(
            @"^\$?\s*(\d{1,3}(\.\d{3})+|\d+)(,\d{1,4})?$",
            RegexOptions.Compiled);

        /// <summary>
        /// Intenta convertir un texto en formato es-AR a decimal.
        /// Devuelve false si el texto está vacío o usa un formato ambiguo (por ejemplo, punto decimal).
        /// </summary>
        public static bool TryParseEsAr(string texto, out decimal valor)
        {
            valor = 0m;
            if (string.IsNullOrWhiteSpace(texto))
                return false;

            var limpio = texto.Trim();
            if (!FormatoEsAr.IsMatch(limpio))
                return false;

            limpio = limpio.Replace("$", string.Empty)
                           .Replace(" ", string.Empty)
                           .Replace(".", string.Empty)
                           .Replace(',', '.');

            return decimal.TryParse(limpio, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out valor);
        }
    }
}
