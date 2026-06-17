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
            catch (Exception)
            {
                ToastService.Show(this.Page, "Error al cargar los barrios. Intente nuevamente.", ToastService.ToastType.Error);
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

                    ToastService.Show(this.Page, "Barrio agregado exitosamente!", ToastService.ToastType.Success);
                    txtNombre.Text = string.Empty;
                    CargarListaBarrios();
                }
                else
                {
                    ToastService.Show(this.Page, "Debe llenar todos los campos.", ToastService.ToastType.Warning);
                }
            }
            catch (InvalidOperationException ex)
            {
                ToastService.Show(this.Page, ex.Message, ToastService.ToastType.Error);
            }
            catch (Exception)
            {
                ToastService.Show(this.Page, "No se pudo agregar el barrio. Intente nuevamente.", ToastService.ToastType.Error);
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
                    ToastService.Show(this.Page, "Barrio eliminado correctamente.", ToastService.ToastType.Success);
                    CargarListaBarrios();
                }
            }
            catch (InvalidOperationException ex)
            {
                ToastService.Show(this.Page, ex.Message, ToastService.ToastType.Error);
            }
            catch (Exception)
            {
                ToastService.Show(this.Page, "No se pudo eliminar el barrio. Intente nuevamente.", ToastService.ToastType.Error);
            }
        }
        protected void dgvBarrio_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            try
            {

                dgvBarrio.PageIndex = e.NewPageIndex;

                CargarListaBarrios();
            }
            catch (Exception)
            {
                ToastService.Show(this.Page, "Error al cambiar de página. Intente nuevamente.", ToastService.ToastType.Error);
            }
        }
    }
}