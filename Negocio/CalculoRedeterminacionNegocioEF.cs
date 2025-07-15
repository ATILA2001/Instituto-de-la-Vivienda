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
    public class CalculoRedeterminacionNegocioEF
    {
        // Desestimado = "35, 36"
        // No iniciado = 37, 38
        // Aprobado = 12, 22, 33, 39
        // En tramite todos los demas

        private readonly int[] IdsDesestimado = { 35, 36 }; // Rechazada, Fuera de Plazo
        private readonly int[] IdsNoIniciado = { 37, 38 }; // Pendiente, No presentada
        private readonly int[] IdsAprobado = { 12, 22, 33, 39 };// RD-11/11-Notificada, RP-09/09-Notificada, RO-11/11-Notificada, ACDIR

        private static bool _cargandoDatos = false;

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
                System.Diagnostics.Debug.WriteLine("Bucle detectado en ListarCertificadosYReliquidaciones");
                return new List<CertificadoDTO>();
            }
            
            var swTotal = Stopwatch.StartNew();
            var sw = Stopwatch.StartNew();

            try
            {
                _cargandoDatos = true;

                using (var context = new IVCdbContext())
                {
                    // OPTIMIZACIÓN 1: Configurar EF para máximo rendimiento
                    var originalLazyLoading = context.Configuration.LazyLoadingEnabled;
                    var originalProxyCreation = context.Configuration.ProxyCreationEnabled;
                    var originalAutoDetect = context.Configuration.AutoDetectChangesEnabled;
                    var originalValidateOnSave = context.Configuration.ValidateOnSaveEnabled;

                    try
                    {
                        context.Configuration.LazyLoadingEnabled = false;
                        context.Configuration.ProxyCreationEnabled = false;
                        context.Configuration.AutoDetectChangesEnabled = false;
                        context.Configuration.ValidateOnSaveEnabled = false;

                        // OPTIMIZACIÓN CRÍTICA AUTORIZANTES: Eliminar Includes costosos y cargar por separado
                        sw.Restart();
                        
                        // 1. Cargar autorizantes SIN includes (súper rápido)
                        IQueryable<AutorizanteEF> autorizantesQuery = context.Autorizantes.AsNoTracking();
                        
                        if (usuario != null && usuario.AreaId > 0)
                        {
                            // Filtro temprano con JOIN directo (más eficiente que Include + Where)
                            autorizantesQuery = from a in context.Autorizantes.AsNoTracking()
                                               join o in context.Obras.AsNoTracking() on a.ObraId equals o.Id
                                               where o.AreaId == usuario.AreaId
                                               select a;
                        }
                        
                        var autorizantesBase = autorizantesQuery.ToList();
                        Debug.WriteLine($"Tiempo autorizantes SIN Include: {sw.ElapsedMilliseconds} ms");

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
                        var areaIds = autorizantesBase.Where(a => a.Obra?.AreaId != null).Select(a => a.Obra.AreaId.Value).Distinct().ToList();
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
                        var taskSigaf = System.Threading.Tasks.Task.Run(() => BuscarMuchosNumerosSigaf(todosLosExpedientes));
                        var taskSade = System.Threading.Tasks.Task.Run(() => BuscarMuchosDatosSade(todosLosExpedientes));
                        
                        // Esperar a que ambas terminen
                        System.Threading.Tasks.Task.WaitAll(taskSigaf, taskSade);
                        
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

                                listaReliqDTO.Add(new CertificadoDTO
                                {
                                    IdReliquidacion = idReliq++,
                                    ExpedientePago = expedienteFinalReliq,
                                    MesAprobacion = certAfectado.MesAprobacion,
                                    MontoTotal = montoReliq,
                                    EstadoRedetId = redet.EstadoRedetEFId,
                                    AutorizanteId = certAfectado.AutorizanteId,
                                    CodigoAutorizante = redet.CodigoRedet,
                                    ObraId = certAfectado.ObraId,
                                    ObraDescripcion = certAfectado.ObraDescripcion,
                                    EmpresaId = certAfectado.EmpresaId,
                                    EmpresaNombre = certAfectado.EmpresaNombre,
                                    AreaId = certAfectado.AreaId,
                                    AreaNombre = certAfectado.AreaNombre,
                                    Contrata = certAfectado.Contrata,
                                    BarrioId = certAfectado.BarrioId,
                                    BarrioNombre = certAfectado.BarrioNombre,
                                    ProyectoId = certAfectado.ProyectoId,
                                    ProyectoNombre = certAfectado.ProyectoNombre,
                                    LineaGestionId = certAfectado.LineaGestionId,
                                    LineaGestionNombre = certAfectado.LineaGestionNombre,
                                    TipoPagoId = 3,
                                    TipoPagoNombre = "RELIQUIDACION",
                                    Porcentaje = certAfectado.Porcentaje,
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
                    finally
                    {
                        // Restaurar configuración original
                        context.Configuration.LazyLoadingEnabled = originalLazyLoading;
                        context.Configuration.ProxyCreationEnabled = originalProxyCreation;
                        context.Configuration.AutoDetectChangesEnabled = originalAutoDetect;
                        context.Configuration.ValidateOnSaveEnabled = originalValidateOnSave;
                    }
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

                                    System.Diagnostics.Debug.WriteLine($"Reliquidación {reliqDTO.CodigoAutorizante}/{reliqDTO.MesAprobacion.Value:MM-yyyy} modificada: '{expedienteAnterior}' → '{reliqDTO.ExpedientePago}'");
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
                                    System.Diagnostics.Debug.WriteLine($"Nueva reliquidación creada: {reliqDTO.CodigoAutorizante}/{reliqDTO.MesAprobacion.Value:MM-yyyy} → '{reliqDTO.ExpedientePago}'");
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

        private void CalcularMontoRedeterminaciones(List<RedeterminacionEF> redeterminaciones, List<AutorizanteEF> autorizantes, List<CertificadoEF> certificados)
        {
            var redeterminacionesCalculadas = new List<RedeterminacionEF>();

            foreach (var redet in redeterminaciones.OrderBy(r => r.Nro))
            {
                var autorizante = autorizantes.FirstOrDefault(a => a.CodigoAutorizante == redet.CodigoAutorizante);
                if (autorizante == null || !redet.Salto.HasValue || !redet.Porcentaje.HasValue) continue;

                var certificadosRelacionados = certificados
                    .Where(c => c.CodigoAutorizante == autorizante.CodigoAutorizante && c.MesAprobacion < redet.Salto)
                    .ToList();

                decimal sumaMontosTotal = certificadosRelacionados.Sum(c => c.MontoTotal);
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
        /// Recreación optimizada de SIGAFHelper.ObtenerTotalImporteDevengados para carga masiva.
        /// Obtiene la suma de importes para una lista de expedientes en una única consulta a la base de datos.
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

        /// <summary>
        /// Recreación optimizada de SADEHelper.ObtenerInfoSADE para carga masiva.
        /// Obtiene el último pase SADE para una lista de expedientes en una única consulta.
        /// </summary>
        /// <param name="expedientes">Lista de expedientes a consultar.</param>
        /// <returns>Un diccionario con el expediente como clave y una tupla (Buzon, Fecha) como valor.</returns>
        private Dictionary<string, (string Buzon, DateTime? Fecha)> BuscarMuchosDatosSade(List<string> expedientes)
        {
            // Se valida la entrada.
            if (expedientes == null || !expedientes.Any())
                return new Dictionary<string, (string, DateTime?)>();

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
                    return ultimosPases
                        .Where(p => p != null) // Se asegura de no incluir grupos que no tuvieran pases.
                        .ToDictionary(p => p.Expediente, p => (p.BuzonDestino, p.FechaUltimoPase));
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
        /// Método para comparar el rendimiento de la optimización conservadora
        /// </summary>
        public void CompararRendimientoConservador(UsuarioEF usuario = null)
        {
            Debug.WriteLine("=".PadRight(80, '='));
            Debug.WriteLine("COMPARACIÓN DE RENDIMIENTO - OPTIMIZACIÓN CONSERVADORA");
            Debug.WriteLine("=".PadRight(80, '='));

            var swTotal = Stopwatch.StartNew();
            
            try
            {
                var resultado = ListarCertificadosYReliquidaciones(usuario);
                swTotal.Stop();
                
                Debug.WriteLine($"RESULTADO FINAL:");
                Debug.WriteLine($"- Total de registros: {resultado?.Count ?? 0}");
                Debug.WriteLine($"- Certificados: {resultado?.Count(r => r.TipoPagoId != 3) ?? 0}");
                Debug.WriteLine($"- Reliquidaciones: {resultado?.Count(r => r.TipoPagoId == 3) ?? 0}");
                Debug.WriteLine($"- Tiempo total: {swTotal.ElapsedMilliseconds} ms");
                Debug.WriteLine($"- Memoria utilizada: {GC.GetTotalMemory(false) / 1024 / 1024} MB");
                
                if (swTotal.ElapsedMilliseconds < 2000)
                {
                    Debug.WriteLine("🚀 EXCELENTE! Tiempo menor a 2 segundos");
                }
                else if (swTotal.ElapsedMilliseconds < 3000)
                {
                    Debug.WriteLine("⚡ MUY BUENO! Tiempo menor a 3 segundos");
                }
                else if (swTotal.ElapsedMilliseconds < 4000)
                {
                    Debug.WriteLine("✅ BUENO! Mejora significativa lograda");
                }
                else
                {
                    Debug.WriteLine("⚠️ Funcional, pero necesita más optimización");
                }
                
                var mejoraPorcentual = ((8500.0 - swTotal.ElapsedMilliseconds) / 8500.0) * 100;
                Debug.WriteLine($"- Mejora vs original (8.5s): {mejoraPorcentual:F1}%");
                Debug.WriteLine("=".PadRight(80, '='));
                
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR durante la prueba: {ex.Message}");
                if (ex.InnerException != null)
                    Debug.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                
                Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
            
            Debug.WriteLine("=".PadRight(80, '='));
        }

        /// <summary>
        /// Método estático para probar el rendimiento sin necesidad de instancia
        /// </summary>
        public static void ProbarRendimientoOptimizado(UsuarioEF usuario = null)
        {
            try
            {
                Debug.WriteLine("=== INICIANDO PRUEBA DE RENDIMIENTO CONSERVADORA ===");
                
                var instancia = new CalculoRedeterminacionNegocioEF();
                instancia.CompararRendimientoConservador(usuario);
                
                Debug.WriteLine("=== PRUEBA COMPLETADA ===");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERROR en ProbarRendimientoOptimizado: {ex.Message}");
                Debug.WriteLine($"Tipo de excepción: {ex.GetType().Name}");
                if (ex.InnerException != null)
                    Debug.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
        }
    }



}
