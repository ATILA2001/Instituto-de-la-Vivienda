using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebForms
{
    public partial class Error : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["error"] != null)
            {
                // HtmlEncode para evitar inyección de HTML desde la sesión
                lblMensaje.Text = Server.HtmlEncode(Session["error"].ToString());
                Session.Remove("error"); // Evita redirecciones repetidas
            }
        }
    }
}