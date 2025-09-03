using Dominio;
using Dominio.DTO;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.ConstrainedExecution;
using System.Threading.Tasks;
using System.Web;

namespace Negocio
{
    /// <summary>
    /// Clase de negocio especializada en cálculos complejos de redeterminaciones de precios.
    /// 
    /// RESPONSABILIDADES PRINCIPALES:
    /// 1. GENERACIÓN DE REDETERMINACIONES VIRTUALES:
    ///    - Convierte autorizantes base en redeterminaciones calculadas
    ///    - Aplica lógica de negocio para calcular saltos, porcentajes y montos
    ///    - Genera códigos únicos para redeterminaciones (CodigoRedet-R1, R2, etc.)
    /// 
    /// 2. INTEGRACIÓN DE DATOS EXTERNOS:
    ///    - SIGAF: Cálculo de importes devengados por expediente
    ///    - SADE: Información de buzones y fechas de últimos pases
    ///    - Mapeo de estados entre sistemas heterogéneos
    /// 
    /// 3. OPTIMIZACIÓN DE CONSULTAS MASIVAS:
    ///    - Paginación real en base de datos (no en memoria)
    ///    - Carga separada de entidades relacionadas
    ///    - Consultas bulk para datos SIGAF y SADE
    ///    - Disable de features EF innecesarias para performance
    /// 
    /// 4. UNIFICACIÓN DE VISTAS:
    ///    - Combina autorizantes reales + redeterminaciones virtuales
    ///    - Presenta vista única para interfaces de usuario
    ///    - Mantiene consistencia de DTOs entre tipos
    /// 
    /// DISEÑO DE PERFORMANCE:
    /// - Context configurado para consultas masivas (LazyLoading=false, etc.)
    /// - Uso intensivo de AsNoTracking() para consultas de solo lectura
    /// - Carga por separado de tablas relacionadas para evitar JOINs costosos
    /// - Diccionarios en memoria para lookups rápidos
    /// 
    /// MAPEO DE ESTADOS REDETERMINACIONES:
    /// - Estados específicos de redeterminaciones se mapean a estados generales
    /// - Permite visualización unificada en UI con estados comprensibles
    /// - IdsDesestimado, IdsNoIniciado, IdsAprobado definen reglas de mapeo
    /// 
    /// PATRÓN DE INTEGRACIÓN:
    /// - Métodos BuscarMuchos*() para consultas bulk optimizadas
    /// - Métodos Calcular*() para lógica de negocio específica
    /// - Métodos Listar*() para diferentes necesidades de paginación
    /// 
    /// CONSIDERACIONES ESPECIALES:
    /// - _cargandoDatos: Flag para prevenir recursión infinita
    /// - Manejo de grandes volúmenes de datos (miles de autorizantes)
    /// - Compatibilidad con filtros TreeViewSearch y paginación externa
    /// </summary>
    public class CalculoRedeterminacionNegocioEF
    {

        // Desestimado = "35, 36"
        // No iniciado = 37, 38
        // Aprobado = 12, 22, 33, 39
        // En tramite todos los demas

        private readonly int[] idsDesestimado = { 35, 36 }; // Rechazada, Fuera de Plazo
        private readonly int[] idsNoIniciado = { 37, 38 }; // Pendiente, No presentada
        private readonly int[] idsAprobado = { 12, 22, 33, 39 };// RD-11/11-Notificada, RP-09/09-Notificada, RO-11/11-Notificada, ACDIR

        // Estado = new EstadoAutorizante { Id = redet.Etapa.Id, Nombre = redet.Etapa.Nombre }, /////////////////////////////////////////////////////////////////////////////////
        private EstadoAutorizante MapearEstadoRedetAEstadoAutorizante(RedeterminacionEF redet)
        {
            if (idsDesestimado.Contains(redet.Etapa.Id))
                return new EstadoAutorizante { Id = 3, Nombre = "DESESTIMADO" };

            if (idsNoIniciado.Contains(redet.Etapa.Id))
                return new EstadoAutorizante { Id = 4, Nombre = "NO INICIADO" };

            if (idsAprobado.Contains(redet.Etapa.Id))
                return new EstadoAutorizante { Id = 1, Nombre = "APROBADO" };

            return new EstadoAutorizante { Id = 2, Nombre = "EN TRAMITE" };
        }




        /// <summary>
        /// Devuelve todos los autorizantes y redeterminaciones como DTOs, sin paginación ni filtros complejos.
        /// Replica la lógica de ListarCertificadosYReliquidaciones pero para autorizantes y redeterminaciones.
        /// </summary>
        public List<AutorizanteDTO> ListarAutorizantesYRedeterminaciones(UsuarioEF usuario = null)
        {
            var swTotal = Stopwatch.StartNew();
            var autorizantesYRedeterminaciones = new List<AutorizanteDTO>();
            try
            {
                using (var context = new IVCdbContext())
                {
                    context.Configuration.LazyLoadingEnabled = false;
                    context.Configuration.ProxyCreationEnabled = false;
                    context.Configuration.AutoDetectChangesEnabled = false;
                    context.Configuration.ValidateOnSaveEnabled = false;

                    List<AutorizanteEF> autorizantes;
                    if (usuario != null && usuario.Tipo == false) // Usuario normal (no administrador)
                    {
                        var obrasDelArea = context.Obras.AsNoTracking()
                            .Where(o => o.AreaId == usuario.AreaId)
                            .Select(o => o.Id)
                            .ToList();
                        autorizantes = context.Autorizantes.AsNoTracking()
                            .Where(a => obrasDelArea.Contains(a.ObraId))
                            .ToList();
                    }
                    else // Administrador o sin filtro de área
                    {
                        autorizantes = context.Autorizantes.AsNoTracking().ToList();
                    }

                    // Cargar obras y entidades relacionadas
                    var obraIds = autorizantes.Select(a => a.ObraId).Distinct().ToList();
                    var obrasDict = context.Obras.AsNoTracking().Where(o => obraIds.Contains(o.Id)).ToDictionary(o => o.Id);

                    var areaIds = obrasDict.Values.Where(o => o.AreaId.HasValue).Select(o => o.AreaId.Value).Distinct().ToList();
                    var areasDict = context.Areas.AsNoTracking().Where(ar => areaIds.Contains(ar.Id)).ToDictionary(ar => ar.Id);
                    var barrioIds = obrasDict.Values.Where(o => o.BarrioId.HasValue).Select(o => o.BarrioId.Value).Distinct().ToList();
                    var barriosDict = context.Barrios.AsNoTracking().Where(b => barrioIds.Contains(b.Id)).ToDictionary(b => b.Id);
                    var empresaIds = obrasDict.Values.Where(o => o.EmpresaId.HasValue).Select(o => o.EmpresaId.Value).Distinct().ToList();
                    var empresasDict = context.Empresas.AsNoTracking().Where(e => empresaIds.Contains(e.Id)).ToDictionary(e => e.Id);
                    var contrataIds = obrasDict.Values.Where(o => o.ContrataId.HasValue).Select(o => o.ContrataId.Value).Distinct().ToList();
                    var contratasDict = context.Contratas.AsNoTracking().Where(c => contrataIds.Contains(c.Id)).ToDictionary(c => c.Id);

                    var estadoIds = autorizantes.Select(a => a.EstadoId).Distinct().ToList();
                    var estadosDict = context.EstadosAutorizante.AsNoTracking().Where(e => estadoIds.Contains(e.Id)).ToDictionary(e => e.Id);
                    var conceptoIds = autorizantes.Select(a => a.ConceptoId).Distinct().ToList();
                    var conceptosDict = context.Conceptos.AsNoTracking().Where(c => conceptoIds.Contains(c.Id)).ToDictionary(c => c.Id);

                    // Asignar relaciones
                    foreach (var auth in autorizantes)
                    {
                        if (obrasDict.ContainsKey(auth.ObraId))
                        {
                            auth.Obra = obrasDict[auth.ObraId];
                            if (auth.Obra.AreaId.HasValue && areasDict.ContainsKey(auth.Obra.AreaId.Value))
                                auth.Obra.Area = areasDict[auth.Obra.AreaId.Value];
                            if (auth.Obra.BarrioId.HasValue && barriosDict.ContainsKey(auth.Obra.BarrioId.Value))
                                auth.Obra.Barrio = barriosDict[auth.Obra.BarrioId.Value];
                            if (auth.Obra.EmpresaId.HasValue && empresasDict.ContainsKey(auth.Obra.EmpresaId.Value))
                                auth.Obra.Empresa = empresasDict[auth.Obra.EmpresaId.Value];
                            if (auth.Obra.ContrataId.HasValue && contratasDict.ContainsKey(auth.Obra.ContrataId.Value))
                                auth.Obra.Contrata = contratasDict[auth.Obra.ContrataId.Value];

                            auth.Estado = estadosDict[auth.EstadoId];
                            auth.Concepto = conceptosDict[auth.ConceptoId];
                        }
                    }

                    // Cargar redeterminaciones
                    var codigosAutorizante = autorizantes.Select(a => a.CodigoAutorizante).ToList();
                    var redeterminaciones = context.Redeterminaciones.AsNoTracking()
                        .Where(r => codigosAutorizante.Contains(r.CodigoAutorizante))
                        .Include("Etapa")
                        .ToList();

                    // IMPORTANTE: Debemos cargar el autorizante manualmente, ya que las redeterminaciones y los autorizantes se relacionan a traves de CodigoAutorizante.
                    foreach (var redet in redeterminaciones)
                    {
                        var autorizante = autorizantes.FirstOrDefault(a => a.CodigoAutorizante == redet.CodigoAutorizante);
                        if (autorizante != null)
                        {
                            // Asi accedemos a obra, área, barrio, empresa, contrata asignados en autorizante
                            redet.Autorizante = autorizante;
                        }
                    }

                    // Expedientes de autorizantes y redeterminaciones
                    var expedientesValidos = autorizantes
                        .Where(a => !string.IsNullOrEmpty(a.Expediente))
                        .Select(a => a.Expediente)
                        .Union(redeterminaciones
                            .Where(r => !string.IsNullOrEmpty(r.Autorizante?.Expediente))
                            .Select(r => r.Autorizante.Expediente))
                        .Distinct()
                        .ToList();

                    var datosSade = BuscarMuchosDatosSade(expedientesValidos);


                    // Convertir autorizantes a DTO
                    foreach (var auth in autorizantes)
                    {
                        var dto = new AutorizanteDTO
                        {
                            Id = auth.Id,
                            CodigoAutorizante = auth.CodigoAutorizante,
                            ObraId = auth.ObraId,
                            ObraDescripcion = auth.Obra?.Descripcion,
                            AreaId = auth.Obra?.AreaId,
                            AreaNombre = auth.Obra?.Area?.Nombre,
                            BarrioId = auth.Obra?.BarrioId,
                            BarrioNombre = auth.Obra?.Barrio?.Nombre,
                            EmpresaId = auth.Obra?.EmpresaId,
                            EmpresaNombre = auth.Obra?.Empresa?.Nombre,
                            Contrata = $"{auth.Obra?.Contrata?.Nombre} {auth.Obra.Numero}/{auth.Obra.Anio}",
                            EstadoId = auth.EstadoId,
                            EstadoNombre = auth.Estado?.Nombre,
                            Detalle = auth.Detalle,
                            Expediente = auth.Expediente,
                            ConceptoId = auth.ConceptoId,
                            ConceptoNombre = auth.Concepto.Nombre,
                            MontoAutorizado = auth.MontoAutorizado,
                            MesAprobacion = auth.MesAprobacion,
                            MesBase = auth.MesBase,
                            BuzonSade = datosSade.TryGetValue(auth.Expediente, out var sadeInfo) ? sadeInfo.Buzon : null,
                            FechaSade = datosSade.TryGetValue(auth.Expediente, out sadeInfo) ? sadeInfo.Fecha : (DateTime?)null,

                        };
                        autorizantesYRedeterminaciones.Add(dto);
                    }

                    CalcularMontoRedeterminaciones(redeterminaciones, autorizantes);

                    // Convertir redeterminaciones a AutorizanteDTO (con lógica extendida)
                    foreach (var redet in redeterminaciones)
                    {
                        var auth = redet.Autorizante;
                        var obra = auth?.Obra;

                        var estado = MapearEstadoRedetAEstadoAutorizante(redet);
                        var dto = new AutorizanteDTO
                        {
                            IdRedeterminacion = redet.Id,
                            CodigoAutorizante = redet.CodigoRedet,
                            ObraId = obra?.Id,
                            ObraDescripcion = obra?.Descripcion,
                            AreaId = obra?.AreaId,
                            AreaNombre = obra?.Area?.Nombre,
                            BarrioId = obra?.BarrioId,
                            BarrioNombre = obra?.Barrio?.Nombre,
                            EmpresaId = obra?.EmpresaId,
                            EmpresaNombre = obra?.Empresa?.Nombre,
                            Contrata = $"{obra?.Contrata?.Nombre} {obra.Numero}/{obra.Anio}",
                            EstadoId = estado.Id,
                            EstadoNombre = estado.Nombre,
                            Detalle = $"{redet.Tipo} - {redet.Etapa?.Nombre}",
                            Expediente = redet.Expediente,
                            ConceptoId = 11,
                            ConceptoNombre = "REDETERMINACION",
                            MontoAutorizado = redet.MontoRedet ?? 0,
                            MesAprobacion = redet.Salto,
                            MesBase = null,
                            //BuzonSade = (!string.IsNullOrEmpty(redet.Autorizante?.Expediente) && datosSade.ContainsKey(redet.Autorizante.Expediente)) ? datosSade[redet.Autorizante.Expediente].Buzon : null,
                            //FechaSade = (!string.IsNullOrEmpty(redet.Autorizante?.Expediente) && datosSade.ContainsKey(redet.Autorizante.Expediente)) ? datosSade[redet.Autorizante.Expediente].Fecha : (DateTime?)null,

                            BuzonSade = datosSade.TryGetValue(redet.Expediente, out var sadeInfo) ? sadeInfo.Buzon : null,
                            FechaSade = datosSade.TryGetValue(redet.Expediente, out sadeInfo) ? sadeInfo.Fecha : (DateTime?)null,
                        };
                        autorizantesYRedeterminaciones.Add(dto);




                    }

                    Debug.WriteLine($"[ListarAutorizantesYRedeterminaciones] Total DTOs devueltos: {autorizantesYRedeterminaciones.Count}");
                    swTotal.Stop();
                    Debug.WriteLine($"[ListarAutorizantesYRedeterminaciones] Tiempo total: {swTotal.ElapsedMilliseconds} ms");
                    return autorizantesYRedeterminaciones;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en ListarAutorizantesYRedeterminaciones: {ex.Message}");
                return new List<AutorizanteDTO>();
            }
        }

        #region Configuración y Mapeo de Estados

        /// <summary>
        /// Mapeo de IDs específicos de redeterminaciones a estados generales.
        /// 
        /// DISEÑO DE MAPEO:
        /// - Estados específicos de redeterminaciones (BD) -> Estados generales (UI)
        /// - Simplifica visualización y filtrado para usuarios finales
        /// - Mantiene granularidad en BD pero presenta vista consolidada
        /// 
        /// CATEGORÍAS DE ESTADOS:
        /// - DESESTIMADO: Rechazada (35), Fuera de Plazo (36)
        /// - NO INICIADO: Pendiente (37), No presentada (38)  
        /// - APROBADO: RD-11/11-Notificada (12), RP-09/09-Notificada (22), etc.
        /// - EN TRAMITE: Todos los demás estados no categorizados arriba
        /// </summary>

        // Desestimado = "35, 36"
        // No iniciado = 37, 38
        // Aprobado = 12, 22, 33, 39
        // En tramite todos los demas

        private readonly int[] IdsDesestimado = { 35, 36 }; // Rechazada, Fuera de Plazo
        private readonly int[] IdsNoIniciado = { 37, 38 }; // Pendiente, No presentada
        private readonly int[] IdsAprobado = { 12, 22, 33, 39 };// RD-11/11-Notificada, RP-09/09-Notificada, RO-11/11-Notificada, ACDIR

        /// <summary>
        /// Flag de protección contra recursión infinita en carga de datos.
        /// 
        /// PROBLEMÁTICA:
        /// - Algunos métodos de cálculo pueden disparar otros métodos de carga
        /// - Sin protección, se pueden generar bucles infinitos
        /// - Especialmente crítico en ListarCertificadosYReliquidaciones()
        /// 
        /// USO:
        /// - Se establece en true al iniciar operaciones críticas
        /// - Se verifica al inicio de métodos susceptibles a recursión
        /// - Se resetea en finally para garantizar limpieza
        /// </summary>
        private static bool _cargandoDatos = false;

        /// <summary>
        /// Obtiene el cache de SADE desde la sesión HTTP
        /// </summary>
        private Dictionary<string, (string Buzon, DateTime? Fecha)> ObtenerCacheSade()
        {
            if (HttpContext.Current?.Session == null) return null;
            return HttpContext.Current.Session["CacheSade"] as Dictionary<string, (string, DateTime?)>;
        }

        /// <summary>
        /// Guarda el cache de SADE en la sesión HTTP
        /// </summary>
        private void GuardarCacheSade(Dictionary<string, (string Buzon, DateTime? Fecha)> cache)
        {
            if (HttpContext.Current?.Session != null)
            {
                HttpContext.Current.Session["CacheSade"] = cache;
            }
        }

        /// <summary>
        /// Mapea un ID específico de estado de redeterminación a un estado general.
        /// 
        /// LÓGICA DE MAPEO:
        /// 1. null o IdsNoIniciado -> "NO INICIADO" (Id=4)
        /// 2. IdsAprobado -> "APROBADO" (Id=1)  
        /// 3. IdsDesestimado -> "DESESTIMADO" (Id=3)
        /// 4. Cualquier otro -> "EN TRAMITE" (Id=2)
        /// 
        /// PARÁMETROS:
        /// - estadoRedetId: ID específico de estado de redeterminación (puede ser null)
        /// 
        /// RETORNO:
        /// - EstadoAutorizanteEF con ID y Nombre del estado general mapeado
        /// 
        /// USO:
        /// - Convertir estados granulares de redeterminaciones a vista unificada
        /// - Consistencia entre autorizantes y redeterminaciones en UI
        /// - Filtros y agrupaciones por estado general
        /// </summary>
        public EstadoAutorizanteEF MapearEstadoDesdeId(int? estadoRedetId)
        {
            //int? estadoRedetId = dto.EstadoRedetId;

            if (!estadoRedetId.HasValue || IdsNoIniciado.Contains(estadoRedetId.Value))
                return new EstadoAutorizanteEF { Id = 4, Nombre = "NO INICIADO" };

            if (IdsAprobado.Contains(estadoRedetId.Value))
                return new EstadoAutorizanteEF { Id = 1, Nombre = "APROBADO" };

            if (IdsDesestimado.Contains(estadoRedetId.Value))
                return new EstadoAutorizanteEF { Id = 3, Nombre = "DESESTIMADO" };

            return new EstadoAutorizanteEF { Id = 2, Nombre = "EN TRAMITE" };
        }

        /// <summary>
        /// Genera lista de estados únicos para uso en filtros TreeViewSearch.
        /// 
        /// PROPÓSITO:
        /// - Proporcionar opciones de filtrado consistentes en UI
        /// - Evitar duplicados en listas de selección
        /// - Ordenar alfabéticamente para mejor UX
        /// 
        /// IMPLEMENTACIÓN:
        /// - Utiliza MapearEstadoDesdeId() con IDs representativos
        /// - Genera un estado de cada categoría principal
        /// - OrderBy para ordenamiento alfabético
        /// 
        /// RETORNO:
        /// - Lista de EstadoAutorizanteEF únicos y ordenados
        /// - Incluye: APROBADO, DESESTIMADO, EN TRAMITE, NO INICIADO
        /// 
        /// USO:
        /// - Poblar filtros TreeViewSearch de estado
        /// - Dropdowns de selección de estado
        /// - Interfaces de búsqueda y reportes
        /// </summary>
        public List<EstadoAutorizanteEF> ObtenerEstadosParaFiltro()
        {
            return new List<EstadoAutorizanteEF>
            {
                MapearEstadoDesdeId(12), // APROBADO
                MapearEstadoDesdeId(35), // DESESTIMADO
                MapearEstadoDesdeId(37), // NO INICIADO
                MapearEstadoDesdeId(0)   // EN TRAMITE
            }.OrderBy(e => e.Nombre).ToList();
        }

        /// <summary>
        /// NUEVO MÉTODO: Réplica de la lógica de negocio original que genera reliquidaciones.
        /// Optimiza la carga inicial de datos y luego procesa la lógica en memoria.
        /// </summary>
        /// <param name="usuario">Filtra los resultados por el área del usuario, si se proporciona.</param>
        /// <returns>Una lista de DTOs que incluye Certificados originales y Reliquidaciones generadas.</returns>
        public List<CertificadoDTO> ListarCertificadosYReliquidaciones(UsuarioEF usuario = null)
        {
            if (_cargandoDatos)
            {
                Debug.WriteLine("Bucle detectado en ListarCertificadosYReliquidaciones");
                return new List<CertificadoDTO>();
            }

            var swTotal = Stopwatch.StartNew();
            var sw = Stopwatch.StartNew();

            try
            {
                _cargandoDatos = true;

                using (var context = new IVCdbContext())
                {
                    context.Configuration.LazyLoadingEnabled = false;
                    context.Configuration.ProxyCreationEnabled = false;
                    context.Configuration.AutoDetectChangesEnabled = false;
                    context.Configuration.ValidateOnSaveEnabled = false;

                    // OPTIMIZACIÓN CRÍTICA AUTORIZANTES: Eliminar JOIN costoso y usar consultas separadas
                    sw.Restart();

                    List<AutorizanteEF> autorizantesBase;

                    if (usuario != null && usuario.Tipo == false) // Usuario normal (no administrador)
                    {
                        // OPTIMIZACIÓN: Consulta en dos pasos para evitar JOIN costoso sin índices
                        // Paso 1: Obtener IDs de obras del área específica (~20ms)
                        var obrasDelArea = context.Obras.AsNoTracking()
                            .Where(o => o.AreaId == usuario.AreaId)
                            .Select(o => o.Id)
                            .ToList();
                        Debug.WriteLine($"Tiempo obras del área {usuario.AreaId} ({obrasDelArea.Count} obras): {sw.ElapsedMilliseconds} ms");

                        // Paso 2: Filtrar autorizantes usando Contains con la lista de IDs (~50ms)
                        sw.Restart();
                        autorizantesBase = context.Autorizantes.AsNoTracking()
                            .Where(a => obrasDelArea.Contains(a.ObraId))
                            .ToList();
                        Debug.WriteLine($"Tiempo autorizantes filtrados por área ({autorizantesBase.Count} autorizantes): {sw.ElapsedMilliseconds} ms");
                    }
                    else // Administrador o sin filtro de área
                    {
                        // Sin filtro de área: cargar todos los autorizantes
                        autorizantesBase = context.Autorizantes.AsNoTracking().ToList();
                        Debug.WriteLine($"Tiempo autorizantes SIN filtro de área ({autorizantesBase.Count} autorizantes): {sw.ElapsedMilliseconds} ms");
                    }

                    // 2. Cargar obras por separado (más eficiente que Include)
                    sw.Restart();
                    var obraIdsNecesarias = autorizantesBase.Select(a => a.ObraId).Distinct().ToList();
                    var obrasDict = context.Obras.AsNoTracking()
                        .Where(o => obraIdsNecesarias.Contains(o.Id))
                        .ToDictionary(o => o.Id);

                    // Asignar obras a autorizantes
                    foreach (var auth in autorizantesBase)
                    {
                        if (obrasDict.ContainsKey(auth.ObraId))
                            auth.Obra = obrasDict[auth.ObraId];
                    }
                    Debug.WriteLine($"Tiempo carga obras separada: {sw.ElapsedMilliseconds} ms");

                    // 3. Cargar datos relacionados en consultas separadas (más eficiente que JOINs masivos)
                    sw.Restart();
                    var empresaIds = autorizantesBase.Where(a => a.Obra?.EmpresaId != null).Select(a => a.Obra.EmpresaId.Value).Distinct().ToList();
                    var areaIds = obrasDict.Values.Where(o => o.AreaId.HasValue).Select(o => o.AreaId.Value).Distinct().ToList();
                    var barrioIds = autorizantesBase.Where(a => a.Obra?.BarrioId != null).Select(a => a.Obra.BarrioId.Value).Distinct().ToList();
                    var contrataIds = autorizantesBase.Where(a => a.Obra?.ContrataId != null).Select(a => a.Obra.ContrataId.Value).Distinct().ToList();
                    var obraIds = autorizantesBase.Where(a => a.Obra != null).Select(a => a.Obra.Id).Distinct().ToList();

                    var empresasDict = context.Empresas.AsNoTracking()
                        .Where(e => empresaIds.Contains(e.Id))
                        .ToDictionary(e => e.Id);

                    var areasDict = context.Areas.AsNoTracking()
                        .Where(ar => areaIds.Contains(ar.Id))
                        .ToDictionary(ar => ar.Id);

                    var barriosDict = context.Barrios.AsNoTracking()
                        .Where(b => barrioIds.Contains(b.Id))
                        .ToDictionary(b => b.Id);

                    var proyectosDict = context.Proyectos.AsNoTracking()
                        .Include(p => p.LineaGestionEF)
                        .Where(p => obraIds.Contains(p.ObraId))
                        .ToDictionary(p => p.ObraId);

                    var contratasDict = context.Contratas.AsNoTracking()
                        .Where(c => contrataIds.Contains(c.Id))
                        .ToDictionary(c => c.Id);
                    Debug.WriteLine($"Tiempo datos relacionados: {sw.ElapsedMilliseconds} ms");

                    // 4. Redeterminaciones optimizadas (carga simple sin Include costoso de Etapa)
                    sw.Restart();
                    var codigosAutorizanteBase = autorizantesBase.Select(a => a.CodigoAutorizante).ToList();

                    // Cargar redeterminaciones directamente (sin Include costoso)
                    var redeterminacionesList = context.Redeterminaciones.AsNoTracking()
                        .Where(r => codigosAutorizanteBase.Contains(r.CodigoAutorizante))
                        .ToList();

                    var redeterminacionesDict = redeterminacionesList.GroupBy(r => r.CodigoAutorizante)
                        .ToDictionary(g => g.Key, g => g.ToList());

                    Debug.WriteLine($"Tiempo redeterminaciones OPTIMIZADAS: {sw.ElapsedMilliseconds} ms");

                    // 5. Ensamblar estructura final en memoria (súper rápido)
                    sw.Restart();
                    foreach (var auth in autorizantesBase)
                    {
                        if (auth.Obra != null)
                        {
                            // Asignar relaciones desde diccionarios (O(1) lookup)
                            if (auth.Obra.EmpresaId.HasValue && empresasDict.ContainsKey(auth.Obra.EmpresaId.Value))
                                auth.Obra.Empresa = empresasDict[auth.Obra.EmpresaId.Value];

                            if (auth.Obra.AreaId.HasValue && areasDict.ContainsKey(auth.Obra.AreaId.Value))
                                auth.Obra.Area = areasDict[auth.Obra.AreaId.Value];

                            if (auth.Obra.BarrioId.HasValue && barriosDict.ContainsKey(auth.Obra.BarrioId.Value))
                                auth.Obra.Barrio = barriosDict[auth.Obra.BarrioId.Value];

                            if (proyectosDict.ContainsKey(auth.Obra.Id))
                                auth.Obra.Proyecto = proyectosDict[auth.Obra.Id];

                            if (auth.Obra.ContrataId.HasValue && contratasDict.ContainsKey(auth.Obra.ContrataId.Value))
                                auth.Obra.Contrata = contratasDict[auth.Obra.ContrataId.Value];
                        }

                        // Asignar redeterminaciones desde diccionario
                        auth.Redeterminaciones = redeterminacionesDict.ContainsKey(auth.CodigoAutorizante)
                            ? redeterminacionesDict[auth.CodigoAutorizante]
                            : new List<RedeterminacionEF>();
                    }
                    Debug.WriteLine($"Tiempo ensamblado en memoria: {sw.ElapsedMilliseconds} ms");

                    var autorizantes = autorizantesBase; // Resultado final optimizado
                    Debug.WriteLine($"Tiempo total consulta autorizantes OPTIMIZADA: {sw.ElapsedMilliseconds} ms");

                    sw.Restart();
                    var codigosAutorizante = autorizantes.Select(a => a.CodigoAutorizante).ToList();
                    var certificados = context.Certificados.AsNoTracking()
                                            .Include(c => c.TipoPago)
                                            .Where(c => codigosAutorizante.Contains(c.CodigoAutorizante))
                                            .ToList();
                    Debug.WriteLine($"Tiempo consulta certificados: {sw.ElapsedMilliseconds} ms");

                    sw.Restart();
                    var redeterminaciones = redeterminacionesList; // Ya están cargadas desde la optimización anterior
                    Debug.WriteLine($"Tiempo consulta redeterminaciones: {sw.ElapsedMilliseconds} ms");

                    sw.Restart();
                    var expedientesReliq = context.ExpedientesReliq.AsNoTracking().ToList()
                            .ToDictionary(
                                e => new Tuple<string, int, int>(e.CodigoRedet, e.MesAprobacion.Month, e.MesAprobacion.Year),
                                e => e.Expediente
                            );
                    Debug.WriteLine($"Tiempo consulta expedientesReliq: {sw.ElapsedMilliseconds} ms");

                    // OPTIMIZACIÓN CONSERVADORA: Solo paralelismo en consultas SIGAF/SADE
                    sw.Restart();
                    var todosLosExpedientes = certificados.Select(c => c.ExpedientePago)
                                                         .Concat(redeterminaciones.Select(r => r.Expediente))
                                                         .Where(e => !string.IsNullOrWhiteSpace(e))
                                                         .Distinct()
                                                         .ToList();
                    Debug.WriteLine($"Tiempo procesamiento todosLosExpedientes: {sw.ElapsedMilliseconds} ms");

                    // PARALELISMO CONSERVADOR: Solo SIGAF y SADE en paralelo
                    sw.Restart();
                    Dictionary<string, decimal> datosSigaf = null;
                    Dictionary<string, (string Buzon, DateTime? Fecha)> datosSade = null;

                    // Ejecutar ambas consultas en paralelo para máximo rendimiento
                    var taskSigaf = Task.Run(() => BuscarMuchosNumerosSigaf(todosLosExpedientes));
                    var taskSade = Task.Run(() => BuscarMuchosDatosSade(todosLosExpedientes));

                    // Esperar a que ambas terminen
                    Task.WaitAll(taskSigaf, taskSade);

                    datosSigaf = taskSigaf.Result;
                    datosSade = taskSade.Result;

                    Debug.WriteLine($"Tiempo consulta SIGAF/SADE EN PARALELO: {sw.ElapsedMilliseconds} ms");

                    sw.Restart();
                    CalcularMontoRedeterminaciones(redeterminaciones, autorizantes, certificados);
                    Debug.WriteLine($"Tiempo CalcularMontoRedeterminaciones: {sw.ElapsedMilliseconds} ms");

                    // OPTIMIZACIÓN 3: Proyección optimizada con let statements
                    sw.Restart();
                    var listaFinalDTO = (from cert in certificados
                                         join auth in autorizantes on cert.CodigoAutorizante equals auth.CodigoAutorizante
                                         let obra = auth.Obra
                                         let empresa = obra?.Empresa
                                         let area = obra?.Area
                                         let barrio = obra?.Barrio
                                         let proyecto = obra?.Proyecto
                                         let lineaGestion = proyecto?.LineaGestionEF
                                         let contrata = obra?.Contrata
                                         let tipoPago = cert.TipoPago
                                         select new CertificadoDTO
                                         {
                                             Id = cert.Id,
                                             ExpedientePago = cert.ExpedientePago,
                                             MontoTotal = cert.MontoTotal,
                                             MesAprobacion = cert.MesAprobacion,
                                             AutorizanteId = auth.Id,
                                             CodigoAutorizante = cert.CodigoAutorizante,

                                             ObraId = obra?.Id,
                                             ObraDescripcion = obra?.Descripcion,
                                             EmpresaId = obra?.EmpresaId,
                                             EmpresaNombre = empresa?.Nombre,
                                             AreaId = obra?.AreaId,
                                             AreaNombre = area?.Nombre,
                                             Contrata = contrata != null && obra != null ?
                                                       string.Concat(contrata.Nombre, " ", obra.Numero, "/", obra.Anio) : null,
                                             BarrioId = obra?.BarrioId,
                                             BarrioNombre = barrio?.Nombre,
                                             ProyectoId = proyecto?.Id,
                                             ProyectoNombre = proyecto?.Nombre,
                                             LineaGestionId = lineaGestion?.Id,
                                             LineaGestionNombre = lineaGestion?.Nombre,
                                             TipoPagoId = cert.TipoPagoId,
                                             TipoPagoNombre = tipoPago?.Nombre,
                                             EstadoRedetId = auth.Redeterminaciones?.OrderByDescending(r => r.Id).Select(r => (int?)r.EstadoRedetEFId).FirstOrDefault(),
                                             Porcentaje = (auth.MontoAutorizado > 0) ? (cert.MontoTotal / auth.MontoAutorizado) * 100 : 0,
                                         }).ToList();

                    Debug.WriteLine($"Tiempo proyección inicial OPTIMIZADA: {sw.ElapsedMilliseconds} ms");

                    // OPTIMIZACIÓN CONSERVADORA: Generar reliquidaciones con lookup optimizado
                    sw.Restart();
                    var listaReliqDTO = new List<CertificadoDTO>();
                    int idReliq = 0;

                    // Crear diccionario para lookup más rápido de autorizantes por redeterminación
                    var autorizantesPorRedet = new Dictionary<int, AutorizanteEF>();
                    foreach (var auth in autorizantes)
                    {
                        foreach (var redet in auth.Redeterminaciones)
                        {
                            autorizantesPorRedet[redet.Id] = auth;
                        }
                    }

                    foreach (var redet in redeterminaciones)
                    {
                        if (!autorizantesPorRedet.TryGetValue(redet.Id, out var autorizanteDeRedet)) continue;

                        var certificadosAfectados = listaFinalDTO.Where(c => c.CodigoAutorizante == autorizanteDeRedet.CodigoAutorizante && c.MesAprobacion > redet.Salto);

                        foreach (var certAfectado in certificadosAfectados)
                        {
                            decimal montoReliq = (redet.MontoRedet ?? 0) * ((certAfectado.Porcentaje ?? 0) / 100);

                            var claveReliq = new Tuple<string, int, int>(redet.CodigoRedet, certAfectado.MesAprobacion.Value.Month, certAfectado.MesAprobacion.Value.Year);

                            expedientesReliq.TryGetValue(claveReliq, out string expedienteFinalReliq);

                            // CORRECCIÓN: La reliquidación debe tener los datos del autorizante de la redeterminación, no del certificado afectado
                            var obraRedet = autorizanteDeRedet.Obra;
                            var proyectoRedet = obraRedet?.Proyecto;
                            var empresaRedet = obraRedet?.Empresa;
                            var areaRedet = obraRedet?.Area;
                            var barrioRedet = obraRedet?.Barrio;
                            var contrataRedet = obraRedet?.Contrata;
                            var lineaGestionRedet = proyectoRedet?.LineaGestionEF;

                            listaReliqDTO.Add(new CertificadoDTO
                            {
                                IdReliquidacion = ++idReliq,
                                ExpedientePago = expedienteFinalReliq,
                                MesAprobacion = certAfectado.MesAprobacion,
                                MontoTotal = montoReliq,
                                EstadoRedetId = redet.EstadoRedetEFId,
                                AutorizanteId = autorizanteDeRedet.Id, // ID del autorizante de la redeterminación
                                CodigoAutorizante = redet.CodigoRedet,
                                // Datos de la obra de la redeterminación (no del certificado afectado)
                                ObraId = obraRedet?.Id,
                                ObraDescripcion = obraRedet?.Descripcion,
                                EmpresaId = obraRedet?.EmpresaId,
                                EmpresaNombre = empresaRedet?.Nombre,
                                AreaId = obraRedet?.AreaId,
                                AreaNombre = areaRedet?.Nombre,
                                Contrata = contrataRedet != null && obraRedet != null ?
                                          string.Concat(contrataRedet.Nombre, " ", obraRedet.Numero, "/", obraRedet.Anio) : null,
                                BarrioId = obraRedet?.BarrioId,
                                BarrioNombre = barrioRedet?.Nombre,
                                // CORRECCIÓN PRINCIPAL: ProyectoId de la redeterminación, no del certificado afectado
                                ProyectoId = proyectoRedet?.Id,
                                ProyectoNombre = proyectoRedet?.Nombre,
                                LineaGestionId = lineaGestionRedet?.Id,
                                LineaGestionNombre = lineaGestionRedet?.Nombre,
                                TipoPagoId = 3,
                                TipoPagoNombre = "RELIQUIDACION",
                                Porcentaje = certAfectado.Porcentaje, // El porcentaje sí viene del certificado afectado
                            });
                        }
                    }

                    Debug.WriteLine($"Tiempo generación de Reliquidaciones: {sw.ElapsedMilliseconds} ms");
                    listaFinalDTO.AddRange(listaReliqDTO);

                    // OPTIMIZACIÓN CONSERVADORA: Reutilizar datos SIGAF/SADE ya cargados
                    sw.Restart();
                    var expedientesConCertificados = listaFinalDTO
                        .Where(c => !string.IsNullOrWhiteSpace(c.ExpedientePago))
                        .Select(c => c.ExpedientePago)
                        .Distinct()
                        .ToList();

                    // PASAMOS los datos ya cargados para evitar reconsultas
                    var resultado = CalcularCertificadosPorExpedientes(
                        expedientesConCertificados,
                        listaFinalDTO,
                        false,
                        datosSigaf,  // Reutilizar datos ya cargados
                        datosSade);  // Reutilizar datos ya cargados
                    Debug.WriteLine($"Tiempo CalcularCertificadosPorExpedientes OPTIMIZADO: {sw.ElapsedMilliseconds} ms");

                    swTotal.Stop();
                    Debug.WriteLine($"Tiempo total ListarCertificadosYReliquidaciones OPTIMIZADO: {swTotal.ElapsedMilliseconds} ms");

                    Debug.WriteLine($"Cantidad de listaFinalDTO: {listaFinalDTO.Count}");
                    return resultado;
                }


            }
            finally
            {
                _cargandoDatos = false;
            }
        }

        /// <summary>
        /// Método reutilizable que recalcula todos los certificados asociados a los expedientes especificados
        /// </summary>
        /// <param name="expedientes">Lista de expedientes a recalcular (anterior y nuevo)</param>
        /// <param name="todosLosCertificados">Lista de certificados que podrían verse afectados</param>
        /// <param name="persistirEnBD">Indica si se deben guardar los cambios en la base de datos</param>
        /// <param name="datosSigafPrecargados">Datos SIGAF ya cargados para evitar consultas duplicadas</param>
        /// <param name="datosSadePrecargados">Datos SADE ya cargados para evitar consultas duplicadas</param>
        /// <returns>Lista de certificados actualizados</returns>
        public List<CertificadoDTO> CalcularCertificadosPorExpedientes(
            List<string> expedientes,
            List<CertificadoDTO> todosLosCertificados,
            bool persistirEnBD = false,
            Dictionary<string, decimal> datosSigafPrecargados = null,
            Dictionary<string, (string Buzon, DateTime? Fecha)> datosSadePrecargados = null)
        {
            try
            {
                // Filtrar solo expedientes válidos
                var expedientesValidos = expedientes
                    .Where(e => !string.IsNullOrWhiteSpace(e))
                    .Distinct()
                    .ToList();

                //if (!expedientesValidos.Any() && certificados.All(c => !c.ExpedientePago))
                //    return certificados; // Nada que hacer si no hay expedientes para procesar

                // OPTIMIZACIÓN: Usar datos precargados si están disponibles, sino cargar
                var datosSigaf = datosSigafPrecargados ?? BuscarMuchosNumerosSigaf(expedientesValidos);
                var datosSade = datosSadePrecargados ?? BuscarMuchosDatosSade(expedientesValidos);

                Debug.WriteLine($"Datos SIGAF/SADE: {(datosSigafPrecargados != null ? "REUTILIZADOS" : "CARGADOS")} - SIGAF: {datosSigaf.Count}, SADE: {datosSade.Count}");

                // Para cada expediente, recalcular todos los certificados asociados
                foreach (var expediente in expedientesValidos)
                {
                    // Obtener certificados con este expediente
                    var certificadosDelExpediente = todosLosCertificados
                        .Where(c => string.Equals(c.ExpedientePago, expediente, StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    if (!certificadosDelExpediente.Any())
                        continue;

                    // Recalcular valores de SIGAF para todo el expediente
                    foreach (var cert in certificadosDelExpediente)
                    {
                        // Calcular SIGAF compartido
                        cert.Sigaf = CalcularSIGAF(cert.ExpedientePago, cert.MontoTotal, todosLosCertificados, datosSigaf);

                        // Calcular propiedades adicionales
                        //var autorizante = autorizantes.FirstOrDefault(a => a.CodigoAutorizante == cert.CodigoAutorizante);

                        // PASARLE TAMBIEN cert.Sigaf y no datosSigaf
                        var calculosAdicionales = CalcularPropiedadesAdicionales(
                            cert.ExpedientePago,
                            cert.MontoTotal,
                            cert.Sigaf,
                            datosSade);

                        // Actualizar propiedades
                        cert.Estado = calculosAdicionales.Estado;
                        cert.BuzonSade = calculosAdicionales.BuzonSade;
                        cert.FechaSade = calculosAdicionales.FechaSade;
                    }
                }

                // Actualizamos también todos los certificados que no tengan expediente
                foreach (var cert in todosLosCertificados.Where(c => string.IsNullOrWhiteSpace(c.ExpedientePago)))
                {
                    cert.Estado = "NO INICIADO";
                    cert.BuzonSade = null;
                    cert.FechaSade = null;
                    cert.Sigaf = null;
                }


                // Persistir cambios en la base de datos si es necesario
                if (persistirEnBD)
                {
                    GuardarCambiosEnBaseDeDatos(todosLosCertificados);
                }

                return todosLosCertificados;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en RecalcularCertificadosPorExpedientes: {ex.Message}");
                return todosLosCertificados; // Devolvemos la lista original sin cambios en caso de error
            }
        }

        // Método auxiliar para guardar cambios en la base de datos
        private void GuardarCambiosEnBaseDeDatos(List<CertificadoDTO> certificados)
        {
            try
            {
                using (var context = new IVCdbContext())
                {
                    // Configuración para mejorar el rendimiento de EF
                    context.Configuration.AutoDetectChangesEnabled = true;
                    context.Configuration.ValidateOnSaveEnabled = false;

                    using (var transaction = context.Database.BeginTransaction())
                    {
                        try
                        {
                            int cambiosEsperados = 0;

                            // Certificados normales (no reliquidaciones)
                            var certificadosNormales = certificados
                                .Where(c => c.TipoPagoId != 3 && c.Id > 0)
                                .ToList();

                            foreach (var certDTO in certificadosNormales)
                            {
                                // Usar AsNoTracking y luego adjuntar para evitar problemas de caché
                                var cert = context.Certificados.Find(certDTO.Id);
                                if (cert != null && cert.ExpedientePago != certDTO.ExpedientePago)
                                {
                                    // Guardar expediente anterior para log
                                    string expedienteAnterior = cert.ExpedientePago;

                                    // Actualizar y marcar explícitamente como modificado
                                    cert.ExpedientePago = certDTO.ExpedientePago;
                                    context.Entry(cert).Property(c => c.ExpedientePago).IsModified = true;

                                    System.Diagnostics.Debug.WriteLine($"Certificado {cert.Id} modificado: '{expedienteAnterior}' → '{cert.ExpedientePago}'");
                                    cambiosEsperados++;
                                }
                            }

                            // Reliquidaciones
                            var reliquidaciones = certificados
                                .Where(c => c.TipoPagoId == 3 && c.MesAprobacion.HasValue)
                                .ToList();

                            foreach (var reliqDTO in reliquidaciones)
                            {
                                // Validar datos obligatorios
                                if (string.IsNullOrEmpty(reliqDTO.CodigoAutorizante) || !reliqDTO.MesAprobacion.HasValue)
                                    continue;

                                // Buscar usando LINQ en lugar de SQL directo
                                var reliq = context.ExpedientesReliq
                                    .Where(e =>
                                        e.CodigoRedet == reliqDTO.CodigoAutorizante &&
                                        e.MesAprobacion.Year == reliqDTO.MesAprobacion.Value.Year &&
                                        e.MesAprobacion.Month == reliqDTO.MesAprobacion.Value.Month)
                                    .FirstOrDefault();

                                if (reliq != null && reliq.Expediente != reliqDTO.ExpedientePago)
                                {
                                    // Guardar expediente anterior para log
                                    string expedienteAnterior = reliq.Expediente;

                                    // Actualizar y marcar explícitamente como modificado
                                    reliq.Expediente = reliqDTO.ExpedientePago;
                                    context.Entry(reliq).Property(r => r.Expediente).IsModified = true;

                                    cambiosEsperados++;
                                }
                                else if (reliq == null && !string.IsNullOrWhiteSpace(reliqDTO.ExpedientePago))
                                {
                                    // Crear nueva reliquidación
                                    var nuevaReliq = new ExpedienteReliqEF
                                    {
                                        CodigoRedet = reliqDTO.CodigoAutorizante,
                                        MesAprobacion = reliqDTO.MesAprobacion.Value,
                                        Expediente = reliqDTO.ExpedientePago
                                    };

                                    context.ExpedientesReliq.Add(nuevaReliq);
                                    cambiosEsperados++;
                                }
                            }

                            // Forzar la detección de cambios antes de guardar
                            context.ChangeTracker.DetectChanges();

                            // Guardar cambios y confirmar transacción
                            var totalCambios = context.SaveChanges();
                            transaction.Commit();

                            System.Diagnostics.Debug.WriteLine($"Total de cambios guardados: {totalCambios} (esperados: {cambiosEsperados})");
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            System.Diagnostics.Debug.WriteLine($"ERROR: Transacción revertida: {ex.Message}");
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR en GuardarCambiosEnBaseDeDatos: {ex.Message}");
                if (ex.InnerException != null)
                    System.Diagnostics.Debug.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                throw;
            }
        }


        /// <summary>
        /// Calcula SIGAF directamente usando los datos en memoria sin causar bucles infinitos
        /// Replica la lógica de SIGAFHelper.CalcularSIGAFCompartido pero sin dependencias circulares
        /// </summary>
        private decimal? CalcularSIGAF(string expedientePago, decimal montoTotal, List<CertificadoDTO> todosLosCertificados, Dictionary<string, decimal> datosSigaf)
        {
            try
            {
                // Validaciones robustas
                if (string.IsNullOrWhiteSpace(expedientePago) || datosSigaf == null || !datosSigaf.ContainsKey(expedientePago))
                    return null;

                decimal totalDevengado = datosSigaf[expedientePago];
                if (totalDevengado <= 0)
                    return null;

                // Filtrar certificados válidos del mismo expediente
                var certificadosMismoExpediente = todosLosCertificados
                    ?.Where(c => !string.IsNullOrWhiteSpace(c.ExpedientePago) &&
                          c.ExpedientePago.Equals(expedientePago, StringComparison.OrdinalIgnoreCase))
                    ?.ToList() ?? new List<CertificadoDTO>();

                if (!certificadosMismoExpediente.Any())
                    return totalDevengado;

                if (certificadosMismoExpediente.Count == 1)
                    return totalDevengado;

                decimal sumaTotalMontos = certificadosMismoExpediente.Sum(c => c.MontoTotal);
                if (sumaTotalMontos > 0)
                    return totalDevengado * montoTotal / sumaTotalMontos;

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en CalcularSigafDirecto: {ex.Message}");
                return null;
            }
        }

        private void CalcularMontoRedeterminaciones(List<RedeterminacionEF> redeterminaciones, List<AutorizanteEF> autorizantes, List<CertificadoEF> certificados = null)
        {
            var redeterminacionesCalculadas = new List<RedeterminacionEF>();

            foreach (var redet in redeterminaciones.OrderBy(r => r.Nro))
            {
                var autorizante = autorizantes.FirstOrDefault(a => a.CodigoAutorizante == redet.CodigoAutorizante);
                if (autorizante == null || !redet.Salto.HasValue || !redet.Porcentaje.HasValue) continue;



                if (certificados != null)
                {
                    certificados = certificados
                        .Where(c => c.CodigoAutorizante == autorizante.CodigoAutorizante && c.MesAprobacion < redet.Salto)
                        .ToList();
                }
                else
                {
                    using (IVCdbContext context = new IVCdbContext())
                    {
                        certificados = context.Certificados
                        .Where(c => c.CodigoAutorizante == autorizante.CodigoAutorizante && c.MesAprobacion < redet.Salto)
                        .ToList();
                    }
                }
                decimal sumaMontosTotal = certificados.Sum(c => c.MontoTotal);
                decimal porcentajeUtilizado = (autorizante.MontoAutorizado > 0) ? (sumaMontosTotal / autorizante.MontoAutorizado) * 100 : 0;
                decimal faltante = Math.Max(0, 100 - porcentajeUtilizado);
                decimal montoCalculado = 0;

                if (redet.Nro == 1)
                {
                    montoCalculado = ((autorizante.MontoAutorizado * redet.Porcentaje.Value) / 100) * (faltante / 100);
                }
                else
                {
                    var redeterminacionesAnteriores = redeterminacionesCalculadas
                        .Where(r => r.CodigoAutorizante == redet.CodigoAutorizante && r.Nro < redet.Nro && r.MontoRedet.HasValue)
                        .ToList();

                    decimal sumaMontosRedetAnteriores = redeterminacionesAnteriores.Sum(r => r.MontoRedet.Value);
                    montoCalculado = (((autorizante.MontoAutorizado + sumaMontosRedetAnteriores) * redet.Porcentaje.Value) / 100) * (faltante / 100);
                }

                redet.MontoRedet = montoCalculado;
                redeterminacionesCalculadas.Add(redet);
            }
        }

        /// <summary>
        /// Calcula las propiedades adicionales para un certificado (Porcentaje, Estado, Sigaf, BuzonSade, FechaSade)
        /// </summary>
        public (string Estado, string BuzonSade, DateTime? FechaSade) CalcularPropiedadesAdicionales(string expedientePago, decimal? montoTotal, decimal? sigaf, Dictionary<string, (string Buzon, DateTime? Fecha)> datosSade)
        {
            try
            {
                // 3. Estado
                string estado;
                if (string.IsNullOrEmpty(expedientePago))
                {
                    estado = "NO INICIADO";
                }
                else if (sigaf > 0)
                {
                    estado = "DEVENGADO";
                }
                else
                {
                    estado = "EN TRAMITE";
                }

                // 4. Datos SADE (consulta en memoria)
                var (buzon, fecha) = datosSade.ContainsKey(expedientePago) ? datosSade[expedientePago] : (null, null);

                return (estado, buzon, fecha);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en RealizarCalculosAdicionales: {ex.Message}");
                return ("ERROR", null, null);
            }
        }



        /// <summary>
        /// Consulta masiva optimizada para obtener importes SIGAF de múltiples expedientes.
        /// 
        /// PROPÓSITO:
        /// - Reemplaza múltiples llamadas individuales a SIGAFHelper.ObtenerTotalImporteDevengados
        /// - Obtiene suma de importes devengados por expediente en una sola consulta
        /// - Mejora drasticamente performance en procesamiento de lotes grandes
        /// 
        /// LÓGICA DE CONSULTA:
        /// 1. WHERE expedientes.Contains(d.EeFinanciera) - Filtro por lista de expedientes
        /// 2. GroupBy(d.EeFinanciera) - Agrupa devengados por expediente
        /// 3. Sum(d.ImportePp) - Suma importes por grupo (expediente)
        /// 4. ToDictionary() - Convierte a diccionario para lookups O(1)
        /// 
        /// EQUIVALENCIA SQL:
        /// Para cada expediente, replica:
        /// SELECT EeFinanciera, SUM(ISNULL(ImportePp, 0))
        /// FROM Devengados
        /// WHERE EeFinanciera IN (@expedientes)
        /// GROUP BY EeFinanciera
        /// 
        /// OPTIMIZACIONES:
        /// - Consulta única vs N consultas individuales
        /// - AsNoTracking() para consultas de solo lectura
        /// - WHERE ... IN (...) eficiente para múltiples expedientes
        /// - GroupBy + Sum ejecutado en BD (no en memoria)
        /// - Dictionary para lookups instantáneos posteriormente
        /// 
        /// MANEJO DE NULOS:
        /// - (decimal?)d.ImportePp ?? 0 maneja valores NULL en ImportePp
        /// - Garantiza que suma nunca sea NULL
        /// - Comportamiento consistente con lógica original
        /// 
        /// ESTRUCTURA DE RETORNO:
        /// - Dictionary<string, decimal>
        /// - Key: Expediente (EeFinanciera)
        /// - Value: Suma total de ImportePp para ese expediente
        /// 
        /// CASOS DE USO:
        /// - Calcular total devengado para mostrar vs autorizado
        /// - Determinar saldo disponible de autorizantes
        /// - Cálculos de porcentajes de ejecución
        /// 
        /// PARÁMETROS:
        /// - expedientes: List<string> - Lista de números de expediente a consultar
        /// 
        /// RETORNO:
        /// - Dictionary con suma de importes devengados por expediente
        /// - Dictionary vacío si no hay expedientes o error
        /// 
        /// CONSIDERACIONES:
        /// - Tabla Devengados puede ser muy grande (performance crítica)
        /// - Índice recomendado en EeFinanciera para eficiencia
        /// - Manejo robusto de errores para no interrumpir flujo principal
        /// </summary>
        /// <param name="expedientes">Lista de expedientes a consultar.</param>
        /// <returns>Un diccionario con el expediente como clave y la suma de importes como valor.</returns>
        private Dictionary<string, decimal> BuscarMuchosNumerosSigaf(List<string> expedientes)
        {
            // Se valida la entrada para evitar consultas innecesarias.
            if (expedientes == null || !expedientes.Any())
                return new Dictionary<string, decimal>();

            try
            {
                using (var context = new IVCdbContext())
                {
                    // La consulta se traduce a un eficiente "WHERE ... IN (...)" en SQL.
                    return context.Devengados
                        .AsNoTracking()
                        .Where(d => expedientes.Contains(d.EeFinanciera))
                        // Se agrupa por expediente para poder sumarizar por cada uno.
                        // Esto es el equivalente a ejecutar la consulta original para cada expediente.
                        .GroupBy(d => d.EeFinanciera)
                        // Se proyecta el resultado.
                        .Select(g => new
                        {
                            // g.Key es el expediente por el que se agrupó.
                            Expediente = g.Key,
                            // Se realiza la suma de ImportePp para el grupo.
                            // El cast a decimal? y el ?? 0 replican la lógica de manejar nulos.
                            TotalImporte = g.Sum(d => (decimal?)d.ImportePp) ?? 0
                        })
                        // Se convierte el resultado en un diccionario para búsquedas en memoria instantáneas (O(1)).
                        .ToDictionary(r => r.Expediente, r => r.TotalImporte);
                }
            }
            catch (Exception ex)
            {
                // Se registra el error y se devuelve un diccionario vacío para no detener la ejecución.
                System.Diagnostics.Debug.WriteLine($"Error en carga masiva de SIGAF: {ex.Message}");
                return new Dictionary<string, decimal>();
            }
        }

        #endregion

        #region Integración de Datos Externos (SADE y SIGAF)

        /// <summary>
        /// Consulta masiva optimizada para obtener información SADE de múltiples expedientes.
        /// 
        /// PROPÓSITO:
        /// - Reemplaza múltiples llamadas individuales a SADEHelper.ObtenerInfoSADE
        /// - Obtiene último pase SADE para lista de expedientes en una sola consulta
        /// - Optimiza performance en procesamiento de grandes volúmenes
        /// 
        /// LÓGICA DE CONSULTA:
        /// 1. WHERE expedientes.Contains(p.Expediente) - Filtro por lista de expedientes
        /// 2. GroupBy(p.Expediente) - Agrupa pases por expediente
        /// 3. OrderByDescending(FechaUltimoPase).FirstOrDefault() - TOP 1 por fecha DESC
        /// 4. ToDictionary() - Convierte a diccionario para lookups O(1)
        /// 
        /// EQUIVALENCIA SQL:
        /// Para cada expediente, replica:
        /// SELECT TOP 1 BuzonDestino, FechaUltimoPase 
        /// FROM PasesSade 
        /// WHERE Expediente = @exp 
        /// ORDER BY FechaUltimoPase DESC
        /// 
        /// OPTIMIZACIONES:
        /// - Consulta única vs N consultas individuales
        /// - AsNoTracking() para consultas de solo lectura
        /// - Materialización con ToList() antes de ToDictionary()
        /// - Filtro inicial con Contains() para reducir dataset
        /// 
        /// ESTRUCTURA DE RETORNO:
        /// - Dictionary<string, (string Buzon, DateTime? Fecha)>
        /// - Key: Expediente
        /// - Value.Buzon: Buzón de destino del último pase
        /// - Value.Fecha: Fecha del último pase
        /// 
        /// CASOS DE USO:
        /// - Mostrar estado actual de expedientes en SADE
        /// - Determinar ubicación actual de trámites
        /// - Calcular días transcurridos desde último movimiento
        /// 
        /// PARÁMETROS:
        /// - expedientes: List<string> - Lista de números de expediente a consultar
        /// 
        /// RETORNO:
        /// - Dictionary con información de último pase SADE por expediente
        /// - Dictionary vacío si no hay expedientes o error
        /// 
        /// MANEJO DE ERRORES:
        /// - Try-catch para errores de BD
        /// - Retorna dictionary vacío en caso de excepción
        /// - Debug.WriteLine para logging de errores
        /// 
        /// VALIDACIONES:
        /// - Verifica expedientes not null/empty antes de consultar
        /// - Filtra nulls en resultado para evitar KeyValuePairs inválidos
        /// </summary>
        /// <param name="expedientes">Lista de expedientes a consultar.</param>
        /// <returns>Un diccionario con el expediente como clave y una tupla (Buzon, Fecha) como valor.</returns>
        private Dictionary<string, (string Buzon, DateTime? Fecha)> BuscarMuchosDatosSade(List<string> expedientes)
        {
            // Se valida la entrada.
            if (expedientes == null || !expedientes.Any())
                return new Dictionary<string, (string, DateTime?)>();

            // Verificar si tenemos cache válido en sesión
            var cacheSade = ObtenerCacheSade();

            if (cacheSade != null)
            {
                Debug.WriteLine($"[CACHE SADE] Cache disponible. Items en cache: {cacheSade.Count}");

                // Verificar si TODOS los expedientes solicitados están en cache (incluso los que no tienen datos)
                bool todoEnCache = expedientes.All(exp => cacheSade.ContainsKey(exp));

                if (todoEnCache)
                {
                    // Filtrar del cache solo los expedientes solicitados que SÍ tienen datos SADE
                    var resultadoCache = new Dictionary<string, (string, DateTime?)>();
                    foreach (var expediente in expedientes)
                    {
                        if (cacheSade.ContainsKey(expediente) && cacheSade[expediente].Buzon != null)
                        {
                            resultadoCache[expediente] = cacheSade[expediente];
                        }
                    }

                    Debug.WriteLine($"[CACHE SADE] Todos los expedientes están en cache. Expedientes con datos SADE: {resultadoCache.Count}/{expedientes.Count}");
                    Debug.WriteLine($"[CACHE SADE] Usando cache para {expedientes.Count} expedientes");
                    return resultadoCache;
                }
                else
                {
                    var expedientesEnCache = expedientes.Count(exp => cacheSade.ContainsKey(exp));
                    Debug.WriteLine($"[CACHE SADE] Cache incompleto. Expedientes en cache: {expedientesEnCache}/{expedientes.Count}");
                }
            }
            else
            {
                Debug.WriteLine($"[CACHE SADE] Cache no disponible");
            }

            Debug.WriteLine($"[CACHE SADE] Consultando BD para {expedientes.Count} expedientes");

            try
            {
                using (var context = new IVCdbContext())
                {
                    // Se agrupa por expediente para poder encontrar el "TOP 1" de cada uno.
                    var ultimosPases = context.PasesSade
                        .AsNoTracking()
                        .Where(p => expedientes.Contains(p.Expediente))
                        .GroupBy(p => p.Expediente)
                        // Para cada grupo (g), se ordena por fecha descendente y se toma el primero.
                        // Esto replica exactamente la lógica "SELECT TOP 1 ... ORDER BY FECHA DESC".
                        .Select(g => g.OrderByDescending(p => p.FechaUltimoPase).FirstOrDefault())
                        .ToList(); // Se materializa la lista de los pases más recientes.

                    // Se convierte la lista resultante en un diccionario para búsquedas rápidas.
                    var resultado = ultimosPases
                        .Where(p => p != null) // Se asegura de no incluir grupos que no tuvieran pases.
                        .ToDictionary(p => p.Expediente, p => (p.BuzonDestino, p.FechaUltimoPase));

                    // Actualizar cache con los resultados Y con los expedientes que no tienen datos
                    var cacheActual = ObtenerCacheSade() ?? new Dictionary<string, (string, DateTime?)>();

                    // Agregar expedientes con datos
                    foreach (var kvp in resultado)
                    {
                        cacheActual[kvp.Key] = kvp.Value;
                    }

                    // Agregar expedientes SIN datos (marcarlos como consultados con null)
                    foreach (var expediente in expedientes)
                    {
                        if (!resultado.ContainsKey(expediente))
                        {
                            cacheActual[expediente] = (null, null); // Sin datos SADE
                        }
                    }

                    GuardarCacheSade(cacheActual);
                    Debug.WriteLine($"[CACHE SADE] Cache actualizado con {resultado.Count} expedientes CON datos + {expedientes.Count - resultado.Count} expedientes SIN datos. Total en cache: {cacheActual.Count}");

                    return resultado;
                }
            }
            catch (Exception ex)
            {
                // Se registra el error y se devuelve un diccionario vacío.
                System.Diagnostics.Debug.WriteLine($"Error en carga masiva de SADE: {ex.Message}");
                return new Dictionary<string, (string, DateTime?)>();
            }
        }

        /// <summary>
        /// Wrapper público para exponer la consulta bulk de SADE a otras clases.
        /// </summary>
        public Dictionary<string, (string Buzon, DateTime? Fecha)> ObtenerDatosSadeBulk(List<string> expedientes)
        {
            return BuscarMuchosDatosSade(expedientes ?? new List<string>());
        }

        /// <summary>
        /// Wrapper público para exponer la consulta bulk de SIGAF a otras clases.
        /// </summary>
        public Dictionary<string, decimal> ObtenerSigafBulk(List<string> expedientes)
        {
            // Llama al método privado que realiza la consulta bulk para SIGAF
            return BuscarMuchosNumerosSigaf(expedientes ?? new List<string>());
        }

        /// <summary>
        /// Limpia el cache de datos SADE para forzar recarga desde BD
        /// </summary>
        public static void LimpiarCacheSade()
        {
            if (HttpContext.Current?.Session != null)
            {
                HttpContext.Current.Session.Remove("CacheSade");
            }
            Debug.WriteLine("[CACHE SADE] Cache limpiado manualmente");
        }

        #endregion

        #region Métodos de Paginación Optimizada

        /// <summary>
        /// Método principal para carga paginada de autorizantes y redeterminaciones.
        /// 
        /// ARQUITECTURA DE PAGINACIÓN REAL:
        /// - Carga solo los registros necesarios para la página actual
        /// - Combina autorizantes reales + redeterminaciones calculadas
        /// - Aplica filtros de seguridad por área de usuario
        /// - Retorna DTOs listos para visualización
        /// 
        /// OPTIMIZACIONES IMPLEMENTADAS:
        /// - EF Context optimizado: LazyLoading=false, ProxyCreation=false, etc.
        /// - Include() estratégico para minimizar consultas N+1
        /// - Paginación en BD con Skip/Take (no en memoria)
        /// - Carga bulk de datos SADE y SIGAF
        /// - Diccionarios para lookups en memoria
        /// 
        /// FLUJO DE PROCESAMIENTO:
        /// 1. Configurar contexto EF para máximo performance
        /// 2. Cargar autorizantes paginados con Include de entidades relacionadas
        /// 3. Aplicar filtro de área de usuario (seguridad)
        /// 4. Cargar redeterminaciones relacionadas con autorizantes de la página
        /// 5. Calcular montos de redeterminaciones con lógica de negocio
        /// 6. Integrar datos SADE (buzones y fechas)
        /// 7. Calcular datos SIGAF (importes devengados)
        /// 8. Convertir a DTOs y aplicar CalcularCamposDerivados()
        /// 9. Ordenar resultado final
        /// 
        /// DATOS INTEGRADOS:
        /// - Autorizantes: Datos base de tabla Autorizantes
        /// - Redeterminaciones: Calculadas a partir de autorizantes
        /// - SADE: BuscarMuchosDatosSade() para último pase por expediente
        /// - SIGAF: BuscarMuchosNumerosSigaf() para importes devengados
        /// - Estados: Mapeados según lógica de MapearEstadoDesdeId()
        /// 
        /// PARÁMETROS:
        /// - usuario: UsuarioEF para filtro por área (null = sin filtro)
        /// - pageIndex: Índice de página base 0 (0 = primera página)
        /// - pageSize: Cantidad de registros por página (12 por defecto)
        /// 
        /// RETORNO:
        /// - Lista de AutorizanteDTO paginados y enriquecidos
        /// - Incluye tanto autorizantes como redeterminaciones
        /// - DTOs con campos calculados y datos integrados
        /// 
        /// PERFORMANCE:
        /// - Diseñado para manejar miles de autorizantes
        /// - Solo carga datos necesarios para página actual
        /// - Consultas bulk para datos externos
        /// - Stopwatch para monitoreo de tiempos
        /// 
        /// USO TÍPICO:
        /// - AutorizantesAdminEF.aspx.cs para carga de páginas
        /// - Interfaces que requieren paginación real
        /// - Sistemas con grandes volúmenes de datos
        /// </summary>
        /// <param name="usuario">Usuario para filtrar por área</param>
        /// <param name="pageIndex">Índice de página (0-based)</param>
        /// <param name="pageSize">Tamaño de página (12 por defecto)</param>
        /// <returns>Lista de autorizantes paginados</returns>
        public List<AutorizanteDTO> ListarAutorizantesPaginados(UsuarioEF usuario = null, int pageIndex = 0, int pageSize = 12)
        {
            var swTotal = Stopwatch.StartNew();
            var sw = Stopwatch.StartNew();

            try
            {
                using (var context = new IVCdbContext())
                {
                    // Configurar EF para máximo rendimiento
                    context.Configuration.LazyLoadingEnabled = false;
                    context.Configuration.ProxyCreationEnabled = false;
                    context.Configuration.AutoDetectChangesEnabled = false;
                    context.Configuration.ValidateOnSaveEnabled = false;

                    Debug.WriteLine($"=== PAGINACIÓN REAL ===");
                    Debug.WriteLine($"Página: {pageIndex + 1}, Tamaño: {pageSize}");

                    // 1. TIMING: Configuración de queries autorizantes
                    sw.Restart();
                    var queryAutorizantes = context.Autorizantes.AsNoTracking()
                        .Include(a => a.Obra)
                        .Include(a => a.Obra.Area)
                        .Include(a => a.Obra.Barrio)
                        .Include(a => a.Obra.Empresa)
                        .Include(a => a.Obra.Contrata)
                        .Include(a => a.Obra.Proyecto)
                        .Include(a => a.Concepto)
                        .Include(a => a.Estado)
                        .AsQueryable();

                    // Aplicar filtro por área si el usuario lo tiene
                    if (usuario != null && usuario.AreaId > 0)
                    {
                        queryAutorizantes = queryAutorizantes.Where(a => a.Obra.AreaId == usuario.AreaId);
                    }

                    // Contar total de autorizantes
                    var totalAutorizantes = queryAutorizantes.Count();
                    Debug.WriteLine($"Tiempo configuración query autorizantes + count: {sw.ElapsedMilliseconds} ms");

                    // 2. TIMING: Configuración de queries redeterminaciones
                    sw.Restart();
                    var queryRedeterminaciones = context.Redeterminaciones.AsNoTracking()
                        .Include(r => r.Autorizante)
                        .Include(r => r.Autorizante.Obra)
                        .Include(r => r.Autorizante.Obra.Area)
                        .Include(r => r.Autorizante.Obra.Barrio)
                        .Include(r => r.Autorizante.Obra.Empresa)
                        .Include(r => r.Autorizante.Obra.Contrata)
                        .Include(r => r.Autorizante.Concepto)
                        .Include(r => r.Etapa)
                        .AsQueryable();

                    // Aplicar filtro por área si el usuario lo tiene
                    if (usuario != null && usuario.AreaId > 0)
                    {
                        queryRedeterminaciones = queryRedeterminaciones.Where(r => r.Autorizante.Obra.AreaId == usuario.AreaId);
                    }

                    // Contar total de redeterminaciones
                    var totalRedeterminaciones = queryRedeterminaciones.Count();
                    var totalRegistros = totalAutorizantes + totalRedeterminaciones;
                    Debug.WriteLine($"Tiempo configuración query redeterminaciones + count: {sw.ElapsedMilliseconds} ms");

                    // 3. TIMING: Carga completa de autorizantes y redeterminaciones
                    sw.Restart();
                    List<AutorizanteEF> autorizantes = queryAutorizantes
                        .OrderBy(a => a.Id)
                        .ToList();
                    Debug.WriteLine($"Tiempo carga completa autorizantes ({autorizantes.Count}): {sw.ElapsedMilliseconds} ms");

                    sw.Restart();
                    List<RedeterminacionEF> redeterminaciones = queryRedeterminaciones
                        .OrderBy(AutorizanteEF => AutorizanteEF.Id)
                        .ToList();
                    Debug.WriteLine($"Tiempo carga completa redeterminaciones ({redeterminaciones.Count}): {sw.ElapsedMilliseconds} ms");

                    // 4. TIMING: Carga de certificados para cálculo de montos
                    sw.Restart();
                    var codigosAutorizante = autorizantes.Select(a => a.CodigoAutorizante).ToList();
                    var certificados = context.Certificados.AsNoTracking()
                        .Where(c => codigosAutorizante.Contains(c.CodigoAutorizante))
                        .ToList();
                    Debug.WriteLine($"Tiempo carga certificados ({certificados.Count}): {sw.ElapsedMilliseconds} ms");

                    // 5. TIMING: Cálculo de montos redeterminaciones
                    sw.Restart();
                    CalcularMontoRedeterminaciones(redeterminaciones, autorizantes, certificados);
                    Debug.WriteLine($"Tiempo cálculo montos redeterminaciones: {sw.ElapsedMilliseconds} ms");

                    // 6. TIMING: Obtención de expedientes válidos y datos SADE
                    sw.Restart();
                    var expedientesValidos = autorizantes
                        .Where(a => !string.IsNullOrEmpty(a.Expediente))
                        .Select(a => a.Expediente)
                        .Union(redeterminaciones
                            .Where(r => !string.IsNullOrEmpty(r.Autorizante?.Expediente))
                            .Select(r => r.Autorizante.Expediente))
                        .Distinct()
                        .ToList();

                    var datosSade = BuscarMuchosDatosSade(expedientesValidos);
                    Debug.WriteLine($"Tiempo obtención datos SADE ({expedientesValidos.Count} expedientes): {sw.ElapsedMilliseconds} ms");

                    // 7. TIMING: Paginación y construcción de DTOs
                    sw.Restart();

                    // 3. Determinar qué registros necesitamos para esta página
                    var skip = pageIndex * pageSize;
                    var take = pageSize;

                    var resultado = new List<AutorizanteDTO>();

                    // 4. Obtener autorizantes necesarios para la página
                    if (skip < totalAutorizantes)
                    {
                        var autorizantesEnPagina = autorizantes
                            .OrderBy(a => a.Id)
                            .Skip(skip)
                            .Take(Math.Min(take, totalAutorizantes - skip))
                            .ToList();

                        resultado.AddRange(autorizantesEnPagina.Select(a =>
                        {
                            var dto = new AutorizanteDTO
                            {
                                Id = a.Id,
                                CodigoAutorizante = a.CodigoAutorizante,
                                Expediente = a.Expediente,
                                Detalle = a.Detalle,
                                MontoAutorizado = a.MontoAutorizado,
                                MesAprobacion = a.MesAprobacion,
                                MesBase = a.MesBase,
                                ConceptoId = a.ConceptoId,
                                ConceptoNombre = a.Concepto?.Nombre,
                                EstadoId = a.EstadoId,
                                EstadoNombre = a.Estado?.Nombre,
                                ObraId = a.ObraId,
                                ObraDescripcion = a.Obra?.Descripcion,
                                ObraNumero = a.Obra?.Numero,
                                ObraAnio = a.Obra?.Anio,
                                AreaId = a.Obra?.AreaId,
                                AreaNombre = a.Obra?.Area?.Nombre,
                                BarrioId = a.Obra?.BarrioId,
                                BarrioNombre = a.Obra?.Barrio?.Nombre,
                                EmpresaId = a.Obra?.EmpresaId,
                                EmpresaNombre = a.Obra?.Empresa?.Nombre,
                                ContrataId = a.Obra?.ContrataId,
                                ContrataNombre = a.Obra?.Contrata?.Nombre
                            };

                            // Aplicar datos SADE si existen
                            if (!string.IsNullOrEmpty(a.Expediente) && datosSade.ContainsKey(a.Expediente))
                            {
                                var sadeInfo = datosSade[a.Expediente];
                                dto.BuzonSade = sadeInfo.Buzon;
                                dto.FechaSade = sadeInfo.Fecha;
                            }

                            dto.CalcularCamposDerivados();
                            return dto;
                        }));

                        take -= resultado.Count;
                    }

                    // 5. Si aún necesitamos más registros, obtener redeterminaciones
                    if (take > 0)
                    {
                        var skipRedeterminaciones = Math.Max(0, skip - totalAutorizantes);
                        var redeterminacionesEnPagina = redeterminaciones
                            .OrderBy(r => r.Id)
                            .Skip(skipRedeterminaciones)
                            .Take(take)
                            .ToList();

                        resultado.AddRange(redeterminacionesEnPagina.Select(r =>
                        {
                            var dto = new AutorizanteDTO
                            {
                                Id = r.Id,
                                CodigoAutorizante = r.CodigoRedet,
                                Expediente = r.Expediente,
                                Detalle = $"{r.Tipo} - {r.Etapa.Nombre}",
                                MontoAutorizado = r.MontoRedet ?? 0,
                                MesAprobacion = r.Salto,
                                MesBase = null,
                                ConceptoId = 11, // Las redeterminaciones no tienen concepto específico
                                ConceptoNombre = "REDETERMINACION",
                                EstadoId = MapearEstadoDesdeId(r.EstadoRedetEFId).Id,
                                EstadoNombre = MapearEstadoDesdeId(r.EstadoRedetEFId).Nombre,
                                ObraId = r.Autorizante?.ObraId,
                                ObraDescripcion = r.Autorizante?.Obra?.Descripcion,
                                ObraNumero = r.Autorizante?.Obra?.Numero,
                                ObraAnio = r.Autorizante?.Obra?.Anio,
                                AreaId = r.Autorizante?.Obra?.AreaId,
                                AreaNombre = r.Autorizante?.Obra?.Area?.Nombre,
                                BarrioId = r.Autorizante?.Obra?.BarrioId,
                                BarrioNombre = r.Autorizante?.Obra?.Barrio?.Nombre,
                                EmpresaId = r.Autorizante?.Obra?.EmpresaId,
                                EmpresaNombre = r.Autorizante?.Obra?.Empresa?.Nombre,
                                ContrataId = r.Autorizante?.Obra?.ContrataId,
                                ContrataNombre = r.Autorizante?.Obra?.Contrata?.Nombre
                            };

                            // Aplicar datos SADE si existen (usar expediente del autorizante base)
                            if (!string.IsNullOrEmpty(r.Autorizante?.Expediente) && datosSade.ContainsKey(r.Autorizante.Expediente))
                            {
                                var sadeInfo = datosSade[r.Autorizante.Expediente];
                                dto.BuzonSade = sadeInfo.Buzon;
                                dto.FechaSade = sadeInfo.Fecha;
                            }

                            dto.CalcularCamposDerivados();
                            return dto;
                        }));
                    }

                    Debug.WriteLine($"Tiempo construcción DTOs paginados: {sw.ElapsedMilliseconds} ms");
                    Debug.WriteLine($"Registros en página actual: {resultado.Count}");
                    Debug.WriteLine($"Total registros disponibles: {totalRegistros} (Autorizantes: {totalAutorizantes}, Redeterminaciones: {totalRedeterminaciones})");
                    Debug.WriteLine($"Total tiempo paginación: {swTotal.ElapsedMilliseconds} ms");

                    return resultado;

                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en ListarAutorizantesPaginados: {ex.Message}");
                return new List<AutorizanteDTO>();
            }
        }

        /// <summary>
        /// Método optimizado para contar el total de registros combinados.
        /// 
        /// PROPÓSITO:
        /// - Calcular total de registros para cálculos de paginación
        /// - Soporta filtro de seguridad por área de usuario
        /// - Operación COUNT optimizada (no carga datos en memoria)
        /// 
        /// CÁLCULO COMBINADO:
        /// - Total = COUNT(Autorizantes) + COUNT(Redeterminaciones)
        /// - Ambas consultas respetan filtro de área si está presente
        /// - Queries separadas más eficientes que UNION para conteo
        /// 
        /// OPTIMIZACIONES:
        /// - AsNoTracking() para máximo performance en conteo
        /// - COUNT() ejecutado en BD, no en memoria
        /// - Sin Include() innecesarios para conteo simple
        /// - Context de vida corta (using pattern)
        /// 
        /// FILTRO DE SEGURIDAD:
        /// - Si usuario.AreaId > 0: filtra por área organizacional
        /// - Solo cuenta registros del área del usuario
        /// - Garantiza que total refleje datos accesibles
        /// 
        /// PARÁMETROS:
        /// - usuario: UsuarioEF para filtro por área (null = sin filtro)
        /// 
        /// RETORNO:
        /// - int: Total de registros (autorizantes + redeterminaciones)
        /// - 0 en caso de error o sin datos
        /// 
        /// USO TÍPICO:
        /// - Cálculos de paginación externa
        /// - Validaciones de rangos de página
        /// - Información de totales en UI
        /// 
        /// CONSIDERACIONES:
        /// - Muy eficiente para grandes volúmenes
        /// - No incluye datos calculados adicionales
        /// - Resultado debe coincidir con registros paginados
        /// </summary>
        public int ContarTotalAutorizantes(UsuarioEF usuario = null)
        {
            try
            {
                using (var context = new IVCdbContext())
                {
                    int totalAutorizantes;
                    int totalRedeterminaciones;

                    if (usuario != null && usuario.AreaId > 0)
                    {
                        // OPTIMIZACIÓN: Usar IN con subquery en lugar de JOIN para contar
                        // Obtener IDs de obras del área
                        var obrasDelArea = context.Obras.AsNoTracking()
                            .Where(o => o.AreaId == usuario.AreaId)
                            .Select(o => o.Id)
                            .ToList();

                        // Contar autorizantes usando Contains (más eficiente que JOIN)
                        totalAutorizantes = context.Autorizantes.AsNoTracking()
                            .Count(a => obrasDelArea.Contains(a.ObraId));

                        // Contar redeterminaciones usando subquery optimizada
                        var codigosAutorizanteArea = context.Autorizantes.AsNoTracking()
                            .Where(a => obrasDelArea.Contains(a.ObraId))
                            .Select(a => a.CodigoAutorizante)
                            .ToList();

                        totalRedeterminaciones = context.Redeterminaciones.AsNoTracking()
                            .Count(r => codigosAutorizanteArea.Contains(r.CodigoAutorizante));
                    }
                    else
                    {
                        // Sin filtro: contar directamente
                        totalAutorizantes = context.Autorizantes.AsNoTracking().Count();
                        totalRedeterminaciones = context.Redeterminaciones.AsNoTracking().Count();
                    }

                    return totalAutorizantes + totalRedeterminaciones;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en ContarTotalAutorizantes: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Obtiene TODOS los autorizantes + redeterminaciones para cálculo de subtotales (sin paginación)
        /// </summary>
        public List<AutorizanteDTO> ListarAutorizantesCompleto(UsuarioEF usuario = null)
        {
            var swTotal = Stopwatch.StartNew();
            var sw = Stopwatch.StartNew();

            try
            {
                using (var context = new IVCdbContext())
                {
                    // Configurar EF para máximo rendimiento
                    context.Configuration.LazyLoadingEnabled = false;
                    context.Configuration.ProxyCreationEnabled = false;
                    context.Configuration.AutoDetectChangesEnabled = false;
                    context.Configuration.ValidateOnSaveEnabled = false;

                    try
                    {
                        Debug.WriteLine($"=== CARGA COMPLETA PARA SUBTOTALES ===");

                        var resultado = new List<AutorizanteDTO>();

                        // 1. TIMING: Carga optimizada de autorizantes (sin Include costoso)
                        sw.Restart();

                        List<AutorizanteEF> autorizantes;


                        if (usuario != null && usuario.Tipo == false)
                        {
                            // Usuario NO administrador: filtrar por área
                            var obrasDelArea = context.Obras.AsNoTracking()
                                .Where(o => o.AreaId == usuario.AreaId)
                                .Select(o => o.Id)
                                .ToList();

                            autorizantes = context.Autorizantes.AsNoTracking()
                                .Where(a => obrasDelArea.Contains(a.ObraId))
                                .ToList();
                        }
                        else
                        {
                            // Usuario administrador (Tipo == true): cargar todos los autorizantes
                            autorizantes = context.Autorizantes.AsNoTracking().ToList();
                        }

                        Debug.WriteLine($"Tiempo carga OPTIMIZADA autorizantes ({autorizantes.Count}): {sw.ElapsedMilliseconds} ms");
                        Debug.WriteLine($"[ListarAutorizantesCompleto] Autorizantes: {autorizantes.Count}");

                        // 1.1. Cargar datos relacionados por separado (más eficiente que Include)
                        sw.Restart();

                        // Primero obtener obraIds de los autorizantes
                        var obraIds = autorizantes.Where(a => a.ObraId != 0).Select(a => a.ObraId).Distinct().ToList();
                        // Cargar obrasDict
                        var obrasDict = context.Obras.AsNoTracking()
                            .Where(o => obraIds.Contains(o.Id))
                            .ToDictionary(o => o.Id);
                        // Ahora calcular los demás IDs usando obrasDict y autorizantes

                        var areaIds = obrasDict.Values.Where(o => o.AreaId.HasValue).Select(o => o.AreaId.Value).Distinct().ToList();
                        var empresaIds = obrasDict.Values.Where(o => o.EmpresaId.HasValue).Select(o => o.EmpresaId.Value).Distinct().ToList();
                        var barrioIds = obrasDict.Values.Where(o => o.BarrioId.HasValue).Select(o => o.BarrioId.Value).Distinct().ToList();
                        var contrataIds = obrasDict.Values.Where(o => o.ContrataId.HasValue).Select(o => o.ContrataId.Value).Distinct().ToList();

                        // Calcular conceptoIds y estadoIds a partir de los autorizantes
                        var conceptoIds = autorizantes.Select(a => a.ConceptoId).Distinct().ToList();
                        var estadoIds = autorizantes.Select(a => a.EstadoId).Distinct().ToList();

                        var areasDict = context.Areas.AsNoTracking()
                            .Where(ar => areaIds.Contains(ar.Id))
                            .ToDictionary(ar => ar.Id);


                        var barriosDict = context.Barrios.AsNoTracking()
                            .Where(b => barrioIds.Contains(b.Id))
                            .ToDictionary(b => b.Id);

                        var empresasDict = context.Empresas.AsNoTracking()
                            .Where(e => empresaIds.Contains(e.Id))
                            .ToDictionary(e => e.Id);

                        var contratasDict = context.Contratas.AsNoTracking()
                            .Where(c => contrataIds.Contains(c.Id))
                            .ToDictionary(c => c.Id);

                        var conceptosDict = context.Conceptos.AsNoTracking()
                            .Where(c => conceptoIds.Contains(c.Id))
                            .ToDictionary(c => c.Id);

                        var estadosDict = context.EstadosAutorizante.AsNoTracking()
                            .Where(e => estadoIds.Contains(e.Id))
                            .ToDictionary(e => e.Id);

                        // Asignar relaciones en memoria (súper rápido)
                        foreach (var auth in autorizantes)
                        {
                            if (obrasDict.ContainsKey(auth.ObraId))
                            {
                                auth.Obra = obrasDict[auth.ObraId];

                                if (auth.Obra.AreaId.HasValue && areasDict.ContainsKey(auth.Obra.AreaId.Value))
                                    auth.Obra.Area = areasDict[auth.Obra.AreaId.Value];

                                if (auth.Obra.BarrioId.HasValue && barriosDict.ContainsKey(auth.Obra.BarrioId.Value))
                                    auth.Obra.Barrio = barriosDict[auth.Obra.BarrioId.Value];

                                if (auth.Obra.EmpresaId.HasValue && empresasDict.ContainsKey(auth.Obra.EmpresaId.Value))
                                    auth.Obra.Empresa = empresasDict[auth.Obra.EmpresaId.Value];

                                if (auth.Obra.ContrataId.HasValue && contratasDict.ContainsKey(auth.Obra.ContrataId.Value))
                                    auth.Obra.Contrata = contratasDict[auth.Obra.ContrataId.Value];
                            }

                            if (conceptosDict.ContainsKey(auth.ConceptoId))
                                auth.Concepto = conceptosDict[auth.ConceptoId];

                            if (estadosDict.ContainsKey(auth.EstadoId))
                                auth.Estado = estadosDict[auth.EstadoId];
                        }

                        Debug.WriteLine($"Tiempo carga datos relacionados: {sw.ElapsedMilliseconds} ms");

                        // 2. TIMING: Carga de todas las redeterminaciones
                        sw.Restart();
                        var queryRedeterminaciones = context.Redeterminaciones.AsNoTracking()
                            .Include(r => r.Autorizante)
                            .Include(r => r.Autorizante.Obra)
                            .Include(r => r.Autorizante.Obra.Area)
                            .Include(r => r.Autorizante.Obra.Barrio)
                            .Include(r => r.Autorizante.Obra.Empresa)
                            .Include(r => r.Autorizante.Obra.Contrata)
                            .Include(r => r.Autorizante.Concepto)
                            .Include(r => r.Etapa)
                            .AsQueryable();

                        // Aplicar filtro por área si el usuario lo tiene
                        if (usuario != null && usuario.AreaId > 0)
                        {
                            queryRedeterminaciones = queryRedeterminaciones.Where(r => r.Autorizante.Obra.AreaId == usuario.AreaId);
                        }

                        var redeterminaciones = queryRedeterminaciones.ToList();
                        Debug.WriteLine($"Tiempo carga completa redeterminaciones ({redeterminaciones.Count}): {sw.ElapsedMilliseconds} ms");
                        Debug.WriteLine($"[ListarAutorizantesCompleto] Redeterminaciones: {redeterminaciones.Count}");

                        // 3. TIMING: Carga de certificados y cálculo de montos
                        sw.Restart();
                        var codigosAutorizante = autorizantes.Select(a => a.CodigoAutorizante).ToList();
                        var certificados = context.Certificados.AsNoTracking()
                            .Where(c => codigosAutorizante.Contains(c.CodigoAutorizante))
                            .ToList();
                        Debug.WriteLine($"Tiempo carga certificados completos ({certificados.Count}): {sw.ElapsedMilliseconds} ms");

                        sw.Restart();
                        CalcularMontoRedeterminaciones(redeterminaciones, autorizantes, certificados);
                        Debug.WriteLine($"Tiempo cálculo montos redeterminaciones completo: {sw.ElapsedMilliseconds} ms");

                        // 4. TIMING: Obtención de datos SADE
                        sw.Restart();
                        var expedientesValidos = autorizantes
                            .Where(a => !string.IsNullOrEmpty(a.Expediente))
                            .Select(a => a.Expediente)
                            .Union(redeterminaciones
                                .Where(r => !string.IsNullOrEmpty(r.Autorizante?.Expediente))
                                .Select(r => r.Autorizante.Expediente))
                            .Distinct()
                            .ToList();

                        var datosSade = BuscarMuchosDatosSade(expedientesValidos);
                        Debug.WriteLine($"[TIMING] CalculoRedeterminacionNegocioEF.ListarAutorizantesCompleto() - 4. Obtención de datos SADE: {sw.ElapsedMilliseconds} ms");

                        // 5. TIMING: Construcción de DTOs
                        sw.Restart();

                        // 3. Convertir autorizantes a DTO
                        resultado.AddRange(autorizantes.Select(a =>
                        {
                            var dto = new AutorizanteDTO
                            {
                                Id = a.Id,
                                CodigoAutorizante = a.CodigoAutorizante,
                                Expediente = a.Expediente,
                                Detalle = a.Detalle,
                                MontoAutorizado = a.MontoAutorizado,
                                MesAprobacion = a.MesAprobacion,
                                MesBase = a.MesBase,
                                ConceptoId = a.ConceptoId,
                                ConceptoNombre = a.Concepto?.Nombre,
                                EstadoId = a.EstadoId,
                                EstadoNombre = a.Estado?.Nombre,
                                ObraId = a.ObraId,
                                ObraDescripcion = a.Obra?.Descripcion,
                                ObraNumero = a.Obra?.Numero,
                                ObraAnio = a.Obra?.Anio,
                                AreaId = a.Obra?.AreaId,
                                AreaNombre = a.Obra?.Area?.Nombre,
                                BarrioId = a.Obra?.BarrioId,
                                BarrioNombre = a.Obra?.Barrio?.Nombre,
                                EmpresaId = a.Obra?.EmpresaId,
                                EmpresaNombre = a.Obra?.Empresa?.Nombre,
                                ContrataId = a.Obra?.ContrataId,
                                ContrataNombre = a.Obra?.Contrata?.Nombre
                            };
                            // Aplicar datos SADE si existen
                            if (!string.IsNullOrEmpty(a.Expediente) && datosSade.ContainsKey(a.Expediente))
                            {
                                var sadeInfo = datosSade[a.Expediente];
                                dto.BuzonSade = sadeInfo.Buzon;
                                dto.FechaSade = sadeInfo.Fecha;
                            }
                            dto.CalcularCamposDerivados();
                            return dto;
                        }));

                        // 4. Convertir redeterminaciones a DTO
                        resultado.AddRange(redeterminaciones.Select(r =>
                        {
                            var dto = new AutorizanteDTO
                            {
                                Id = r.Id,
                                CodigoAutorizante = r.CodigoRedet,
                                Expediente = r.Autorizante?.Expediente,
                                Detalle = $"{r.Tipo} - {r.Etapa}",
                                MontoAutorizado = r.MontoRedet ?? 0,
                                MesAprobacion = r.Salto,
                                MesBase = null,
                                ConceptoId = 11,
                                ConceptoNombre = "REDETERMINACION",
                                EstadoId = MapearEstadoDesdeId(r.EstadoRedetEFId).Id,
                                EstadoNombre = MapearEstadoDesdeId(r.EstadoRedetEFId).Nombre,
                                ObraId = r.Autorizante?.ObraId,
                                ObraDescripcion = r.Autorizante?.Obra?.Descripcion,
                                ObraNumero = r.Autorizante?.Obra?.Numero,
                                ObraAnio = r.Autorizante?.Obra?.Anio,
                                AreaId = r.Autorizante?.Obra?.AreaId,
                                AreaNombre = r.Autorizante?.Obra?.Area?.Nombre,
                                BarrioId = r.Autorizante?.Obra?.BarrioId,
                                BarrioNombre = r.Autorizante?.Obra?.Barrio?.Nombre,
                                EmpresaId = r.Autorizante?.Obra?.EmpresaId,
                                EmpresaNombre = r.Autorizante?.Obra?.Empresa?.Nombre,
                                ContrataId = r.Autorizante?.Obra?.ContrataId,
                                ContrataNombre = r.Autorizante?.Obra?.Contrata?.Nombre
                            };

                            // Aplicar datos SADE si existen (usar expediente del autorizante base)
                            if (!string.IsNullOrEmpty(r.Autorizante?.Expediente) && datosSade.ContainsKey(r.Autorizante.Expediente))
                            {
                                var sadeInfo = datosSade[r.Autorizante.Expediente];
                                dto.BuzonSade = sadeInfo.Buzon;
                                dto.FechaSade = sadeInfo.Fecha;
                            }

                            dto.CalcularCamposDerivados();
                            return dto;
                        }));

                        Debug.WriteLine($"[TIMING] CalculoRedeterminacionNegocioEF.ListarAutorizantesCompleto() - 5. Construcción de DTOs: {sw.ElapsedMilliseconds} ms");

                        Debug.WriteLine($"=== CARGA COMPLETA PARA SUBTOTALES ===");
                        Debug.WriteLine($"Total tiempo carga completa: {swTotal.ElapsedMilliseconds} ms");

                        Debug.WriteLine($"[ListarAutorizantesCompleto] Total DTOs devueltos: {resultado.Count}");
                        return resultado;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error en ListarAutorizantesCompleto: {ex.Message}");
                        return new List<AutorizanteDTO>();
                    }

                } // using IVCdbContext
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en ListarAutorizantesCompleto: {ex.Message}");
                return new List<AutorizanteDTO>();
            }
        }
        #endregion

        #region Métodos de Paginación Optimizada para Certificados
        /// <summary>
        /// Lista certificados y reliquidaciones con paginación real en BD y filtros aplicados.
        /// Versión optimizada que replica el patrón de AutorizantesAdminEF para alta performance.
        /// 
        /// DISEÑO DE OPTIMIZACIÓN:
        /// - Consultas paginadas en BD (no carga en memoria)
        /// - Filtros aplicados a nivel de BD mediante IQueryable
        /// - Carga separada de entidades relacionadas para evitar JOINs costosos
        /// - Configuración EF optimizada para consultas masivas
        /// - Resultado encapsulado con total de registros para paginación externa
        /// 
        /// FILTROS SOPORTADOS:
        /// - filtroTexto: Búsqueda en múltiples campos (expediente, empresa, código, etc.)
        /// - *Ids: Filtros por entidades relacionadas (área, obra, barrio, etc.)
        /// - mesesCertificado: Filtro por fechas de certificación
        /// 
        /// RESULTADO:
        /// - Datos: Lista paginada de CertificadoDTO para la página actual
        /// - TotalRegistros: Conteo total para controles de paginación
        /// </summary>
        /// <param name="pageIndex">Página a consultar (base 1)</param>
        /// <param name="pageSize">Cantidad de registros por página</param>
        /// <param name="filtroTexto">Filtro de texto general</param>
        /// <param name="areaIds">IDs de áreas para filtrar</param>
        /// <param name="obraIds">IDs de obras para filtrar</param>
        /// <param name="barrioIds">IDs de barrios para filtrar</param>
        /// <param name="proyectoIds">IDs de proyectos para filtrar</param>
        /// <param name="empresaIds">IDs de empresas para filtrar</param>
        /// <param name="autorizanteIds">IDs de autorizantes para filtrar</param>
        /// <param name="estadoIds">IDs de estados para filtrar</param>
        /// <param name="tipoPagoIds">IDs de tipos de pago para filtrar</param>
        /// <param name="mesesCertificado">Fechas de certificación para filtrar</param>
        /// <param name="lineaGestionIds">IDs de líneas de gestión para filtrar</param>
        /// <returns>Resultado paginado con datos y total de registros</returns>
        public (List<CertificadoDTO> Datos, int TotalRegistros) ListarCertificadosPaginados(
            int pageIndex = 1,
            int pageSize = 25,
            string filtroTexto = "",
            List<int> areaIds = null,
            List<int> obraIds = null,
            List<int> barrioIds = null,
            List<int> proyectoIds = null,
            List<int> empresaIds = null,
            List<int> autorizanteIds = null,
            List<int> estadoIds = null,
            List<int> tipoPagoIds = null,
            List<DateTime> mesesCertificado = null,
            List<int> lineaGestionIds = null)
        {
            var swTotal = Stopwatch.StartNew();

            try
            {
                using (var context = new IVCdbContext())
                {
                    // Configurar EF para máximo rendimiento
                    context.Configuration.LazyLoadingEnabled = false;
                    context.Configuration.ProxyCreationEnabled = false;
                    context.Configuration.AutoDetectChangesEnabled = false;
                    context.Configuration.ValidateOnSaveEnabled = false;

                    Debug.WriteLine($"=== PAGINACIÓN CERTIFICADOS ===");
                    Debug.WriteLine($"Página: {pageIndex}, Tamaño: {pageSize}");

                    // 1. Query base para certificados
                    var queryCertificados = context.Certificados.AsNoTracking()
                        .Join(context.Autorizantes.AsNoTracking(), c => c.CodigoAutorizante, a => a.CodigoAutorizante,
                              (c, a) => new { Cert = c, Auth = a })
                        .Join(context.Obras.AsNoTracking(), ca => ca.Auth.ObraId, o => o.Id,
                              (ca, o) => new { ca.Cert, ca.Auth, Obra = o })
                        .AsQueryable();

                    // 2. Query base para reliquidaciones
                    var queryReliquidaciones = context.ExpedientesReliq.AsNoTracking()
                        .Join(context.Autorizantes.AsNoTracking(), er => er.CodigoRedet, a => a.CodigoAutorizante,
                              (er, a) => new { Reliq = er, Auth = a })
                        .Join(context.Obras.AsNoTracking(), ra => ra.Auth.ObraId, o => o.Id,
                              (ra, o) => new { ra.Reliq, ra.Auth, Obra = o })
                        .AsQueryable();

                    // 3. Aplicar filtros de texto
                    if (!string.IsNullOrEmpty(filtroTexto))
                    {
                        var filtroLower = filtroTexto.ToLower();

                        queryCertificados = queryCertificados.Where(x =>
                            x.Cert.ExpedientePago.ToLower().Contains(filtroLower) ||
                            x.Auth.CodigoAutorizante.ToLower().Contains(filtroLower) ||
                            x.Obra.Descripcion.ToLower().Contains(filtroLower));

                        queryReliquidaciones = queryReliquidaciones.Where(x =>
                            x.Reliq.Expediente.ToLower().Contains(filtroLower) ||
                            x.Auth.CodigoAutorizante.ToLower().Contains(filtroLower) ||
                            x.Obra.Descripcion.ToLower().Contains(filtroLower));
                    }

                    // 4. Aplicar filtros por IDs
                    if (areaIds?.Any() == true)
                    {
                        queryCertificados = queryCertificados.Where(x => areaIds.Contains(x.Obra.AreaId.Value));
                        queryReliquidaciones = queryReliquidaciones.Where(x => areaIds.Contains(x.Obra.AreaId.Value));
                    }

                    if (obraIds?.Any() == true)
                    {
                        queryCertificados = queryCertificados.Where(x => obraIds.Contains(x.Obra.Id));
                        queryReliquidaciones = queryReliquidaciones.Where(x => obraIds.Contains(x.Obra.Id));
                    }

                    if (barrioIds?.Any() == true)
                    {
                        queryCertificados = queryCertificados.Where(x => x.Obra.BarrioId.HasValue && barrioIds.Contains(x.Obra.BarrioId.Value));
                        queryReliquidaciones = queryReliquidaciones.Where(x => x.Obra.BarrioId.HasValue && barrioIds.Contains(x.Obra.BarrioId.Value));
                    }

                    if (empresaIds?.Any() == true)
                    {
                        queryCertificados = queryCertificados.Where(x => x.Obra.EmpresaId.HasValue && empresaIds.Contains(x.Obra.EmpresaId.Value));
                        queryReliquidaciones = queryReliquidaciones.Where(x => x.Obra.EmpresaId.HasValue && empresaIds.Contains(x.Obra.EmpresaId.Value));
                    }

                    if (autorizanteIds?.Any() == true)
                    {
                        queryCertificados = queryCertificados.Where(x => autorizanteIds.Contains(x.Auth.Id));
                        queryReliquidaciones = queryReliquidaciones.Where(x => autorizanteIds.Contains(x.Auth.Id));
                    }

                    if (tipoPagoIds?.Any() == true)
                    {
                        queryCertificados = queryCertificados.Where(x => tipoPagoIds.Contains(x.Cert.TipoPagoId));
                        // Las reliquidaciones siempre tienen TipoPagoId = 3
                        if (!tipoPagoIds.Contains(3))
                        {
                            // Excluir todas las reliquidaciones si no se incluye tipo 3
                            queryReliquidaciones = queryReliquidaciones.Where(x => false);
                        }
                    }

                    if (mesesCertificado?.Any() == true)
                    {
                        var fechas = mesesCertificado.Select(d => d.Date).ToList();
                        queryCertificados = queryCertificados.Where(x => x.Cert.MesAprobacion.HasValue && fechas.Contains(x.Cert.MesAprobacion.Value));
                        queryReliquidaciones = queryReliquidaciones.Where(x => fechas.Contains(x.Reliq.MesAprobacion));
                    }

                    // 5. Contar totales (antes de paginación)
                    var totalCertificados = queryCertificados.Count();
                    var totalReliquidaciones = queryReliquidaciones.Count();
                    var totalRegistros = totalCertificados + totalReliquidaciones;

                    Debug.WriteLine($"Total registros filtrados: {totalRegistros} (Cert: {totalCertificados}, Reliq: {totalReliquidaciones})");

                    // 6. Aplicar paginación y obtener datos
                    var skip = (pageIndex - 1) * pageSize;
                    var resultado = new List<CertificadoDTO>();

                    // 6.1. Obtener certificados para la página actual
                    if (skip < totalCertificados)
                    {
                        var certificadosEnPagina = queryCertificados
                            .OrderBy(x => x.Cert.Id)
                            .Skip(skip)
                            .Take(Math.Min(pageSize, totalCertificados - skip))
                            .Select(x => new
                            {
                                x.Cert,
                                x.Auth,
                                x.Obra,
                                // Campos adicionales que necesitamos
                                AreaId = x.Obra.AreaId,
                                BarrioId = x.Obra.BarrioId,
                                EmpresaId = x.Obra.EmpresaId,
                                ContrataId = x.Obra.ContrataId
                            })
                            .ToList();

                        // Cargar datos relacionados por separado para mejor performance
                        var entidadesIds = ExtraerIdsEntidadesRelacionadas(certificadosEnPagina);
                        var entidadesRelacionadas = CargarEntidadesRelacionadas(context, entidadesIds);

                        // Convertir a DTOs
                        foreach (var item in certificadosEnPagina)
                        {
                            var dto = new CertificadoDTO
                            {
                                Id = item.Cert.Id,
                                CodigoAutorizante = item.Auth.CodigoAutorizante,
                                ExpedientePago = item.Cert.ExpedientePago,
                                MontoTotal = item.Cert.MontoTotal,
                                MesAprobacion = item.Cert.MesAprobacion,
                                TipoPagoId = item.Cert.TipoPagoId,
                                ObraId = item.Obra.Id,
                                ObraDescripcion = item.Obra.Descripcion,
                                AutorizanteId = item.Auth.Id,
                                AreaId = item.AreaId,
                                BarrioId = item.BarrioId,
                                EmpresaId = item.EmpresaId
                            };

                            // Asignar nombres de entidades relacionadas
                            AsignarNombresEntidades(dto, entidadesRelacionadas);

                            resultado.Add(dto);
                        }
                    }

                    // 6.2. Si necesitamos más registros, obtener reliquidaciones
                    var restante = pageSize - resultado.Count;
                    if (restante > 0 && totalReliquidaciones > 0)
                    {
                        var skipReliq = Math.Max(0, skip - totalCertificados);

                        var reliquidacionesEnPagina = queryReliquidaciones
                            .OrderBy(x => x.Reliq.Id)
                            .Skip(skipReliq)
                            .Take(restante)
                            .Select(x => new
                            {
                                x.Reliq,
                                x.Auth,
                                x.Obra,
                                AreaId = x.Obra.AreaId,
                                BarrioId = x.Obra.BarrioId,
                                EmpresaId = x.Obra.EmpresaId,
                                ContrataId = x.Obra.ContrataId
                            })
                            .ToList();

                        // Cargar datos relacionados
                        var entidadesIds = ExtraerIdsEntidadesRelacionadas(reliquidacionesEnPagina);
                        var entidadesRelacionadas = CargarEntidadesRelacionadas(context, entidadesIds);

                        // Convertir a DTOs
                        foreach (var item in reliquidacionesEnPagina)
                        {
                            var dto = new CertificadoDTO
                            {
                                IdReliquidacion = item.Reliq.Id,
                                CodigoAutorizante = item.Auth.CodigoAutorizante,
                                ExpedientePago = item.Reliq.Expediente,
                                MontoTotal = 0, // Las reliquidaciones no tienen monto en esta tabla
                                MesAprobacion = item.Reliq.MesAprobacion,
                                TipoPagoId = 3, // Reliquidación siempre es tipo 3
                                ObraId = item.Obra.Id,
                                ObraDescripcion = item.Obra.Descripcion,
                                AutorizanteId = item.Auth.Id,
                                AreaId = item.AreaId,
                                BarrioId = item.BarrioId,
                                EmpresaId = item.EmpresaId
                            };

                            // Asignar nombres de entidades relacionadas
                            AsignarNombresEntidades(dto, entidadesRelacionadas);

                            resultado.Add(dto);
                        }
                    }

                    Debug.WriteLine($"Registros en página actual: {resultado.Count}");
                    Debug.WriteLine($"Total tiempo paginación certificados: {swTotal.ElapsedMilliseconds} ms");

                    return (resultado, totalRegistros);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en ListarCertificadosPaginados: {ex.Message}");
                return (new List<CertificadoDTO>(), 0);
            }
        }

        /// <summary>
        /// Extrae IDs de entidades relacionadas para carga optimizada por separado.
        /// </summary>
        private dynamic ExtraerIdsEntidadesRelacionadas(IEnumerable<dynamic> items)
        {
            var areaIds = new List<int>();
            var barrioIds = new List<int>();
            var empresaIds = new List<int>();
            var contrataIds = new List<int>();

            foreach (var item in items)
            {
                if (item.AreaId.HasValue) areaIds.Add(item.AreaId.Value);
                if (item.BarrioId.HasValue) barrioIds.Add(item.BarrioId.Value);
                if (item.EmpresaId.HasValue) empresaIds.Add(item.EmpresaId.Value);
                if (item.ContrataId.HasValue) contrataIds.Add(item.ContrataId.Value);
            }

            return new
            {
                AreaIds = areaIds.Distinct().ToList(),
                BarrioIds = barrioIds.Distinct().ToList(),
                EmpresaIds = empresaIds.Distinct().ToList(),
                ContrataIds = contrataIds.Distinct().ToList()
            };
        }

        /// <summary>
        /// Carga todas las entidades relacionadas en consultas separadas para mejor performance.
        /// </summary>
        private dynamic CargarEntidadesRelacionadas(IVCdbContext context, dynamic entidadesIds)
        {
            var areaIds = (List<int>)entidadesIds.AreaIds;
            var barrioIds = (List<int>)entidadesIds.BarrioIds;
            var empresaIds = (List<int>)entidadesIds.EmpresaIds;
            var contrataIds = (List<int>)entidadesIds.ContrataIds;

            var areas = context.Areas.AsNoTracking()
                .Where(a => areaIds.Contains(a.Id))
                .ToDictionary(a => a.Id);

            var barrios = context.Barrios.AsNoTracking()
                .Where(b => barrioIds.Contains(b.Id))
                .ToDictionary(b => b.Id);

            var empresas = context.Empresas.AsNoTracking()
                .Where(e => empresaIds.Contains(e.Id))
                .ToDictionary(e => e.Id);

            var contratas = context.Contratas.AsNoTracking()
                .Where(c => contrataIds.Contains(c.Id))
                .ToDictionary(c => c.Id);

            var tiposPago = context.TiposPago.AsNoTracking()
                .ToDictionary(t => t.Id);

            return new
            {
                Areas = areas,
                Barrios = barrios,
                Empresas = empresas,
                Contratas = contratas,
                TiposPago = tiposPago
            };
        }

        /// <summary>
        /// Asigna nombres de entidades relacionadas al DTO desde los diccionarios pre-cargados.
        /// </summary>
        private void AsignarNombresEntidades(CertificadoDTO dto, dynamic entidades)
        {
            if (dto.AreaId.HasValue && entidades.Areas.ContainsKey(dto.AreaId.Value))
                dto.AreaNombre = entidades.Areas[dto.AreaId.Value].Nombre;

            if (dto.BarrioId.HasValue && entidades.Barrios.ContainsKey(dto.BarrioId.Value))
                dto.BarrioNombre = entidades.Barrios[dto.BarrioId.Value].Nombre;

            if (dto.EmpresaId.HasValue && entidades.Empresas.ContainsKey(dto.EmpresaId.Value))
                dto.EmpresaNombre = entidades.Empresas[dto.EmpresaId.Value].Nombre;

            if (entidades.TiposPago.ContainsKey(dto.TipoPagoId))
                dto.TipoPagoNombre = entidades.TiposPago[dto.TipoPagoId].Nombre;

            // Calcular estado basado en expediente y datos SADE
            dto.Estado = string.IsNullOrEmpty(dto.ExpedientePago) ? "NO INICIADO" : "EN TRAMITE";
        }
        #endregion

        #region Métodos de Paginación Simple para Certificados

        /// <summary>
        /// Lista certificados paginados de forma simple, similar a ListarAutorizantesPaginados.
        /// Utiliza solo filtro por usuario/área, sin filtros complejos.
        /// 
        /// ARQUITECTURA SIMPLE:
        /// - Recibe solo usuario, pageIndex y pageSize
        /// - Aplica filtro por área del usuario si corresponde
        /// - Retorna solo los registros de la página actual
        /// - Optimizado para rendimiento con paginación real en BD
        /// 
        /// USO TÍPICO:
        /// - CertificadosAdminEF.aspx.cs para carga de páginas
        /// - Interfaces que requieren paginación real
        /// - Sistemas con grandes volúmenes de datos
        /// </summary>
        /// <param name="usuario">Usuario para filtrar por área</param>
        /// <param name="pageIndex">Índice de página (0-based)</param>
        /// <param name="pageSize">Tamaño de página (12 por defecto)</param>
        /// <returns>Lista de certificados paginados</returns>
        public List<CertificadoDTO> ListarCertificadosPaginadosSimple(UsuarioEF usuario = null, int pageIndex = 0, int pageSize = 12)
        {
            try
            {
                using (var context = new IVCdbContext())
                {
                    // Configurar EF para máximo rendimiento
                    context.Configuration.LazyLoadingEnabled = false;
                    context.Configuration.ProxyCreationEnabled = false;
                    context.Configuration.AutoDetectChangesEnabled = false;
                    context.Configuration.ValidateOnSaveEnabled = false;

                    System.Diagnostics.Debug.WriteLine($"=== PAGINACIÓN SIMPLE CERTIFICADOS ===");
                    System.Diagnostics.Debug.WriteLine($"Página: {pageIndex + 1}, Tamaño: {pageSize}");

                    // Query base para certificados usando la misma estructura simplificada
                    var query = from cert in context.Certificados.AsNoTracking()
                                join auth in context.Autorizantes.AsNoTracking() on cert.CodigoAutorizante equals auth.CodigoAutorizante
                                join obra in context.Obras.AsNoTracking() on auth.ObraId equals obra.Id
                                join area in context.Areas.AsNoTracking() on obra.AreaId equals area.Id
                                join barrio in context.Barrios.AsNoTracking() on obra.BarrioId equals barrio.Id into barrioLeft
                                from barrio in barrioLeft.DefaultIfEmpty()
                                join empresa in context.Empresas.AsNoTracking() on obra.EmpresaId equals empresa.Id into empresaLeft
                                from empresa in empresaLeft.DefaultIfEmpty()
                                join tipoPago in context.TiposPago.AsNoTracking() on cert.TipoPagoId equals tipoPago.Id into tipoPagoLeft
                                from tipoPago in tipoPagoLeft.DefaultIfEmpty()
                                select new { cert, auth, obra, area, barrio, empresa, tipoPago };

                    // Aplicar filtro por área si el usuario lo tiene
                    if (usuario != null && usuario.AreaId > 0)
                    {
                        query = query.Where(x => x.obra.AreaId == usuario.AreaId);
                        System.Diagnostics.Debug.WriteLine($"Filtro por área: {usuario.AreaId}");
                    }

                    // Aplicar paginación
                    int skip = pageIndex * pageSize;
                    var certificadosPagina = query
                        .OrderBy(x => x.cert.Id)
                        .Skip(skip)
                        .Take(pageSize)
                        .ToList();

                    System.Diagnostics.Debug.WriteLine($"Registros obtenidos: {certificadosPagina.Count}");

                    // Convertir a DTO usando solo las propiedades que existen en CertificadoDTO
                    var resultado = certificadosPagina.Select(item => new CertificadoDTO
                    {
                        Id = item.cert.Id,
                        CodigoAutorizante = item.cert.CodigoAutorizante,
                        MontoTotal = item.cert.MontoTotal,
                        MesAprobacion = item.cert.MesAprobacion,
                        TipoPagoId = item.cert.TipoPagoId,
                        TipoPagoNombre = item.tipoPago?.Nombre ?? "",
                        AutorizanteId = item.auth.Id,
                        ObraId = item.obra.Id,
                        ObraDescripcion = item.obra.Descripcion,
                        AreaId = item.area.Id,
                        AreaNombre = item.area.Nombre,
                        BarrioId = item.barrio?.Id,
                        BarrioNombre = item.barrio?.Nombre ?? "",
                        EmpresaId = item.empresa?.Id,
                        EmpresaNombre = item.empresa?.Nombre ?? "",
                        ExpedientePago = item.cert.ExpedientePago,
                        Estado = string.IsNullOrEmpty(item.cert.ExpedientePago) ? "NO INICIADO" : "EN TRAMITE"
                    }).ToList();

                    return resultado;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en ListarCertificadosPaginadosSimple: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Cuenta el total de certificados disponibles para el usuario.
        /// Equivalente a ContarTotalAutorizantes para certificados.
        /// 
        /// OPTIMIZACIÓN:
        /// - Solo ejecuta COUNT(*), no carga datos
        /// - Aplica mismo filtro que ListarCertificadosPaginadosSimple
        /// - Retorna solo el número total de registros
        /// </summary>
        /// <param name="usuario">Usuario para filtrar por área</param>
        /// <returns>Total de certificados disponibles</returns>
        public int ContarTotalCertificados(UsuarioEF usuario = null)
        {
            try
            {
                using (var context = new IVCdbContext())
                {
                    // Configurar EF para máximo rendimiento
                    context.Configuration.LazyLoadingEnabled = false;
                    context.Configuration.ProxyCreationEnabled = false;
                    context.Configuration.AutoDetectChangesEnabled = false;

                    // Query base para contar certificados - misma lógica que el método de listado
                    var query = from cert in context.Certificados.AsNoTracking()
                                join auth in context.Autorizantes.AsNoTracking() on cert.CodigoAutorizante equals auth.CodigoAutorizante
                                join obra in context.Obras.AsNoTracking() on auth.ObraId equals obra.Id
                                select new { cert, obra };

                    // Aplicar filtro por área si el usuario lo tiene
                    if (usuario != null && usuario.AreaId > 0)
                    {
                        query = query.Where(x => x.obra.AreaId == usuario.AreaId);
                    }

                    int total = query.Count();
                    System.Diagnostics.Debug.WriteLine($"Total certificados disponibles: {total}");

                    return total;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en ContarTotalCertificados: {ex.Message}");
                throw;
            }
        }

        #endregion

    }
}
