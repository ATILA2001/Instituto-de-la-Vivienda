using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio
{
    [Serializable]
    public class Area
    {
        public int Id {  get; set; }
        public String Nombre { get; set; }

        public Area() { }

        public override string ToString()
        {
            return  Nombre ;
        }
    }
}
