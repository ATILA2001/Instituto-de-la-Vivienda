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
            cblEmpresa.AcceptChanges += OnAcceptChanges;
            cblBarrio.AcceptChanges += OnAcceptChanges;
            cblArea.AcceptChanges += OnAcceptChanges;
        }

        private void OnAcceptChanges(object sender, EventArgs e)
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

        protected void Page_PreRender(object sender, EventArgs e)
        {
            // Configure validators if we're in editing mode
            if (ViewState["EditingObraId"] != null)
            {
                // Disable the Area validator since the field is hidden
                rfvArea.Enabled = false;
            }
            else
            {
                // Enable validators in add mode
                rfvArea.Enabled = true;
            }
        }

        protected void btnShowAddModal_Click(object sender, EventArgs e)
        {
            // Clear any existing data
            ClearFormFields();

            // Reset the modal title and button text to "Add" and show Area field
            ScriptManager.RegisterStartupScript(this, this.GetType(), "ResetModalTitleAndShow", @"
                $(document).ready(function() {
                    $('#modalAgregar .modal-title').text('Agregar Obra');
                    document.getElementById('" + btnAgregar.ClientID + @"').value = 'Agregar';
                    
                    // Show the Area dropdown and its container
                    $('#areaContainer').show();
                    
                    // Show the modal
                    $('#modalAgregar').modal('show');
                });", true);

            btnAgregar.Text = "Agregar";

            // Clear any editing state
            ViewState["EditingObraId"] = null;
            ViewState["EditingAreaId"] = null;
            ViewState["EditingAreaNombre"] = null;
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
            ClearFormFields();
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

        private void CargarListaObras(string filtro = null)
        {
            try
            {
                var selectedEmpresas = cblEmpresa.SelectedValues;
                var selectedBarrios = cblBarrio.SelectedValues;
                var selectedAreas = cblArea.SelectedValues;

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
            try
            {
                // Get the ID of the selected row
                int idObra = Convert.ToInt32(dgvObra.SelectedDataKey.Value);

                // Get the list of obras from session
                List<Obra> listaObras = (List<Obra>)Session["listaObraAdmin"];

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

                    // Select the corresponding values in the dropdowns (except Area which will be hidden)
                    if (obraSeleccionada.Empresa != null)
                        SelectDropDownListByValue(ddlEmpresa, obraSeleccionada.Empresa.Id.ToString());

                    if (obraSeleccionada.Contrata != null)
                        SelectDropDownListByValue(ddlContrata, obraSeleccionada.Contrata.Id.ToString());

                    if (obraSeleccionada.Barrio != null)
                        SelectDropDownListByValue(ddlBarrio, obraSeleccionada.Barrio.Id.ToString());

                    // Store the ID of the obra being edited in ViewState
                    ViewState["EditingObraId"] = idObra;

                    // Store the Area data for use during update
                    if (obraSeleccionada.Area != null)
                    {
                        ViewState["EditingAreaId"] = obraSeleccionada.Area.Id;
                        ViewState["EditingAreaNombre"] = obraSeleccionada.Area.Nombre;
                    }

                    // Update modal title and hide the Area field
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "UpdateModalAndShow", @"
                        $(document).ready(function() {
                            // Change title and button text
                            $('#modalAgregar .modal-title').text('Modificar Obra');
                            document.getElementById('" + btnAgregar.ClientID + @"').value = 'Actualizar';
                            
                            // Hide the Area dropdown and its container
                            $('#areaContainer').hide();
                            
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

        protected void btnAgregar_Click(object sender, EventArgs e)
        {
            // If page is valid (all validators passed), proceed with creating the new object
            if (Page.IsValid)
            {
                ObraNegocio negocio = new ObraNegocio();
                Obra obra = new Obra();

                try
                {
                    // Assign common values to the obra
                    obra.Numero = int.Parse(txtNumero.Text.Trim());
                    obra.Año = int.Parse(txtAño.Text.Trim());
                    obra.Etapa = int.Parse(txtEtapa.Text.Trim());
                    obra.ObraNumero = int.Parse(txtObra.Text.Trim());
                    obra.Descripcion = txtDescripcion.Text.Trim();
                    obra.Empresa = new Empresa { Id = int.Parse(ddlEmpresa.SelectedValue) };
                    obra.Contrata = new Contrata { Id = int.Parse(ddlContrata.SelectedValue) };
                    obra.Barrio = new Barrio { Id = int.Parse(ddlBarrio.SelectedValue) };

                    // Check if we're editing an existing obra or adding a new one
                    if (ViewState["EditingObraId"] != null)
                    {
                        // We're updating an existing obra
                        obra.Id = (int)ViewState["EditingObraId"];

                        // Use the stored Area data for the update
                        if (ViewState["EditingAreaId"] != null)
                        {
                            obra.Area = new Area
                            {
                                Id = (int)ViewState["EditingAreaId"],
                                Nombre = ViewState["EditingAreaNombre"]?.ToString()
                            };
                        }

                        if (negocio.modificar(obra))
                        {
                            lblMensaje.Text = "Obra modificada exitosamente!";
                            lblMensaje.CssClass = "alert alert-success";

                            // Clear the editing state
                            ViewState["EditingObraId"] = null;
                            ViewState["EditingAreaId"] = null;
                            ViewState["EditingAreaNombre"] = null;
                        }
                        else
                        {
                            lblMensaje.Text = "Hubo un problema al modificar la obra.";
                            lblMensaje.CssClass = "alert alert-danger";
                        }
                    }
                    else
                    {
                        // We're adding a new obra - use the selected Area
                        obra.Area = new Area
                        {
                            Id = int.Parse(ddlArea.SelectedValue),
                            Nombre = ddlArea.SelectedItem.Text
                        };

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