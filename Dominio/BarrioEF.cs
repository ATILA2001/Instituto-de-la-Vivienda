using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dominio
{
    [Table("Barrios")] // Mapeo a la tabla Barrios
    public class BarrioEF
    {
        [Key]
        public int Id { get; set; }
        public string Nombre { get; set; }

        public virtual ICollection<ObraEF> Obras { get; set; }
    }
}
