using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dominio
{
    [Table("Certificados")]
    public class CertificadoEF
    {
        [Key]
        public int Id { get; set; }

        // Se mapea la propiedad a la columna CODIGO_AUTORIZANTE.
        // Esta será la clave lógica para enlazar con AutorizanteEF en la capa de negocio.
        [Column("CODIGO_AUTORIZANTE")]
        public string CodigoAutorizante { get; set; }

        // Propiedad NOT MAPPED para asignar el autorizante manualmente en la capa de negocio
        [NotMapped]
        public AutorizanteEF Autorizante { get; set; }

        [Column("EXPEDIENTE_PAGO")]
        public string ExpedientePago { get; set; }

        [Column("MONTO_TOTAL")]
        public decimal MontoTotal { get; set; }

        [Column("MES_APROBACION")]
        public DateTime? MesAprobacion { get; set; }

        // Se mapea la propiedad a la columna TIPO_PAGO.
        [Column("TIPO_PAGO")]
        public int TipoPagoId { get; set; }

        [ForeignKey("TipoPagoId")]
        public virtual TipoPagoEF TipoPago { get; set; }
    }
}