using Dominio;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Negocio
{
    public class CertificadoNegocioEF
    {
        /// <summary>
        /// Obtiene una lista de todos los certificados con sus relaciones cargadas.
        /// </summary>
        public IQueryable<CertificadoEF> Listar()
        {
            using (var context = new IVCdbContext())
            {
                return context.Certificados
                    .Include(c => c.Autorizante.Obra.Area)
                    .Include(c => c.Autorizante.Obra.Contrata)
                    .Include(c => c.Autorizante.Obra.Barrio)
                    .Include(c => c.Autorizante.Obra.Empresa)
                    .Include(c => c.Autorizante.Obra.Proyecto.LineaGestionEF)
                    .Include(c => c.TipoPago)
                    .AsNoTracking();
            }
        }

        /// <summary>
        /// Agrega un nuevo certificado a la base de datos.
        /// </summary>
        public bool Agregar(CertificadoEF nuevoCertificado)
        {
            using (var context = new IVCdbContext())
            {
                context.Certificados.Add(nuevoCertificado);
                return context.SaveChanges() > 0;
            }
        }

        /// <summary>
        /// Obtiene un certificado específico por su ID.
        /// Esencial para el patrón de "recuperar y modificar".
        /// </summary>
        /// <param name="id">El ID del certificado a buscar.</param>
        /// <returns>La entidad CertificadoEF o null si no se encuentra.</returns>
        public CertificadoEF ObtenerPorId(int id)
        {
            using (var context = new IVCdbContext())
            {
                // Find es la forma más eficiente de obtener una entidad por su clave primaria.
                return context.Certificados.Find(id);
            }
        }


        /// <summary>
        /// Modifica un certificado existente.
        /// </summary>
        public bool Modificar(CertificadoEF certificadoModificado)
        {
            using (var context = new IVCdbContext())
            {
                context.Entry(certificadoModificado).State = EntityState.Modified;
                return context.SaveChanges() > 0;
            }
        }

        /// <summary>
        /// Elimina un certificado por su ID.
        /// </summary>
        public bool Eliminar(int id)
        {
            using (var context = new IVCdbContext())
            {
                var certificado = context.Certificados.Find(id);
                if (certificado != null)
                {
                    context.Certificados.Remove(certificado);
                    return context.SaveChanges() > 0;
                }
                return false;
            }
        }
    }
}
