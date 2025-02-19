using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio
{
    public class Redeterminacion
    {
        public int Id { get; set; }
        public string Obra { get; set; } 
        public Autorizante Autorizante { get; set; }
        public string Expediente { get; set; }
        public DateTime? Salto { get; set; }
        public int? Nro { get; set; }
        public string Tipo { get; set; }
        public EstadoRedet Etapa { get; set; }
        public string Observaciones { get; set; }
        public string CodigoRedet { get; set; } 
        public decimal? Porcentaje { get; set; }

        public Redeterminacion() { }
    }
}
