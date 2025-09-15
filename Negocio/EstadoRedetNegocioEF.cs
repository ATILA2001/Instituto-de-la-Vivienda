using Dominio;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Data;

namespace Negocio
{
    public class EstadoRedetNegocioEF
    {
        /// <summary>
        /// Devuelve los estados en un List<EstadoRedetEF> para uso en DropDownList (Columnas: ID, NOMBRE)
        /// </summary>
        public List<EstadoRedetEF> ListarParaDDL()
        {
            try
            {
                using (var context = new IVCdbContext())
                {
                    return context.EstadosRedet.AsNoTracking()
                        .OrderBy(e => e.Id)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error al obtener los estados de redeterminación para DDL", ex);
            }
        }


        public EstadoRedetEF ObtenerPorId(int id)
        {
            try
            {
                using (var context = new IVCdbContext())
                {
                    return context.EstadosRedet.AsNoTracking()
                        .FirstOrDefault(e => e.Id == id);
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error al obtener el estado de redeterminación con ID {id}", ex);
            }
        }


        public bool Agregar(EstadoRedetEF estado)
        {
            if (estado == null) throw new ArgumentNullException(nameof(estado));
            try
            {
                using (var context = new IVCdbContext())
                {
                    context.EstadosRedet.Add(estado);
                    return context.SaveChanges() > 0;
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error al agregar el estado de redeterminación", ex);
            }
        }

        public bool Modificar(EstadoRedetEF estado)
        {
            if (estado == null) throw new ArgumentNullException(nameof(estado));
            try
            {
                using (var context = new IVCdbContext())
                {
                    context.Entry(estado).State = EntityState.Modified;
                    return context.SaveChanges() > 0;
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error al modificar el estado de redeterminación", ex);
            }
        }

        public bool Eliminar(int id)
        {
            try
            {
                using (var context = new IVCdbContext())
                {
                    var estado = context.EstadosRedet.Find(id);
                    if (estado != null)
                    {
                        context.EstadosRedet.Remove(estado);
                        return context.SaveChanges() > 0;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error al eliminar el estado de redeterminación", ex);
            }
        }
    }
}
