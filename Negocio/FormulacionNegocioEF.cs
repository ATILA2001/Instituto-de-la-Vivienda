using Dominio;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Negocio
{
    public class FormulacionNegocioEF
    {

        public List<FormulacionEF> ListarPorUsuario(UsuarioEF usuario)
        {
            using (var context = new IVCdbContext())
            {
                var query = context.Formulaciones
                    .Include(f => f.ObraEF)
                    .Include(f => f.ObraEF.Area)
                    .Include(f => f.ObraEF.Empresa)
                    .Include(f => f.ObraEF.Contrata)
                    .Include(f => f.ObraEF.Barrio)
                    .Include(f => f.ObraEF.Proyecto)
                    .Include(f => f.ObraEF.Proyecto.LineaGestionEF)
                    .Include(f => f.UnidadMedidaEF)
                    .Include(f => f.PrioridadEF);

                // Solo filtrar por área si NO es administrador
                if (!usuario.Tipo)
                {
                    var filtroAreaIds = usuario.IvcAreaIds;
                    if (filtroAreaIds != null && filtroAreaIds.Count > 0)
                        query = query.Where(f => f.ObraEF.AreaId.HasValue && filtroAreaIds.Contains(f.ObraEF.AreaId.Value));
                }

                var resultado = query.ToList();

                return resultado;
            }

        }

        public bool Agregar(FormulacionEF formulacion)
        {
            using (var context = new IVCdbContext())
            {
                context.Formulaciones.Add(formulacion);
                return context.SaveChanges() > 0;
            }
        }

        public bool Modificar(FormulacionEF formulacion)
        {
            using (var context = new IVCdbContext())
            {
                context.Entry(formulacion).State = EntityState.Modified;
                return context.SaveChanges() > 0;
            }
        }

        public bool Eliminar(int id)
        {
            using (var context = new IVCdbContext())
            {
                var entity = context.Formulaciones.Find(id);
                if (entity == null) return false;
                context.Formulaciones.Remove(entity);
                context.SaveChanges();
            }
            return true;
        }

        /// <summary>
        /// Obtiene una formulación por su Id incluyendo las entidades relacionadas necesarias.
        /// Devuelve null si no existe.
        /// </summary>
        public FormulacionEF ObtenerPorId(int id)
        {
            using (var context = new IVCdbContext())
            {
                var entidad = context.Formulaciones
                    .Include(f => f.ObraEF)
                    .Include(f => f.ObraEF.Area)
                    .Include(f => f.ObraEF.Empresa)
                    .Include(f => f.ObraEF.Contrata)
                    .Include(f => f.ObraEF.Barrio)
                    .Include(f => f.ObraEF.Proyecto)
                    .Include(f => f.ObraEF.Proyecto.LineaGestionEF)
                    .Include(f => f.UnidadMedidaEF)
                    .Include(f => f.PrioridadEF)
                    .FirstOrDefault(f => f.Id == id);

                return entidad;
            }
        }

        #region Pivot por obra (ciclo de 3 años)

        /// <summary>
        /// Arma el viewmodel pivoteado de una obra a partir de sus filas FORMULACION del ciclo.
        /// Los campos compartidos se toman de cualquier fila (son idénticos); los montos se
        /// ubican por año en las posiciones del ciclo.
        /// </summary>
        private FormulacionPivotEF BuildPivot(int obraId, List<FormulacionEF> filas, int[] anios)
        {
            var first = filas.OrderBy(f => f.FechaPeriodo).FirstOrDefault();
            var pivot = new FormulacionPivotEF
            {
                ObraId = obraId,
                ObraEF = first?.ObraEF,
                Ppi = first?.Ppi,
                Techos = first?.Techos,
                MesBase = first?.MesBase,
                UnidadMedidaId = first?.UnidadMedidaId,
                UnidadMedidaEF = first?.UnidadMedidaEF,
                ValorMedida = first?.ValorMedida,
                PrioridadId = first?.PrioridadId,
                PrioridadEF = first?.PrioridadEF,
                BreveDescripcion = first?.BreveDescripcion,
                FechaInicio = first?.FechaInicio,
                FechaFin = first?.FechaFin,
                Observaciones = first?.Observaciones
            };
            pivot.MontoAnio1 = filas.FirstOrDefault(f => f.FechaPeriodo.Year == anios[0])?.Monto;
            pivot.MontoAnio2 = filas.FirstOrDefault(f => f.FechaPeriodo.Year == anios[1])?.Monto;
            pivot.MontoAnio3 = filas.FirstOrDefault(f => f.FechaPeriodo.Year == anios[2])?.Monto;
            return pivot;
        }

        /// <summary>
        /// Cuenta las OBRAS distintas (no filas) con formulación en el ciclo vigente, aplicando filtros.
        /// </summary>
        public int ContarObrasConFiltros(UsuarioEF usuario, string filtroGeneral,
            List<int> areas, List<int> lineasGestion, List<int> proyectos, List<int> prioridades,
            List<int> obras = null, List<int> empresas = null, List<int> barrios = null)
        {
            var anios = FormulacionCiclo.Anios;
            using (var context = new IVCdbContext())
            {
                var query = BuildBaseQuery(context, usuario);
                query = ApplyOptionalFilters(query, filtroGeneral, areas, lineasGestion, proyectos, null, prioridades, obras, null, empresas, null, barrios);
                query = query.Where(f => anios.Contains(f.FechaPeriodo.Year));
                return query.Select(f => f.ObraId).Distinct().Count();
            }
        }

        /// <summary>
        /// Devuelve una página de OBRAS (1 fila lógica por obra) con sus montos del ciclo pivoteados.
        /// </summary>
        public List<FormulacionPivotEF> ListarPivotPaginadoConFiltros(UsuarioEF usuario, int pageIndex, int pageSize, string filtroGeneral,
            List<int> areas, List<int> lineasGestion, List<int> proyectos, List<int> prioridades,
            List<int> obras = null, List<int> empresas = null, List<int> barrios = null)
        {
            var anios = FormulacionCiclo.Anios;
            using (var context = new IVCdbContext())
            {
                // 1) Página de obras ordenadas por descripción (aplica filtros + ventana del ciclo)
                var filtrado = BuildBaseQuery(context, usuario);
                filtrado = ApplyOptionalFilters(filtrado, filtroGeneral, areas, lineasGestion, proyectos, null, prioridades, obras, null, empresas, null, barrios);
                filtrado = filtrado.Where(f => anios.Contains(f.FechaPeriodo.Year));

                var obrasPage = filtrado
                    .Select(f => new { f.ObraId, f.ObraEF.Descripcion })
                    .Distinct()
                    .OrderBy(x => x.Descripcion)
                    .ThenBy(x => x.ObraId)
                    .Skip(pageIndex * pageSize)
                    .Take(pageSize)
                    .ToList();

                var obraIdsPage = obrasPage.Select(x => x.ObraId).ToList();

                if (!obraIdsPage.Any())
                    return new List<FormulacionPivotEF>();

                // 2) Traer las filas (con includes) de las obras de esta página
                var filas = BuildBaseQuery(context, usuario)
                    .Where(f => anios.Contains(f.FechaPeriodo.Year) && obraIdsPage.Contains(f.ObraId))
                    .ToList();

                // 3) Pivotear preservando el orden de la página
                var aniosArr = anios;
                var porObra = filas.GroupBy(f => f.ObraId).ToDictionary(g => g.Key, g => g.ToList());
                return obraIdsPage
                    .Where(id => porObra.ContainsKey(id))
                    .Select(id => BuildPivot(id, porObra[id], aniosArr))
                    .ToList();
            }
        }

        /// <summary>
        /// Lista TODAS las obras del usuario pivoteadas (sin paginar), ordenadas por descripción.
        /// Pensado para exportar a Excel con las mismas columnas que la grilla.
        /// </summary>
        public List<FormulacionPivotEF> ListarPivotPorUsuario(UsuarioEF usuario)
        {
            var anios = FormulacionCiclo.Anios;
            using (var context = new IVCdbContext())
            {
                var filas = BuildBaseQuery(context, usuario)
                    .Where(f => anios.Contains(f.FechaPeriodo.Year))
                    .ToList();

                return filas
                    .GroupBy(f => f.ObraId)
                    .Select(g => BuildPivot(g.Key, g.ToList(), anios))
                    .OrderBy(p => p.ObraEF?.Descripcion)
                    .ToList();
            }
        }

        /// <summary>
        /// Carga el pivot de una obra puntual para edición. Devuelve null si la obra no tiene
        /// ninguna formulación en el ciclo vigente.
        /// </summary>
        public FormulacionPivotEF ObtenerPivotPorObra(int obraId)
        {
            var anios = FormulacionCiclo.Anios;
            using (var context = new IVCdbContext())
            {
                var filas = context.Formulaciones
                    .Include(f => f.ObraEF)
                    .Include(f => f.UnidadMedidaEF)
                    .Include(f => f.PrioridadEF)
                    .Where(f => f.ObraId == obraId && anios.Contains(f.FechaPeriodo.Year))
                    .ToList();

                return filas.Any() ? BuildPivot(obraId, filas, anios) : null;
            }
        }

        /// <summary>
        /// Guarda la formulación de una obra para el ciclo (alta y edición unificadas).
        /// Por cada año: si viene monto, hace upsert de la fila con los campos compartidos;
        /// si el monto viene vacío, elimina la fila de ese año si existía.
        /// </summary>
        public bool GuardarPivot(int obraId, FormulacionEF compartido, decimal?[] montos)
        {
            var anios = FormulacionCiclo.Anios;
            using (var context = new IVCdbContext())
            {
                var existentes = context.Formulaciones
                    .Where(f => f.ObraId == obraId && anios.Contains(f.FechaPeriodo.Year))
                    .ToList();

                for (int i = 0; i < anios.Length; i++)
                {
                    int anio = anios[i];
                    decimal? monto = montos[i];
                    var fila = existentes.FirstOrDefault(f => f.FechaPeriodo.Year == anio);

                    if (!monto.HasValue)
                    {
                        if (fila != null) context.Formulaciones.Remove(fila);
                        continue;
                    }

                    if (fila == null)
                    {
                        fila = new FormulacionEF
                        {
                            ObraId = obraId,
                            FechaPeriodo = FormulacionCiclo.FechaPeriodo(anio)
                        };
                        context.Formulaciones.Add(fila);
                    }

                    fila.Monto = monto;
                    fila.MesBase = compartido.MesBase;
                    fila.Observaciones = compartido.Observaciones;
                    fila.Ppi = compartido.Ppi;
                    fila.Techos = compartido.Techos;
                    fila.UnidadMedidaId = compartido.UnidadMedidaId;
                    fila.ValorMedida = compartido.ValorMedida;
                    fila.PrioridadId = compartido.PrioridadId;
                    fila.BreveDescripcion = compartido.BreveDescripcion;
                    fila.FechaInicio = compartido.FechaInicio;
                    fila.FechaFin = compartido.FechaFin;
                }

                context.SaveChanges();
                return true;
            }
        }

        /// <summary>
        /// Elimina todas las filas de formulación de una obra dentro del ciclo vigente.
        /// </summary>
        public bool EliminarPorObra(int obraId)
        {
            var anios = FormulacionCiclo.Anios;
            using (var context = new IVCdbContext())
            {
                var filas = context.Formulaciones
                    .Where(f => f.ObraId == obraId && anios.Contains(f.FechaPeriodo.Year))
                    .ToList();

                if (!filas.Any()) return false;

                context.Formulaciones.RemoveRange(filas);
                context.SaveChanges();
                return true;
            }
        }

        #endregion

        #region Métodos para Paginación

        /// <summary>
        /// Obtiene el total de registros para un usuario específico con filtros opcionales
        /// </summary>
        public int ContarPorUsuario(UsuarioEF usuario, string filtroGeneral = null)
        {
            using (var context = new IVCdbContext())
            {
                var query = context.Formulaciones
                    .Include(f => f.ObraEF)
                    .Include(f => f.ObraEF.Empresa)
                    .Include(f => f.ObraEF.Contrata)
                    .Include(f => f.ObraEF.Barrio);

                // Filtrar por área si NO es administrador
                if (!usuario.Tipo)
                {
                    var filtroAreaIds = usuario.IvcAreaIds;
                    if (filtroAreaIds != null && filtroAreaIds.Count > 0)
                        query = query.Where(f => f.ObraEF.AreaId.HasValue && filtroAreaIds.Contains(f.ObraEF.AreaId.Value));
                }

                // Aplicar filtro general de búsqueda
                if (!string.IsNullOrEmpty(filtroGeneral))
                {
                    query = query.Where(f =>
                        f.ObraEF.Descripcion.Contains(filtroGeneral) ||
                        f.ObraEF.Empresa.Nombre.Contains(filtroGeneral) ||
                        f.ObraEF.Contrata.Nombre.Contains(filtroGeneral) ||
                        f.ObraEF.Barrio.Nombre.Contains(filtroGeneral) ||
                        (f.Observaciones != null && f.Observaciones.Contains(filtroGeneral)));
                }

                return query.Count();
            }

        }

        /// <summary>
        /// Obtiene una página específica de formulaciones para un usuario con filtros opcionales
        /// </summary>
        public List<FormulacionEF> ListarPorUsuarioPaginado(UsuarioEF usuario, int pageIndex, int pageSize, string filtroGeneral = null)
        {
            using (var context = new IVCdbContext())
            {

                var query = context.Formulaciones
                    .Include(f => f.ObraEF)
                    .Include(f => f.ObraEF.Area)
                    .Include(f => f.ObraEF.Empresa)
                    .Include(f => f.ObraEF.Contrata)
                    .Include(f => f.ObraEF.Barrio)
                    .Include(f => f.ObraEF.Proyecto)
                    .Include(f => f.ObraEF.Proyecto.LineaGestionEF)
                    .Include(f => f.UnidadMedidaEF)
                    .Include(f => f.PrioridadEF);

                // Filtrar por área si NO es administrador
                if (!usuario.Tipo)
                {
                    var filtroAreaIds = usuario.IvcAreaIds;
                    if (filtroAreaIds != null && filtroAreaIds.Count > 0)
                        query = query.Where(f => f.ObraEF.AreaId.HasValue && filtroAreaIds.Contains(f.ObraEF.AreaId.Value));
                }

                // Aplicar filtro general de búsqueda
                if (!string.IsNullOrEmpty(filtroGeneral))
                {
                    query = query.Where(f =>
                        f.ObraEF.Descripcion.Contains(filtroGeneral) ||
                        f.ObraEF.Empresa.Nombre.Contains(filtroGeneral) ||
                        f.ObraEF.Contrata.Nombre.Contains(filtroGeneral) ||
                        f.ObraEF.Barrio.Nombre.Contains(filtroGeneral) ||
                        (f.Observaciones != null && f.Observaciones.Contains(filtroGeneral)));
                }

                // Aplicar paginación
                var resultado = query
                    .OrderBy(f => f.ObraEF.Descripcion) // Orden consistente para paginación
                    .Skip(pageIndex * pageSize)
                    .Take(pageSize)
                    .ToList();

                return resultado;
            }

        }

        /// <summary>
        /// Versión extendida: aplica además filtros discretos por listas de IDs y montos.
        /// Mantiene la firma original separada para no romper código existente.
        /// </summary>
        public int ContarPorUsuarioConFiltros(UsuarioEF usuario, string filtroGeneral,
            List<int> areas, List<int> lineasGestion, List<int> proyectos, List<decimal> montos, List<int> prioridades,
            List<int> obras = null, List<int> anios = null, List<int> empresas = null, List<int> contratas = null, List<int> barrios = null)
        {
            using (var context = new IVCdbContext())
            {
                var query = BuildBaseQuery(context, usuario);

                query = ApplyOptionalFilters(query, filtroGeneral, areas, lineasGestion, proyectos, montos, prioridades, obras, anios, empresas, contratas, barrios);

                return query.Count();
            }
        }

        /// <summary>
        /// Versión extendida paginada con filtros discretos.
        /// </summary>
        public List<FormulacionEF> ListarPorUsuarioPaginadoConFiltros(UsuarioEF usuario, int pageIndex, int pageSize, string filtroGeneral,
            List<int> areas, List<int> lineasGestion, List<int> proyectos, List<decimal> montos, List<int> prioridades,
            List<int> obras = null, List<int> anios = null, List<int> empresas = null, List<int> contratas = null, List<int> barrios = null)
        {
            using (var context = new IVCdbContext())
            {
                var query = BuildBaseQuery(context, usuario);
                query = ApplyOptionalFilters(query, filtroGeneral, areas, lineasGestion, proyectos, montos, prioridades, obras, anios, empresas, contratas, barrios);

                return query
                    .OrderBy(f => f.ObraEF.Descripcion)
                    .Skip(pageIndex * pageSize)
                    .Take(pageSize)
                    .ToList();
            }
        }

        /// <summary>
        /// Construye el query base con includes y restricción por usuario.
        /// </summary>
        private IQueryable<FormulacionEF> BuildBaseQuery(IVCdbContext context, UsuarioEF usuario)
        {
            var query = context.Formulaciones
                    .Include(f => f.ObraEF)
                    .Include(f => f.ObraEF.Area)
                    .Include(f => f.ObraEF.Empresa)
                    .Include(f => f.ObraEF.Contrata)
                    .Include(f => f.ObraEF.Barrio)
                    .Include(f => f.ObraEF.Proyecto)
                    .Include(f => f.ObraEF.Proyecto.LineaGestionEF)
                    .Include(f => f.UnidadMedidaEF)
                    .Include(f => f.PrioridadEF)
                    .AsQueryable();

            if (!usuario.Tipo)
            {
                var filtroAreaIds = usuario.IvcAreaIds;
                if (filtroAreaIds != null && filtroAreaIds.Count > 0)
                    query = query.Where(f => f.ObraEF.AreaId.HasValue && filtroAreaIds.Contains(f.ObraEF.AreaId.Value));
            }

            return query;
        }

        /// <summary>
        /// Aplica filtros opcionales sobre el query.
        /// </summary>
        private IQueryable<FormulacionEF> ApplyOptionalFilters(IQueryable<FormulacionEF> query, string filtroGeneral,
            List<int> areas, List<int> lineasGestion, List<int> proyectos, List<decimal> montos, List<int> prioridades,
            List<int> obras = null, List<int> anios = null, List<int> empresas = null, List<int> contratas = null, List<int> barrios = null)
        {
            if (!string.IsNullOrWhiteSpace(filtroGeneral))
            {
                query = query.Where(f =>
                    f.ObraEF.Descripcion.Contains(filtroGeneral) ||
                    f.ObraEF.Empresa.Nombre.Contains(filtroGeneral) ||
                    f.ObraEF.Contrata.Nombre.Contains(filtroGeneral) ||
                    f.ObraEF.Barrio.Nombre.Contains(filtroGeneral) ||
                    (f.Observaciones != null && f.Observaciones.Contains(filtroGeneral)) ||
                    (f.PrioridadEF != null && f.PrioridadEF.Nombre.Contains(filtroGeneral)) ||
                    (f.ObraEF.Proyecto != null && f.ObraEF.Proyecto.Nombre.Contains(filtroGeneral)) ||
                    (f.ObraEF.Proyecto.LineaGestionEF != null && f.ObraEF.Proyecto.LineaGestionEF.Nombre.Contains(filtroGeneral))
                );
            }

            if (areas != null && areas.Any())
                query = query.Where(f => f.ObraEF.Area != null && areas.Contains(f.ObraEF.Area.Id));
            if (lineasGestion != null && lineasGestion.Any())
                query = query.Where(f => f.ObraEF.Proyecto.LineaGestionEF != null && lineasGestion.Contains(f.ObraEF.Proyecto.LineaGestionEF.Id));
            if (proyectos != null && proyectos.Any())
                query = query.Where(f => f.ObraEF.Proyecto != null && proyectos.Contains(f.ObraEF.Proyecto.Id));
            if (montos != null && montos.Any())
                query = query.Where(f => f.Monto.HasValue && montos.Contains(f.Monto.Value));
            if (prioridades != null && prioridades.Any())
                query = query.Where(f => f.PrioridadEF != null && prioridades.Contains(f.PrioridadEF.Id));
            if (obras != null && obras.Any())
                query = query.Where(f => obras.Contains(f.ObraId));
            if (anios != null && anios.Any())
                query = query.Where(f => anios.Contains(f.FechaPeriodo.Year));
            if (empresas != null && empresas.Any())
                query = query.Where(f => f.ObraEF.EmpresaId.HasValue && empresas.Contains(f.ObraEF.EmpresaId.Value));
            if (contratas != null && contratas.Any())
                query = query.Where(f => f.ObraEF.ContrataId.HasValue && contratas.Contains(f.ObraEF.ContrataId.Value));
            if (barrios != null && barrios.Any())
                query = query.Where(f => f.ObraEF.BarrioId.HasValue && barrios.Contains(f.ObraEF.BarrioId.Value));

            return query;
        }

        #endregion
    }
}