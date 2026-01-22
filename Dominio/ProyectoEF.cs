using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dominio
{
    [Table("BD_Proyectos")] // Mapeo a la tabla BD_Proyectos
    public class ProyectoEF
    {
        public int Id { get; set; }

        [Key, ForeignKey("ObraEF")]
        [Column("ID_BASE")] // Mapeo a la columna ID_BASE
        public int ObraId { get; set; }
        public virtual ObraEF ObraEF { get; set; }

        [Column("PROYECTO")]
        public string Nombre { get; set; }

        [Column("SUBPROYECTO")]
        public string NombreSubProyecto { get; set; }

        [Column("AUTORIZADO2025")]
        public decimal Autorizado2025 { get; set; }
        [Column("AUTORIZADO2026")]
        public decimal Autorizado2026 { get; set; }

        [Column("LINEA_DE_GESTION")]
        public int LineaGestionEFId { get; set; }
        [ForeignKey("LineaGestionEFId")]
        public virtual LineaGestionEF LineaGestionEF { get; set; }
    }
}
