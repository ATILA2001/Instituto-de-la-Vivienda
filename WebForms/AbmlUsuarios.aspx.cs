using Dominio;
using Negocio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

namespace WebForms
{
    public partial class AbmlUsuarios : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarGrilla();
            }
        }

        private void CargarGrilla()
        {
            var lista = new UsuarioNegocio().listarUsuario();
            Session["listaUsuario"] = lista;
            dgvUsuario.DataSource = lista;
            dgvUsuario.DataBind();
        }

        protected void dgvUsuario_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                var chk = e.Row.FindControl("chkPlani") as HtmlInputCheckBox;
                var usuario = e.Row.DataItem as Usuario;
                if (chk != null && usuario != null)
                    chk.Checked = usuario.IsPlanningOpenOverride;
            }
        }

        protected void chkPlani_CheckedChanged(object sender, EventArgs e)
        {
            var chk = (HtmlInputCheckBox)sender;
            var row = (GridViewRow)chk.NamingContainer;
            int userId = Convert.ToInt32(dgvUsuario.DataKeys[row.RowIndex].Value);
            new UsuarioNegocio().SetPlanningOpenOverride(userId, chk.Checked);
            CargarGrilla();
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
                    "setTimeout(function() { window.location.replace('AbmlUsuarios.aspx') }, 3000);", true);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}