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
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        protected void btnIniciar_Click(object sender, EventArgs e)
        {
            Usuario usuario;
            UsuarioNegocio negocio = new UsuarioNegocio();
            try
            {
                usuario = new Usuario(txtEmail.Text.Trim(), txtPass.Text.Trim());
                if (negocio.Logear(usuario))
                {
                    Session.Add("Usuario", usuario);
                    if (Session["Usuario"] != null && ((Dominio.Usuario)Session["Usuario"]).Tipo)
                    {
                        Response.Redirect("HomeAdmin.aspx", false);
                    }
                    else
                    {
                        Response.Redirect("Default.aspx", false);
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
        protected void btnVolver_Click(object sender, EventArgs e)
        {
            Response.Redirect("Default.aspx");
        }
    }
}