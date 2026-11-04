using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using Dominio;
using Negocio.Infraestructura;

namespace Negocio.Redeterminaciones
{
    /// <summary>
    /// Acceso a los datos externos que alimentan las grillas de redeterminaciones: último pase SADE
    /// (PASES_SADE), devengado SIGAF (DEVENGADOS) y número especial ACDIR (GEDO). Único lugar que
    /// consulta esas tablas y que maneja su caché en sesión.
    ///
    /// Caché: se guarda en sesión el resultado de cada expediente consultado, también cuando no tiene
    /// datos, para no volver a la base en el mismo request ni en los siguientes. Se limpia al modificar
    /// expedientes (LimpiarCacheSade). El devengado no se cachea porque cambia con cada carga de SIGAF.
    /// </summary>
    public class DatosExternosNegocioEF
    {
        private const string ClaveCacheSade = "CacheSade";
        private const string ClaveCacheAcdir = "CacheAcdir";

        /// <summary>Tamaño de lote para las consultas IN (...) de ACDIR, por el límite de parámetros de SQL Server.</summary>
        private const int TamanioLoteAcdir = 800;

        // ---------- SADE ----------

        /// <summary>Último pase SADE de un expediente; SinDatos si no hay.</summary>
        public UltimoPaseSade ObtenerUltimoPaseSade(string expediente)
        {
            if (string.IsNullOrWhiteSpace(expediente))
                return UltimoPaseSade.SinDatos;

            using (var context = new IVCdbContext())
            {
                var pase = context.PasesSade
                    .AsNoTracking()
                    .Where(p => p.Expediente == expediente)
                    .OrderByDescending(p => p.FechaUltimoPase)
                    .FirstOrDefault();

                return pase == null ? UltimoPaseSade.SinDatos : new UltimoPaseSade(pase.BuzonDestino, pase.FechaUltimoPase);
            }
        }

        /// <summary>
        /// Último pase SADE de varios expedientes. Devuelve sólo los que tienen datos.
        /// Usa la caché de sesión si contiene todos los expedientes pedidos; si no, consulta la base y la actualiza.
        /// </summary>
        public Dictionary<string, UltimoPaseSade> ObtenerUltimosPasesSade(List<string> expedientes)
        {
            if (expedientes == null || !expedientes.Any())
                return new Dictionary<string, UltimoPaseSade>();

            var cache = ObtenerCacheSade();
            if (cache != null && expedientes.All(cache.ContainsKey))
            {
                return expedientes
                    .Distinct()
                    .Where(e => cache[e].TieneDatos)
                    .ToDictionary(e => e, e => cache[e]);
            }

            try
            {
                Dictionary<string, UltimoPaseSade> resultado;
                using (var context = new IVCdbContext())
                {
                    // "TOP 1 ... ORDER BY fecha DESC" por expediente.
                    var ultimosPases = context.PasesSade
                        .AsNoTracking()
                        .Where(p => expedientes.Contains(p.Expediente))
                        .GroupBy(p => p.Expediente)
                        .Select(g => g.OrderByDescending(p => p.FechaUltimoPase).FirstOrDefault())
                        .ToList();

                    resultado = ultimosPases
                        .Where(p => p != null)
                        .ToDictionary(p => p.Expediente, p => new UltimoPaseSade(p.BuzonDestino, p.FechaUltimoPase));
                }

                // Actualizar la caché con los que tienen datos y marcar los que no como consultados.
                var cacheActual = cache ?? new Dictionary<string, UltimoPaseSade>();
                foreach (var kvp in resultado)
                    cacheActual[kvp.Key] = kvp.Value;
                foreach (var expediente in expedientes)
                {
                    if (!resultado.ContainsKey(expediente))
                        cacheActual[expediente] = UltimoPaseSade.SinDatos;
                }
                ContextoWeb.Actual?.GuardarEnSesion(ClaveCacheSade, cacheActual);

                return resultado;
            }
            catch (Exception ex)
            {
                Trace.TraceError("[SADE] Error en carga masiva: {0}", ex);
                throw;
            }
        }

        /// <summary>Descarta la caché SADE de la sesión para forzar la recarga desde la base.</summary>
        public static void LimpiarCacheSade()
        {
            ContextoWeb.Actual?.QuitarDeSesion(ClaveCacheSade);
        }

        private static Dictionary<string, UltimoPaseSade> ObtenerCacheSade()
        {
            return ContextoWeb.Actual?.ObtenerDeSesion(ClaveCacheSade) as Dictionary<string, UltimoPaseSade>;
        }

        // ---------- SIGAF (DEVENGADOS) ----------

        /// <summary>Suma de IMPORTE_PP devengado por expediente, en una sola consulta.</summary>
        public Dictionary<string, decimal> ObtenerDevengados(List<string> expedientes)
        {
            if (expedientes == null || !expedientes.Any())
                return new Dictionary<string, decimal>();

            try
            {
                using (var context = new IVCdbContext())
                {
                    return context.Devengados
                        .AsNoTracking()
                        .Where(d => expedientes.Contains(d.EeFinanciera))
                        .GroupBy(d => d.EeFinanciera)
                        .Select(g => new { Expediente = g.Key, Total = g.Sum(d => (decimal?)d.ImportePp) ?? 0 })
                        .ToDictionary(r => r.Expediente, r => r.Total);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("[SIGAF] Error en carga masiva: {0}", ex);
                throw;
            }
        }

        // ---------- ACDIR (GEDO) ----------

        private class AcdirRowDto
        {
            public string Expediente { get; set; }
            public string Acdir { get; set; }
            public DateTime? Fecha { get; set; }
        }

        /// <summary>
        /// Número especial ACDIR (GEDO) más reciente por expediente. Devuelve sólo los expedientes que tienen uno.
        /// Consulta en lotes, con caché de sesión (también de los no encontrados).
        /// </summary>
        public Dictionary<string, string> ObtenerAcdirs(List<string> expedientes)
        {
            var resultado = new Dictionary<string, string>();
            var expedientesValidos = (expedientes ?? new List<string>())
                .Where(e => !string.IsNullOrEmpty(e))
                .Distinct()
                .ToList();
            if (expedientesValidos.Count == 0)
                return resultado;

            try
            {
                var cacheActual = ContextoWeb.Actual?.ObtenerDeSesion(ClaveCacheAcdir) as Dictionary<string, string>
                                  ?? new Dictionary<string, string>();

                var faltantes = expedientesValidos.Where(e => !cacheActual.ContainsKey(e)).ToList();
                if (faltantes.Any())
                {
                    for (int i = 0; i < faltantes.Count; i += TamanioLoteAcdir)
                    {
                        var lote = faltantes.Skip(i).Take(TamanioLoteAcdir).ToList();

                        foreach (var fila in ConsultarAcdir(lote))
                            cacheActual[fila.Expediente] = fila.Acdir;

                        foreach (var expediente in lote)
                        {
                            if (!cacheActual.ContainsKey(expediente))
                                cacheActual[expediente] = null;
                        }
                    }

                    ContextoWeb.Actual?.GuardarEnSesion(ClaveCacheAcdir, cacheActual);
                }

                foreach (var expediente in expedientesValidos)
                {
                    if (cacheActual.TryGetValue(expediente, out var valor) && !string.IsNullOrEmpty(valor))
                        resultado[expediente] = valor;
                }

                return resultado;
            }
            catch (Exception ex)
            {
                Trace.TraceError("[ACDIR] Error en consulta bulk: {0}", ex);
                throw;
            }
        }

        /// <summary>
        /// Fila de GEDO con la mayor FECHA_FIRMA por expediente. SQL parametrizado: la entidad AcdirEF declara
        /// NRO_EXPEDIENTE como clave pero GEDO tiene varias filas por expediente y no mapea FECHA_FIRMA; hasta
        /// corregir ese mapeo, la consulta se hace a mano.
        /// </summary>
        private static List<AcdirRowDto> ConsultarAcdir(List<string> lote)
        {
            var parametros = new List<SqlParameter>();
            var marcadores = new List<string>();
            for (int p = 0; p < lote.Count; p++)
            {
                var nombre = "@p" + p;
                marcadores.Add(nombre);
                parametros.Add(new SqlParameter(nombre, lote[p]));
            }

            var sql = $@"
SELECT g.NRO_EXPEDIENTE AS Expediente, g.NRO_ESPECIAL AS Acdir, g.FECHA_FIRMA AS Fecha
FROM GEDO g
INNER JOIN (
    SELECT NRO_EXPEDIENTE, MAX(FECHA_FIRMA) AS MaxFecha
    FROM GEDO
    WHERE NRO_EXPEDIENTE IN ({string.Join(",", marcadores)})
    GROUP BY NRO_EXPEDIENTE
) m ON g.NRO_EXPEDIENTE = m.NRO_EXPEDIENTE AND g.FECHA_FIRMA = m.MaxFecha";

            using (var context = new IVCdbContext())
            {
                return context.Database.SqlQuery<AcdirRowDto>(sql, parametros.ToArray()).ToList();
            }
        }
    }
}
