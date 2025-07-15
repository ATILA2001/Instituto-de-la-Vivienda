using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dominio
{
    [Table("Movimientos")] // Mapeo a la tabla Movimientos
    public class MovimientoEF
    {
        [Key]
        public int Id { get; set; }
        public DateTime? Fecha { get; set; }
        public decimal Monto { get; set; }

        public int ObraId { get; set; }
        [ForeignKey("ObraId")]
        public virtual ObraEF ObraEF { get; set; }

        public string Proyecto { get; set; }
        [Column("Sub_Proyecto")]
        public string SubProyecto { get; set; }
        public string Linea { get; set; }
        [Column("Autorizado_Nuevo")]
        public decimal? AutorizadoNuevo { get; set; }
    }
}
