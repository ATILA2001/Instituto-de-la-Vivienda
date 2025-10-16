using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio
{

    [Table("GEDO")]
    public class AcdirEF
    {
        [Key]
        [Column("NRO_EXPEDIENTE")]
        public string Expediente { get; set; }

        [Column("NRO_ESPECIAL")]
        public string Acdir { get; set; }
    }

}
