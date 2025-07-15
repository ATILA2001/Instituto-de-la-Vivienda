using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dominio
{
    [Table("Obras")]
    public class ObraEF
    {
        [Key]
        public int Id { get; set; }        [Required]
        public string Descripcion { get; set; }

        // Mapeo correcto - las columnas existen en BD
        [Column("NUMERO")]
        public int? Numero { get; set; }  // Cambiar a int? como en Obra.cs original
        [Column("AÑO")]
        public int? Anio { get; set; }     // Mapeo correcto a la columna AÑO de la BD

        // Relaciones
        [Column("EMPRESA")]
        public int? EmpresaId { get; set; }
        [ForeignKey("EmpresaId")]
        public virtual EmpresaEF Empresa { get; set; }

        [Column("CONTRATA")]
        public int? ContrataId { get; set; }
        [ForeignKey("ContrataId")]
        public virtual ContrataEF Contrata { get; set; }

        [Column("BARRIO")]
        public int? BarrioId { get; set; }
        [ForeignKey("BarrioId")]
        public virtual BarrioEF Barrio { get; set; }

        [Column("AREA")]
        public int? AreaId { get; set; }
        [ForeignKey("AreaId")]
        public virtual AreaEF Area { get; set; }

        // Relación 0..1 a 1 con BdProyectoEF
        public virtual ProyectoEF Proyecto { get; set; }

        // Relación 0..1 a 1 con FormulacionEF
        public virtual FormulacionEF Formulacion { get; set; }

        // Relación 1 a muchos con AutorizanteEF
        public virtual ICollection<AutorizanteEF> Autorizantes { get; set; }

        // Relación 1 a muchos con MovimientoEF
        public virtual ICollection<MovimientoEF> Movimientos { get; set; }
    }
}

