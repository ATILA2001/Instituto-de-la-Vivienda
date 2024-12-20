using Dominio;
using Negocio;
using System;
using System.Collections.Generic;
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
            {
                                CargarListaAutorizantes();
            }
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
            Response.Redirect("modificarBarrio.aspx?codM=" + idSeleccionado);
        }
        protected void dgvAutorizante_RowDeleting(object sender, GridViewDeleteEventArgs e)
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

        protected void dgvAutorizante_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            try
            {
                // Cambiar el índice de la página
                dgvAutorizante.PageIndex = e.NewPageIndex;

                // Refrescar el listado de empresas
                CargarListaAutorizantes();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cambiar de página: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }
    }
}