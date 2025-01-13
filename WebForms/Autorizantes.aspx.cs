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
    public partial class Autorizantes : System.Web.UI.Page
    {
        private AutorizanteNegocio negocio = new AutorizanteNegocio();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {   ddlEstado.DataSource = ObtenerEstado();  
                ddlEstado.DataTextField = "Nombre";      
                ddlEstado.DataValueField = "Id";         
                ddlEstado.DataBind();


ddlObra.DataSource = ObtenerObras();
                ddlObra.DataTextField = "Nombre";
                ddlObra.DataValueField = "Id";
                ddlObra.DataBind();
                CargarListaAutorizantes();
            }
        }
        protected void btnLimpiar_Click(object sender, EventArgs e)
        {
            txtConcepto.Text = string.Empty;
            txtDetalle.Text = string.Empty;
            txtMontoAutorizado.Text = string.Empty;
            txtExpediente.Text = string.Empty;
            txtFecha.Text = string.Empty;
            ddlObra.SelectedIndex = -1;
            ddlEstado.SelectedIndex = -1;
        }

        private void CargarListaAutorizantes()
        {
            try
            {
                Usuario usuarioLogueado = (Usuario)Session["usuario"];
                Session["listaAutorizante"] = negocio.listar(usuarioLogueado);
                dgvAutorizante.DataSource = Session["listaAutorizante"];
                dgvAutorizante.DataBind();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cargar los Autorizantes: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        protected void dgvAutorizante_SelectedIndexChanged(object sender, EventArgs e)
        {
            var idSeleccionado = dgvAutorizante.SelectedDataKey.Value.ToString();
            Response.Redirect("ModificarAutorizante.aspx?codM=" + idSeleccionado);
        }
        protected void dgvAutorizante_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {

            try
            {
              var id = dgvAutorizante.DataKeys[e.RowIndex].Value.ToString();
                if (negocio.eliminar(id))
                {
                    lblMensaje.Text = "Autorizante eliminado correctamente.";
                   lblMensaje.CssClass = "alert alert-success";
                    CargarListaAutorizantes();
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al eliminar el Autorizante: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        protected void dgvAutorizante_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            try
            {
                dgvAutorizante.PageIndex = e.NewPageIndex;

                CargarListaAutorizantes();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cambiar de página: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }
        protected void btnAgregar_Click(object sender, EventArgs e)
        {
            AutorizanteNegocio autorizanteNegocio = new AutorizanteNegocio();
            Autorizante nuevoAutorizante = new Autorizante();

            nuevoAutorizante.Obra = new Obra();
            nuevoAutorizante.Obra.Id = int.Parse(ddlObra.SelectedValue);
            nuevoAutorizante.Concepto = txtConcepto.Text;
            nuevoAutorizante.Detalle = txtDetalle.Text;
            nuevoAutorizante.Expediente = txtExpediente.Text;
            nuevoAutorizante.Estado = new EstadoAutorizante();
            nuevoAutorizante.Estado.Id = int.Parse(ddlEstado.SelectedValue);
            nuevoAutorizante.MontoAutorizado = decimal.Parse(txtMontoAutorizado.Text);
            nuevoAutorizante.Fecha = DateTime.Parse(txtFecha.Text);

            autorizanteNegocio.agregar(nuevoAutorizante);

            lblMensaje.Text = "Autorizante agregado con éxito.";
            CargarListaAutorizantes(); 
        }
        private DataTable ObtenerEstado()
        {
            EstadoAutorizanteNegocio empresaNegocio = new EstadoAutorizanteNegocio();
            return empresaNegocio.listarddl();
        }

        private DataTable ObtenerContratas()
        {
            ContrataNegocio contrataNegocio = new ContrataNegocio();
            return contrataNegocio.listarddl();
        }

        private DataTable ObtenerObras()
        {
            ObraNegocio barrioNegocio = new ObraNegocio();
            Usuario usuarioLogueado = (Usuario)Session["usuario"];
            return barrioNegocio.listarddl(usuarioLogueado);
        }
    }
}