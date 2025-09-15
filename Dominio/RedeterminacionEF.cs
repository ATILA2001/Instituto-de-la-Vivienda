using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace Dominio
{
    [Table("Redeterminaciones")]
    public class RedeterminacionEF
    {
        [Key]
        public int Id { get; set; }

        [NotMapped]
        public virtual AutorizanteEF Autorizante { get; set; }

        [Column("CODIGO_AUTORIZANTE")]
        public string CodigoAutorizante { get; set; }

        public string Expediente { get; set; }

        public DateTime? Salto { get; set; }

        public int? Nro { get; set; }

        public string Tipo { get; set; }

        [Column("ETAPA")]
        public int? EstadoRedetEFId { get; set; }


        [ForeignKey("EstadoRedetEFId")]
        public virtual EstadoRedetEF Etapa { get; set; }

        public string Observaciones { get; set; }

        // Columna calculada en BD: se lee pero no se envía en INSERT/UPDATE
        [Column("Codigo_Redet")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string CodigoRedet { get; set; }

        [Column("PORCENTAJE_PONDERACION")]
        public decimal? Porcentaje { get; set; }


        [Column("ID_USUARIO")]
        public int? UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public virtual UsuarioEF Usuario { get; set; }

        [NotMapped]
        public string Empresa { get; set; }
        [NotMapped]
        public string Area { get; set; }

        [NotMapped]
        public DateTime? FechaSade { get; set; }

        [NotMapped]
        public string BuzonSade { get; set; }

        [NotMapped]
        public decimal? MontoRedet { get; set; }
    }
}
