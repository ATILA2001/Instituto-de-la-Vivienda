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
    public partial class AbmlEmpresa : System.Web.UI.Page
    {
        private EmpresaNegocio negocio = new EmpresaNegocio();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarListaEmpresas();
            }
        }

        private void CargarListaEmpresas()
        {
            try
            {
                Session["listaEmpresa"] = negocio.listar();
                dgvEmpresa.DataSource = Session["listaEmpresa"];
                dgvEmpresa.DataBind();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cargar las empresas: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        protected void btnAgregar_Click(object sender, EventArgs e)
        {
            Empresa empresa = new Empresa();
            try
            {
                if (txtNombre.Text.Trim() != string.Empty)
                {
                    empresa.Nombre = txtNombre.Text.Trim();
                    negocio.agregar(empresa);

                    lblMensaje.Text = "¡Empresa agregada exitosamente!";
                    lblMensaje.CssClass = "alert alert-success";
                    txtNombre.Text = string.Empty;

                    // Refrescar listado
                    CargarListaEmpresas();
                }
                else
                {
                    lblMensaje.Text = "Debe llenar todos los campos.";
                    lblMensaje.CssClass = "alert alert-danger";
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al agregar la empresa: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        protected void dgvEmpresa_SelectedIndexChanged(object sender, EventArgs e)
        {
            var idSeleccionado = dgvEmpresa.SelectedDataKey.Value.ToString();
            Response.Redirect("modificarEmpresa.aspx?codM=" + idSeleccionado);
        }

        protected void dgvEmpresa_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            try
            {
                var id = Convert.ToInt32(dgvEmpresa.DataKeys[e.RowIndex].Value);
                if (negocio.eliminar(id))
                {
                    lblMensaje.Text = "Barrio eliminado correctamente.";
                    lblMensaje.CssClass = "alert alert-success";
                    CargarListaEmpresas(); // Actualizar el GridView
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al eliminar el barrio: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }
        protected void dgvEmpresa_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            try
            {
                // Cambiar el índice de la página
                dgvEmpresa.PageIndex = e.NewPageIndex;

                // Refrescar el listado de empresas
                CargarListaEmpresas();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cambiar de página: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

    }
}