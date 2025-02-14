using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio
{
    public class EstadoRedet
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public EstadoRedet() { }
        public override string ToString()
        {
            return Nombre;
        }
    }
}
