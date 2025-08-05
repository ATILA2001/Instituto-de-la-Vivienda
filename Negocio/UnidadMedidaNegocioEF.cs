using Dominio;
using System.Collections.Generic;
using System.Linq;

namespace Negocio
{
    public class UnidadMedidaNegocioEF
    {
        public List<UnidadesMedidaEF> Listar()
        {
            using (var context = new IVCdbContext())
            {
                return context.Set<UnidadesMedidaEF>()
                    .OrderBy(u => u.Nombre)
                    .ToList();
            }
        }

        public UnidadesMedidaEF ObtenerPorId(int id)
        {
            using (var context = new IVCdbContext())
            {
                return context.Set<UnidadesMedidaEF>().Find(id);
            }
        }
    }
}