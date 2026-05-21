using Negocio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebForms
{
    public partial class Admin : System.Web.UI.MasterPage
    {
        public class AppLinkItem
        {
            public string Label { get; set; }
            public string Url { get; set; }
        }

        protected List<AppLinkItem> OtherApps { get; private set; } = new List<AppLinkItem>();

        protected void Page_Init(object sender, EventArgs e)
        {
            // Comprobación básica de usuario logueado
            if (Session["Usuario"] == null)
            {
                UserHelper.EnsureSessionUserFromClaims();
            }

            if (Session["Usuario"] == null)
            {
                Response.Redirect("Startup.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                Response.End();
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // ...existing code...
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            BindAppSwitcher();
        }

        private void BindAppSwitcher()
        {
            var principal = HttpContext.Current?.User as ClaimsPrincipal;
            if (principal != null)
            {
                var authWebBase = (Environment.GetEnvironmentVariable("AuthWeb__BaseUrl")
                    ?? System.Configuration.ConfigurationManager.AppSettings["AuthWebBaseUrl"]
                    ?? string.Empty).TrimEnd('/');
                const string currentClientId = "PlaniLocal";
                OtherApps = principal.Claims
                    .Where(c => c.Type == "available_app"
                                && !string.Equals(c.Value, currentClientId, StringComparison.OrdinalIgnoreCase))
                    .Select(c => new AppLinkItem
                    {
                        Label = GetAppDisplayName(c.Value),
                        Url = authWebBase + "/connect/switch-app?clientId=" + Uri.EscapeDataString(c.Value)
                    })
                    .ToList();
            }

            if (rptOtherApps != null)
            {
                rptOtherApps.DataSource = OtherApps;
                rptOtherApps.DataBind();
            }

            if (phAppSwitcher != null)
            {
                phAppSwitcher.Visible = OtherApps.Count > 0;
            }
        }

        private static string GetAppDisplayName(string clientId)
        {
            switch (clientId)
            {
                case "sai":
                    return "Sistema de Administracion de Inventario";
                case "PlaniLocal":
                    return "Administracion Financiera";
                default:
                    return clientId;
            }
        }

        protected void btnCerrarSesion_Click(object sender, EventArgs e)
        {
            IvcLogoutHelper.SignOutAndRedirect(Context);
        }
    }
}
