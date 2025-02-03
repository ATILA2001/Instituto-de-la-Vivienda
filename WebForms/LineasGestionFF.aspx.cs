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
    public partial class LineasGestionFF : System.Web.UI.Page
    {
        LineaGestionFFNegocio negocio = new LineaGestionFFNegocio();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                cargarLineasGestion();
            }
        }

        private void cargarLineasGestion()
        {
            AccesoDatos datos = new AccesoDatos();
            try
            {
                Session["listaLineasFF"] = negocio.listar();
                dgvLineaGestionFF.DataSource = Session["listaLineasFF"];
                dgvLineaGestionFF.DataBind();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cargar las Lineas de gestion: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }

            try
            {
                datos.setearConsulta("SELECT ID, NOMBRE FROM LINEA_DE_GESTION ORDER BY NOMBRE");
                datos.ejecutarLectura();

                ddlLineaGestion.DataSource = datos.Lector;
                ddlLineaGestion.DataValueField = "ID";
                ddlLineaGestion.DataTextField = "NOMBRE";
                ddlLineaGestion.DataBind();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }
        protected void btnAgregarFF_Click(object sender, EventArgs e)
        {
            try
            {
                LineaGestionFF nuevaFF = new LineaGestionFF
                {
                    LineaGestion = new LineaGestion { Id = int.Parse(ddlLineaGestion.SelectedValue) },
                    Nombre = txtNombreFF.Text.Trim(),
                    Fuente = txtFuente.Text.Trim()
                };

                LineaGestionFFNegocio negocio = new LineaGestionFFNegocio();
                negocio.agregar(nuevaFF);
                // Mostrar mensaje de éxito
                lblMensaje.Text = "¡Línea de Gestión agregada exitosamente!";
                lblMensaje.ForeColor = System.Drawing.Color.Green;

                Session["listaLineasFF"] = negocio.listar();
                dgvLineaGestionFF.DataSource = Session["listaLineasFF"];
            }
            catch (Exception ex)
            {  // Mostrar error si ocurre un problema
                lblMensaje.Text = "Error al agregar la Línea de Gestión: " + ex.Message;
                lblMensaje.ForeColor = System.Drawing.Color.Red;
            }
        }
        protected void dgvLineaGestionFF_SelectedIndexChanged(object sender, EventArgs e)
        {
        //    var idSeleccionado = dgvLineaGestion.SelectedDataKey.Value.ToString();
        //    Response.Redirect("modificarEmpresa.aspx?codM=" + idSeleccionado);
        }

        protected void dgvLineaGestionFF_RowDeleting(object sender, GridViewDeleteEventArgs e)
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
        protected void dgvLineaGestionFF_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            try
            {
                // Cambiar el índice de la página
                dgvLineaGestionFF.PageIndex = e.NewPageIndex;

                // Refrescar el listado de empresas
                cargarLineasGestion();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cambiar de página: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

    }
}