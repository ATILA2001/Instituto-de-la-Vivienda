using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio
{
    [Table("AUTORIZANTES_PRESUPUESTO")]
    public class AutorizantePresupuestoEF
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Index("IX_AutorizanteId_Unique", IsUnique = true)]
        public int AutorizanteId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Importe { get; set; }

        [Required]
        [StringLength(255)]
        public string Norma { get; set; }

        [Required]
        [Column(TypeName = "date")]
        public DateTime FechaNorma { get; set; }

        // Propiedad de navegación para la relación uno a uno
        [ForeignKey("AutorizanteId")]
        public virtual AutorizanteEF Autorizante { get; set; }
    }
}
