using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio
{
    public class Obra
    {
        public int Id { get; set; }
        public Area Area { get; set; }
        public Empresa Empresa { get; set; }
        public int? Numero { get; set; } // Puede ser null
        public Contrata Contrata { get; set; } // Puede ser null
        public int? Año { get; set; } // Puede ser null
        public int? Etapa { get; set; } // Puede ser null
        public int? ObraNumero { get; set; } // Puede ser null
        public Barrio Barrio { get; set; } // Puede ser null
        public string Descripcion { get; set; } // Puede ser null
        public Obra() { }

    }
}
