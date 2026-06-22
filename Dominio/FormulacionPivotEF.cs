using System;

namespace Dominio
{
    /// <summary>
    /// Vista "pivoteada" de la formulación de una obra: una sola fila lógica que agrupa
    /// las (hasta) 3 filas FORMULACION de los años del ciclo vigente.
    ///
    /// No se persiste: se arma en la capa de negocio para mostrar 1 fila por obra en la
    /// grilla y para cargar/editar los 3 montos en un solo formulario. Los campos
    /// compartidos son idénticos en las filas reales de la obra (se escriben iguales en
    /// las 3). Los montos son posicionales: Anio1 = año base, Anio2 = base+1, Anio3 = base+2.
    /// </summary>
    public class FormulacionPivotEF
    {
        public int ObraId { get; set; }
        public ObraEF ObraEF { get; set; }

        // Campos compartidos por la obra (iguales en las 3 filas del ciclo)
        public int? Ppi { get; set; }
        public decimal? Techos { get; set; }
        public DateTime? MesBase { get; set; }
        public int? UnidadMedidaId { get; set; }
        public UnidadesMedidaEF UnidadMedidaEF { get; set; }
        public decimal? ValorMedida { get; set; }
        public int? PrioridadId { get; set; }
        public PrioridadesEF PrioridadEF { get; set; }
        public string BreveDescripcion { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public string Observaciones { get; set; }

        // Montos por posición del ciclo
        public decimal? MontoAnio1 { get; set; }
        public decimal? MontoAnio2 { get; set; }
        public decimal? MontoAnio3 { get; set; }

        /// <summary>Suma de los montos de los 3 años (los nulos cuentan como 0).</summary>
        public decimal TotalMonto => (MontoAnio1 ?? 0m) + (MontoAnio2 ?? 0m) + (MontoAnio3 ?? 0m);
    }
}
