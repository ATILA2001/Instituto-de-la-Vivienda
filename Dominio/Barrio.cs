using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio
{
    public class Barrio
    {
        public int Id { get; set; }
        public String Nombre { get; set; }

        public Barrio() { }

        public override string ToString()
        {
            return "Barrio: " + Nombre;
        }
    }
}
