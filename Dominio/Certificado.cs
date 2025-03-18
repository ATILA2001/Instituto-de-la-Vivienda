using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio
{
    public class Certificado
    {
        public int Id { get; set; } 
        public string Porcentaje { get; set; }

        public Autorizante Autorizante { get; set; }

        public string ExpedientePago { get; set; } 

        public TipoPago Tipo { get; set; }

        public decimal MontoTotal { get; set; } 
        public decimal? Sigaf { get; set; }
        public DateTime? MesAprobacion { get; set; }
        public DateTime? FechaSade { get; set; }
        public string BuzonSade {  get; set; }
        public string Empresa { get; set; }
        public string Estado { get; set; }

        public Certificado() { }
    }
}
