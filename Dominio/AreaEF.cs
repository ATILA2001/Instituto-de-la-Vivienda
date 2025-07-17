using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dominio
{
    [Table("Areas")] // Mapeo a la tabla Areas
    public class AreaEF
    {
        [Key]
        public int Id { get; set; }
        public string Nombre { get; set; }

        public virtual ICollection<ObraEF> Obras { get; set; }
    }
}
