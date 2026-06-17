using System;
using System.Web.UI;

namespace WebForms
{
    public partial class LogoutConfirmation : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                IvcLogoutHelper.SignOutAndRedirect(Context);
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
