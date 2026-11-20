using System;
using System.Collections.Generic;
using System.Linq;
using Dominio.DTO;
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
        public void ConExpedienteConEspacios_EncuentraSade()
        {
            var (estado, buzon, _) = new CalculoRedeterminacionNegocioEF().CalcularPropiedadesAdicionales(" EX-1 ", 100m, 50m, Sade);

            Assert.AreEqual("DEVENGADO", estado);
            Assert.AreEqual("DGTAL", buzon);
        }

        [TestMethod]
        public void NormalizarPasesSade_RecortaIgnoraMayusculasYConservaElMasReciente()
        {
            var normalizado = DatosExternosNegocioEF.NormalizarPasesSade(new[]
            {
                new KeyValuePair<string, UltimoPaseSade>("ex-1 ", new UltimoPaseSade("VIEJO", new DateTime(2026, 1, 1))),
                new KeyValuePair<string, UltimoPaseSade>("EX-1", new UltimoPaseSade("NUEVO", new DateTime(2026, 5, 2))),
                new KeyValuePair<string, UltimoPaseSade>("  ", new UltimoPaseSade("BASURA", null)),
            });

            Assert.AreEqual(1, normalizado.Count);
            Assert.AreEqual("NUEVO", normalizado["Ex-1"].Buzon);
        }

        [TestMethod]
        public void ConExpedienteSinSigafNiSade_EnTramite()
        {
            var (estado, buzon, fecha) = new CalculoRedeterminacionNegocioEF().CalcularPropiedadesAdicionales("EX-2", 100m, null, Sade);

            Assert.AreEqual("EN TRAMITE", estado);
            Assert.IsNull(buzon);
            Assert.IsNull(fecha);
        }

        // ---------- Devengado SIGAF: claves y reparto en certificados ----------

        private static CertificadoDTO Cert(int id, string expediente, decimal monto) =>
            new CertificadoDTO { Id = id, CodigoAutorizante = "A-1", ExpedientePago = expediente, MontoTotal = monto };

        [TestMethod]
        public void NormalizarDevengados_RecortaIgnoraMayusculasYSumaCoincidencias()
        {
            var normalizado = DatosExternosNegocioEF.NormalizarDevengados(new Dictionary<string, decimal>
            {
                ["EX-1"] = 100m,
                ["ex-1 "] = 50m,
                ["  "] = 999m,
                ["EX-2"] = 7m,
            });

            Assert.AreEqual(2, normalizado.Count);
            Assert.AreEqual(150m, normalizado["Ex-1"]);
            Assert.AreEqual(7m, normalizado["EX-2"]);
        }

        [TestMethod]
        public void Certificados_ClaveDevengadoConOtraGrafia_SeReparteProporcionalmente()
        {
            // Antes: la clave "ex-1 " (grafía de DEVENGADOS) no coincidía con "EX-1" y los certificados quedaban sin SIGAF.
            var certificados = new List<CertificadoDTO> { Cert(1, "EX-1", 25m), Cert(2, "ex-1", 75m), Cert(3, "EX-9", 10m) };
            var sigaf = new Dictionary<string, decimal> { ["ex-1 "] = 400m };

            new CalculoRedeterminacionNegocioEF().CalcularCertificadosPorExpedientes(
                new List<string> { "EX-1" }, certificados, persistirEnBD: false,
                datosSigafPrecargados: sigaf, datosSadePrecargados: new Dictionary<string, UltimoPaseSade>());

            Assert.AreEqual(100m, certificados[0].Sigaf);
            Assert.AreEqual(300m, certificados[1].Sigaf);
            Assert.AreEqual("DEVENGADO", certificados[0].Estado);
            Assert.AreEqual("DEVENGADO", certificados[1].Estado);
            Assert.IsNull(certificados[2].Sigaf, "el expediente no recalculado no se toca");
        }

        [TestMethod]
        public void Certificados_ExpedientesRepetidosEnLaLista_MismoResultado()
        {
            var certificados = new List<CertificadoDTO> { Cert(1, "EX-1", 25m), Cert(2, "EX-1 ", 75m) };

            new CalculoRedeterminacionNegocioEF().CalcularCertificadosPorExpedientes(
                new List<string> { "EX-1", "ex-1 ", " EX-1" }, certificados, persistirEnBD: false,
                datosSigafPrecargados: new Dictionary<string, decimal> { ["EX-1"] = 400m },
                datosSadePrecargados: new Dictionary<string, UltimoPaseSade> { ["EX-1"] = new UltimoPaseSade("DGTAL", new DateTime(2026, 5, 2)) });

            Assert.AreEqual(100m, certificados[0].Sigaf);
            Assert.AreEqual(300m, certificados[1].Sigaf);
            Assert.AreEqual("DGTAL", certificados[1].BuzonSade, "el pago con espacios también encuentra su pase SADE");
        }

        [TestMethod]
        public void Certificados_ExpedienteSinDevengado_SigafNullYEnTramite()
        {
            var certificados = new List<CertificadoDTO> { Cert(1, "EX-1", 25m) };

            new CalculoRedeterminacionNegocioEF().CalcularCertificadosPorExpedientes(
                new List<string> { "EX-1" }, certificados, persistirEnBD: false,
                datosSigafPrecargados: new Dictionary<string, decimal>(), datosSadePrecargados: new Dictionary<string, UltimoPaseSade>());

            Assert.IsNull(certificados[0].Sigaf);
            Assert.AreEqual("EN TRAMITE", certificados[0].Estado);
        }
    }
}
