using Dominio;
using Dominio.DTO;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negocio
{
    public class AutorizanteNegocioEF
    {
        #region Consultas
        /// <summary>
        /// Lista completa para dropdown (sin tracking).
        /// </summary>
        public List<AutorizanteEF> ListarParaDDL(UsuarioEF usuario = null)
        {
            try
            {
                using (var context = new IVCdbContext())
                {
                    var query = context.Autorizantes.AsNoTracking()
                        .Include(a => a.Obra);

                    // Aplica filtro de seguridad por área solo si:
                    // 1. Usuario no es null
                    // 2. Usuario no es administrador (Tipo = false) 
                    // 3. Usuario tiene un área asignada
                    if (usuario != null && !usuario.Tipo)
                    {
                        int? areaId = usuario.AreaId ?? (usuario.Area != null ? usuario.Area.Id : (int?)null);

                        if (areaId.HasValue)
                        {
                            query = query.Where(a => a.Obra.AreaId == areaId.Value);
                        }
                    }

                    return query
                        .OrderBy(a => a.CodigoAutorizante)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Error al obtener los autorizantes", ex);
            }
        }

        /// <summary>
        /// Obtiene una lista filtrada de autorizantes para un usuario específico.
        /// 
        /// FILTRO DE SEGURIDAD:
        /// - Solo autorizantes cuyas obras pertenecen al área del usuario
        /// - Implementa seguridad a nivel de datos por área organizacional
        /// - Evita acceso a información fuera del alcance del usuario
        /// 
        /// LÓGICA DE FILTRO:
        /// 1. JOIN entre Autorizantes y Obras
        /// 2. WHERE Obra.AreaId == Usuario.AreaId
        /// 3. SELECT solo autorizantes que cumplan condición
        /// 
        /// OPTIMIZACIÓN:
        /// - JOIN directo en LINQ más eficiente que Include + Where
        /// - AsNoTracking() para consultas de solo lectura
        /// - Filtro aplicado en base de datos, no en memoria
        /// 
        /// PARÁMETROS:
        /// - usuario: UsuarioEF con AreaId válido para filtro
        /// 
        /// RETORNO:
        /// - Lista de AutorizanteEF filtrados por área del usuario
        /// - Lista vacía si usuario no tiene área asignada
        /// 
        /// USO TÍPICO:
        /// - Dropdowns en páginas con control de acceso por área
        /// - Listados que respetan jerarquía organizacional
        /// </summary>


        /// <summary>
        /// Obtiene un autorizante específico por su código único.
        /// 
        /// BÚSQUEDA POR CÓDIGO:
        /// - CodigoAutorizante es la clave de negocio (no PK técnica)
        /// - Código único identificatorio del autorizante en el sistema
        /// - Usado para búsquedas desde interfaces de usuario
        /// 
        /// OPTIMIZACIÓN:
        /// - AsNoTracking() para consultas de solo lectura
        /// - FirstOrDefault() para obtener único resultado o null
        /// - Índice recomendado en CodigoAutorizante para performance
        /// 
        /// PARÁMETROS:
        /// - codigoAutorizante: string - Código único del autorizante
        /// 
        /// RETORNO:
        /// - AutorizanteEF si existe el código
        /// - null si no se encuentra el código
        /// 
        /// USO TÍPICO:
        /// - Búsquedas por código desde UI
        /// - Validaciones de existencia
        /// - Obtener datos para mostrar/editar
        /// </summary>
        public AutorizanteEF ObtenerPorCodigo(string codigoAutorizante)
        {
            using (var context = new IVCdbContext())
            {
                return context.Autorizantes.AsNoTracking().FirstOrDefault(a => a.CodigoAutorizante == codigoAutorizante);
            }
        }

        /// <summary>
        /// Obtiene un autorizante específico por su ID de base de datos.
        /// 
        /// BÚSQUEDA POR PK:
        /// - Id es la Primary Key técnica de la tabla
        /// - Búsqueda más eficiente (índice clustered)
        /// - Usado para operaciones internas y edición
        /// 
        /// TRACKING HABILITADO:
        /// - NO usa AsNoTracking() porque típicamente se usa para edición
        /// - Entity Framework trackea cambios para posterior SaveChanges()
        /// - Find() method optimizado para búsquedas por PK
        /// 
        /// PARÁMETROS:
        /// - id: int - Primary Key del autorizante
        /// 
        /// RETORNO:
        /// - AutorizanteEF con tracking habilitado si existe
        /// - null si no se encuentra el ID
        /// 
        /// USO TÍPICO:
        /// - Cargar entidad para edición
        /// - Operaciones que requieren tracking de cambios
        /// - Preparar datos para modificación/eliminación
        /// </summary>
        public AutorizanteEF ObtenerPorId(int id)
        {
            using (var context = new IVCdbContext())
            {
                return context.Autorizantes.FirstOrDefault(a => a.Id == id);
            }
        }

        #endregion

        #region Operaciones de Escritura (CREATE, UPDATE, DELETE)

        /// <summary>
        /// Agrega un nuevo autorizante a la base de datos.
        /// 
        /// OPERACIÓN CREATE:
        /// - Inserta nueva entidad en tabla Autorizantes
        /// - Entity Framework asigna automáticamente nuevo ID
        /// - Validaciones de BD aplicadas automáticamente
        /// 
        /// VALIDACIONES IMPLÍCITAS:
        /// - Constraints de BD (NOT NULL, FK, etc.)
        /// - Validaciones de DataAnnotations en AutorizanteEF
        /// - Unicidad de CodigoAutorizante (si constraint existe)
        /// 
        /// TRANSACCIONALIDAD:
        /// - SaveChanges() ejecuta en transacción automática
        /// - Si falla, no se persiste nada (rollback automático)
        /// - Context se dispone automáticamente al salir
        /// 
        /// PARÁMETROS:
        /// - nuevoAutorizante: AutorizanteEF completo con datos requeridos
        /// 
        /// RETORNO:
        /// - true: Autorizante insertado correctamente
        /// - false: Error en inserción (violación constraints, etc.)
        /// 
        /// USO TÍPICO:
        /// - Formularios de alta de autorizantes
        /// - Importación masiva de datos
        /// - APIs de creación de autorizantes
        /// 
        /// CONSIDERACIONES:
        /// - nuevoAutorizante.Id será ignorado (auto-generado)
        /// - Propiedades requeridas deben estar pobladas
        /// - Referencias FK deben existir en BD
        /// </summary>
        public bool Agregar(AutorizanteEF nuevoAutorizante)
        {
            using (var context = new IVCdbContext())
            {
                context.Autorizantes.Add(nuevoAutorizante);
                return context.SaveChanges() > 0;
            }
        }

        /// <summary>
        /// Modifica un autorizante existente en la base de datos.
        /// 
        /// OPERACIÓN UPDATE:
        /// - Actualiza entidad existente por ID
        /// - Entity Framework detecta cambios automáticamente
        /// - Solo campos modificados se actualizan en BD
        /// 
        /// PATRÓN DE MODIFICACIÓN:
        /// 1. Context.Entry() marca entidad como Modified
        /// 2. EF detecta diferencias con estado original
        /// 3. Genera UPDATE SQL con solo campos cambiados
        /// 4. SaveChanges() ejecuta UPDATE en transacción
        /// 
        /// VALIDACIONES:
        /// - Entidad debe tener ID válido existente
        /// - Constraints de BD aplicadas en SaveChanges()
        /// - Concurrencia optimista (si está configurada)
        /// 
        /// PARÁMETROS:
        /// - autorizanteModificado: AutorizanteEF con ID existente y datos nuevos
        /// 
        /// RETORNO:
        /// - true: Autorizante actualizado correctamente
        /// - false: Error en actualización (ID no existe, constraints, etc.)
        /// 
        /// USO TÍPICO:
        /// - Formularios de edición de autorizantes
        /// - Actualizaciones masivas de datos
        /// - APIs de modificación de autorizantes
        /// 
        /// CONSIDERACIONES:
        /// - autorizanteModificado.Id debe existir en BD
        /// - Todas las propiedades se consideran para actualización
        /// - Para actualizaciones parciales usar métodos específicos
        /// </summary>
        public bool Modificar(AutorizanteEF autorizanteModificado)
        {
            using (var context = new IVCdbContext())
            {
                context.Entry(autorizanteModificado).State = EntityState.Modified;
                return context.SaveChanges() > 0;
            }
        }


        public bool Eliminar(int id)
        {
            using (var context = new IVCdbContext())
            {
                var autorizante = context.Autorizantes.FirstOrDefault(a => a.Id == id);
                if (autorizante == null) return false;
                // Evitar intentar borrar si hay dependencias (certificados o redeterminaciones)
                bool tieneCertificados = context.Certificados.Any(c => c.CodigoAutorizante == autorizante.CodigoAutorizante);
                if (tieneCertificados) return false;
                bool tieneRedeterminaciones = context.Redeterminaciones.Any(r => r.CodigoAutorizante == autorizante.CodigoAutorizante);
                if (tieneRedeterminaciones) return false;

                context.Autorizantes.Remove(autorizante);
                try { return context.SaveChanges() > 0; } catch { return false; }
            }
        }

        #endregion

        #region Actualizaciones Especializadas

        /// <summary>
        /// Actualiza únicamente el estado de un autorizante o redeterminación.
        /// 
        /// FUNCIONALIDAD DUAL:
        /// Este método maneja tanto autorizantes como redeterminaciones debido a que
        /// ambos tipos de entidades se muestran en la misma grilla y requieren
        /// capacidad de actualización de estado.
        /// 
        /// LÓGICA DE BÚSQUEDA:
        /// 1. PASO 1: Buscar en tabla Autorizantes por CodigoAutorizante
        ///    - Si encuentra: actualizar campo EstadoId
        ///    - Si NO encuentra: continuar al paso 2
        /// 
        /// 2. PASO 2: Buscar en tabla Redeterminaciones por CodigoRedet
        ///    - Redeterminaciones usan códigos como "XXX-XXX-XXXX-R1", "XXX-XXX-XXXX-R2", etc.
        ///    - Si encuentra: actualizar campo EstadoRedetEFId
        ///    - Si NO encuentra: retornar false
        /// 
        /// OPTIMIZACIÓN:
        /// - Actualización parcial: solo el campo de estado
        /// - No carga entidad completa, solo busca y actualiza
        /// - FirstOrDefault() con condición específica
        /// - SaveChanges() solo si hay cambios
        /// 
        /// CASOS DE USO:
        /// - DropDownList de Estado en grilla editable
        /// - Cambios de estado masivos o automatizados
        /// - Workflows de aprobación/rechazo
        /// - Sincronización de estados desde sistemas externos
        /// 
        /// PARÁMETROS:
        /// - codigoAutorizante: string - Código del autorizante O redeterminación
        /// - estadoId: int - ID del nuevo estado a asignar
        /// 
        /// RETORNO:
        /// - true: Estado actualizado correctamente en cualquiera de las dos tablas
        /// - false: Código no encontrado en ninguna tabla o error en actualización
        /// 
        /// EJEMPLO DE CÓDIGOS:
        /// - Autorizante: "AUT-2024-001"
        /// - Redeterminación: "AUT-2024-001-R1", "AUT-2024-001-R2"
        /// 
        /// TABLA DE MAPEO DE CAMPOS:
        /// - Autorizantes.EstadoId -> EstadoAutorizanteEF.Id
        /// - Redeterminaciones.EstadoRedetEFId -> EstadoAutorizanteEF.Id
        /// 
        /// CONSIDERACIONES:
        /// - Estados deben existir en tabla EstadoAutorizanteEF
        /// - Validación de FK aplicada automáticamente
        /// - Ambas tablas referencian la misma tabla de estados
        /// </summary>
        public bool ActualizarEstado(string codigoAutorizante, int estadoId)
        {
            using (var context = new IVCdbContext())
            {
                var autorizante = context.Autorizantes.FirstOrDefault(a => a.CodigoAutorizante == codigoAutorizante);
                if (autorizante != null)
                {
                    autorizante.EstadoId = estadoId;
                    return context.SaveChanges() > 0;
                }
                var redet = context.Redeterminaciones.FirstOrDefault(r => r.CodigoRedet == codigoAutorizante);
                if (redet != null)
                {
                    redet.EstadoRedetEFId = estadoId;
                    return context.SaveChanges() > 0;
                }
                return false;
            }
        }

        /// <summary>
        /// Actualiza únicamente el campo expediente de un autorizante específico.
        /// 
        /// ACTUALIZACIÓN PARCIAL OPTIMIZADA:
        /// - Modifica solo el campo Expediente, no toda la entidad
        /// - Búsqueda eficiente por CodigoAutorizante (clave de negocio)
        /// - SaveChanges() ejecuta UPDATE con solo el campo modificado
        /// 
        /// FUNCIONALIDAD ESPECÍFICA:
        /// - Solo maneja autorizantes (no redeterminaciones)
        /// - Campo Expediente típicamente contiene número de expediente administrativo
        /// - Usado para vincular autorizantes con trámites externos
        /// 
        /// CASOS DE USO:
        /// - Actualización de expedientes desde sistemas externos
        /// - Corrección de números de expediente
        /// - Vinculación posterior con trámites SADE
        /// - Carga masiva de expedientes faltantes
        /// 
        /// VALIDACIONES:
        /// - Verifica existencia del autorizante por código
        /// - No valida formato del expediente (acepta cualquier string)
        /// - Permite valores null o vacíos si el diseño de BD lo permite
        /// 
        /// PARÁMETROS:
        /// - codigoAutorizante: string - Código único del autorizante
        /// - expediente: string - Nuevo número/código de expediente
        /// 
        /// RETORNO:
        /// - true: Expediente actualizado correctamente
        /// - false: Autorizante no encontrado o error en actualización
        /// 
        /// LIMITACIONES:
        /// - Solo actualiza autorizantes, no redeterminaciones
        /// - No incluye validación de formato de expediente
        /// - No verifica duplicidad de expedientes
        /// 
        /// EXTENSIONES POSIBLES:
        /// - Agregar validación de formato de expediente
        /// - Soporte para redeterminaciones si es necesario
        /// - Logging de cambios para auditoría
        /// - Validación de unicidad de expedientes
        /// </summary>
        public bool ActualizarExpediente(string codigoAutorizante, string expediente)
        {
            using (var context = new IVCdbContext())
            {
                var autorizante = context.Autorizantes.FirstOrDefault(a => a.CodigoAutorizante == codigoAutorizante);
                if (autorizante == null) return false;
                autorizante.Expediente = expediente;
                return context.SaveChanges() > 0;
            }
        }
        #endregion
    }
}
