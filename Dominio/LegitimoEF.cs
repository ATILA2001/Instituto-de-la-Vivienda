using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dominio
{
    [Table("LEGITIMOS_ABONOS")]
    public class LegitimoEF
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Column("OBRA")]
        public int ObraId { get; set; }

        [ForeignKey("ObraId")]
        public virtual ObraEF ObraEF { get; set; }

        [Column("CODIGO_AUTORIZANTE")]
        public string CodigoAutorizante { get; set; }

        [Column("EXPEDIENTE")]
        public string Expediente { get; set; }

        [Column("INICIO_EJECUCION")]
        public DateTime? InicioEjecucion { get; set; }

        [Column("FIN_EJECUCION")]
        public DateTime? FinEjecucion { get; set; }

        [Column("CERTIFICADO")]
        public decimal? Certificado { get; set; }

        [Column("MES_APROBACION")]
        public DateTime? MesAprobacion { get; set; }

        // Propiedades auxiliares para uso en vista/export. Se rellenan en la capa de negocio.
        [NotMapped]
        public string Empresa { get; set; }

        [NotMapped]
        public string Linea { get; set; }

        [NotMapped]
        public decimal? Sigaf { get; set; }

        [NotMapped]
        public string BuzonSade { get; set; }

        [NotMapped]
        public DateTime? FechaSade { get; set; }

        [NotMapped]
        public string Estado { get; set; }
    }
}
