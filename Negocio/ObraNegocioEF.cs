// No markdown fences found in the file. No changes made.
using Dominio;
using Dominio.DTO;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Negocio
{
    /// <summary>
    /// Operaciones de negocio para Obras.
    /// Delega cálculos complejos a CalculoObraNegocioEF para mantener la clase reducida.
    /// </summary>
    public class ObraNegocioEF
    {
        /// <summary>
        /// Lista obras proyectadas a DTO para el grid, delegando cálculos financieros.
        /// Se respeta el usuario: si es administrador devuelve todas, si no filtra por el area del usuario.
        /// </summary>
        public List<ObraEF> ListarParaDDL(UsuarioEF usuario = null)
        {
            try
            {
                using (var context = new IVCdbContext())
                {

                    var query = context.Obras.AsNoTracking()
                         .OrderBy(o => o.Descripcion)
                         .ToList();
                    if (usuario != null && !usuario.Tipo)
                    {
                        if (usuario.AreaId.HasValue)
                            query = query.Where(o => o.AreaId == usuario.AreaId.Value).ToList();
                        else if (usuario.Area != null)
                            query = query.Where(o => o.AreaId == usuario.Area.Id).ToList();
                    }

                    return query
                        .OrderBy(o => o.Descripcion)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error al obtener las obras", ex);
            }
        }

        public List<ObraEF> ListarParaDDLNoEnFormulacion(int? includeObraId = null, UsuarioEF usuario = null)
        {
            using (var context = new IVCdbContext())
            {
                var query = context.Obras.AsNoTracking().Where(o => !context.Formulaciones.Any(f => f.ObraId == o.Id) || (includeObraId.HasValue && o.Id == includeObraId.Value));
                if (usuario != null && !usuario.Tipo)
                {
                    if (usuario.AreaId.HasValue) query = query.Where(o => o.AreaId == usuario.AreaId.Value);
                    else if (usuario.Area != null) query = query.Where(o => o.AreaId == usuario.Area.Id);
                }
                return query.OrderBy(o => o.Descripcion).ToList();
            }
        }

        public List<ObraDTO> ListarTodo()
        {
            try
            {
                using (var context = new IVCdbContext())
                {
                    var obras = context.Obras.AsNoTracking()
                        .Include(o => o.Empresa)
                        .Include(o => o.Area)
                        .Include(o => o.Barrio)
                        .Include(o => o.Contrata)
                        .Include("Proyecto")
                        .Include("Proyecto.LineaGestionEF")
                        .OrderBy(o => o.Descripcion)
                        .ToList();

                    // Calcular finanzas y construir DTOs
                    var calc = new CalculoObraNegocioEF();
                    var finanzas = calc.ObtenerFinanzasPorObras(obras.Select(o => o.Id).ToList());
                    return calc.ConstruirObraDTOs(obras, finanzas);
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error al obtener las obras DTO", ex);
            }
        }

        public List<ObraDTO> ListarPorArea(int areaId)
        {
            try
            {
                using (var context = new IVCdbContext())
                {
                    var obras = context.Obras.AsNoTracking()
                        .Where(o => o.AreaId == areaId)
                        .Include(o => o.Empresa)
                        .Include(o => o.Area)
                        .Include(o => o.Barrio)
                        .Include(o => o.Contrata)
                        .Include("Proyecto")
                        .Include("Proyecto.LineaGestionEF")
                        .OrderBy(o => o.Descripcion)
                        .ToList();

                    var calc = new CalculoObraNegocioEF();
                    var finanzas = calc.ObtenerFinanzasPorObras(obras.Select(o => o.Id).ToList());
                    return calc.ConstruirObraDTOs(obras, finanzas);
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error al obtener las obras por área (DTO)", ex);
            }
        }

        public ObraEF ObtenerPorId(int id)
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

        // Helpers para filtros
        public List<AreaEF> ListarAreas()
        {
            using (var context = new IVCdbContext()) return context.Areas.AsNoTracking().OrderBy(a => a.Nombre).ToList();
        }

        public List<EmpresaEF> ListarEmpresas()
        {
            using (var context = new IVCdbContext()) return context.Empresas.AsNoTracking().OrderBy(e => e.Nombre).ToList();
        }

        public List<BarrioEF> ListarBarrios()
        {
            using (var context = new IVCdbContext()) return context.Barrios.AsNoTracking().OrderBy(b => b.Nombre).ToList();
        }
    }
}
