using System.Collections.Generic;
using System.Linq;
using Dominio;

namespace Negocio.Redeterminaciones
{
    /// <summary>
    /// Mapeo de la etapa de una redeterminación (tabla ESTADOS_REDET) al estado general con el que se
    /// muestra en la grilla de Autorizantes (mismos ids y nombres que ESTADOS_AUTORIZANTES).
    /// Única definición del lado de la aplicación; la vista SQL dbo.vw_PBI_Redeterminaciones repite
    /// los mismos ids (EstadoGeneralId / EstadoGeneral).
    /// </summary>
    public static class EstadoRedetMapper
    {
        /// <summary>37 Pendiente, 38 No presentada.</summary>
        public static readonly int[] EtapasNoIniciadas = { 37, 38 };

        /// <summary>12 RD-11/11-Notificada, 22 RP-09/09-Notificada, 33 RO-11/11-Notificada, 39 ACDIR.</summary>
        public static readonly int[] EtapasAprobadas = { 12, 22, 33, 39 };

        /// <summary>35 Rechazada, 36 Fuera de Plazo. Definidas en RedeterminacionCalculator (R6).</summary>
        public static int[] EtapasDesestimadas => RedeterminacionCalculator.EtapasDesestimadas;

        public const int EstadoAprobadoId = 1;
        public const int EstadoEnTramiteId = 2;
        public const int EstadoDesestimadoId = 3;
        public const int EstadoNoIniciadoId = 4;

        /// <summary>
        /// Estado general a partir del id de etapa. Sin etapa se considera NO INICIADO; cualquier etapa
        /// que no esté en las listas es EN TRAMITE.
        /// </summary>
        public static EstadoAutorizanteEF MapearEstadoDesdeId(int? etapaId)
        {
            if (!etapaId.HasValue || EtapasNoIniciadas.Contains(etapaId.Value))
                return new EstadoAutorizanteEF { Id = EstadoNoIniciadoId, Nombre = "NO INICIADO" };

            if (EtapasAprobadas.Contains(etapaId.Value))
                return new EstadoAutorizanteEF { Id = EstadoAprobadoId, Nombre = "APROBADO" };

            if (EtapasDesestimadas.Contains(etapaId.Value))
                return new EstadoAutorizanteEF { Id = EstadoDesestimadoId, Nombre = "DESESTIMADO" };

            return new EstadoAutorizanteEF { Id = EstadoEnTramiteId, Nombre = "EN TRAMITE" };
        }

        /// <summary>Los cuatro estados generales, ordenados por nombre, para poblar filtros.</summary>
        public static List<EstadoAutorizanteEF> ObtenerEstadosParaFiltro()
        {
            return new List<EstadoAutorizanteEF>
            {
                MapearEstadoDesdeId(EtapasAprobadas[0]),
                MapearEstadoDesdeId(EtapasDesestimadas[0]),
                MapearEstadoDesdeId(EtapasNoIniciadas[0]),
                MapearEstadoDesdeId(0), // EN TRAMITE
            }.OrderBy(e => e.Nombre).ToList();
        }
    }
}
