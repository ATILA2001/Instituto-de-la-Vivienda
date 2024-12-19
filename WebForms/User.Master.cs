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
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnCerrarSession_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Response.Redirect("Login.aspx", false);
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