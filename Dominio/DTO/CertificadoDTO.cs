using System;

namespace Dominio.DTO
{
    /// <summary>
    /// DTO (Data Transfer Object) para representar los datos aplanados de un Certificado para la grilla.
    /// Combina propiedades de CertificadoEF y sus entidades relacionadas para una consulta eficiente.
    /// </summary>

    [Serializable]
    public class CertificadoDTO
    {
        // Propiedades de CertificadoEF
        public int Id { get; set; }
        public int IdReliquidacion { get; set; }
        public decimal MontoTotal { get; set; }
        public DateTime? MesAprobacion { get; set; }
        public string ExpedientePago { get; set; }
        public string CodigoAutorizante { get; set; }

        // Propiedades calculadas que se llenarán después
        public string Estado { get; set; }
        public decimal? Porcentaje { get; set; }
        public decimal? Sigaf { get; set; }
        public string BuzonSade { get; set; }
        public DateTime? FechaSade { get; set; }
        public string Contrata { get; set; }

        // Propiedades aplanadas de entidades relacionadas
        public int? AutorizanteId { get; set; }
        public string AutorizanteEstado { get; set; }
        public int? ObraId { get; set; }
        public string ObraDescripcion { get; set; }
        public int? AreaId { get; set; }
        public string AreaNombre { get; set; }
        public int? BarrioId { get; set; }
        public string BarrioNombre { get; set; }
        public int? EmpresaId { get; set; }
        public string EmpresaNombre { get; set; }
        public int? ProyectoId { get; set; }
        public string ProyectoNombre { get; set; }
        public int? LineaGestionId { get; set; }
        public string LineaGestionNombre { get; set; }
        public int? TipoPagoId { get; set; }
        public string TipoPagoNombre { get; set; }
        public int? EstadoRedetId { get; set; }
    }
}