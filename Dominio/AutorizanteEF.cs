using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dominio
{
    [Table("Autorizantes")]
    public class AutorizanteEF
    {
        
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }        

        [Key]
        [Column("CODIGO_AUTORIZANTE")]
        public string CodigoAutorizante { get; set; }        

        public string Detalle { get; set; }
        
        [Column("EXPEDIENTE")]
        public string Expediente { get; set; }

        [Column("MONTO_AUTORIZADO")]
        public decimal MontoAutorizado { get; set; }

        // La columna Fecha probablemente tenga otro nombre en la BD, o sea calculada
        [NotMapped]
        public DateTime? Fecha { get; set; }

        [Column("MES_BASE")]
        public DateTime? MesBase { get; set; }

        // Empresa y BuzonSade probablemente sean campos calculados o de otras tablas
        [NotMapped]
        public string Empresa { get; set; }

        [NotMapped]
        public string BuzonSade { get; set; }

        [Column("OBRA")]
        public int ObraId { get; set; }
        [ForeignKey("ObraId")]
        public virtual ObraEF Obra { get; set; }

        [Column("CONCEPTO")]
        public int ConceptoId { get; set; }
        [ForeignKey("ConceptoId")]
        public virtual ConceptoEF Concepto { get; set; }

        [Column("ESTADO")]
        public int EstadoId { get; set; }        
        [ForeignKey("EstadoId")]
        public virtual EstadoAutorizanteEF Estado { get; set; }

        // NOTA: La colección de Certificados se eliminó para evitar conflictos de mapeo en EF.
        // La relación se manejará manualmente en la capa de negocio usando CodigoAutorizante.
        public virtual ICollection<RedeterminacionEF> Redeterminaciones { get; set; }
    }
}