using System;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Script.Serialization;

namespace WebForms
{
    public partial class StartupPage : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var user = Context.GetOwinContext().Authentication.User;
            if (user?.Identity?.IsAuthenticated != true)
            {
                LitMessage.Text = "No autenticado. Inicie sesión desde el portal de autenticación.";
                return;
            }


            var returnUrl = Request.QueryString["returnUrl"];

            var firstPage = user.Claims.FirstOrDefault(c => c.Type == "first_page")?.Value;
            if (string.IsNullOrWhiteSpace(firstPage))
            {
                var permsJsonTmp = user.Claims.FirstOrDefault(c => c.Type == "perms_json")?.Value;
                firstPage = TryGetFirstPageFromPermissions(permsJsonTmp);
            }

            // If a returnUrl or a first_page exist, redirect there; otherwise show claims below
            if (!string.IsNullOrWhiteSpace(returnUrl))
            {
                Response.Redirect(returnUrl, endResponse: false);
                return;
            }

            if (!string.IsNullOrWhiteSpace(firstPage))
            {
                Response.Redirect(firstPage, endResponse: false);
                return;
            }

            var claimsHtml = new System.Text.StringBuilder();
            claimsHtml.AppendLine("<h3>No se encontraron páginas habilitadas para el usuario.</h3>");

            claimsHtml.AppendLine("<h4>Todos los claims</h4><ul>");
            foreach (var claim in user.Claims)
            {
                claimsHtml.AppendLine($"<li><strong>{HttpUtility.HtmlEncode(claim.Type)}</strong>: {HttpUtility.HtmlEncode(claim.Value)}</li>");
            }
            claimsHtml.AppendLine("</ul>");

            LitMessage.Text = claimsHtml.ToString();
        }

        private static string TryGetFirstPageFromPermissions(string permsJson)
        {
            if (string.IsNullOrWhiteSpace(permsJson))
            {
                return null;
            }

            try
            {
                var serializer = new JavaScriptSerializer();
                var payload = serializer.Deserialize<PermissionsPayload>(permsJson);
                return payload?.pages?.FirstOrDefault()?.url;
            }
            catch
            {
                return null;
            }
        }

        private sealed class PermissionsPayload
        {
            public PagePermission[] pages { get; set; }
        }

        private sealed class PagePermission
        {
            public string url { get; set; }
            public string[] actions { get; set; }
        }
    }
}