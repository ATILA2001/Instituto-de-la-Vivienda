using Negocio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebForms
{
    public partial class AbmlUsuarios : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            UsuarioNegocio negocio = new UsuarioNegocio();
            Session.Add("listaUsuario", negocio.listarUsuario());
            dgvUsuario.DataSource = Session["listaUsuario"];
            dgvUsuario.DataBind();
        }

        protected void dgvUsuario_SelectedIndexChanged(object sender, EventArgs e)
        {
            var codM = dgvUsuario.SelectedDataKey.Value.ToString();
            Response.Redirect("ModificarUsuario.aspx?codM=" + codM);
        }

        protected void dgvUsuario_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            UsuarioNegocio negocio = new UsuarioNegocio();
            try
            {
                var codP = dgvUsuario.DataKeys[e.RowIndex].Value.ToString();
                if (negocio.eliminar(codP))
                {
                    lblMensaje.Text = "¡Se Eliminó correctamente!";
                    lblMensaje.CssClass = "alert alert-success";
                    ScriptManager.RegisterClientScriptBlock(this, typeof(Page), "redirectJS",
                    "setTimeout(function() { window.location.replace('listarMarca.aspx') }, 3000);", true);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}