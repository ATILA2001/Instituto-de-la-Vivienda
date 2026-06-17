using Dominio;
using Negocio;
using System;
using WebForms.CustomControls;
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
            catch (Exception)
            {
                ToastService.Show(this.Page, "No se pudieron cargar las líneas de gestión. Intente nuevamente.", ToastService.ToastType.Error);
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
            catch
            {
                throw;
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
                ToastService.Show(this.Page, "¡Línea de Gestión FF agregada exitosamente!", ToastService.ToastType.Success);

                Session["listaLineasFF"] = negocio.listar();
                dgvLineaGestionFF.DataSource = Session["listaLineasFF"];
            }
            catch (InvalidOperationException ex)
            {  // Mostrar error si ocurre un problema
                ToastService.Show(this.Page, ex.Message, ToastService.ToastType.Error);
            }
            catch (Exception)
            {
                ToastService.Show(this.Page, "No se pudo agregar la línea de gestión. Intente nuevamente.", ToastService.ToastType.Error);
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
            catch (Exception)
            {
                ToastService.Show(this.Page, "Error al cambiar de página. Intente nuevamente.", ToastService.ToastType.Error);
            }
        }

    }
}