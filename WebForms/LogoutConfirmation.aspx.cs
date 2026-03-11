using System;
using System.Web;
using System.Web.UI;
using Owin;

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
                Session.Clear();
                Session.Abandon();
                Context.GetOwinContext().Authentication.SignOut("Identity.Application");
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                Response.Cache.SetExpires(DateTime.UtcNow.AddHours(-1));
                Response.Cache.SetNoStore();
                Response.AppendHeader("Pragma", "no-cache");
            }
            catch
            {
                Session.Clear();
                Session.Abandon();
                Context.GetOwinContext().Authentication.SignOut("Identity.Application");
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
