using Dominio;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

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
                    // NUEVO: Caso especial para usuario con AreaId 18 - acceso a áreas 1, 2 y 3
                    if (usuario.AreaId == 19)
                    {
                        var areasPermitidas = new List<int> { 1, 2, 3 };
                        query = query.Where(l => l.ObraEF.AreaId.HasValue && areasPermitidas.Contains(l.ObraEF.AreaId.Value));
                    }
                    else if (usuario.AreaId.HasValue)
                        query = query.Where(l => l.ObraEF.AreaId == usuario.AreaId);
                    else if (usuario.Area != null)
                        query = query.Where(l => l.ObraEF.AreaId == usuario.Area.Id);
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

        public int ContarPorUsuario(UsuarioEF usuario, string filtroGeneral = null)
        {
            using (var context = new IVCdbContext())
            {
                var query = context.Legitimos
                    .Include(l => l.ObraEF)
                    .Include(l => l.ObraEF.Barrio)
                    .Include(l => l.ObraEF.Empresa)
                    .AsQueryable();

                if (!usuario.Tipo)
                {
                    // NUEVO: Caso especial para usuario con AreaId 18 - acceso a áreas 1, 2 y 3
                    if (usuario.AreaId == 19)
                    {
                        var areasPermitidas = new List<int> { 1, 2, 3 };
                        query = query.Where(l => l.ObraEF.AreaId.HasValue && areasPermitidas.Contains(l.ObraEF.AreaId.Value));
                    }
                    else if (usuario.AreaId.HasValue)
                        query = query.Where(l => l.ObraEF.AreaId == usuario.AreaId);
                    else if (usuario.Area != null)
                        query = query.Where(l => l.ObraEF.AreaId == usuario.Area.Id);
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
                // NUEVO: Caso especial para usuario con AreaId 18 - acceso a áreas 1, 2 y 3
                if (usuario.AreaId == 19)
                {
                    var areasPermitidas = new List<int> { 1, 2, 3 };
                    query = query.Where(l => l.ObraEF.AreaId.HasValue && areasPermitidas.Contains(l.ObraEF.AreaId.Value));
                }
                else if (usuario.AreaId.HasValue)
                    query = query.Where(l => l.ObraEF.AreaId == usuario.AreaId);
                else if (usuario.Area != null)
                    query = query.Where(l => l.ObraEF.AreaId == usuario.Area.Id);
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

                    foreach (var l in page)
                    {
                        // Obtener sigaf si existe
                        decimal? sigafVal = null;
                        if (!string.IsNullOrWhiteSpace(l.Expediente) && datosSigaf != null && datosSigaf.TryGetValue(l.Expediente, out var sVal))
                            sigafVal = sVal;

                        // Calcular propiedades adicionales exactamente como en CertificadosEF
                        var (estado, buzon, fecha) = calculoRedeterminacionNegocioEF.CalcularPropiedadesAdicionales(l.Expediente, l.Certificado, sigafVal, datosSade ?? new Dictionary<string, (string, DateTime?)>());

                        l.Sigaf = sigafVal;
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

                    foreach (var l in list)
                    {
                        decimal? sigafVal = null;
                        if (!string.IsNullOrWhiteSpace(l.Expediente) && datosSigaf != null && datosSigaf.TryGetValue(l.Expediente, out var sVal))
                            sigafVal = sVal;

                        var (estado, buzon, fecha) = calculoRedeterminacionNegocioEF.CalcularPropiedadesAdicionales(l.Expediente, l.Certificado, sigafVal, datosSade ?? new Dictionary<string, (string, DateTime?)>());

                        l.Sigaf = sigafVal;
                        l.BuzonSade = buzon;
                        l.FechaSade = fecha;
                        l.Estado = estado;
                    }
                }

                return list;
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
                    // NUEVO: Caso especial para usuario con AreaId 18 - acceso a áreas 1, 2 y 3
                    if (usuario.AreaId == 19)
                    {
                        var areasPermitidas = new List<int> { 1, 2, 3 };
                        query = query.Where(l => l.ObraEF.AreaId.HasValue && areasPermitidas.Contains(l.ObraEF.AreaId.Value));
                    }
                    else if (usuario.AreaId.HasValue)
                        query = query.Where(l => l.ObraEF.AreaId == usuario.AreaId);
                    else if (usuario.Area != null)
                        query = query.Where(l => l.ObraEF.AreaId == usuario.Area.Id);
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
