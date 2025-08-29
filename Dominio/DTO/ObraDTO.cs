using System;

namespace Dominio.DTO
{
    // DTO plano usado por la grilla de Obras (renombrado desde ObraGridDTO)
    [Serializable]
    public class ObraDTO
    {
        public int Id { get; set; }
        public string Area { get; set; }
        public int? AreaId { get; set; }
        public string Empresa { get; set; }
        public int? EmpresaId { get; set; }
        public string Contrata { get; set; }
        public int? ContrataId { get; set; }
        public int? Numero { get; set; }
        public int? Anio { get; set; }
        public string Barrio { get; set; }
        public int? BarrioId { get; set; }
        public string Descripcion { get; set; }
        public string LineaGestionNombre { get; set; }
        public string ProyectoNombre { get; set; }
        public int? ProyectoId { get; set; }

        // Campos financieros precalculados
        public decimal? AutorizadoNuevo { get; set; }
        public decimal? MontoCertificado { get; set; }
        public decimal? Porcentaje { get; set; }
        public decimal? MontoInicial { get; set; }
        public decimal? MontoActual { get; set; }
        public decimal? MontoFaltante { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
    }
}
