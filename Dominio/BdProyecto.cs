using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio
{
    public class BdProyecto
    {
        public int Id { get; set; }
        public Obra Obra { get; set; }

        public string Proyecto { get; set; }
        public string SubProyecto { get; set; }
        public LineaGestion LineaGestion { get; set; }
        public decimal AutorizadoInicial { get; set; }
        public decimal AutorizadoNuevo { get; set; }
    }
}
