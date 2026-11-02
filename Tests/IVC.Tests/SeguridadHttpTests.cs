using Microsoft.VisualStudio.TestTools.UnitTesting;
using Negocio.Infraestructura;

namespace IVC.Tests
{
    [TestClass]
    public class SeguridadHttpTests
    {
        [TestMethod]
        public void ReturnUrl_ConTabOSaltoDeLinea_EsInvalido()
        {
            Assert.IsTrue(SeguridadHttp.TieneCaracteresInvalidosParaUrl("/\t/evil.com"));
            Assert.IsTrue(SeguridadHttp.TieneCaracteresInvalidosParaUrl("/\n/evil.com"));
            Assert.IsTrue(SeguridadHttp.TieneCaracteresInvalidosParaUrl("/Obras\r\nSet-Cookie: x"));
            Assert.IsTrue(SeguridadHttp.TieneCaracteresInvalidosParaUrl("/Obras\0EF.aspx"));
            Assert.IsTrue(SeguridadHttp.TieneCaracteresInvalidosParaUrl("/Obras EF.aspx"));
        }

        [TestMethod]
        public void ReturnUrl_Normal_EsValido()
        {
            Assert.IsFalse(SeguridadHttp.TieneCaracteresInvalidosParaUrl("/ObrasEF.aspx?x=1&y=2"));
            Assert.IsFalse(SeguridadHttp.TieneCaracteresInvalidosParaUrl("https://ivc/AutorizantesEF.aspx"));
            Assert.IsFalse(SeguridadHttp.TieneCaracteresInvalidosParaUrl(null));
            Assert.IsFalse(SeguridadHttp.TieneCaracteresInvalidosParaUrl(""));
        }
    }
}
