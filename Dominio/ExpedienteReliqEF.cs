using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dominio
{
    [Table("EXPEDIENTES_RELIQ")]
    public class ExpedienteReliqEF
    {
        [Key]
        public int Id { get; set; }

        [Column("CODIGO_REDET")]
        public string CodigoRedet { get; set; }

        [Column("MES_APROBACION")]
        public DateTime MesAprobacion { get; set; }

        [Column("EXPEDIENTE")]
        public string Expediente { get; set; }
    }
}
