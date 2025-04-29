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
    public partial class ObrasAdmin : System.Web.UI.Page
    {
        private ObraNegocio negocio = new ObraNegocio();

        protected void Page_Init(object sender, EventArgs e)
        {
            cblEmpresa.SelectedIndexChanged += OnCheckBoxListSearch_SelectedIndexChanged;
            cblBarrio.SelectedIndexChanged += OnCheckBoxListSearch_SelectedIndexChanged;
            cblArea.SelectedIndexChanged += OnCheckBoxListSearch_SelectedIndexChanged;
        }

        private void OnCheckBoxListSearch_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarListaObras();
        }

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
        protected void ddlAreaFiltro_SelectedIndexChanged(object sender, EventArgs e)
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
        private DataTable ObtenerAreas()
        {
            AreaNegocio areaNegocio = new AreaNegocio();
            return areaNegocio.listarddl();
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

        private void CargarListaObras(string filtro= null)
        {
            try
            {
                var selectedEmpresas = cblEmpresa.Items.Cast<ListItem>().Where(i => i.Selected).Select(i => i.Text).ToList();

                var selectedBarrios = cblBarrio.Items.Cast<ListItem>().Where(i => i.Selected).Select(i => i.Text).ToList();
                var selectedAreas = cblArea.Items.Cast<ListItem>().Where(i => i.Selected).Select(i => i.Text).ToList();

                Session["listaObraAdmin"] = negocio.listar(selectedBarrios, selectedEmpresas, selectedAreas, filtro);
                dgvObra.DataSource = Session["listaObraAdmin"];
                dgvObra.DataBind();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cargar las obras: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        protected void dgvObra_SelectedIndexChanged(object sender, EventArgs e)
        {
            var idSeleccionado = dgvObra.SelectedDataKey.Value.ToString();
            Response.Redirect("ModificarObra.aspx?codM=" + idSeleccionado);
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

        protected void btnAgregar_Click(object sender, EventArgs e)
        {
            // If page is valid (all validators passed), proceed with creating the new object
            if (Page.IsValid)
            {
                ObraNegocio negocio = new ObraNegocio();
                Obra nuevaObra = new Obra();

                try
                {
                    // Assign values to the new obra
                    nuevaObra.Numero = int.Parse(txtNumero.Text.Trim());
                    nuevaObra.Año = int.Parse(txtAño.Text.Trim());
                    nuevaObra.Etapa = int.Parse(txtEtapa.Text.Trim());
                    nuevaObra.ObraNumero = int.Parse(txtObra.Text.Trim());
                    nuevaObra.Descripcion = txtDescripcion.Text.Trim();

                    // Assign relationship objects
                    nuevaObra.Empresa = new Empresa { Id = int.Parse(ddlEmpresa.SelectedValue) };
                    nuevaObra.Contrata = new Contrata { Id = int.Parse(ddlContrata.SelectedValue) };
                    nuevaObra.Barrio = new Barrio { Id = int.Parse(ddlBarrio.SelectedValue) };
                    Usuario usuarioLogueado = (Usuario)Session["usuario"];
                    nuevaObra.Area = new Area();
                    nuevaObra.Area.Id = int.Parse(ddlArea.SelectedValue);
                    nuevaObra.Area.Nombre = ddlArea.SelectedItem.Text;

                    // Call the add method from ObraNegocio
                    if (negocio.agregar(nuevaObra))
                    {
                        lblMensaje.Text = "Obra agregada exitosamente!";
                        lblMensaje.CssClass = "alert alert-success";

                        // Clear fields
                        ClearFormFields();

                        // Refresh the works list
                        CargarListaObras();
                    }
                    else
                    {
                        lblMensaje.Text = "Hubo un problema al agregar la obra.";
                        lblMensaje.CssClass = "alert alert-danger";
                    }
                }
                catch (Exception ex)
                {
                    lblMensaje.Text = $"Error al agregar la obra: {ex.Message}";
                    lblMensaje.CssClass = "alert alert-danger";
                }
            }
        }

        private void ClearFormFields()
        {
            txtNumero.Text = string.Empty;
            txtAño.Text = string.Empty;
            txtEtapa.Text = string.Empty;
            txtObra.Text = string.Empty;
            txtDescripcion.Text = string.Empty;
            ddlEmpresa.SelectedIndex = 0;
            ddlContrata.SelectedIndex = 0;
            ddlBarrio.SelectedIndex = 0;
            ddlArea.SelectedIndex = 0;
        }
        private void BindDropDownList()
        {
            ddlEmpresa.Items.Clear();
            ddlArea.Items.Clear();
            ddlContrata.Items.Clear();
            ddlBarrio.Items.Clear();

            // Add empty items to each dropdown
            ddlEmpresa.Items.Add(new ListItem("Seleccione una empresa", ""));
            ddlArea.Items.Add(new ListItem("Seleccione un área", ""));
            ddlContrata.Items.Add(new ListItem("Seleccione una contrata", ""));
            ddlBarrio.Items.Add(new ListItem("Seleccione un barrio", ""));

            // Set AppendDataBoundItems property to true for all dropdowns
            ddlEmpresa.AppendDataBoundItems = true;
            ddlArea.AppendDataBoundItems = true;
            ddlContrata.AppendDataBoundItems = true;
            ddlBarrio.AppendDataBoundItems = true;

            ddlEmpresa.DataSource = ObtenerEmpresas();
            ddlEmpresa.DataTextField = "Nombre";
            ddlEmpresa.DataValueField = "Id";
            ddlEmpresa.DataBind();

            ddlArea.DataSource = ObtenerAreas();    // Método para obtener los datos de los Barrios
            ddlArea.DataTextField = "Nombre";
            ddlArea.DataValueField = "Id";
            ddlArea.DataBind();


            ddlContrata.DataSource = ObtenerContratas();
            ddlContrata.DataTextField = "Nombre";
            ddlContrata.DataValueField = "Id";
            ddlContrata.DataBind();

            ddlBarrio.DataSource = ObtenerBarrios();
            ddlBarrio.DataTextField = "Nombre";
            ddlBarrio.DataValueField = "Id";
            ddlBarrio.DataBind();

            cblBarrio.DataSource = ObtenerBarrios();
            cblBarrio.DataTextField = "Nombre";
            cblBarrio.DataValueField = "Id";
            cblBarrio.DataBind();


            cblEmpresa.DataSource = ObtenerEmpresas();
            cblEmpresa.DataTextField = "Nombre";
            cblEmpresa.DataValueField = "Id";
            cblEmpresa.DataBind();

            cblArea.DataSource = ObtenerAreas();
            cblArea.DataTextField = "Nombre";
            cblArea.DataValueField = "Id";
            cblArea.DataBind();


        }

        private DataRow CrearFilaTodos(DataTable table)
        {
            DataRow row = table.NewRow();
            row["Id"] = 0;
            row["Nombre"] = "Todos";
            return row;
        }
        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            string filtro = txtBuscar.Text.Trim();
            CargarListaObras(filtro);
        }

        protected void BtnClearFilters_Click(object sender, EventArgs e)
        {
            txtBuscar.Text = string.Empty;
            cblArea.ClearSelection();
            cblBarrio.ClearSelection();
            cblEmpresa.ClearSelection();
            CargarListaObras();
        }

    }
}