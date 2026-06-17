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
        protected string AdminPanelUrl { get; private set; } = "/admin";

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
            var currentUser = UserHelper.GetFullCurrentUser();
            bool isAdmin = currentUser?.Tipo == true;
            if (principal != null)
            {
                var authWebBase = (Environment.GetEnvironmentVariable("AuthWeb__BaseUrl")
                    ?? System.Configuration.ConfigurationManager.AppSettings["AuthWebBaseUrl"]
                    ?? string.Empty).TrimEnd('/');
                AdminPanelUrl = string.IsNullOrWhiteSpace(authWebBase) ? "/admin" : authWebBase + "/admin";
                var currentClientId = GetCurrentClientId(principal);
                var availableAppIds = principal.Claims
                    .Where(c => c.Type == "available_app"
                                && !string.IsNullOrWhiteSpace(c.Value))
                    .Select(c => c.Value)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                OtherApps = availableAppIds
                    .Where(clientId => isAdmin || string.IsNullOrWhiteSpace(currentClientId) || !string.Equals(clientId, currentClientId, StringComparison.OrdinalIgnoreCase))
                    .Select(clientId => new AppLinkItem
                    {
                        Label = GetAppDisplayName(clientId),
                        Url = authWebBase + "/connect/switch-app?clientId=" + Uri.EscapeDataString(clientId)
                    })
                    .ToList();

                if (phAppSwitcher != null)
                {
                    phAppSwitcher.Visible = isAdmin || availableAppIds.Count > 1;
                }
            }

            if (rptOtherApps != null)
            {
                rptOtherApps.DataSource = OtherApps;
                rptOtherApps.DataBind();
            }

            if (phAppSwitcher != null)
            {
                phAppSwitcher.Visible = isAdmin || phAppSwitcher.Visible;
            }
            if (phAdminPanelDivider != null)
            {
                phAdminPanelDivider.Visible = isAdmin && OtherApps.Count > 0;
            }
            if (phAdminPanelLink != null)
            {
                phAdminPanelLink.Visible = isAdmin;
            }
            if (lnkAdminPanel != null)
            {
                lnkAdminPanel.NavigateUrl = AdminPanelUrl;
            }
        }

        private static string GetCurrentClientId(ClaimsPrincipal principal)
        {
            var activeApp = principal.Claims
                .FirstOrDefault(c => string.Equals(c.Type, "app", StringComparison.OrdinalIgnoreCase)
                                  && !string.IsNullOrWhiteSpace(c.Value))
                ?.Value;

            if (!string.IsNullOrWhiteSpace(activeApp))
            {
                return activeApp;
            }

            return Environment.GetEnvironmentVariable("AuthWeb__ClientId")
                ?? System.Configuration.ConfigurationManager.AppSettings["AuthWebClientId"];
        }

        private static string GetAppDisplayName(string clientId) => clientId;

        protected void btnCerrarSesion_Click(object sender, EventArgs e)
        {
            IvcLogoutHelper.SignOutAndRedirect(Context);
        }
    }
}
