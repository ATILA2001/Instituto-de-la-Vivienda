using Dominio;
using Negocio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebForms
{
    public partial class LoginRegister : System.Web.UI.MasterPage
    {
        protected void Page_Init(object sender, EventArgs e)
        {
            var authType = HttpContext.Current.User.Identity.AuthenticationType;
            if (HttpContext.Current.User.Identity.IsAuthenticated && 
                (authType == "NTLM" || authType == "Kerberos"))
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

                    usuario = new Usuario(domain, userName);

                    if (negocio.LogearIntegSecur(userName, usuario))
                    {
                        Session.Add("Usuario", usuario);
                        if (Session["Usuario"] != null && ((Dominio.Usuario)Session["Usuario"]).Tipo == true)
                        {
                            Response.Redirect("BdProyectos.aspx", false);
                        }
                        else
                        {
                            if (((Dominio.Usuario)Session["Usuario"]).Estado == true)
                            {
                                if (((Dominio.Usuario)Session["Usuario"]).Area != null && ((Dominio.Usuario)Session["Usuario"]).Area.Id == 16)
                                {
                                    Response.Redirect("Redeterminaciones.aspx", false);
                                }
                                else
                                {
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

                    Session.Add("error", ex.ToString());
                    Response.Redirect("Error.aspx");
                }
            }

        }
    }
}