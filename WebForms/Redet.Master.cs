using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebForms
{
    public partial class Redet : System.Web.UI.MasterPage
    {
        protected void Page_Init(object sender, EventArgs e)
        {
            Debug.WriteLine("Pasa por master redet: " + DateTime.Now);

            // Skip authentication for certain pages
            string currentPage = System.IO.Path.GetFileName(Request.Url.AbsolutePath);
            if (currentPage.Equals("LogoutConfirmation.aspx", StringComparison.OrdinalIgnoreCase) ||
                currentPage.Equals("Error.aspx", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            // Comprobación básica de usuario logueado
            if (Session["Usuario"] == null)
            {
                Response.Redirect("Login.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                Response.End();
            }
        }


        protected void btnCerrarSession_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Response.Redirect("LogoutConfirmation.aspx", false);
        }

    }
}