using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio
{
    public class Empresa
    {
        public int Id { get; set; }
        public String Nombre { get; set; }

        public Empresa() { }

        public override string ToString()
        {
            return "Empresa: " + Nombre;
        }
    }
}
