using Dominio;
using System.Collections.Generic;
using System.Linq;

namespace Negocio
{
    public class PrioridadNegocioEF
    {
        public List<PrioridadesEF> Listar()
        {
            using (var context = new IVCdbContext())
            {
                return context.Set<PrioridadesEF>()
                    .OrderBy(p => p.Nombre)
                    .ToList();
            }
        }

        public PrioridadesEF ObtenerPorId(int id)
        {
            using (var context = new IVCdbContext())
            {
                return context.Set<PrioridadesEF>().Find(id);
            }
        }
    }
}