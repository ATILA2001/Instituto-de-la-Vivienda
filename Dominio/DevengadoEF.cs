using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dominio
{    [Table("DEVENGADOS")]
    public class DevengadoEF
    {
        [Key]
        [Column("EE_FINANCIERA")]
        public string EeFinanciera { get; set; }

        [Column("IMPORTE_PP")]
        public decimal? ImportePp { get; set; }
    }
}
