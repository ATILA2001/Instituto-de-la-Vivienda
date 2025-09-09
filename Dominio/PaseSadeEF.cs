using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dominio
{
    [Table("PASES_SADE")]
    public class PaseSadeEF
    {
        [Key]
        [Column("EXPEDIENTE")]
        public string Expediente { get; set; }

        [Column("BUZON DESTINO")]
        public string BuzonDestino { get; set; }
        [Column("FECHA ULTIMO PASE")]
        public DateTime? FechaUltimoPase { get; set; }
    }
}