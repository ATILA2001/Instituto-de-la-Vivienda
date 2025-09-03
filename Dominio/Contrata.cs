using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio
{
    [Serializable]
    public class Contrata
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public Contrata() { }
        public override string ToString()
        {
            return Nombre;
        }
    }
}
