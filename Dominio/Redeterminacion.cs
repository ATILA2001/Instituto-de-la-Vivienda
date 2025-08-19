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
        public Obra Obra { get; set; }
        public Autorizante Autorizante { get; set; }
        public string Expediente { get; set; }
        public DateTime? Salto { get; set; }
        public int? Nro { get; set; }
        public string Tipo { get; set; }
        public EstadoRedet Etapa { get; set; }
        public string Observaciones { get; set; }
        public string CodigoRedet { get; set; }
        public decimal? Porcentaje { get; set; }
        public String Empresa { get; set; }
        public String Area { get; set; }
        public DateTime? FechaSade { get; set; }
        public string BuzonSade { get; set; }

        public decimal? MontoRedet { get; set; }
        public Usuario Usuario { get; set; }

        public Redeterminacion() { }
    }
}
