using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dominio
{
    public class FormulacionEF
    {
        [Key, ForeignKey("ObraEF")]
        public int ObraId { get; set; }
        public virtual ObraEF ObraEF { get; set; }
        // Agrega aquí los campos adicionales de Formulacion
    }
}
