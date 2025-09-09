using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dominio
{
    [Table("FORMULACION")]
    public class FormulacionEF
    {

        [Key]
        [DatabaseGenerated(databaseGeneratedOption: DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("ObraEF")]
        [Column("ID_BASE")]
        public int ObraId { get; set; }
        public virtual ObraEF ObraEF { get; set; }

        [Column("MONTO_26")]
        public decimal? Monto_26 { get; set; }
        [Column("MONTO_27")]
        public decimal? Monto_27 { get; set; }
        [Column("MONTO_28")]
        public decimal? Monto_28 { get; set; }
        [Column("MES_BASE")]
        public DateTime? MesBase { get; set; }
        [Column("OBSERVACIONES")]
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
    }
}
