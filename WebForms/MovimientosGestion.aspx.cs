using Dominio;
using Negocio;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebForms
{
    public partial class MovimientosGestion : System.Web.UI.Page
    {
        MovimientoNegocio negocio = new MovimientoNegocio();


        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                List<Movimiento> listaCompleta = negocio.listar(new List<string>(), null);
                Session["movimientosCompleto"] = listaCompleta;

                BindDropDownList();
                CargarListaMovimientos();
            }
        }
        public void OnAcceptChanges(object sender, EventArgs e)
        {
            CargarListaMovimientos();
        }

        protected void dgvMovimiento_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.Header)
            {
                List<Movimiento> movimientosCompleto = Session["movimientosCompleto"] as List<Movimiento>;

                if (movimientosCompleto == null)
                {
                    movimientosCompleto = negocio.listar(new List<string>(), null);
                    Session["movimientosCompleto"] = movimientosCompleto;
                }

                if (movimientosCompleto == null || !movimientosCompleto.Any())
                {
                    return; // No hay datos para poblar los filtros.
                }

                // Poblar filtro de Obra
                var cblsHeaderObra = e.Row.FindControl("cblsHeaderObra") as WebForms.CustomControls.TreeViewSearch;
                if (cblsHeaderObra != null)
                {
                    var obrasUnicas = movimientosCompleto
                        .Where(m => m.Obra != null && m.Obra.Id != 0)
                        .Select(m => new { Id = m.Obra.Id, Nombre = m.Obra.Descripcion })
                        .Distinct() 
                        .OrderBy(x => x.Nombre)
                        .ToList();
                    cblsHeaderObra.DataTextField = "Nombre";
                    cblsHeaderObra.DataValueField = "Id"; 
                    cblsHeaderObra.DataSource = obrasUnicas;
                    cblsHeaderObra.DataBind();
                }

                // Poblar filtro de Fecha
                var cblsHeaderFecha = e.Row.FindControl("cblsHeaderFecha") as WebForms.CustomControls.TreeViewSearch;
                if (cblsHeaderFecha != null)
                {
                    var fechasUnicas = movimientosCompleto
                        .Where(m => m.Fecha.HasValue)
                        .Select(m => m.Fecha.Value.Date)
                        .Distinct()
                        .OrderByDescending(d => d) // Ordenar por fecha (primer día del mes), los más recientes primero.
                        .Select(d => new
                        {
                            Nombre = d.ToString("MMMM yyyy", new CultureInfo("es-ES")),
                            Valor = d.ToString("yyyy-MM-dd")
                        })
                        .ToList();
                    cblsHeaderFecha.DataTextField = "Nombre";    // Lo que el usuario ve.
                    cblsHeaderFecha.DataValueField = "Valor";   // Lo que se usa para filtrar.
                    cblsHeaderFecha.DataSource = fechasUnicas;
                    cblsHeaderFecha.DataBind();
                }
            }
        }


        protected void Page_PreRender(object sender, EventArgs e)
        {
            // Configure validators if we're in editing mode
            if (ViewState["EditingMovimientoId"] != null)
            {
                // Disable the Obra validator since the field is hidden
                rfvObra.Enabled = false;
            }
            else
            {
                // Enable validators in add mode
                rfvObra.Enabled = true;
            }
        }

        protected void btnShowAddModal_Click(object sender, EventArgs e)
        {
            // Clear any existing data
            LimpiarFormulario();

            // Reset the modal title and button text to "Add" and show Obra field
            ScriptManager.RegisterStartupScript(this, this.GetType(), "ResetModalTitleAndShow", @"
                $(document).ready(function() {
                    $('#modalAgregar .modal-title').text('Agregar Movimiento');
                    document.getElementById('" + btnAgregar.ClientID + @"').value = 'Agregar';
                    
                    // Show the Obra dropdown and its container
                    $('#obraContainer').show();
                    
                    // Show the modal
                    $('#modalAgregar').modal('show');
                });", true);

            btnAgregar.Text = "Agregar";

            // Clear any editing state
            ViewState["EditingMovimientoId"] = null;
            ViewState["EditingObraId"] = null;
        }

        protected void btnLimpiar_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();
        }

        private void LimpiarFormulario()
        {
            txtMovimiento.Text = string.Empty;
            txtFecha.Text = string.Empty;
            if (ddlObra.Items.Count > 0)
                ddlObra.SelectedIndex = 0;
        }

        private void CargarListaMovimientos(string filtro = null)
        {
            try
            {
                List<Movimiento> listaCompleta = negocio.listar(new List<string>(), null);
                
                Session["movimientosCompleto"] = listaCompleta;

                IEnumerable<Movimiento> listaFiltrada = listaCompleta;

                // Obtener valores de los filtros de cabecera
                List<string> selectedHeaderObras = new List<string>();
                List<string> selectedHeaderFechas = new List<string>();


                if (dgvMovimiento.HeaderRow != null)
                {
                    var cblsHeaderObraControl = dgvMovimiento.HeaderRow.FindControl("cblsHeaderObra") as WebForms.CustomControls.TreeViewSearch;
                    if (cblsHeaderObraControl != null) selectedHeaderObras = cblsHeaderObraControl.SelectedValues;

                    var cblsHeaderFechaControl = dgvMovimiento.HeaderRow.FindControl("cblsHeaderFecha") as WebForms.CustomControls.TreeViewSearch;
                    if (cblsHeaderFechaControl != null) selectedHeaderFechas = cblsHeaderFechaControl.SelectedValues;
                }


                // Aplicar filtro de texto general
                string filtroTextoGeneral = string.IsNullOrEmpty(filtro) ? txtBuscar.Text.Trim().ToUpper() : filtro.Trim().ToUpper();

                if (!string.IsNullOrEmpty(filtroTextoGeneral))
                {
                    listaFiltrada = listaFiltrada.Where(m =>
                        (m.Obra?.Descripcion?.ToUpper().Contains(filtroTextoGeneral) ?? false) ||
                        (m.Proyecto?.ToUpper().Contains(filtroTextoGeneral) ?? false) ||
                        (m.SubProyecto?.ToUpper().Contains(filtroTextoGeneral) ?? false) ||
                        (m.Linea?.ToUpper().Contains(filtroTextoGeneral) ?? false) ||
                        (m.Monto.ToString().Contains(filtroTextoGeneral)) ||
                        (m.AutorizadoNuevo?.ToString().Contains(filtroTextoGeneral) ?? false)
                    );
                }

                // Aplicar filtros de cabecera
                if (selectedHeaderObras.Any())
                    listaFiltrada = listaFiltrada.Where(m => m.Obra != null && selectedHeaderObras.Contains(m.Obra.Id.ToString()));

                if (selectedHeaderFechas.Any())
                {
                    var fechasSeleccionadas = selectedHeaderFechas
                        .Select(s => DateTime.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt) ? (DateTime?)dt : null)
                        .Where(d => d.HasValue)
                        .Select(d => d.Value.Date)
                        .ToList();
                    if (fechasSeleccionadas.Any())
                    {
                        listaFiltrada = listaFiltrada.Where(l => l.Fecha.HasValue && fechasSeleccionadas.Contains(l.Fecha.Value.Date));
                    }
                }

                List<Movimiento> resultadoFinal = listaFiltrada.ToList();
                Session["listaMovimiento"] = resultadoFinal;
                dgvMovimiento.DataSource = resultadoFinal;
                dgvMovimiento.DataBind();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cargar los Movimientos: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }



        protected void dgvMovimiento_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                // Get the ID of the selected row
                int idMovimiento = Convert.ToInt32(dgvMovimiento.SelectedDataKey.Value);

                // Get the list of movimientos from session
                List<Movimiento> listaMovimientos = (List<Movimiento>)Session["listaMovimiento"];

                // Find the selected movimiento
                Movimiento movimientoSeleccionado = listaMovimientos.FirstOrDefault(m => m.Id == idMovimiento);

                if (movimientoSeleccionado != null)
                {
                    // Set button text to "Actualizar"
                    btnAgregar.Text = "Actualizar";

                    // Load the movimiento data into the form fields
                    txtMovimiento.Text = movimientoSeleccionado.Monto.ToString("0.00");

                    if (movimientoSeleccionado.Fecha.HasValue)
                        txtFecha.Text = movimientoSeleccionado.Fecha.Value.ToString("yyyy-MM-dd");

                    // Store the IDs for update
                    ViewState["EditingMovimientoId"] = idMovimiento;
                    if (movimientoSeleccionado.Obra != null)
                        ViewState["EditingObraId"] = movimientoSeleccionado.Obra.Id;

                    // Update modal title and hide the Obra field
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "UpdateModalAndShow", @"
                        $(document).ready(function() {
                            // Change title and button text
                            $('#modalAgregar .modal-title').text('Modificar Movimiento');
                            document.getElementById('" + btnAgregar.ClientID + @"').value = 'Actualizar';
                            
                            // Hide the Obra dropdown and its container
                            $('#obraContainer').hide();
                            
                            // Show the modal
                            $('#modalAgregar').modal('show');
                        });", true);
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cargar los datos del movimiento: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        protected void dgvMovimiento_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            try
            {
                var id = Convert.ToInt32(dgvMovimiento.DataKeys[e.RowIndex].Value);
                if (negocio.eliminar(id))
                {
                    lblMensaje.Text = "Movimiento eliminado correctamente.";
                    lblMensaje.CssClass = "alert alert-success";
                    CargarListaMovimientos();
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al eliminar el movimiento: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        protected void dgvMovimiento_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            try
            {
                dgvMovimiento.PageIndex = e.NewPageIndex;
                CargarListaMovimientos();
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
                Movimiento movimiento = new Movimiento();
                movimiento.Monto = decimal.Parse(txtMovimiento.Text.Replace('.', ','));
                movimiento.Fecha = string.IsNullOrWhiteSpace(txtFecha.Text)
                    ? null
                    : (DateTime?)DateTime.Parse(txtFecha.Text);

                // Check if we're editing an existing movimiento or adding a new one
                if (ViewState["EditingMovimientoId"] != null)
                {
                    // We're updating an existing movimiento
                    movimiento.Id = (int)ViewState["EditingMovimientoId"];

                    // Use the stored Obra ID
                    if (ViewState["EditingObraId"] != null)
                    {
                        movimiento.Obra = new Obra { Id = (int)ViewState["EditingObraId"] };
                    }

                    if (negocio.modificar(movimiento))
                    {
                        lblMensaje.Text = "Movimiento modificado exitosamente!";
                        lblMensaje.CssClass = "alert alert-success";

                        // Clear the editing state
                        ViewState["EditingMovimientoId"] = null;
                        ViewState["EditingObraId"] = null;
                    }
                    else
                    {
                        lblMensaje.Text = "Hubo un problema al modificar el movimiento.";
                        lblMensaje.CssClass = "alert alert-danger";
                    }
                }
                else
                {
                    // We're adding a new movimiento
                    movimiento.Obra = new Obra { Id = int.Parse(ddlObra.SelectedValue) };

                    if (negocio.agregar(movimiento))
                    {
                        lblMensaje.Text = "Movimiento agregado exitosamente!";
                        lblMensaje.CssClass = "alert alert-success";
                    }
                    else
                    {
                        lblMensaje.Text = "Hubo un problema al agregar el movimiento.";
                        lblMensaje.CssClass = "alert alert-danger";
                    }
                }

                // Clear fields
                LimpiarFormulario();

                // Reset the modal title and button text
                ScriptManager.RegisterStartupScript(this, this.GetType(), "ResetModalTitle",
                    "$('#modalAgregar .modal-title').text('Agregar Movimiento');", true);
                btnAgregar.Text = "Agregar";

                // Hide the modal
                ScriptManager.RegisterStartupScript(this, this.GetType(), "HideModal",
                    "$('#modalAgregar').modal('hide');", true);

                // Refresh the movimientos list
                CargarListaMovimientos();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        private void BindDropDownList()
        {
            var meses = Enumerable.Range(0, 36) // 36 meses entre 2024 y 2026
            .Select(i => new DateTime(2024, 1, 1).AddMonths(i))
            .Select(fecha => new
            {
                Texto = fecha.ToString("MMMM yyyy", new System.Globalization.CultureInfo("es-ES")), // Texto: "Enero 2024"
                Valor = fecha.ToString("yyyy-MM-dd")
            });

            ddlObra.DataSource = ObtenerObras();
            ddlObra.DataTextField = "Nombre";
            ddlObra.DataValueField = "Id";
            ddlObra.DataBind();

            // Añadir opción predeterminada
            ddlObra.Items.Insert(0, new ListItem("Seleccione una obra", ""));
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

        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            string filtro = txtBuscar.Text.Trim();
            CargarListaMovimientos(filtro);
        }

        private DataTable ObtenerObras()
        {
            ObraNegocio barrioNegocio = new ObraNegocio();
            return barrioNegocio.listarddl();
        }

        protected void BtnClearFilters_Click(object sender, EventArgs e)
        {
            // Limpiar filtros de cabecera
            if (dgvMovimiento.HeaderRow != null)
            {
                ClearFilter("cblsHeaderObra");
                ClearFilter("cblsHeaderFecha");
            }

            CargarListaMovimientos();
            ScriptManager.RegisterStartupScript(this, this.GetType(), "SetFiltersClearedFlag", "sessionStorage.setItem('filtersCleared', 'true');", true);
        }

        private void ClearFilter(string controlId)
        {
            if (dgvMovimiento.HeaderRow != null)
            {
                var control = dgvMovimiento.HeaderRow.FindControl(controlId) as WebForms.CustomControls.TreeViewSearch;
                if (control != null)
                {
                    control.ClearSelection();

                    string controlInstanceId = control.ID;

                    string sessionKey = $"TreeViewSearch_SelectedValues_{controlInstanceId}";
                    if (HttpContext.Current.Session[sessionKey] != null)
                    {
                        HttpContext.Current.Session.Remove(sessionKey);
                    }

                    string contextKey = $"TreeViewSearch_{controlInstanceId}_ContextSelectedValues";
                    if (HttpContext.Current.Items.Contains(contextKey))
                    {
                        HttpContext.Current.Items.Remove(contextKey);
                    }
                }
            }
        }
    }
}