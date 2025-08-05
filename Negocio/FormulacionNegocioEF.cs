using Dominio;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Negocio
{
    public class FormulacionNegocioEF
    {
        private readonly IVCdbContext _context;

        public FormulacionNegocioEF(IVCdbContext context)
        {
            _context = context;
        }

        public List<FormulacionEF> ListarPorUsuario(UsuarioEF usuario)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[FormulacionNegocioEF] Iniciando ListarPorUsuario para usuario: {usuario?.Nombre}");
                System.Diagnostics.Debug.WriteLine($"[FormulacionNegocioEF] Tipo (Es Admin): {usuario?.Tipo}");
                System.Diagnostics.Debug.WriteLine($"[FormulacionNegocioEF] AreaId del usuario: {usuario?.AreaId}");

                var query = _context.Formulaciones
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

                System.Diagnostics.Debug.WriteLine($"[FormulacionNegocioEF] Query LINQ construida, ejecutando...");

                var resultado = query.ToList();
                
                System.Diagnostics.Debug.WriteLine($"[FormulacionNegocioEF] Query ejecutada exitosamente. Registros encontrados: {resultado.Count}");
                
                return resultado;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FormulacionNegocioEF] ERROR en ListarPorUsuario: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[FormulacionNegocioEF] StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[FormulacionNegocioEF] InnerException: {ex.InnerException.Message}");
                }
                throw;
            }
        }

        public void Agregar(FormulacionEF formulacion)
        {
            _context.Formulaciones.Add(formulacion);
            _context.SaveChanges();
        }

        public void Modificar(FormulacionEF formulacion)
        {
            _context.Entry(formulacion).State = EntityState.Modified;
            _context.SaveChanges();
        }

        public bool Eliminar(int id)
        {
            var entity = _context.Formulaciones.Find(id);
            if (entity == null) return false;
            _context.Formulaciones.Remove(entity);
            _context.SaveChanges();
            return true;
        }

        #region Métodos para Paginación

        /// <summary>
        /// Obtiene el total de registros para un usuario específico con filtros opcionales
        /// </summary>
        public int ContarPorUsuario(UsuarioEF usuario, string filtroGeneral = null)
        {
            try
            {
                var query = _context.Formulaciones
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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FormulacionNegocioEF] ERROR en ContarPorUsuario: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Obtiene una página específica de formulaciones para un usuario con filtros opcionales
        /// </summary>
        public List<FormulacionEF> ListarPorUsuarioPaginado(UsuarioEF usuario, int pageIndex, int pageSize,
            string filtroGeneral = null)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[FormulacionNegocioEF] ListarPorUsuarioPaginado - Página: {pageIndex}, Tamaño: {pageSize}");

                var query = _context.Formulaciones
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

                System.Diagnostics.Debug.WriteLine($"[FormulacionNegocioEF] Página obtenida exitosamente. Registros: {resultado.Count}");
                
                return resultado;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FormulacionNegocioEF] ERROR en ListarPorUsuarioPaginado: {ex.Message}");
                throw;
            }
        }

        #endregion
    }
}