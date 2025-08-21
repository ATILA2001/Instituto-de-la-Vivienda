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
    public partial class Redeterminaciones : System.Web.UI.Page
    {
        RedeterminacionNegocio negocio = new RedeterminacionNegocio();

        protected void OnAcceptChanges(object sender, EventArgs e)
        {
            CargarListaRedeterminacion();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Force complete reload on initial page load
                CargarListaRedeterminacion(null, true);
                BindDropDownList();
            }
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            // Configure validators if we're in editing mode
            if (ViewState["EditingRedeterminacionId"] != null)
            {
                // Disable the Autorizante validator since the field is hidden
                rfvAutorizante.Enabled = false;
            }
            else
            {
                // Enable validators in add mode
                rfvAutorizante.Enabled = true;
            }
        }

        protected void btnShowAddModal_Click(object sender, EventArgs e)
        {
            // Clear any existing data
            LimpiarFormulario();

            // Reset the modal title and button text to "Add" and show Autorizante field
            ScriptManager.RegisterStartupScript(this, this.GetType(), "ResetModalTitleAndShow", @"
        $(document).ready(function() {
            $('#modalAgregar .modal-title').text('Agregar Redeterminación');
            document.getElementById('" + btnAgregar.ClientID + @"').value = 'Agregar';
            
            // Show the Autorizante dropdown and its label
            $('#autorizanteContainer').show();
            
            // Show the modal
            $('#modalAgregar').modal('show');
        });", true);

            btnAgregar.Text = "Agregar";

            // Clear any editing state
            ViewState["EditingRedeterminacionId"] = null;
            ViewState["EditingAutorizanteId"] = null;
            ViewState["EditingCodigoAutorizante"] = null;
        }

        protected void btnLimpiar_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();
        }

        private void LimpiarFormulario()
        {
            txtExpediente.Text = string.Empty;
            txtSalto.Text = string.Empty;
            txtNro.Text = string.Empty;
            txtTipo.Text = string.Empty;
            txtPorcentaje.Text = string.Empty;
            txtObservacion.Text = string.Empty;

            if (ddlAutorizante.Items.Count > 0)
                ddlAutorizante.SelectedIndex = 0;

            if (ddlEtapa.Items.Count > 0)
                ddlEtapa.SelectedIndex = 0;
        }

        protected void BtnClearFilters_Click(object sender, EventArgs e)
        {
            txtBuscar.Text = string.Empty;
            // Limpiar filtros de cabecera
            ClearHeaderFilter("cblsHeaderObra");
            ClearHeaderFilter("cblsHeaderAutorizante");
            ClearHeaderFilter("cblsHeaderEstado");
            CargarListaRedeterminacion();
            ScriptManager.RegisterStartupScript(this, this.GetType(), "SetFiltersClearedFlag", "sessionStorage.setItem('filtersCleared', 'true');", true);
        }

        private void ClearHeaderFilter(string controlId)
        {
            if (dgvRedeterminacion.HeaderRow != null)
            {
                var control = dgvRedeterminacion.HeaderRow.FindControl(controlId) as WebForms.CustomControls.TreeViewSearch;
                if (control != null)
                {
                    control.ClearSelection();
                    string controlInstanceId = control.ID;
                    string sessionKey = $"TreeViewSearch_SelectedValues_{controlInstanceId}";
                    if (HttpContext.Current.Session[sessionKey] != null) HttpContext.Current.Session.Remove(sessionKey);
                    string contextKey = $"TreeViewSearch_{controlInstanceId}_ContextSelectedValues";
                    if (HttpContext.Current.Items.Contains(contextKey)) HttpContext.Current.Items.Remove(contextKey);
                }
            }
        }

        protected string FormatDaysWithColor(double days)
        {
            int dayCount = (int)Math.Floor(days);
            string color;

            if (dayCount < 7)
            {
                color = "green"; // Verde para menos de 7 días
            }
            else if (dayCount >= 7 && dayCount <= 14)
            {
                color = "orange"; // Amarillo para entre 7 y 14 días
            }
            else
            {
                color = "red"; // Rojo para más de 14 días
            }

            return $"<span style='color: {color}; font-weight: bold;'>{dayCount}</span>";
        }
        protected void BtnToggleMismatch_ServerChange(object sender, EventArgs e)
        {
            ShowOnlyMismatchedRecords = chkShowMismatchOnly.Checked;
            CargarListaRedeterminacion();
        }

        protected bool ShowOnlyMismatchedRecords
        {
            get { return ViewState["ShowOnlyMismatchedRecords"] as bool? ?? false; }
            set { ViewState["ShowOnlyMismatchedRecords"] = value; }
        }
        protected bool IsBuzonEtapaMismatch(string etapaNombre, string buzonSade)
        {
            if (string.IsNullOrEmpty(etapaNombre) || string.IsNullOrEmpty(buzonSade))
                return false;

            // Diccionario de correspondencias entre etapas y buzones
            var correspondencias = new Dictionary<string, List<string>>
    {
        {"RD-01/11-Subsanacion Empresa", new List<string>{"IVC-4010 DEPTO REDETERMINACIONES"}},
        {"RD-02/11-Análisis Tecnica", new List<string>{
            "IVC-3300 GO PLANEAMIENTO Y EVALUACIÓN",
            "IVC-3430 DEPTO OBRAS 1",
            "IVC-3400 GO INSPECCIION Y AUDITORIA DE OBRAS",
            "11000",
            "IVC-2600 GO PLANIFICACION Y CONTROL",
            "VLMOHAREM",
            "IVC-12400 GO LOGISTICA",
            "IVC-3000 DG OBRAS",
            "IVC-3420 DEPTO AUDITORIA 2",
            "IVC-9500 GO MODERNIZACION"
        }},
        {"RD-03/11-Análisis DGAyF", new List<string>{"IVC-4010 DEPTO REDETERMINACIONES"}},
        {"RD-04/11-Dgtal-Dictamen", new List<string>{
            "IVC-5210 DEPTO OBRAS PUBLICAS",
            "IVC-5220 DEPTO SUMINISTROS Y OBRAS MENORES",
            "IVC-5200 GO ASESORAMIENTO Y CONTROL DE LEGALIDAD OBRA PUBLICA Y SUMINISTROS"
        }},
        // Mantener las demás correspondencias...
    };

            // Verificar si el buzón corresponde a la etapa
            if (correspondencias.ContainsKey(etapaNombre))
            {
                bool coincide = correspondencias[etapaNombre].Any(b => buzonSade.Contains(b));
                return !coincide; // Retorna true si NO coincide (hay mismatch)
            }

            return false;
        }

        private void CargarListaRedeterminacion(string filtro = null, bool forzarRecargaCompleta = false)
        {
            try
            {
                List<Redeterminacion> redeterminacionesCompletas;

                if (forzarRecargaCompleta || Session["redeterminacionesCompletas"] == null)
                {
                    // Only load from database when forced or data doesn't exist in session
                    redeterminacionesCompletas = negocio.listar();
                    Session["redeterminacionesCompletas"] = redeterminacionesCompletas;
                }
                else
                {
                    // Use cached data from session
                    redeterminacionesCompletas = (List<Redeterminacion>)Session["redeterminacionesCompletas"];
                }

                if (redeterminacionesCompletas == null)
                {
                    lblMensaje.Text = "No se pudieron cargar los datos de las redeterminaciones.";
                    lblMensaje.CssClass = "alert alert-warning";
                    dgvRedeterminacion.DataSource = null;
                    dgvRedeterminacion.DataBind();
                    return;
                }

                IEnumerable<Redeterminacion> listaFiltrada = redeterminacionesCompletas;

                // Filtrar por mismatch si el toggle está activo
                if (ShowOnlyMismatchedRecords)
                {
                    listaFiltrada = listaFiltrada.Where(r =>
                        IsBuzonEtapaMismatch(r.Etapa?.Nombre, r.BuzonSade));
                }
                // Aplicar filtro de texto general (txtBuscar)
                string filtroGeneral = string.IsNullOrEmpty(filtro) ? txtBuscar.Text.Trim().ToUpper() : filtro.Trim().ToUpper();
                if (!string.IsNullOrEmpty(filtroGeneral))
                {
                    listaFiltrada = listaFiltrada.Where(r =>
                        (r.Autorizante?.Obra?.Descripcion?.ToUpper().Contains(filtroGeneral) ?? false) ||
                        (r.CodigoRedet?.ToUpper().Contains(filtroGeneral) ?? false) ||
                        (r.Autorizante?.CodigoAutorizante?.ToUpper().Contains(filtroGeneral) ?? false) ||
                        (r.Etapa?.Nombre?.ToUpper().Contains(filtroGeneral) ?? false) ||
                        (r.Expediente?.ToUpper().Contains(filtroGeneral) ?? false) ||
                        (r.Tipo?.ToUpper().Contains(filtroGeneral) ?? false) ||
                        (r.Observaciones?.ToUpper().Contains(filtroGeneral) ?? false) ||
                        (r.Empresa?.ToUpper().Contains(filtroGeneral) ?? false) ||
                        (r.Area?.ToUpper().Contains(filtroGeneral) ?? false)
                    );
                }

                // Aplicar filtros de cabecera
                if (dgvRedeterminacion.HeaderRow != null)
                {
                    var cblsHeaderObra = dgvRedeterminacion.HeaderRow.FindControl("cblsHeaderObra") as WebForms.CustomControls.TreeViewSearch;
                    var selectedObras = cblsHeaderObra?.SelectedValues;
                    if (selectedObras != null && selectedObras.Any())
                    {
                        listaFiltrada = listaFiltrada.Where(r => r.Autorizante?.Obra != null && selectedObras.Contains(r.Autorizante.Obra.Id.ToString()));
                    }

                    var cblsHeaderAutorizante = dgvRedeterminacion.HeaderRow.FindControl("cblsHeaderAutorizante") as WebForms.CustomControls.TreeViewSearch;
                    var selectedAutorizantes = cblsHeaderAutorizante?.SelectedValues;
                    if (selectedAutorizantes != null && selectedAutorizantes.Any())
                    {
                        // Asumiendo que el filtro de autorizante usa CodigoAutorizante como ValueField, ya que Id no está directamente en Autorizante en el modelo Redeterminacion.
                        // Si Autorizante.Id estuviera disponible y fuera el DataValueField, se usaría r.Autorizante.Id.ToString()
                        listaFiltrada = listaFiltrada.Where(r => r.Autorizante != null && selectedAutorizantes.Contains(r.Autorizante.CodigoAutorizante));
                    }

                    var cblsHeaderEstado = dgvRedeterminacion.HeaderRow.FindControl("cblsHeaderEstado") as WebForms.CustomControls.TreeViewSearch;
                    var selectedEstados = cblsHeaderEstado?.SelectedValues;
                    if (selectedEstados != null && selectedEstados.Any())
                    {
                        listaFiltrada = listaFiltrada.Where(r => r.Etapa != null && selectedEstados.Contains(r.Etapa.Id.ToString()));
                    }
                }

                List<Redeterminacion> resultadoFinal = listaFiltrada.ToList();
                Session["listaRedeterminacion"] = resultadoFinal; // Guardar lista filtrada para operaciones como edición/eliminación
                dgvRedeterminacion.DataSource = resultadoFinal;
                dgvRedeterminacion.DataBind();

                if (!resultadoFinal.Any())
                {
                    lblMensaje.Text = "No se encontraron redeterminaciones con los filtros aplicados.";
                }
                else
                {
                    lblMensaje.Text = "";
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cargar las redeterminaciones: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
                dgvRedeterminacion.DataSource = new List<Redeterminacion>();
                dgvRedeterminacion.DataBind();
            }

        }

        protected void dgvRedeterminacion_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                // Lógica existente para poblar el DropDownList de Etapas en cada fila
                DropDownList ddlEtapas = (DropDownList)e.Row.FindControl("ddlEtapas");
                if (ddlEtapas != null)
                {
                    DataTable estados = ObtenerTipos(); // Este método ya existe y obtiene los estados
                    ddlEtapas.DataSource = estados;
                    ddlEtapas.DataTextField = "Nombre";
                    ddlEtapas.DataValueField = "Id";
                    ddlEtapas.DataBind();

                    // Establecer el valor seleccionado para el DropDownList de la fila
                    Redeterminacion redetItem = e.Row.DataItem as Redeterminacion;
                    if (redetItem != null && redetItem.Etapa != null)
                    {
                        ListItem item = ddlEtapas.Items.FindByValue(redetItem.Etapa.Id.ToString());
                        if (item != null)
                        {
                            ddlEtapas.SelectedValue = redetItem.Etapa.Id.ToString();
                        }
                        if (!string.IsNullOrEmpty(redetItem.BuzonSade))
                        {
                            string etapaNombre = redetItem.Etapa.Nombre;
                            string buzonSade = redetItem.BuzonSade;

                        }
                    }
                }
            }

            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                // Lógica existente para poblar el DropDownList de Etapas en cada fila
                DropDownList ddlUsuario = (DropDownList)e.Row.FindControl("ddlUsuario");
                if (ddlUsuario != null)
                {
                    DataTable usuarios = ObtenerUsuarios(); // Este método ya existe y obtiene los estados
                    ddlUsuario.DataSource = usuarios;
                    ddlUsuario.DataTextField = "Nombre";
                    ddlUsuario.DataValueField = "Id";
                    ddlUsuario.DataBind();

                    // Añadir opción "Sin Usuario" si no existe
                    if (!ddlUsuario.Items.Cast<ListItem>().Any(x => x.Value == "-1"))
                    {
                        ddlUsuario.Items.Insert(0, new ListItem("Sin Usuario", "-1"));
                    }

                    // Establecer el valor seleccionado para el DropDownList de la fila
                    Redeterminacion redetItem = e.Row.DataItem as Redeterminacion;
                    if (redetItem != null)
                    {
                        if (redetItem.Usuario != null)
                        {
                            ListItem item = ddlUsuario.Items.FindByValue(redetItem.Usuario.Id.ToString());
                            if (item != null)
                            {
                                ddlUsuario.SelectedValue = redetItem.Usuario.Id.ToString();
                            }
                        }
                        else
                        {
                            // Si no hay usuario, seleccionar la opción "Sin Usuario"
                            ddlUsuario.SelectedValue = "-1";
                        }
                    }
                }
            }

            else if (e.Row.RowType == DataControlRowType.Header)
            {
                // Poblar los TreeViewSearch en la cabecera
                List<Redeterminacion> redeterminacionesCompletas = Session["redeterminacionesCompletas"] as List<Redeterminacion>;
                if (redeterminacionesCompletas == null || !redeterminacionesCompletas.Any()) return;

                // Poblar filtro de Obra
                var cblsHeaderObra = e.Row.FindControl("cblsHeaderObra") as WebForms.CustomControls.TreeViewSearch;
                if (cblsHeaderObra != null)
                {
                    var items = redeterminacionesCompletas
                        .Where(r => r.Autorizante?.Obra != null)
                        .Select(r => new { Id = r.Autorizante.Obra.Id, Nombre = r.Autorizante.Obra.Descripcion })
                        .Distinct()
                        .OrderBy(o => o.Nombre)
                        .ToList();
                    cblsHeaderObra.DataSource = items;
                    cblsHeaderObra.DataBind();
                }

                // Poblar filtro de Autorizante
                var cblsHeaderAutorizante = e.Row.FindControl("cblsHeaderAutorizante") as WebForms.CustomControls.TreeViewSearch;
                if (cblsHeaderAutorizante != null)
                {
                    var items = redeterminacionesCompletas
                        .Where(r => r.Autorizante != null)
                        // Usamos CodigoAutorizante como Id y Nombre para el filtro, asumiendo que es el identificador principal para el usuario.
                        // Si Autorizante tuviera un Id numérico y un Nombre descriptivo aparte del código, se usarían esos.
                        .Select(r => new { Id = r.Autorizante.CodigoAutorizante, Nombre = r.Autorizante.CodigoAutorizante })
                        .Distinct()
                        .OrderBy(a => a.Nombre)
                        .ToList();
                    cblsHeaderAutorizante.DataSource = items;
                    cblsHeaderAutorizante.DataBind();
                }

                // Poblar filtro de Estado (Etapa)
                var cblsHeaderEstado = e.Row.FindControl("cblsHeaderEstado") as WebForms.CustomControls.TreeViewSearch;
                if (cblsHeaderEstado != null)
                {
                    var items = redeterminacionesCompletas
                        .Where(r => r.Etapa != null)
                        .Select(r => new { Id = r.Etapa.Id, Nombre = r.Etapa.Nombre })
                        .Distinct()
                        .OrderBy(r => r.Nombre)
                        .ToList();
                    cblsHeaderEstado.DataSource = items;
                    cblsHeaderEstado.DataBind();
                }
            }
        }

        private DataTable ObtenerUsuarios()
        {
            UsuarioNegocio usuarios = new UsuarioNegocio();
            return usuarios.listarDdlRedet();
        }

        protected void dgvRedeterminacion_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                // Get the ID of the selected row
                int idRedeterminacion = Convert.ToInt32(dgvRedeterminacion.SelectedDataKey.Value);

                // Get the list of redeterminaciones from session
                List<Redeterminacion> listaRedeterminaciones = (List<Redeterminacion>)Session["listaRedeterminacion"];

                // Find the selected redeterminacion
                Redeterminacion redeterminacionSeleccionada = listaRedeterminaciones.FirstOrDefault(r => r.Id == idRedeterminacion);

                if (redeterminacionSeleccionada != null)
                {
                    // Set button text to "Actualizar"
                    btnAgregar.Text = "Actualizar";

                    // Load the redeterminacion data into the form fields
                    txtExpediente.Text = redeterminacionSeleccionada.Expediente;

                    if (redeterminacionSeleccionada.Salto.HasValue)
                        txtSalto.Text = redeterminacionSeleccionada.Salto.Value.ToString("yyyy-MM-dd");

                    if (redeterminacionSeleccionada.Nro.HasValue)
                        txtNro.Text = redeterminacionSeleccionada.Nro.Value.ToString();

                    txtTipo.Text = redeterminacionSeleccionada.Tipo;

                    if (redeterminacionSeleccionada.Porcentaje.HasValue)
                        txtPorcentaje.Text = redeterminacionSeleccionada.Porcentaje.Value.ToString();

                    txtObservacion.Text = redeterminacionSeleccionada.Observaciones;

                    // Select the corresponding values in the dropdowns
                    if (redeterminacionSeleccionada.Etapa != null)
                        SelectDropDownListByValue(ddlEtapa, redeterminacionSeleccionada.Etapa.Id.ToString());

                    // Store the IDs for update
                    ViewState["EditingRedeterminacionId"] = idRedeterminacion;

                    if (redeterminacionSeleccionada.Autorizante != null)
                    {
                        ViewState["EditingAutorizanteId"] = redeterminacionSeleccionada.Autorizante.Id;
                        ViewState["EditingCodigoAutorizante"] = redeterminacionSeleccionada.CodigoRedet;
                    }

                    // Update modal title and hide the Autorizante field
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "UpdateModalAndShow", @"
                $(document).ready(function() {
                    // Change title and button text
                    $('#modalAgregar .modal-title').text('Modificar Redeterminación');
                    document.getElementById('" + btnAgregar.ClientID + @"').value = 'Actualizar';
                    
                    // Hide the Autorizante dropdown and its label
                    $('#autorizanteContainer').hide();
                    
                    // Show the modal
                    $('#modalAgregar').modal('show');
                });", true);
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cargar los datos de la redeterminación: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        protected void dgvRedeterminacion_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            try
            {
                var id = Convert.ToInt32(dgvRedeterminacion.DataKeys[e.RowIndex].Value);
                if (negocio.eliminar(id))
                {
                    lblMensaje.Text = "Redeterminación eliminada correctamente.";
                    lblMensaje.CssClass = "alert alert-success";
                    CargarListaRedeterminacion(null, true);
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al eliminar la redeterminación: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        protected void dgvRedeterminacion_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            try
            {
                dgvRedeterminacion.PageIndex = e.NewPageIndex;
                CargarListaRedeterminacion();
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
                RedeterminacionNegocio redeterminacionNegocio = new RedeterminacionNegocio();
                Redeterminacion redeterminacion = new Redeterminacion();

                // Common data for both add and update operations
                redeterminacion.Expediente = txtExpediente.Text.Trim();
                redeterminacion.Salto = string.IsNullOrWhiteSpace(txtSalto.Text)
                    ? null
                    : (DateTime?)DateTime.Parse(txtSalto.Text);
                redeterminacion.Nro = string.IsNullOrWhiteSpace(txtNro.Text)
                    ? null
                    : (int?)int.Parse(txtNro.Text);
                redeterminacion.Tipo = txtTipo.Text.Trim();
                redeterminacion.Etapa = new EstadoRedet { Id = int.Parse(ddlEtapa.SelectedValue) };
                redeterminacion.Observaciones = txtObservacion.Text.Trim();
                redeterminacion.Porcentaje = decimal.Parse(txtPorcentaje.Text);

                // Check if we're editing an existing redeterminacion or adding a new one
                if (ViewState["EditingRedeterminacionId"] != null)
                {
                    // We're updating an existing redeterminacion
                    redeterminacion.Id = (int)ViewState["EditingRedeterminacionId"];

                    // Use the stored Autorizante info for the update
                    if (ViewState["EditingAutorizanteId"] != null && ViewState["EditingCodigoAutorizante"] != null)
                    {
                        redeterminacion.Autorizante = new Autorizante { Id = (int)ViewState["EditingAutorizanteId"] };
                        redeterminacion.CodigoRedet = ViewState["EditingCodigoAutorizante"].ToString();
                    }

                    if (redeterminacionNegocio.modificar(redeterminacion))
                    {
                        lblMensaje.Text = "Redeterminación modificada exitosamente!";
                        lblMensaje.CssClass = "alert alert-success";

                        // Clear the editing state
                        ViewState["EditingRedeterminacionId"] = null;
                        ViewState["EditingAutorizanteId"] = null;
                        ViewState["EditingCodigoAutorizante"] = null;
                    }
                    else
                    {
                        lblMensaje.Text = "Hubo un problema al modificar la redeterminación.";
                        lblMensaje.CssClass = "alert alert-danger";
                    }
                }
                else
                {
                    // We're adding a new redeterminacion
                    redeterminacion.Autorizante = new Autorizante { CodigoAutorizante = ddlAutorizante.SelectedItem.Text };
                    redeterminacion.CodigoRedet = ddlAutorizante.SelectedItem.Text;

                    if (redeterminacionNegocio.agregar(redeterminacion))
                    {
                        lblMensaje.Text = "Redeterminación agregada exitosamente!";
                        lblMensaje.CssClass = "alert alert-success";
                    }
                    else
                    {
                        lblMensaje.Text = "Hubo un problema al agregar la redeterminación.";
                        lblMensaje.CssClass = "alert alert-danger";
                    }
                }

                // Clear fields
                LimpiarFormulario();

                // Reset the modal title and button text
                ScriptManager.RegisterStartupScript(this, this.GetType(), "ResetModalTitle",
                    "$('#modalAgregar .modal-title').text('Agregar Redeterminación');", true);
                btnAgregar.Text = "Agregar";

                // Hide the modal
                ScriptManager.RegisterStartupScript(this, this.GetType(), "HideModal",
                    "$('#modalAgregar').modal('hide');", true);

                // MODIFIED: Force reload after database change
                CargarListaRedeterminacion(null, true);
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        private void SelectDropDownListByValue(DropDownList dropDown, string value)
        {
            if (dropDown != null && dropDown.Items.Count > 0)
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
        }

        private DataTable ObtenerTipos()
        {
            EstadoRedetNegocio tipoPagNegocio = new EstadoRedetNegocio();
            return tipoPagNegocio.listarddl();
        }

        private DataTable ObtenerAutorizantes()
        {
            AutorizanteNegocio autorizanteNegocio = new AutorizanteNegocio();
            return autorizanteNegocio.listarddl();
        }

        private void BindDropDownList()
        {
            try
            {
                // Cargar etapas
                DataTable tiposEtapas = ObtenerTipos();
                if (tiposEtapas != null && tiposEtapas.Rows.Count > 0)
                {
                    ddlEtapa.DataSource = tiposEtapas;
                    ddlEtapa.DataTextField = "Nombre";
                    ddlEtapa.DataValueField = "Id";
                    ddlEtapa.DataBind();

                    // Añadir opción predeterminada
                    ddlEtapa.Items.Insert(0, new ListItem("Seleccione una etapa", ""));
                }

                // Cargar autorizantes
                DataTable autorizantes = ObtenerAutorizantes();
                if (autorizantes != null && autorizantes.Rows.Count > 0)
                {
                    ddlAutorizante.DataSource = autorizantes;
                    ddlAutorizante.DataTextField = "Nombre";
                    ddlAutorizante.DataValueField = "Id";
                    ddlAutorizante.DataBind();

                    // Añadir opción predeterminada
                    ddlAutorizante.Items.Insert(0, new ListItem("Seleccione un autorizante", ""));
                }

            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cargar los datos iniciales: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }



        protected void txtExpediente_TextChanged(object sender, EventArgs e)
        {
            TextBox txtExpediente = (TextBox)sender;
            GridViewRow row = (GridViewRow)txtExpediente.NamingContainer;

            int idRedeterminacion = Convert.ToInt32(dgvRedeterminacion.DataKeys[row.RowIndex].Value);
            string nuevoExpediente = txtExpediente.Text;

            try
            {
                RedeterminacionNegocio negocio = new RedeterminacionNegocio();

                if (negocio.ActualizarExpediente(idRedeterminacion, nuevoExpediente))
                {

                    CargarListaRedeterminacion(null, true);

                    lblMensaje.Text = "Expediente actualizado correctamente.";
                    lblMensaje.CssClass = "alert alert-success";
                }
                else
                {
                    lblMensaje.Text = "Error al actualizar el expediente.";
                    lblMensaje.CssClass = "alert alert-danger";
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = "Error al actualizar el expediente: " + ex.Message;
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            string filtro = txtBuscar.Text.Trim();

            WebForms.CustomControls.TreeViewSearch.ClearAllFiltersOnPage(this.Page);

            CargarListaRedeterminacion(filtro);
        }

        protected void ddlEtapas_SelectedIndexChanged(object sender, EventArgs e)
        {
            DropDownList ddlEtapas = (DropDownList)sender;
            GridViewRow row = (GridViewRow)ddlEtapas.NamingContainer;

            List<Redeterminacion> listaRedeterminaciones = (List<Redeterminacion>)Session["listaRedeterminacion"];
            int id = int.Parse(dgvRedeterminacion.DataKeys[row.RowIndex].Value.ToString());
            Redeterminacion redeterminacion = listaRedeterminaciones.Find(r => r.Id == id);

            if (redeterminacion != null)
            {
                redeterminacion.Etapa.Id = int.Parse(ddlEtapas.SelectedValue);
                RedeterminacionNegocio negocio = new RedeterminacionNegocio();
                negocio.ActualizarEstado(redeterminacion);
                CargarListaRedeterminacion(null, true);

                lblMensaje.Text = "Etapa actualizada correctamente.";
                lblMensaje.CssClass = "alert alert-success";
            }
        }

        protected void ddlUsuario_SelectedIndexChanged(object sender, EventArgs e)
        {
            DropDownList ddlUsuario = (DropDownList)sender;
            GridViewRow row = (GridViewRow)ddlUsuario.NamingContainer;

            List<Redeterminacion> listaRedeterminaciones = (List<Redeterminacion>)Session["listaRedeterminacion"];
            int id = int.Parse(dgvRedeterminacion.DataKeys[row.RowIndex].Value.ToString());
            Redeterminacion redeterminacion = listaRedeterminaciones.Find(r => r.Id == id);

            if (redeterminacion != null)
            {
                if (ddlUsuario.SelectedValue == "-1")
                {
                    // Si se selecciona "Sin Usuario", asignar null al usuario
                    redeterminacion.Usuario = null;
                }
                else
                {
                    // Asegurarse que el objeto Usuario existe
                    if (redeterminacion.Usuario == null)
                    {
                        redeterminacion.Usuario = new Usuario();
                    }
                    redeterminacion.Usuario.Id = int.Parse(ddlUsuario.SelectedValue);
                }

                RedeterminacionNegocio negocio = new RedeterminacionNegocio();
                negocio.ActualizarUsuario(redeterminacion);
                CargarListaRedeterminacion(null, true);

                lblMensaje.Text = "Usuario actualizado correctamente.";
                lblMensaje.CssClass = "alert alert-success";
            }
        }

    }
}