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
    public partial class LegitimosAdmin : System.Web.UI.Page
    {
        private LegitimoNegocio negocio = new LegitimoNegocio();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Cargar la lista completa una vez y guardarla en sesión
                // Idealmente, habría un método negocio.ListarTodosLegitimos() o similar.
                //List<Legitimo> listaCompleta = negocio.listarFiltro(new List<string>(), new List<string>(), new List<string>(), new List<string>(), new List<string>(), new List<string>(), null);

                //Session["legitimosCompleto"] = listaCompleta;
                CargarListaLegitimos(null, true);
                BindDropDownList();
            }
            else
            {
                if (ViewState["EditingLegitimoId"] != null && ViewState["EditingObraId"] != null)
                {
                    int obraId = (int)ViewState["EditingObraId"];
                    ddlObra.ClearSelection();
                    ListItem item = ddlObra.Items.FindByValue(obraId.ToString());
                    if (item != null)
                    {
                        item.Selected = true;
                    }
                    ddlObra.Enabled = false;
                }
            }
        }
        protected void btnExportarExcel_Click(object sender, EventArgs e)
        {
            try
            {
                // Obtener todos los legítimos abonos (sin paginación)  
                List<Legitimo> legitimos;

                if (Session["legitimosCompleto"] != null)
                {
                    legitimos = (List<Legitimo>)Session["legitimosCompleto"];
                }
                else
                {
                    // Correct method call based on provided type signatures  
                    legitimos = negocio.listarFiltro(new List<string>(), new List<string>(), new List<string>(), new List<string>(), new List<string>(), new List<string>(), null);
                    Session["legitimosCompleto"] = legitimos;
                }

                if (legitimos.Any())
                {
                    // Definir mapeo de columnas (encabezado de columna -> ruta de propiedad)  
                    var mapeoColumnas = new Dictionary<string, string>
                    {
                        { "Area", "Obra.Area.Nombre" },
                        { "Área", "Obra.Area.Nombre" },
                        { "Obra", "Obra.Descripcion" },
                        { "Empresa", "Empresa" },
                        { "Código Autorizante", "CodigoAutorizante" },
                        { "Codigo Autorizante", "CodigoAutorizante" },
                        { "Expediente", "Expediente" },
                        { "Inicio Ejecución", "InicioEjecucion" },
                        { "Inicio Ejecucion", "InicioEjecucion" },
                        { "Fin Ejecución", "FinEjecucion" },
                        { "Fin Ejecucion", "FinEjecucion" },
                        { "Certificado", "Certificado" },
                        { "Mes Aprobación", "MesAprobacion" },
                        { "Mes Aprobacion", "MesAprobacion" },
                        { "Estado", "Estado" },
                        { "Sigaf", "Sigaf" },
                        { "Buzon sade", "BuzonSade" },
                        { "Fecha sade", "FechaSade" },
                        { "Linea de gestion", "Linea" }
                    };

                    // Exportar a Excel  
                    ExcelHelper.ExportarDatosGenericos(dgvLegitimos, legitimos, mapeoColumnas, "LegitimosAbonos");
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
        public void OnAcceptChanges(object sender, EventArgs e)
        {
            CargarListaLegitimos();
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            // Configure validators if we're in editing mode
            if (ViewState["EditingLegitimoId"] != null)
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
            $('#modalAgregar .modal-title').text('Agregar Legítimo');
            document.getElementById('" + btnAgregar.ClientID + @"').value = 'Agregar';
            
            // Show the Obra dropdown and its label
            $('#obraContainer').show();
            
            // Show the modal
            $('#modalAgregar').modal('show');
        });", true);

            btnAgregar.Text = "Agregar";

            // Clear any editing state
            ViewState["EditingLegitimoId"] = null;
            ViewState["EditingObraId"] = null;
        }

        protected void btnLimpiar_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();
        }

        protected void btnAgregar_Click(object sender, EventArgs e)
        {
            if (Page.IsValid)
            {
                try
                {
                    LegitimoNegocio negocio = new LegitimoNegocio();
                    Legitimo legitimo = new Legitimo();

                    // Common data for both add and update operations
                    legitimo.CodigoAutorizante = txtAutorizante.Text.Trim();
                    legitimo.Expediente = txtExpediente.Text.Trim();
                    legitimo.InicioEjecucion = DateTime.Parse(txtInicioEjecucion.Text);
                    legitimo.FinEjecucion = DateTime.Parse(txtFinEjecucion.Text);
                    legitimo.Certificado = decimal.Parse(txtCertificado.Text);
                    legitimo.MesAprobacion = DateTime.Parse(txtMesAprobacion.Text);

                    // Check if we're editing an existing legitimo or adding a new one
                    if (ViewState["EditingLegitimoId"] != null)
                    {
                        // We're updating an existing legitimo
                        legitimo.Id = (int)ViewState["EditingLegitimoId"];

                        // Use the stored Obra ID from ViewState for the update
                        if (ViewState["EditingObraId"] != null)
                        {
                            legitimo.Obra = new Obra { Id = (int)ViewState["EditingObraId"] };
                        }

                        if (negocio.modificar(legitimo))
                        {
                            lblMensaje.Text = "Legítimo modificado exitosamente!";
                            lblMensaje.CssClass = "alert alert-success";

                            // Clear the editing state
                            ViewState["EditingLegitimoId"] = null;
                            ViewState["EditingObraId"] = null;
                        }
                        else
                        {
                            lblMensaje.Text = "Hubo un problema al modificar el legítimo.";
                            lblMensaje.CssClass = "alert alert-danger";
                        }
                    }
                    else
                    {
                        // We're adding a new legitimo - use the selected value from the dropdown
                        legitimo.Obra = new Obra { Id = int.Parse(ddlObra.SelectedValue) };

                        if (negocio.agregar(legitimo))
                        {
                            lblMensaje.Text = "Legítimo agregado exitosamente!";
                            lblMensaje.CssClass = "alert alert-success";
                        }
                        else
                        {
                            lblMensaje.Text = "Hubo un problema al agregar el legítimo.";
                            lblMensaje.CssClass = "alert alert-danger";
                        }
                    }

                    // Clear fields
                    LimpiarFormulario();

                    // Reset the modal title and button text
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "ResetModalTitle",
                        "$('#modalAgregar .modal-title').text('Agregar Legítimo');", true);
                    btnAgregar.Text = "Agregar";

                    // Hide the modal
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "HideModal",
                        "$('#modalAgregar').modal('hide');", true);

                    // Refresh the legitimos list
                    CargarListaLegitimos(null,true);
                    CalcularSubtotal();
                }
                catch (Exception ex)
                {
                    lblMensaje.Text = $"Error: {ex.Message}";
                    lblMensaje.CssClass = "alert alert-danger";
                }
            }
        }

        private void LimpiarFormulario()
        {
            txtAutorizante.Text = string.Empty;
            txtExpediente.Text = string.Empty;
            txtInicioEjecucion.Text = string.Empty;
            txtFinEjecucion.Text = string.Empty;
            txtCertificado.Text = string.Empty;
            txtMesAprobacion.Text = string.Empty;
            ddlObra.SelectedIndex = 0;

            // Make sure the dropdown is enabled
            ddlObra.Enabled = true;
        }

        private DataTable ObtenerAreas()
        {
            AreaNegocio areaNegocio = new AreaNegocio();
            return areaNegocio.listarddl();
        }

        private void CargarListaLegitimos(string filtro = null, bool forzarRecargaCompleta = false)
        {
            try
            {
                //List<Legitimo> listaCompleta = negocio.listarFiltro(null, null, null, null, null, null, null);
                List<Legitimo> listaCompleta;

                if (forzarRecargaCompleta || Session["legitimosCompleto"] == null)
                {
                    listaCompleta = negocio.listarFiltro(new List<string>(), new List<string>(), new List<string>(), new List<string>(), new List<string>(), new List<string>(), null);
                }
                else
                {
                    listaCompleta = (List<Legitimo>)Session["legitimosCompleto"];
                }


                Session["legitimosCompleto"] = listaCompleta;

                IEnumerable<Legitimo> listaFiltrada = listaCompleta;

                // Obtener valores de los filtros de cabecera
                List<string> selectedAreas = new List<string>();
                List<string> selectedEmpresas = new List<string>();
                List<string> selectedCodigosAutorizante = new List<string>();
                List<string> selectedMesesAprobacion = new List<string>(); // Valores "yyyy-MM-dd"
                List<string> selectedEstados = new List<string>();
                List<string> selectedLineas = new List<string>();

                if (dgvLegitimos.HeaderRow != null)
                {
                    var cblsHeaderAreaControl = dgvLegitimos.HeaderRow.FindControl("cblsHeaderArea") as WebForms.CustomControls.TreeViewSearch;
                    if (cblsHeaderAreaControl != null) selectedAreas = cblsHeaderAreaControl.SelectedValues;

                    var cblsHeaderEmpresaControl = dgvLegitimos.HeaderRow.FindControl("cblsHeaderEmpresa") as WebForms.CustomControls.TreeViewSearch;
                    if (cblsHeaderEmpresaControl != null) selectedEmpresas = cblsHeaderEmpresaControl.SelectedValues;

                    var cblsHeaderCodigoAutorizanteControl = dgvLegitimos.HeaderRow.FindControl("cblsHeaderCodigoAutorizante") as WebForms.CustomControls.TreeViewSearch;
                    if (cblsHeaderCodigoAutorizanteControl != null) selectedCodigosAutorizante = cblsHeaderCodigoAutorizanteControl.SelectedValues;

                    var cblsHeaderMesAprobacionControl = dgvLegitimos.HeaderRow.FindControl("cblsHeaderMesAprobacion") as WebForms.CustomControls.TreeViewSearch;
                    if (cblsHeaderMesAprobacionControl != null) selectedMesesAprobacion = cblsHeaderMesAprobacionControl.SelectedValues;

                    var cblsHeaderEstadoControl = dgvLegitimos.HeaderRow.FindControl("cblsHeaderEstado") as WebForms.CustomControls.TreeViewSearch;
                    if (cblsHeaderEstadoControl != null) selectedEstados = cblsHeaderEstadoControl.SelectedValues;

                    var cblsHeaderLineaControl = dgvLegitimos.HeaderRow.FindControl("cblsHeaderLinea") as WebForms.CustomControls.TreeViewSearch;
                    if (cblsHeaderLineaControl != null) selectedLineas = cblsHeaderLineaControl.SelectedValues;
                }

                string filtroTextoGeneral = string.IsNullOrEmpty(filtro) ? txtBuscar.Text.Trim().ToUpper() : filtro.Trim().ToUpper();

                if (!string.IsNullOrEmpty(filtroTextoGeneral))
                {
                    listaFiltrada = listaFiltrada.Where(l =>
                        (l.Obra?.Area?.Nombre?.ToUpper().Contains(filtroTextoGeneral) ?? false) ||
                        (l.Obra?.Descripcion?.ToUpper().Contains(filtroTextoGeneral) ?? false) ||
                        (l.Empresa?.ToUpper().Contains(filtroTextoGeneral) ?? false) ||
                        (l.CodigoAutorizante?.ToUpper().Contains(filtroTextoGeneral) ?? false) ||
                        (l.Expediente?.ToUpper().Contains(filtroTextoGeneral) ?? false) ||
                        (l.Estado?.ToUpper().Contains(filtroTextoGeneral) ?? false) ||
                        (l.Linea?.ToUpper().Contains(filtroTextoGeneral) ?? false) ||
                        (l.Sigaf.ToString().Contains(filtroTextoGeneral)) ||
                        (l.BuzonSade?.ToUpper().Contains(filtroTextoGeneral) ?? false)
                    );
                }

                if (selectedAreas.Any())
                    listaFiltrada = listaFiltrada.Where(l => selectedAreas.Contains(l.Obra.Area.Nombre ?? ""));
                if (selectedEmpresas.Any())
                    listaFiltrada = listaFiltrada.Where(l => selectedEmpresas.Contains(l.Empresa ?? ""));
                if (selectedCodigosAutorizante.Any())
                    listaFiltrada = listaFiltrada.Where(l => selectedCodigosAutorizante.Contains(l.CodigoAutorizante ?? ""));
                if (selectedEstados.Any())
                    listaFiltrada = listaFiltrada.Where(l => selectedEstados.Contains(l.Estado ?? ""));


                if (selectedLineas.Any())                  
                    listaFiltrada = listaFiltrada.Where(l => selectedLineas.Contains(l.Linea ?? ""));
                

                if (selectedMesesAprobacion.Any())
                {
                    var fechasSeleccionadas = selectedMesesAprobacion
                        .Select(s => DateTime.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt) ? (DateTime?)dt : null)
                        .Where(d => d.HasValue)
                        .Select(d => d.Value.Date)
                        .ToList();
                    if (fechasSeleccionadas.Any())
                    {
                        listaFiltrada = listaFiltrada.Where(l => l.MesAprobacion.HasValue && fechasSeleccionadas.Contains(l.MesAprobacion.Value.Date));
                    }
                }

                List<Legitimo> resultadoFinal = listaFiltrada.ToList();
                Session["listaLegitimos"] = resultadoFinal;
                dgvLegitimos.DataSource = resultadoFinal;
                dgvLegitimos.DataBind();
                CalcularSubtotal();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cargar los legítimos: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
                dgvLegitimos.DataSource = null;
                dgvLegitimos.DataBind();
                txtSubtotal.Text = 0.ToString("C");
            }
        }

        protected void dgvLegitimos_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.Header)
            {
                List<Legitimo> legitimosCompleto = Session["legitimosCompleto"] as List<Legitimo>;

                if (legitimosCompleto == null)
                {
                    legitimosCompleto = negocio.listarFiltro(new List<string>(), new List<string>(), new List<string>(), new List<string>(), new List<string>(), new List<string>(), null);
                    Session["legitimosCompleto"] = legitimosCompleto;
                }

                if (legitimosCompleto == null || !legitimosCompleto.Any())
                {
                    return; // No hay datos para poblar los filtros.
                }     

                var cblsHeaderArea = e.Row.FindControl("cblsHeaderArea") as WebForms.CustomControls.TreeViewSearch;
                if (cblsHeaderArea != null)
                {
                    var areasUnicas = legitimosCompleto
                        .Where(l => l.Obra?.Area != null)
                        .Select(l => l.Obra.Area.Nombre)
                        .Distinct()
                        .OrderBy(nombre => nombre)
                        .Select(nombre => new { Nombre = nombre }) // El control espera un objeto con propiedad "Nombre" y "Id" (o lo que se especifique en DataTextField/ValueField)
                        .ToList();
                    cblsHeaderArea.DataTextField = "Nombre";
                    cblsHeaderArea.DataValueField = "Nombre"; // Filtra por el nombre
                    cblsHeaderArea.DataSource = areasUnicas;
                    cblsHeaderArea.DataBind();
                }

                var cblsHeaderEmpresa = e.Row.FindControl("cblsHeaderEmpresa") as WebForms.CustomControls.TreeViewSearch;
                if (cblsHeaderEmpresa != null)
                {
                    var empresasUnicas = legitimosCompleto
                        .Where(l => l != null)
                        .Select(l => l.Empresa)
                        .Distinct()
                        .OrderBy(nombre => nombre)
                        .Select(nombre => new { Nombre = nombre })
                        .ToList();
                    cblsHeaderEmpresa.DataTextField = "Nombre";
                    cblsHeaderEmpresa.DataValueField = "Nombre";
                    cblsHeaderEmpresa.DataSource = empresasUnicas;
                    cblsHeaderEmpresa.DataBind();
                }

                var cblsHeaderCodigoAutorizante = e.Row.FindControl("cblsHeaderCodigoAutorizante") as WebForms.CustomControls.TreeViewSearch;
                if (cblsHeaderCodigoAutorizante != null)
                {
                    var codigosUnicos = legitimosCompleto
                        .Where(l => l != null)
                        .Select(l => l.CodigoAutorizante)
                        .Distinct()
                        .OrderBy(nombre => nombre)
                        .Select(nombre => new { Nombre = nombre })
                        .ToList();
                    cblsHeaderCodigoAutorizante.DataTextField = "Nombre";
                    cblsHeaderCodigoAutorizante.DataValueField = "Nombre";
                    cblsHeaderCodigoAutorizante.DataSource = codigosUnicos;
                    cblsHeaderCodigoAutorizante.DataBind();
                }

                var cblsHeaderMesAprobacion = e.Row.FindControl("cblsHeaderMesAprobacion") as WebForms.CustomControls.TreeViewSearch;
                if (cblsHeaderMesAprobacion != null)
                {
                    var mesesUnicos = legitimosCompleto
                        .Where(l => l.MesAprobacion.HasValue)
                        .Select(l => l.MesAprobacion.Value.Date) // Normalizar a solo fecha
                        .Distinct()
                        .OrderByDescending(d => d)
                        .Select(d => new {
                            Nombre = d.ToString("MMMM yyyy", new CultureInfo("es-ES")),
                            Valor = d.ToString("yyyy-MM-dd")
                        })
                        .ToList();
                    cblsHeaderMesAprobacion.DataTextField = "Nombre";
                    cblsHeaderMesAprobacion.DataValueField = "Valor";
                    cblsHeaderMesAprobacion.DataSource = mesesUnicos;
                    cblsHeaderMesAprobacion.DataBind();
                }

                var cblsHeaderEstado = e.Row.FindControl("cblsHeaderEstado") as WebForms.CustomControls.TreeViewSearch;
                if (cblsHeaderEstado != null)
                {
                    var estadosUnicos = legitimosCompleto
                        .Where(l => l != null)
                        .Select(l => l.Estado)
                        .Distinct()
                        .OrderBy(nombre => nombre)
                        .Select(nombre => new { Nombre = nombre })
                        .ToList();
                    cblsHeaderEstado.DataTextField = "Nombre";
                    cblsHeaderEstado.DataValueField = "Nombre";
                    cblsHeaderEstado.DataSource = estadosUnicos;
                    cblsHeaderEstado.DataBind();
                }

                var cblsHeaderLinea = e.Row.FindControl("cblsHeaderLinea") as WebForms.CustomControls.TreeViewSearch;
                if (cblsHeaderLinea != null)
                {
                    var lineasUnicas = legitimosCompleto
                        .Where(l => l != null)
                        .Select(l => l.Linea)
                        .Distinct()
                        .OrderBy(nombre => nombre)
                        .Select(nombre => new { Nombre = nombre })
                        .ToList();
                    cblsHeaderLinea.DataTextField = "Nombre";
                    cblsHeaderLinea.DataValueField = "Nombre";
                    cblsHeaderLinea.DataSource = lineasUnicas;
                    cblsHeaderLinea.DataBind();
                }
            }
        }


        protected void dgvLegitimos_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                // Get the ID of the selected row
                int idLegitimo = Convert.ToInt32(dgvLegitimos.SelectedDataKey.Value);

                // Get the list of legitimos from session
                List<Legitimo> legitimosList = (List<Legitimo>)Session["listaLegitimos"];

                // Find the selected legitimo
                Legitimo legitimoSeleccionado = legitimosList.FirstOrDefault(l => l.Id == idLegitimo);

                if (legitimoSeleccionado != null)
                {
                    // Set button text to "Actualizar"
                    btnAgregar.Text = "Actualizar";

                    // Fill form data
                    txtAutorizante.Text = legitimoSeleccionado.CodigoAutorizante;
                    txtExpediente.Text = legitimoSeleccionado.Expediente;

                    if (legitimoSeleccionado.InicioEjecucion.HasValue)
                        txtInicioEjecucion.Text = legitimoSeleccionado.InicioEjecucion.Value.ToString("yyyy-MM-dd");

                    if (legitimoSeleccionado.FinEjecucion.HasValue)
                        txtFinEjecucion.Text = legitimoSeleccionado.FinEjecucion.Value.ToString("yyyy-MM-dd");

                    txtCertificado.Text = legitimoSeleccionado.Certificado.ToString();

                    if (legitimoSeleccionado.MesAprobacion.HasValue)
                        txtMesAprobacion.Text = legitimoSeleccionado.MesAprobacion.Value.ToString("yyyy-MM-dd");

                    // Store the ID of the legitimo being edited in ViewState
                    ViewState["EditingLegitimoId"] = idLegitimo;

                    // Also store the Obra ID so we can use it later in the update process
                    if (legitimoSeleccionado.Obra != null)
                        ViewState["EditingObraId"] = legitimoSeleccionado.Obra.Id;

                    // Update modal title and hide the Obra field
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "UpdateModalAndShow", @"
                $(document).ready(function() {
                    // Change title and button text
                    $('#modalAgregar .modal-title').text('Modificar Legítimo');
                    document.getElementById('" + btnAgregar.ClientID + @"').value = 'Actualizar';
                    
                    // Hide the Obra dropdown and its label (assuming they're in the same div with class mb-3)
                    $('#obraContainer').hide();
                    
                    // Show the modal
                    $('#modalAgregar').modal('show');
                });", true);
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cargar los datos del legítimo: {ex.Message}";
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

        protected void dgvLegitimos_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            try
            {
                var id = dgvLegitimos.DataKeys[e.RowIndex].Value.ToString();
                if (negocio.eliminar(id))
                {
                    lblMensaje.Text = "Legítimo eliminado correctamente.";
                    lblMensaje.CssClass = "alert alert-success";
                    CargarListaLegitimos(null,true);
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al eliminar el legítimo: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        protected void dgvLegitimos_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            try
            {
                dgvLegitimos.PageIndex = e.NewPageIndex;
                CargarListaLegitimos();
                CalcularSubtotal();


            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cambiar de página: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        private DataTable ObtenerObras()
        {
            ObraNegocio barrioNegocio = new ObraNegocio();
            return barrioNegocio.listarddl();
        }

        private DataTable ObtenerEstadosExpedientes()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("ID", typeof(int));
            dt.Columns.Add("NOMBRE", typeof(string));

            DataRow _1 = dt.NewRow();
            _1["ID"] = 0;
            _1["NOMBRE"] = "NO INICIADO";
            dt.Rows.Add(_1);

            DataRow _2 = dt.NewRow();
            _2["ID"] = 1;
            _2["NOMBRE"] = "EN TRAMITE";
            dt.Rows.Add(_2);

            DataRow _3 = dt.NewRow();
            _3["ID"] = 2;
            _3["NOMBRE"] = "DEVENGADO";
            dt.Rows.Add(_3);

            return dt;
        }

        protected void txtExpediente_TextChanged(object sender, EventArgs e)
        {
            TextBox txtExpediente = (TextBox)sender;
            GridViewRow row = (GridViewRow)txtExpediente.NamingContainer;

            int id = (int)dgvLegitimos.DataKeys[row.RowIndex].Value;

            string nuevoExpediente = txtExpediente.Text;

            try
            {
                LegitimoNegocio negocio = new LegitimoNegocio();
                negocio.ActualizarExpediente(id, nuevoExpediente);

                lblMensaje.Text = "Expediente actualizado correctamente.";
                CargarListaLegitimos(null, true);
                CalcularSubtotal();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = "Error al actualizar el expediente: " + ex.Message;
            }
        }

        private void CalcularSubtotal()
        {
            decimal subtotal = 0;
            List<Legitimo> dataSource = dgvLegitimos.DataSource as List<Legitimo>;

            if (dataSource != null)
            {
                subtotal = (decimal)dataSource.Sum(l => l.Certificado);
            }
            else if (Session["listaLegitimos"] != null) // Fallback si el DataSource no está directamente asignado pero sí en sesión
            {
                dataSource = (List<Legitimo>)Session["listaLegitimos"];
                subtotal = (decimal)dataSource.Sum(l => l.Certificado);
            }
            txtSubtotal.Text = subtotal.ToString("C");
        }

        private void BindDropDownList()
        {
            // Clear existing items
            ddlObra.Items.Clear();

            // Set AppendDataBoundItems to true
            ddlObra.AppendDataBoundItems = true;

            // Add empty option
            ddlObra.Items.Add(new ListItem("Seleccione una obra", ""));

            ddlObra.DataSource = ObtenerObras();
            ddlObra.DataTextField = "Nombre";
            ddlObra.DataValueField = "Id";
            ddlObra.DataBind();
        }

        private DataTable ObtenerLegitimos()
        {
            LegitimoNegocio barrioNegocio = new LegitimoNegocio();
            return barrioNegocio.listarddl();
        }

        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            string filtro = txtBuscar.Text.Trim();
            CargarListaLegitimos(filtro);
        }

        private DataTable ObtenerEmpresas()
        {
            EmpresaNegocio empresaNegocio = new EmpresaNegocio();
            return empresaNegocio.listarddl();
        }

        private DataTable ObtenerLineaGestion()
        {
            LineaGestionNegocio barrioNegocio = new LineaGestionNegocio();
            return barrioNegocio.listarddl();
        }

        protected void BtnClearFilters_Click(object sender, EventArgs e)
        {
            txtBuscar.Text = string.Empty;

            WebForms.CustomControls.TreeViewSearch.ClearAllFiltersOnPage(this.Page);

            CargarListaLegitimos();
        }



    }
}
