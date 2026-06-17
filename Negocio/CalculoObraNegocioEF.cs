using Dominio;
using Dominio.DTO;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Negocio
{
    public class CalculoObraNegocioEF
    {
        /// <summary>
        /// Calcula los valores financieros por obra usando consultas EF (no legacy ADO).
        /// Devuelve un diccionario { ObraId -> aggregate primitive values } y no depende de DTO auxiliares.
        /// </summary>
        public Dictionary<int, (decimal? Autorizado2026, decimal? MontoCertificado, decimal? Porcentaje, decimal? MontoInicial, decimal? MontoActual, decimal? MontoFaltante, DateTime? FechaInicio, DateTime? FechaFin)> ObtenerFinanzasPorObras(List<int> obraIds = null)
        {
            try
            {
                using (var context = new IVCdbContext())
                {
                    // Resolver obras si no se recibieron: si hay usuario y no es admin filtrar por su área, sino todas.
                    if (obraIds == null || !obraIds.Any())
                    {
                        if (!UserHelper.IsUserAdmin())
                        {
                            var filtroAreaIds = UserHelper.GetFullCurrentUser().IvcAreaIds;
                            if (filtroAreaIds != null && filtroAreaIds.Count > 0)
                            {
                                obraIds = context.Obras.AsNoTracking()
                                    .Where(o => o.AreaId.HasValue && filtroAreaIds.Contains(o.AreaId.Value))
                                    .Select(o => o.Id)
                                    .ToList();
                            }
                            else
                                obraIds = new List<int>();
                        }
                        else
                        {
                            obraIds = context.Obras.AsNoTracking().Select(o => o.Id).ToList();
                        }
                    }

                    if (obraIds == null || !obraIds.Any()) return new Dictionary<int, (decimal?, decimal?, decimal?, decimal?, decimal?, decimal?, DateTime?, DateTime?)>();

                    // No usamos año en el cálculo; se calcula el total histórico por obra.
                    var proyectos = context.Proyectos.AsNoTracking()
                        .Where(p => obraIds.Contains(p.ObraId))
                        .Select(p => new { p.ObraId, p.Autorizado2026 })
                        .ToList();

                    var dictAutorizado2026 = proyectos.ToDictionary(p => p.ObraId, p => (decimal?)p.Autorizado2026);

                    var autorizantesGroup = context.Autorizantes.AsNoTracking()
                        .Where(a => obraIds.Contains(a.ObraId))
                        .GroupBy(a => a.ObraId)
                        .Select(g => new
                        {
                            ObraId = g.Key,
                            SumAutorizantes = g.Sum(a => (decimal?)a.MontoAutorizado),
                            SumAutorizantesConcepto4 = g.Where(a => a.ConceptoId == 4).Sum(a => (decimal?)a.MontoAutorizado)
                        })
                        .ToList();

                    var dictAutorizantes = autorizantesGroup.ToDictionary(x => x.ObraId, x => new { x.SumAutorizantes, x.SumAutorizantesConcepto4 });

                    var legitimosGroup = context.Legitimos.AsNoTracking()
                        .Where(l => obraIds.Contains(l.ObraId))
                        .GroupBy(l => l.ObraId)
                        .Select(g => new
                        {
                            ObraId = g.Key,
                            SumLegitimos = g.Sum(l => (decimal?)l.Certificado),
                            MinLegitimo = g.Min(l => l.MesAprobacion),
                            MaxLegitimo = g.Max(l => l.MesAprobacion)
                        })
                        .ToList();

                    var dictLegitimos = legitimosGroup.ToDictionary(x => x.ObraId, x => x);

                    var certificadosJoin = (from c in context.Certificados.AsNoTracking()
                                            join a in context.Autorizantes.AsNoTracking() on c.CodigoAutorizante equals a.CodigoAutorizante
                                            where obraIds.Contains(a.ObraId)
                                            group c by a.ObraId into g
                                            select new
                                            {
                                                ObraId = g.Key,
                                                SumCertificados = g.Sum(c => (decimal?)c.MontoTotal),
                                                // SumCertificadosYear removido: usamos el total histórico
                                                MinCert = g.Min(c => c.MesAprobacion),
                                                MaxCert = g.Max(c => c.MesAprobacion)
                                            }).ToList();

                    var dictCertificados = certificadosJoin.ToDictionary(x => x.ObraId, x => x);

                    var result = new Dictionary<int, (decimal?, decimal?, decimal?, decimal?, decimal?, decimal?, DateTime?, DateTime?)>();
                    foreach (var id in obraIds)
                    {
                        decimal? Autorizado2026 = null;
                        dictAutorizado2026.TryGetValue(id, out Autorizado2026);

                        decimal sumAutorizantes = 0m, sumAutorizantesConcepto4 = 0m;
                        if (dictAutorizantes.TryGetValue(id, out var ag)) { sumAutorizantes = ag.SumAutorizantes.GetValueOrDefault(0m); sumAutorizantesConcepto4 = ag.SumAutorizantesConcepto4.GetValueOrDefault(0m); }

                        decimal sumLegitimos = 0m;
                        DateTime? minLeg = null, maxLeg = null;
                        if (dictLegitimos.TryGetValue(id, out var lg)) { sumLegitimos = lg.SumLegitimos.GetValueOrDefault(0m); minLeg = lg.MinLegitimo; maxLeg = lg.MaxLegitimo; }

                        decimal sumCertificados = 0m;
                        DateTime? minCert = null, maxCert = null;
                        if (dictCertificados.TryGetValue(id, out var cg)) { sumCertificados = cg.SumCertificados.GetValueOrDefault(0m); minCert = cg.MinCert; maxCert = cg.MaxCert; }

                        decimal? montoCertificado = (sumCertificados + sumLegitimos);

                        decimal? montoInicial = (sumAutorizantesConcepto4 + sumLegitimos);

                        decimal? montoActual = (sumAutorizantes + sumLegitimos);

                        decimal? montoFaltante = (montoActual - (sumCertificados + sumLegitimos));

                        DateTime? fechaInicio = null, fechaFin = null;
                        var mins = new List<DateTime?> { minCert, minLeg }.Where(d => d.HasValue).ToList();
                        if (mins.Any()) fechaInicio = mins.Min();
                        var maxs = new List<DateTime?> { maxCert, maxLeg }.Where(d => d.HasValue).ToList();
                        if (maxs.Any()) fechaFin = maxs.Max();

                        decimal? porcentaje = null;
                        if (Autorizado2026.HasValue && Autorizado2026.Value > 0)
                        {
                            porcentaje = (montoCertificado.HasValue ? (montoCertificado.Value / Autorizado2026.Value) * 100m : 0m);
                        }

                        result[id] = (Autorizado2026, montoCertificado, porcentaje, montoInicial, montoActual, montoFaltante, fechaInicio, fechaFin);
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error al calcular finanzas por obras (EF)", ex);
            }
        }

        /// <summary>
        /// Calcula la ejecución física acumulada (suma de PorcEjecFisica) de un autorizante,
        /// considerando solo certificados (los legítimos no cuentan para ejecución física).
        /// También devuelve el faltante físico (100 - acumulado).
        /// </summary>
        public (decimal EjecFisica, decimal FaltanteEjecFisica) EjecFisicaPorAutorizante(string codigoAutorizante)
        {
            using (var context = new IVCdbContext())
            {
                var total = context.Certificados.AsNoTracking()
                    .Where(c => c.CodigoAutorizante == codigoAutorizante && c.PorcEjecFisica.HasValue)
                    .Sum(c => (decimal?)c.PorcEjecFisica) ?? 0m;

                return (total, 100m - total);
            }
        }

        /// <summary>
        /// Calcula la ejecución física de una obra como promedio ponderado por MontoAutorizado.
        /// EjecObra = Σ(MontoAutorizado_i * SumaEjec_i) / Σ(MontoAutorizado_i)
        /// SumaEjec_i = suma de PorcEjecFisica de certificados del autorizante i (legítimos no cuentan).
        /// </summary>
        public (decimal EjecFisica, decimal FaltanteEjecFisica) EjecFisicaPorObra(int obraId)
        {
            using (var context = new IVCdbContext())
            {
                var autorizantes = context.Autorizantes.AsNoTracking()
                    .Where(a => a.ObraId == obraId)
                    .Select(a => new { a.CodigoAutorizante, a.MontoAutorizado })
                    .ToList();

                if (!autorizantes.Any()) return (0m, 100m);

                var codigos = autorizantes.Select(a => a.CodigoAutorizante).Distinct().ToList();

                var sumCert = context.Certificados.AsNoTracking()
                    .Where(c => codigos.Contains(c.CodigoAutorizante) && c.PorcEjecFisica.HasValue)
                    .GroupBy(c => c.CodigoAutorizante)
                    .Select(g => new { Codigo = g.Key, Suma = g.Sum(c => (decimal?)c.PorcEjecFisica) ?? 0m })
                    .ToDictionary(g => g.Codigo, g => g.Suma);

                decimal sumPeso = autorizantes.Sum(a => a.MontoAutorizado);
                if (sumPeso == 0m) return (0m, 100m);

                decimal numerador = autorizantes.Sum(a =>
                    a.MontoAutorizado * (sumCert.TryGetValue(a.CodigoAutorizante, out var v) ? v : 0m));

                var total = numerador / sumPeso;
                return (total, 100m - total);
            }
        }

        /// <summary>
        /// Calcula en bulk la ejecución física acumulada por CodigoAutorizante.
        /// Solo considera certificados (los legítimos no cuentan para ejecución física).
        /// </summary>
        public Dictionary<string, decimal> ObtenerEjecFisicaBulkPorCodigos(List<string> codigos)
        {
            if (codigos == null || !codigos.Any()) return new Dictionary<string, decimal>();
            using (var context = new IVCdbContext())
            {
                return context.Certificados.AsNoTracking()
                    .Where(c => codigos.Contains(c.CodigoAutorizante) && c.PorcEjecFisica.HasValue)
                    .GroupBy(c => c.CodigoAutorizante)
                    .Select(g => new { Codigo = g.Key, Suma = g.Sum(c => (decimal?)c.PorcEjecFisica) ?? 0m })
                    .ToList()
                    .ToDictionary(g => g.Codigo, g => g.Suma);
            }
        }

        /// <summary>
        /// Calcula en bulk la ejecución física por ObraId como promedio ponderado por MontoAutorizado.
        /// EjecObra = Σ(MontoAutorizado_i * SumaEjec_i) / Σ(MontoAutorizado_i)
        /// Solo considera certificados (los legítimos no cuentan para ejecución física).
        /// </summary>
        public Dictionary<int, decimal> ObtenerEjecFisicaBulkPorObras(List<int> obraIds)
        {
            if (obraIds == null || !obraIds.Any()) return new Dictionary<int, decimal>();
            using (var context = new IVCdbContext())
            {
                var autorizantes = context.Autorizantes.AsNoTracking()
                    .Where(a => obraIds.Contains(a.ObraId))
                    .Select(a => new { a.ObraId, a.CodigoAutorizante, a.MontoAutorizado })
                    .ToList();

                if (!autorizantes.Any()) return new Dictionary<int, decimal>();

                var codigos = autorizantes.Select(a => a.CodigoAutorizante).Distinct().ToList();

                var ejecPorCodigo = context.Certificados.AsNoTracking()
                    .Where(c => codigos.Contains(c.CodigoAutorizante) && c.PorcEjecFisica.HasValue)
                    .GroupBy(c => c.CodigoAutorizante)
                    .Select(g => new { Codigo = g.Key, Suma = g.Sum(c => (decimal?)c.PorcEjecFisica) ?? 0m })
                    .ToList()
                    .ToDictionary(g => g.Codigo, g => g.Suma);

                var result = new Dictionary<int, decimal>();
                foreach (var grupo in autorizantes.GroupBy(a => a.ObraId))
                {
                    decimal sumPeso = grupo.Sum(a => a.MontoAutorizado);
                    if (sumPeso == 0m) continue;

                    decimal numerador = grupo.Sum(a =>
                        a.MontoAutorizado * (ejecPorCodigo.TryGetValue(a.CodigoAutorizante, out var v) ? v : 0m));

                    result[grupo.Key] = numerador / sumPeso;
                }
                return result;
            }
        }

        /// <summary>
        /// Construye una lista de ObraDTO a partir de entidades ObraEF y el diccionario financiero.
        /// </summary>
        public List<ObraDTO> ConstruirObraDTOs(IEnumerable<ObraEF> obras, Dictionary<int, (decimal? Autorizado2026, decimal? MontoCertificado, decimal? Porcentaje, decimal? MontoInicial, decimal? MontoActual, decimal? MontoFaltante, DateTime? FechaInicio, DateTime? FechaFin)> finanzas, Dictionary<int, decimal> ejecFisicaDict = null)
        {
            var listaDto = obras.Select(o => new ObraDTO
            {
                Id = o.Id,
                Area = o.Area?.Nombre,
                AreaId = o.AreaId,
                Empresa = o.Empresa?.Nombre,
                EmpresaId = o.EmpresaId,
                Contrata = o.Contrata?.Nombre,
                ContrataId = o.ContrataId,
                Numero = o.Numero,
                ObraNumero = o.ObraNumero,
                Etapa = o.Etapa,
                Anio = o.Anio,
                Barrio = o.Barrio?.Nombre,
                BarrioId = o.BarrioId ?? 0,
                Descripcion = o.Descripcion,
                LineaGestionNombre = o.Proyecto?.LineaGestionEF?.Nombre,
                ProyectoNombre = o.Proyecto?.Nombre,
                ProyectoId = o.Proyecto != null ? o.Proyecto.Id : (int?)null,
                Autorizado2026 = finanzas != null && finanzas.ContainsKey(o.Id) ? finanzas[o.Id].Autorizado2026 : (decimal?)null,
                MontoCertificado = finanzas != null && finanzas.ContainsKey(o.Id) ? finanzas[o.Id].MontoCertificado : (decimal?)null,
                Porcentaje = finanzas != null && finanzas.ContainsKey(o.Id) ? finanzas[o.Id].Porcentaje : (decimal?)null,
                MontoInicial = finanzas != null && finanzas.ContainsKey(o.Id) ? finanzas[o.Id].MontoInicial : (decimal?)null,
                MontoActual = finanzas != null && finanzas.ContainsKey(o.Id) ? finanzas[o.Id].MontoActual : (decimal?)null,
                MontoFaltante = finanzas != null && finanzas.ContainsKey(o.Id) ? finanzas[o.Id].MontoFaltante : (decimal?)null,
                FechaInicio = finanzas != null && finanzas.ContainsKey(o.Id) ? finanzas[o.Id].FechaInicio : (DateTime?)null,
                FechaFin = finanzas != null && finanzas.ContainsKey(o.Id) ? finanzas[o.Id].FechaFin : (DateTime?)null,
                EjecFisica = ejecFisicaDict != null && ejecFisicaDict.ContainsKey(o.Id) ? (decimal?)ejecFisicaDict[o.Id] : (decimal?)null,
                FaltanteEjecFisica = ejecFisicaDict != null && ejecFisicaDict.ContainsKey(o.Id) ? (decimal?)(100m - ejecFisicaDict[o.Id]) : (decimal?)null
            }).ToList();

            return listaDto;
        }
    }
}
