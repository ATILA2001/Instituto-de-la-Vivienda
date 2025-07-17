using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Dominio
{
    public class UnidadMedidaEF
    {
        [Key]
        public int Id { get; set; }
        public string Nombre { get; set; }
    }
}
