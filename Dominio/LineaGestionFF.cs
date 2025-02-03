using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio
{
    public class LineaGestionFF
    {
        public int Id { get; set; }
        public LineaGestion LineaGestion { get; set; }
        public string Nombre { get; set; }
        public string Fuente { get; set; }

    }
}
