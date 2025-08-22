using Dominio;
using Negocio;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text.RegularExpressions;

namespace WebForms
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnIniciar_Click(object sender, EventArgs e)
        {
            Usuario usuario = null;
            UsuarioNegocio negocio = new UsuarioNegocio();
            try
            {
                // Validar si las credenciales son correctas

                using (PrincipalContext pc = new PrincipalContext(ContextType.Domain, "BUENOSAIRES"))
                {

                    bool isValid = pc.ValidateCredentials(txtEmail.Text.Trim(), txtPass.Text.Trim());
                    if (isValid)
                    {
                        var emailPattern = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
                        var cuilPattern = new Regex(@"^(20|23|27|30|33)\d{8}\d$");
                        bool exito = false;

                        if (emailPattern.IsMatch(txtEmail.Text.Trim()))
                        {
                            usuario = new Usuario(txtEmail.Text.Trim(), txtPass.Text.Trim());
                            exito = negocio.Logear(usuario);
                        }
                        else if (cuilPattern.IsMatch(txtEmail.Text.Trim()))
                        {
                            // CreateWithDomain
                            usuario = Usuario.CreateWithDomain(txtEmail.Text.Trim(), txtPass.Text.Trim());
                            exito = negocio.LogearIntegSecur(usuario, txtEmail.Text.Trim());
                        }

                        if (exito) {
                            Session.Add("Usuario", usuario);
                            if (Session["Usuario"] != null && ((Dominio.Usuario)Session["Usuario"]).Tipo == true)
                            {
                                Response.Redirect("AutorizantesEF.aspx", false);
                            }
                            else
                            {
                                if (((Dominio.Usuario)Session["Usuario"]).Estado == true)
                                {
                                    if (((Dominio.Usuario)Session["Usuario"]).Area != null &&
                                        ((Dominio.Usuario)Session["Usuario"]).Area.Id == 16)
                                    {
                                        Response.Redirect("Redeterminaciones.aspx", false);
                                    }
                                    else
                                    {
                                        Response.Redirect("AutorizantesEF.aspx", false);
                                    }
                                }
                                else
                                {
                                    Session.Add("error",
                                        "Usuario no habilitado a ingresar, solicitar acceso al area correspondiente.");
                                    Response.Redirect("Error.aspx", false);
                                }
                            }
                        }
                    }
                    else
                    {
                        Session.Add("error", "Usuario o Contraseña Incorrectos");
                        Response.Redirect("Error.aspx", false);
                    }
                }
            }


            catch (Exception ex) {

                        Session.Add("error", ex.ToString());
                        Response.Redirect("Error.aspx");
            }
            
        }
    }
}