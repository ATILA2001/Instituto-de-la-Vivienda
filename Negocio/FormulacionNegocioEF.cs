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
                    // Usuario no administrador: filtrar por su área
                    if (usuario.AreaId.HasValue)
                    {
                        query = query.Where(f => f.ObraEF.AreaId == usuario.AreaId);
                        System.Diagnostics.Debug.WriteLine($"[FormulacionNegocioEF] Usuario no admin - Filtrando por AreaId: {usuario.AreaId}");
                    }
                    else if (usuario.Area != null)
                    {
                        query = query.Where(f => f.ObraEF.AreaId == usuario.Area.Id);
                        System.Diagnostics.Debug.WriteLine($"[FormulacionNegocioEF] Usuario no admin - Filtrando por Area.Id: {usuario.Area.Id}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[FormulacionNegocioEF] Usuario administrador - Mostrando todas las formulaciones");
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
                    if (usuario.AreaId.HasValue)
                        query = query.Where(f => f.ObraEF.AreaId == usuario.AreaId);
                    else if (usuario.Area != null)
                        query = query.Where(f => f.ObraEF.AreaId == usuario.Area.Id);
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
                    if (usuario.AreaId.HasValue)
                        query = query.Where(f => f.ObraEF.AreaId == usuario.AreaId);
                    else if (usuario.Area != null)
                        query = query.Where(f => f.ObraEF.AreaId == usuario.Area.Id);
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
            List<int> areas, List<int> lineasGestion, List<int> proyectos, List<decimal> montos26, List<int> prioridades)
        {
            using (var context = new IVCdbContext())
            {
                var query = BuildBaseQuery(context, usuario);

                query = ApplyOptionalFilters(query, filtroGeneral, areas, lineasGestion, proyectos, montos26, prioridades);

                return query.Count();
            }
        }

        /// <summary>
        /// Versión extendida paginada con filtros discretos.
        /// </summary>
        public List<FormulacionEF> ListarPorUsuarioPaginadoConFiltros(UsuarioEF usuario, int pageIndex, int pageSize, string filtroGeneral,
            List<int> areas, List<int> lineasGestion, List<int> proyectos, List<decimal> montos26, List<int> prioridades)
        {
            using (var context = new IVCdbContext())
            {
                var query = BuildBaseQuery(context, usuario);
                query = ApplyOptionalFilters(query, filtroGeneral, areas, lineasGestion, proyectos, montos26, prioridades);

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
                if (usuario.AreaId.HasValue)
                    query = query.Where(f => f.ObraEF.AreaId == usuario.AreaId);
                else if (usuario.Area != null)
                    query = query.Where(f => f.ObraEF.AreaId == usuario.Area.Id);
            }

            return query;
        }

        /// <summary>
        /// Aplica filtros opcionales sobre el query.
        /// </summary>
        private IQueryable<FormulacionEF> ApplyOptionalFilters(IQueryable<FormulacionEF> query, string filtroGeneral,
            List<int> areas, List<int> lineasGestion, List<int> proyectos, List<decimal> montos26, List<int> prioridades)
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
            if (montos26 != null && montos26.Any())
                query = query.Where(f => f.Monto_26.HasValue && montos26.Contains(f.Monto_26.Value));
            if (prioridades != null && prioridades.Any())
                query = query.Where(f => f.PrioridadEF != null && prioridades.Contains(f.PrioridadEF.Id));

            return query;
        }

        #endregion
    }
}