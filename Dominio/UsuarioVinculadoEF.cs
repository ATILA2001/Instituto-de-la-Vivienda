using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dominio
{
    [Table("UsuariosVinculados")]
    public class UsuarioVinculadoEF
    {
        [Key]
        [MaxLength(450)]
        public string AuthUserId { get; set; }

        [Required]
        public string Nombre { get; set; }

        [Column("PLANIFICACION_ABIERTA")]
        public bool IsPlanningOpenOverride { get; set; }
    }
}
