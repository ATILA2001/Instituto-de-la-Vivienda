using Dominio;
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
    public partial class CertificadosAdmin : System.Web.UI.Page
    {
        CertificadoNegocio negocio = new CertificadoNegocio();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindDropDownList();
                CargarListaCertificados();
            }
        }

        private void CargarListaCertificados()
        {
            try
            {
                string autorizanteFiltrado = ddlAutorizanteFiltro.SelectedValue == "0" ? null : ddlAutorizanteFiltro.SelectedItem.Text;
                string tipoFiltrado = ddlTipoFiltro.SelectedValue == "0" ? null : ddlTipoFiltro.SelectedItem.Text;
                DateTime? mesAprobacion = null;
                string empresa = ddlEmpresa.SelectedValue == "0" ? null : ddlEmpresa.SelectedItem.Text;
                if (!string.IsNullOrWhiteSpace(txtMesAprobacionFiltro.Text))
                {
                    mesAprobacion = DateTime.Parse(txtMesAprobacionFiltro.Text);
                }
                Session["listaCertificado"] = negocio.listarFiltroAdmin(autorizanteFiltrado, tipoFiltrado, mesAprobacion, empresa);
                dgvCertificado.DataSource = Session["listaCertificado"];
                dgvCertificado.DataBind();
                CalcularSubtotal();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cargar los Certificados: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        protected void dgvCertificado_SelectedIndexChanged(object sender, EventArgs e)
        {
            var idSeleccionado = dgvCertificado.SelectedDataKey.Value.ToString();
            Response.Redirect("ModificarCertificadoAdmin.aspx?codM=" + idSeleccionado);
        }
        protected void dgvCertificado_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {

            try
            {
                var id = Convert.ToInt32(dgvCertificado.DataKeys[e.RowIndex].Value);
                if (negocio.eliminar(id))
                {
                    lblMensaje.Text = "Certificado eliminado correctamente.";
                    lblMensaje.CssClass = "alert alert-success";
                    CargarListaCertificados(); 
                    CalcularSubtotal();
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al eliminar el certificado: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        protected void dgvCertificado_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            try
            {
                dgvCertificado.PageIndex = e.NewPageIndex;

                CargarListaCertificados();
                CalcularSubtotal();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cambiar de página: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }
        private DataTable ObtenerTiposFiltro()
        {
            TipoPagoNegocio tipoPagNegocio = new TipoPagoNegocio();
            return tipoPagNegocio.listarddl();
        }

        private DataTable ObtenerAutorizantesFiltro()
        {
            AutorizanteNegocio autorizanteNegocio = new AutorizanteNegocio();
            return autorizanteNegocio.listarddl();
        }
 
        protected void ddlAutorizanteFiltro_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarListaCertificados();
            CalcularSubtotal();
        }
        protected void ddlTipoFiltro_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarListaCertificados();
            CalcularSubtotal();
        }
        protected void ddlEmpresa_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarListaCertificados();
            CalcularSubtotal();
        }
        private void BindDropDownList()
        {

            var tiposFiltro = ObtenerTiposFiltro();
            tiposFiltro.Rows.InsertAt(CrearFilaTodos(tiposFiltro), 0);
            ddlTipoFiltro.DataSource = tiposFiltro;
            ddlTipoFiltro.DataTextField = "Nombre";
            ddlTipoFiltro.DataValueField = "Id";
            ddlTipoFiltro.DataBind();

            var empresa = ObtenerEmpresas();
            empresa.Rows.InsertAt(CrearFilaTodos(empresa), 0);
            ddlEmpresa.DataSource = empresa; 
            ddlEmpresa.DataTextField = "Nombre";        
            ddlEmpresa.DataValueField = "Id";        
            ddlEmpresa.DataBind();

            var autorizantesFiltro = ObtenerAutorizantesFiltro();
            autorizantesFiltro.Rows.InsertAt(CrearFilaTodos(autorizantesFiltro), 0);
            ddlAutorizanteFiltro.DataSource = autorizantesFiltro;
            ddlAutorizanteFiltro.DataTextField = "Nombre";
            ddlAutorizanteFiltro.DataValueField = "Id";
            ddlAutorizanteFiltro.DataBind();
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

        protected void btnFiltrarMes_Click(object sender, EventArgs e)
        {
            CargarListaCertificados();
            CalcularSubtotal();
        }
        private void CalcularSubtotal()
        {
            decimal subtotal = 0;

            foreach (GridViewRow row in dgvCertificado.Rows)
            {
                var cellValue = row.Cells[6].Text;
                if (decimal.TryParse(cellValue, System.Globalization.NumberStyles.Currency, null, out decimal monto))
                {
                    subtotal += monto;
                }
            }

            txtSubtotal.Text = subtotal.ToString("C");
        }

    }
}