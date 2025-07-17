using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dominio
{
    [Table("Estados_Autorizantes")] // Mapeo a la tabla Estados_Autorizantes
    public class EstadoAutorizanteEF
    {
        [Key]
        public int Id { get; set; }
        public string Nombre { get; set; }

        public virtual ICollection<AutorizanteEF> Autorizantes { get; set; }
    }
}
