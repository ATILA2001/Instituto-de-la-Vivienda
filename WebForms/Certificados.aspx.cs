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
    public partial class Certificados : System.Web.UI.Page
    {
        CertificadoNegocio negocio = new CertificadoNegocio();
        CalculoRedeterminacionNegocio calculoRedeterminacionNegocio = new CalculoRedeterminacionNegocio();
        protected void btnExportarExcel_Click(object sender, EventArgs e)
        {
            try
            {
                // Obtener usuario logueado
                Usuario usuarioLogueado = (Usuario)Session["usuario"];
                if (usuarioLogueado == null || usuarioLogueado.Area == null)
                {
                    lblMensaje.Text = "No se pudo determinar el área del usuario para exportar.";
                    lblMensaje.CssClass = "alert alert-warning";
                    return;
                }

                // Obtener todos los certificados del usuario desde la sesión o volver a cargarlos
                List<Certificado> certificados;

                if (Session["certificadosUsuarioCompleto"] != null)
                {
                    certificados = (List<Certificado>)Session["certificadosUsuarioCompleto"];
                }
                else
                {
                    certificados = calculoRedeterminacionNegocio.listarCertReliq(usuarioLogueado);
                    Session["certificadosUsuarioCompleto"] = certificados;
                }


                if (certificados.Any())
                {
                    // Definir mapeo de columnas (encabezado de columna -> ruta de propiedad)
                    var mapeoColumnas = new Dictionary<string, string>
            {
                { "Obra", "Autorizante.Obra.Descripcion" },
                { "Contrata", "Autorizante.Obra.Contrata.Nombre" },
                { "Detalle", "Autorizante.Detalle" },
                { "Empresa", "Empresa" },
                { "Código Autorizante", "Autorizante.CodigoAutorizante" },
                { "Codigo Autorizante", "Autorizante.CodigoAutorizante" },
                { "Expediente", "ExpedientePago" },
                { "Estado", "Estado" },
                { "Tipo", "Tipo.Nombre" },
                { "Monto Certificado", "MontoTotal" },
                { "Mes Certificado", "MesAprobacion" },
                { "Porcentaje", "Porcentaje" },
                { "Sigaf", "Sigaf" },
                { "Buzon sade", "BuzonSade" },
                { "Fecha sade", "FechaSade" },
                { "Área", "Autorizante.Obra.Area.Nombre" },
                { "Area", "Autorizante.Obra.Area.Nombre" }
            };

                    // Exportar a Excel
                    ExcelHelper.ExportarDatosGenericos(dgvCertificado, certificados, mapeoColumnas, "Certificados");
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
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                Usuario usuarioLogueado = (Usuario)Session["usuario"];
                if (usuarioLogueado != null && usuarioLogueado.Area != null)
                {
                    // Cargar la lista completa de certificados para el área del usuario y guardarla en sesión
                    //List<Certificado> listaCompletaUsuario = negocio.listarFiltro(usuarioLogueado,
                    //    new List<string>(), new List<string>(), new List<string>(), new List<string>(), new List<string>(), null);
                    //Session["certificadosUsuarioCompleto"] = listaCompletaUsuario;
                    CargarListaCertificados(null, true);
                }
                else
                {
                    Session["certificadosUsuarioCompleto"] = new List<Certificado>();
                    lblMensaje.Text = "No se pudo determinar el área del usuario. No se pueden cargar los certificados.";
                    lblMensaje.CssClass = "alert alert-warning";
                }

                BindDropDownList();
                CargarListaCertificados();
            }
        }

        protected void OnAcceptChanges(object sender, EventArgs e)
        {
            CargarListaCertificados();
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            // Configure validators if we're in editing mode
            if (ViewState["EditingCertificadoId"] != null)
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
                    $('#modalAgregar .modal-title').text('Agregar Certificado');
                    document.getElementById('" + btnAgregar.ClientID + @"').value = 'Agregar';
                    
                    // Show the Autorizante dropdown and its label
                    $('#autorizanteContainer').show();
                    
                    // Show the modal
                    $('#modalAgregar').modal('show');
                });", true);

            btnAgregar.Text = "Agregar";

            // Clear any editing state
            ViewState["EditingCertificadoId"] = null;
            ViewState["EditingAutorizanteId"] = null;
            ViewState["EditingCodigoAutorizante"] = null;
        }

        private void CalcularSubtotal()
        {
            decimal subtotal = 0;
            List<Certificado> dataSource = dgvCertificado.DataSource as List<Certificado>;

            if (dataSource != null)
            {
                subtotal = dataSource.Sum(c => c.MontoTotal);
            }
            else if (Session["listaCertificado"] != null)
            {
                dataSource = (List<Certificado>)Session["listaCertificado"];
                subtotal = dataSource.Sum(c => c.MontoTotal);
            }
            txtSubtotal.Text = subtotal.ToString("C");
        }

        private void CargarListaCertificados(string filtro = null, bool forzarRecargaCompleta = false)
        {
            try
            {
                List<Certificado> listaCompleta;

                Usuario usuarioLogueado = (Usuario)Session["usuario"];
                if (usuarioLogueado != null && usuarioLogueado.Area != null)
                {

                    if (forzarRecargaCompleta || Session["certificadosUsuarioCompleto"] == null)
                    {
                        //listaCompleta = negocio.listarFiltro(usuarioLogueado, new List<string>(), new List<string>(), new List<string>(), new List<string>(), new List<string>(), null);
                        listaCompleta = calculoRedeterminacionNegocio.listarCertReliq(usuarioLogueado);
                        Session["certificadosUsuarioCompleto"] = listaCompleta;
                    }
                    else
                    {
                        listaCompleta = (List<Certificado>)Session["certificadosUsuarioCompleto"];
                    }

                }
                else
                {
                    dgvCertificado.DataSource = new List<Certificado>();
                    dgvCertificado.DataBind();
                    CalcularSubtotal();
                    return;
                }



                IEnumerable<Certificado> listaFiltrada = listaCompleta;

                // Obtener valores de los filtros de cabecera
                List<string> selectedHeaderObra = new List<string>();
                List<string> selectedHeaderEmpresas = new List<string>();
                List<string> selectedHeaderCodigosAutorizante = new List<string>();
                List<string> selectedHeaderEstados = new List<string>();
                List<string> selectedHeaderTipos = new List<string>(); // IDs de TipoPago
                List<string> selectedHeaderMesesCertificado = new List<string>(); // Formato yyyy-MM-dd o yyyy_MM o year_yyyy

                if (dgvCertificado.HeaderRow != null)
                {
                    var cblsHeaderObraControl = dgvCertificado.HeaderRow.FindControl("cblsHeaderObra") as CustomControls.TreeViewSearch;
                    if (cblsHeaderObraControl != null) selectedHeaderObra = cblsHeaderObraControl.SelectedValues;


                    var cblsHeaderEmpresaCtrl = dgvCertificado.HeaderRow.FindControl("cblsHeaderEmpresa") as WebForms.CustomControls.TreeViewSearch;
                    if (cblsHeaderEmpresaCtrl != null) selectedHeaderEmpresas = cblsHeaderEmpresaCtrl.SelectedValues;

                    var cblsHeaderCodigoAutorizanteCtrl = dgvCertificado.HeaderRow.FindControl("cblsHeaderCodigoAutorizante") as WebForms.CustomControls.TreeViewSearch;
                    if (cblsHeaderCodigoAutorizanteCtrl != null) selectedHeaderCodigosAutorizante = cblsHeaderCodigoAutorizanteCtrl.SelectedValues;

                    var cblsHeaderEstadoCtrl = dgvCertificado.HeaderRow.FindControl("cblsHeaderEstado") as WebForms.CustomControls.TreeViewSearch;
                    if (cblsHeaderEstadoCtrl != null) selectedHeaderEstados = cblsHeaderEstadoCtrl.SelectedValues;

                    var cblsHeaderTipoCtrl = dgvCertificado.HeaderRow.FindControl("cblsHeaderTipo") as WebForms.CustomControls.TreeViewSearch;
                    if (cblsHeaderTipoCtrl != null) selectedHeaderTipos = cblsHeaderTipoCtrl.SelectedValues;

                    var cblsHeaderMesCertificadoCtrl = dgvCertificado.HeaderRow.FindControl("cblsHeaderMesCertificado") as WebForms.CustomControls.TreeViewSearch;
                    if (cblsHeaderMesCertificadoCtrl != null) selectedHeaderMesesCertificado = cblsHeaderMesCertificadoCtrl.SelectedValues;
                }

                // Aplicar filtro de texto general
                string filtroTexto = string.IsNullOrEmpty(filtro) ? txtBuscar.Text.Trim().ToUpper() : filtro.Trim().ToUpper();
                if (!string.IsNullOrEmpty(filtroTexto))
                {
                    listaFiltrada = listaFiltrada.Where(c =>
                        (c.Autorizante?.Obra?.Descripcion?.ToUpper().Contains(filtroTexto) ?? false) ||
                        (c.Autorizante?.Obra?.Contrata?.Nombre?.ToUpper().Contains(filtroTexto) ?? false) ||
                        (c.Autorizante?.Detalle?.ToUpper().Contains(filtroTexto) ?? false) ||
                        (c.Empresa?.ToUpper().Contains(filtroTexto) ?? false) ||
                        (c.Autorizante?.CodigoAutorizante?.ToUpper().Contains(filtroTexto) ?? false) ||
                        (c.ExpedientePago?.ToUpper().Contains(filtroTexto) ?? false) ||
                        (c.Estado?.ToUpper().Contains(filtroTexto) ?? false) ||
                        (c.Tipo?.Nombre?.ToUpper().Contains(filtroTexto) ?? false) ||
                        (c.BuzonSade?.ToUpper().Contains(filtroTexto) ?? false)
                    );
                }

                // Aplicar filtros de cabecera
                if (selectedHeaderObra.Any())
                    listaFiltrada = listaFiltrada.Where(m => m.Autorizante?.Obra != null && selectedHeaderObra.Contains(m.Autorizante.Obra.Id.ToString()));

                if (selectedHeaderEmpresas.Any())
                    listaFiltrada = listaFiltrada.Where(c => !string.IsNullOrEmpty(c.Empresa) && selectedHeaderEmpresas.Contains(c.Empresa));

                if (selectedHeaderCodigosAutorizante.Any())
                    listaFiltrada = listaFiltrada.Where(c => c.Autorizante != null && !string.IsNullOrEmpty(c.Autorizante.CodigoAutorizante) && selectedHeaderCodigosAutorizante.Contains(c.Autorizante.CodigoAutorizante));

                if (selectedHeaderEstados.Any())
                    listaFiltrada = listaFiltrada.Where(c => !string.IsNullOrEmpty(c.Estado) && selectedHeaderEstados.Contains(c.Estado));

                if (selectedHeaderTipos.Any())
                    listaFiltrada = listaFiltrada.Where(c => c.Tipo != null && selectedHeaderTipos.Contains(c.Tipo.Id.ToString()));

                if (selectedHeaderMesesCertificado.Any())
                {
                    List<DateTime> fechasSeleccionadasDia = new List<DateTime>();
                    List<Tuple<int, int>> mesesAnioSeleccionados = new List<Tuple<int, int>>(); // Año, Mes
                    List<int> aniosSeleccionados = new List<int>();

                    foreach (var sel in selectedHeaderMesesCertificado)
                    {
                        if (DateTime.TryParseExact(sel, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dtDia))
                        {
                            fechasSeleccionadasDia.Add(dtDia.Date);
                        }
                        else if (sel.Contains("_") && !sel.StartsWith("year_")) // Formato "yyyy_MM"
                        {
                            var parts = sel.Split('_');
                            if (parts.Length == 2 && int.TryParse(parts[0], out int year) && int.TryParse(parts[1], out int month))
                            {
                                mesesAnioSeleccionados.Add(Tuple.Create(year, month));
                            }
                        }
                        else if (sel.StartsWith("year_")) // Formato "year_yyyy"
                        {
                            if (int.TryParse(sel.Substring(5), out int year))
                            {
                                aniosSeleccionados.Add(year);
                            }
                        }
                    }
                    if (fechasSeleccionadasDia.Any() || mesesAnioSeleccionados.Any() || aniosSeleccionados.Any())
                    {
                        listaFiltrada = listaFiltrada.Where(c => c.MesAprobacion.HasValue &&
                            (fechasSeleccionadasDia.Contains(c.MesAprobacion.Value.Date) ||
                             mesesAnioSeleccionados.Any(m => m.Item1 == c.MesAprobacion.Value.Year && m.Item2 == c.MesAprobacion.Value.Month) ||
                             aniosSeleccionados.Contains(c.MesAprobacion.Value.Year)));
                    }
                }

                List<Certificado> resultadoFinal = listaFiltrada.ToList();
                Session["listaCertificado"] = resultadoFinal;
                dgvCertificado.DataSource = resultadoFinal;
                dgvCertificado.DataBind();
                CalcularSubtotal();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cargar los Certificados: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
                System.Diagnostics.Debug.WriteLine($"Error en CargarListaCertificados (User): {ex.ToString()}");
            }

        }

        protected void dgvCertificado_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                // Get the ID of the selected row
                int idCertificado = Convert.ToInt32(dgvCertificado.SelectedDataKey.Value);

                // Get the list of certificados from session
                List<Certificado> certificadosList = (List<Certificado>)Session["listaCertificado"];

                // Find the selected certificado
                Certificado certificadoSeleccionado = certificadosList.FirstOrDefault(c => c.Id == idCertificado);

                if (certificadoSeleccionado != null)
                {
                    // Change modal title and button text for editing mode
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "UpdateModalTitle",
                        "$('#modalAgregar .modal-title').text('Modificar Certificado');", true);
                    btnAgregar.Text = "Actualizar";

                    // Load the certificado data into the form fields
                    txtExpediente.Text = certificadoSeleccionado.ExpedientePago;
                    txtMontoAutorizado.Text = certificadoSeleccionado.MontoTotal.ToString("0.00");

                    if (certificadoSeleccionado.MesAprobacion.HasValue)
                        txtFecha.Text = certificadoSeleccionado.MesAprobacion.Value.ToString("yyyy-MM-dd");

                    // Select the corresponding value in the Tipo dropdown
                    if (certificadoSeleccionado.Tipo != null)
                        SelectDropDownListByValue(ddlTipo, certificadoSeleccionado.Tipo.Id.ToString());

                    // Store the ID of the certificado being edited in ViewState
                    ViewState["EditingCertificadoId"] = idCertificado;

                    // Also store both the Autorizante ID and CodigoAutorizante for the update
                    if (certificadoSeleccionado.Autorizante != null)
                    {
                        ViewState["EditingAutorizanteId"] = certificadoSeleccionado.Autorizante.Id;
                        ViewState["EditingCodigoAutorizante"] = certificadoSeleccionado.Autorizante.CodigoAutorizante;
                    }

                    // Update modal title and hide the Autorizante field
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "UpdateModalAndShow", @"
                        $(document).ready(function() {
                            // Change title and button text
                            $('#modalAgregar .modal-title').text('Modificar Certificado');
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
                lblMensaje.Text = $"Error al cargar los datos del certificado: {ex.Message}";
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

        protected void dgvCertificado_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            try
            {
                var id = Convert.ToInt32(dgvCertificado.DataKeys[e.RowIndex].Value);
                if (negocio.eliminar(id))
                {
                    lblMensaje.Text = "Certificado eliminado correctamente.";
                    lblMensaje.CssClass = "alert alert-success";
                    CargarListaCertificados(null, true);
                    CalcularSubtotal();
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al eliminar el certificado: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        protected void dgvCertificado_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            try
            {
                dgvCertificado.PageIndex = e.NewPageIndex;
                CargarListaCertificados();
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
                    CertificadoNegocio certificadoNegocio = new CertificadoNegocio();
                    Certificado certificado = new Certificado();

                    // Common data for both add and update operations
                    certificado.ExpedientePago = txtExpediente.Text.Trim();
                    certificado.MontoTotal = decimal.Parse(txtMontoAutorizado.Text.Trim());

                    certificado.MesAprobacion = string.IsNullOrWhiteSpace(txtFecha.Text)
                        ? null
                        : (DateTime?)DateTime.Parse(txtFecha.Text);

                    certificado.Tipo = new TipoPago { Id = int.Parse(ddlTipo.SelectedValue) };

                    // Check if we're editing an existing certificado or adding a new one
                    if (ViewState["EditingCertificadoId"] != null)
                    {
                        // We're updating an existing certificado
                        certificado.Id = (int)ViewState["EditingCertificadoId"];

                        // Use both the ID and CodigoAutorizante from ViewState for the update
                        if (ViewState["EditingAutorizanteId"] != null && ViewState["EditingCodigoAutorizante"] != null)
                        {
                            certificado.Autorizante = new Autorizante
                            {
                                Id = (int)ViewState["EditingAutorizanteId"],
                                CodigoAutorizante = ViewState["EditingCodigoAutorizante"].ToString()
                            };
                        }

                        if (certificadoNegocio.modificar(certificado))
                        {
                            lblMensaje.Text = "Certificado modificado exitosamente!";
                            lblMensaje.CssClass = "alert alert-success";

                            // Clear the editing state
                            ViewState["EditingCertificadoId"] = null;
                            ViewState["EditingAutorizanteId"] = null;
                            ViewState["EditingCodigoAutorizante"] = null;
                        }
                        else
                        {
                            lblMensaje.Text = "Hubo un problema al modificar el certificado.";
                            lblMensaje.CssClass = "alert alert-danger";
                        }
                    }
                    else
                    {
                        // We're adding a new certificado
                        // IMPORTANTE: Usamos CodigoAutorizante en lugar del Id, manteniendo el comportamiento original
                        certificado.Autorizante = new Autorizante
                        {
                            CodigoAutorizante = ddlAutorizante.SelectedItem.Text
                        };

                        if (certificadoNegocio.agregar(certificado))
                        {
                            lblMensaje.Text = "Certificado agregado exitosamente!";
                            lblMensaje.CssClass = "alert alert-success";
                        }
                        else
                        {
                            lblMensaje.Text = "Hubo un problema al agregar el certificado.";
                            lblMensaje.CssClass = "alert alert-danger";
                        }
                    }

                    // Clear fields
                    LimpiarFormulario();

                    // Reset the modal title and button text
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "ResetModalTitle",
                        "$('#modalAgregar .modal-title').text('Agregar Certificado');", true);
                    btnAgregar.Text = "Agregar";

                    // Hide the modal
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "HideModal",
                        "$('#modalAgregar').modal('hide');", true);

                    // Refresh the certificados list
                    CargarListaCertificados(null, true);
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
            txtExpediente.Text = string.Empty;
            txtMontoAutorizado.Text = string.Empty;
            txtFecha.Text = string.Empty;
            ddlAutorizante.SelectedIndex = 0;
            ddlTipo.SelectedIndex = 0;
        }

        private DataTable ObtenerTipos()
        {
            TipoPagoNegocio tipoPagNegocio = new TipoPagoNegocio();
            return tipoPagNegocio.listarddl();
        }

        private DataTable ObtenerAutorizantes()
        {
            AutorizanteNegocio autorizanteNegocio = new AutorizanteNegocio();
            Usuario usuarioLogueado = (Usuario)Session["usuario"];
            return autorizanteNegocio.listarddl(usuarioLogueado);
        }

        //private DataTable ObtenerEstadosExpedientes()
        //{
        //    DataTable dt = new DataTable();
        //    dt.Columns.Add("ID", typeof(int));
        //    dt.Columns.Add("NOMBRE", typeof(string));

        //    DataRow _1 = dt.NewRow();
        //    _1["ID"] = 0;
        //    _1["NOMBRE"] = "NO INICIADO";
        //    dt.Rows.Add(_1);

        //    DataRow _2 = dt.NewRow();
        //    _2["ID"] = 1;
        //    _2["NOMBRE"] = "EN TRAMITE";
        //    dt.Rows.Add(_2);

        //    DataRow _3 = dt.NewRow();
        //    _3["ID"] = 2;
        //    _3["NOMBRE"] = "DEVENGADO";
        //    dt.Rows.Add(_3);

        //    return dt;
        //}

        protected void dgvCertificado_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.Header)
            {
                List<Certificado> certificadosUsuarioCompleto = Session["certificadosUsuarioCompleto"] as List<Certificado>;
                if (certificadosUsuarioCompleto == null || !certificadosUsuarioCompleto.Any()) return;


                // Poblar filtro de Obra
                var cblsHeaderObra = e.Row.FindControl("cblsHeaderObra") as WebForms.CustomControls.TreeViewSearch;
                if (cblsHeaderObra != null)
                {
                    var obrasUnicas = certificadosUsuarioCompleto
                        .Where(a => a.Autorizante?.Obra != null)
                        .Select(a => new { Id = a.Autorizante.Obra.Id, Nombre = a.Autorizante.Obra.Descripcion })
                        .Distinct()
                        .OrderBy(o => o.Nombre)
                        .ToList();

                    cblsHeaderObra.DataTextField = "Nombre";
                    cblsHeaderObra.DataValueField = "Id";
                    cblsHeaderObra.DataSource = obrasUnicas;
                    cblsHeaderObra.DataBind();

                }

                // Poblar filtro de Empresa
                var cblsHeaderEmpresa = e.Row.FindControl("cblsHeaderEmpresa") as WebForms.CustomControls.TreeViewSearch;
                if (cblsHeaderEmpresa != null)
                {
                    var items = certificadosUsuarioCompleto
                        .Where(c => c != null)
                        .Select(c => c.Empresa)
                        .Distinct()
                        .OrderBy(emp => emp)
                        .Select(emp => new { Nombre = emp }) // El UserControl espera DataTextField y DataValueField
                        .ToList();
                    cblsHeaderEmpresa.DataTextField = "Nombre";
                    cblsHeaderEmpresa.DataValueField = "Nombre"; // Usar Nombre como valor también
                    cblsHeaderEmpresa.DataSource = items;
                    cblsHeaderEmpresa.DataBind();
                }

                // Poblar filtro de Código Autorizante
                var cblsHeaderCodigoAutorizante = e.Row.FindControl("cblsHeaderCodigoAutorizante") as WebForms.CustomControls.TreeViewSearch;
                if (cblsHeaderCodigoAutorizante != null)
                {
                    var items = certificadosUsuarioCompleto
                        .Where(c => c.Autorizante != null)
                        .Select(c => c.Autorizante.CodigoAutorizante)
                        .Distinct()
                        .OrderBy(cod => cod)
                        .Select(cod => new { Codigo = cod })
                        .ToList();
                    cblsHeaderCodigoAutorizante.DataTextField = "Codigo";
                    cblsHeaderCodigoAutorizante.DataValueField = "Codigo";
                    cblsHeaderCodigoAutorizante.DataSource = items;
                    cblsHeaderCodigoAutorizante.DataBind();
                }

                // Poblar filtro de Estado
                var cblsHeaderEstado = e.Row.FindControl("cblsHeaderEstado") as WebForms.CustomControls.TreeViewSearch;
                if (cblsHeaderEstado != null)
                {
                    var items = certificadosUsuarioCompleto
                        .Where(c => c != null)
                        .Select(c => c.Estado)
                        .Distinct()
                        .OrderBy(est => est)
                        .Select(est => new { Nombre = est })
                        .ToList();
                    cblsHeaderEstado.DataTextField = "Nombre";
                    cblsHeaderEstado.DataValueField = "Nombre";
                    cblsHeaderEstado.DataSource = items;
                    cblsHeaderEstado.DataBind();
                }

                // Poblar filtro de Tipo
                var cblsHeaderTipo = e.Row.FindControl("cblsHeaderTipo") as WebForms.CustomControls.TreeViewSearch;
                if (cblsHeaderTipo != null)
                {
                    var items = certificadosUsuarioCompleto
                        .Where(c => c.Tipo != null)
                        .Select(c => c.Tipo)
                        .GroupBy(t => t.Id)
                        .Select(g => g.First())
                        .OrderBy(t => t.Nombre)
                        .Select(t => new { Id = t.Id.ToString(), Nombre = t.Nombre })
                        .ToList();
                    cblsHeaderTipo.DataTextField = "Nombre";
                    cblsHeaderTipo.DataValueField = "Id";
                    cblsHeaderTipo.DataSource = items;
                    cblsHeaderTipo.DataBind();
                }

                // Poblar filtro de Mes Certificado
                var cblsHeaderMesCertificado = e.Row.FindControl("cblsHeaderMesCertificado") as WebForms.CustomControls.TreeViewSearch;
                if (cblsHeaderMesCertificado != null)
                {
                    var mesesCertificadoUnicos = certificadosUsuarioCompleto
                        .Where(c => c.MesAprobacion.HasValue)
                        .Select(c => c.MesAprobacion.Value)
                        .Distinct()
                        .OrderByDescending(d => d)
                        .Select(d => new
                        {
                            Id = d.ToString("yyyy-MM-dd"), // Formato que el UserControl puede parsear
                            Nombre = d.ToString("MMMM yyyy", new CultureInfo("es-ES")) // Para visualización si es necesario
                        })
                        .ToList();
                    cblsHeaderMesCertificado.DataTextField = "Nombre"; // El UserControl generará la jerarquía
                    cblsHeaderMesCertificado.DataValueField = "Id";    // El UserControl usará Id para el valor
                    cblsHeaderMesCertificado.DataSource = mesesCertificadoUnicos;
                    cblsHeaderMesCertificado.DataBind();
                }
            }
        }

        private void BindDropDownList()
        {// Clear existing items first
            ddlTipo.Items.Clear();
            ddlAutorizante.Items.Clear();

            // Set AppendDataBoundItems to true
            ddlTipo.AppendDataBoundItems = true;
            ddlAutorizante.AppendDataBoundItems = true;

            // Add empty items
            ddlTipo.Items.Add(new ListItem("Seleccione un tipo", ""));
            ddlAutorizante.Items.Add(new ListItem("Seleccione un autorizante", ""));

            // Bind data sources
            ddlTipo.DataSource = ObtenerTipos();
            ddlTipo.DataTextField = "Nombre";
            ddlTipo.DataValueField = "Id";
            ddlTipo.DataBind();

            ddlAutorizante.DataSource = ObtenerAutorizantes();
            ddlAutorizante.DataTextField = "Nombre";
            ddlAutorizante.DataValueField = "Id";
            ddlAutorizante.DataBind();
        }
        //private DataTable ObtenerEmpresas()
        //{
        //    EmpresaNegocio empresaNegocio = new EmpresaNegocio();
        //    return empresaNegocio.listarddl();
        //}

        protected void txtExpediente_TextChanged(object sender, EventArgs e)
        {
            // Identifica el TextBox modificado
            TextBox txtExpediente = (TextBox)sender;
            GridViewRow row = (GridViewRow)txtExpediente.NamingContainer;

            // Obtiene la clave del registro desde DataKeyNames
            int id = int.Parse(dgvCertificado.DataKeys[row.RowIndex].Value.ToString());

            // Nuevo valor del expediente
            string nuevoExpediente = txtExpediente.Text;

            // Actualiza en la base de datos
            try
            {
                // Llama al método del negocio para actualizar el expediente
                CertificadoNegocio negocio = new CertificadoNegocio();
                negocio.ActualizarExpediente(id, nuevoExpediente);

                // Mensaje de éxito o retroalimentación opcional
                lblMensaje.Text = "Expediente actualizado correctamente.";
                CargarListaCertificados(null, true);
                CalcularSubtotal();

            }
            catch (Exception ex)
            {
                // Manejo de errores
                lblMensaje.Text = "Error al actualizar el expediente: " + ex.Message;
            }
        }
        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            string filtro = txtBuscar.Text.Trim();
            CargarListaCertificados(filtro);
        }

        protected void BtnClearFilters_Click(object sender, EventArgs e)
        {
            txtBuscar.Text = string.Empty;

            WebForms.CustomControls.TreeViewSearch.ClearAllFiltersOnPage(this.Page);

            CargarListaCertificados();
        }

    }
}