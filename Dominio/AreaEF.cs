using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dominio
{
    [Table("Areas")] // Mapeo a la tabla Areas
    public class AreaEF
    {
        [Key]
        public int Id { get; set; }
        public string Nombre { get; set; }

        /// <summary>
        /// ID del área en Auth.Web. Permite correlacionar el claim "area" (entero) de la cookie
        /// con el área local de IVC. Nullable: si es NULL el área no está mapeada.
        /// </summary>
        public int? AuthAreaId { get; set; }

        public virtual ICollection<ObraEF> Obras { get; set; }
    }
}
