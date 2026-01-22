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

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Load complete list for filters
                Session["listaProyectosCompleta"] = bdProyectoNegocio.Listar(null, null, null, null);

                BindDropDownList();
                CargarListaProyectos();
                CalcularSubtotal();
            }
        }

        public void OnAcceptChanges(object sender, EventArgs e)
        {
            CargarListaProyectos();
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

        private void CargarListaProyectos(string filtro = null, bool forzarRecargaCompleta = false)
        {
            try
            {
                List<string> selectedAreas = new List<string>();
                List<string> selectedProyectos = new List<string>();
                List<string> selectedLineas = new List<string>();

                if (dgvBdProyecto.HeaderRow != null)
                {
                    var cblHeaderArea = dgvBdProyecto.HeaderRow.FindControl("cblHeaderArea") as WebForms.CustomControls.TreeViewSearch;
                    if (cblHeaderArea != null) selectedAreas = cblHeaderArea.SelectedValues;

                    var cblHeaderProyecto = dgvBdProyecto.HeaderRow.FindControl("cblHeaderProyecto") as WebForms.CustomControls.TreeViewSearch;
                    if (cblHeaderProyecto != null) selectedProyectos = cblHeaderProyecto.SelectedValues;

                    var cblHeaderLineaGestion = dgvBdProyecto.HeaderRow.FindControl("cblHeaderLineaGestion") as WebForms.CustomControls.TreeViewSearch;
                    if (cblHeaderLineaGestion != null) selectedLineas = cblHeaderLineaGestion.SelectedValues;
                }

                // Si el filtro de texto general está vacío, tomar el del TextBox txtBuscar
                if (string.IsNullOrEmpty(filtro))
                {
                    filtro = txtBuscar.Text.Trim();
                }

                // Load complete list for filters if needed
                if (forzarRecargaCompleta || Session["listaProyectosCompleta"] == null)
                {
                    // Only load from database when forced or data doesn't exist in session
                    Session["listaProyectosCompleta"] = bdProyectoNegocio.Listar(null, null, null, null);
                }

                // Always fetch filtered list based on current parameters
                Session["listaProyectos"] = bdProyectoNegocio.Listar(selectedLineas, selectedProyectos, selectedAreas, filtro);
                dgvBdProyecto.DataSource = Session["listaProyectos"];
                dgvBdProyecto.DataBind();
                CalcularSubtotal();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cargar los Proyectos: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
                dgvBdProyecto.DataSource = null; // Asegurar que la grilla esté vacía en caso de error
                dgvBdProyecto.DataBind();
                //txtSubtotal.Text = 0.ToString("C");
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

        //private DataTable ObtenerAreas()
        //{
        //    AreaNegocio areaNegocio = new AreaNegocio();
        //    return areaNegocio.listarddl();
        //}

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
            //txtSubtotal.Text = subtotal.ToString("C");
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
                    txtMontoAutorizado2025.Text = proyectoSeleccionado.Autorizado2025.ToString("0.00");

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
                    CargarListaProyectos(null, true); // Force complete reload
                    CalcularSubtotal();
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al eliminar el proyecto: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        protected void dgvBdProyecto_RowDataBound(object sender, GridViewRowEventArgs e)
        {

            // Asegura que solo se procese la fila de cabecera
            if (e.Row.RowType == DataControlRowType.Header)
            {
                List<BdProyecto> proyectosCompleto = Session["listaProyectosCompleta"] as List<BdProyecto>;
                if (proyectosCompleto == null || !proyectosCompleto.Any())
                {
                    // Si no hay datos completos, intentar recargarlos (aunque idealmente ya deberían estar)
                    proyectosCompleto = bdProyectoNegocio.Listar(null, null, null, null);
                    Session["listaProyectosCompleta"] = proyectosCompleto;
                    if (proyectosCompleto == null || !proyectosCompleto.Any()) return;
                }

                var cblsHeaderArea = e.Row.FindControl("cblHeaderArea") as WebForms.CustomControls.TreeViewSearch;
                // Poblar filtro de Área
                if (cblsHeaderArea != null)
                {
                    var areasUnicas = proyectosCompleto
                        .Where(c => c.Obra?.Area != null && c.Obra.Area.Id > 0) // Validar Id del área
                        .Select(c => c.Obra.Area) // Seleccionar el objeto Area completo
                        .GroupBy(a => a.Id)       // Agrupar por el Id numérico del Area para obtener unicidad
                        .Select(g => g.First())   // Tomar el primer objeto Area de cada grupo
                        .Select(a => new { Id = a.Id, Nombre = a.Nombre }) // Proyectar a Id (numérico) y Nombre
                        .OrderBy(a => a.Nombre)
                        .ToList();

                    cblsHeaderArea.DataSource = areasUnicas;
                    cblsHeaderArea.DataBind();
                }


                // Poblar filtro de Proyecto
                var cblsHeaderProyecto = e.Row.FindControl("cblHeaderProyecto") as WebForms.CustomControls.TreeViewSearch;
                if (cblsHeaderProyecto != null)
                {
                    var proyectosUnicos = proyectosCompleto
                        .Where(p => p != null)
                        .Select(p => p.Proyecto)
                        .Distinct()
                        .OrderBy(nombre => nombre)
                        .Select(nombre => new { Nombre = nombre })
                        .ToList();

                    cblsHeaderProyecto.DataSource = proyectosUnicos;
                    cblsHeaderProyecto.DataBind();
                }

                // Poblar filtro de Línea de Gestión
                var cblsHeaderLinea = e.Row.FindControl("cblHeaderLineaGestion") as WebForms.CustomControls.TreeViewSearch;
                if (cblsHeaderLinea != null)
                {
                    var lineasUnicos = proyectosCompleto
                        .Where(p => p.LineaGestion != null)
                        .Select(p => p.LineaGestion)
                        .GroupBy(lg => lg.Id)
                        .Select(g => g.First())
                        .OrderBy(lg => lg.Nombre)
                        .ToList();

                    cblsHeaderLinea.DataSource = lineasUnicos;
                    cblsHeaderLinea.DataBind();
                }
            }
        }


        protected void dgvBdProyecto_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            try
            {
                dgvBdProyecto.PageIndex = e.NewPageIndex;
                CargarListaProyectos();
                CalcularSubtotal();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cambiar de página: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
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
                proyecto.Autorizado2025 = Convert.ToDecimal(txtMontoAutorizado2025.Text);

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
                CargarListaProyectos(null, true);
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
            txtMontoAutorizado2025.Text = string.Empty;
            ddlObra.SelectedIndex = 0;
            ddlLineaGestion.SelectedIndex = 0;
        }
        private void BindDropDownList()
        {
            ddlObra.DataSource = ObtenerObras();
            ddlObra.DataTextField = "Nombre";
            ddlObra.DataValueField = "Id";
            ddlObra.DataBind();

            ddlLineaGestion.DataSource = ObtenerLineaGestion();
            ddlLineaGestion.DataTextField = "Nombre";
            ddlLineaGestion.DataValueField = "Id";
            ddlLineaGestion.DataBind();
        }

        private DataTable ObtenerObras()
        {
            ObraNegocio barrioNegocio = new ObraNegocio();
            return barrioNegocio.listarddlProyecto();
        }
        //private DataTable ObtenerProyecto()
        //{
        //    BdProyectoNegocio barrioNegocio = new BdProyectoNegocio();
        //    return barrioNegocio.listarddl();
        //}
        private DataTable ObtenerLineaGestion()
        {
            LineaGestionNegocio barrioNegocio = new LineaGestionNegocio();
            return barrioNegocio.listarddl();
        }

        protected void BtnClearFilters_Click(object sender, EventArgs e)
        {
            txtBuscar.Text = string.Empty;

            WebForms.CustomControls.TreeViewSearch.ClearAllFiltersOnPage(this.Page);

            CargarListaProyectos();
        }



    }
}