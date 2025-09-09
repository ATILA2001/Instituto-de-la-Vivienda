using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dominio
{
    [Table("PRIORIDADES")]
    public class PrioridadesEF
    {
        [Key]
        public int Id { get; set; }
        public string Nombre { get; set; }
        // Agrega aquí las relaciones necesarias si las hay
    }
}
