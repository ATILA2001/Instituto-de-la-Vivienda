using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio
{
    public class Legitimo
    {
        public int Id { get; set; } 
        public Obra Obra { get; set; } 
        public string CodigoAutorizante { get; set; } 
        public string Expediente { get; set; } 
        public DateTime? InicioEjecucion { get; set; } 
        public DateTime? FinEjecucion { get; set; } 
        public decimal? Certificado { get; set; }
        public DateTime? MesAprobacion { get; set; }
        public decimal? Sigaf { get; set; }
        public DateTime? FechaSade { get; set; }
        public string BuzonSade { get; set; }
        public string Linea { get; set; }
        public string Empresa { get; set; }
    }
}
