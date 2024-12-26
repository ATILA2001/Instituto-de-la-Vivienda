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
            {// Llenar el DropDownList para Empresas
                ddlEstado.DataSource = ObtenerEstado();  // Método para obtener los datos de las Empresas
                ddlEstado.DataTextField = "Nombre";         // Columna que se muestra
                ddlEstado.DataValueField = "Id";            // Columna que se almacena
                ddlEstado.DataBind();



                // Llenar el DropDownList para Barrios
                ddlObra.DataSource = ObtenerObras();    // Método para obtener los datos de los Barrios
                ddlObra.DataTextField = "Nombre";
                ddlObra.DataValueField = "Id";
                ddlObra.DataBind();
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
            //var idSeleccionado = dgvAutorizante.SelectedDataKey.Value.ToString();
            //Response.Redirect("modificarBarrio.aspx?codM=" + idSeleccionado);
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
        protected void btnAgregar_Click(object sender, EventArgs e)
        {
            // Instancia de negocio y autorización.
            AutorizanteNegocio autorizanteNegocio = new AutorizanteNegocio();
            Autorizante nuevoAutorizante = new Autorizante();

            // Asignación de valores desde los controles del formulario.
            nuevoAutorizante.Obra = new Obra();
            nuevoAutorizante.Obra.Id = int.Parse(ddlObra.SelectedValue);
            nuevoAutorizante.Concepto = txtConcepto.Text;
            nuevoAutorizante.Detalle = txtDetalle.Text;
            nuevoAutorizante.Expediente = txtExpediente.Text;
            nuevoAutorizante.Estado = new EstadoAutorizante();
            nuevoAutorizante.Estado.Id = int.Parse(ddlEstado.SelectedValue);
            nuevoAutorizante.MontoAutorizado = decimal.Parse(txtMontoAutorizado.Text);
            nuevoAutorizante.Fecha = DateTime.Parse(txtFecha.Text);

            // Llamar al método que agrega el registro.
            autorizanteNegocio.agregar(nuevoAutorizante);

            // Mostrar mensaje de éxito.
            lblMensaje.Text = "Autorizante agregado con éxito.";
            CargarListaAutorizantes();  // Re-cargar la lista para mostrar los cambios.
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