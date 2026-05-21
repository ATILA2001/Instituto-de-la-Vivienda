using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Script.Serialization;

namespace WebForms
{
    public partial class Inicio : System.Web.UI.Page
    {
        private static readonly HomePageLink[] Pages =
        {
            new HomePageLink("ObrasEF.aspx", "Obras", "Consulta y gestion de obras."),
            new HomePageLink("AutorizantesEF.aspx", "Autorizantes", "Administracion de autorizantes."),
            new HomePageLink("CertificadosEF.aspx", "Certificados", "Carga y seguimiento de certificados."),
            new HomePageLink("LegitimosEF.aspx", "Legitimos Abonos", "Gestion de legitimos abonos."),
            new HomePageLink("FormulacionEF.aspx", "Formulacion", "Formulacion presupuestaria."),
            new HomePageLink("BdProyectos.aspx", "Proyectos", "Base de proyectos."),
            new HomePageLink("MovimientosGestion.aspx", "Movimientos", "Movimientos de gestion."),
            new HomePageLink("LineasGestion.aspx", "Lineas de Gestion", "Lineas de gestion sin fuente financiera."),
            new HomePageLink("LineasGestionFF.aspx", "Lineas con FF", "Lineas de gestion con fuente financiera."),
            new HomePageLink("PanelConfiguracion.aspx", "Panel de Configuracion", "Configuracion administrativa."),
            new HomePageLink("RedeterminacionesEF.aspx", "Redeterminaciones", "Seguimiento de redeterminaciones."),
            new HomePageLink("AbmlBarrio.aspx", "Barrios", "Administracion de barrios."),
            new HomePageLink("AbmlEmpresa.aspx", "Empresas", "Administracion de empresas.")
        };

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack)
            {
                return;
            }

            var user = Context.GetOwinContext().Authentication.User;
            var visiblePages = IsAdmin(user)
                ? Pages
                : Pages.Where(page => CanAccess(user, page.Url)).ToArray();

            rptPages.DataSource = visiblePages;
            rptPages.DataBind();
            pnlEmpty.Visible = visiblePages.Length == 0;
        }

        private static bool IsAdmin(ClaimsPrincipal user)
        {
            if (user?.Identity?.IsAuthenticated != true)
            {
                return false;
            }

            return user.Claims.Any(c =>
                IsRoleClaim(c.Type)
                && (c.Value.Equals("Admin", StringComparison.OrdinalIgnoreCase)
                    || c.Value.Equals("Administrador", StringComparison.OrdinalIgnoreCase)
                    || c.Value.Equals("Administradores", StringComparison.OrdinalIgnoreCase)));
        }

        private static bool CanAccess(ClaimsPrincipal user, string pageUrl)
        {
            var permsJson = user?.Claims.FirstOrDefault(c => c.Type == "perms_json")?.Value;
            var requestedPath = NormalizePath(pageUrl);
            return ParseAllowedUrls(permsJson)
                .Select(NormalizePath)
                .Any(allowed => string.Equals(allowed, requestedPath, StringComparison.OrdinalIgnoreCase));
        }

        private static IEnumerable<string> ParseAllowedUrls(string permsJson)
        {
            if (string.IsNullOrWhiteSpace(permsJson))
            {
                return Enumerable.Empty<string>();
            }

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
            if (string.IsNullOrWhiteSpace(url))
            {
                return "/";
            }

            url = url.Trim();
            if (Uri.TryCreate(url, UriKind.Absolute, out var absolute))
            {
                url = string.IsNullOrWhiteSpace(absolute.PathAndQuery) ? "/" : absolute.PathAndQuery;
            }

            if (url.StartsWith("~/", StringComparison.Ordinal)) url = url.Substring(1);
            if (!url.StartsWith("/", StringComparison.Ordinal)) url = "/" + url;
            var queryIndex = url.IndexOf("?", StringComparison.Ordinal);
            return queryIndex >= 0 ? url.Substring(0, queryIndex) : url;
        }

        private static bool IsRoleClaim(string claimType)
        {
            return string.Equals(claimType, ClaimTypes.Role, StringComparison.OrdinalIgnoreCase)
                || string.Equals(claimType, "role", StringComparison.OrdinalIgnoreCase)
                || string.Equals(claimType, "roles", StringComparison.OrdinalIgnoreCase)
                || claimType.EndsWith("/role", StringComparison.OrdinalIgnoreCase);
        }

        protected sealed class HomePageLink
        {
            public HomePageLink(string url, string title, string description)
            {
                Url = url;
                Title = title;
                Description = description;
            }

            public string Url { get; }
            public string Title { get; }
            public string Description { get; }
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
