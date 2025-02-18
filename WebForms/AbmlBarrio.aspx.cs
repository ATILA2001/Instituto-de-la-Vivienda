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
    public partial class AbmlBarrio : System.Web.UI.Page
    {
        private BarrioNegocio negocio = new BarrioNegocio();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarListaBarrios();
            }
        }
        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            string filtro = txtBuscar.Text.Trim();
            CargarListaBarrios(filtro);
        }
        private void CargarListaBarrios(string filtro = null)
        {
            try
            {
                Session["listaBarrio"] = negocio.listar(filtro);
                dgvBarrio.DataSource = Session["listaBarrio"];
                dgvBarrio.DataBind();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cargar los barrios: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        protected void btnAgregar_Click(object sender, EventArgs e)
        {
            Barrio barrio = new Barrio();
            try
            {
                if (txtNombre.Text.Trim() != string.Empty)
                {
                    barrio.Nombre = txtNombre.Text.Trim();
                    negocio.agregar(barrio);

                    lblMensaje.Text = "Barrio agregado exitosamente!";
                    lblMensaje.CssClass = "alert alert-success";
                    txtNombre.Text = string.Empty;
                    CargarListaBarrios();
                }
                else
                {
                    lblMensaje.Text = "Debe llenar todos los campos.";
                    lblMensaje.CssClass = "alert alert-danger";
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al agregar el barrio: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        protected void dgvBarrio_SelectedIndexChanged(object sender, EventArgs e)
        {
            var idSeleccionado = dgvBarrio.SelectedDataKey.Value.ToString();
            Response.Redirect("ModificarBarrio.aspx?codM=" + idSeleccionado);
        }

        protected void dgvBarrio_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {

            try
            {
                var id = Convert.ToInt32(dgvBarrio.DataKeys[e.RowIndex].Value);
                if (negocio.eliminar(id))
                {
                    lblMensaje.Text = "Barrio eliminado correctamente.";
                    lblMensaje.CssClass = "alert alert-success";
                    CargarListaBarrios();
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al eliminar el barrio: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }
        protected void dgvBarrio_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            try
            {

                dgvBarrio.PageIndex = e.NewPageIndex;

                CargarListaBarrios();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cambiar de página: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }
    }
}