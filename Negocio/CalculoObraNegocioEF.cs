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
                    int userAreaId = UserHelper.GetUserAreaId();

                    if (obraIds == null || !obraIds.Any())
                    {
                        if (!UserHelper.IsUserAdmin())
                        {
                            if (userAreaId != 0)
                                obraIds = context.Obras.AsNoTracking().Where(o => o.AreaId == userAreaId).Select(o => o.Id).ToList();
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
        /// Construye una lista de ObraDTO a partir de entidades ObraEF y el diccionario financiero.
        /// </summary>
        public List<ObraDTO> ConstruirObraDTOs(IEnumerable<ObraEF> obras, Dictionary<int, (decimal? Autorizado2026, decimal? MontoCertificado, decimal? Porcentaje, decimal? MontoInicial, decimal? MontoActual, decimal? MontoFaltante, DateTime? FechaInicio, DateTime? FechaFin)> finanzas)
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
                FechaFin = finanzas != null && finanzas.ContainsKey(o.Id) ? finanzas[o.Id].FechaFin : (DateTime?)null
            }).ToList();

            return listaDto;
        }
    }
}
