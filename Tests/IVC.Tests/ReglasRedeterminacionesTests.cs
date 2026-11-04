using System;
using System.Collections.Generic;
using System.Linq;
using Dominio;
using Dominio.DTO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Negocio.Redeterminaciones;

namespace IVC.Tests
{
    /// <summary>
    /// Reglas R1 a R6 del cálculo de redeterminaciones (ver Database/vw_PowerBI_Redeterminaciones.sql).
    /// Escenario base: autorizante de 1.000 con un certificado de 250 en el mes del salto (25 % ejecutado,
    /// faltante 75 %). Una redeterminación del 10 % vale 1.000 * 0,10 * 0,75 = 75.
    /// </summary>
    [TestClass]
    public class ReglasRedeterminacionesTests
    {
        private static readonly DateTime Salto = new DateTime(2026, 3, 15);

        private static AutorizanteEF Autorizante(string codigo = "A-1", decimal monto = 1000m) =>
            new AutorizanteEF { Id = 1, CodigoAutorizante = codigo, MontoAutorizado = monto };

        private static CertificadoEF Certificado(int id, string codigo, decimal monto, DateTime mes, int tipoPago = 2) =>
            new CertificadoEF { Id = id, CodigoAutorizante = codigo, MontoTotal = monto, MesAprobacion = mes, TipoPagoId = tipoPago };

        private static RedeterminacionEF Redet(int id, string codigo, int? nro, decimal? porcentaje, DateTime? salto, int? etapa = 12) =>
            new RedeterminacionEF { Id = id, CodigoAutorizante = codigo, Nro = nro, Porcentaje = porcentaje, Salto = salto, EstadoRedetEFId = etapa };

        private static CertificadoDTO Dto(int id, decimal monto, DateTime mes, decimal montoAutorizado = 1000m, int tipoPago = 2) =>
            new CertificadoDTO
            {
                Id = id,
                CodigoAutorizante = "A-1",
                MontoTotal = monto,
                MesAprobacion = mes,
                TipoPagoId = tipoPago,
                Porcentaje = ReglasRedeterminaciones.PorcentajeSobreAutorizado(monto, montoAutorizado),
            };

        // ---------- Paso A/B/C: MontoRedet ----------

        [TestMethod]
        public void R1_CertificadoDelMesAnterior_CuentaComoEjecutado()
        {
            var redet = Redet(1, "A-1", 1, 10m, Salto);
            var certs = new[] { Certificado(1, "A-1", 250m, new DateTime(2026, 2, 28)) };

            ReglasRedeterminaciones.CalcularMontos(new[] { redet }, new[] { Autorizante() }, certs);

            Assert.AreEqual(25m, redet.PorcentajeEjecutadoPreSalto);
            Assert.AreEqual(75m, redet.MontoRedet);
        }

        [TestMethod]
        public void R1_CertificadoDelMesDelSalto_NoCuentaComoEjecutado()
        {
            // Dictamen del área (28/10/2026): el cálculo se hace al 1° del mes del salto; ese mes reliquida.
            var redet = Redet(1, "A-1", 1, 10m, Salto);
            var certs = new[] { Certificado(1, "A-1", 250m, new DateTime(2026, 3, 1)) }; // mismo mes, día anterior al salto

            ReglasRedeterminaciones.CalcularMontos(new[] { redet }, new[] { Autorizante() }, certs);

            Assert.AreEqual(0m, redet.PorcentajeEjecutadoPreSalto);
            Assert.AreEqual(100m, redet.MontoRedet);
        }

        [TestMethod]
        public void R1_CertificadoDelMesSiguiente_NoCuentaComoEjecutado()
        {
            var redet = Redet(1, "A-1", 1, 10m, Salto);
            var certs = new[] { Certificado(1, "A-1", 250m, new DateTime(2026, 4, 1)) };

            ReglasRedeterminaciones.CalcularMontos(new[] { redet }, new[] { Autorizante() }, certs);

            Assert.AreEqual(0m, redet.PorcentajeEjecutadoPreSalto);
            Assert.AreEqual(100m, redet.MontoRedet); // faltante 100 %
        }

        [TestMethod]
        public void R2_AnticipoPreSalto_CuentaComoEjecutado()
        {
            var redet = Redet(1, "A-1", 1, 10m, Salto);
            var certs = new[] { Certificado(1, "A-1", 250m, new DateTime(2026, 1, 1), TipoPagoAnticipoId) };

            ReglasRedeterminaciones.CalcularMontos(new[] { redet }, new[] { Autorizante() }, certs);

            Assert.AreEqual(75m, redet.MontoRedet);
        }

        [TestMethod]
        public void PasoC_SegundaRedeterminacion_AcumulaLaPrimera()
        {
            var r1 = Redet(1, "A-1", 1, 10m, Salto);
            var r2 = Redet(2, "A-1", 2, 10m, new DateTime(2026, 6, 1));
            var certs = new[] { Certificado(1, "A-1", 250m, new DateTime(2026, 2, 1)) }; // anterior al salto de marzo

            ReglasRedeterminaciones.CalcularMontos(new[] { r2, r1 }, new[] { Autorizante() }, certs);

            Assert.AreEqual(75m, r1.MontoRedet);
            Assert.AreEqual((1000m + 75m) * 0.10m * 0.75m, r2.MontoRedet); // 80,625
        }

        [TestMethod]
        public void R6_Desestimada_MontoCeroYNoAcumula()
        {
            var r1 = Redet(1, "A-1", 1, 10m, Salto, etapa: 35); // Rechazada
            var r2 = Redet(2, "A-1", 2, 10m, new DateTime(2026, 6, 1));
            var certs = new[] { Certificado(1, "A-1", 250m, new DateTime(2026, 2, 1)) }; // anterior al salto de marzo

            ReglasRedeterminaciones.CalcularMontos(new[] { r1, r2 }, new[] { Autorizante() }, certs);

            Assert.AreEqual(0m, r1.MontoRedet);
            Assert.AreEqual(75m, r2.MontoRedet); // base sin la desestimada
        }

        [TestMethod]
        public void R6_FueraDePlazo_TambienDesestimada()
        {
            Assert.IsTrue(ReglasRedeterminaciones.EsDesestimada(36));
            Assert.IsFalse(ReglasRedeterminaciones.EsDesestimada(37));
            Assert.IsFalse(ReglasRedeterminaciones.EsDesestimada(null));
        }

        [TestMethod]
        public void PasoB_FaltanteNuncaNegativo()
        {
            var redet = Redet(1, "A-1", 1, 10m, Salto);
            var certs = new[] { Certificado(1, "A-1", 1500m, new DateTime(2026, 1, 1)) }; // 150 % ejecutado

            ReglasRedeterminaciones.CalcularMontos(new[] { redet }, new[] { Autorizante() }, certs);

            Assert.AreEqual(150m, redet.PorcentajeEjecutadoPreSalto);
            Assert.AreEqual(0m, redet.MontoRedet);
        }

        [TestMethod]
        public void MontoAutorizadoCero_PorcentajeCeroYMontoCero()
        {
            var redet = Redet(1, "A-1", 1, 10m, Salto);
            var certs = new[] { Certificado(1, "A-1", 100m, new DateTime(2026, 1, 1)) };

            ReglasRedeterminaciones.CalcularMontos(new[] { redet }, new[] { Autorizante(monto: 0m) }, certs);

            Assert.AreEqual(0m, redet.PorcentajeEjecutadoPreSalto);
            Assert.AreEqual(0m, redet.MontoRedet);
        }

        [TestMethod]
        public void SinSaltoOSinPorcentaje_NoSeCalcula()
        {
            var sinSalto = Redet(1, "A-1", 1, 10m, null);
            var sinPorcentaje = Redet(2, "A-1", 2, null, Salto);

            ReglasRedeterminaciones.CalcularMontos(new[] { sinSalto, sinPorcentaje }, new[] { Autorizante() }, new CertificadoEF[0]);

            Assert.IsNull(sinSalto.PorcentajeEjecutadoPreSalto);
            Assert.IsNull(sinSalto.MontoRedet);
            Assert.IsNull(sinPorcentaje.MontoRedet);
        }

        [TestMethod]
        public void R5_CadaAutorizanteConSusPropiosCertificados()
        {
            var a1 = Autorizante("A-1");
            var a2 = new AutorizanteEF { Id = 2, CodigoAutorizante = "A-2", MontoAutorizado = 1000m };
            var r1 = Redet(1, "A-1", 1, 10m, Salto);
            var r2 = Redet(2, "A-2", 1, 10m, Salto);
            var certs = new[] { Certificado(1, "A-1", 250m, new DateTime(2026, 1, 1)) }; // sólo A-1 tiene ejecutado

            ReglasRedeterminaciones.CalcularMontos(new[] { r1, r2 }, new[] { a1, a2 }, certs);

            Assert.AreEqual(75m, r1.MontoRedet);
            Assert.AreEqual(100m, r2.MontoRedet); // faltante 100 %, no hereda los certificados de A-1
        }

        [TestMethod]
        public void Orden_NroAscendente_SinNroAlFinal_DesempatePorId()
        {
            var lista = new[]
            {
                Redet(5, "A-1", null, 1m, Salto),
                Redet(4, "A-1", 2, 1m, Salto),
                Redet(3, "A-1", 1, 1m, Salto),
                Redet(2, "A-1", 1, 1m, Salto),
            };

            var ids = ReglasRedeterminaciones.OrdenarParaAcumular(lista).Select(r => r.Id).ToArray();

            CollectionAssert.AreEqual(new[] { 2, 3, 4, 5 }, ids);
        }

        // ---------- Paso 2/3: reliquidaciones ----------

        /// <summary>Tipo de pago "anticipo financiero": desde el 28/10/2026 se trata igual que un certificado (R2).</summary>
        private const int TipoPagoAnticipoId = 1;

        private static RedeterminacionEF RedetCalculada(decimal montoRedet = 75m, decimal preSalto = 25m, int? etapa = 12) =>
            new RedeterminacionEF { Id = 1, CodigoAutorizante = "A-1", Salto = Salto, MontoRedet = montoRedet, PorcentajeEjecutadoPreSalto = preSalto, EstadoRedetEFId = etapa };

        [TestMethod]
        public void R1_ReliquidanElMesDelSaltoYLosPosteriores()
        {
            var certs = new[]
            {
                Dto(3, 100m, new DateTime(2026, 2, 28)), // anterior: no reliquida
                Dto(1, 100m, new DateTime(2026, 3, 1)),  // mes del salto: reliquida y es el primero (R3)
                Dto(2, 100m, new DateTime(2026, 4, 1)),  // posterior: reliquida
            };

            var reliq = ReglasRedeterminaciones.GenerarReliquidaciones(RedetCalculada(), certs);

            Assert.AreEqual(2, reliq.Count);
            Assert.AreEqual(1, reliq[0].Certificado.Id);
            Assert.AreEqual(35m, reliq[0].PorcentajeAplicado);
            Assert.AreEqual(2, reliq[1].Certificado.Id);
        }

        [TestMethod]
        public void R3_PrimerCertificadoPostSalto_AbsorbeElPreSalto()
        {
            var certs = new[]
            {
                Dto(1, 100m, new DateTime(2026, 4, 1)), // 10 % propio + 25 % pre-salto = 35 %
                Dto(2, 100m, new DateTime(2026, 5, 1)), // 10 %
            };

            var reliq = ReglasRedeterminaciones.GenerarReliquidaciones(RedetCalculada(), certs);

            Assert.AreEqual(35m, reliq[0].PorcentajeAplicado);
            Assert.AreEqual(75m * 0.35m, reliq[0].Monto); // 26,25
            Assert.AreEqual(10m, reliq[1].PorcentajeAplicado);
            Assert.AreEqual(7.5m, reliq[1].Monto);
        }

        [TestMethod]
        public void R3_OrdenPorMesYLuegoPorId_DeterminaElPrimero()
        {
            var certs = new[]
            {
                Dto(9, 100m, new DateTime(2026, 5, 1)),
                Dto(2, 100m, new DateTime(2026, 4, 1)),
                Dto(1, 100m, new DateTime(2026, 4, 1)),
            };

            var reliq = ReglasRedeterminaciones.GenerarReliquidaciones(RedetCalculada(), certs);

            CollectionAssert.AreEqual(new[] { 1, 2, 9 }, reliq.Select(r => r.Certificado.Id).ToArray());
            Assert.AreEqual(35m, reliq[0].PorcentajeAplicado);
        }

        [TestMethod]
        public void R2_AnticipoPostSalto_ReliquidaComoUnCertificado()
        {
            // Dictamen del área (28/10/2026): el anticipo se redetermina y reliquida igual que un certificado.
            var certs = new[]
            {
                Dto(1, 100m, new DateTime(2026, 4, 1), tipoPago: TipoPagoAnticipoId), // 10 % propio + 25 % pre-salto
                Dto(2, 100m, new DateTime(2026, 5, 1)),
            };

            var reliq = ReglasRedeterminaciones.GenerarReliquidaciones(RedetCalculada(), certs);

            Assert.AreEqual(2, reliq.Count);
            Assert.AreEqual(1, reliq[0].Certificado.Id);
            Assert.AreEqual(35m, reliq[0].PorcentajeAplicado, "el anticipo, si es el primero post-salto, absorbe el pre-salto (R3)");
            Assert.AreEqual(75m * 0.35m, reliq[0].Monto);
            Assert.AreEqual(10m, reliq[1].PorcentajeAplicado);
        }

        [TestMethod]
        public void R7_LaReliquidacionQueSuperaElTope_SeRecortaYLasSiguientesNoSeGeneran()
        {
            // MontoRedet 75, pre-salto 25 %. Primero 50 % + 25 % = 75 % => 56,25; segundo 50 % => 37,50 pero sólo quedan 18,75.
            var certs = new[]
            {
                Dto(1, 500m, new DateTime(2026, 4, 1)),
                Dto(2, 500m, new DateTime(2026, 5, 1)),
                Dto(3, 100m, new DateTime(2026, 6, 1)),
            };

            var reliq = ReglasRedeterminaciones.GenerarReliquidaciones(RedetCalculada(), certs);

            Assert.AreEqual(2, reliq.Count, "la tercera no se genera: el tope ya está cubierto");
            Assert.AreEqual(56.25m, reliq[0].Monto);
            Assert.AreEqual(50m, reliq[1].PorcentajeAplicado, "el porcentaje aplicado se informa sin recortar");
            Assert.AreEqual(18.75m, reliq[1].Monto);
            Assert.AreEqual(75m, reliq.Sum(r => r.Monto));
        }

        [TestMethod]
        public void R7_SumaExactaAlTope_NoRecortaYNoGeneraMas()
        {
            var certs = new[]
            {
                Dto(1, 500m, new DateTime(2026, 4, 1)), // 75 % => 56,25
                Dto(2, 250m, new DateTime(2026, 5, 1)), // 25 % => 18,75; suma 75 = MontoRedet
                Dto(3, 100m, new DateTime(2026, 6, 1)),
            };

            var reliq = ReglasRedeterminaciones.GenerarReliquidaciones(RedetCalculada(), certs);

            Assert.AreEqual(2, reliq.Count);
            Assert.AreEqual(18.75m, reliq[1].Monto);
        }

        [TestMethod]
        public void R7_MontoRedetCero_NoGeneraReliquidaciones()
        {
            // Autorizante 100 % ejecutado antes del salto: no hay nada que reliquidar.
            var certs = new[] { Dto(1, 100m, new DateTime(2026, 4, 1)) };

            var reliq = ReglasRedeterminaciones.GenerarReliquidaciones(RedetCalculada(montoRedet: 0m, preSalto: 100m), certs);

            Assert.AreEqual(0, reliq.Count);
        }

        [TestMethod]
        public void R6_Desestimada_NoReliquida()
        {
            var certs = new[] { Dto(1, 100m, new DateTime(2026, 4, 1)) };

            var reliq = ReglasRedeterminaciones.GenerarReliquidaciones(RedetCalculada(montoRedet: 0m, etapa: 36), certs);

            Assert.AreEqual(0, reliq.Count);
        }

        [TestMethod]
        public void SinMontoRedet_NoReliquida()
        {
            var redet = RedetCalculada();
            redet.MontoRedet = null;

            var reliq = ReglasRedeterminaciones.GenerarReliquidaciones(redet, new[] { Dto(1, 100m, new DateTime(2026, 4, 1)) });

            Assert.AreEqual(0, reliq.Count);
        }

        // ---------- SIGAF y estado ----------

        [TestMethod]
        public void Sigaf_UnSoloPago_SeLlevaTodoElDevengado()
        {
            Assert.AreEqual(500m, ReglasRedeterminaciones.ProrratearSigaf(500m, 80m, new[] { 80m }));
        }

        [TestMethod]
        public void Sigaf_VariosPagos_ProporcionalAlMonto()
        {
            Assert.AreEqual(100m, ReglasRedeterminaciones.ProrratearSigaf(400m, 25m, new[] { 25m, 75m }));
        }

        [TestMethod]
        public void Sigaf_SinDevengadoOSumaCero_Null()
        {
            Assert.IsNull(ReglasRedeterminaciones.ProrratearSigaf(0m, 25m, new[] { 25m, 75m }));
            Assert.IsNull(ReglasRedeterminaciones.ProrratearSigaf(400m, 0m, new[] { 0m, 0m }));
        }

        [DataTestMethod]
        [DataRow(null, null, "NO INICIADO")]
        [DataRow("", 10.0, "NO INICIADO")]
        [DataRow("EX-1", 10.0, "DEVENGADO")]
        [DataRow("EX-1", 0.0, "EN TRAMITE")]
        [DataRow("EX-1", null, "EN TRAMITE")]
        public void Estado_SegunExpedienteYSigaf(string expediente, double? sigaf, string esperado)
        {
            Assert.AreEqual(esperado, ReglasRedeterminaciones.DeterminarEstado(expediente, (decimal?)sigaf));
        }
    }
}
