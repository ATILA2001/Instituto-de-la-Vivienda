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
    public partial class Obras : System.Web.UI.Page
    {
        private ObraNegocio negocio = new ObraNegocio();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindDropDownList();
                CargarListaObras();
            }
        }
        protected void ddlFiltroEmpresa_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarListaObras();
        }
        protected void ddlBarrioFiltro_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarListaObras();
        }
        protected void btnLimpiar_Click(object sender, EventArgs e)
        {
            // Limpiar todos los TextBox
            txtNumero.Text = string.Empty;
            txtAño.Text = string.Empty;
            txtEtapa.Text = string.Empty;
            txtObra.Text = string.Empty;
            txtDescripcion.Text = string.Empty;

            // Limpiar los DropDownLists si es necesario
            ddlEmpresa.SelectedIndex = -1;
            ddlContrata.SelectedIndex = -1;
            ddlBarrio.SelectedIndex = -1;
        }
        private DataTable ObtenerEmpresas()
        {
            EmpresaNegocio empresaNegocio = new EmpresaNegocio();
            return empresaNegocio.listarddl();
        }

        private DataTable ObtenerContratas()
        {
            ContrataNegocio contrataNegocio = new ContrataNegocio();
            return contrataNegocio.listarddl();
        }

        private DataTable ObtenerBarrios()
        {
            BarrioNegocio barrioNegocio = new BarrioNegocio();
            return barrioNegocio.listarddl();
        }
        private void CargarListaObras()
        {
            try
            {

                string barrio = ddlBarrioFiltro.SelectedValue == "0" ? null : ddlBarrioFiltro.SelectedItem.Text;
                string empresa = ddlFiltroEmpresa.SelectedValue == "0" ? null : ddlFiltroEmpresa.SelectedItem.Text;

                Usuario usuarioLogueado = (Usuario)Session["usuario"];
                Session["listaObra"] = negocio.listar(usuarioLogueado,barrio, empresa);
                dgvObra.DataSource = Session["listaObra"];
                dgvObra.DataBind();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cargar las obras: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        protected void btnAgregar_Click(object sender, EventArgs e)
        {
            ObraNegocio negocio = new ObraNegocio();
            Obra nuevaObra = new Obra();

            try
            {
                // Validar que los campos no estén vacíos o nulos
                if (txtDescripcion.Text.Trim() != string.Empty &&
                    txtNumero.Text.Trim() != string.Empty &&
                    txtAño.Text.Trim() != string.Empty &&
                    txtEtapa.Text.Trim() != string.Empty &&
                    txtObra.Text.Trim() != string.Empty &&
                    ddlEmpresa.SelectedIndex != -1 &&
                    ddlContrata.SelectedIndex != -1 &&
                    ddlBarrio.SelectedIndex != -1)
                {
                    // Asignar los valores a la nueva obra
                    nuevaObra.Numero = int.Parse(txtNumero.Text.Trim());
                    nuevaObra.Año = int.Parse(txtAño.Text.Trim());
                    nuevaObra.Etapa = int.Parse(txtEtapa.Text.Trim());
                    nuevaObra.ObraNumero = int.Parse(txtObra.Text.Trim());
                    nuevaObra.Descripcion = txtDescripcion.Text.Trim();

                    // Asignar los objetos de relaciones
                    nuevaObra.Empresa = new Empresa { Id = int.Parse(ddlEmpresa.SelectedValue) };
                    nuevaObra.Contrata = new Contrata { Id = int.Parse(ddlContrata.SelectedValue) };
                    nuevaObra.Barrio = new Barrio { Id = int.Parse(ddlBarrio.SelectedValue) };
                    Usuario usuarioLogueado = (Usuario)Session["usuario"];
                    nuevaObra.Area = new Area();
                    nuevaObra.Area.Id = usuarioLogueado.Area.Id;

                    nuevaObra.Area.Nombre = usuarioLogueado.Area.Nombre;

                    // Llamar al método agregar de ObraNegocio
                    if (negocio.agregar(nuevaObra))
                    {
                        lblMensaje.Text = "Obra agregada exitosamente!";
                        lblMensaje.CssClass = "alert alert-success";

                        // Limpiar los campos
                        txtNumero.Text = string.Empty;
                        txtAño.Text = string.Empty;
                        txtEtapa.Text = string.Empty;
                        txtObra.Text = string.Empty;
                        txtDescripcion.Text = string.Empty;
                        ddlEmpresa.SelectedIndex = -1;
                        ddlContrata.SelectedIndex = -1;
                        ddlBarrio.SelectedIndex = -1;

                        // Refrescar la lista de obras
                        CargarListaObras();
                    }
                    else
                    {
                        lblMensaje.Text = "Hubo un problema al agregar la obra.";
                        lblMensaje.CssClass = "alert alert-danger";
                    }
                }
                else
                {
                    lblMensaje.Text = "Debe llenar todos los campos correctamente.";
                    lblMensaje.CssClass = "alert alert-danger";
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al agregar la obra: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        protected void dgvObra_SelectedIndexChanged(object sender, EventArgs e)
        {
            var idSeleccionado = dgvObra.SelectedDataKey.Value.ToString();
            Response.Redirect("ModificarObraUser.aspx?codM=" + idSeleccionado);
        }
        protected void dgvObra_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {

            try
            {
                var id = Convert.ToInt32(dgvObra.DataKeys[e.RowIndex].Value);
                if (negocio.eliminar(id))
                {
                    lblMensaje.Text = "Obra eliminada correctamente.";
                    lblMensaje.CssClass = "alert alert-success";
                    CargarListaObras(); // Actualizar el GridView
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al eliminar la obra: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }
        private void BindDropDownList()
        {
            
            ddlEmpresa.DataSource = ObtenerEmpresas();  
            ddlEmpresa.DataTextField = "Nombre";         
            ddlEmpresa.DataValueField = "Id";            
            ddlEmpresa.DataBind();

            
            ddlContrata.DataSource = ObtenerContratas();  
            ddlContrata.DataTextField = "Nombre";
            ddlContrata.DataValueField = "Id";
            ddlContrata.DataBind();

            ddlBarrio.DataSource = ObtenerBarrios();    
            ddlBarrio.DataTextField = "Nombre";
            ddlBarrio.DataValueField = "Id";
            ddlBarrio.DataBind();

            var barrio = ObtenerBarrios();
            barrio.Rows.InsertAt(CrearFilaTodos(barrio), 0);
            ddlBarrioFiltro.DataSource = barrio;
            ddlBarrioFiltro.DataTextField = "Nombre";
            ddlBarrioFiltro.DataValueField = "Id";
            ddlBarrioFiltro.DataBind();

            var empresa = ObtenerEmpresas();
            empresa.Rows.InsertAt(CrearFilaTodos(empresa), 0);
            ddlFiltroEmpresa.DataSource = empresa;
            ddlFiltroEmpresa.DataTextField = "Nombre";
            ddlFiltroEmpresa.DataValueField = "Id";
            ddlFiltroEmpresa.DataBind();

        }

        private DataRow CrearFilaTodos(DataTable table)
        {
            DataRow row = table.NewRow();
            row["Id"] = 0;
            row["Nombre"] = "Todos";
            return row;
        }
        protected void dgvObra_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            try
            {
                // Cambiar el índice de la página
                dgvObra.PageIndex = e.NewPageIndex;

                // Refrescar el listado de empresas
                CargarListaObras();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cambiar de página: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }
    }
}