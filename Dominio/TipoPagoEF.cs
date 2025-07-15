using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dominio
{
    [Table("Tipo_Pago")] // Mapeo a la tabla Tipo_Pago
    public class TipoPagoEF
    {
        [Key]
        public int Id { get; set; }
        public string Nombre { get; set; }

        public virtual ICollection<CertificadoEF> Certificados { get; set; }
    }
}
