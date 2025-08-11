using System;
using System.Web;
using System.Web.Security;
using System.Web.UI;

namespace WebForms
{
    public partial class Logout : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            LogoutAndRedirect();
        }

        private void LogoutAndRedirect()
        {
            try
            {
                // Capturar nombre usuario antes de limpiar (opcional para logs)
                string usuarioNombre = "";
                if (Session["Usuario"] != null)
                {
                    var usuario = (Dominio.Usuario)Session["Usuario"];
                    usuarioNombre = usuario.Nombre ?? "Usuario desconocido";
                }

                // Limpiar sesión
                Session.Clear();
                Session.Abandon();
                
                // Limpiar cookies de autenticación
                if (Request.Cookies[FormsAuthentication.FormsCookieName] != null)
                {
                    HttpCookie authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, "");
                    authCookie.Expires = DateTime.Now.AddYears(-1);
                    Response.Cookies.Add(authCookie);
                }
                
                // Log opcional
                // Logger.LogInfo($"Usuario {usuarioNombre} cerró sesión: {DateTime.Now}");
                
                // REDIRECCIÓN INMEDIATA - No mostrar nada
                Response.Redirect("LogoutConfirmation.aspx", true);
                
            }
            catch (Exception ex)
            {
                // Aún así limpiar y redirigir
                Session.Clear();
                Session.Abandon();
                
                // Redirigir con parámetro de error (opcional)
                Response.Redirect("LogoutConfirmation.aspx?error=1", true);
            }
        }
    }
}
