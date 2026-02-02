using System;
using System.Collections.Generic;
using System.Linq;
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
                "/Authentication.aspx",
                "/Startup.aspx",
                "/AccessDenied.aspx",
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

            if (!IsUserAuthorizedForPath(user.Claims.FirstOrDefault(c => c.Type == "perms_json")?.Value, requestedPath))
            {
                RedirectToAccessDenied(requestedPath);
            }
        }

        private void RedirectToLogin()
        {
            var target = BuildAuthLoginUrl();
            Context.Response.Redirect(target, true);
        }

        private static string BuildAuthLoginUrl()
        {
            var baseUrl = WebConfigurationManager.AppSettings["AuthWebBaseUrl"]
                ?? WebConfigurationManager.AppSettings["AuthWebUrl"];

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
            catch
            {
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

    }
}