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

        protected void Page_Init(object sender, EventArgs e)
        {
            cblEmpresa.SelectedIndexChanged += OnCheckBoxListSearch_SelectedIndexChanged;
            cblBarrio.SelectedIndexChanged += OnCheckBoxListSearch_SelectedIndexChanged;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindDropDownList();
                CargarListaObras();
            }
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            // No hay validadores específicos que necesiten manejarse de manera diferente
            // Ya que en este caso el área siempre viene del usuario logueado
        }

        protected void btnShowAddModal_Click(object sender, EventArgs e)
        {
            // Clear any existing data
            ClearFormFields();

            // Reset the modal title and button text to "Add" and show Obra field
            ScriptManager.RegisterStartupScript(this, this.GetType(), "ResetModalTitleAndShow", @"
                $(document).ready(function() {
                    $('#modalAgregar .modal-title').text('Agregar Obra');
                    document.getElementById('" + btnAgregar.ClientID + @"').value = 'Agregar';
                    
                    // Show the modal
                    $('#modalAgregar').modal('show');
                });", true);

            btnAgregar.Text = "Agregar";

            // Clear any editing state
            ViewState["EditingObraId"] = null;
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

        private void CargarListaObras(string filtro = null)
        {
            try
            {
                var selectedEmpresas = cblEmpresa.Items.Cast<ListItem>().Where(i => i.Selected).Select(i => i.Text).ToList();
                var selectedBarrios = cblBarrio.Items.Cast<ListItem>().Where(i => i.Selected).Select(i => i.Text).ToList();

                Usuario usuarioLogueado = (Usuario)Session["usuario"];
                Session["listaObra"] = negocio.listar(usuarioLogueado, selectedBarrios, selectedEmpresas, filtro);
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
            // If page is valid (all validators passed), proceed with creating the new object
            if (Page.IsValid)
            {
                ObraNegocio negocio = new ObraNegocio();
                Obra obra = new Obra();

                try
                {
                    // Common data for both add and update operations
                    obra.Numero = int.Parse(txtNumero.Text.Trim());
                    obra.Año = int.Parse(txtAño.Text.Trim());
                    obra.Etapa = int.Parse(txtEtapa.Text.Trim());
                    obra.ObraNumero = int.Parse(txtObra.Text.Trim());
                    obra.Descripcion = txtDescripcion.Text.Trim();
                    obra.Empresa = new Empresa { Id = int.Parse(ddlEmpresa.SelectedValue) };
                    obra.Contrata = new Contrata { Id = int.Parse(ddlContrata.SelectedValue) };
                    obra.Barrio = new Barrio { Id = int.Parse(ddlBarrio.SelectedValue) };

                    // Get area from user session
                    Usuario usuarioLogueado = (Usuario)Session["usuario"];
                    obra.Area = new Area();
                    obra.Area.Id = usuarioLogueado.Area.Id;
                    obra.Area.Nombre = usuarioLogueado.Area.Nombre;

                    // Check if we're editing or adding
                    if (ViewState["EditingObraId"] != null)
                    {
                        // We're updating an existing obra
                        obra.Id = (int)ViewState["EditingObraId"];

                        if (negocio.modificar(obra))
                        {
                            lblMensaje.Text = "Obra modificada exitosamente!";
                            lblMensaje.CssClass = "alert alert-success";

                            // Clear the editing state
                            ViewState["EditingObraId"] = null;
                        }
                        else
                        {
                            lblMensaje.Text = "Hubo un problema al modificar la obra.";
                            lblMensaje.CssClass = "alert alert-danger";
                        }
                    }
                    else
                    {
                        // We're adding a new obra
                        if (negocio.agregar(obra))
                        {
                            lblMensaje.Text = "Obra agregada exitosamente!";
                            lblMensaje.CssClass = "alert alert-success";
                        }
                        else
                        {
                            lblMensaje.Text = "Hubo un problema al agregar la obra.";
                            lblMensaje.CssClass = "alert alert-danger";
                        }
                    }

                    // Clear fields
                    ClearFormFields();

                    // Reset the modal title and button text
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "ResetModalTitle",
                        "$('#modalAgregar .modal-title').text('Agregar Obra');", true);
                    btnAgregar.Text = "Agregar";

                    // Hide the modal
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "HideModal",
                        "$('#modalAgregar').modal('hide');", true);

                    // Refresh the works list
                    CargarListaObras();
                }
                catch (Exception ex)
                {
                    lblMensaje.Text = $"Error: {ex.Message}";
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
        }

        protected void dgvObra_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                // Get the ID of the selected row
                int idObra = Convert.ToInt32(dgvObra.SelectedDataKey.Value);

                // Get the list of obras from session
                List<Obra> listaObras = (List<Obra>)Session["listaObra"];

                // Find the selected obra
                Obra obraSeleccionada = listaObras.FirstOrDefault(o => o.Id == idObra);

                if (obraSeleccionada != null)
                {
                    // Set button text to "Actualizar"
                    btnAgregar.Text = "Actualizar";

                    // Load the obra data into the form fields
                    txtNumero.Text = obraSeleccionada.Numero?.ToString();
                    txtAño.Text = obraSeleccionada.Año.ToString();
                    txtEtapa.Text = obraSeleccionada.Etapa.ToString();
                    txtObra.Text = obraSeleccionada.ObraNumero.ToString();
                    txtDescripcion.Text = obraSeleccionada.Descripcion;

                    // Select the corresponding values in the dropdowns
                    if (obraSeleccionada.Empresa != null)
                        SelectDropDownListByValue(ddlEmpresa, obraSeleccionada.Empresa.Id.ToString());

                    if (obraSeleccionada.Contrata != null)
                        SelectDropDownListByValue(ddlContrata, obraSeleccionada.Contrata.Id.ToString());

                    if (obraSeleccionada.Barrio != null)
                        SelectDropDownListByValue(ddlBarrio, obraSeleccionada.Barrio.Id.ToString());

                    // Store the ID of the obra being edited in ViewState
                    ViewState["EditingObraId"] = idObra;

                    // Update modal title and show it
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "UpdateModalAndShow", @"
                        $(document).ready(function() {
                            // Change title and button text
                            $('#modalAgregar .modal-title').text('Modificar Obra');
                            document.getElementById('" + btnAgregar.ClientID + @"').value = 'Actualizar';
                            
                            // Show the modal
                            $('#modalAgregar').modal('show');
                        });", true);
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cargar los datos de la obra: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        // Helper method to select dropdown item by value
        private void SelectDropDownListByValue(DropDownList dropDown, string value)
        {
            // Clear any current selection
            dropDown.ClearSelection();

            // Try to find and select the item
            ListItem item = dropDown.Items.FindByValue(value);
            if (item != null)
            {
                item.Selected = true;
            }
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
        private void BindDropDownList()
        {
            ddlEmpresa.Items.Clear();
            ddlContrata.Items.Clear();
            ddlBarrio.Items.Clear();

            // Add empty items to each dropdown
            ddlEmpresa.Items.Add(new ListItem("Seleccione una empresa", ""));
            ddlContrata.Items.Add(new ListItem("Seleccione una contrata", ""));
            ddlBarrio.Items.Add(new ListItem("Seleccione un barrio", ""));

            // Set AppendDataBoundItems property to true for all dropdowns
            ddlEmpresa.AppendDataBoundItems = true;
            ddlContrata.AppendDataBoundItems = true;
            ddlBarrio.AppendDataBoundItems = true;

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

            cblBarrio.DataSource = ObtenerBarrios();
            cblBarrio.DataTextField = "Nombre";
            cblBarrio.DataValueField = "Id";
            cblBarrio.DataBind();


            cblEmpresa.DataSource = ObtenerEmpresas();
            cblEmpresa.DataTextField = "Nombre";
            cblEmpresa.DataValueField = "Id";
            cblEmpresa.DataBind();

        }


      
        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            string filtro = txtBuscar.Text.Trim();
            CargarListaObras(filtro);
        }

        protected void OnCheckBoxListSearch_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarListaObras();
        }

        protected void BtnClearFilters_Click(object sender, EventArgs e)
        {
            txtBuscar.Text = string.Empty;
            cblEmpresa.ClearSelection();
            cblBarrio.ClearSelection();
            CargarListaObras();
        }
    }
}