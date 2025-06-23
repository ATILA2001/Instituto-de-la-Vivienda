using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio
{
    [Serializable]
    public class Obra
    {
        public int Id { get; set; }
        public Area Area { get; set; }
        public Empresa Empresa { get; set; }
        public int? Numero { get; set; }
        public string ContrataFormateada { get; set; }
        public Contrata Contrata { get; set; } 
        public int? Año { get; set; } 
        public int? Etapa { get; set; } 
        public int? ObraNumero { get; set; } 
        public Barrio Barrio { get; set; } 
        public string Descripcion { get; set; } 

        public  decimal? AutorizadoInicial { get; set; }
        public decimal? AutorizadoNuevo { get; set; }
        public decimal? MontoInicial { get; set; }
        public decimal? MontoActual { get; set; }
        public decimal? MontoFaltante { get; set; }
        public LineaGestion LineaGestion {  get; set; }
        public BdProyecto Proyecto { get; set; }


        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }

        public decimal? MontoCertificado { get; set; }
        public decimal? Porcentaje { get; set; }

        public Obra() { }

    }
}
