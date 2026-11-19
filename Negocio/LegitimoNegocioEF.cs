using Dominio;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Negocio.Redeterminaciones;

namespace Negocio
{
    public class LegitimoNegocioEF
    {
        public List<LegitimoEF> ListarPorUsuario(UsuarioEF usuario)
        {
            using (var context = new IVCdbContext())
            {
                var query = context.Legitimos
                    .Include(l => l.ObraEF)
                    .Include(l => l.ObraEF.Area)
                    .Include(l => l.ObraEF.Barrio)
                    .Include(l => l.ObraEF.Empresa)
                    .Include(l => l.ObraEF.Proyecto)
                    .Include(l => l.ObraEF.Proyecto.LineaGestionEF)
                    .AsQueryable();

                if (!usuario.Tipo)
                {
                    var filtroAreaIds = UserHelper.AreaIdsParaFiltro(usuario);
                    query = query.Where(l => l.ObraEF.AreaId.HasValue && filtroAreaIds.Contains(l.ObraEF.AreaId.Value));
                }

                var list = query.ToList();
                // Rellenar propiedades auxiliares que no están en la tabla
                foreach (var l in list)
                {
                    try
                    {
                        l.Empresa = l.ObraEF?.Empresa?.Nombre;
                        l.Linea = l.ObraEF?.Proyecto?.LineaGestionEF?.Nombre;
                        // Sigaf/FechaSade/BuzonSade se dejarán nulos aquí; se calculan en capas superiores si es necesario
                        l.Sigaf = null;
                        l.FechaSade = null;
                        l.BuzonSade = null;
                        // Determinar estado por defecto: si no tiene expediente => NO INICIADO, si tiene expediente pero sin SIGAF => EN TRAMITE
                        if (string.IsNullOrWhiteSpace(l.Expediente))
                            l.Estado = "NO INICIADO";
                        else
                            l.Estado = "EN TRAMITE";
                    }
                    catch { }
                }
                return list;
            }
        }

        public void Agregar(LegitimoEF legitimo)
        {
            using (var context = new IVCdbContext())
            {
                context.Legitimos.Add(legitimo);
                context.SaveChanges();
            }
        }

        public void Modificar(LegitimoEF legitimo)
        {
            using (var context = new IVCdbContext())
            {
                context.Entry(legitimo).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public bool Eliminar(int id)
        {
            using (var context = new IVCdbContext())
            {
                var entity = context.Legitimos.Find(id);
                if (entity == null) return false;
                context.Legitimos.Remove(entity);
                context.SaveChanges();
            }
            return true;
        }

        /// <summary>
        /// Actualiza únicamente el porcentaje de ejecución física de un legítimo.
        /// </summary>
        public bool ActualizarEjecFisica(int id, decimal? porcentaje)
        {
            using (var context = new IVCdbContext())
            {
                var entity = context.Legitimos.Find(id);
                if (entity == null) return false;
                entity.PorcEjecFisica = porcentaje;
                return context.SaveChanges() > 0;
            }
        }

        public int ContarPorUsuario(UsuarioEF usuario, string filtroGeneral = null)
        {
            using (var context = new IVCdbContext())
            {
                var query = context.Legitimos
                    .Include(l => l.ObraEF)
                    .Include(l => l.ObraEF.Barrio)
                    .Include(l => l.ObraEF.Area)
                    .Include(l => l.ObraEF.Empresa)
                    .AsQueryable();

                if (!usuario.Tipo)
                {
                    var filtroAreaIds = UserHelper.AreaIdsParaFiltro(usuario);
                    query = query.Where(l => l.ObraEF.AreaId.HasValue && filtroAreaIds.Contains(l.ObraEF.AreaId.Value));
                }

                if (!string.IsNullOrEmpty(filtroGeneral))
                {
                    query = query.Where(l =>
                        l.ObraEF.Descripcion.Contains(filtroGeneral) ||
                        (l.Expediente != null && l.Expediente.Contains(filtroGeneral)) ||
                        (l.CodigoAutorizante != null && l.CodigoAutorizante.Contains(filtroGeneral)));
                }

                return query.Count();
            }
        }

        #region Métodos para Paginación con filtros (paradigma FormulacionEF)

        private IQueryable<LegitimoEF> BuildBaseQuery(IVCdbContext context, UsuarioEF usuario)
        {
            var query = context.Legitimos
                .Include(l => l.ObraEF)
                .Include(l => l.ObraEF.Area)
                .Include(l => l.ObraEF.Barrio)
                .Include(l => l.ObraEF.Empresa)
                .Include(l => l.ObraEF.Proyecto)
                .Include(l => l.ObraEF.Proyecto.LineaGestionEF)
                .AsQueryable();

            if (!usuario.Tipo)
            {
                var filtroAreaIds = UserHelper.AreaIdsParaFiltro(usuario);
                query = query.Where(l => l.ObraEF.AreaId.HasValue && filtroAreaIds.Contains(l.ObraEF.AreaId.Value));
            }

            return query;
        }

        private IQueryable<LegitimoEF> ApplyOptionalFilters(IQueryable<LegitimoEF> query, string filtroGeneral,
            List<int> areas, List<int> lineasGestion, List<string> empresas, List<string> autorizantes, List<DateTime?> meses)
        {
            if (!string.IsNullOrWhiteSpace(filtroGeneral))
            {
                query = query.Where(l =>
                    (l.ObraEF != null && l.ObraEF.Descripcion.Contains(filtroGeneral)) ||
                    (l.ObraEF != null && l.ObraEF.Empresa != null && l.ObraEF.Empresa.Nombre.Contains(filtroGeneral)) ||
                    (l.ObraEF != null && l.ObraEF.Proyecto != null && l.ObraEF.Proyecto.LineaGestionEF != null && l.ObraEF.Proyecto.LineaGestionEF.Nombre.Contains(filtroGeneral)) ||
                    (l.CodigoAutorizante != null && l.CodigoAutorizante.Contains(filtroGeneral)) ||
                    (l.Expediente != null && l.Expediente.Contains(filtroGeneral))
                );
            }

            if (areas != null && areas.Any())
                query = query.Where(l => l.ObraEF.AreaId.HasValue && areas.Contains(l.ObraEF.AreaId.Value));
            if (lineasGestion != null && lineasGestion.Any())
                query = query.Where(l => l.ObraEF.Proyecto != null && l.ObraEF.Proyecto.LineaGestionEF != null && lineasGestion.Contains(l.ObraEF.Proyecto.LineaGestionEF.Id));
            if (empresas != null && empresas.Any())
                query = query.Where(l => l.ObraEF.Empresa != null && empresas.Contains(l.ObraEF.Empresa.Nombre));
            if (autorizantes != null && autorizantes.Any())
                query = query.Where(l => autorizantes.Contains(l.CodigoAutorizante));
            if (meses != null && meses.Any())
            {
                var mesesFiltered = meses.Where(m => m.HasValue).Select(m => m.Value.Date).ToList();
                if (mesesFiltered.Any())
                    query = query.Where(l => l.MesAprobacion.HasValue && mesesFiltered.Contains(DbFunctions.TruncateTime(l.MesAprobacion).Value));
            }

            return query;
        }

        public int ContarPorUsuarioConFiltros(UsuarioEF usuario, string filtroGeneral,
            List<int> areas, List<int> lineasGestion, List<string> empresas, List<string> autorizantes, List<DateTime?> meses)
        {
            using (var context = new IVCdbContext())
            {
                var query = BuildBaseQuery(context, usuario);
                query = ApplyOptionalFilters(query, filtroGeneral, areas, lineasGestion, empresas, autorizantes, meses);
                return query.Count();
            }
        }

        public List<LegitimoEF> ListarPorUsuarioPaginadoConFiltros(UsuarioEF usuario, int pageIndex, int pageSize, string filtroGeneral,
            List<int> areas, List<int> lineasGestion, List<string> empresas, List<string> autorizantes, List<DateTime?> meses)
        {
            using (var context = new IVCdbContext())
            {
                var query = BuildBaseQuery(context, usuario);
                query = ApplyOptionalFilters(query, filtroGeneral, areas, lineasGestion, empresas, autorizantes, meses);

                var page = query.OrderBy(l => l.ObraEF.Descripcion)
                    .Skip(pageIndex * pageSize)
                    .Take(pageSize)
                    .ToList();

                // Rellenar propiedades auxiliares
                foreach (var l in page)
                {
                    try
                    {
                        l.Empresa = l.ObraEF?.Empresa?.Nombre;
                        l.Linea = l.ObraEF?.Proyecto?.LineaGestionEF?.Nombre;
                        l.Sigaf = null; // se rellenará a continuación si hay expedientes
                        l.FechaSade = null;
                        l.BuzonSade = null;
                        // Estado por defecto antes de calcular SIGAF/SADE
                        if (string.IsNullOrWhiteSpace(l.Expediente))
                            l.Estado = "NO INICIADO";
                        else
                            l.Estado = "EN TRAMITE";
                    }
                    catch { }
                }

                // Calcular SIGAF/SADE en bloque para los expedientes de la página
                var expedientesPagina = page.Where(p => !string.IsNullOrWhiteSpace(p.Expediente)).Select(p => p.Expediente).Distinct().ToList();
                if (expedientesPagina.Any())
                {
                    var calculoRedeterminacionNegocioEF = new CalculoRedeterminacionNegocioEF();
                    var datosSade = calculoRedeterminacionNegocioEF.ObtenerDatosSadeBulk(expedientesPagina);
                    var datosSigaf = calculoRedeterminacionNegocioEF.ObtenerSigafBulk(expedientesPagina);

                    // El reparto del devengado se hace entre todos los legítimos del usuario que comparten el
                    // expediente, no sólo los de la página ni los que pasan los filtros opcionales: se traen
                    // (sólo las columnas necesarias) los hermanos que quedaron fuera de la página.
                    var idsPagina = new HashSet<int>(page.Select(p => p.Id));
                    var hermanos = BuildBaseQuery(context, usuario)
                        .Where(l => expedientesPagina.Contains(l.Expediente))
                        .Select(l => new { l.Id, l.Expediente, l.Certificado })
                        .ToList()
                        .Where(h => !idsPagina.Contains(h.Id))
                        .Select(h => new LegitimoEF { Id = h.Id, Expediente = h.Expediente, Certificado = h.Certificado });
                    AsignarSigafProrrateado(page.Concat(hermanos), datosSigaf);

                    foreach (var l in page)
                    {
                        // Calcular propiedades adicionales exactamente como en CertificadosEF
                        var (estado, buzon, fecha) = calculoRedeterminacionNegocioEF.CalcularPropiedadesAdicionales(l.Expediente, l.Certificado, l.Sigaf, datosSade ?? new Dictionary<string, UltimoPaseSade>());

                        l.BuzonSade = buzon;
                        l.FechaSade = fecha;
                        l.Estado = estado;
                    }
                }

                return page;
            }
        }

        /// <summary>
        /// Devuelve la lista completa filtrada (sin paginar) para poblar cabeceras y exportación.
        /// También rellena propiedades auxiliares Sigaf/SADE/Estado para cada elemento.
        /// </summary>
        public List<LegitimoEF> ListarPorUsuarioConFiltrosCompleto(UsuarioEF usuario, string filtroGeneral,
            List<int> areas, List<int> lineasGestion, List<string> empresas, List<string> autorizantes, List<DateTime?> meses)
        {
            using (var context = new IVCdbContext())
            {
                var query = BuildBaseQuery(context, usuario);
                query = ApplyOptionalFilters(query, filtroGeneral, areas, lineasGestion, empresas, autorizantes, meses);
                var list = query.OrderBy(l => l.ObraEF.Descripcion).ToList();

                // Rellenar Empresa/Linea rápidamente
                foreach (var l in list)
                {
                    try
                    {
                        l.Empresa = l.ObraEF?.Empresa?.Nombre;
                        l.Linea = l.ObraEF?.Proyecto?.LineaGestionEF?.Nombre;
                        // Estado por defecto antes de calcular SIGAF/SADE
                        if (string.IsNullOrWhiteSpace(l.Expediente))
                            l.Estado = "NO INICIADO";
                        else
                            l.Estado = "EN TRAMITE";
                    }
                    catch { }
                }

                // Calcular datos SADE/SIGAF en bloque para todos los expedientes filtrados
                var expedientes = list.Where(x => !string.IsNullOrWhiteSpace(x.Expediente)).Select(x => x.Expediente).Distinct().ToList();
                if (expedientes.Any())
                {
                    var calculoRedeterminacionNegocioEF = new CalculoRedeterminacionNegocioEF();
                    var datosSade = calculoRedeterminacionNegocioEF.ObtenerDatosSadeBulk(expedientes);
                    var datosSigaf = calculoRedeterminacionNegocioEF.ObtenerSigafBulk(expedientes);

                    AsignarSigafProrrateado(list, datosSigaf);

                    foreach (var l in list)
                    {
                        var (estado, buzon, fecha) = calculoRedeterminacionNegocioEF.CalcularPropiedadesAdicionales(l.Expediente, l.Certificado, l.Sigaf, datosSade ?? new Dictionary<string, UltimoPaseSade>());

                        l.BuzonSade = buzon;
                        l.FechaSade = fecha;
                        l.Estado = estado;
                    }
                }

                return list;
            }
        }

        /// <summary>
        /// Reparte el devengado SIGAF de cada expediente entre los legítimos que lo comparten, en proporción
        /// a su monto (misma regla que los certificados: ReglasRedeterminaciones.ProrratearSigaf). Un único
        /// legítimo se lleva todo. Los legítimos sin expediente o sin dato SIGAF quedan con Sigaf null.
        /// La lista debe incluir a todos los legítimos del expediente que participan del reparto.
        /// </summary>
        public static void AsignarSigafProrrateado(IEnumerable<LegitimoEF> legitimos, IReadOnlyDictionary<string, decimal> datosSigaf)
        {
            if (legitimos == null) return;

            var devengados = DatosExternosNegocioEF.NormalizarDevengados(datosSigaf);

            var grupos = legitimos
                .Where(l => l != null && !string.IsNullOrWhiteSpace(l.Expediente))
                .GroupBy(l => l.Expediente.Trim(), StringComparer.OrdinalIgnoreCase);

            foreach (var grupo in grupos)
            {
                decimal? totalDevengado = devengados.TryGetValue(grupo.Key, out var total) ? total : (decimal?)null;
                var montos = grupo.Select(l => l.Certificado ?? 0m).ToList();

                foreach (var l in grupo)
                {
                    l.Sigaf = totalDevengado.HasValue
                        ? ReglasRedeterminaciones.ProrratearSigaf(totalDevengado.Value, l.Certificado ?? 0m, montos)
                        : null;
                }
            }
        }

        public decimal SubtotalPorUsuarioConFiltros(UsuarioEF usuario, string filtroGeneral,
            List<int> areas, List<int> lineasGestion, List<string> empresas, List<string> autorizantes, List<DateTime?> meses)
        {
            using (var context = new IVCdbContext())
            {
                var query = BuildBaseQuery(context, usuario);
                query = ApplyOptionalFilters(query, filtroGeneral, areas, lineasGestion, empresas, autorizantes, meses);
                return query.Sum(l => (decimal?)l.Certificado) ?? 0m;
            }
        }

        #endregion

        public List<LegitimoEF> ListarPorUsuarioPaginado(UsuarioEF usuario, int pageIndex, int pageSize, string filtroGeneral = null)
        {
            using (var context = new IVCdbContext())
            {
                var query = context.Legitimos
                    .Include(l => l.ObraEF)
                    .Include(l => l.ObraEF.Area)
                    .Include(l => l.ObraEF.Empresa)
                    .Include(l => l.ObraEF.Proyecto)
                    .Include(l => l.ObraEF.Proyecto.LineaGestionEF)
                    .AsQueryable();

                if (!usuario.Tipo)
                {
                    var filtroAreaIds = UserHelper.AreaIdsParaFiltro(usuario);
                    query = query.Where(l => l.ObraEF.AreaId.HasValue && filtroAreaIds.Contains(l.ObraEF.AreaId.Value));
                }

                if (!string.IsNullOrEmpty(filtroGeneral))
                {
                    query = query.Where(l =>
                        l.ObraEF.Descripcion.Contains(filtroGeneral) ||
                        (l.Expediente != null && l.Expediente.Contains(filtroGeneral)) ||
                        (l.CodigoAutorizante != null && l.CodigoAutorizante.Contains(filtroGeneral)));
                }

                return query.OrderBy(l => l.ObraEF.Descripcion)
                    .Skip(pageIndex * pageSize)
                    .Take(pageSize)
                    .ToList();
            }
        }

        public LegitimoEF ObtenerPorId(int id)
        {
            using (var context = new IVCdbContext())
            {
                var entity = context.Legitimos
                    .Include(l => l.ObraEF)
                    .Include(l => l.ObraEF.Area)
                    .Include(l => l.ObraEF.Barrio)
                    .Include(l => l.ObraEF.Empresa)
                    .Include(l => l.ObraEF.Proyecto)
                    .Include(l => l.ObraEF.Proyecto.LineaGestionEF)
                    .FirstOrDefault(l => l.Id == id);
                return entity;
            }
        }
    }
}
