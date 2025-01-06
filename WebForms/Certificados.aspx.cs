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
    public partial class Certificados : System.Web.UI.Page
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

        private void CalcularSubtotal()
        {
            decimal subtotal = 0;

            foreach (GridViewRow row in dgvCertificado.Rows)
            {
                // Encuentra la columna que contiene los montos
                var cellValue = row.Cells[5].Text; // Suponiendo que la columna de 'Monto Autorizado' está en la posición 5
                if (decimal.TryParse(cellValue, System.Globalization.NumberStyles.Currency, null, out decimal monto))
                {
                    subtotal += monto;
                }
            }

            txtSubtotal.Text = subtotal.ToString("C");
        }
        protected void btnLimpiar_Click(object sender, EventArgs e)
        {
            // Limpiar todos los TextBox
            txtMontoAutorizado.Text = string.Empty;
            txtExpediente.Text = string.Empty;
            txtFecha.Text = string.Empty;
            ddlAutorizante.SelectedIndex = -1;
            ddlTipo.SelectedIndex = -1;
        }

        private void CargarListaCertificados()
        {
            try
            {
                Usuario usuarioLogueado = (Usuario)Session["usuario"]; 
                string autorizanteFiltrado = ddlAutorizanteFiltro.SelectedValue == "0" ? null : ddlAutorizanteFiltro.SelectedItem.Text;
                string tipoFiltrado = ddlTipoFiltro.SelectedValue == "0" ? null : ddlTipoFiltro.SelectedItem.Text;
                DateTime? mesAprobacion = null;
                if (!string.IsNullOrWhiteSpace(txtMesAprobacionFiltro.Text))
                {
                    mesAprobacion = DateTime.Parse(txtMesAprobacionFiltro.Text);
                }
                Session["listaCertificado"] = negocio.listarFiltro(usuarioLogueado, autorizanteFiltrado, tipoFiltrado, mesAprobacion);
                dgvCertificado.DataSource = Session["listaCertificado"];
                dgvCertificado.DataBind();
                CalcularSubtotal();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cargar los Autorizantes: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        protected void dgvCertificado_SelectedIndexChanged(object sender, EventArgs e)
        {
            var idSeleccionado = dgvCertificado.SelectedDataKey.Value.ToString();
            Response.Redirect("ModificarCertificado.aspx?codM=" + idSeleccionado);
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
                    CargarListaCertificados(); // Actualizar el GridView
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
                // Cambiar el índice de la página
                dgvCertificado.PageIndex = e.NewPageIndex;

                // Refrescar el listado de empresas
                CargarListaCertificados();
                CalcularSubtotal();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cambiar de página: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }
        protected void btnAgregar_Click(object sender, EventArgs e)
        {
            try
            {
                // Instancia de negocio y certificado.
                CertificadoNegocio certificadoNegocio = new CertificadoNegocio();
                Certificado nuevoCertificado = new Certificado
                {
                    Autorizante = new Autorizante
                    {
                        CodigoAutorizante = ddlAutorizante.SelectedItem.Text
                    },
                    ExpedientePago = string.IsNullOrWhiteSpace(txtExpediente.Text) ? null : txtExpediente.Text,
                    Tipo = new TipoPago
                    {
                        Id = int.Parse(ddlTipo.SelectedValue)
                    },
                    MontoTotal = decimal.Parse(txtMontoAutorizado.Text),
                    MesAprobacion = string.IsNullOrWhiteSpace(txtFecha.Text)
                        ? null
                        : (DateTime?)DateTime.Parse(txtFecha.Text)
                };

                // Llamar al método que agrega el registro.
                certificadoNegocio.agregar(nuevoCertificado);

                // Mostrar mensaje de éxito.
                lblMensaje.Text = "Certificado agregado con éxito.";
                lblMensaje.ForeColor = System.Drawing.Color.Green;

                // Re-cargar la lista para mostrar los cambios.
                CargarListaCertificados();
                CalcularSubtotal();
            }
            catch (Exception ex)
            {
                // Mostrar mensaje de error.
                lblMensaje.Text = $"Error al agregar el certificado: {ex.Message}";
                lblMensaje.ForeColor = System.Drawing.Color.Red;
            }
        }

        private DataTable ObtenerContratas()
        {
            ContrataNegocio contrataNegocio = new ContrataNegocio();
            return contrataNegocio.listarddl();
        }

    
        private DataTable ObtenerTipos()
        {
            TipoPagoNegocio tipoPagNegocio = new TipoPagoNegocio();
            return tipoPagNegocio.listarddl();
        }
        private DataTable ObtenerTiposFiltro()
        {
            TipoPagoNegocio tipoPagNegocio = new TipoPagoNegocio();
            return tipoPagNegocio.listarddl();
        }
        private DataTable ObtenerAutorizantesFiltro()
        {
            AutorizanteNegocio autorizanteNegocio = new AutorizanteNegocio();

            Usuario usuarioLogueado = (Usuario)Session["usuario"];
            return autorizanteNegocio.listarddl(usuarioLogueado);
        }
        private DataTable ObtenerAutorizantes()
        {
            AutorizanteNegocio autorizanteNegocio = new AutorizanteNegocio();

            Usuario usuarioLogueado = (Usuario)Session["usuario"];
            return autorizanteNegocio.listarddl(usuarioLogueado);
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

        private void BindDropDownList()
        {
            // Obtener los tipos.
            var tipos = ObtenerTipos();
            ddlTipo.DataSource = tipos;
            ddlTipo.DataTextField = "Nombre";
            ddlTipo.DataValueField = "Id";
            ddlTipo.DataBind();

            var tiposFiltro = ObtenerTiposFiltro();
            tiposFiltro.Rows.InsertAt(CrearFilaTodos(tiposFiltro), 0);
            ddlTipoFiltro.DataSource = tiposFiltro;
            ddlTipoFiltro.DataTextField = "Nombre";
            ddlTipoFiltro.DataValueField = "Id";
            ddlTipoFiltro.DataBind();

            // Obtener los autorizantes.
            var autorizantes = ObtenerAutorizantes();
            ddlAutorizante.DataSource = autorizantes;
            ddlAutorizante.DataTextField = "Nombre";
            ddlAutorizante.DataValueField = "Id";
            ddlAutorizante.DataBind();

            // Obtener los autorizantes para filtro.
            var autorizantesFiltro = ObtenerAutorizantesFiltro();
            autorizantesFiltro.Rows.InsertAt(CrearFilaTodos(autorizantesFiltro), 0);
            ddlAutorizanteFiltro.DataSource = autorizantesFiltro;
            ddlAutorizanteFiltro.DataTextField = "Nombre";
            ddlAutorizanteFiltro.DataValueField = "Id";
            ddlAutorizanteFiltro.DataBind();
        }

        // Crear una fila con valor "Todos".
        private DataRow CrearFilaTodos(DataTable table)
        {
            DataRow row = table.NewRow();
            row["Id"] = 0;             // Valor del ítem.
            row["Nombre"] = "Todos";   // Texto mostrado.
            return row;
        }

        protected void btnFiltrarMes_Click(object sender, EventArgs e)
        {
            CargarListaCertificados();
            CalcularSubtotal();
        }
    }

}