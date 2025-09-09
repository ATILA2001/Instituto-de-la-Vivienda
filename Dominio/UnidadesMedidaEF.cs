using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dominio
{
    [Table("UNIDADES_MEDIDA")]
    public class UnidadesMedidaEF
    {
        [Key]
        public int Id { get; set; }
        public string Nombre { get; set; }
    }
}
