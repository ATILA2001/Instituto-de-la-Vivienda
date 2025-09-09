using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebForms
{
    public partial class User : System.Web.UI.MasterPage
    {

        protected void Page_Init(object sender, EventArgs e)
        {
            // Comprobación básica de usuario logueado
            if (Session["Usuario"] == null)
            {
                Response.Redirect("Login.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                Response.End();
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // ...existing code...
        }

        protected void btnCerrarSession_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Response.Redirect("LogoutConfirmation.aspx", false);
        }

        protected void btnBuscar_Click(object sender, EventArgs e)
        {
            //Session["Busqueda"] = txtBuscar.Text.Trim();
            //Response.Redirect("Catalogo.aspx", false);
            //txtBuscar.Text = string.Empty;
        }

        protected void btnPerfil_Click(object sender, EventArgs e)
        {
            Response.Redirect("Perfil.aspx", false);
        }
    }
}