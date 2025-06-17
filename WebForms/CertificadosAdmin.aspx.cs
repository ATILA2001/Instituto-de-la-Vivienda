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
    public partial class CertificadosAdmin : System.Web.UI.Page
    {
        CertificadoNegocio negocio = new CertificadoNegocio();
        CalculoRedeterminacionNegocio calculoRedeterminacionNegocio = new CalculoRedeterminacionNegocio();

        public void OnAcceptChanges(object sender, EventArgs e)
        {
            CargarListaCertificados();
        }

        protected void btnExportarExcel_Click(object sender, EventArgs e)
        {
            try
            {
                // Obtener datos completos
                List<Certificado> certificados = Session["certificadosCompleto"] as List<Certificado>
                    ?? calculoRedeterminacionNegocio.listarCertReliq();

                if (certificados != null && certificados.Any())
                {
                    // Definir mapeo de columnas (encabezado de columna -> ruta de propiedad)
                    var mapeoColumnas = new Dictionary<string, string>
            {
                { "Obra", "Autorizante.Obra.Descripcion" },
                { "Contrata", "Autorizante.Obra.Contrata.Nombre" },
                { "Detalle", "Autorizante.Detalle" },
                { "Empresa", "Empresa" },
                { "Código Autorizante", "Autorizante.CodigoAutorizante" },
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
                { "Barrio", "Autorizante.Obra.Barrio.Nombre" },
                { "Proyecto", "Autorizante.Obra.Proyecto.Proyecto" },
                { "Línea", "Autorizante.Obra.LineaGestion.Nombre" }
            };

                    // Exportar usando el método genérico
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
                CargarListaCertificados(null, true);

                // Validación adicional para evitar pasar null a BindDropDownList
                var certificados = Session["certificadosCompleto"] as List<Certificado>;
                if (certificados != null && certificados.Any())
                {
                    BindDropDownList(certificados);
                }
                else
                {
                    lblMensaje.Text = "No se encontraron certificados para mostrar.";
                    lblMensaje.CssClass = "alert alert-warning";
                    System.Diagnostics.Debug.WriteLine("ATENCIÓN: La lista de certificados está vacía o nula.");
                }
            }
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

        private void CargarListaCertificados(string filtro = null, bool forzarRecargaCompleta = false)
        {
            try
            {
                // 1. Obtener valores de filtros de cabecera
                List<string> selectedHeaderArea = new List<string>();
                List<string> selectedHeaderObra = new List<string>();
                List<string> selectedHeaderBarrio = new List<string>();
                List<string> selectedHeaderProyecto = new List<string>();
                List<string> selectedHeaderEmpresa = new List<string>();
                List<string> selectedHeaderCodigoAutorizante = new List<string>();
                List<string> selectedHeaderEstado = new List<string>();
                List<string> selectedHeaderTipo = new List<string>();
                List<string> selectedHeaderMesCertificado = new List<string>();
                List<string> selectedHeaderLinea = new List<string>();

                if (dgvCertificado.HeaderRow != null)
                {
                    var cblsHeaderAreaControl = dgvCertificado.HeaderRow.FindControl("cblsHeaderArea") as WebForms.CustomControls.TreeViewSearch;
                    if (cblsHeaderAreaControl != null)
                    {
                        selectedHeaderArea = cblsHeaderAreaControl.SelectedValues;
                    }

                    var cblsHeaderObraControl = dgvCertificado.HeaderRow.FindControl("cblsHeaderObra") as CustomControls.TreeViewSearch;
                    if (cblsHeaderObraControl != null)
                    {
                        selectedHeaderObra = cblsHeaderObraControl.SelectedValues;
                    }

                    var cblsHeaderBarrioControl = dgvCertificado.HeaderRow.FindControl("cblsHeaderBarrio") as WebForms.CustomControls.TreeViewSearch;
                    if (cblsHeaderBarrioControl != null)
                    {
                        selectedHeaderBarrio = cblsHeaderBarrioControl.SelectedValues;
                    }

                    var cblsHeaderProyectoControl = dgvCertificado.HeaderRow.FindControl("cblsHeaderProyecto") as WebForms.CustomControls.TreeViewSearch;
                    if (cblsHeaderProyectoControl != null)
                    {
                        selectedHeaderProyecto = cblsHeaderProyectoControl.SelectedValues;
                    }
                    
                    var cblsHeaderEmpresaControl = dgvCertificado.HeaderRow.FindControl("cblsHeaderEmpresa") as WebForms.CustomControls.TreeViewSearch;
                    if (cblsHeaderEmpresaControl != null)
                    {
                        selectedHeaderEmpresa = cblsHeaderEmpresaControl.SelectedValues;
                    }

                    var cblsHeaderCodigoAutorizanteControl = dgvCertificado.HeaderRow.FindControl("cblsHeaderCodigoAutorizante") as WebForms.CustomControls.TreeViewSearch;
                    if (cblsHeaderCodigoAutorizanteControl != null)
                    {
                        selectedHeaderCodigoAutorizante = cblsHeaderCodigoAutorizanteControl.SelectedValues;
                    }

                    var cblsHeaderEstadoControl = dgvCertificado.HeaderRow.FindControl("cblsHeaderEstado") as WebForms.CustomControls.TreeViewSearch;
                    if (cblsHeaderEstadoControl != null)
                    {
                        selectedHeaderEstado = cblsHeaderEstadoControl.SelectedValues;
                    }

                    var cblsHeaderTipoControl = dgvCertificado.HeaderRow.FindControl("cblsHeaderTipo") as WebForms.CustomControls.TreeViewSearch;
                    if (cblsHeaderTipoControl != null)
                    {
                        selectedHeaderTipo = cblsHeaderTipoControl.SelectedValues;
                    }

                    var cblsHeaderMesCertificadoControl = dgvCertificado.HeaderRow.FindControl("cblsHeaderMesCertificado") as WebForms.CustomControls.TreeViewSearch;
                    if (cblsHeaderMesCertificadoControl != null)
                    {
                        selectedHeaderMesCertificado = cblsHeaderMesCertificadoControl.SelectedValues;
                    }

                    var cblsHeaderLineaControl = dgvCertificado.HeaderRow.FindControl("cblsHeaderLinea") as WebForms.CustomControls.TreeViewSearch;
                    if (cblsHeaderLineaControl != null)
                    {
                        selectedHeaderLinea = cblsHeaderLineaControl.SelectedValues;
                    }
                }

                // 2. Obtener lista completa de certificados
                //List<Certificado> listaCompleta = calculoRedeterminacionNegocio.listarCertReliq();

                //Session["certificadosCompleto"] = listaCompleta;

                List<Certificado> listaCompleta;

                //if (forzarRecargaCompleta || Session["certificadosCompleto"] == null)
                //{
                //    listaCompleta = calculoRedeterminacionNegocio.listarCertReliq();
                //    Session["certificadosCompleto"] = listaCompleta;
                //}
                //else
                //{
                //    listaCompleta = (List<Certificado>)Session["certificadosCompleto"];
                //}

                if (forzarRecargaCompleta || Session["certificadosCompleto"] == null)
                {
                    listaCompleta = calculoRedeterminacionNegocio.listarCertReliq();
                    if (listaCompleta == null || !listaCompleta.Any())
                    {
                        lblMensaje.Text = "No se encontraron certificados en la base de datos.";
                        lblMensaje.CssClass = "alert alert-warning";
                        System.Diagnostics.Debug.WriteLine("ATENCIÓN: listarCertReliq() devolvió una lista vacía o nula.");
                    }
                    Session["certificadosCompleto"] = listaCompleta;
                }
                else
                {
                    listaCompleta = (List<Certificado>)Session["certificadosCompleto"];
                }

                if (listaCompleta == null || !listaCompleta.Any())
                {
                    dgvCertificado.DataSource = null;
                    dgvCertificado.DataBind();
                    return;
                }

                IEnumerable<Certificado> listaFiltrada = listaCompleta;

                // 3. Aplicar filtro de texto general (si se proporciona 'filtro' o se usa txtBuscar.Text)
                string filtroTextoGeneral = string.IsNullOrEmpty(filtro) ? txtBuscar.Text.Trim().ToUpper() : filtro.Trim().ToUpper();
                if (!string.IsNullOrEmpty(filtroTextoGeneral))
                {
                    listaFiltrada = listaFiltrada.Where(c =>
                        (c.Autorizante?.Obra?.Area?.Nombre?.ToUpper().Contains(filtroTextoGeneral) ?? false) ||
                        (c.Autorizante?.Obra?.Contrata?.Nombre?.ToUpper().Contains(filtroTextoGeneral) ?? false) ||
                        (c.Autorizante?.Obra?.Descripcion?.ToUpper().Contains(filtroTextoGeneral) ?? false) ||
                        (c.Autorizante?.Obra?.Barrio?.Nombre?.ToUpper().Contains(filtroTextoGeneral) ?? false) ||
                        (c.Autorizante?.Obra?.Proyecto?.Proyecto?.ToUpper().Contains(filtroTextoGeneral) ?? false) ||
                        (c.Empresa?.ToUpper().Contains(filtroTextoGeneral) ?? false) ||
                        (c.Autorizante?.CodigoAutorizante?.ToUpper().Contains(filtroTextoGeneral) ?? false) ||
                        (c.ExpedientePago?.ToUpper().Contains(filtroTextoGeneral) ?? false) ||
                        (c.Estado?.ToUpper().Contains(filtroTextoGeneral) ?? false) ||
                        (c.Tipo?.Nombre?.ToUpper().Contains(filtroTextoGeneral) ?? false) ||
                        (c.Sigaf.ToString().ToUpper().Contains(filtroTextoGeneral)) ||
                        (c.BuzonSade?.ToUpper().Contains(filtroTextoGeneral) ?? false)
                    );
                }

                // 4. Aplicar filtros de cabecera
                if (selectedHeaderArea != null && selectedHeaderArea.Any())
                {
                    listaFiltrada = listaFiltrada.Where(c => c.Autorizante?.Obra?.Area != null && selectedHeaderArea.Contains(c.Autorizante.Obra.Area.Nombre));
                }

                if (selectedHeaderObra.Any())
                    listaFiltrada = listaFiltrada.Where(m => m.Autorizante?.Obra != null && selectedHeaderObra.Contains(m.Autorizante.Obra.Id.ToString()));


                if (selectedHeaderBarrio != null && selectedHeaderBarrio.Any())
                {
                    listaFiltrada = listaFiltrada.Where(c => c.Autorizante?.Obra?.Barrio != null && selectedHeaderBarrio.Contains(c.Autorizante.Obra.Barrio.Id.ToString()));
                }
                
                if (selectedHeaderProyecto != null && selectedHeaderProyecto.Any())
                {
                    listaFiltrada = listaFiltrada.Where(c => c.Autorizante?.Obra?.Proyecto != null && selectedHeaderProyecto.Contains(c.Autorizante.Obra.Proyecto.Id.ToString()));
                }

                if (selectedHeaderEmpresa != null && selectedHeaderEmpresa.Any())
                {
                    listaFiltrada = listaFiltrada.Where(c => c.Autorizante?.Obra?.Empresa != null && selectedHeaderEmpresa.Contains(c.Autorizante.Obra.Empresa.Id.ToString()));
                }

                if (selectedHeaderCodigoAutorizante != null && selectedHeaderCodigoAutorizante.Any())
                {
                    listaFiltrada = listaFiltrada.Where(c => c.Autorizante?.CodigoAutorizante != null && selectedHeaderCodigoAutorizante.Contains(c.Autorizante.CodigoAutorizante));
                }

                if (selectedHeaderEstado != null && selectedHeaderEstado.Any())
                {
                    listaFiltrada = listaFiltrada.Where(c => c.Estado != null && selectedHeaderEstado.Contains(c.Estado));
                }

                if (selectedHeaderTipo != null && selectedHeaderTipo.Any())
                {
                    listaFiltrada = listaFiltrada.Where(c => c.Tipo != null && selectedHeaderTipo.Contains(c.Tipo.Id.ToString()));
                }

                if (selectedHeaderLinea != null && selectedHeaderLinea.Any())
                {
                    listaFiltrada = listaFiltrada.Where(c => c.Autorizante?.Obra?.LineaGestion != null && selectedHeaderLinea.Contains(c.Autorizante.Obra.LineaGestion.Id.ToString()));
                }

                if (selectedHeaderMesCertificado != null && selectedHeaderMesCertificado.Any())
                {
                    List<DateTime> fechasSeleccionadasDia = new List<DateTime>();
                    List<Tuple<int, int>> mesesAnioSeleccionados = new List<Tuple<int, int>>(); // Año, Mes
                    List<int> aniosSeleccionados = new List<int>();

                    foreach (var sel in selectedHeaderMesCertificado)
                    {
                        if (DateTime.TryParseExact(sel, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dtDia))
                        {
                            fechasSeleccionadasDia.Add(dtDia.Date);
                        }
                        else if (sel.Contains("_") && !sel.StartsWith("year_"))
                        {
                            var parts = sel.Split('_');
                            if (parts.Length == 2 && int.TryParse(parts[0], out int year) && int.TryParse(parts[1], out int month))
                            {
                                mesesAnioSeleccionados.Add(Tuple.Create(year, month));
                            }
                        }
                        else if (sel.StartsWith("year_"))
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

                // 5. Actualizar GridView y otros elementos
                List<Certificado> resultadoFinal = listaFiltrada.ToList();
                Session["listaCertificado"] = resultadoFinal;

                dgvCertificado.DataSource = resultadoFinal;
                dgvCertificado.DataBind();
                CalcularSubtotal();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = "Error al cargar los certificados: " + ex.Message;
                System.Diagnostics.Debug.WriteLine($"Error en CargarListaCertificados: {ex.ToString()}");
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
                    // Set button text to "Actualizar"
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
        protected void dgvCertificado_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {

            try
            {
                var id = Convert.ToInt32(dgvCertificado.DataKeys[e.RowIndex].Value);
                if (negocio.eliminar(id))
                {
                    lblMensaje.Text = "Certificado eliminado correctamente.";
                    lblMensaje.CssClass = "alert alert-success";
                    CargarListaCertificados(null,true); 
                    CalcularSubtotal();
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al eliminar el certificado: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        protected void dgvCertificado_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.Header) 
            {
                List<Certificado> certificadosCompleto = (List<Certificado>)Session["certificadosCompleto"];//calculoRedeterminacionNegocio.listarCertReliq();

                var cblsHeaderArea = e.Row.FindControl("cblsHeaderArea") as CustomControls.TreeViewSearch;
                var cblsHeaderObra = e.Row.FindControl("cblsHeaderObra") as CustomControls.TreeViewSearch;
                var cblsHeaderBarrio = e.Row.FindControl("cblsHeaderBarrio") as CustomControls.TreeViewSearch;
                var cblsHeaderProyecto = e.Row.FindControl("cblsHeaderProyecto") as CustomControls.TreeViewSearch;
                var cblsHeaderEmpresa = e.Row.FindControl("cblsHeaderEmpresa") as CustomControls.TreeViewSearch;
                var cblsHeaderCodigoAutorizante = e.Row.FindControl("cblsHeaderCodigoAutorizante") as CustomControls.TreeViewSearch;
                var cblsHeaderEstado = e.Row.FindControl("cblsHeaderEstado") as CustomControls.TreeViewSearch;
                var cblsHeaderTipo = e.Row.FindControl("cblsHeaderTipo") as CustomControls.TreeViewSearch;
                var cblsHeaderMesCertificado = e.Row.FindControl("cblsHeaderMesCertificado") as CustomControls.TreeViewSearch;
                var cblsHeaderLinea = e.Row.FindControl("cblsHeaderLinea") as CustomControls.TreeViewSearch;

                if (cblsHeaderArea != null)
                {
                    var areasUnicas = certificadosCompleto
                        .Where(c => c.Autorizante?.Obra?.Area != null)
                        .Select(c => new { Nombre = c.Autorizante.Obra.Area.Nombre, Id = c.Autorizante.Obra.Area.Nombre })
                        .Distinct()
                        .OrderBy(a => a.Nombre)
                        .ToList();

                    cblsHeaderArea.DataSource = areasUnicas;
                    cblsHeaderArea.DataBind();
                }

                //var cblsHeaderObra = e.Row.FindControl("cblsHeaderObra") as WebForms.CustomControls.TreeViewSearch;
                if (cblsHeaderObra != null)
                {
                    var obrasUnicas = certificadosCompleto
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

                if (cblsHeaderBarrio != null)
                {
                    var barriosUnicos = certificadosCompleto
                        .Where(c => c.Autorizante?.Obra?.Barrio != null)
                        .Select(c => new { Nombre = c.Autorizante.Obra.Barrio.Nombre, Id = c.Autorizante.Obra.Barrio.Id })
                        .Distinct()
                        .OrderBy(a => a.Nombre)
                        .ToList();

                    cblsHeaderBarrio.DataSource = barriosUnicos;
                    cblsHeaderBarrio.DataBind();
                }
                
                if (cblsHeaderProyecto != null)
                {
                    var proyectosUnicos = certificadosCompleto
                        .Where(c => c.Autorizante?.Obra?.Proyecto != null)
                        .Select(c => new { Nombre = c.Autorizante.Obra.Proyecto.Proyecto, Id = c.Autorizante.Obra.Proyecto.Id  })
                        .Distinct()
                        .OrderBy(a => a.Nombre)
                        .ToList();

                    cblsHeaderProyecto.DataSource = proyectosUnicos;
                    cblsHeaderProyecto.DataBind();
                }

                if (cblsHeaderEmpresa != null)
                {
                    var empresasUnicas = certificadosCompleto
                        .Where(c => c.Autorizante?.Obra?.Empresa != null)
                        .Select(c => new { Nombre = c.Autorizante.Obra.Empresa.Nombre, Id = c.Autorizante.Obra.Empresa.Id })
                        .Distinct()
                        .OrderBy(a => a.Nombre)
                        .ToList();

                    cblsHeaderEmpresa.DataSource = empresasUnicas;
                    cblsHeaderEmpresa.DataBind();
                }

                if (cblsHeaderCodigoAutorizante != null)
                {
                    var codigoAutorizantesUnicos = certificadosCompleto
                        .Where(c => c.Autorizante?.CodigoAutorizante != null)
                        .Select(c => new { Nombre = c.Autorizante.CodigoAutorizante, Id = c.Autorizante.CodigoAutorizante })
                        .Distinct()
                        .OrderBy(a => a.Nombre)
                        .ToList();

                    cblsHeaderCodigoAutorizante.DataSource = codigoAutorizantesUnicos;
                    cblsHeaderCodigoAutorizante.DataBind();
                }

                if (cblsHeaderEstado != null)
                {
                    var estadoUnicos = certificadosCompleto
                        .Where(c => c.Estado != null)
                        .Select(c => new { Nombre = c.Estado, Id = c.Estado })
                        .Distinct()
                        .OrderBy(a => a.Nombre)
                        .ToList();

                    cblsHeaderEstado.DataSource = estadoUnicos;
                    cblsHeaderEstado.DataBind();
                }

                if (cblsHeaderTipo != null)
                {
                    var tipoUnicos = certificadosCompleto
                        .Where(c => c.Tipo != null)
                        .Select(c => new { Nombre = c.Tipo.Nombre, Id = c.Tipo.Id })
                        .Distinct()
                        .OrderBy(a => a.Nombre)
                        .ToList();

                    cblsHeaderTipo.DataSource = tipoUnicos;
                    cblsHeaderTipo.DataBind();
                }

                if (cblsHeaderMesCertificado != null)
                {
                    var mesesCertificadoUnicos = certificadosCompleto
                        .Where(c => c.MesAprobacion.HasValue)
                        .Select(c => c.MesAprobacion.Value) // Obtener solo las fechas
                        .Distinct()
                        .OrderByDescending(d => d) // Ordenar por fecha, más recientes primero
                        .Select(d => new {
                            // El UserControl usará 'Id' para el valor del nodo y para parsear la fecha.
                            // El UserControl generará la jerarquía Año/Mes/Día.
                            Id = d.ToString("yyyy-MM-dd"), // Formato que el UserControl puede parsear a DateTime
                                                           // 'Nombre' se usa si el UserControl no detecta que son fechas o como fallback.
                                                           // Para fechas, el UserControl suele generar su propio texto (Año, Mes, Día).
                            Nombre = d.ToString("MMMM yyyy", new CultureInfo("es-ES"))
                        })
                        .ToList();

                    cblsHeaderMesCertificado.DataSource = mesesCertificadoUnicos;
                    cblsHeaderMesCertificado.DataBind();

                }

                if (cblsHeaderLinea != null)
                {
                    var lineaUnicos = certificadosCompleto
                        .Where(c => c.Autorizante?.Obra?.LineaGestion != null)
                        .Select(c => c.Autorizante.Obra.LineaGestion)
                        .GroupBy(lg => lg.Id) // Agrupar por ID para obtener líneas únicas
                        .Select(g => g.First()) // Tomar la primera de cada grupo (línea única)
                        .Select(lg => new { Id = lg.Id.ToString(), Nombre = lg.Nombre }) // Proyectar a un objeto anónimo
                        .OrderBy(lg => lg.Nombre)
                        .ToList();

                    cblsHeaderLinea.DataSource = lineaUnicos;
                    cblsHeaderLinea.DataBind();
                }

            }
        }
 
        private DataTable ObtenerTipos()
        {
            TipoPagoNegocio tipoPagNegocio = new TipoPagoNegocio();
            return tipoPagNegocio.listarddl();
        }

        private DataTable ObtenerAutorizantes()
        {
            AutorizanteNegocio autorizanteNegocio = new AutorizanteNegocio();

            return autorizanteNegocio.listarddl();
        }

        /// <summary>
        /// Populates dropdown lists and TreeViewSearch controls used for filtering.
        /// The date filter (cblFecha) is populated based on the distinct MesAprobacion
        /// values found in the provided list of certificates.
        /// </summary>
        /// <param name="certificados">A list of Certificado objects used to extract distinct dates for the date filter.</param>
        private void BindDropDownList(List<Certificado> certificados)
        {
            ddlTipo.DataSource = ObtenerTipos();
            ddlTipo.DataTextField = "Nombre";
            ddlTipo.DataValueField = "Id";
            ddlTipo.DataBind();

            ddlAutorizante.DataSource = ObtenerAutorizantes();
            ddlAutorizante.DataTextField = "Nombre";
            ddlAutorizante.DataValueField = "Id";
            ddlAutorizante.DataBind();
        }


        protected void txtExpediente_TextChanged(object sender, EventArgs e)
        {
            try
            {
                // Identifica el TextBox modificado
                TextBox txtExpediente = (TextBox)sender;
                GridViewRow row = (GridViewRow)txtExpediente.NamingContainer;

                // Obtiene el índice de la fila en la GridView - esto es crucial
                int rowIndex = row.RowIndex;

                // Obtiene la clave del registro desde DataKeyNames
                int id = int.Parse(dgvCertificado.DataKeys[rowIndex].Value.ToString());

                // Obtenemos directamente el certificado de la lista actual por índice de fila
                // en lugar de buscar por ID
                List<Certificado> certificados = (List<Certificado>)Session["listaCertificado"];

                // Esto garantiza que obtenemos el certificado exacto que se muestra en la GridView
                Certificado certificado = certificados[rowIndex];

                // Nuevo valor del expediente
                string nuevoExpediente = txtExpediente.Text;

                if (certificado != null)
                {
                    // Verificamos si es una reliquidación
                    bool esReliquidacion = certificado.Tipo != null &&
                        (certificado.Tipo.Id == 2 ||
                         certificado.Tipo.Nombre.ToUpper().Contains("RELIQUIDACION") ||
                         certificado.Tipo.Nombre.ToUpper().Contains("REDETERMINACION"));

                    // Actualiza el expediente en el certificado en memoria
                    certificado.ExpedientePago = nuevoExpediente;

                    // Si tiene ID válido en la base de datos, actualizar también allí
                    if (certificado.Id > 0)
                    {
                        CertificadoNegocio negocio = new CertificadoNegocio();
                        negocio.ActualizarExpediente(certificado.Id, nuevoExpediente);
                    }

                    // Actualizar el estado según la lógica requerida
                    if (string.IsNullOrEmpty(nuevoExpediente))
                    {
                        certificado.Estado = "NO INICIADO";
                    }
                    else if (!certificado.Sigaf.HasValue || certificado.Sigaf == 0)
                    {
                        certificado.Estado = "EN TRAMITE";
                    }
                    else
                    {
                        certificado.Estado = "DEVENGADO";
                    }

                    // Si es un certificado de reliquidación, guardar en EXPEDIENTES_RELIQ
                    if (esReliquidacion)
                    {
                        if (certificado.MesAprobacion.HasValue &&
                            certificado.Autorizante != null &&
                            !string.IsNullOrEmpty(certificado.Autorizante.CodigoAutorizante))
                        {
                            try
                            {
                                string codigoRedet = certificado.Autorizante.CodigoAutorizante;

                                ExpedienteReliqNegocio expedienteReliqNegocio = new ExpedienteReliqNegocio();
                                expedienteReliqNegocio.GuardarOActualizar(
                                    codigoRedet,
                                    certificado.MesAprobacion.Value,
                                    nuevoExpediente);

                                // Log para depuración
                                System.Diagnostics.Debug.WriteLine($"Guardando expediente {nuevoExpediente} para redeterminación {codigoRedet}, mes {certificado.MesAprobacion.Value:MMM yyyy}");
                            }
                            catch (Exception exReliq)
                            {
                                // Mostrar error completo para depuración
                                System.Diagnostics.Debug.WriteLine($"Error al actualizar expediente reliquidación: {exReliq.Message}");
                                System.Diagnostics.Debug.WriteLine($"Stack Trace: {exReliq.StackTrace}");
                            }
                        }
                    }

                    // Actualizar la lista en Session para que los cambios persistan
                    Session["listaCertificado"] = certificados;
                    CargarListaCertificados(null, true);

                    // Mensaje de éxito
                    lblMensaje.Text = "Expediente actualizado correctamente.";
                    lblMensaje.CssClass = "alert alert-success";

                    CalcularSubtotal();
                }
                else
                {
                    lblMensaje.Text = "No se pudo identificar el certificado. Por favor, inténtelo de nuevo.";
                    lblMensaje.CssClass = "alert alert-warning";
                }
            }
            catch (Exception ex)
            {
                // Manejo de errores con más detalles
                lblMensaje.Text = "Error al actualizar el expediente: " + ex.Message;
                lblMensaje.CssClass = "alert alert-danger";
                System.Diagnostics.Debug.WriteLine($"Error completo: {ex}");
            }
        }
        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            string filtro = txtBuscar.Text.Trim();
            CargarListaCertificados(filtro);
        }

        
        private void CalcularSubtotal()
        {
            decimal subtotal = 0;

            foreach (GridViewRow row in dgvCertificado.Rows)
            {
                var cellValue = row.Cells[11].Text;
                if (decimal.TryParse(cellValue, System.Globalization.NumberStyles.Currency, null, out decimal monto))
                {
                    subtotal += monto;
                }
            }

            txtSubtotal.Text = subtotal.ToString("C");
        }


        protected void BtnClearFilters_Click(object sender, EventArgs e)
        {
            txtBuscar.Text = string.Empty;

            WebForms.CustomControls.TreeViewSearch.ClearAllFiltersOnPage(this.Page);

            CargarListaCertificados();
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

    }
}