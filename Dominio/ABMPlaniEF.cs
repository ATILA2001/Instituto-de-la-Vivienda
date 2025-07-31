using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dominio
{
    [Table("ABM_PLANI")]
    public class ABMPlaniEF
    {
        [Key]
        public int Id { get; set; }

        [Column("PLANIFICACION_ABIERTA")]
        public bool IsPlanningOpen { get; set; }
        [Column("FORMULACION_ABIERTA")]
        public bool IsFormulationOpen { get; set; }
        // Puedes agregar más campos según la estructura real de la tabla
    }
}
