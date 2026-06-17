using Dominio;
using Negocio;
using System;
using WebForms.CustomControls;
using System.Collections.Generic;
using System.Data;
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
        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            string filtro = txtBuscar.Text.Trim();
            CargarListaEmpresas(filtro);
        }
        private void CargarListaEmpresas(string filtro = null)
        {


            try
            {
                Session["listaEmpresa"] = negocio.listar(filtro);
                dgvEmpresa.DataSource = Session["listaEmpresa"];
                dgvEmpresa.DataBind();
            }
            catch (Exception)
            {
                ToastService.Show(this.Page, "Error al cargar las empresas. Intente nuevamente.", ToastService.ToastType.Error);
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

                    ToastService.Show(this.Page, "¡Empresa agregada exitosamente!", ToastService.ToastType.Success);
                    txtNombre.Text = string.Empty;

                    CargarListaEmpresas();
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
                ToastService.Show(this.Page, "No se pudo agregar la empresa. Intente nuevamente.", ToastService.ToastType.Error);
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
                    ToastService.Show(this.Page, "Empresa eliminada correctamente.", ToastService.ToastType.Success);
                    CargarListaEmpresas();
                }
            }
            catch (InvalidOperationException ex)
            {
                ToastService.Show(this.Page, ex.Message, ToastService.ToastType.Error);
            }
            catch (Exception)
            {
                ToastService.Show(this.Page, "No se pudo eliminar la empresa. Intente nuevamente.", ToastService.ToastType.Error);
            }
        }


    }
}