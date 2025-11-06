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
        //[Index("IX_AutorizanteId_Unique", IsUnique = true)]
        public int AutorizanteId { get; set; }

        [Required]
       
        public decimal Importe { get; set; }

        [Required]
      
        public string Norma { get; set; }

        [Required]
        
        public DateTime FechaNorma { get; set; }

   
    }
}
