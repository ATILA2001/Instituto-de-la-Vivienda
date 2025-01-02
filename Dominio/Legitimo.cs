using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio
{
    public class Legitimo
    {
        public int Id { get; set; } // Corresponde a la columna ID
        public Obra Obra { get; set; } // Corresponde a la columna OBRA (clave foránea)
        public string CodigoAutorizante { get; set; } // Corresponde a la columna CODIGO_AUTORIZANTE
        public string Expediente { get; set; } // Corresponde a la columna EXPEDIENTE
        public DateTime? InicioEjecucion { get; set; } // Corresponde a la columna INICIO_EJECUCION (admite valores nulos)
        public DateTime? FinEjecucion { get; set; } // Corresponde a la columna FIN_EJECUCION (admite valores nulos)
        public decimal? Certificado { get; set; } // Corresponde a la columna CERTIFICADO (admite valores nulos)
        public DateTime? MesAprobacion { get; set; }
    }
}
