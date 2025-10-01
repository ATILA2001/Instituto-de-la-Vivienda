using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Negocio{
    public class AuthModule : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            // Suscribirse al evento 'AuthenticateRequest', que se dispara en cada petición.
            context.AuthenticateRequest += OnAuthenticateRequest;
        }
        private void OnAuthenticateRequest(object sender, EventArgs e)
        {
            HttpApplication app = (HttpApplication)sender;
            HttpContext context = app.Context;
            string requestedPath = context.Request.Path.ToLower();
            var publicPaths = new List<string>
            {
                "/authentication.aspx",
                "/error.aspx",
            };
            if (publicPaths.Contains(requestedPath))
            {
                return;
            }
            HttpCookie authCookie = context.Request.Cookies["Jwt"];
            ControlarQueELUsuarioEsteLogueadoYAutorizado(requestedPath, authCookie);
        } 
        private void ControlarQueELUsuarioEsteLogueadoYAutorizado(string requestedPath, HttpCookie authCookie)
        {
            if (NoHayCookie(authCookie) || NoEsUnTokenValido(authCookie.Value))
            {
                RedirectToLogin();
                return; 
            }

            ControlarQueELUsuarioEsteAutorizado(requestedPath, authCookie.Value);
        }
        private void ControlarQueELUsuarioEsteAutorizado(string requestedPath, string token)
        {
            // TODO
        }
        private bool NoHayCookie(HttpCookie authCookie)
        {
            return authCookie == null || string.IsNullOrEmpty(authCookie.Value);
        }
        private bool NoEsUnTokenValido(string token)
        {
            return !TokenNegocio.Instance.EsTokenValido(token);
        }
        private void RedirectToLogin()
        {
            HttpContext.Current.Response.Redirect("~/Authentication.aspx", true);
        }

        public void Dispose()
        {
            // No se necesitan recursos para liberar en este módulo.
        }
    }
}