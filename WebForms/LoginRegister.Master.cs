using Dominio;
using Negocio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Diagnostics;
using System.Security.Principal;


namespace WebForms
{
    public partial class LoginRegister : System.Web.UI.MasterPage
    {
        protected void Page_Init(object sender, EventArgs e)
        {
            // Skip authentication for certain pages
            string currentPage = System.IO.Path.GetFileName(Request.Url.AbsolutePath);
            if (currentPage.Equals("Authentication.aspx", StringComparison.OrdinalIgnoreCase) ||
                currentPage.Equals("Error.aspx", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (!IsPostBack)
            {
                if (Page.User.Identity.IsAuthenticated)
                {
                    string username = Page.User.Identity.Name;
                    // Aquí podés usar el username para lógica de negocio, etc.
                    Debug.WriteLine("Usuario User.Identity: " + username);
                }
                else
                {
                    // Redirigir al login si no está autenticado
                    Response.Redirect("/Authentication.aspx");
                }
            }

        }
        protected string GetRandomBackgroundImage()
        {
            string[] imagenes = {
                "Images/1-IVC.jpg",
                "Images/2-IVC.jpg",
                "Images/3-IVC.jpg",
                "Images/4-IVC.jpg",
                "Images/5-IVC.jpg",
                "Images/6-IVC.jpg",
                "Images/7-IVC.jpg",
                "Images/8-IVC.jpg",
                "Images/9-IVC.jpg",
                "Images/10-IVC.jpg",
                "Images/11-IVC.jpg",
                "Images/12-IVC.jpg",
                "Images/13-IVC.jpg",
                "Images/14-IVC.jpg",
                "Images/15-IVC.jpg",
                "Images/16-IVC.jpg",
                "Images/17-IVC.jpg",
                "Images/18-IVC.jpg",
                "Images/19-IVC.jpg",
                "Images/20-IVC.jpg",
                "Images/21-IVC.jpg",
                "Images/22-IVC.jpg",
                "Images/23-IVC.jpg"
            };

            Random random = new Random();
            int indiceAleatorio = random.Next(0, imagenes.Length);

            // Ensure the returned path is resolved relative to the application root
            // so it works correctly regardless of the current page's folder.
            string relativePath = "~/" + imagenes[indiceAleatorio];
            return ResolveUrl(relativePath);
        }
    }
}