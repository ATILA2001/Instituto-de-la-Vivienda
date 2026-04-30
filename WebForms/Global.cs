using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Web;
using System.Web.Configuration;
using System.Web.Script.Serialization;
using System.Web.UI;

namespace WebForms
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            // Configurar el modo de validaci�n no intrusiva
            ValidationSettings.UnobtrusiveValidationMode = UnobtrusiveValidationMode.WebForms;

            // Agregar el mapping de jQuery
            ScriptManager.ScriptResourceMapping.AddDefinition("jquery",
                new ScriptResourceDefinition
                {
                    Path = "~/Scripts/jquery-3.6.0.min.js",
                    DebugPath = "~/Scripts/jquery-3.6.0.js",
                    CdnPath = "https://code.jquery.com/jquery-3.6.0.min.js",
                    CdnDebugPath = "https://code.jquery.com/jquery-3.6.0.js"
                });
        }

        protected void Application_AuthorizeRequest(object sender, EventArgs e)
        {
            string requestedPath = Context.Request.Path;

            // Lista de rutas públicas que no requieren autenticación.
            var publicPaths = new List<string>
            {
                "/Startup.aspx",
                "/AccessDenied.aspx",
                "/LogoutConfirmation.aspx",
                "/Error.aspx",
                "/",
                "/ScriptResource.axd",
                "/WebResource.axd",
            };

            if (publicPaths.Any(p => requestedPath.EndsWith(p)))
            {
                return;
            }

            ControlarQueELUsuarioEsteLogueado(requestedPath);
        }

        private void ControlarQueELUsuarioEsteLogueado(string requestedPath)
        {
            var user = Context.GetOwinContext().Authentication.User;
            if (user?.Identity?.IsAuthenticated != true)
            {
                RedirectToLogin();
                return;
            }

            var isAdmin = user.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Any(c => c.Value.Equals("Admin", StringComparison.OrdinalIgnoreCase)
                       || c.Value.Equals("Administrador", StringComparison.OrdinalIgnoreCase)
                       || c.Value.Equals("Administradores", StringComparison.OrdinalIgnoreCase));

            // Los admins tienen acceso total — skip validación de perms_version y perms_json
            if (isAdmin) return;

            // Para usuarios normales: validar que la versión de permisos sigue vigente
            ValidarPermsVersion(user);

            if (!IsUserAuthorizedForPath(user.Claims.FirstOrDefault(c => c.Type == "perms_json")?.Value, requestedPath))
            {
                RedirectToAccessDenied(requestedPath);
            }
        }

        private void RedirectToLogin()
        {
            var target = BuildAuthLoginUrl();
            var returnUrl = Context.Request.Url?.AbsoluteUri;
            if (!string.IsNullOrWhiteSpace(returnUrl))
                target += "?returnUrl=" + HttpUtility.UrlEncode(returnUrl);
            Context.Response.Redirect(target, true);
        }

        private static string BuildAuthLoginUrl()
        {
            var baseUrl = Environment.GetEnvironmentVariable("AuthWeb__BaseUrl")
                ?? WebConfigurationManager.AppSettings["AuthWebBaseUrl"];

            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                return "/Account/Login";
            }

            baseUrl = baseUrl.Trim().TrimEnd('/');
            return baseUrl + "/Account/Login";
        }

        private void RedirectToAccessDenied(string returnUrl)
        {
            var target = "~/AccessDenied.aspx";
            if (!string.IsNullOrWhiteSpace(returnUrl))
            {
                target = target + "?returnUrl=" + HttpUtility.UrlEncode(returnUrl);
            }

            Context.Response.Redirect(target, true);
        }

        private static bool IsUserAuthorizedForPath(string permsJson, string requestedPath)
        {
            if (string.IsNullOrWhiteSpace(permsJson) || string.IsNullOrWhiteSpace(requestedPath))
            {
                return false;
            }

            var normalizedRequested = NormalizePath(requestedPath);

            foreach (var url in ParseAllowedUrls(permsJson))
            {
                if (string.IsNullOrWhiteSpace(url))
                {
                    continue;
                }

                var normalizedAllowed = NormalizePath(url);
                if (string.Equals(normalizedAllowed, normalizedRequested, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static IEnumerable<string> ParseAllowedUrls(string permsJson)
        {
            try
            {
                var serializer = new JavaScriptSerializer();
                var payload = serializer.Deserialize<PermissionsPayload>(permsJson);
                return payload?.pages?.Select(p => p?.url) ?? Enumerable.Empty<string>();
            }
            catch (Exception ex)
            {
                // perms_json malformado o con formato inesperado: el usuario verá "Access Denied" en todas las páginas.
                // Loguear para detectar cambios de formato entre Auth.Web e IVC.
                Trace.TraceError("[ParseAllowedUrls] No se pudo deserializar perms_json. Ex: {0}", ex);
                return Enumerable.Empty<string>();
            }
        }

        private static string NormalizePath(string url)
        {
            url = url.Trim();
            if (Uri.TryCreate(url, UriKind.Absolute, out var absolute))
            {
                url = string.IsNullOrWhiteSpace(absolute.PathAndQuery) ? "/" : absolute.PathAndQuery;
            }

            if (url.StartsWith("~/")) url = url.Substring(1);
            if (!url.StartsWith("/")) url = "/" + url;
            var queryIndex = url.IndexOf("?", StringComparison.Ordinal);
            return queryIndex >= 0 ? url.Substring(0, queryIndex) : url;
        }

        private sealed class PermissionsPayload
        {
            public PagePermission[] pages { get; set; }
        }

        private sealed class PagePermission
        {
            public string url { get; set; }
        }

        // DTO para deserializar la respuesta de GET /api/permissions/version
        private sealed class PermissionVersionDto
        {
            public int Version { get; set; }
        }

        /// <summary>
        /// Verifica que la versión de permisos en la cookie coincida con la actual en Auth.Web.
        /// Cachea el resultado en Session durante 5 minutos (TTL) para no llamar en cada request.
        /// Comportamiento fail-open: si Auth.Web no responde en 2 s, deja pasar.
        /// </summary>
        private void ValidarPermsVersion(System.Security.Claims.ClaimsPrincipal user)
        {
            var cookieVersion = user.Claims
                .FirstOrDefault(c => string.Equals(c.Type, "perms_version", StringComparison.OrdinalIgnoreCase))
                ?.Value;

            // Si la cookie no tiene perms_version (usuario antiguo o admin sin claim), fail open
            if (string.IsNullOrWhiteSpace(cookieVersion))
                return;

            // Verificar caché de sesión (TTL 5 min)
            var session = Context.Session;
            if (session != null)
            {
                var cachedVersion = session["PermsVersion_Cached"] as string;
                var cachedAt = session["PermsVersion_CachedAt"] as DateTime?;

                if (cachedVersion != null && cachedAt.HasValue
                    && (DateTime.UtcNow - cachedAt.Value).TotalMinutes < 5)
                {
                    if (!string.Equals(cookieVersion, cachedVersion, StringComparison.Ordinal))
                        RedirectToLogin();
                    return;
                }
            }

            // Llamar a Auth.Web: GET /api/permissions/version
            // Reenviamos la cookie de autenticación compartida
            string serverVersion = null;
            try
            {
                var baseUrl = Environment.GetEnvironmentVariable("AuthWeb__BaseUrl")
                    ?? WebConfigurationManager.AppSettings["AuthWebBaseUrl"]
                    ?? WebConfigurationManager.AppSettings["AuthWebUrl"];

                if (string.IsNullOrWhiteSpace(baseUrl))
                {
                    System.Diagnostics.Trace.TraceWarning("perms_version: AuthWebBaseUrl no configurado — fail open.");
                    return;
                }

                var url = baseUrl.Trim().TrimEnd('/') + "/api/permissions/version";

                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Timeout = 2000; // 2 segundos máximo
                request.Method = "GET";
                // Reenviar la cookie de sesión compartida para que el endpoint pueda autenticar
                var cookieHeader = Context.Request.Headers["Cookie"];
                if (!string.IsNullOrWhiteSpace(cookieHeader))
                    request.Headers["Cookie"] = cookieHeader;

                using (var response = (HttpWebResponse)request.GetResponse())
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    var json = reader.ReadToEnd();
                    var serializer = new JavaScriptSerializer();
                    var dto = serializer.Deserialize<PermissionVersionDto>(json);
                    serverVersion = dto?.Version.ToString();
                }
            }
            catch (Exception ex)
            {
                // Cualquier error (timeout, conexión rechazada, etc.) → fail open
                System.Diagnostics.Trace.TraceWarning("perms_version: Auth.Web no disponible, fail open. " + ex.Message);
                return;
            }

            // Actualizar caché de sesión con la versión obtenida
            if (session != null && serverVersion != null)
            {
                session["PermsVersion_Cached"] = serverVersion;
                session["PermsVersion_CachedAt"] = DateTime.UtcNow;
            }

            if (!string.Equals(cookieVersion, serverVersion, StringComparison.Ordinal))
                RedirectToLogin();
        }

    }
}