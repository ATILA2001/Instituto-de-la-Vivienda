using System;

namespace WebForms
{
    public partial class AccessDenied : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Un usuario autenticado sin ninguna página permitida (o cuya primera página es de administración)
        /// llega acá desde Startup o desde Admin.Master y no tiene otra salida: la cookie compartida sigue
        /// vigente y cualquier entrada vuelve a esta página. Cerrar sesión lo lleva al login de Auth.Web.
        /// </summary>
        protected void btnCerrarSesion_Click(object sender, EventArgs e)
        {
            IvcLogoutHelper.SignOutAndRedirect(Context);
        }
    }
}
