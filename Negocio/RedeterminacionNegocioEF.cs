using Dominio;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Diagnostics;

namespace Negocio
{
    /// <summary>
    /// Operaciones de negocio para Redeterminaciones usando Entity Framework.
    /// </summary>
    public class RedeterminacionNegocioEF
    {

        /// <summary>
        /// Lista redeterminaciones aplicando filtros opcionales por etapa, autorizante u obra.
        /// </summary>
        public List<RedeterminacionEF> Listar(
            List<int> etapasIds = null,
            List<int> autorizantesIds = null,
            List<int> obrasIds = null,
            string filtro = null)
        {
            using (var context = new IVCdbContext())
            {
                IQueryable<RedeterminacionEF> query = context.Redeterminaciones.AsNoTracking().Include(r => r.Etapa);

                if (etapasIds != null && etapasIds.Any())
                    query = query.Where(r => etapasIds.Contains(r.EstadoRedetEFId.Value));

                // Filtrar por autorizantes mediante subconsulta sobre Autorizantes (join por CodigoAutorizante)
                if (autorizantesIds != null && autorizantesIds.Any())
                    query = query.Where(r => context.Autorizantes.Any(a => a.CodigoAutorizante == r.CodigoAutorizante && autorizantesIds.Contains(a.Id)));

                // Filtrar por obras mediante subconsulta sobre Autorizantes -> ObraId
                if (obrasIds != null && obrasIds.Any())
                    query = query.Where(r => context.Autorizantes.Any(a => a.CodigoAutorizante == r.CodigoAutorizante && obrasIds.Contains(a.ObraId)));

                if (!string.IsNullOrWhiteSpace(filtro))
                {
                    var like = filtro.Trim().ToUpper();
                    query = query.Where(r =>
                        (r.CodigoRedet ?? string.Empty).ToUpper().Contains(like) ||
                        (r.Expediente ?? string.Empty).ToUpper().Contains(like) ||
                        ((r.Observaciones ?? string.Empty).ToUpper().Contains(like)) ||
                        context.Autorizantes.Any(a => a.CodigoAutorizante == r.CodigoAutorizante && a.Obra != null && (a.Obra.Descripcion ?? string.Empty).ToUpper().Contains(like)) ||
                        context.Autorizantes.Any(a => a.CodigoAutorizante == r.CodigoAutorizante && a.Obra != null && a.Obra.Empresa != null && (a.Obra.Empresa.Nombre ?? string.Empty).ToUpper().Contains(like))
                    );
                }

                var lista = query
                    .OrderByDescending(r => r.Id)
                    .ToList();

                // Cargar manualmente Autorizantes relacionados para que UI pueda leer Obra/Empresa/Area
                var codigos = lista.Select(r => r.CodigoAutorizante).Where(c => !string.IsNullOrEmpty(c)).Distinct().ToList();
                if (codigos.Any())
                {
                    var autores = context.Autorizantes
                        .Where(a => codigos.Contains(a.CodigoAutorizante))
                        .Include(a => a.Obra)
                        .Include("Obra.Area")
                        .Include("Obra.Empresa")
                        .ToList();

                    var autoresMap = autores.ToDictionary(a => a.CodigoAutorizante, a => a);
                    foreach (var r in lista)
                    {
                        if (!string.IsNullOrEmpty(r.CodigoAutorizante) && autoresMap.TryGetValue(r.CodigoAutorizante, out var auth))
                        {
                            r.Autorizante = auth;
                            r.Empresa = auth.Obra?.Empresa?.Nombre;
                            r.Area = auth.Obra?.Area?.Nombre;
                        }
                    }
                }

                // Añadir datos de último pase SADE (BuzonSade, FechaSade)
                var expedientes = lista
                    .Where(r => !string.IsNullOrEmpty(r.Expediente))
                    .Select(r => r.Expediente.Trim())
                    .Distinct()
                    .ToList();
                if (expedientes.Count > 0)
                {
                    var pases = context.PasesSade
                        .Where(p => expedientes.Contains(p.Expediente))
                        .ToList()
                        .GroupBy(p => p.Expediente)
                        .Select(g => g.OrderByDescending(p => p.FechaUltimoPase).FirstOrDefault())
                        .Where(p => p != null)
                        .ToDictionary(p => p.Expediente, p => (p.BuzonDestino, (DateTime?)p.FechaUltimoPase));

                    foreach (var r in lista)
                    {
                        if (!string.IsNullOrEmpty(r.Expediente) && pases.TryGetValue(r.Expediente.Trim(), out var info))
                        {
                            r.BuzonSade = info.BuzonDestino;
                            r.FechaSade = info.Item2;
                        }
                    }
                }

                return lista;
            }
        }

        public RedeterminacionEF ObtenerPorId(int id)
        {
            using (var context = new IVCdbContext())
            {
                // Importante: 'Autorizante' es [NotMapped] en RedeterminacionEF, no puede usarse en Include.
                var redet = context.Redeterminaciones
                    .AsNoTracking()
                    .FirstOrDefault(r => r.Id == id);

                if (redet == null)
                    return null;

                // Carga manual del Autorizante y su Obra/Empresa/Area usando CodigoAutorizante
                if (!string.IsNullOrEmpty(redet.CodigoAutorizante))
                {
                    var auth = context.Autorizantes
                        .AsNoTracking()
                        .Include(a => a.Obra)
                        .Include("Obra.Area")
                        .Include("Obra.Empresa")
                        .FirstOrDefault(a => a.CodigoAutorizante == redet.CodigoAutorizante);
                    if (auth != null)
                    {
                        redet.Autorizante = auth; // propiedad NotMapped para uso en UI
                        redet.Empresa = auth.Obra?.Empresa?.Nombre;
                        redet.Area = auth.Obra?.Area?.Nombre;
                    }
                }

                // Obtener último pase SADE (Buzon / Fecha) si hay expediente
                if (!string.IsNullOrEmpty(redet.Expediente))
                {
                    var pase = context.PasesSade
                        .AsNoTracking()
                        .Where(p => p.Expediente == redet.Expediente)
                        .OrderByDescending(p => p.FechaUltimoPase)
                        .FirstOrDefault();
                    if (pase != null)
                    {
                        redet.BuzonSade = pase.BuzonDestino;
                        redet.FechaSade = pase.FechaUltimoPase;
                    }
                }

                return redet;
            }
        }

        public bool Agregar(RedeterminacionEF redet)
        {
            if (redet == null) throw new ArgumentNullException(nameof(redet));

            using (var context = new IVCdbContext())
            {
                context.Redeterminaciones.Add(redet);
                return context.SaveChanges() > 0;
            }
        }

        /// <summary>
        /// Modifica una redeterminación existente (debe venir con Id válido).
        /// </summary>
        public bool Modificar(RedeterminacionEF redet)
        {
            using (var context = new IVCdbContext())
            {
                context.Entry(redet).State = EntityState.Modified;
                return context.SaveChanges() > 0;
            }
        }

        /// <summary>
        /// Elimina una redeterminación por Id.
        /// </summary>
        public bool Eliminar(int id)
        {
            using (var context = new IVCdbContext())
            {
                var entity = context.Redeterminaciones.Find(id);
                if (entity == null) return false;
                context.Redeterminaciones.Remove(entity);
                return context.SaveChanges() > 0;
            }
        }

        /// <summary>
        /// Actualiza solo el expediente de una redeterminación.
        /// </summary>
        public bool ActualizarExpediente(int id, string expediente)
        {
            using (var context = new IVCdbContext())
            {
                var entity = context.Redeterminaciones.Find(id);
                if (entity == null) throw new ApplicationException("Redeterminación no encontrada.");
                entity.Expediente = expediente;
                return context.SaveChanges() > 0;
            }
        }

        /// <summary>
        /// Actualiza solo el estado (etapa) de una redeterminación.
        /// </summary>
        public bool ActualizarEstado(int id, int estadoRedetId)
        {
            using (var context = new IVCdbContext())
            {
                var entity = context.Redeterminaciones.Find(id);
                if (entity == null) throw new ApplicationException("Redeterminación no encontrada.");
                entity.EstadoRedetEFId = estadoRedetId;
                return context.SaveChanges() > 0;
            }
        }

        /// <summary>
        /// Actualiza el usuario (ID_USUARIO).
        /// </summary>
        public bool ActualizarUsuario(int id, int? usuarioId)
        {
            using (var context = new IVCdbContext())
            {
                var entity = context.Redeterminaciones.Find(id);
                if (entity == null) throw new ApplicationException("Redeterminación no encontrada.");

                if (usuarioId.HasValue)
                {
                    // Validamos que el usuario exista.
                    var usuarioExists = context.Usuarios.Any(u => u.Id == usuarioId.Value);
                    if (!usuarioExists)
                        throw new ApplicationException("Usuario no encontrado.");
                }

                entity.UsuarioId = usuarioId; // puede ser null para desasignar
                return context.SaveChanges() > 0;
            }
        }


        /// <summary>
        /// Cuenta el total de redeterminaciones aplicando filtros simples.
        /// </summary>
        public int ContarConFiltros(string filtroTexto,
            List<int> obrasIds,
            List<int> autorizantesIds,
            List<int> estadosIds)
        {
            try
            {
                using (var ctx = new IVCdbContext())
                {
                    // Construir query base sin navegar por propiedades NotMapped
                    IQueryable<RedeterminacionEF> q = ctx.Set<RedeterminacionEF>().AsNoTracking();

                    // Aplicar filtro de texto si existe; para campos relacionados usamos subconsultas sobre Autorizantes
                    if (!string.IsNullOrWhiteSpace(filtroTexto))
                    {
                        string ft = filtroTexto.Trim().ToUpper();
                        q = q.Where(r =>
                            ((r.Expediente ?? string.Empty).ToUpper().Contains(ft)) ||
                            ((r.CodigoRedet ?? string.Empty).ToUpper().Contains(ft)) ||
                            ((r.Tipo ?? string.Empty).ToUpper().Contains(ft)) ||
                            ((r.Observaciones ?? string.Empty).ToUpper().Contains(ft)) ||
                            ctx.Autorizantes.Any(a => a.CodigoAutorizante == r.CodigoAutorizante && a.Obra != null && (a.Obra.Descripcion ?? string.Empty).ToUpper().Contains(ft)) ||
                            ctx.Autorizantes.Any(a => a.CodigoAutorizante == r.CodigoAutorizante && a.Obra != null && a.Obra.Empresa != null && (a.Obra.Empresa.Nombre ?? string.Empty).ToUpper().Contains(ft))
                        );
                    }

                    // Aplicar filtro por obras (mediante Autorizantes.ObraId)
                    if (obrasIds != null && obrasIds.Any())
                    {
                        q = q.Where(r => ctx.Autorizantes.Any(a => a.CodigoAutorizante == r.CodigoAutorizante && obrasIds.Contains(a.ObraId)));
                    }

                    // Aplicar filtro por autorizantes (Autorizantes.Id)
                    if (autorizantesIds != null && autorizantesIds.Any())
                    {
                        q = q.Where(r => ctx.Autorizantes.Any(a => a.CodigoAutorizante == r.CodigoAutorizante && autorizantesIds.Contains(a.Id)));
                    }

                    // Aplicar filtro por estado/etapa (EstadoRedetEFId)
                    if (estadosIds != null && estadosIds.Any())
                    {
                        q = q.Where(r => estadosIds.Contains(r.EstadoRedetEFId.Value));
                    }

                    return q.Count();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ContarPorUsuarioConFiltros error: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Lista las redeterminaciones paginadas aplicando filtros simples.
        /// pageIndex es 0-based.
        /// La implementación actual aplica filtro de texto y paginación; otros filtros se dejan como punto de extensión.
        /// </summary>
        public List<RedeterminacionEF> ListarPaginadoConFiltros(int pageIndex,
            int pageSize,
            string filtroTexto,
            List<int> obrasIds,
            List<int> autorizantesIds,
            List<int> estadosIds)
        {
            try
            {
                using (var ctx = new IVCdbContext())
                {
                    // Base query sin navegar por propiedades NotMapped (solo incluimos Etapa)
                    IQueryable<RedeterminacionEF> q = ctx.Set<RedeterminacionEF>().AsNoTracking().Include(r => r.Etapa);

                    // Aplicar filtro de texto (para campos relacionados usamos subconsultas sobre Autorizantes)
                    if (!string.IsNullOrWhiteSpace(filtroTexto))
                    {
                        string ft = filtroTexto.Trim().ToUpper();
                        q = q.Where(r =>
                            ((r.Expediente ?? string.Empty).ToUpper().Contains(ft)) ||
                            ((r.CodigoRedet ?? string.Empty).ToUpper().Contains(ft)) ||
                            ((r.Tipo ?? string.Empty).ToUpper().Contains(ft)) ||
                            ((r.Observaciones ?? string.Empty).ToUpper().Contains(ft)) ||
                            ctx.Autorizantes.Any(a => a.CodigoAutorizante == r.CodigoAutorizante && a.Obra != null && (a.Obra.Descripcion ?? string.Empty).ToUpper().Contains(ft)) ||
                            ctx.Autorizantes.Any(a => a.CodigoAutorizante == r.CodigoAutorizante && a.Obra != null && a.Obra.Empresa != null && (a.Obra.Empresa.Nombre ?? string.Empty).ToUpper().Contains(ft))
                        );
                    }

                    // Aplicar filtro por obras (mediante Autorizantes.ObraId)
                    if (obrasIds != null && obrasIds.Any())
                    {
                        q = q.Where(r => ctx.Autorizantes.Any(a => a.CodigoAutorizante == r.CodigoAutorizante && obrasIds.Contains(a.ObraId)));
                    }

                    // Aplicar filtro por autorizantes
                    if (autorizantesIds != null && autorizantesIds.Any())
                    {
                        q = q.Where(r => ctx.Autorizantes.Any(a => a.CodigoAutorizante == r.CodigoAutorizante && autorizantesIds.Contains(a.Id)));
                    }

                    // Aplicar filtro por estados/etapas
                    if (estadosIds != null && estadosIds.Any())
                    {
                        q = q.Where(r => estadosIds.Contains(r.EstadoRedetEFId.Value));
                    }

                    // Orden, paginación y materialización
                    var lista = q.OrderBy(r => r.CodigoAutorizante).ThenBy(r => r.Nro)
                                 .Skip(pageIndex * pageSize).Take(pageSize)
                                 .ToList();

                    // Cargar manualmente Autorizantes relacionados para que la UI pueda leer Obra/Empresa/Area
                    var codigos = lista.Select(r => r.CodigoAutorizante).Where(c => !string.IsNullOrEmpty(c)).Distinct().ToList();
                    if (codigos.Any())
                    {
                        var autores = ctx.Autorizantes
                            .Where(a => codigos.Contains(a.CodigoAutorizante))
                            .Include(a => a.Obra)
                            .Include("Obra.Area")
                            .Include("Obra.Empresa")
                            .ToList();

                        var autoresMap = autores.ToDictionary(a => a.CodigoAutorizante, a => a);
                        foreach (var r in lista)
                        {
                            if (!string.IsNullOrEmpty(r.CodigoAutorizante) && autoresMap.TryGetValue(r.CodigoAutorizante, out var auth))
                            {
                                r.Autorizante = auth;
                                r.Empresa = auth.Obra?.Empresa?.Nombre;
                                r.Area = auth.Obra?.Area?.Nombre;
                            }
                        }
                    }

                    // Añadir datos de último pase SADE (BuzonSade, FechaSade)
                    var expedientes = lista
                        .Where(r => !string.IsNullOrEmpty(r.Expediente))
                        .Select(r => r.Expediente.Trim())
                        .Distinct()
                        .ToList();
                    if (expedientes.Count > 0)
                    {
                        var pases = ctx.PasesSade
                            .Where(p => expedientes.Contains(p.Expediente))
                            .ToList()
                            .GroupBy(p => p.Expediente)
                            .Select(g => g.OrderByDescending(p => p.FechaUltimoPase).FirstOrDefault())
                            .Where(p => p != null)
                            .ToDictionary(p => p.Expediente, p => (p.BuzonDestino, (DateTime?)p.FechaUltimoPase));

                        foreach (var r in lista)
                        {
                            if (!string.IsNullOrEmpty(r.Expediente) && pases.TryGetValue(r.Expediente.Trim(), out var info))
                            {
                                r.BuzonSade = info.BuzonDestino;
                                r.FechaSade = info.Item2;
                            }
                        }
                    }

                    return lista;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ListarPaginadoConFiltros error: {ex.Message}");
                return new List<RedeterminacionEF>();
            }
        }
    }
}
