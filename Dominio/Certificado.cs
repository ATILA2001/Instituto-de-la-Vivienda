using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio
{
    public class Certificado
    {
        public int Id { get; set; } // IDENTITY (1, 1) PRIMARY KEY
        public decimal Porcentaje { get; set; }

        public Autorizante Autorizante { get; set; }

        public string ExpedientePago { get; set; } // EXPEDIENTE_PAGO VARCHAR(50)

        public TipoPago Tipo { get; set; } // TIPO_PAGO INT

        public decimal MontoTotal { get; set; } // MONTO_TOTAL DECIMAL(15, 2)
        public decimal? Sigaf { get; set; }
        public DateTime? MesAprobacion { get; set; }
        public DateTime? FechaSade { get; set; }
        public string BuzonSade {  get; set; }
        public string Empresa { get; set; }
        public Certificado() { }
    }
}
