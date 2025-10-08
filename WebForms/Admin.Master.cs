using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebForms
{
    public partial class Admin : System.Web.UI.MasterPage
    {
        protected void Page_Init(object sender, EventArgs e)
        {
            // Comprobación básica de usuario logueado
            if (Session["Usuario"] == null)
            {
                Response.Redirect("Authentication.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                Response.End();
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // ...existing code...
        }
        protected void btnCerrarSesion_Click(object sender, EventArgs e) // codigo duplicado en site.master.cs
        {
            Session.Clear();
            Context.Request.Cookies.Clear();
            Response.Redirect("Authentication.aspx", false);
        }
    }
}