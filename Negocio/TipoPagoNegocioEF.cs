using Dominio;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negocio
{
    public class TipoPagoNegocioEF
    {
        private readonly IVCdbContext _context;

        public TipoPagoNegocioEF()
        {
            _context = new IVCdbContext();
        }

        public List<TipoPagoEF> Listar()
        {
            return _context.TiposPago.AsNoTracking().ToList();
        }
    }
}
