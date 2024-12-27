using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio
{
    [Serializable]
    public class Autorizante
    {
        public int Id { get; set; } // ID (clave primaria) de tipo INT
        public Obra Obra { get; set; } // Obra (clave foránea) de tipo INT
        public string CodigoAutorizante { get; set; } // Codigo del autorizante de tipo VARCHAR(50)
        public string Detalle { get; set; } // Detalle de tipo VARCHAR(255)
        public string Concepto { get; set; } // Concepto de tipo VARCHAR(255)
        public EstadoAutorizante Estado { get; set; } // Estado (clave foránea) de tipo INT
        public string Expediente { get; set; } // Expediente de tipo VARCHAR(50)
        public decimal MontoAutorizado { get; set; } // Monto autorizado de tipo DECIMAL(15, 2)
        public bool AutorizacionGG { get; set; } // Autorización GG (BIT) de tipo bool
        public DateTime? Fecha { get; set; }

        public Autorizante() { }

    }
}
