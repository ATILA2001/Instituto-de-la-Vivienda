using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dominio
{
    [Table("Empresas")] // Mapeo a la tabla Empresas
    public class EmpresaEF
    {
        [Key]
        public int Id { get; set; }
        public string Nombre { get; set; }

        public virtual ICollection<ObraEF> Obras { get; set; }
    }
}
