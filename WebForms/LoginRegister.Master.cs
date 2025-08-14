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
            if (currentPage.Equals("LogoutConfirmation.aspx", StringComparison.OrdinalIgnoreCase) ||
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
                    // Response.Redirect("~/Login.aspx");
                }
            }

            var authType = HttpContext.Current.User.Identity.AuthenticationType;

            Debug.WriteLine("httpContext authenticated!!! contexto " + HttpContext.Current.User.Identity.IsAuthenticated);
            Debug.WriteLine("Página cargada: " + DateTime.Now);

            if (HttpContext.Current.User.Identity.IsAuthenticated && 
                (authType == "NTLM" || authType == "Kerberos" || authType == "Negotiate"))
            {
                Usuario usuario;
                UsuarioNegocio negocio = new UsuarioNegocio();
                try
                {
                    //string fullUserName = HttpContext.Current.User.Identity.Name;
                    //string[] parts = fullUserName.Split('\\');

                    var fullUserName = HttpContext.Current.User.Identity.Name ?? string.Empty;
                    var parts = fullUserName.Split(new[] { '\\' }, 2);

                    // If the user is in a domain, the format is DOMAIN\username
                    string domain = parts.Length == 2 ? parts[0] : string.Empty;
                    string userName = parts.Length == 2 ? parts[1] : parts[0];

                    //string userName = fullUserName.Contains("\\") ? fullUserName.Split('\\')[1] : fullUserName;

                    usuario = Usuario.CreateWithDomain(domain, userName);

                    if (negocio.LogearIntegSecur(usuario, userName))
                    {
                        Session.Add("Usuario", usuario);
                        if (Session["Usuario"] != null && ((Dominio.Usuario)Session["Usuario"]).Tipo == true)
                        {
                            Debug.WriteLine("Next page BdProyectos.aspx: " + DateTime.Now);
                            Response.Redirect("BdProyectos.aspx", false);
                        }
                        else
                        {
                            if (((Dominio.Usuario)Session["Usuario"]).Estado == true)
                            {
                                if (((Dominio.Usuario)Session["Usuario"]).Area != null && ((Dominio.Usuario)Session["Usuario"]).Area.Id == 16)
                                {
                                    Debug.WriteLine("Next page Redeterminaciones.aspx: " + DateTime.Now);

                                    Response.Redirect("Redeterminaciones.aspx", false);
                                }
                                else
                                {
                                    Debug.WriteLine("Next page Obras.aspx: " + DateTime.Now);
                                    Response.Redirect("Obras.aspx", false);
                                }
                            }
                            else
                            {
                                Session.Add("error", "Usuario no habilitado a ingresar, solicitar acceso al area correspondiente.");
                                Response.Redirect("Error.aspx", false);
                            }
                        }
                    }
                    else
                    {
                        Session.Add("error", "Usuario o Contraseña Incorrectos");
                        Response.Redirect("Error.aspx", false);
                    }

                }
                catch (Exception ex)
                {

                    Debug.WriteLine("Entro al catch exception!!!!!!!!!!!!!!!!!!!!!!");
                    Session.Add("error", ex.ToString());
                    Response.Redirect("Error.aspx");
                }
            }

        }
        protected string GetRandomBackgroundImage()
        {
            string[] imagenes = {
                "Images/1- IVC.jpg",
                "Images/2- IVC.jpg",
                "Images/3- IVC.jpg",
                "Images/4- IVC.jpg",
                "Images/5- IVC.jpg",
                "Images/6- IVC.jpg",
                "Images/7- IVC.jpg",
                "Images/8- IVC.jpg",
                "Images/9- IVC.jpg",
                "Images/10- IVC.jpg",
                "Images/11- IVC.jpg",
                "Images/12- IVC.jpg",
                "Images/13- IVC.jpg",
                "Images/14- IVC.jpg",
                "Images/15- IVC.jpg",
                "Images/16- IVC.jpg",
                "Images/17- IVC.jpg",
                "Images/18- IVC.jpg",
                "Images/19- IVC.jpg",
                "Images/20- IVC.jpg",
                "Images/21- IVC.jpg",
                "Images/22- IVC.jpg",
                "Images/23- IVC.jpg"
            };

            Random random = new Random();
            int indiceAleatorio = random.Next(0, imagenes.Length);

            return imagenes[indiceAleatorio];
        }
    }
}