using Dominio;
using Negocio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebForms
{
    public partial class ModificarUsuario : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            int codM;
            UsuarioNegocio negocio = new UsuarioNegocio();
            if (!IsPostBack)
            {
                if (Request.QueryString["codM"] != null)
                {
                    ValidationSettings.UnobtrusiveValidationMode = UnobtrusiveValidationMode.None;
                    codM = Convert.ToInt32(Request.QueryString["codM"]);
                    List <Usuario> temp = (List<Usuario>)Session["listaUsuario"];
                    Usuario selected = temp.Find(x => x.Id == codM);
                    txtNombre.Text = selected.Nombre;
                    txtEmail.Text = selected.Correo;
                    txtTipo.Text = selected.Tipo ? "Admin" : "User";
                    ddlEstado.SelectedValue = selected.Estado ? "1" : "0";
                }
            }

        }
        protected void btnModificar_Click(object sender, EventArgs e)
        {
            Usuario usuario = new Usuario();
            UsuarioNegocio negocio = new UsuarioNegocio();

            if (txtNombre.Text.Trim() != string.Empty && txtEmail.Text.Trim() != string.Empty && txtTipo.Text.Trim() != string.Empty )
            {
                usuario.Id = int.Parse(Request.QueryString["codM"].ToString());
                usuario.Nombre = txtNombre.Text.Trim();
                usuario.Correo = txtEmail.Text.Trim(); 
                usuario.Estado = ddlEstado.SelectedValue == "1";
                negocio.ModificarUsuario(usuario);
                lblMensaje.Text = "Se modificó el Usuario exitosamente.";
                lblMensaje.CssClass = "alert alert-success";

                ScriptManager.RegisterClientScriptBlock(this, typeof(Page), "redirectJS",
                "setTimeout(function() { window.location.replace('AbmlUsuarios.aspx') }, 3000);", true);
            }
            else
            {
                lblMensaje.Text = "Tiene que llenar todos los campos.";
                lblMensaje.CssClass = "alert alert-success";
            }
        }
        protected void btnVolver_Click(object sender, EventArgs e)
        {
            Response.Redirect("AbmlUsuarios.aspx");
        }

    }
}