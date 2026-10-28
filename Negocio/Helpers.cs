using Dominio;
using Dominio.DTO;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Negocio.Infraestructura;
using System.Security.Claims;

namespace Negocio
{
    /// <summary>
    /// Clase helper para exportar datos a Excel (.xlsx)
    /// </summary>
    public static class ExcelHelper
    {
        /// <summary>
        /// Exporta datos de cualquier tipo a Excel usando un mapeo (Header -> RutaPropiedad)
        /// </summary>
        /// <summary>
        /// Genera un libro Excel (.xlsx) con una hoja "Datos": una columna por entrada del mapeo
        /// (encabezado -> ruta de propiedad) y una fila por elemento. Devuelve el archivo en memoria;
        /// enviarlo al navegador es responsabilidad de la capa web.
        /// </summary>
        public static byte[] GenerarXlsx<T>(IEnumerable<T> datos, Dictionary<string, string> mapeoColumnas)
        {
            if (mapeoColumnas == null || mapeoColumnas.Count == 0)
                throw new ArgumentException("El mapeo de columnas está vacío");

            if (datos == null || !datos.Any())
                throw new ArgumentException("No hay datos para exportar");

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Datos");
                int fila = 1;
                int columnaActual = 1;

                // Normalizar mapeo para facilitar comparaciones
                Dictionary<string, string> mapeoNormalizado = new Dictionary<string, string>();
                foreach (var kvp in mapeoColumnas)
                {
                    // kvp.Key -> texto del encabezado a imprimir
                    // kvp.Value -> ruta de la propiedad en el objeto
                    string claveNormalizada = NormalizarTexto(kvp.Key);
                    mapeoNormalizado[claveNormalizada] = kvp.Value;
                }

                
                // Agregar encabezados desde el mapeo (usar la clave original como texto)
                foreach (var kvp in mapeoColumnas)
                {
                    worksheet.Cells[fila, columnaActual].Value = kvp.Key;
                    FormatearEncabezado(worksheet.Cells[fila, columnaActual]);
                    columnaActual++;
                }

                // Agregar datos
                fila++;
                foreach (var item in datos)
                {
                    columnaActual = 1;
                    foreach (var kvp in mapeoColumnas)
                    {
                        string header = kvp.Key;
                        string rutaPropiedad = kvp.Value;

                        object valor = ObtenerValorPorRuta(item, rutaPropiedad);

                        // Formatear según tipo de dato
                        if (valor is decimal || valor is double || valor is float)
                        {
                            worksheet.Cells[fila, columnaActual].Value = Convert.ToDouble(valor);
                            worksheet.Cells[fila, columnaActual].Style.Numberformat.Format = "#,##0.00";
                        }
                        else if (valor is DateTime)
                        {
                            worksheet.Cells[fila, columnaActual].Value = (DateTime)valor;
                            worksheet.Cells[fila, columnaActual].Style.Numberformat.Format = "dd-MM-yyyy";
                        }
                        else
                        {
                            worksheet.Cells[fila, columnaActual].Value = valor?.ToString();
                        }

                        columnaActual++;
                    }
                    fila++;
                }

                AjustarAnchosColumnas(worksheet, columnaActual - 1);
                return package.GetAsByteArray();
            }
        }

        /// <summary>
        /// Normaliza un texto para comparaciones consistentes: minúsculas y sin acentos
        /// </summary>
        private static string NormalizarTexto(string texto)
        {
            if (string.IsNullOrEmpty(texto))
                return string.Empty;

            // Convertir a minúsculas
            texto = texto.ToLowerInvariant();

            // Eliminar acentos
            string normalizedString = texto.Normalize(NormalizationForm.FormD);
            StringBuilder stringBuilder = new StringBuilder();

            foreach (char c in normalizedString)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    stringBuilder.Append(c);
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        /// <summary>
        /// Obtiene el valor de una propiedad anidada usando una ruta de acceso
        /// Ejemplo: "Autorizante.Obra.Area.Nombre"
        /// </summary>
        /// <param name="obj">Objeto base</param>
        /// <param name="rutaPropiedad">Ruta de propiedades separadas por puntos</param>
        /// <returns>Valor de la propiedad o null si no se encuentra</returns>
        private static object ObtenerValorPorRuta(object obj, string rutaPropiedad)
        {
            if (obj == null || string.IsNullOrEmpty(rutaPropiedad))
                return null;

            string[] propiedades = rutaPropiedad.Split('.');
            object resultado = obj;

            // Navegar por cada nivel de propiedad
            foreach (string nombrePropiedad in propiedades)
            {
                if (resultado == null)
                    return null;

                Type tipo = resultado.GetType();
                var propiedad = tipo.GetProperty(nombrePropiedad);

                if (propiedad == null)
                    return null;

                // Obtener el valor de esta propiedad y continuar
                resultado = propiedad.GetValue(resultado);
            }

            return resultado;
        }
        /// <summary>
        /// Formatea una celda como encabezado
        /// </summary>
        private static void FormatearEncabezado(ExcelRange cell)
        {
            cell.Style.Font.Bold = true;
            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
        }

        /// <summary>
        /// Ajusta el ancho de las columnas en función del contenido
        /// </summary>
        private static void AjustarAnchosColumnas(ExcelWorksheet worksheet, int columnCount)
        {
            for (int i = 1; i <= columnCount; i++)
            {
                worksheet.Column(i).AutoFit();
            }
        }

    }

    public static class UserHelper
    {

        /// <summary>
        /// Obtiene el usuario actual completo desde la sesión
        /// </summary>
        public static UsuarioEF GetFullCurrentUser()
        {
            // Siempre pasar por EnsureSessionUserFromClaims: valida que el usuario cacheado en
            // Session corresponda al principal autenticado actual antes de devolverlo.
            var userEntity = EnsureSessionUserFromClaims();
            if (userEntity != null)
            {
                return new UsuarioEF
                {
                    Id = userEntity.Id,
                    Nombre = userEntity.Nombre,
                    Correo = userEntity.Correo,
                    Tipo = userEntity.Tipo,
                    Estado = userEntity.Estado,
                    Area = userEntity.Area,
                    AreaIds = userEntity.AreaIds,
                    IvcAreaIds = userEntity.IvcAreaIds,
                    AreaId = userEntity.Area?.Id ?? 0,
                    IsPlanningOpenOverride = userEntity.IsPlanningOpenOverride,
                };
            }

            // Si no hay usuario en sesión, devolver un usuario por defecto sin área
            return new UsuarioEF
            {
                Id = 0,
                Nombre = "Usuario null",
                Correo = null,
                Tipo = false, // Usuario normal por defecto
                Estado = false,
                AreaId = 0, // 0 significa sin filtro de área
            };
        }

        /// <summary>
        /// Clave de Session que guarda el NameIdentifier del principal con el que se construyó Session["Usuario"].
        /// </summary>
        private const string SessionUsuarioAuthIdKey = "UsuarioAuthId";

        /// <summary>
        /// Si hay claims autenticados, asegura Session["Usuario"] y devuelve el usuario; si no, devuelve null.
        ///
        /// El usuario cacheado sólo se reutiliza si pertenece al mismo principal (NameIdentifier) que
        /// está autenticado ahora. La sesión ASP.NET (ASP.NET_SessionId) es independiente de la cookie
        /// compartida de Auth.Web: si otra persona inicia sesión en el mismo navegador, o si el usuario
        /// vuelve a loguearse tras un cambio de roles/áreas, Session["Usuario"] seguiría teniendo el
        /// nombre, el flag de administrador y las áreas anteriores. SAI/SAF no tienen este problema
        /// porque derivan todo del ClaimsPrincipal en cada request.
        /// </summary>
        public static UsuarioEF EnsureSessionUserFromClaims()
        {
            var contexto = ContextoWeb.Actual;
            if (contexto == null || !contexto.SesionDisponible)
            {
                return null;
            }

            var principal = contexto.Usuario;
            if (principal?.Identity?.IsAuthenticated != true)
            {
                // Sin principal autenticado no se devuelve ningún usuario cacheado. Si había uno, se limpia
                // toda la Session: las grillas y exportaciones cachean listas ya filtradas por área.
                if (contexto.ObtenerDeSesion("Usuario") != null)
                    contexto.LimpiarSesion();
                return null;
            }

            var identidadActual = ClaveDeIdentidad(principal);

            if (contexto.ObtenerDeSesion("Usuario") is UsuarioEF existing
                && string.Equals(contexto.ObtenerDeSesion(SessionUsuarioAuthIdKey) as string, identidadActual, StringComparison.Ordinal))
            {
                return existing;
            }

            // Cambió la persona, o la misma persona volvió a loguearse con otros roles/áreas/permisos.
            // Las listas cacheadas en Session (GridDataAutorizantes, CertificadosCompleto, CacheSade, etc.)
            // fueron calculadas con la identidad anterior: se descartan todas.
            if (contexto.ObtenerDeSesion("Usuario") != null)
                contexto.LimpiarSesion();

            var built = BuildUserFromClaims(principal);
            if (built == null)
            {
                return null;
            }

            contexto.GuardarEnSesion("Usuario", built);
            contexto.GuardarEnSesion(SessionUsuarioAuthIdKey, identidadActual);
            return built;
        }

        /// <summary>
        /// Identidad efectiva del principal: NameIdentifier más los claims que afectan a lo que el usuario
        /// puede ver o hacer en IVC (roles, áreas y versión de permisos). Si cualquiera cambia, el
        /// UsuarioEF cacheado deja de ser válido aunque sea la misma persona.
        /// </summary>
        private static string ClaveDeIdentidad(ClaimsPrincipal principal)
        {
            string Join(string type) => string.Join(",", principal.Claims
                .Where(c => string.Equals(c.Type, type, StringComparison.OrdinalIgnoreCase))
                .Select(c => c.Value)
                .OrderBy(v => v, StringComparer.Ordinal));

            return string.Join("|",
                principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty,
                principal.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty,
                Join(ClaimTypes.Role),
                Join("area"),
                principal.FindFirst("perms_version")?.Value ?? string.Empty);
        }

        private static UsuarioEF BuildUserFromClaims(ClaimsPrincipal principal)
        {
            if (principal == null)
            {
                return null;
            }

            var name = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var email = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var userIdValue = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            int.TryParse(userIdValue, out var parsedId);

            // El claim "area" ahora contiene IDs enteros de Auth.Web (no nombres)
            var authAreaIds = principal.Claims
                .Where(c => string.Equals(c.Type, "area", StringComparison.OrdinalIgnoreCase))
                .Select(c => c.Value)
                .Where(v => !string.IsNullOrWhiteSpace(v) && int.TryParse(v, out _))
                .Select(int.Parse)
                .ToList();

            var roles = principal.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .ToArray();

            var isAdmin = roles.Any(r =>
                r.Equals("Admin", StringComparison.OrdinalIgnoreCase)
                || r.Equals("Administrador", StringComparison.OrdinalIgnoreCase)
                || r.Equals("Administradores", StringComparison.OrdinalIgnoreCase));

            var displayName = !string.IsNullOrWhiteSpace(name) ? name : email;

            // Resolver áreas IVC a partir de los AuthAreaIds del claim
            AreaEF primaryArea = null;
            List<int> ivcAreaIdsList = new List<int>();
            bool isPlanningOpenOverride = false;
            if (authAreaIds.Count > 0 || !string.IsNullOrWhiteSpace(userIdValue))
            {
                try
                {
                    using (var context = new IVCdbContext())
                    {
                        if (authAreaIds.Count > 0)
                        {
                            var ivcAreas = context.Areas.AsNoTracking()
                                .Where(a => a.AuthAreaId != null && authAreaIds.Contains(a.AuthAreaId.Value))
                                .ToList();

                            primaryArea = ivcAreas.FirstOrDefault();
                            ivcAreaIdsList = ivcAreas.Select(a => a.Id).ToList();
                        }

                        // Upsert en UsuariosVinculados: cargar override y registrar al usuario si es su primer login
                        if (!string.IsNullOrWhiteSpace(userIdValue))
                        {
                            var ivcUser = context.UsuariosVinculados.FirstOrDefault(u => u.AuthUserId == userIdValue);
                            if (ivcUser == null)
                            {
                                ivcUser = new UsuarioVinculadoEF
                                {
                                    AuthUserId = userIdValue,
                                    Nombre = displayName ?? "Usuario",
                                    IsPlanningOpenOverride = false
                                };
                                context.UsuariosVinculados.Add(ivcUser);
                                context.SaveChanges();
                            }
                            else
                            {
                                // Refrescar nombre si cambió en Auth.Web
                                if (!string.IsNullOrWhiteSpace(displayName) && ivcUser.Nombre != displayName)
                                {
                                    ivcUser.Nombre = displayName;
                                    context.SaveChanges();
                                }
                                isPlanningOpenOverride = ivcUser.IsPlanningOpenOverride;
                            }
                        }
                    }
                }
                catch (Exception dbEx)
                {
                    // Si falla la DB no se construye un usuario parcial: quedaría cacheado en Session
                    // durante toda la sesión sin áreas resueltas (y antes, al no filtrar por lista vacía,
                    // veía datos de todas las áreas). Se propaga para que Application_Error muestre el error.
                    Trace.TraceError("[BuildUserFromClaims] Error resolviendo áreas IVC desde DB. AuthAreaIds=[{0}]. Ex: {1}",
                        string.Join(",", authAreaIds), dbEx);
                    throw new InvalidOperationException("No se pudo resolver el usuario actual contra la base de datos de IVC.", dbEx);
                }
            }

            return new UsuarioEF
            {
                Id = parsedId,
                Nombre = displayName ?? "Usuario",
                Correo = email,
                Tipo = isAdmin,
                Estado = true,
                AreaIds = authAreaIds,
                Area = primaryArea,
                IvcAreaIds = ivcAreaIdsList,
                IsPlanningOpenOverride = isPlanningOpenOverride,
            };
        }

        /// <summary>
        /// Devuelve true si el usuario actual es administrador
        /// </summary>
        public static bool IsUserAdmin()
        {
            UsuarioEF user = GetFullCurrentUser();
            return user.Tipo; // true: Administrador, false: Usuario normal
        }

        /// <summary>
        /// Devuelve true si el usuario actual pertenece a un área específica (por ID de IVC).
        /// La comparación se hace vía AuthAreaId: busca el AuthAreaId del área IVC y verifica
        /// que esté en los AreaIds del usuario (leídos del claim "area" de Auth.Web).
        /// </summary>
        public static bool IsUserInArea(int ivcAreaId)
        {
            var user = GetFullCurrentUser();
            if (user == null) return false;
            if (user.Tipo) return false;
            if (user.IvcAreaIds == null || user.IvcAreaIds.Count == 0) return false;
            return user.IvcAreaIds.Contains(ivcAreaId);
        }

        /// <summary>
        /// Lista de áreas IVC con la que debe filtrarse la data de un usuario no administrador.
        /// Nunca devuelve null: un usuario sin áreas resueltas obtiene una lista vacía y, por lo
        /// tanto, ninguna fila (fail-closed, igual que Auth.Web cuando el usuario no tiene áreas).
        /// </summary>
        public static List<int> AreaIdsParaFiltro(UsuarioEF usuario)
        {
            return usuario?.IvcAreaIds ?? new List<int>();
        }

        // Reglas de edición. Son las mismas que usan Site.Master y OnPreRender de las páginas para
        // ocultar botones y columnas; se repiten acá para verificarlas también en los handlers de
        // postback, porque ocultar un control no impide que el postback llegue (patrón de SAF).

        /// <summary>
        /// Puede dar de alta/modificar/eliminar en el módulo de planificación
        /// (Obras, Autorizantes, Certificados, Legítimos): administrador, o usuario normal que no sea
        /// de Redeterminaciones ni Secretaría con la planificación abierta (global o por override).
        /// </summary>
        public static bool PuedeModificarPlanificacion()
        {
            var user = GetFullCurrentUser();
            // Estado == false es el usuario "por defecto" que se devuelve sin sesión autenticada: nunca puede modificar.
            if (user == null || !user.Estado) return false;
            if (IsUserInArea(Dominio.IvcAreaIds.Redeterminaciones) || IsUserInArea(Dominio.IvcAreaIds.Secretaria)) return false;
            return user.Tipo || ABMPlaniNegocio.GetIsPlanningOpen() || user.IsPlanningOpenOverride;
        }

        /// <summary>
        /// Puede actualizar inline el expediente y el estado de un autorizante en la grilla de Autorizantes.
        /// Además de quienes pueden modificar planificación, el área Redeterminaciones hace el seguimiento
        /// del trámite y edita esos dos campos aunque no pueda crear, modificar por modal ni eliminar.
        /// </summary>
        public static bool PuedeEditarSeguimientoAutorizante()
        {
            var user = GetFullCurrentUser();
            if (user == null || !user.Estado) return false;
            return PuedeModificarPlanificacion() || IsUserInArea(Dominio.IvcAreaIds.Redeterminaciones);
        }

        /// <summary>
        /// Puede modificar en Formulación: administrador, o usuario normal que no sea de
        /// Redeterminaciones ni Secretaría con la formulación abierta.
        /// </summary>
        public static bool PuedeModificarFormulacion()
        {
            var user = GetFullCurrentUser();
            if (user == null || !user.Estado) return false;
            if (user.Tipo) return true;
            if (IsUserInArea(Dominio.IvcAreaIds.Redeterminaciones) || IsUserInArea(Dominio.IvcAreaIds.Secretaria)) return false;
            return ABMPlaniNegocio.GetIsFormulationOpen();
        }

        /// <summary>
        /// Puede modificar en la página de Redeterminaciones: cualquier usuario con acceso a la página
        /// salvo Secretaría (área de sólo lectura).
        /// </summary>
        public static bool PuedeModificarRedeterminaciones()
        {
            var user = GetFullCurrentUser();
            if (user == null || !user.Estado) return false;
            if (user.Tipo) return true;
            return !IsUserInArea(Dominio.IvcAreaIds.Secretaria);
        }

    }
}

