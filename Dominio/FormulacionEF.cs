using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dominio
{
    [Table("FORMULACION")]
    public class FormulacionEF
    {

        public const int ObservacionesMaxLength = 255;
        public const int BreveDescripcionMaxLength = 500;
        [Key]
        [DatabaseGenerated(databaseGeneratedOption: DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("ObraEF")]
        [Column("ID_BASE")]
        public int ObraId { get; set; }
        public virtual ObraEF ObraEF { get; set; }

        [Column("FECHA_PERIODO")]
        public DateTime FechaPeriodo { get; set; }
        [Column("MONTO")]
        public decimal? Monto { get; set; }
        [Column("MES_BASE")]
        public DateTime? MesBase { get; set; }
        [Column("OBSERVACIONES")]
        [StringLength(ObservacionesMaxLength)]
        public string Observaciones { get; set; }
        [Column("VALOR_MEDIDA")]
        public decimal? ValorMedida { get; set; }
        [Column("PPI")]
        public int? Ppi { get; set; }
        [Column("TECHOS")]
        public decimal? Techos { get; set; }

        [ForeignKey("UnidadMedidaEF")]
        [Column("ID_UNIDAD_MEDIDA")]
        public int? UnidadMedidaId { get; set; }
        public virtual UnidadesMedidaEF UnidadMedidaEF { get; set; }

        [ForeignKey("PrioridadEF")]
        [Column("PRIORIDAD")]
        public int? PrioridadId { get; set; }
        public virtual PrioridadesEF PrioridadEF { get; set; }

        [Column("BREVE_DESCRIPCION")]
        [StringLength(BreveDescripcionMaxLength)]
        public string BreveDescripcion { get; set; }
        [Column("FECHA_INICIO")]
        public DateTime? FechaInicio { get; set; }
        [Column("FECHA_FIN")]
        public DateTime? FechaFin { get; set; }
    }
}
