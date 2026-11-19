using System.Collections.Generic;
using System.Linq;
using Dominio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Negocio;

namespace IVC.Tests
{
    /// <summary>
    /// Reparto del devengado SIGAF entre los legítimos que comparten expediente (LegitimoNegocioEF.AsignarSigafProrrateado).
    /// Misma regla que los certificados: proporcional al monto, un único pago se lleva todo.
    /// </summary>
    [TestClass]
    public class LegitimoSigafTests
    {
        private static LegitimoEF Legitimo(int id, string expediente, decimal? monto, decimal? sigafPrevio = null) =>
            new LegitimoEF { Id = id, Expediente = expediente, Certificado = monto, Sigaf = sigafPrevio };

        [TestMethod]
        public void ExpedienteRepetido_RepartoProporcionalAlMonto()
        {
            // Antes: cada legítimo del expediente mostraba el devengado completo (400 y 400).
            var lista = new List<LegitimoEF> { Legitimo(1, "EX-1", 25m), Legitimo(2, "EX-1", 75m) };

            LegitimoNegocioEF.AsignarSigafProrrateado(lista, new Dictionary<string, decimal> { ["EX-1"] = 400m });

            Assert.AreEqual(100m, lista[0].Sigaf);
            Assert.AreEqual(300m, lista[1].Sigaf);
            Assert.AreEqual(400m, lista.Sum(l => l.Sigaf));
        }

        [TestMethod]
        public void ExpedienteUnico_SeLlevaTodoElDevengado()
        {
            var lista = new List<LegitimoEF> { Legitimo(1, "EX-1", 80m) };

            LegitimoNegocioEF.AsignarSigafProrrateado(lista, new Dictionary<string, decimal> { ["EX-1"] = 500m });

            Assert.AreEqual(500m, lista[0].Sigaf);
        }

        [TestMethod]
        public void ExpedientesDistintos_SeReparteCadaUnoPorSeparado()
        {
            var lista = new List<LegitimoEF>
            {
                Legitimo(1, "EX-1", 50m), Legitimo(2, "EX-1", 50m),
                Legitimo(3, "EX-2", 10m),
            };

            LegitimoNegocioEF.AsignarSigafProrrateado(lista, new Dictionary<string, decimal> { ["EX-1"] = 200m, ["EX-2"] = 30m });

            Assert.AreEqual(100m, lista[0].Sigaf);
            Assert.AreEqual(100m, lista[1].Sigaf);
            Assert.AreEqual(30m, lista[2].Sigaf);
        }

        [TestMethod]
        public void SinDatoSigafOSinExpediente_SigafNull()
        {
            var lista = new List<LegitimoEF>
            {
                Legitimo(1, "EX-SIN-DEVENGADO", 50m, sigafPrevio: 999m), // valor viejo del snapshot: debe limpiarse
                Legitimo(2, null, 50m),
                Legitimo(3, "", 50m),
            };

            LegitimoNegocioEF.AsignarSigafProrrateado(lista, new Dictionary<string, decimal> { ["EX-1"] = 400m });

            Assert.IsTrue(lista.All(l => l.Sigaf == null));
        }

        [TestMethod]
        public void DatosNull_NoFallaYDejaSigafNull()
        {
            var lista = new List<LegitimoEF> { Legitimo(1, "EX-1", 50m, sigafPrevio: 10m) };

            LegitimoNegocioEF.AsignarSigafProrrateado(lista, null);

            Assert.IsNull(lista[0].Sigaf);
        }

        [TestMethod]
        public void MontosNulosOCero_ConVariosLegitimos_SigafNull()
        {
            var lista = new List<LegitimoEF> { Legitimo(1, "EX-1", null), Legitimo(2, "EX-1", 0m) };

            LegitimoNegocioEF.AsignarSigafProrrateado(lista, new Dictionary<string, decimal> { ["EX-1"] = 400m });

            Assert.IsTrue(lista.All(l => l.Sigaf == null));
        }

        [TestMethod]
        public void ExpedienteConEspaciosYDistintaCapitalizacion_SeAgrupaIgual()
        {
            // La clave del devengado viene con la grafía de la tabla DEVENGADOS (agrupación SQL),
            // que puede no coincidir en mayúsculas ni en espacios con el expediente cargado.
            var lista = new List<LegitimoEF> { Legitimo(1, "ex-1 ", 25m), Legitimo(2, "EX-1", 75m) };

            LegitimoNegocioEF.AsignarSigafProrrateado(lista, new Dictionary<string, decimal> { ["Ex-1  "] = 400m });

            Assert.AreEqual(100m, lista[0].Sigaf);
            Assert.AreEqual(300m, lista[1].Sigaf);
        }

        [TestMethod]
        public void LegitimoNullEnLaLista_SeIgnora()
        {
            var lista = new List<LegitimoEF> { Legitimo(1, "EX-1", 50m), null };

            LegitimoNegocioEF.AsignarSigafProrrateado(lista, new Dictionary<string, decimal> { ["EX-1"] = 100m });

            Assert.AreEqual(100m, lista[0].Sigaf);
        }
    }
}
