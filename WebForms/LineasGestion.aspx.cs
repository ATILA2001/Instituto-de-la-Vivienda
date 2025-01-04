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
    public partial class LineasGestion : System.Web.UI.Page
    {
        LineaGestionNegocio negocio = new LineaGestionNegocio();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarListaLineaGestion();
            }
        }

        private void CargarListaLineaGestion()
        {
            try
            {
                Session["listaLineas"] = negocio.listar();
                dgvLineaGestion.DataSource = Session["listaLineas"];
                dgvLineaGestion.DataBind();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cargar las Lineas de gestion: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }
        protected void btnAgregar_Click(object sender, EventArgs e)
        {
            try
            {
                string nombre = txtNombre.Text;
                string tipo = txtTipo.Text;
                string grupo = txtGrupo.Text;
                string reparticion = txtReparticion.Text;
                LineaGestion linea = new LineaGestion();
                linea.Nombre = nombre;
                linea.Tipo = tipo;
                linea.Grupo = grupo;
                linea.Reparticion = reparticion;
                // Llamar a la lógica para agregar la nueva línea de gestión
               negocio.agregar(linea);

                // Mostrar mensaje de éxito
                lblMensaje.Text = "¡Línea de Gestión agregada exitosamente!";
                lblMensaje.ForeColor = System.Drawing.Color.Green;

                 
                List<LineaGestion> listaLineaGestion = negocio.listar();
                dgvLineaGestion.DataSource = listaLineaGestion;
                dgvLineaGestion.DataBind();
            }
            catch (Exception ex)
            {
                // Mostrar error si ocurre un problema
                lblMensaje.Text = "Error al agregar la Línea de Gestión: " + ex.Message;
                lblMensaje.ForeColor = System.Drawing.Color.Red;
            }
        }
        protected void dgvLineaGestion_SelectedIndexChanged(object sender, EventArgs e)
        {
            var idSeleccionado = dgvLineaGestion.SelectedDataKey.Value.ToString();
            Response.Redirect("modificarEmpresa.aspx?codM=" + idSeleccionado);
        }

        protected void dgvLineaGestion_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            //try
            //{
            //    var id = Convert.ToInt32(dgvLineaGestion.DataKeys[e.RowIndex].Value);
            //    if (negocio.eliminar(id))
            //    {
            //        lblMensaje.Text = "Empresa eliminada correctamente.";
            //        lblMensaje.CssClass = "alert alert-success";
            //        CargarListaLineaGestion(); // Actualizar el GridView
            //    }
            //}
            //catch (Exception ex)
            //{
            //    lblMensaje.Text = $"Error al eliminar la empresa: {ex.Message}";
            //    lblMensaje.CssClass = "alert alert-danger";
            //}
        }
        protected void dgvLineaGestion_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            try
            {
                // Cambiar el índice de la página
                dgvLineaGestion.PageIndex = e.NewPageIndex;

                // Refrescar el listado de empresas
                CargarListaLineaGestion();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cambiar de página: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }
    }
}