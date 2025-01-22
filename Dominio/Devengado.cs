using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio
{
    public class Devengado
    {
        public int Id { get; set; }

        public int? Ejercicio { get; set; }

        public string TipoDev { get; set; }

        public int NumeroDevengado { get; set; }

        public string EstadoFirma { get; set; }

        public string Expediente { get; set; }

        public int? Ente { get; set; }

        public long? Cuit { get; set; }

        public string Descripcion { get; set; }

        public int? Obra { get; set; }

        public decimal? ImporteDevengado { get; set; }

        public int? Jurisdiccion { get; set; }

        public int? SubJurisdiccion { get; set; }

        public int? Entidad { get; set; }

        public int? Programa { get; set; }

        public int? SubPrograma { get; set; }

        public int? Proyecto { get; set; }

        public int? Actividad { get; set; }

        public int? Obra2 { get; set; }

        public int? Inciso { get; set; }

        public int? Principal { get; set; }

        public int? Parcial { get; set; }

        public int? SubParcial { get; set; }

        public int? FuenteFinanciera { get; set; }

        public int? UbicacionGeo { get; set; }

        public int? CuentaEscritural { get; set; }

        public string CuentaPagadora { get; set; }

        public decimal? ImportePP { get; set; }

        public decimal? ImportePagado { get; set; }

        public decimal? SaldoAPagarPP { get; set; }

        public decimal? TotalDevengado { get; set; }

        public decimal? TotalPagado { get; set; }

        public decimal? DeudaDev { get; set; }

        public DateTime? FechaImputacion { get; set; }

        public string EeFinanciera { get; set; }

        public string EeAutorizante { get; set; }

        public decimal? MontoDevengado { get; set; }
        public string Concatenado { get; set; }
        public string Tipo { get; set; }
    }
}
