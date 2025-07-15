using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dominio
{
    public class LegitimoEF
    {
        [Key]
        public int Id { get; set; }
        public int ObraId { get; set; }
        [ForeignKey("ObraId")]
        public virtual ObraEF ObraEF { get; set; }
        // Agrega aquí los campos adicionales de Legitimo
    }
}
