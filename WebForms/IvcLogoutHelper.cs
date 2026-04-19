using System;
using System.Web;
using System.Web.Configuration;
using Owin;

namespace WebForms
{
    /// <summary>
    /// Helper centralizado para el cierre de sesión en IVC.
    /// Limpia la sesión OWIN y de ASP.NET, inhibe caché y redirige al login de Auth.Web.
    /// </summary>
    internal static class IvcLogoutHelper
    {
        /// <summary>
        /// Realiza el cierre de sesión completo: limpia sesión, invalida cookie OWIN,
        /// deshabilita caché de respuesta y redirige al login de Auth.Web.
        /// </summary>
        /// <param name="context">HttpContext actual (disponible como <c>this.Context</c> en Page/MasterPage).</param>
        public static void SignOutAndRedirect(HttpContext context)
        {
            PerformSignOut(context);
            var baseUrl = (WebConfigurationManager.AppSettings["AuthWebBaseUrl"]
                           ?? WebConfigurationManager.AppSettings["AuthWebUrl"]
                           ?? string.Empty).Trim().TrimEnd('/');

            var loginUrl = string.IsNullOrEmpty(baseUrl) ? "/Account/Login" : baseUrl + "/Account/Login";
            context.Response.Redirect(loginUrl, true);
        }

        private static void PerformSignOut(HttpContext context)
        {
            try
            {
                context.Session.Clear();
                context.Session.Abandon();
                context.GetOwinContext().Authentication.SignOut("Identity.Application");
                context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
                context.Response.Cache.SetExpires(DateTime.UtcNow.AddHours(-1));
                context.Response.Cache.SetNoStore();
                context.Response.AppendHeader("Pragma", "no-cache");
            }
            catch
            {
                // En caso de error parcial garantizamos al menos limpiar sesión y cookie
                context.Session?.Clear();
                context.Session?.Abandon();
                context.GetOwinContext().Authentication.SignOut("Identity.Application");
            }
        }
    }
}
