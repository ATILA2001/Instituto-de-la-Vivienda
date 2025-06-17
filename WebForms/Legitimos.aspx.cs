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
    public partial class Legitimos : System.Web.UI.Page
    {
        private LegitimoNegocio negocio = new LegitimoNegocio();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                Usuario usuarioLogueado = (Usuario)Session["usuario"];
                if (usuarioLogueado != null && usuarioLogueado.Area != null)
                {
                    // Cargar la lista completa de legitimos para el área del usuario y guardarla en sesión
                    //List<Legitimo> listaCompletaUsuario = negocio.listarFiltro(usuarioLogueado,
                    //    new List<string>(), new List<string>(), new List<string>(), new List<string>(), null);
                    //Session["legitimosUsuarioCompleto"] = listaCompletaUsuario;
                    CargarListaLegitimos(null, true);
                }
                else
                {
                    Session["legitimosUsuarioCompleto"] = new List<Legitimo>();
                    lblMensaje.Text = "No se pudo determinar el área del usuario. No se pueden cargar los legítimos.";
                    lblMensaje.CssClass = "alert alert-warning";
                }

                BindDropDownList(); 
                CargarListaLegitimos();
            }
        }
        protected void OnAcceptChanges(object sender, EventArgs e)
        {
            CargarListaLegitimos();
        }


        protected void btnExportarExcel_Click(object sender, EventArgs e)
        {
            try
            {
                // Obtener usuario logueado
                Usuario usuarioLogueado = (Usuario)Session["usuario"];
                if (usuarioLogueado == null || usuarioLogueado.Area == null)
                {
                    lblMensaje.Text = "No se pudo determinar el área del usuario para exportar.";
                    lblMensaje.CssClass = "text-warning";
                    return;
                }

                // Obtener todos los legítimos abonos del usuario desde la sesión o volver a cargarlos
                List<Legitimo> legitimos;

                if (Session["legitimosUsuarioCompleto"] != null)
                {
                    legitimos = (List<Legitimo>)Session["legitimosUsuarioCompleto"];
                }
                else
                {
                    // Correct method call based on provided type signatures
                    legitimos = negocio.listarFiltro(usuarioLogueado, new List<string>(), new List<string>(), new List<string>(), new List<string>(), null);
                    Session["legitimosUsuarioCompleto"] = legitimos;
                }

                if (legitimos.Any())
                {
                    // Definir mapeo de columnas (encabezado de columna -> ruta de propiedad)
                    var mapeoColumnas = new Dictionary<string, string>
                    {
                        { "Obra", "Obra.Descripcion" },
                        { "Empresa", "Empresa" },
                        { "Código Autorizante", "CodigoAutorizante" },
                        { "Expediente", "Expediente" },
                        { "Inicio Ejecución", "InicioEjecucion" },
                        { "Fin Ejecución", "FinEjecucion" },
                        { "Certificado", "Certificado" },
                        { "Mes Aprobación", "MesAprobacion" },
                        { "Estado", "Estado" },
                        { "Sigaf", "Sigaf" },
                        { "Buzon sade", "BuzonSade" },
                        { "Fecha sade", "FechaSade" }
                    };

                    // Exportar a Excel
                    ExcelHelper.ExportarDatosGenericos(dgvLegitimos, legitimos, mapeoColumnas, "LegitimosAbonos");
                }
                else
                {
                    lblMensaje.Text = "No hay datos para exportar";
                    lblMensaje.CssClass = "text-warning";
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = "Error al exportar: " + ex.Message;
                lblMensaje.CssClass = "text-danger";
            }
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
                    // Change modal title and button text for editing mode
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "UpdateModalTitle",
                        "$('#modalAgregar .modal-title').text('Modificar Legítimo');", true);
                    btnAgregar.Text = "Actualizar";

                    // Load the legitimo data into the form fields
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
                            
                            // Hide the Obra dropdown and its label
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
        private void CargarListaLegitimos(string filtro = null, bool forzarRecargaCompleta = false)
        {
            try
            {
                List<Legitimo> listaCompleta;

                    Usuario usuarioLogueado = (Usuario)Session["usuario"];
                    if (usuarioLogueado != null && usuarioLogueado.Area != null)
                    {
                        //listaCompleta = negocio.listarFiltro(usuarioLogueado, new List<string>(), new List<string>(), new List<string>(), new List<string>(), null);
                        //Session["legitimosUsuarioCompleto"] = listaCompleta;

                        if (forzarRecargaCompleta || Session["legitimosUsuarioCompleto"] == null)
                        {
                            listaCompleta = negocio.listarFiltro(usuarioLogueado, new List<string>(), new List<string>(), new List<string>(), new List<string>(), null);
                            Session["legitimosUsuarioCompleto"] = listaCompleta;
                        }
                        else
                        {
                            listaCompleta = (List<Legitimo>)Session["legitimosUsuarioCompleto"];
                        }

                    }
                    else
                    {
                        dgvLegitimos.DataSource = new List<Legitimo>();
                        dgvLegitimos.DataBind();
                        CalcularSubtotal();
                        return;
                    }


                IEnumerable<Legitimo> listaFiltrada = listaCompleta;

                // Obtener valores de los filtros de cabecera
                List<string> selectedHeaderEmpresas = new List<string>();
                List<string> selectedHeaderAutorizantes = new List<string>();
                List<string> selectedHeaderMeses = new List<string>(); // Formato "yyyy-MM-dd"
                List<string> selectedHeaderEstados = new List<string>();

                if (dgvLegitimos.HeaderRow != null)
                {
                    var cblsHeaderEmpresaCtrl = dgvLegitimos.HeaderRow.FindControl("cblsHeaderEmpresa") as WebForms.CustomControls.TreeViewSearch;
                    if (cblsHeaderEmpresaCtrl != null) selectedHeaderEmpresas = cblsHeaderEmpresaCtrl.SelectedValues;

                    var cblsHeaderAutorizanteCtrl = dgvLegitimos.HeaderRow.FindControl("cblsHeaderAutorizante") as WebForms.CustomControls.TreeViewSearch;
                    if (cblsHeaderAutorizanteCtrl != null) selectedHeaderAutorizantes = cblsHeaderAutorizanteCtrl.SelectedValues;

                    var cblsHeaderMesAprobacionCtrl = dgvLegitimos.HeaderRow.FindControl("cblsHeaderMesAprobacion") as WebForms.CustomControls.TreeViewSearch;
                    if (cblsHeaderMesAprobacionCtrl != null) selectedHeaderMeses = cblsHeaderMesAprobacionCtrl.SelectedValues;

                    var cblsHeaderEstadoCtrl = dgvLegitimos.HeaderRow.FindControl("cblsHeaderEstado") as WebForms.CustomControls.TreeViewSearch;
                    if (cblsHeaderEstadoCtrl != null) selectedHeaderEstados = cblsHeaderEstadoCtrl.SelectedValues;
                }

                // Aplicar filtro de texto general
                string filtroTexto = string.IsNullOrEmpty(filtro) ? txtBuscar.Text.Trim().ToUpper() : filtro.Trim().ToUpper();

                if (!string.IsNullOrEmpty(filtroTexto))
                {
                    listaFiltrada = listaFiltrada.Where(l =>
                        (l.Obra?.Descripcion?.ToUpper().Contains(filtroTexto) ?? false) ||
                        (l.Empresa?.ToUpper().Contains(filtroTexto) ?? false) ||
                        (l.CodigoAutorizante?.ToUpper().Contains(filtroTexto) ?? false) ||
                        (l.Expediente?.ToUpper().Contains(filtroTexto) ?? false) ||
                        (l.Estado?.ToUpper().Contains(filtroTexto) ?? false) ||
                        (l.Sigaf.ToString().Contains(filtroTexto)) ||
                        (l.BuzonSade?.ToUpper().Contains(filtroTexto) ?? false)
                    );
                }

                // Aplicar filtros de cabecera
                if (selectedHeaderEmpresas.Any())
                    listaFiltrada = listaFiltrada.Where(l => !string.IsNullOrEmpty(l.Empresa) && selectedHeaderEmpresas.Contains(l.Empresa));

                if (selectedHeaderAutorizantes.Any())
                    listaFiltrada = listaFiltrada.Where(l => !string.IsNullOrEmpty(l.CodigoAutorizante) && selectedHeaderAutorizantes.Contains(l.CodigoAutorizante));

                if (selectedHeaderEstados.Any())
                    listaFiltrada = listaFiltrada.Where(l => !string.IsNullOrEmpty(l.Estado) && selectedHeaderEstados.Contains(l.Estado));

                if (selectedHeaderMeses.Any())
                {
                    // Asumimos que selectedHeaderMeses contiene strings en formato "yyyy-MM-dd"
                    var fechasSeleccionadas = selectedHeaderMeses
                        .Select(s => DateTime.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt) ? (DateTime?)dt : null)
                        .Where(d => d.HasValue)
                        .Select(d => d.Value.Date) // Comparamos solo la parte de la fecha
                        .ToList();
                    if (fechasSeleccionadas.Any())
                    {
                        listaFiltrada = listaFiltrada.Where(l => l.MesAprobacion.HasValue && fechasSeleccionadas.Contains(l.MesAprobacion.Value.Date));
                    }
                }

                List<Legitimo> resultadoFinal = listaFiltrada.ToList();
                Session["listaLegitimos"] = resultadoFinal; // Actualizar la sesión con la lista filtrada
                dgvLegitimos.DataSource = resultadoFinal;
                dgvLegitimos.DataBind();
                CalcularSubtotal();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cargar los legítimos: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
                System.Diagnostics.Debug.WriteLine($"Error en CargarListaLegitimos (User): {ex.ToString()}");
            }

        }


        protected void dgvLegitimos_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.Header)
            {
                List<Legitimo> legitimosUsuarioCompleto = Session["legitimosUsuarioCompleto"] as List<Legitimo>;
                if (legitimosUsuarioCompleto == null || !legitimosUsuarioCompleto.Any()) return;

                // Poblar filtro de Empresa
                var cblsHeaderEmpresa = e.Row.FindControl("cblsHeaderEmpresa") as WebForms.CustomControls.TreeViewSearch;
                if (cblsHeaderEmpresa != null)
                {
                    var items = legitimosUsuarioCompleto
                        .Where(l => l != null)
                        .Select(l => l.Empresa)
                        .Distinct()
                        .OrderBy(emp => emp)
                        .Select(emp => new { Nombre = emp })
                        .ToList();
                    cblsHeaderEmpresa.DataTextField = "Nombre";
                    cblsHeaderEmpresa.DataValueField = "Nombre";
                    cblsHeaderEmpresa.DataSource = items;
                    cblsHeaderEmpresa.DataBind();
                }

                // Poblar filtro de Código Autorizante
                var cblsHeaderAutorizante = e.Row.FindControl("cblsHeaderAutorizante") as WebForms.CustomControls.TreeViewSearch;
                if (cblsHeaderAutorizante != null)
                {
                    var items = legitimosUsuarioCompleto
                        .Where(l => l != null)
                        .Select(l => l.CodigoAutorizante)
                        .Distinct()
                        .OrderBy(cod => cod)
                        .Select(cod => new { Nombre = cod })
                        .ToList();
                    cblsHeaderAutorizante.DataTextField = "Nombre";
                    cblsHeaderAutorizante.DataValueField = "Nombre";
                    cblsHeaderAutorizante.DataSource = items;
                    cblsHeaderAutorizante.DataBind();
                }

                // Poblar filtro de Mes Aprobación
                var cblsHeaderMesAprobacion = e.Row.FindControl("cblsHeaderMesAprobacion") as WebForms.CustomControls.TreeViewSearch;
                if (cblsHeaderMesAprobacion != null)
                {
                    var mesesUnicos = legitimosUsuarioCompleto
                        .Where(l => l.MesAprobacion.HasValue)
                        .Select(l => l.MesAprobacion.Value.Date)
                        .Distinct()
                        .OrderByDescending(d => d)
                        .Select(d => new {
                            Nombre = d.ToString("MMMM yyyy", new CultureInfo("es-ES")),
                            Valor = d.ToString("yyyy-MM-dd") // Usar yyyy-MM-dd para el valor
                        })
                        .ToList();
                    cblsHeaderMesAprobacion.DataTextField = "Nombre";
                    cblsHeaderMesAprobacion.DataValueField = "Valor";
                    cblsHeaderMesAprobacion.DataSource = mesesUnicos;
                    cblsHeaderMesAprobacion.DataBind();
                }

                // Poblar filtro de Estado
                var cblsHeaderEstado = e.Row.FindControl("cblsHeaderEstado") as WebForms.CustomControls.TreeViewSearch;
                if (cblsHeaderEstado != null)
                {
                    var items = legitimosUsuarioCompleto
                        .Where(l => l != null)
                        .Select(l => l.Estado)
                        .Distinct()
                        .OrderBy(est => est)
                        .Select(est => new { Nombre = est })
                        .ToList();
                    cblsHeaderEstado.DataTextField = "Nombre";
                    cblsHeaderEstado.DataValueField = "Nombre";
                    cblsHeaderEstado.DataSource = items;
                    cblsHeaderEstado.DataBind();
                }
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
                    CargarListaLegitimos();
                    CalcularSubtotal();

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
                    CargarListaLegitimos();
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
        }
        private DataTable ObtenerObras()
        {
            ObraNegocio barrioNegocio = new ObraNegocio();
            Usuario usuarioLogueado = (Usuario)Session["usuario"];
            return barrioNegocio.listarddl(usuarioLogueado);
        }
        private DataTable ObtenerLegitimos()
        {
            LegitimoNegocio barrioNegocio = new LegitimoNegocio();
            Usuario usuarioLogueado = (Usuario)Session["usuario"];
            return barrioNegocio.listarddl(usuarioLogueado);
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
            TextBox txtExpedienteControl = (TextBox)sender; // El control que disparó el evento
            GridViewRow row = (GridViewRow)txtExpedienteControl.NamingContainer; // Obtener la fila del GridView

            // Asegurarse de que el DataKeyNames="ID" esté configurado en el GridView
            int idLegitimo = Convert.ToInt32(dgvLegitimos.DataKeys[row.RowIndex].Value);
            string nuevoExpediente = txtExpedienteControl.Text;

            try
            {
                // No es necesario crear una nueva instancia de LegitimoNegocio si ya tienes una a nivel de clase.
                // LegitimoNegocio negocioLocal = new LegitimoNegocio(); 
                if (negocio.ActualizarExpediente(idLegitimo, nuevoExpediente))
                {
                    lblMensaje.Text = "Expediente actualizado correctamente.";
                    lblMensaje.CssClass = "alert alert-success";

                    // Recargar la lista completa en sesión y luego la lista filtrada
                    Usuario usuarioLogueado = (Usuario)Session["usuario"];
                    if (usuarioLogueado != null && usuarioLogueado.Area != null)
                    {
                        List<Legitimo> listaCompletaUsuario = negocio.listarFiltro(usuarioLogueado,
                            new List<string>(), new List<string>(), new List<string>(), new List<string>(), null);
                        Session["legitimosUsuarioCompleto"] = listaCompletaUsuario;
                    }
                    CargarListaLegitimos(txtBuscar.Text.Trim());
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
        private void CalcularSubtotal()
        {
            decimal subtotal = 0;
            List<Legitimo> dataSource = dgvLegitimos.DataSource as List<Legitimo>;

            if (dataSource != null)
            {
                subtotal = dataSource.Sum(l => l.Certificado ?? 0); // Usar ?? 0 si Certificado es nullable decimal
            }
            else if (Session["listaLegitimos"] != null)
            {
                dataSource = (List<Legitimo>)Session["listaLegitimos"];
                subtotal = dataSource.Sum(l => l.Certificado ?? 0);
            }
            txtSubtotal.Text = subtotal.ToString("C");
        }
        private void BindDropDownList()
        {// Clear existing items
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
        private DataTable ObtenerEmpresas()
        {
            EmpresaNegocio empresaNegocio = new EmpresaNegocio();
            return empresaNegocio.listarddl();
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
            CargarListaLegitimos(filtro);
        }

        protected void BtnClearFilters_Click(object sender, EventArgs e)
        {
            txtBuscar.Text = string.Empty;

            WebForms.CustomControls.TreeViewSearch.ClearAllFiltersOnPage(this.Page);

            CargarListaLegitimos();
        }

        

    }

}