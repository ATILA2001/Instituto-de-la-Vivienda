using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dominio
{    [Table("BD_Proyectos")] // Mapeo a la tabla BD_Proyectos
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

        [Column("AUTORIZADO_INICIAL")]
        public decimal AutorizadoInicial { get; set; }
        [Column("AUTORIZADO_NUEVO")]
        public decimal AutorizadoNuevo { get; set; }

        [Column("LINEA_DE_GESTION")]
        public int LineaGestionEFId { get; set; }
        [ForeignKey("LineaGestionEFId")]
        public virtual LineaGestionEF LineaGestionEF { get; set; }
    }
}
