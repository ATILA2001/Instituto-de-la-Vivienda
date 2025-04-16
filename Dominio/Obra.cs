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
        public decimal? MontoInicial { get; set; }
        public decimal? MontoActual { get; set; }
        public decimal? MontoFaltante { get; set; }
        public string Linea {  get; set; }
        public string Proyecto { get; set; }


        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }

        public decimal? MontoCertificado { get; set; }
        public decimal? Porcentaje { get; set; }

        public Obra() { }

        public Obra(int id, Area area, Empresa empresa, int? numero = null, Contrata contrata = null,
                    int? año = null, int? etapa = null, int? obraNumero = null, Barrio barrio = null,
                    string descripcion = null, decimal? autorizadoInicial = null, decimal? autorizadoNuevo = null,
                    decimal? montoInicial = null, decimal? montoActual = null, decimal? montoFaltante = null,
                    string linea = null, string proyecto = null, DateTime? fechaInicio = null,
                    DateTime? fechaFin = null, decimal? montoCertificado = null, decimal? porcentaje = null)
        {
            Id = id;
            Area = area;
            Empresa = empresa;
            Numero = numero;
            Contrata = contrata;
            Año = año;
            Etapa = etapa;
            ObraNumero = obraNumero;
            Barrio = barrio;
            Descripcion = descripcion;
            AutorizadoInicial = autorizadoInicial;
            AutorizadoNuevo = autorizadoNuevo;
            MontoInicial = montoInicial;
            MontoActual = montoActual;
            MontoFaltante = montoFaltante;
            Linea = linea;
            Proyecto = proyecto;
            FechaInicio = fechaInicio;
            FechaFin = fechaFin;
            MontoCertificado = montoCertificado;
            Porcentaje = porcentaje;
        }


    }
}
