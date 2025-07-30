using Dominio;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Negocio
{
    public class EstadoAutorizanteNegocioEF
    {
        /// <summary>
        /// Lista todos los estados de autorizante
        /// </summary>
        public List<EstadoAutorizanteEF> Listar()
        {
            try
            {
                using (var context = new IVCdbContext())
                {
                    return context.EstadosAutorizante.AsNoTracking()
                        .OrderBy(e => e.Nombre)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error al obtener los estados de autorizante", ex);
            }
        }

        /// <summary>
        /// Obtiene un estado por su ID
        /// </summary>
        public EstadoAutorizanteEF ObtenerPorId(int id)
        {
            try
            {
                using (var context = new IVCdbContext())
                {
                    return context.EstadosAutorizante.AsNoTracking()
                        .FirstOrDefault(e => e.Id == id);
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error al obtener el estado con ID {id}", ex);
            }
        }

        /// <summary>
        /// Agrega un nuevo estado de autorizante
        /// </summary>
        public bool Agregar(EstadoAutorizanteEF estado)
        {
            try
            {
                using (var context = new IVCdbContext())
                {
                    context.EstadosAutorizante.Add(estado);
                    return context.SaveChanges() > 0;
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error al agregar el estado de autorizante", ex);
            }
        }

        /// <summary>
        /// Modifica un estado existente
        /// </summary>
        public bool Modificar(EstadoAutorizanteEF estado)
        {
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
                throw new ApplicationException("Error al modificar el estado de autorizante", ex);
            }
        }

        /// <summary>
        /// Elimina un estado por su ID
        /// </summary>
        public bool Eliminar(int id)
        {
            try
            {
                using (var context = new IVCdbContext())
                {
                    var estado = context.EstadosAutorizante.Find(id);
                    if (estado != null)
                    {
                        context.EstadosAutorizante.Remove(estado);
                        return context.SaveChanges() > 0;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error al eliminar el estado de autorizante", ex);
            }
        }
    }
}
