using System;
using System.Collections.Generic;
using System.Linq;

namespace Negocio.Redeterminaciones
{
    /// <summary>
    /// Mapeo de la etapa de una redeterminación (tabla ESTADOS_REDET) al estado general con el que se
    /// muestra en la grilla de Autorizantes. Única definición del lado de la aplicación; la vista SQL
    /// dbo.vw_PBI_Redeterminaciones repite los mismos ids (EstadoGeneralId / EstadoGeneral).
    /// </summary>
    public static class MapeoEstadoRedet
    {
        /// <summary>37 Pendiente, 38 No presentada.</summary>
        public static readonly IReadOnlyList<int> EtapasNoIniciadas = Array.AsReadOnly(new[] { 37, 38 });

        /// <summary>12 RD-11/11-Notificada, 22 RP-09/09-Notificada, 33 RO-11/11-Notificada, 39 ACDIR.</summary>
        public static readonly IReadOnlyList<int> EtapasAprobadas = Array.AsReadOnly(new[] { 12, 22, 33, 39 });

        /// <summary>35 Rechazada, 36 Fuera de Plazo. Definidas en ReglasRedeterminaciones (R6).</summary>
        public static IReadOnlyList<int> EtapasDesestimadas => ReglasRedeterminaciones.EtapasDesestimadas;

        /// <summary>
        /// Estado general a partir del id de etapa. Sin etapa se considera NO INICIADO; cualquier etapa
        /// que no esté en las listas es EN TRAMITE.
        /// </summary>
        public static EstadoGeneral MapearEstadoDesdeId(int? etapaId)
        {
            if (!etapaId.HasValue || EtapasNoIniciadas.Contains(etapaId.Value))
                return EstadoGeneral.NoIniciado;

            if (EtapasAprobadas.Contains(etapaId.Value))
                return EstadoGeneral.Aprobado;

            if (EtapasDesestimadas.Contains(etapaId.Value))
                return EstadoGeneral.Desestimado;

            return EstadoGeneral.EnTramite;
        }

        /// <summary>Los cuatro estados generales, ordenados por nombre, para poblar filtros.</summary>
        public static List<EstadoGeneral> ObtenerEstadosParaFiltro()
        {
            return new List<EstadoGeneral>
            {
                EstadoGeneral.Aprobado,
                EstadoGeneral.Desestimado,
                EstadoGeneral.NoIniciado,
                EstadoGeneral.EnTramite,
            }.OrderBy(e => e.Nombre).ToList();
        }
    }
}
