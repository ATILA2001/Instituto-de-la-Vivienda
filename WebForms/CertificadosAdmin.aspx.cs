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
                CargarListaCertificados();
            }
        }

        private void CargarListaCertificados()
        {
            try
            {
                Usuario usuarioLogueado = (Usuario)Session["usuario"];
                Session["listaCertificado"] = negocio.listar();
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
            CargarListaCertificados();  // Re-cargar la lista para mostrar los cambios.
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