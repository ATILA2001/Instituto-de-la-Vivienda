using Dominio;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Negocio
{
    public class ObraNegocioEF
    {
        /// <summary>
        /// Lista todas las obras para dropdown lists
        /// </summary>
        public List<ObraEF> ListarParaDDL()
        {
            try
            {
                using (var context = new IVCdbContext())
                {
                    return context.Obras.AsNoTracking()
                        .OrderBy(o => o.Descripcion)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error al obtener las obras", ex);
            }
        }

        /// <summary>
        /// Lista obras para dropdown excluyendo las que ya tienen Formulación.
        /// Permite incluir explícitamente una obra (por ejemplo, al editar) y filtrar por área del usuario no admin.
        /// </summary>
        /// <param name="includeObraId">ID de obra a incluir aunque ya esté en Formulación (útil al editar).</param>
        /// <param name="usuario">Usuario actual; si no es admin y tiene área, filtra por área.</param>
        public List<ObraEF> ListarParaDDLNoEnFormulacion(int? includeObraId = null, UsuarioEF usuario = null)
        {
            try
            {
                using (var context = new IVCdbContext())
                {
                    var query = context.Obras.AsNoTracking().Where(o =>
                        !context.Formulaciones.Any(f => f.ObraId == o.Id) ||
                        (includeObraId.HasValue && o.Id == includeObraId.Value));

                    // Si el usuario no es admin, filtrar por su área (si la tiene)
                    if (usuario != null && !usuario.Tipo)
                    {
                        if (usuario.AreaId.HasValue)
                            query = query.Where(o => o.AreaId == usuario.AreaId.Value);
                        else if (usuario.Area != null)
                            query = query.Where(o => o.AreaId == usuario.Area.Id);
                    }

                    return query
                        .OrderBy(o => o.Descripcion)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error al obtener las obras disponibles para Formulación", ex);
            }
        }

        /// <summary>
        /// Lista todas las obras con información completa
        /// </summary>
        public List<ObraEF> Listar()
        {
            try
            {
                using (var context = new IVCdbContext())
                {
                    return context.Obras.AsNoTracking()
                        .Include(o => o.Empresa)
                        .Include(o => o.Area)
                        .Include(o => o.Barrio)
                        .Include(o => o.Contrata)
                        .OrderBy(o => o.Descripcion)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error al obtener las obras", ex);
            }
        }

        /// <summary>
        /// Lista obras filtradas por área del usuario
        /// </summary>
        public List<ObraEF> ListarPorArea(int areaId)
        {
            try
            {
                using (var context = new IVCdbContext())
                {
                    return context.Obras.AsNoTracking()
                        .Where(o => o.AreaId == areaId)
                        .Include(o => o.Empresa)
                        .Include(o => o.Area)
                        .Include(o => o.Barrio)
                        .Include(o => o.Contrata)
                        .OrderBy(o => o.Descripcion)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error al obtener las obras por área", ex);
            }
        }

        /// <summary>
        /// Obtiene una obra por su ID
        /// </summary>
        public ObraEF ObtenerPorId(int id)
        {
            try
            {
                using (var context = new IVCdbContext())
                {
                    return context.Obras.AsNoTracking()
                        .Include(o => o.Empresa)
                        .Include(o => o.Area)
                        .Include(o => o.Barrio)
                        .Include(o => o.Contrata)
                        .FirstOrDefault(o => o.Id == id);
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error al obtener la obra con ID {id}", ex);
            }
        }

        /// <summary>
        /// Agrega una nueva obra
        /// </summary>
        public bool Agregar(ObraEF obra)
        {
            try
            {
                using (var context = new IVCdbContext())
                {
                    context.Obras.Add(obra);
                    return context.SaveChanges() > 0;
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error al agregar la obra", ex);
            }
        }

        /// <summary>
        /// Modifica una obra existente
        /// </summary>
        public bool Modificar(ObraEF obra)
        {
            try
            {
                using (var context = new IVCdbContext())
                {
                    context.Entry(obra).State = EntityState.Modified;
                    return context.SaveChanges() > 0;
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error al modificar la obra", ex);
            }
        }

        /// <summary>
        /// Elimina una obra por su ID
        /// </summary>
        public bool Eliminar(int id)
        {
            try
            {
                using (var context = new IVCdbContext())
                {
                    var obra = context.Obras.Find(id);
                    if (obra != null)
                    {
                        context.Obras.Remove(obra);
                        return context.SaveChanges() > 0;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error al eliminar la obra", ex);
            }
        }
    }
}
