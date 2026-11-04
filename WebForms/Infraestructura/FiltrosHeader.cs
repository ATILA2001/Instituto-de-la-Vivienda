using System;
using System.Collections.Generic;
using System.Linq;

namespace WebForms.Infraestructura
{
    /// <summary>
    /// Opciones para los TreeViewSearch de cabecera de las grillas, derivadas del snapshot que el usuario
    /// ya tiene en sesión. Así el filtro ofrece sólo valores que el usuario puede ver: antes las listas de
    /// áreas, obras y proyectos salían de la tabla completa y mostraban nombres de otras áreas.
    /// </summary>
    public static class FiltrosHeader
    {
        public sealed class Opcion
        {
            public int Id { get; set; }
            public string Nombre { get; set; }
        }

        /// <summary>Valores distintos por Id, ordenados por nombre. Ignora los elementos sin Id.</summary>
        public static List<Opcion> Distintos<T>(IEnumerable<T> datos, Func<T, int?> id, Func<T, string> nombre)
        {
            return (datos ?? Enumerable.Empty<T>())
                .Select(d => new { Id = id(d), Nombre = nombre(d) })
                .Where(x => x.Id.HasValue)
                .GroupBy(x => x.Id.Value)
                .Select(g => new Opcion { Id = g.Key, Nombre = g.First().Nombre ?? string.Empty })
                .OrderBy(o => o.Nombre)
                .ToList();
        }
    }
}
