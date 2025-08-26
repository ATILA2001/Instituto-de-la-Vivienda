using Dominio;
using Negocio;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

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

            Debug.WriteLine("OBJ USUARIO CREAADO USUARIO CREADO");

            try
            {
                // Validar si las credenciales son correctas

                using (PrincipalContext pc = new PrincipalContext(ContextType.Domain, "BUENOSAIRES"))
                {

                    //bool isValid = pc.ValidateCredentials(txtEmail.Text.Trim(), txtPass.Text.Trim());
                    if (true)
                    {
                        var emailPattern = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
                        var cuilPattern = new Regex(@"^(20|23|27|30|33)\d{8}\d$");
                        bool exito = false;

                        if (emailPattern.IsMatch(txtEmail.Text.Trim()))
                        {
                            Debug.WriteLine("------------ESTA ENTRANDO POR CORREO @@@@@@@@@@@@@@@@@@@@@@@@@@@ ");
                            usuario = new Usuario(txtEmail.Text.Trim(), txtPass.Text.Trim());
                            exito = negocio.Logear(usuario);
                        }
                        else if (cuilPattern.IsMatch(txtEmail.Text.Trim()))
                        {
                            // CreateWithDomain
                            Debug.WriteLine("------------ESTA ENTRANDO POR CUIL AD. AD. AD. AD. AD. AD. AD. ");
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
                        else
                        {
                            Debug.WriteLine("Correo: " + usuario.Correo + "Area: " + usuario.Area);
                            Session.Add("error", "Usuario o Contraseña Incorrectos");
                            Response.Redirect("Error.aspx", false);
                        }

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