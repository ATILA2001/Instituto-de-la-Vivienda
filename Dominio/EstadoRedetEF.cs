using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dominio
{
    [Table("Estados_Redet")] // Mapeo a la tabla Estados_Redet
    public class EstadoRedetEF
    {
        [Key]
        public int Id { get; set; }
        public string Nombre { get; set; }

        public virtual ICollection<RedeterminacionEF> Redeterminaciones { get; set; }
    }
}
