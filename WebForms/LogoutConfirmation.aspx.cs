using System;
using System.Web;
using System.Web.Security;
using System.Web.UI;

namespace WebForms
{
    public partial class LogoutConfirmation : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                LogoutUser();
            }
        }

        private void LogoutUser()
        {
            try
            {
                // Log del usuario que está cerrando sesión (opcional)
                string usuarioNombre = "";
                if (Session["Usuario"] != null)
                {
                    var usuario = (Dominio.Usuario)Session["Usuario"];
                    usuarioNombre = usuario.Nombre ?? "Usuario desconocido";
                    // Aquí podrías registrar el logout en base de datos si es necesario
                }

                // Limpiar todas las variables de sesión
                Session.Clear();

                // Abandonar la sesión completamente
                Session.Abandon();

                // Limpiar cookies de autenticación si las hay
                if (Request.Cookies[FormsAuthentication.FormsCookieName] != null)
                {
                    HttpCookie authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, "");
                    authCookie.Expires = DateTime.Now.AddYears(-1);
                    Response.Cookies.Add(authCookie);
                }

                // Limpiar cache del navegador para prevenir botón "atrás"
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                Response.Cache.SetExpires(DateTime.UtcNow.AddHours(-1));
                Response.Cache.SetNoStore();
                Response.AppendHeader("Pragma", "no-cache");

                // Log del logout exitoso (opcional)
                // Logger.LogInfo($"Usuario {usuarioNombre} cerró sesión exitosamente: {DateTime.Now}");

            }
            catch (Exception ex)
            {
                // Manejo de errores - aún así limpiar lo que se pueda
                Session.Clear();
                Session.Abandon();

                // Log del error pero no redirigir - mostrar mensaje de error en la misma página
                // Logger.LogError($"Error durante logout: {ex.Message}");

                // Opcional: mostrar mensaje de error en la página
                // ClientScript.RegisterStartupScript(this.GetType(), "alert", 
                //     "alert('Logout completado con advertencias.');", true);
            }
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            // Asegurar que no haya cache de esta página
            Response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
            Response.Headers.Add("Pragma", "no-cache");
            Response.Headers.Add("Expires", "0");
        }
    }
}
