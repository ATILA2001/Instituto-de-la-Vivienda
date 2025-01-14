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
    public partial class Legitimos : System.Web.UI.Page
    {
        private LegitimoNegocio negocio = new LegitimoNegocio();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                ddlObra.DataSource = ObtenerObras();    
                ddlObra.DataTextField = "Nombre";
                ddlObra.DataValueField = "Id";
                ddlObra.DataBind();
                CargarListaLegitimos();
            }
        }
        protected void btnLimpiar_Click(object sender, EventArgs e)
        {
            txtAutorizante.Text = string.Empty;
            txtExpediente.Text = string.Empty;
            txtInicioEjecucion.Text = string.Empty;
            txtFinEjecucion.Text = string.Empty;
            txtCertificado.Text = string.Empty;
            txtMesAprobacion.Text = string.Empty;
            ddlObra.SelectedIndex = -1;
        }

        private void CargarListaLegitimos()
        {
            try
            {
                Usuario usuarioLogueado = (Usuario)Session["usuario"];
                Session["listaLegitimos"] = negocio.listar(usuarioLogueado);
                dgvLegitimos.DataSource = Session["listaLegitimos"];
                dgvLegitimos.DataBind();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cargar los legítimos: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }



        protected void dgvLegitimos_SelectedIndexChanged(object sender, EventArgs e)
        {
            var idSeleccionado = dgvLegitimos.SelectedDataKey.Value.ToString();
            Response.Redirect($"ModificarLegitimo.aspx?id={idSeleccionado}");
        }

        protected void dgvLegitimos_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            try
            {
                var id = dgvLegitimos.DataKeys[e.RowIndex].Value.ToString();
                if (negocio.eliminar(id))
                {
                    lblMensaje.Text = "Legítimo eliminado correctamente.";
                    lblMensaje.CssClass = "alert alert-success";
                    CargarListaLegitimos();                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al eliminar el legítimo: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        protected void dgvLegitimos_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            try
            {
                dgvLegitimos.PageIndex = e.NewPageIndex;
                CargarListaLegitimos();
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
                Legitimo nuevoLegitimo = new Legitimo
                {
                    CodigoAutorizante = txtAutorizante.Text,
                    Expediente = txtExpediente.Text,
                    InicioEjecucion = DateTime.Parse(txtInicioEjecucion.Text),
                    FinEjecucion = DateTime.Parse(txtFinEjecucion.Text),
                    Certificado = decimal.Parse(txtCertificado.Text),
                    MesAprobacion = DateTime.Parse(txtMesAprobacion.Text)
                };
                nuevoLegitimo.Obra = new Obra
                {
                    Id = int.Parse(ddlObra.SelectedValue)
                };

                if (negocio.agregar(nuevoLegitimo))
                {
                    lblMensaje.Text = "Legítimo agregado con éxito.";
                    lblMensaje.CssClass = "alert alert-success";
                    CargarListaLegitimos();
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al agregar el legítimo: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        private DataTable ObtenerObras()
        {
            ObraNegocio barrioNegocio = new ObraNegocio();
            Usuario usuarioLogueado = (Usuario)Session["usuario"];
            return barrioNegocio.listarddl(usuarioLogueado);
        }

    }

}