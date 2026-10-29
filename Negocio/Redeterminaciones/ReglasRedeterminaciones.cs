using System;
using System.Collections.Generic;
using System.Linq;
using Dominio;
using Dominio.DTO;

namespace Negocio.Redeterminaciones
{
    /// <summary>
    /// Reglas de cálculo de redeterminaciones y reliquidaciones, sin acceso a datos ni a la web.
    ///
    /// Es la única implementación de las reglas R1 a R6 del lado de la aplicación y debe dar lo mismo que
    /// las vistas SQL de Database/vw_PowerBI_Redeterminaciones.sql (documentación funcional en
    /// Docs/calculo-redeterminaciones.html, Sección 6). Cualquier cambio de regla se hace acá, en las
    /// vistas y en los tests de Tests/IVC.Tests/ReglasRedeterminacionesTests.cs.
    ///
    /// Reglas vigentes (fijadas el 22/09/2026; R6 el 01/10/2026; R1, R2 y R7 el 28/10/2026 por dictamen del área):
    ///  R1. El cálculo se hace al primero del mes del salto: cuentan como ejecutado los certificados de meses
    ///      estrictamente anteriores, y reliquidan los del mes del salto y posteriores. La comparación es mes a mes
    ///      (MesAprobacion es el mes de ejecución de la obra).
    ///  R2. El anticipo financiero (tipo de pago 1) se trata igual que un certificado: cuenta como ejecutado
    ///      si es de un mes anterior al salto y reliquida si es del mes del salto o posterior.
    ///  R3. El primer certificado posterior al salto absorbe el porcentaje ejecutado pre-salto.
    ///  R4. Los legítimos abonos no participan del reparto del SIGAF (se resuelve fuera de esta clase).
    ///  R5. Cada autorizante se calcula con sus propios certificados.
    ///  R6. Rechazada (35) o Fuera de Plazo (36): monto cero, no acumula y no reliquida.
    ///  R7. Tope: la suma de las reliquidaciones de una redeterminación no supera su MontoRedet. La que lo
    ///      alcanza se recorta al resto disponible y las siguientes no se generan.
    /// </summary>
    public static class ReglasRedeterminaciones
    {
        /// <summary>Tipo de pago con el que se exponen las reliquidaciones virtuales.</summary>
        public const int TipoPagoReliquidacionId = 3;

        /// <summary>Etapas de ESTADOS_REDET que anulan el monto (R6): 35 Rechazada, 36 Fuera de Plazo.</summary>
        public static readonly IReadOnlyList<int> EtapasDesestimadas = Array.AsReadOnly(new[] { 35, 36 });

        public static DateTime PrimerDiaDelMes(DateTime fecha) => new DateTime(fecha.Year, fecha.Month, 1);

        /// <summary>R1: sólo los meses estrictamente anteriores al del salto cuentan como ejecutado.</summary>
        public static bool EsMesAnterior(DateTime fecha, DateTime salto) => PrimerDiaDelMes(fecha) < PrimerDiaDelMes(salto);

        /// <summary>R1: el mes del salto y los posteriores reliquidan.</summary>
        public static bool EsMesDelSaltoOPosterior(DateTime fecha, DateTime salto) => PrimerDiaDelMes(fecha) >= PrimerDiaDelMes(salto);

        /// <summary>R6.</summary>
        public static bool EsDesestimada(int? etapaId) => etapaId.HasValue && EtapasDesestimadas.Contains(etapaId.Value);

        /// <summary>Porcentaje que representa un monto sobre el monto autorizado; 0 si el autorizado no es positivo.</summary>
        public static decimal PorcentajeSobreAutorizado(decimal monto, decimal montoAutorizado) =>
            montoAutorizado > 0 ? monto / montoAutorizado * 100 : 0;

        /// <summary>
        /// Calcula MontoRedet y PorcentajeEjecutadoPreSalto de cada redeterminación (Pasos A, B y C) y los
        /// escribe en las redeterminaciones recibidas. Las que no tienen salto o porcentaje, o cuyo autorizante
        /// no está en la lista, no se tocan.
        /// </summary>
        public static void CalcularMontos(
            IEnumerable<RedeterminacionEF> redeterminaciones,
            IEnumerable<AutorizanteEF> autorizantes,
            IEnumerable<CertificadoEF> certificados)
        {
            var calculadas = new List<RedeterminacionEF>();

            // R5: cada autorizante con sus propios certificados. No se filtra ni reasigna la lista dentro del bucle.
            var certificadosPorAutorizante = (certificados ?? Enumerable.Empty<CertificadoEF>())
                .Where(c => !string.IsNullOrEmpty(c.CodigoAutorizante))
                .GroupBy(c => c.CodigoAutorizante)
                .ToDictionary(g => g.Key, g => g.ToList());

            var autorizantesPorCodigo = (autorizantes ?? Enumerable.Empty<AutorizanteEF>())
                .Where(a => !string.IsNullOrEmpty(a.CodigoAutorizante))
                .GroupBy(a => a.CodigoAutorizante)
                .ToDictionary(g => g.Key, g => g.First());

            foreach (var redet in OrdenarParaAcumular(redeterminaciones))
            {
                if (string.IsNullOrEmpty(redet.CodigoAutorizante)
                    || !autorizantesPorCodigo.TryGetValue(redet.CodigoAutorizante, out var autorizante))
                    continue;
                if (!redet.Salto.HasValue || !redet.Porcentaje.HasValue)
                    continue;

                certificadosPorAutorizante.TryGetValue(autorizante.CodigoAutorizante, out var certificadosDelAutorizante);

                // Paso A. R1: meses estrictamente anteriores al del salto. R2: todos los tipos de pago, incluido el anticipo.
                decimal ejecutadoPreSalto = (certificadosDelAutorizante ?? new List<CertificadoEF>())
                    .Where(c => c.MesAprobacion.HasValue && EsMesAnterior(c.MesAprobacion.Value, redet.Salto.Value))
                    .Sum(c => c.MontoTotal);
                decimal porcentajeUtilizado = PorcentajeSobreAutorizado(ejecutadoPreSalto, autorizante.MontoAutorizado);

                // Paso B. El faltante nunca es negativo.
                decimal faltante = Math.Max(0, 100 - porcentajeUtilizado);
                redet.PorcentajeEjecutadoPreSalto = porcentajeUtilizado;

                // Paso C. Base = MontoAutorizado + redeterminaciones ya calculadas del mismo autorizante.
                decimal acumuladoAnterior = calculadas
                    .Where(r => r.CodigoAutorizante == redet.CodigoAutorizante && r.MontoRedet.HasValue)
                    .Sum(r => r.MontoRedet.Value);

                // R6: desestimada => 0, y por lo tanto no suma en la base de las siguientes.
                redet.MontoRedet = EsDesestimada(redet.EstadoRedetEFId)
                    ? 0
                    : (autorizante.MontoAutorizado + acumuladoAnterior) * redet.Porcentaje.Value / 100 * (faltante / 100);

                calculadas.Add(redet);
            }
        }

        /// <summary>
        /// Mismo orden que ROW_NUMBER de la vista SQL: Nro ascendente, sin Nro al final, desempate por Id.
        /// </summary>
        public static IEnumerable<RedeterminacionEF> OrdenarParaAcumular(IEnumerable<RedeterminacionEF> redeterminaciones) =>
            (redeterminaciones ?? Enumerable.Empty<RedeterminacionEF>())
                .OrderBy(r => r.Nro.HasValue ? 0 : 1)
                .ThenBy(r => r.Nro)
                .ThenBy(r => r.Id);

        /// <summary>
        /// Reliquidaciones de una redeterminación ya calculada sobre los certificados de su autorizante (Pasos 2 y 3).
        /// Devuelve vacío si no hay salto o monto, o si está desestimada (R6).
        /// </summary>
        /// <param name="redet">Redeterminación con Salto, MontoRedet y PorcentajeEjecutadoPreSalto calculados.</param>
        /// <param name="certificadosDelAutorizante">Certificados reales del autorizante, con Porcentaje sobre el autorizado.</param>
        public static List<ReliquidacionCalculada> GenerarReliquidaciones(
            RedeterminacionEF redet,
            IEnumerable<CertificadoDTO> certificadosDelAutorizante)
        {
            var resultado = new List<ReliquidacionCalculada>();
            if (redet == null || !redet.Salto.HasValue || !redet.MontoRedet.HasValue)
                return resultado;
            if (EsDesestimada(redet.EstadoRedetEFId))
                return resultado;

            // Paso 2. R1: mes del salto y posteriores. R2: todos los tipos de pago, incluido el anticipo.
            // Orden por mes y luego por Id para que "el primer certificado post-salto" sea estable (R3).
            var afectados = (certificadosDelAutorizante ?? Enumerable.Empty<CertificadoDTO>())
                .Where(c => c.MesAprobacion.HasValue
                         && EsMesDelSaltoOPosterior(c.MesAprobacion.Value, redet.Salto.Value))
                .OrderBy(c => c.MesAprobacion)
                .ThenBy(c => c.Id);

            bool esPrimero = true;
            decimal tope = redet.MontoRedet.Value;
            decimal acumulado = 0;
            foreach (var cert in afectados)
            {
                // R7: una vez alcanzado el MontoRedet no se generan más reliquidaciones.
                if (acumulado >= tope)
                    break;

                // Paso 3a / 3b (R3): el primero absorbe el porcentaje ejecutado pre-salto.
                decimal porcentajeAplicado = (cert.Porcentaje ?? 0) + (esPrimero ? (redet.PorcentajeEjecutadoPreSalto ?? 0) : 0);
                esPrimero = false;

                // R7: la que supera el tope se recorta al resto disponible.
                decimal monto = Math.Min(tope * (porcentajeAplicado / 100), tope - acumulado);
                acumulado += monto;

                resultado.Add(new ReliquidacionCalculada(cert, porcentajeAplicado, monto));
            }

            return resultado;
        }

        /// <summary>
        /// SIGAF prorrateado: el devengado total del expediente se reparte entre los pagos del mismo expediente
        /// en proporción a su monto. Un único pago se lleva todo. Devuelve null si no hay devengado positivo
        /// o si la suma de montos no permite prorratear.
        /// </summary>
        public static decimal? ProrratearSigaf(decimal totalDevengado, decimal montoPago, IReadOnlyCollection<decimal> montosMismoExpediente)
        {
            if (totalDevengado <= 0)
                return null;
            if (montosMismoExpediente == null || montosMismoExpediente.Count <= 1)
                return totalDevengado;

            decimal suma = montosMismoExpediente.Sum();
            return suma > 0 ? totalDevengado * montoPago / suma : (decimal?)null;
        }

        /// <summary>Estado de un pago: sin expediente NO INICIADO; con SIGAF positivo DEVENGADO; si no, EN TRAMITE.</summary>
        public static string DeterminarEstado(string expediente, decimal? sigaf)
        {
            if (string.IsNullOrEmpty(expediente))
                return "NO INICIADO";
            return sigaf > 0 ? "DEVENGADO" : "EN TRAMITE";
        }
    }

    /// <summary>Resultado de GenerarReliquidaciones: el certificado afectado y el monto que se reliquida sobre él.</summary>
    public sealed class ReliquidacionCalculada
    {
        public ReliquidacionCalculada(CertificadoDTO certificado, decimal porcentajeAplicado, decimal monto)
        {
            Certificado = certificado;
            PorcentajeAplicado = porcentajeAplicado;
            Monto = monto;
        }

        public CertificadoDTO Certificado { get; }
        public decimal PorcentajeAplicado { get; }
        public decimal Monto { get; }
    }
}
