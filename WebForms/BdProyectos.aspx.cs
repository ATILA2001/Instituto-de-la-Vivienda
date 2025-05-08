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
    public partial class BdProyectos : System.Web.UI.Page
    {
        BdProyectoNegocio bdProyectoNegocio = new BdProyectoNegocio();

        protected void Page_Init(object sender, EventArgs e)
        {
            cblArea.AcceptChanges += CblFiltro_AcceptChanges;
            cblLinea.AcceptChanges += CblFiltro_AcceptChanges;
            cblProyecto.AcceptChanges += CblFiltro_AcceptChanges;
        }

        private void CblFiltro_AcceptChanges(object sender, EventArgs e)
        {
            CargarListaProyectos();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindDropDownList();
                CargarListaProyectos();
                CalcularSubtotal();
            }
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            // Configure validators if we're in editing mode
            if (ViewState["EditingProyectoId"] != null)
            {
                // No special validations needed to disable
            }
            else
            {
                // Normal validation in add mode
            }
        }

        protected void btnShowAddModal_Click(object sender, EventArgs e)
        {
            // Clear any existing data
            LimpiarFormulario();

            // Reset the modal title and button text to "Add" and show Obra field
            ScriptManager.RegisterStartupScript(this, this.GetType(), "ResetModalTitleAndShow", @"
                $(document).ready(function() {
                    $('#modalAgregar .modal-title').text('Agregar Proyecto');
                    document.getElementById('" + btnAgregar.ClientID + @"').value = 'Agregar';
                    
                    // Show the Obra dropdown
                    $('#obraContainer').show();
                    
                    // Show the modal
                    $('#modalAgregar').modal('show');
                });", true);

            btnAgregar.Text = "Agregar";

            // Clear any editing state
            ViewState["EditingProyectoId"] = null;
            ViewState["EditingObraId"] = null;
        }

        private void CargarListaProyectos(string filtro = null)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("CargarListaProyectos EJECUTADO");
                var selectedAreas = cblArea.SelectedValues;
                var selectedLineas = cblLinea.SelectedValues;
                var selectedProyectos = cblProyecto.SelectedValues;

                // Imprimir los valores obtenidos para depuración
                System.Diagnostics.Debug.WriteLine("Selected Areas: " + (selectedAreas.Any() ? string.Join(", ", selectedAreas) : "NINGUNA"));
                System.Diagnostics.Debug.WriteLine("Selected Lineas: " + (selectedLineas.Any() ? string.Join(", ", selectedLineas) : "NINGUNA"));
                System.Diagnostics.Debug.WriteLine("Selected Proyectos: " + (selectedProyectos.Any() ? string.Join(", ", selectedProyectos) : "NINGUNO"));


                Session["listaProyectos"] = bdProyectoNegocio.Listar(selectedLineas, selectedProyectos, selectedAreas, filtro);
                dgvBdProyecto.DataSource = Session["listaProyectos"];
                dgvBdProyecto.DataBind();
                CalcularSubtotal();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cargar los Proyectos: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            string filtro = txtBuscar.Text.Trim();
            CargarListaProyectos(filtro);
        }

        protected void ddlAreaFiltro_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarListaProyectos();
            CalcularSubtotal();
        }

        private DataTable ObtenerAreas()
        {
            AreaNegocio areaNegocio = new AreaNegocio();
            return areaNegocio.listarddl();
        }

        protected void ddlLinea_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarListaProyectos();
            CalcularSubtotal();
        }

        protected void ddlProyecto_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarListaProyectos();
            CalcularSubtotal();
        }

        private void CalcularSubtotal()
        {
            decimal subtotal = 0;

            foreach (GridViewRow row in dgvBdProyecto.Rows)
            {
                var cellValue = row.Cells[6].Text;
                if (decimal.TryParse(cellValue, System.Globalization.NumberStyles.Currency, null, out decimal monto))
                {
                    subtotal += monto;
                }
            }
            txtSubtotal.Text = subtotal.ToString("C");
        }

        protected void dgvBdProyecto_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                // Get the ID of the selected row
                int idProyecto = Convert.ToInt32(dgvBdProyecto.SelectedDataKey.Value);

                // Get the list of projects from session
                List<BdProyecto> listaProyectos = (List<BdProyecto>)Session["listaProyectos"];

                // Find the selected project
                BdProyecto proyectoSeleccionado = listaProyectos.FirstOrDefault(p => p.Id == idProyecto);

                if (proyectoSeleccionado != null)
                {
                    // Set button text to "Actualizar"
                    btnAgregar.Text = "Actualizar";

                    // Load the project data into the form fields
                    txtProyecto.Text = proyectoSeleccionado.Proyecto;
                    txtSubProyecto.Text = proyectoSeleccionado.SubProyecto;
                    txtMontoAutorizadoInicial.Text = proyectoSeleccionado.AutorizadoInicial.ToString("0.00");

                    // Select the corresponding values in the dropdowns
                    if (proyectoSeleccionado.LineaGestion != null)
                        SelectDropDownListByValue(ddlLineaGestion, proyectoSeleccionado.LineaGestion.Id.ToString());

                    // Store the IDs for update
                    ViewState["EditingProyectoId"] = idProyecto;
                    if (proyectoSeleccionado.Obra != null)
                        ViewState["EditingObraId"] = proyectoSeleccionado.Obra.Id;

                    // Update modal title and hide the Obra field (read-only)
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "UpdateModalAndShow", @"
                        $(document).ready(function() {
                            // Change title and button text
                            $('#modalAgregar .modal-title').text('Modificar Proyecto');
                            document.getElementById('" + btnAgregar.ClientID + @"').value = 'Actualizar';
                            
                            // Hide the Obra dropdown
                            $('#obraContainer').hide();
                            
                            // Show the modal
                            $('#modalAgregar').modal('show');
                        });", true);
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cargar los datos del proyecto: {ex.Message}";
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

        protected void dgvBdProyecto_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            try
            {
                var id = Convert.ToInt32(dgvBdProyecto.DataKeys[e.RowIndex].Value);
                if (bdProyectoNegocio.eliminar(id))
                {
                    lblMensaje.Text = "Proyecto eliminado correctamente.";
                    lblMensaje.CssClass = "alert alert-success";
                    CargarListaProyectos();
                    CalcularSubtotal();
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al eliminar el proyecto: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        protected void dgvBdProyecto_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            CargarListaProyectos();
            CalcularSubtotal();
        }

        protected void btnAgregar_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            try
            {
                BdProyecto proyecto = new BdProyecto();
                proyecto.Proyecto = txtProyecto.Text;
                proyecto.SubProyecto = txtSubProyecto.Text;
                proyecto.LineaGestion = new LineaGestion();
                proyecto.LineaGestion.Id = int.Parse(ddlLineaGestion.SelectedValue);
                proyecto.LineaGestion.Nombre = ddlLineaGestion.SelectedItem.Text;
                proyecto.AutorizadoInicial = Convert.ToDecimal(txtMontoAutorizadoInicial.Text);

                if (ViewState["EditingProyectoId"] != null)
                {
                    // Editing existing project
                    proyecto.Id = (int)ViewState["EditingProyectoId"];

                    // Use the stored Obra ID
                    if (ViewState["EditingObraId"] != null)
                    {
                        proyecto.Obra = new Obra { Id = (int)ViewState["EditingObraId"] };
                    }

                    if (bdProyectoNegocio.modificar(proyecto))
                    {
                        lblMensaje.Text = "Proyecto modificado exitosamente!";
                        lblMensaje.CssClass = "alert alert-success";

                        // Clear the editing state
                        ViewState["EditingProyectoId"] = null;
                        ViewState["EditingObraId"] = null;
                    }
                    else
                    {
                        lblMensaje.Text = "Hubo un problema al modificar el proyecto.";
                        lblMensaje.CssClass = "alert alert-danger";
                    }
                }
                else
                {
                    // Adding new project
                    proyecto.Obra = new Obra();
                    proyecto.Obra.Id = Convert.ToInt32(ddlObra.SelectedValue);

                    if (bdProyectoNegocio.agregar(proyecto))
                    {
                        lblMensaje.Text = "Proyecto agregado exitosamente!";
                        lblMensaje.CssClass = "alert alert-success";
                    }
                    else
                    {
                        lblMensaje.Text = "Hubo un problema al agregar el proyecto.";
                        lblMensaje.CssClass = "alert alert-danger";
                    }
                }

                // Clear fields
                LimpiarFormulario();

                // Reset the modal title and button text
                ScriptManager.RegisterStartupScript(this, this.GetType(), "ResetModalTitle",
                    "$('#modalAgregar .modal-title').text('Agregar Proyecto');", true);
                btnAgregar.Text = "Agregar";

                // Hide the modal
                ScriptManager.RegisterStartupScript(this, this.GetType(), "HideModal",
                    "$('#modalAgregar').modal('hide');", true);

                // Refresh the projects list
                CargarListaProyectos();
                CalcularSubtotal();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = "Error: " + ex.Message;
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        private void LimpiarFormulario()
        {
            txtProyecto.Text = string.Empty;
            txtSubProyecto.Text = string.Empty;
            txtMontoAutorizadoInicial.Text = string.Empty;
            ddlObra.SelectedIndex = 0;
            ddlLineaGestion.SelectedIndex = 0;
        }
        private void BindDropDownList()
        {
            cblProyecto.DataSource = ObtenerProyecto();
            cblProyecto.DataTextField = "Nombre";
            cblProyecto.DataValueField = "Id";
            cblProyecto.DataBind();

            ddlObra.DataSource = ObtenerObras();
            ddlObra.DataTextField = "Nombre";
            ddlObra.DataValueField = "Id";
            ddlObra.DataBind();

            cblArea.DataSource = ObtenerAreas();
            cblArea.DataTextField = "Nombre";
            cblArea.DataValueField = "Id";
            cblArea.DataBind();

            ddlLineaGestion.DataSource = ObtenerLineaGestion();
            ddlLineaGestion.DataTextField = "Nombre";
            ddlLineaGestion.DataValueField = "Id";
            ddlLineaGestion.DataBind();

           
            cblLinea.DataSource = ObtenerLineaGestion();
            cblLinea.DataTextField = "Nombre";
            cblLinea.DataValueField = "Id";
            cblLinea.DataBind();
        }
       
        private DataTable ObtenerObras()
        {
            ObraNegocio barrioNegocio = new ObraNegocio();
            return barrioNegocio.listarddlProyecto();
        }
        private DataTable ObtenerProyecto()
        {
            BdProyectoNegocio barrioNegocio = new BdProyectoNegocio();
            return barrioNegocio.listarddl();
        }
        private DataTable ObtenerLineaGestion()
        {
            LineaGestionNegocio barrioNegocio = new LineaGestionNegocio();
            return barrioNegocio.listarddl();
        }

        protected void BtnClearFilters_Click(object sender, EventArgs e)
        {
            txtBuscar.Text = string.Empty;
            cblArea.ClearSelection();
            cblLinea.ClearSelection();
            cblProyecto.ClearSelection();
            CargarListaProyectos();
        }

    }
}