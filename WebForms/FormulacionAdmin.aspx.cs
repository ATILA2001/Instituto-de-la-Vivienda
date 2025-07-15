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
    public partial class FormulacionAdmin : System.Web.UI.Page
    {
        private FormulacionNegocio negocio = new FormulacionNegocio();
        protected void btnExportarExcel_Click(object sender, EventArgs e)
        {
            try
            {
                // Obtener datos completos (sin paginación)
                List<Formulacion> formulaciones;

                if (Session["formulacionesCompletas"] != null)
                {
                    formulaciones = (List<Formulacion>)Session["formulacionesCompletas"];
                }
                else
                {
                    FormulacionNegocio negocio = new FormulacionNegocio();
                    formulaciones = negocio.listar();
                    Session["formulacionesCompletas"] = formulaciones;
                }

                if (formulaciones.Any())
                {
                    // Definir mapeo de columnas
                    var mapeoColumnas = new Dictionary<string, string>
                    {
                        { "Área", "Obra.Area.Nombre" },
                        { "Area", "Obra.Area.Nombre" }, // Versión sin acento
                        { "Empresa", "Obra.Empresa.Nombre" },
                        { "Contrata", "Obra.Contrata.Nombre" },
                        { "Barrio", "Obra.Barrio.Nombre" },
                        { "Nombre de Obra", "Obra.Descripcion" },
                        { "Linea de Gestión", "Obra.LineaGestion.Nombre" },
                        { "Línea de Gestión", "Obra.LineaGestion.Nombre" }, // Versión con acento
                        { "Proyecto", "Obra.Proyecto.Proyecto" },
                        { "Plurianual (2026,2027,2028)", "Plurianual" },
                        { "Monto 2026", "Monto_26" },
                        { "Monto 2027", "Monto_27" },
                        { "Monto 2028", "Monto_28" },
                        { "Mes Base", "MesBase" },
                        { "PPI (Presupuesto)", "Ppi" },
                        { "Unidad de Medida", "UnidadMedida.Nombre" },
                        { "Valor de Medida", "ValorMedida" },
                        { "Techos 2026", "Techos2026" },
                        { "Observaciones", "Observacion" },
                        { "Prioridad", "Prioridad.Nombre" }
                    };

                    // Exportar a Excel
                    ExcelHelper.ExportarDatosGenericos(dgvFormulacion, formulaciones, mapeoColumnas, "Formulaciones");
                }
                else
                {
                    lblMensaje.Text = "No hay datos para exportar";
                    lblMensaje.CssClass = "alert alert-warning";
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = "Error al exportar: " + ex.Message;
                lblMensaje.CssClass = "alert alert-danger";
            }
        }
        protected void OnAcceptChanges(object sender, EventArgs e)
        {
            CargarListaFormulaciones();
        }
        protected void dgvFormulacion_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            try
            {
                dgvFormulacion.PageIndex = e.NewPageIndex;
                CargarListaFormulaciones();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cambiar de página: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }
        protected void Page_PreRender(object sender, EventArgs e)
        {
            // Configurar validadores según si estamos en modo edición
            if (ViewState["EditingFormulacionId"] != null)
            {
                // Desactivar el validador de obra en modo edición ya que el campo está oculto
                rfvObra.Enabled = false;
            }
            else
            {
                // Activar validadores en modo agregar
                rfvObra.Enabled = true;
            }
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                List<Formulacion> formulacionesCompletas = negocio.listar();
                Session["formulacionesCompletas"] = formulacionesCompletas;

                BindDropDownList(); 
                CargarListaFormulaciones(); 
            }
        }

        protected void btnShowAddModal_Click(object sender, EventArgs e)
        {
            // Limpiar datos existentes
            ClearFormFields();

            // Restablecer el título del modal y el texto del botón a "Agregar" y mostrar el campo de obra
            ScriptManager.RegisterStartupScript(this, this.GetType(), "ResetModalTitleAndShow", @"
                $(document).ready(function() {
                    $('#modalAgregar .modal-title').text('Agregar Formulación');
                    document.getElementById('" + btnAgregar.ClientID + @"').value = 'Agregar';
                    
                    // Mostrar el campo de obra
                    $('.col-12:first').show();
                    
                    $('#modalAgregar').modal('show');
                });", true);

            btnAgregar.Text = "Agregar";

            // Limpiar cualquier estado de edición
            ViewState["EditingFormulacionId"] = null;
            ViewState["EditingObraId"] = null;
        }

        private DataTable ObtenerObras()
        {
            ObraNegocio obraNegocio = new ObraNegocio();
            return obraNegocio.listarddlFormulacion();
        }

        private DataTable ObtenerUnidadesMedida()
        {
            UnidadMedidaNegocio unidadMedidaNegocio = new UnidadMedidaNegocio();
            return unidadMedidaNegocio.listarddl();
        }

        private void CargarListaFormulaciones(string filtro = null, bool forzarRecargaCompleta = false)
        {
            try
            {
                // 1. Get complete list from session or database as needed
                List<Formulacion> formulacionesCompletas;

                if (forzarRecargaCompleta || Session["formulacionesCompletas"] == null)
                {
                    // Only load from database when forced or data doesn't exist
                    formulacionesCompletas = negocio.listar();
                    Session["formulacionesCompletas"] = formulacionesCompletas;
                }
                else
                {
                    // Use cached data from session
                    formulacionesCompletas = (List<Formulacion>)Session["formulacionesCompletas"];
                }

                IEnumerable<Formulacion> listaFiltrada = formulacionesCompletas;

                // 2. Apply general text filter (txtBuscar)
                string filtroGeneral = string.IsNullOrEmpty(filtro) ? txtBuscar.Text.Trim().ToUpper() : filtro.Trim().ToUpper();
                if (!string.IsNullOrEmpty(filtroGeneral))
                {
                    listaFiltrada = listaFiltrada.Where(f =>
                        (f.Obra?.Area?.Nombre?.ToUpper().Contains(filtroGeneral) ?? false) ||
                        (f.Obra?.Empresa?.Nombre?.ToUpper().Contains(filtroGeneral) ?? false) ||
                        (f.Obra?.Contrata?.Nombre?.ToUpper().Contains(filtroGeneral) ?? false) ||
                        (f.Obra?.Numero?.ToString().Contains(filtroGeneral) ?? false) ||
                        (f.Obra?.Año?.ToString().Contains(filtroGeneral) ?? false) ||
                        (f.Obra?.Barrio?.Nombre?.ToUpper().Contains(filtroGeneral) ?? false) ||
                        (f.Obra?.Descripcion?.ToUpper().Contains(filtroGeneral) ?? false) ||
                        (f.Obra?.LineaGestion?.Nombre?.ToUpper().Contains(filtroGeneral) ?? false) ||
                        (f.Obra?.Proyecto?.Proyecto?.ToUpper().Contains(filtroGeneral) ?? false) ||
                        (f.Monto_26.ToString(CultureInfo.InvariantCulture).Contains(filtroGeneral)) ||
                        (f.Prioridad?.Nombre?.ToUpper().Contains(filtroGeneral) ?? false) ||
                        (f.Observacion?.ToUpper().Contains(filtroGeneral) ?? false)
                    );
                }

                // 3. Get and apply header filters
                if (dgvFormulacion.HeaderRow != null)
                {
                    var cblsHeaderArea = dgvFormulacion.HeaderRow.FindControl("cblsHeaderArea") as WebForms.CustomControls.TreeViewSearch;
                    var filtroHeaderArea = cblsHeaderArea?.SelectedValues;
                    if (filtroHeaderArea != null && filtroHeaderArea.Any())
                    {
                        listaFiltrada = listaFiltrada.Where(f => f.Obra?.Area != null && filtroHeaderArea.Contains(f.Obra.Area.Id.ToString()));
                    }

                    var cblsHeaderLineaGestion = dgvFormulacion.HeaderRow.FindControl("cblsHeaderLineaGestion") as WebForms.CustomControls.TreeViewSearch;
                    var filtroHeaderLineaGestion = cblsHeaderLineaGestion?.SelectedValues;
                    if (filtroHeaderLineaGestion != null && filtroHeaderLineaGestion.Any())
                    {
                        listaFiltrada = listaFiltrada.Where(f => f.Obra?.LineaGestion != null && filtroHeaderLineaGestion.Contains(f.Obra.LineaGestion.Id.ToString()));
                    }

                    var cblsHeaderProyecto = dgvFormulacion.HeaderRow.FindControl("cblsHeaderProyecto") as WebForms.CustomControls.TreeViewSearch;
                    var filtroHeaderProyecto = cblsHeaderProyecto?.SelectedValues;
                    if (filtroHeaderProyecto != null && filtroHeaderProyecto.Any())
                    {
                        listaFiltrada = listaFiltrada.Where(f => f.Obra?.Proyecto != null && filtroHeaderProyecto.Contains(f.Obra.Proyecto.Id.ToString()));
                    }

                    var cblsHeaderMonto2026 = dgvFormulacion.HeaderRow.FindControl("cblsHeaderMonto2026") as WebForms.CustomControls.TreeViewSearch;
                    var filtroHeaderMonto2026 = cblsHeaderMonto2026?.SelectedValues;
                    if (filtroHeaderMonto2026 != null && filtroHeaderMonto2026.Any())
                    {
                        var montosDecimal = filtroHeaderMonto2026.Select(m => decimal.Parse(m, CultureInfo.InvariantCulture)).ToList();
                        listaFiltrada = listaFiltrada.Where(f => montosDecimal.Contains(f.Monto_26));
                    }

                    var cblsHeaderPrioridad = dgvFormulacion.HeaderRow.FindControl("cblsHeaderPrioridad") as WebForms.CustomControls.TreeViewSearch;
                    var filtroHeaderPrioridad = cblsHeaderPrioridad?.SelectedValues;
                    if (filtroHeaderPrioridad != null && filtroHeaderPrioridad.Any())
                    {
                        listaFiltrada = listaFiltrada.Where(f => f.Prioridad != null && filtroHeaderPrioridad.Contains(f.Prioridad.Id.ToString()));
                    }
                }

                List<Formulacion> resultadoFinal = listaFiltrada.ToList();
                Session["listaFormulacionAdmin"] = resultadoFinal; 
                dgvFormulacion.DataSource = resultadoFinal;
                dgvFormulacion.DataBind();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cargar las formulaciones: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }

        }

        protected void dgvFormulacion_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                // Obtener el ID de la fila seleccionada
                int idFormulacion = Convert.ToInt32(dgvFormulacion.SelectedDataKey.Value);

                // Obtener la lista de formulaciones de la sesión
                List<Formulacion> listaFormulaciones = (List<Formulacion>)Session["listaFormulacionAdmin"];

                // Encontrar la formulación seleccionada
                Formulacion formulacionSeleccionada = listaFormulaciones.FirstOrDefault(f => f.Id == idFormulacion);

                if (formulacionSeleccionada != null)
                {
                    // Establecer el texto del botón a "Actualizar"
                    btnAgregar.Text = "Actualizar";

                    // Cargar los datos de la formulación en los campos del formulario
                    if (formulacionSeleccionada.Obra != null)
                    {
                        SelectDropDownListByValue(ddlObra, formulacionSeleccionada.Obra.Id.ToString());
                        // Guardar el ID de la obra en el ViewState para usarlo en la actualización
                        ViewState["EditingObraId"] = formulacionSeleccionada.Obra.Id;
                    }

                    txtMonto26.Text = formulacionSeleccionada.Monto_26.ToString();
                    txtMonto27.Text = formulacionSeleccionada.Monto_27.ToString();
                    txtMonto28.Text = formulacionSeleccionada.Monto_28.ToString();
                    txtPPI.Text = formulacionSeleccionada.Ppi.ToString();
                    txtTechos.Text = formulacionSeleccionada.Techos2026.ToString();
                    txtMesBase.Text = formulacionSeleccionada.MesBase?.ToString("yyyy-MM-dd") ?? string.Empty;

                    if (formulacionSeleccionada.UnidadMedida != null)
                    {
                        SelectDropDownListByValue(ddlUnidadMedida, formulacionSeleccionada.UnidadMedida.Id.ToString());
                    }
                    txtValorMedida.Text = formulacionSeleccionada.ValorMedida.ToString();
                    txtObservaciones.Text = formulacionSeleccionada.Observacion;
                    if (formulacionSeleccionada.Prioridad != null)
                    {
                        SelectDropDownListByValue(ddlPrioridades, formulacionSeleccionada.Prioridad.Id.ToString());
                    }
                    else
                    {
                        ddlPrioridades.SelectedIndex = 0; // Seleccionar el primer elemento si no hay prioridad
                    }

                    // Almacenar el ID de la formulación que se está editando en ViewState
                    ViewState["EditingFormulacionId"] = idFormulacion;

                    // Actualizar el título del modal
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "UpdateModalAndShow", @"
                        $(document).ready(function() {
                            // Cambiar título y texto del botón
                            $('#modalAgregar .modal-title').text('Modificar Formulación');
                            document.getElementById('" + btnAgregar.ClientID + @"').value = 'Actualizar';
                            
                            // Ocultar el campo de obra (el primer div col-12)
                            $('.col-12:first').hide();
                            
                            // Mostrar el modal
                            $('#modalAgregar').modal('show');
                        });", true);
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cargar los datos de la formulación: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        private void SelectDropDownListByValue(DropDownList dropDown, string value)
        {
            // Limpiar cualquier selección actual
            dropDown.ClearSelection();

            // Intentar encontrar y seleccionar el elemento
            ListItem item = dropDown.Items.FindByValue(value);
            if (item != null)
            {
                item.Selected = true;
            }
        }

        protected void dgvFormulacion_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            try
            {
                var id = Convert.ToInt32(dgvFormulacion.DataKeys[e.RowIndex].Value);
                if (negocio.eliminar(id))
                {
                    lblMensaje.Text = "Formulación eliminada correctamente.";
                    lblMensaje.CssClass = "alert alert-success";

                    CargarListaFormulaciones(null, true); // Force reload after database change

                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al eliminar la formulación: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        protected void btnAgregar_Click(object sender, EventArgs e)
        {
            // Si la página es válida (todos los validadores pasaron), proceder con la creación del nuevo objeto
            if (Page.IsValid)
            {
                FormulacionNegocio negocio = new FormulacionNegocio();
                Formulacion formulacion = new Formulacion();

                try
                {
                    // Verificar que la unidad de medida esté seleccionada
                    if (string.IsNullOrEmpty(ddlUnidadMedida.SelectedValue) || ddlUnidadMedida.SelectedValue == "")
                    {
                        lblMensaje.Text = "Error: Debe seleccionar una unidad de medida.";
                        lblMensaje.CssClass = "alert alert-danger";
                        ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowModalWithError",
                            "$('#modalAgregar').modal('show');", true);
                        return;
                    }

                    // Asignar valores comunes a la formulación
                    if (ViewState["EditingFormulacionId"] != null) // Modo edición
                    {
                        formulacion.Id = (int)ViewState["EditingFormulacionId"];

                        // Verificar que exista el ID de obra para edición
                        if (ViewState["EditingObraId"] == null)
                        {
                            lblMensaje.Text = "Error: No se pudo determinar la obra a editar.";
                            lblMensaje.CssClass = "alert alert-danger";
                            ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowModalWithError",
                                "$('#modalAgregar').modal('show');", true);
                            return;
                        }

                        formulacion.Obra = new Obra { Id = (int)ViewState["EditingObraId"] };
                    }
                    else // Modo agregar
                    {
                        // Verificar que se haya seleccionado una obra
                        if (string.IsNullOrEmpty(ddlObra.SelectedValue))
                        {
                            lblMensaje.Text = "Error: Debe seleccionar una obra.";
                            lblMensaje.CssClass = "alert alert-danger";
                            ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowModalWithError",
                                "$('#modalAgregar').modal('show');", true);
                            return;
                        }

                        formulacion.Obra = new Obra { Id = int.Parse(ddlObra.SelectedValue) };
                    }

                    // Asignar el resto de valores comunes
                    formulacion.Monto_26 = decimal.Parse(txtMonto26.Text.Trim());
                    formulacion.Monto_27 = decimal.Parse(txtMonto27.Text.Trim());
                    formulacion.Monto_28 = decimal.Parse(txtMonto28.Text.Trim());
                    if (!string.IsNullOrWhiteSpace(txtPPI.Text.Trim()))
                    {
                        formulacion.Ppi = int.Parse(txtPPI.Text.Trim());
                    }
                    else
                    {
                        formulacion.Ppi = null; // Default value or appropriate handling
                    }
                    if (!string.IsNullOrWhiteSpace(txtTechos.Text.Trim()))
                    {
                        formulacion.Techos2026 = decimal.Parse(txtTechos.Text.Trim());
                    }
                    else
                    {
                        formulacion.Techos2026 = null; // Default value or appropriate handling
                    }

                    // Manejar valores que pueden ser nulos
                    formulacion.MesBase = !string.IsNullOrWhiteSpace(txtMesBase.Text.Trim())
                        ? DateTime.Parse(txtMesBase.Text.Trim())
                        : (DateTime?)null;

                    formulacion.Observacion = !string.IsNullOrWhiteSpace(txtObservaciones.Text.Trim())
                        ? txtObservaciones.Text.Trim()
                        : null;

                    // Asignar prioridad si se seleccionó
                    if (!string.IsNullOrEmpty(ddlPrioridades.SelectedValue) && ddlPrioridades.SelectedValue != "")
                    {
                        formulacion.Prioridad = new Prioridad { Id = int.Parse(ddlPrioridades.SelectedValue) };
                    }

                    // Asignar unidad de medida
                    formulacion.UnidadMedida = new UnidadMedida { Id = int.Parse(ddlUnidadMedida.SelectedValue) };
                    if (!string.IsNullOrEmpty(txtValorMedida.Text))
                        formulacion.ValorMedida = decimal.Parse(txtValorMedida.Text.Trim());
                    else
                        formulacion.ValorMedida = 0;
                    // Ejecutar operación según modo
                    if (ViewState["EditingFormulacionId"] != null)
                    {
                        negocio.modificar(formulacion);
                        lblMensaje.Text = "Formulación modificada exitosamente!";
                    }
                    else
                    {
                        negocio.agregar(formulacion);
                        lblMensaje.Text = "Formulación agregada exitosamente!";
                    }

                    lblMensaje.CssClass = "alert alert-success";

                    // Limpiar estados
                    ViewState["EditingFormulacionId"] = null;
                    ViewState["EditingObraId"] = null;
                    ClearFormFields();

                    // Restablecer UI
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "ResetModalAndUI", @"
            $('#modalAgregar .modal-title').text('Agregar Formulación');
            document.getElementById('" + btnAgregar.ClientID + @"').value = 'Agregar';
            $('.col-12:first').show();
            $('#modalAgregar').modal('hide');
        ", true);

                    btnAgregar.Text = "Agregar";

                    // MODIFIED: Use consistent pattern for loading data
                    CargarListaFormulaciones(null, true); // Force reload after database change
                }
                catch (Exception ex)
                {
                    lblMensaje.Text = $"Error: {ex.Message}";
                    lblMensaje.CssClass = "alert alert-danger";
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "KeepModalOpen",
                        "$('#modalAgregar').modal('show');", true);
                }
            }
        }

        private void ClearFormFields()
        {
            ddlObra.SelectedIndex = 0;
            txtMonto26.Text = string.Empty;
            txtMonto27.Text = string.Empty;
            txtMonto28.Text = string.Empty;
            txtPPI.Text = string.Empty;
            ddlUnidadMedida.SelectedIndex = 0;
            txtValorMedida.Text = string.Empty;
            txtTechos.Text = string.Empty;
            txtMesBase.Text = string.Empty;
            txtObservaciones.Text = string.Empty;
            ddlPrioridades.SelectedIndex = 0; // Seleccionar el primer elemento si no hay prioridad
        }

        protected void dgvFormulacion_DataBound(object sender, EventArgs e)
        {
            if (dgvFormulacion.HeaderRow != null)
            {
                List<Formulacion> formulacionesCompletas = Session["formulacionesCompletas"] as List<Formulacion>;
                if (formulacionesCompletas == null)
                {
                    formulacionesCompletas = negocio.listar(); // Fallback, should be in session
                    Session["formulacionesCompletas"] = formulacionesCompletas;
                }

                var cblsHeaderArea = dgvFormulacion.HeaderRow.FindControl("cblsHeaderArea") as WebForms.CustomControls.TreeViewSearch;
                if (cblsHeaderArea != null)
                {
                    var areasUnicas = formulacionesCompletas
                        .Where(f => f.Obra?.Area != null)
                        .Select(f => new { Id = f.Obra.Area.Id.ToString(), Nombre = f.Obra.Area.Nombre })
                        .Distinct()
                        .OrderBy(a => a.Nombre)
                        .ToList();
                    cblsHeaderArea.DataSource = areasUnicas;
                    cblsHeaderArea.DataBind();
                    cblsHeaderArea.AcceptChanges += OnAcceptChanges;
                }

                var cblsHeaderLineaGestion = dgvFormulacion.HeaderRow.FindControl("cblsHeaderLineaGestion") as WebForms.CustomControls.TreeViewSearch;
                if (cblsHeaderLineaGestion != null)
                {
                    var lineasUnicas = formulacionesCompletas
                        .Where(f => f.Obra?.LineaGestion != null)
                        .Select(f => new { Id = f.Obra.LineaGestion.Id.ToString(), Nombre = f.Obra.LineaGestion.Nombre })
                        .Distinct()
                        .OrderBy(lg => lg.Nombre)
                        .ToList();
                    cblsHeaderLineaGestion.DataSource = lineasUnicas;
                    cblsHeaderLineaGestion.DataBind();
                    cblsHeaderLineaGestion.AcceptChanges += OnAcceptChanges;
                }

                var cblsHeaderProyecto = dgvFormulacion.HeaderRow.FindControl("cblsHeaderProyecto") as WebForms.CustomControls.TreeViewSearch;
                if (cblsHeaderProyecto != null)
                {
                    var proyectosUnicos = formulacionesCompletas
                        .Where(f => f.Obra?.Proyecto != null)
                        .Select(f => new { Id = f.Obra.Proyecto.Id.ToString(), Nombre = f.Obra.Proyecto.Proyecto })
                        .Distinct()
                        .OrderBy(p => p.Nombre)
                        .ToList();
                    cblsHeaderProyecto.DataSource = proyectosUnicos;
                    cblsHeaderProyecto.DataBind();
                    cblsHeaderProyecto.AcceptChanges += OnAcceptChanges;
                }

                var cblsHeaderMonto2026 = dgvFormulacion.HeaderRow.FindControl("cblsHeaderMonto2026") as WebForms.CustomControls.TreeViewSearch;
                if (cblsHeaderMonto2026 != null)
                {
                    var montosUnicos = formulacionesCompletas
                        .Select(f => f.Monto_26)
                        .Distinct()
                        .OrderBy(m => m)
                        .Select(m => new { Id = m.ToString(CultureInfo.InvariantCulture), Nombre = m.ToString("N2") }) // Use "N2" or "C" for display
                        .ToList();
                    cblsHeaderMonto2026.DataSource = montosUnicos;
                    cblsHeaderMonto2026.DataBind();
                    cblsHeaderMonto2026.AcceptChanges += OnAcceptChanges;
                }

                var cblsHeaderPrioridad = dgvFormulacion.HeaderRow.FindControl("cblsHeaderPrioridad") as WebForms.CustomControls.TreeViewSearch;
                if (cblsHeaderPrioridad != null)
                {
                    var prioridadesUnicas = formulacionesCompletas
                        .Where(f => f.Prioridad != null)
                        .Select(f => new { Id = f.Prioridad.Id.ToString(), Nombre = f.Prioridad.Nombre })
                        .Distinct()
                        .OrderBy(p => p.Nombre)
                        .ToList();
                    cblsHeaderPrioridad.DataSource = prioridadesUnicas;
                    cblsHeaderPrioridad.DataBind();
                    cblsHeaderPrioridad.AcceptChanges += OnAcceptChanges;
                }
            }
        }

        private void BindDropDownList()
        {
            ddlObra.Items.Clear();
            ddlUnidadMedida.Items.Clear();
            ddlPrioridades.Items.Clear(); // Añadir esta línea para limpiar las prioridades


            // Agregar elementos vacíos a cada dropdown
            ddlObra.Items.Add(new ListItem("Seleccione una obra", ""));
            ddlUnidadMedida.Items.Add(new ListItem("Seleccione una unidad de medida", ""));
            ddlPrioridades.Items.Add(new ListItem("Seleccione una prioridad", "")); // Añadir esta línea


            // Establecer la propiedad AppendDataBoundItems a true para todos los dropdowns
            ddlObra.AppendDataBoundItems = true;
            ddlUnidadMedida.AppendDataBoundItems = true;
            ddlPrioridades.AppendDataBoundItems = true; // Añadir esta línea

            // Cargar obras
            ddlObra.DataSource = ObtenerObras();
            ddlObra.DataTextField = "NOMBRE";
            ddlObra.DataValueField = "ID";
            ddlObra.DataBind();

            // Cargar unidades de medida
            ddlUnidadMedida.DataSource = ObtenerUnidadesMedida();
            ddlUnidadMedida.DataTextField = "NOMBRE";
            ddlUnidadMedida.DataValueField = "ID";
            ddlUnidadMedida.DataBind();

            ddlPrioridades.DataSource = listarPrioridades();
            ddlPrioridades.DataTextField = "NOMBRE";
            ddlPrioridades.DataValueField = "ID";
            ddlPrioridades.DataBind();
        }
        private DataTable listarPrioridades()
        {
            PrioridadNegocio prioridadNegocio = new PrioridadNegocio();
            return prioridadNegocio.listarddl();
        }
        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            string filtro = txtBuscar.Text.Trim();
            CargarListaFormulaciones(filtro);
        }

        protected void BtnClearFilters_Click(object sender, EventArgs e)
        {
            txtBuscar.Text = string.Empty;

            WebForms.CustomControls.TreeViewSearch.ClearAllFiltersOnPage(this.Page);

            CargarListaFormulaciones();
        }

    }
}