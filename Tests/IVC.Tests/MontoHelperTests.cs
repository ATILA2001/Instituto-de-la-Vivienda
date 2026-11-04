using Microsoft.VisualStudio.TestTools.UnitTesting;
using Negocio;

namespace IVC.Tests
{
    [TestClass]
    public class MontoHelperTests
    {
        [DataTestMethod]
        [DataRow("1234", "1234")]
        [DataRow("12,5", "12.5")]
        [DataRow("1.234,56", "1234.56")]
        [DataRow("$ 1.234,56", "1234.56")]
        [DataRow("$1.234", "1234")]
        [DataRow("0,1234", "0.1234")]
        [DataRow("  12,5  ", "12.5")]
        [DataRow("-4,20", "-4.20")]          // porcentajes negativos existen en producción
        [DataRow("$ -1.234,56", "-1234.56")]  // ToString("C") de un negativo en es-AR
        [DataRow("-$ 1.234,56", "-1234.56")]  // variante según configuración regional
        [DataRow("12,500000", "12.5")]        // escala de la columna mayor a 4 decimales
        [DataRow("$ 1.234,56", "1234.56")] // NBSP entre símbolo y número
        public void TryParseEsAr_AceptaFormatosEsAr(string texto, string esperadoInvariante)
        {
            var esperado = decimal.Parse(esperadoInvariante, System.Globalization.CultureInfo.InvariantCulture);

            var ok = MontoHelper.TryParseEsAr(texto, out var valor);

            Assert.IsTrue(ok, $"Debería aceptar '{texto}'");
            Assert.AreEqual(esperado, valor);
        }

        [DataTestMethod]
        [DataRow("12.5")]        // punto decimal: en es-AR sería 125
        [DataRow("1234.56")]     // punto decimal: en es-AR sería 123456
        [DataRow("1,234.56")]    // formato en-US
        [DataRow("1.23")]        // punto con dos decimales
        [DataRow("--12")]
        [DataRow("12-")]
        [DataRow("abc")]
        [DataRow("")]
        [DataRow("   ")]
        [DataRow(null)]
        public void TryParseEsAr_RechazaFormatosAmbiguosOInvalidos(string texto)
        {
            var ok = MontoHelper.TryParseEsAr(texto, out var valor);

            Assert.IsFalse(ok, $"Debería rechazar '{texto}'");
            Assert.AreEqual(0m, valor);
        }
    }
}
