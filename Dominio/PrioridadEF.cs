using System.ComponentModel.DataAnnotations;

namespace Dominio
{
    public class PrioridadEF
    {
        [Key]
        public int Id { get; set; }
        public string Nombre { get; set; }
        // Agrega aquí las relaciones necesarias si las hay
    }
}
