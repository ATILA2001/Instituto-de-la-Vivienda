using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Negocio;
using Negocio.Redeterminaciones;

namespace IVC.Tests
{
    /// <summary>Estado y datos SADE de un pago (certificado o legítimo) a partir de los datos ya cargados.</summary>
    [TestClass]
    public class PropiedadesPagoTests
    {
        private static readonly Dictionary<string, UltimoPaseSade> Sade = new Dictionary<string, UltimoPaseSade>
        {
            ["EX-1"] = new UltimoPaseSade("DGTAL", new DateTime(2026, 5, 2)),
        };

        [TestMethod]
        public void SinExpediente_NoIniciadoYSinSade_AunqueHayaDatosDeOtros()
        {
            // Antes: Dictionary.ContainsKey(null) lanzaba y el catch dejaba Estado = "ERROR".
            var (estado, buzon, fecha) = new CalculoRedeterminacionNegocioEF().CalcularPropiedadesAdicionales(null, 100m, null, Sade);

            Assert.AreEqual("NO INICIADO", estado);
            Assert.IsNull(buzon);
            Assert.IsNull(fecha);
        }

        [TestMethod]
        public void ConExpedienteYSigaf_DevengadoConDatosSade()
        {
            var (estado, buzon, fecha) = new CalculoRedeterminacionNegocioEF().CalcularPropiedadesAdicionales("EX-1", 100m, 50m, Sade);

            Assert.AreEqual("DEVENGADO", estado);
            Assert.AreEqual("DGTAL", buzon);
            Assert.AreEqual(new DateTime(2026, 5, 2), fecha);
        }

        [TestMethod]
        public void ConExpedienteSinSigafNiSade_EnTramite()
        {
            var (estado, buzon, fecha) = new CalculoRedeterminacionNegocioEF().CalcularPropiedadesAdicionales("EX-2", 100m, null, Sade);

            Assert.AreEqual("EN TRAMITE", estado);
            Assert.IsNull(buzon);
            Assert.IsNull(fecha);
        }
    }
}
