using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dominio
{
    [Table("Linea_de_Gestion")] // Mapeo a la tabla Linea_de_Gestion
    public class LineaGestionEF
    {
        [Key]
        public int Id { get; set; }
        public string Nombre { get; set; }

        public virtual ICollection<ProyectoEF> BdProyectos { get; set; }
    }
}
