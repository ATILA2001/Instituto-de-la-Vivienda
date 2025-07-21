using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio.DTO
{
    /// <summary>
    /// DTO (Data Transfer Object) para Autorizantes optimizado para transferencia de datos
    /// Incluye todas las propiedades necesarias para la interfaz web incluyendo propiedades calculadas
    /// </summary>
    [Serializable]
    public class AutorizanteDTO
    {
        // Propiedades básicas de AutorizanteEF
        public int Id { get; set; }
        public int IdRedeterminacion { get; set; } // ID ficticio para redeterminaciones (similar a IdReliquidacion en CertificadoDTO)
        public string CodigoAutorizante { get; set; }
        public string Detalle { get; set; }
        public string Expediente { get; set; }
        public decimal MontoAutorizado { get; set; }
        public DateTime? MesAprobacion { get; set; }
        public DateTime? MesBase { get; set; }

        // Propiedades de relaciones (aplanadas para mejor rendimiento)
        public int? ObraId { get; set; }
        public string ObraDescripcion { get; set; }
        public int? ObraNumero { get; set; }
        public int? ObraAnio { get; set; }

        public int? EmpresaId { get; set; }
        public string EmpresaNombre { get; set; }

        public int? AreaId { get; set; }
        public string AreaNombre { get; set; }

        public int? BarrioId { get; set; }
        public string BarrioNombre { get; set; }

        public int? ContrataId { get; set; }
        public string ContrataNombre { get; set; }

        public int? ConceptoId { get; set; }
        public string ConceptoNombre { get; set; }

        public int? EstadoId { get; set; }
        public string EstadoNombre { get; set; }

        public int? ProyectoId { get; set; }
        public string ProyectoNombre { get; set; }

        public int? LineaGestionId { get; set; }
        public string LineaGestionNombre { get; set; }

        // Propiedades calculadas/derivadas
        public string Contrata { get; set; } // Campo calculado con formato "NOMBRE NUMERO/AÑO"
        public string Obra { get; set; } // Campo calculado con formato "DESCRIPCION - BARRIO"

        // Propiedades SADE (calculadas externamente)
        public DateTime? FechaSade { get; set; }
        public string BuzonSade { get; set; }

        // Constructor vacío
        public AutorizanteDTO() { }

        // Método helper para construir la cadena de contrata
        public void CalcularCamposDerivados()
        {
            if (!string.IsNullOrEmpty(ContrataNombre) && !string.IsNullOrEmpty(ObraNumero.ToString()) && ObraAnio.HasValue)
            {
                Contrata = $"{ContrataNombre} {ObraNumero}/{ObraAnio}";
            }

            if (!string.IsNullOrEmpty(ObraDescripcion) && !string.IsNullOrEmpty(BarrioNombre))
            {
                Obra = $"{ObraDescripcion} - {BarrioNombre}";
            }
        }
    }
}
