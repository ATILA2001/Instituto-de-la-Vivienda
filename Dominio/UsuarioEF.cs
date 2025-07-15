using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dominio
{
    [Table("Usuarios")]
    public class UsuarioEF
    {
        [Key]
        public int Id { get; set; }
        public string Nombre { get; set; }

        [Column("AREA")]
        public int AreaId { get; set; }
        [ForeignKey("AreaId")]
        public virtual AreaEF Area { get; set; }
    }
}
