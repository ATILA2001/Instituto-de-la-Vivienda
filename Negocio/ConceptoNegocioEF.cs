using Dominio;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Negocio
{
    public class ConceptoNegocioEF
    {
        /// <summary>
        /// Lista todos los conceptos
        /// </summary>
        public List<ConceptoEF> Listar()
        {
            try
            {
                using (var context = new IVCdbContext())
                {
                    return context.Conceptos.AsNoTracking()
                        .OrderBy(c => c.Nombre)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error al obtener los conceptos", ex);
            }
        }

        /// <summary>
        /// Obtiene un concepto por su ID
        /// </summary>
        public ConceptoEF ObtenerPorId(int id)
        {
            try
            {
                using (var context = new IVCdbContext())
                {
                    return context.Conceptos.AsNoTracking()
                        .FirstOrDefault(c => c.Id == id);
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error al obtener el concepto con ID {id}", ex);
            }
        }

        /// <summary>
        /// Agrega un nuevo concepto
        /// </summary>
        public bool Agregar(ConceptoEF concepto)
        {
            try
            {
                using (var context = new IVCdbContext())
                {
                    context.Conceptos.Add(concepto);
                    return context.SaveChanges() > 0;
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error al agregar el concepto", ex);
            }
        }

        /// <summary>
        /// Modifica un concepto existente
        /// </summary>
        public bool Modificar(ConceptoEF concepto)
        {
            try
            {
                using (var context = new IVCdbContext())
                {
                    context.Entry(concepto).State = EntityState.Modified;
                    return context.SaveChanges() > 0;
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error al modificar el concepto", ex);
            }
        }

        /// <summary>
        /// Elimina un concepto por su ID
        /// </summary>
        public bool Eliminar(int id)
        {
            try
            {
                using (var context = new IVCdbContext())
                {
                    var concepto = context.Conceptos.Find(id);
                    if (concepto != null)
                    {
                        context.Conceptos.Remove(concepto);
                        return context.SaveChanges() > 0;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error al eliminar el concepto", ex);
            }
        }
    }
}
