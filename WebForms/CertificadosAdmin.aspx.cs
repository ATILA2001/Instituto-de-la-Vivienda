using Dominio;
using Negocio;
using Negocio.Negocio;
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

        protected void Page_Init(object sender, EventArgs e) 
        {
           cblArea.AcceptChanges += OnAcceptChanges;
           cblBarrio.AcceptChanges += OnAcceptChanges;
           cblProyecto.AcceptChanges += OnAcceptChanges;
           cblEmpresa.AcceptChanges += OnAcceptChanges;
           cblAutorizante.AcceptChanges += OnAcceptChanges;
           cblTipo.AcceptChanges += OnAcceptChanges;
           cblFecha.AcceptChanges += OnAcceptChanges;
           cblEstadoExpediente.AcceptChanges += OnAcceptChanges;
            cblLinea.AcceptChanges += OnAcceptChanges;
        }

        private void OnAcceptChanges(object sender, EventArgs e)
        {
           CargarListaCertificados();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                List<Certificado> listaCertificados = negocio.listarFiltroAdmin();
                BindDropDownList(listaCertificados);
                CargarListaCertificados();
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
                    CargarListaCertificados();
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

        private DataTable ObtenerAreas()
        {
            AreaNegocio areaNegocio = new AreaNegocio();
            return areaNegocio.listarddl();
        }

        private void CargarListaCertificados(string filtro = null)
        {
            try
            {
                var selectedAreas = cblArea.SelectedValues;
                var selectedBarrios = cblBarrio.SelectedValues;
                var selectedProyectos = cblProyecto.SelectedValues;
                var selectedEmpresas = cblEmpresa.SelectedValues;
                var selectedAutorizantes = cblAutorizante.SelectedValues;
                var selectedTipos = cblTipo.SelectedValues;
                var selectedFechas = cblFecha.SelectedValues;
                var selectedLineas = cblLinea.SelectedValues;
                var selectedEstadoExpedientes = cblEstadoExpediente.SelectedValues;


                // Obtenemos la lista completa.
                List<Certificado> listaCompleta = calculoRedeterminacionNegocio.listarCertReliq();

                // Creamos lista secundaria para filtrar con LINQ.
                IEnumerable<Certificado> listaFiltrada = listaCompleta;

                // Filtro de texto general
                if (!string.IsNullOrEmpty(filtro))
                {
                    string filtroLower = filtro.ToLower();
                    listaFiltrada = listaFiltrada.Where(c =>
                        (c.Autorizante?.CodigoAutorizante?.ToLower().Contains(filtroLower) ?? false) ||
                        (c.Autorizante?.Obra?.Descripcion?.ToLower().Contains(filtroLower) ?? false) ||
                        (c.ExpedientePago?.ToLower().Contains(filtroLower) ?? false) ||
                        (c.Autorizante?.Obra?.Empresa?.Nombre?.ToLower().Contains(filtroLower) ?? false) ||
                        (c.Tipo?.Nombre?.ToLower().Contains(filtroLower) ?? false)
                    );
                }

                // Filtros de CheckBoxListSearch.
                if (selectedAreas != null && selectedAreas.Any())
                {
                    // Convertir IDs de string a int
                    var areaIdsInt = selectedAreas.Select(int.Parse).ToList();
                    listaFiltrada = listaFiltrada.Where(c => c.Autorizante?.Obra?.Area != null && areaIdsInt.Contains(c.Autorizante.Obra.Area.Id));
                }

                if (selectedBarrios != null && selectedBarrios.Any())
                {
                    // Convertir IDs de string a int
                    var barrioIdsInt = selectedBarrios.Select(int.Parse).ToList();
                    listaFiltrada = listaFiltrada.Where(c => c.Autorizante?.Obra?.Barrio != null && barrioIdsInt.Contains(c.Autorizante.Obra.Barrio.Id));
                }

                //if (selectedProyectos != null && selectedProyectos.Any())
                //{
                //    var proyectoIdsInt = selectedProyectos.Select(int.Parse).ToList();
                //    listaFiltrada = listaFiltrada.Where(c => c.Autorizante?.Obra?.Proyecto != null && proyectoIdsInt.Contains(c.Autorizante.Obra.Proyecto.Id));
                //}
                if (selectedProyectos != null && selectedProyectos.Any())
                {
                    listaFiltrada = listaFiltrada.Where(c =>
                        c.Autorizante?.Obra?.Proyecto != null &&
                        !string.IsNullOrEmpty(c.Autorizante.Obra.Proyecto.Proyecto) &&
                        selectedProyectos.Contains(c.Autorizante.Obra.Proyecto.Proyecto)
                    );
                }


                if (selectedEmpresas != null && selectedEmpresas.Any())
                {
                    // Convertir IDs de string a int
                    var empresaIdsInt = selectedEmpresas.Select(int.Parse).ToList();
                    listaFiltrada = listaFiltrada.Where(c => c.Autorizante?.Obra?.Empresa != null && empresaIdsInt.Contains(c.Autorizante.Obra.Empresa.Id));
                }

                if (selectedAutorizantes != null && selectedAutorizantes.Any())
                {
                    // Convertir IDs seleccionados (string) a int
                    var autorizanteIdsInt = selectedAutorizantes.Select(int.Parse).ToList();
                    // Comparar Autorizante.Id (int)
                    listaFiltrada = listaFiltrada.Where(c => c.Autorizante != null && autorizanteIdsInt.Contains(c.Autorizante.Id));
                }


                if (selectedTipos != null && selectedTipos.Any())
                {
                    // Convertir IDs de string a int
                    var tipoIdsInt = selectedTipos.Select(int.Parse).ToList();
                    listaFiltrada = listaFiltrada.Where(c => c.Tipo != null && tipoIdsInt.Contains(c.Tipo.Id));
                }

                if (selectedFechas != null && selectedFechas.Any())
                {
                    // Convertir strings "yyyy-MM-dd" a DateTime.Date
                    var fechasDate = selectedFechas
                        .Select(fs => DateTime.ParseExact(fs, "yyyy-MM-dd", CultureInfo.InvariantCulture).Date)
                        .ToList();
                    listaFiltrada = listaFiltrada.Where(c => c.MesAprobacion.HasValue && fechasDate.Contains(c.MesAprobacion.Value.Date));
                }

                if (selectedLineas != null && selectedLineas.Any())
                {
                    var lineaIdsInt = selectedLineas.Select(int.Parse).ToList();
                    listaFiltrada = listaFiltrada.Where(c => c.Autorizante?.Obra?.LineaGestion != null && lineaIdsInt.Contains(c.Autorizante.Obra.LineaGestion.Id));
                }

                if (selectedEstadoExpedientes != null && selectedEstadoExpedientes.Any())
                {
                    // Mapear los IDs ("0", "1", "2") a nombres
                    var selectedEstadoNombres = new List<string>();
                    if (selectedEstadoExpedientes.Contains("0")) selectedEstadoNombres.Add("NO INICIADO");
                    if (selectedEstadoExpedientes.Contains("1")) selectedEstadoNombres.Add("EN TRAMITE");
                    if (selectedEstadoExpedientes.Contains("2")) selectedEstadoNombres.Add("DEVENGADO");
                    selectedEstadoNombres.Add("REDETERMINADO"); // Incluir siempre

                    // Comparar Certificado.Estado (string) con nombres mapeados (insensible a mayúsculas)
                    listaFiltrada = listaFiltrada.Where(c => !string.IsNullOrEmpty(c.Estado) &&
                                                             selectedEstadoNombres.Contains(c.Estado.ToUpper()));
                }


                // Guardamos y enlazamos la lista filtrada.
                List<Certificado> resultadoFinal = listaFiltrada.ToList();
                Session["listaCertificado"] = resultadoFinal;

                dgvCertificado.DataSource = resultadoFinal;
                dgvCertificado.DataBind();
                CalcularSubtotal();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cargar/filtrar los Certificados: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
                dgvCertificado.DataSource = null;
                dgvCertificado.DataBind();
                txtSubtotal.Text = 0.ToString("C");
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
                    CargarListaCertificados(); 
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
        private DataTable ObtenerTiposFiltro()
        {
            TipoPagoNegocio tipoPagNegocio = new TipoPagoNegocio();
            return tipoPagNegocio.listarddl();
        }

        private DataTable ObtenerAutorizantesFiltro()
        {
            AutorizanteNegocio autorizanteNegocio = new AutorizanteNegocio();
            return autorizanteNegocio.listarddl();
        }
 
        protected void ddlAutorizanteFiltro_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarListaCertificados();
            CalcularSubtotal();
        }
        protected void ddlTipoFiltro_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarListaCertificados();
            CalcularSubtotal();
        }
        protected void ddlEmpresa_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarListaCertificados();
            CalcularSubtotal();
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
        private DataTable ObtenerProyectos()
        { 
            BdProyectoNegocio proyectoNegocio = new BdProyectoNegocio();

            return proyectoNegocio.listarddl();
        }

        private DataTable ObtenerBarrios()
        {
            BarrioNegocio barrioNegocio = new BarrioNegocio();

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

        /// <summary>
        /// Populates dropdown lists and CheckBoxListSearch controls used for filtering.
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

            cblTipo.DataSource = ObtenerTipos();
            cblTipo.DataTextField = "Nombre";
            cblTipo.DataValueField = "Id";
            cblTipo.DataBind();

            cblBarrio.DataSource = ObtenerBarrios();
            cblBarrio.DataTextField = "Nombre";
            cblBarrio.DataValueField = "Id";
            cblBarrio.DataBind();

            cblProyecto.DataSource = ObtenerProyectos();
            cblProyecto.DataTextField = "Nombre";
            cblProyecto.DataValueField = "Nombre";
            cblProyecto.DataBind();

            cblEmpresa.DataSource = ObtenerEmpresas();
            cblEmpresa.DataTextField = "Nombre";
            cblEmpresa.DataValueField = "Id";
            cblEmpresa.DataBind();

            cblArea.DataSource = ObtenerAreas();
            cblArea.DataTextField = "Nombre";
            cblArea.DataValueField = "Id";
            cblArea.DataBind();

            cblAutorizante.DataSource = ObtenerAutorizantes();
            cblAutorizante.DataTextField = "Nombre";
            cblAutorizante.DataValueField = "Id";
            cblAutorizante.DataBind();

            cblEstadoExpediente.DataSource = ObtenerEstadosExpedientes();
            cblEstadoExpediente.DataTextField = "Nombre";
            cblEstadoExpediente.DataValueField = "Id";
            cblEstadoExpediente.DataBind();

            cblLinea.DataSource = ObtenerLineasGestion();
            cblLinea.DataTextField = "Nombre";
            cblLinea.DataValueField = "Id";
            cblLinea.DataBind();

            // Obtener fechas distintas (solo la parte Date) y no nulas de los certificados cargados.
            var fechasDisponibles = certificados
                .Where(c => c.MesAprobacion.HasValue) // Filtrar certificados que sí tienen fecha de aprobación.
                .Select(c => c.MesAprobacion.Value.Date) // Seleccionar solo la parte de la fecha (ignorar hora).
                .Distinct() // Obtener solo las fechas únicas.
                .OrderBy(d => d) // Ordenar las fechas ascendentemente.
                .Select(fecha => new // Proyectar a un tipo anónimo que el control pueda usar.
                {
                    // Usar el formato yyyy-MM-dd tanto para el texto como para el valor.
                    // El control CheckBoxListSearch usará este formato para construir la jerarquía Año/Mes/Día.
                    Texto = fecha.ToString("yyyy-MM-dd"),
                    Valor = fecha.ToString("yyyy-MM-dd")
                })
                .ToList(); // Convertir el resultado a una lista.

            // Asignar la lista de fechas reales como DataSource.
            cblFecha.DataSource = fechasDisponibles;
            // Especificar los campos del tipo anónimo para Texto y Valor.
            cblFecha.DataTextField = "Texto";
            cblFecha.DataValueField = "Valor";
            cblFecha.DataBind();

        }

        private DataTable ObtenerEmpresas()
        {
            EmpresaNegocio empresaNegocio = new EmpresaNegocio();
            return empresaNegocio.listarddl();
        }

        private DataTable ObtenerLineasGestion()
        {
            LineaGestionNegocio lineaGestionNegocio = new LineaGestionNegocio();
            return lineaGestionNegocio.listarddl();
        }

        private DataRow CrearFilaTodos(DataTable table)
        {
            DataRow row = table.NewRow();
            row["Id"] = 0;            
            row["Nombre"] = "Todos";   
            return row;
        }
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
                CargarListaCertificados();
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
            cblArea.ClearSelection();
            cblBarrio.ClearSelection();
            cblProyecto.ClearSelection();
            cblEmpresa.ClearSelection();
            cblAutorizante.ClearSelection();
            cblTipo.ClearSelection();
            cblFecha.ClearSelection();
            cblEstadoExpediente.ClearSelection();
            cblLinea.ClearSelection();
            CargarListaCertificados();
        }

    }
}