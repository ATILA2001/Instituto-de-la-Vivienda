using Negocio;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebForms
{
    public partial class AutorizantesPendientes : System.Web.UI.Page
    {
        private AutorizanteNegocio negocio = new AutorizanteNegocio();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindDropDownList();
                CargarListaAutorizantes();
                CalcularSubtotal();
            }
        }
        protected void ddlEmpresa_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarListaAutorizantes();
            CalcularSubtotal();
        }
        protected void ddlObraFiltro_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarListaAutorizantes();
            CalcularSubtotal();
        }
        protected void ddlEstadoFiltro_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarListaAutorizantes();
            CalcularSubtotal();
        }
        protected void ddlAreaFiltro_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarListaAutorizantes();
            CalcularSubtotal();
        }
        private DataTable ObtenerAreas()
        {
            AreaNegocio areaNegocio = new AreaNegocio();
            return areaNegocio.listarddl();
        }

        private void CargarListaAutorizantes()
        {
            try
            {
                int obraFiltrado = int.Parse(ddlObraFiltro.SelectedValue);
                string estadoFiltrado = ddlEstadoFiltro.SelectedValue == "0" ? null : ddlEstadoFiltro.SelectedItem.Text;
                string empresa = ddlEmpresa.SelectedValue == "0" ? null : ddlEmpresa.SelectedItem.Text;
                string area = ddlAreaFiltro.SelectedValue == "0" ? null : ddlAreaFiltro.SelectedItem.Text;

                Session["listaAutorizanteAdmin"] = negocio.listarPendientes(estadoFiltrado, empresa, obraFiltrado, area);
                dgvAutorizante.DataSource = Session["listaAutorizanteAdmin"];
                dgvAutorizante.DataBind();
                CalcularSubtotal();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cargar los Autorizantes: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }
        private void CalcularSubtotal()
        {
            decimal subtotal = 0;

            foreach (GridViewRow row in dgvAutorizante.Rows)
            {
                var cellValue = row.Cells[9].Text;
                if (decimal.TryParse(cellValue, System.Globalization.NumberStyles.Currency, null, out decimal monto))
                {
                    subtotal += monto;
                }
            }

            txtSubtotal.Text = subtotal.ToString("C");
        }

        protected void dgvAutorizante_SelectedIndexChanged(object sender, EventArgs e)
        {
            var idSeleccionado = dgvAutorizante.SelectedDataKey.Value.ToString();
            Response.Redirect("ModificarAutorizanteAdmin.aspx?codM=" + idSeleccionado);
        }
        protected void dgvAutorizante_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {

            try
            {
                var id = dgvAutorizante.DataKeys[e.RowIndex].Value.ToString();
                if (negocio.eliminar(id))
                {
                    lblMensaje.Text = "Barrio eliminado correctamente.";
                    lblMensaje.CssClass = "alert alert-success";
                    CargarListaAutorizantes();
                    CalcularSubtotal();
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al eliminar el barrio: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        protected void dgvAutorizante_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            try
            {
                dgvAutorizante.PageIndex = e.NewPageIndex;

                CargarListaAutorizantes();
                CalcularSubtotal();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cambiar de página: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }
        private void BindDropDownList()
        {
            var tiposFiltro = ObtenerEstado();
            tiposFiltro.Rows.InsertAt(CrearFilaTodos(tiposFiltro), 0);
            ddlEstadoFiltro.DataSource = tiposFiltro;
            ddlEstadoFiltro.DataTextField = "Nombre";
            ddlEstadoFiltro.DataValueField = "Id";
            ddlEstadoFiltro.DataBind();

            var empresa = ObtenerEmpresas();
            empresa.Rows.InsertAt(CrearFilaTodos(empresa), 0);
            ddlEmpresa.DataSource = empresa;
            ddlEmpresa.DataTextField = "Nombre";
            ddlEmpresa.DataValueField = "Id";
            ddlEmpresa.DataBind();

            var area = ObtenerAreas();
            area.Rows.InsertAt(CrearFilaTodos(area), 0);
            ddlAreaFiltro.DataSource = area;
            ddlAreaFiltro.DataTextField = "Nombre";
            ddlAreaFiltro.DataValueField = "Id";
            ddlAreaFiltro.DataBind();

            var obrasFiltro = ObtenerObras();
            obrasFiltro.Rows.InsertAt(CrearFilaTodos(obrasFiltro), 0);
            ddlObraFiltro.DataSource = obrasFiltro;
            ddlObraFiltro.DataTextField = "Nombre";
            ddlObraFiltro.DataValueField = "Id";
            ddlObraFiltro.DataBind();
        }
        private DataTable ObtenerEmpresas()
        {
            EmpresaNegocio empresaNegocio = new EmpresaNegocio();
            return empresaNegocio.listarddl();
        }
        private DataRow CrearFilaTodos(DataTable table)
        {
            DataRow row = table.NewRow();
            row["Id"] = 0;
            row["Nombre"] = "Todos";
            return row;
        }

        private DataTable ObtenerEstado()
        {
            EstadoAutorizanteNegocio empresaNegocio = new EstadoAutorizanteNegocio();
            return empresaNegocio.listarddl();
        }

        private DataTable ObtenerObras()
        {
            ObraNegocio barrioNegocio = new ObraNegocio();
            return barrioNegocio.listarddl();
        }


    }
}