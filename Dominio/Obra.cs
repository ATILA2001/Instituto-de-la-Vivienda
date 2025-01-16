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
        public Contrata Contrata { get; set; } 
        public int? Año { get; set; } 
        public int? Etapa { get; set; } 
        public int? ObraNumero { get; set; } 
        public Barrio Barrio { get; set; } 
        public string Descripcion { get; set; } 

        public  decimal? AutorizadoInicial { get; set; }
        public decimal? AutorizadoNuevo { get; set; }
        public decimal? MontoAutorizante { get; set; }
        public decimal? MontoCertificado { get; set; }
        public decimal? Porcentaje { get; set; }

        public Obra() { }

    }
}
