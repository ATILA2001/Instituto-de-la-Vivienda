using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dominio
{
    [Table("Conceptos")] // Mapeo a la tabla Conceptos
    public class ConceptoEF
    {
        [Key]
        public int Id { get; set; }
        public string Nombre { get; set; }

        public virtual ICollection<AutorizanteEF> Autorizantes { get; set; }
    }
}
