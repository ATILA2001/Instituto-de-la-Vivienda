using Dominio;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negocio
{
    public class AutorizanteNegocioEF
    {
        private readonly IVCdbContext _context;

        public AutorizanteNegocioEF()
        {
            _context = new IVCdbContext();
        }

        public List<AutorizanteEF> ListarParaDDL()
        {
            return _context.Autorizantes.AsNoTracking().ToList();
        }
    }
}
