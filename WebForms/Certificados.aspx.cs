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
   

                ddlTipo.DataSource = ObtenerTipos(); 
                ddlTipo.DataTextField = "Nombre";
                ddlTipo.DataValueField = "Id";
                ddlTipo.DataBind();

                ddlAutorizante.DataSource = ObtenerAutorizantes();
                ddlAutorizante.DataTextField = "Nombre";
                ddlAutorizante.DataValueField = "Id";
                ddlAutorizante.DataBind();

                CargarListaCertificados();
            }
        }

        private void CargarListaCertificados()
        {
            try
            {
                Usuario usuarioLogueado = (Usuario)Session["usuario"];
                Session["listaCertificado"] = negocio.listar(usuarioLogueado);
                dgvCertificado.DataSource = Session["listaCertificado"];
                dgvCertificado.DataBind();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cargar los Autorizantes: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        protected void dgvCertificado_SelectedIndexChanged(object sender, EventArgs e)
        {
            //var idSeleccionado = dgvAutorizante.SelectedDataKey.Value.ToString();
            //Response.Redirect("modificarBarrio.aspx?codM=" + idSeleccionado);
        }
        protected void dgvCertificado_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {

            //try
            //{
            //    var id = Convert.ToInt32(dgvBarrio.DataKeys[e.RowIndex].Value);
            //    if (negocio.eliminar(id))
            //    {
            //        lblMensaje.Text = "Barrio eliminado correctamente.";
            //        lblMensaje.CssClass = "alert alert-success";
            //        CargarListaBarrios(); // Actualizar el GridView
            //    }
            //}
            //catch (Exception ex)
            //{
            //    lblMensaje.Text = $"Error al eliminar el barrio: {ex.Message}";
            //    lblMensaje.CssClass = "alert alert-danger";
            //}
        }

        protected void dgvCertificado_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            try
            {
                // Cambiar el índice de la página
                dgvCertificado.PageIndex = e.NewPageIndex;

                // Refrescar el listado de empresas
                CargarListaCertificados();
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
        private DataTable ObtenerAutorizantes()
        {
            AutorizanteNegocio autorizanteNegocio = new AutorizanteNegocio();

            Usuario usuarioLogueado = (Usuario)Session["usuario"];
            return autorizanteNegocio.listarddl(usuarioLogueado);
        }
    }
}