using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio
{
    public class Usuario
    {
        public int Id { get; set; }
        public String Nombre { get; set; }

        public String Correo { get; set; }

        public String Contrasenia { get; set; }
        public bool Tipo {  get; set; }

        public bool Estado { get; set; }

        public Area Area { get; set; }

        public Usuario() { }

        public Usuario( string correo, string contrasenia)
        {
            Correo = correo;
            Contrasenia = contrasenia;
        }
    }
}
