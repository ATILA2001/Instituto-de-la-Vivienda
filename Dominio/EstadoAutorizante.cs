using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio
{
    [Serializable]
    public class EstadoAutorizante
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public EstadoAutorizante() { }
        public override string ToString()
        {
            return Nombre;
        }
    }
}
