using Dominio;
using Negocio;
using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace WebForms
{
    public partial class PanelConfiguracion : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!UserHelper.IsUserAdmin())
            {
                Response.Redirect("Startup.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (!IsPostBack)
            {
                CargarEstadoGlobal();
                CargarGrilla();
            }
        }

        private void CargarEstadoGlobal()
        {
            chkPlanificacion.Checked = ABMPlaniNegocio.GetIsPlanningOpen();
            chkFormulacion.Checked = ABMPlaniNegocio.GetIsFormulationOpen();
            ActualizarVisibilidadSecciones();
        }

        private void CargarGrilla()
        {
            var lista = new UsuariosVinculadosNegocio().Listar();
            if (lista.Count == 0)
            {
                dgvUsuariosVinculados.Visible = false;
                panelSinUsuarios.Visible = true;
            }
            else
            {
                dgvUsuariosVinculados.Visible = true;
                panelSinUsuarios.Visible = false;
                dgvUsuariosVinculados.DataSource = lista;
                dgvUsuariosVinculados.DataBind();
            }
        }

        private void ActualizarVisibilidadSecciones()
        {
            bool planificacionAbierta = ABMPlaniNegocio.GetIsPlanningOpen();
            panelOverrides.Visible = !planificacionAbierta;
            panelPlanificacionAbierta.Visible = planificacionAbierta;
        }

        protected void chkPlanificacion_ServerChange(object sender, EventArgs e)
        {
            ABMPlaniNegocio.SetIsPlanningOpen(chkPlanificacion.Checked);
            ActualizarVisibilidadSecciones();
            if (!chkPlanificacion.Checked)
                CargarGrilla();
        }

        protected void chkFormulacion_ServerChange(object sender, EventArgs e)
        {
            ABMPlaniNegocio.SetIsFormulationOpen(chkFormulacion.Checked);
        }

        protected void dgvUsuariosVinculados_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                var chk = e.Row.FindControl("chkOverride") as HtmlInputCheckBox;
                var usuario = e.Row.DataItem as UsuarioVinculadoEF;
                if (chk != null && usuario != null)
                    chk.Checked = usuario.IsPlanningOpenOverride;
            }
        }

        protected void chkOverride_ServerChange(object sender, EventArgs e)
        {
            var chk = (HtmlInputCheckBox)sender;
            var row = (GridViewRow)chk.NamingContainer;
            var authUserId = dgvUsuariosVinculados.DataKeys[row.RowIndex].Value.ToString();
            new UsuariosVinculadosNegocio().SetPlanningOverride(authUserId, chk.Checked);
            CargarGrilla();
        }
    }
}
