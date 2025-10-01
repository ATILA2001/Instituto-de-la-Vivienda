using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.UI;
using Negocio;

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
                "/Error.aspx",
                "/",
                "/ScriptResource.axd",
                "/WebResource.axd",  
            };

            if (publicPaths.Any(p => requestedPath.EndsWith(p)))
            {
                return;
            }

            HttpCookie authCookie = Context.Request.Cookies["Jwt"];
            ControlarQueELUsuarioEsteLogueado(requestedPath, authCookie);
        }

        private void ControlarQueELUsuarioEsteLogueado(string requestedPath, HttpCookie authCookie)
        {
            if (NoHayCookie(authCookie) || NoEsUnTokenValido(authCookie.Value))
            {
                RedirectToLogin();
                return;
            }
        }

        private bool NoHayCookie(HttpCookie authCookie)
        {
            return authCookie == null || string.IsNullOrEmpty(authCookie.Value);
        }

        private bool NoEsUnTokenValido(string token)
        {
            return !TokenNegocio.Instance.EsTokenValido(token);
        }

        private void RedirectToLogin() => Context.Response.Redirect("~/Authentication.aspx", true);

    }
}