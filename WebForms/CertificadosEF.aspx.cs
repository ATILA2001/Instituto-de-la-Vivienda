using Dominio;
using Dominio.DTO;
using Negocio;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebForms.CustomControls;

namespace WebForms
{
    /// <summary>
    /// Administración de certificados y reliquidaciones: paginación, filtrado en memoria y operaciones CRUD/export.
    /// </summary>
    public partial class CertificadosEF : Page
    {
        #region Instancias de Negocio
        // Se instancian las clases de negocio de EF.
        readonly CertificadoNegocioEF negocio = new CertificadoNegocioEF();
        readonly CalculoRedeterminacionNegocioEF calculoRedeterminacionNegocio = new CalculoRedeterminacionNegocioEF();

        #endregion

        #region Variables de Paginación Externa

        /// <summary>
        /// Índice de página actual (base 0). Se mantiene en ViewState para persistir entre postbacks.
        /// Este sistema de paginación es independiente del GridView nativo y permite mejor control.
        /// </summary>
        private int currentPageIndex = 0;

        /// <summary>
        /// Cantidad de registros por página. Por defecto 12, configurable por el usuario.
        /// Se mantiene en ViewState para persistir la preferencia del usuario.
        /// </summary>
        private int pageSize = 12;

        /// <summary>
        /// Total de registros disponibles según filtros actuales.
        /// Se calcula una vez y se almacena en ViewState para evitar recálculos.
        /// </summary>
        private int totalRecords = 0;


        #endregion

        #region Eventos de Página
        protected void Page_PreRender(object sender, EventArgs e)
        {
            rfvAutorizante.Enabled = Session["EditingCertificadoId"] == null;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // Cargar valores de paginación desde ViewState
            currentPageIndex = (int)(ViewState["CurrentPageIndex"] ?? 0);
            pageSize = (int)(ViewState["PageSize"] ?? 12);

            if (!IsPostBack)
            {
                BindDropDownList();
                BindGrid();
            }

        }

        #endregion

        #region Métodos de Paginación
    /// <summary>
    /// Carga o reutiliza la lista completa de certificados en Session["GridData"].
    /// Llama al negocio si no hay caché.
    /// </summary>
        private void CargarListaCertificadosCompleta()
        {
            try
            {
                List<CertificadoDTO> todoLosCertificados;

                if (Session["GridData"] == null)
                {
                    UsuarioEF usuarioActual = UserHelper.GetFullCurrentUser();
                    todoLosCertificados = calculoRedeterminacionNegocio.ListarCertificadosYReliquidaciones(usuarioActual);

                    // Guardar en cache
                    Session["GridData"] = todoLosCertificados;
                    ViewState["NecesitaRecarga"] = null;
                }
                else
                {
                    todoLosCertificados = (List<CertificadoDTO>)Session["GridData"];
                }

                // Guardar total de registros en campo
                totalRecords = todoLosCertificados?.Count ?? 0;
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cargar certificados: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
                System.Diagnostics.Trace.TraceError("Error al cargar certificados: " + ex);
            }
        }

    /// <summary>
    /// Filtra en memoria sobre Session["GridData"], pagina y realiza DataBind del GridView.
    /// Guarda el resultado filtrado en Session["FilteredGridData"].
    /// </summary>
        private void BindGrid()
        {
            if (Session["GridData"] == null)
                CargarListaCertificadosCompleta();

            var certificadosCompleta = (List<CertificadoDTO>)Session["GridData"];

            // Aplicar filtro de texto general
            string filtro = txtBuscar.Text.Trim().ToLower();
            if (!string.IsNullOrEmpty(filtro))
            {
                certificadosCompleta = certificadosCompleta.Where(c =>
                    (c.ExpedientePago?.ToLower().Contains(filtro) ?? false) ||
                    (c.EmpresaNombre?.ToLower().Contains(filtro) ?? false) ||
                    (c.CodigoAutorizante?.ToLower().Contains(filtro) ?? false) ||
                    (c.ObraDescripcion?.ToLower().Contains(filtro) ?? false) ||
                    (c.AreaNombre?.ToLower().Contains(filtro) ?? false) ||
                    (c.BarrioNombre?.ToLower().Contains(filtro) ?? false) ||
                    (c.TipoPagoNombre?.ToLower().Contains(filtro) ?? false)
                ).ToList();
            }

            // Aplicar filtros de las columnas (TreeView)
            var certificadosFiltrada = AplicarFiltrosTreeViewEnMemoria(certificadosCompleta);
            Session["FilteredGridData"] = certificadosFiltrada;

            // Configurar paginación usando la lista ya filtrada
            int totalFiltrados = certificadosFiltrada.Count;
            gridviewRegistros.VirtualItemCount = totalFiltrados;
            gridviewRegistros.PageSize = pageSize;
            gridviewRegistros.PageIndex = currentPageIndex;
            gridviewRegistros.DataSource = certificadosFiltrada
                                            .Skip(currentPageIndex * pageSize)
                                            .Take(pageSize)
                                            .ToList();
            gridviewRegistros.DataBind();

            // Poblar filtros del header si corresponde
            PoblarFiltrosHeader();

            // Actualizar totalRecords y paginación según los datos filtrados
            totalRecords = totalFiltrados;
            ConfigurarPaginationControl(certificadosFiltrada);
        }


    /// <summary>
    /// Reconstituye y hace DataBind de la página actual usando la lista filtrada (recalcula si no existe caché).
    /// </summary>
    private void CargarPaginaActual()
        {
            // Guarda el estado actual en ViewState
            ViewState["CurrentPageIndex"] = currentPageIndex;
            ViewState["PageSize"] = pageSize;

            var datosFiltrados = ObtenerDatosFiltradosActuales();

            // Rebind de la grilla usando la lista ya filtrada
            totalRecords = datosFiltrados.Count;
            gridviewRegistros.VirtualItemCount = totalRecords;
            gridviewRegistros.PageSize = pageSize;
            gridviewRegistros.PageIndex = currentPageIndex;
            gridviewRegistros.DataSource = datosFiltrados
                                            .Skip(currentPageIndex * pageSize)
                                            .Take(pageSize)
                                            .ToList();
            gridviewRegistros.DataBind();

            // Asegurar que los filtros del header estén poblados
            PoblarFiltrosHeader();

            // Actualizar paginador y subtotal usando la lista filtrada
            ConfigurarPaginationControl(datosFiltrados);
        }

        /// <summary>
        /// Configura el PaginationControl con la información actual de paginación
        /// </summary>
        private void ConfigurarPaginationControl(List<CertificadoDTO> datosFiltrados)
        {
            if (FindControlRecursive(this, "paginationControl") is PaginationControl paginationControl)
            {
                // Configura las propiedades del control
                paginationControl.TotalRecords = totalRecords;
                paginationControl.CurrentPageIndex = currentPageIndex;
                paginationControl.PageSize = pageSize;
                paginationControl.UpdatePaginationControls();

                // Actualizar subtotal para el control
                CalcularSubtotalParaPaginationControl(datosFiltrados);
            }
        }

        /// <summary>
        /// Calcula y actualiza el subtotal en el PaginationControl
        /// </summary>
        private void CalcularSubtotalParaPaginationControl(List<CertificadoDTO> datosFiltrados)
        {
            try
            {
                if (FindControlRecursive(this, "paginationControl") is PaginationControl paginationControl)
                {
                    var subtotal = datosFiltrados.Sum(c => c.MontoTotal);
                    var cantidad = datosFiltrados.Count;

                    paginationControl.UpdateSubtotal(subtotal, cantidad);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Error al calcular subtotal para PaginationControl: " + ex);
            }
        }


    /// <summary>
    /// Busca un control recursivamente en la jerarquía de controles.
    /// Útil cuando el designer no expone el control directamente.
    /// </summary>
        private Control FindControlRecursive(Control root, string id)
        {
            if (root.ID == id)
                return root;

            foreach (Control control in root.Controls.Cast<Control>())
            {
                Control found = FindControlRecursive(control, id);
                if (found != null)
                    return found;
            }

            return null;
        }

        #endregion

    /// <summary>
    /// Handler de confirmación de filtros del header: invalida caché de filtrado y recarga la primera página.
    /// </summary>
        public void OnAcceptChanges(object sender, EventArgs e)
        {
            currentPageIndex = 0; // Reiniciar a la primera página al aplicar filtros
            ViewState["CurrentPageIndex"] = currentPageIndex;

            // Invalida la caché de filtrado para forzar que se apliquen los nuevos filtros
            Session["FilteredGridData"] = null;

            CargarPaginaActual(); // Cargar datos filtrados y paginados
        }

        #region Eventos del Control de Paginación

        protected void paginationControl_PageChanged(object sender, PaginationEventArgs e)
        {
            currentPageIndex = e.PageIndex;
            ViewState["CurrentPageIndex"] = currentPageIndex;
            CargarPaginaActual();
        }

        protected void paginationControl_PageSizeChanged(object sender, PaginationEventArgs e)
        {
            pageSize = e.PageSize;
            currentPageIndex = 0;
            ViewState["PageSize"] = pageSize;
            ViewState["CurrentPageIndex"] = currentPageIndex;
            CargarPaginaActual();
        }

        #endregion

        protected void btnExportarExcel_Click(object sender, EventArgs e)
        {
            try
            {
                // Usa datos del caché para exportación
                List<CertificadoDTO> todosLosCertificados;

                if (Session["GridData"] != null)
                {
                    todosLosCertificados = (List<CertificadoDTO>)Session["GridData"];
                }
                else
                {
                    CargarListaCertificadosCompleta();
                    todosLosCertificados = (List<CertificadoDTO>)Session["GridData"];
                }

                // Aplicar los mismos filtros que tiene la grilla
                string filtro = txtBuscar.Text.Trim().ToLower();
                if (!string.IsNullOrEmpty(filtro))
                {
                    todosLosCertificados = todosLosCertificados.Where(c =>
                        (c.ExpedientePago?.ToLower().Contains(filtro) ?? false) ||
                        (c.EmpresaNombre?.ToLower().Contains(filtro) ?? false) ||
                        (c.CodigoAutorizante?.ToLower().Contains(filtro) ?? false) ||
                        (c.ObraDescripcion?.ToLower().Contains(filtro) ?? false) ||
                        (c.AreaNombre?.ToLower().Contains(filtro) ?? false) ||
                        (c.BarrioNombre?.ToLower().Contains(filtro) ?? false) ||
                        (c.TipoPagoNombre?.ToLower().Contains(filtro) ?? false)
                    ).ToList();
                }

                // Define mapeo de columnas
                var mapeoColumnas = new Dictionary<string, string>
                {
                    { "Área", "AreaNombre" },
                    { "Obra", "ObraDescripcion" },
                    { "Barrio", "BarrioNombre" },
                    { "Proyecto", "ProyectoNombre" },
                    { "Empresa", "EmpresaNombre" },
                    { "Código Autorizante", "CodigoAutorizante" },
                    { "Expediente", "ExpedientePago" },
                    { "Estado", "Estado" },
                    { "Tipo", "TipoCertificado" },
                    { "Monto", "MontoTotal" },
                    { "Mes Certificado", "MesAprobacion" },
                    { "Línea", "LineaGestionNombre" }
                };

                ExcelHelper.ExportarDatosGenericos(gridviewRegistros, todosLosCertificados, mapeoColumnas, "Certificados");
            }
            catch (Exception ex)
            {
                lblMensaje.Text = "Error al exportar: " + ex.Message;
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        protected void btnShowAddModal_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();

            ScriptManager.RegisterStartupScript(this, this.GetType(), "ResetModalTitleAndShow", @"
            $(document).ready(function() {
            $('#modalAgregar .modal-title').text('Agregar Certificado');
            document.getElementById('" + btnAgregar.ClientID + @"').value = 'Agregar';
            
            $('#autorizanteContainer').show();
            
            $('#modalAgregar').modal('show');
            });", true);

            btnAgregar.Text = "Agregar";

            Session["EditingCertificadoId"] = null;
            Session["EditingAutorizanteId"] = null;
            Session["EditingCodigoAutorizante"] = null;
        }

        protected void btnLimpiar_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();
        }

        protected void btnAgregar_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            try
            {
                bool resultado;
                string expedienteAnterior = null;
                string nuevoExpediente = txtExpediente.Text.Trim();

                int? editingId = Session["EditingCertificadoId"] as int?;
                int certificadoId = editingId ?? 0; // si hay edición, será el id real

                if (editingId != null)
                {
                    // Recupera el certificado existente y modifica sus propiedades
                    CertificadoEF certificadoExistente = negocio.ObtenerPorId(certificadoId);

                    if (certificadoExistente == null)
                    {
                        lblMensaje.Text = "Error: No se encontró el certificado a modificar.";
                        lblMensaje.CssClass = "alert alert-danger";
                        return;
                    }

                    // Guardar expediente anterior para recalculo
                    expedienteAnterior = certificadoExistente.ExpedientePago;

                    // Actualiza solo las propiedades editables
                    certificadoExistente.ExpedientePago = nuevoExpediente;
                    certificadoExistente.MontoTotal = decimal.Parse(txtMontoCertificado.Text.Trim());
                    certificadoExistente.MesAprobacion = string.IsNullOrWhiteSpace(txtFecha.Text) ? (DateTime?)null : DateTime.Parse(txtFecha.Text);
                    certificadoExistente.TipoPagoId = int.Parse(ddlTipo.SelectedValue);

                    resultado = negocio.Modificar(certificadoExistente);
                    lblMensaje.Text = resultado ? "Certificado modificado exitosamente!" : "Hubo un problema al modificar el certificado.";
                }
                else
                {
                    // Crea nuevo certificado con todas las propiedades
                    CertificadoEF nuevoCertificado = new CertificadoEF
                    {
                        ExpedientePago = nuevoExpediente,
                        MontoTotal = decimal.Parse(txtMontoCertificado.Text.Trim()),
                        MesAprobacion = string.IsNullOrWhiteSpace(txtFecha.Text) ? (DateTime?)null : DateTime.Parse(txtFecha.Text),
                        TipoPagoId = int.Parse(ddlTipo.SelectedValue),
                        CodigoAutorizante = ddlAutorizante.SelectedItem.Text
                    };

                    resultado = negocio.Agregar(nuevoCertificado);
                    lblMensaje.Text = resultado ? "Certificado agregado exitosamente!" : "Hubo un problema al agregar el certificado.";
                }

                lblMensaje.CssClass = resultado ? "alert alert-success" : "alert alert-danger";

                if (resultado)
                {
                    // Preparar cache en memoria (si no existe)
                    if (Session["GridData"] == null)
                        CargarListaCertificadosCompleta();

                    var listaCache = Session["GridData"] as List<CertificadoDTO>;

                    // Construir lista de expedientes afectados (anterior y nuevo)
                    var expedientesAfectados = new List<string>();
                    if (!string.IsNullOrWhiteSpace(expedienteAnterior)) expedientesAfectados.Add(expedienteAnterior);
                    if (!string.IsNullOrWhiteSpace(nuevoExpediente)) expedientesAfectados.Add(nuevoExpediente);

                    if (editingId == null)
                    {
                        // Add: el nuevo registro puede no estar presente en la cache -> recargar desde BD
                        Session["GridData"] = null;
                        CargarListaCertificadosCompleta();
                        listaCache = Session["GridData"] as List<CertificadoDTO>;
                    }
                    else
                    {
                        // Update: reflejar cambios editables en el DTO en memoria antes del recálculo
                        if (listaCache != null)
                        {
                            var dto = listaCache.FirstOrDefault(c => c.Id == certificadoId);
                            if (dto != null)
                            {
                                dto.ExpedientePago = nuevoExpediente;
                                dto.MontoTotal = decimal.Parse(txtMontoCertificado.Text.Trim());
                                dto.MesAprobacion = string.IsNullOrWhiteSpace(txtFecha.Text) ? (DateTime?)null : DateTime.Parse(txtFecha.Text);
                                dto.TipoPagoId = int.Parse(ddlTipo.SelectedValue);
                                // Opcional: actualizar texto del tipo/campos derivados si tu DTO lo contiene
                            }
                        }
                    }

                    if (expedientesAfectados.Any() && listaCache != null)
                    {
                        // Recalcula en memoria y recarga la vista
                        RecalcularYActualizarCache(expedientesAfectados, listaCache, persistirEnBD: false, recargarVista: true);
                    }
                    else
                    {
                        // Si no hay expedientes afectados o no hay cache disponible, invalidar para forzar recarga desde BD
                        Session["GridData"] = null;
                        CalculoRedeterminacionNegocioEF.LimpiarCacheSade();
                        CargarPaginaActual();
                    }

                    // Limpiar claves de edición de certificado (si aplica)
                    Session["EditingCertificadoId"] = null;

                    // Asegurar filtrado inválido para forzar recálculo en vista
                    Session["FilteredGridData"] = null;

                    ScriptManager.RegisterStartupScript(this, this.GetType(), "HideModal", "$('#modalAgregar').modal('hide');", true);
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }


        private void LimpiarFormulario()
        {
            txtExpediente.Text = string.Empty;
            txtMontoCertificado.Text = string.Empty;
            txtFecha.Text = string.Empty;
            ddlAutorizante.SelectedIndex = 0;
            ddlTipo.SelectedIndex = 0;
        }


        /// <summary>
        /// Maneja la selección de filas en el GridView para editar certificados y reliquidaciones.
        /// 
        /// FUNCIONALIDAD:
        /// - Permite editar certificados reales (TipoPagoId != 3) obtenidos de BD
        /// - Permite editar reliquidaciones virtuales (TipoPagoId == 3) calculadas dinámicamente
        /// - Carga datos del registro seleccionado en el modal de edición
        /// - Configura la interfaz según el tipo de registro (certificado vs reliquidación)
        /// 
        /// FLUJO DE PROCESAMIENTO:
        /// 1. Obtiene el índice de la fila seleccionada en la página actual del GridView
        /// 2. Calcula el índice real considerando la paginación (página * tamaño + índice local)
        /// 3. Recupera el registro desde los datos filtrados en memoria
        /// 4. Valida si el registro es editable (ID > 0 para certificados, cualquier ID para reliquidaciones)
        /// 5. Carga los datos en el formulario modal
        /// 6. Configura la UI según el tipo de registro
        /// 
        /// TIPOS DE REGISTRO:
        /// - Certificados (TipoPagoId != 3): Se obtienen de BD usando CertificadoNegocioEF.ObtenerPorId()
        /// - Reliquidaciones (TipoPagoId == 3): Se editan directamente desde los datos calculados
        /// 
        /// CONFIGURACIÓN DE UI:
        /// - Certificados: Oculta dropdown de autorizante (no editable), muestra título "Modificar Certificado"
        /// - Reliquidaciones: Oculta dropdown de autorizante, muestra título "Modificar Reliquidación"
        /// - Cambia texto del botón a "Actualizar" y título del modal dinámicamente
        /// 
        /// VALIDACIONES:
        /// - Verifica que el índice esté dentro del rango de datos disponibles
        /// - Solo permite edición de registros válidos (ID > 0 para certificados)
        /// - Maneja errores de carga y muestra mensajes apropiados
        /// 
        /// MANEJO DE SESIÓN:
        /// - Guarda IDs de edición en Session para uso en btnAgregar_Click
        /// - Distingue entre certificados (EditingCertificadoId) y reliquidaciones (EditingReliquidacionId)
        /// - Almacena código autorizante para contexto de edición
        /// </summary>
        protected void gridviewRegistros_SelectedIndexChanged(object sender, EventArgs e)
        {

            try
            {
                int rowIndex = gridviewRegistros.SelectedIndex;

                // Obtener el registro desde los datos filtrados actuales
                var datosFiltradosActuales = ObtenerDatosFiltradosActuales();

                if (datosFiltradosActuales == null || datosFiltradosActuales.Count == 0)
                {
                    lblMensaje.Text = "Error: No hay datos disponibles.";
                    lblMensaje.CssClass = "alert alert-danger";
                    return;
                }

                // Calcular el índice real considerando la página actual
                int indiceReal = (currentPageIndex * pageSize) + rowIndex;
                if (indiceReal < 0 || indiceReal >= datosFiltradosActuales.Count)
                {
                    lblMensaje.Text = "Error: Registro no encontrado.";
                    lblMensaje.CssClass = "alert alert-danger";
                    return;
                }

                CertificadoDTO certificadoSeleccionado = datosFiltradosActuales[indiceReal];

                // Verificar si es reliquidación (TipoPagoId == 3)
                if (certificadoSeleccionado.TipoPagoId == 3)
                {
                    // Configurar para editar reliquidación
                    Session["EditingTipoRegistro"] = "Reliquidacion";
                    Session["EditingReliquidacionId"] = certificadoSeleccionado.IdReliquidacion;
                    Session["EditingCodigoAutorizante"] = certificadoSeleccionado.CodigoAutorizante;

                    // Cargar datos de la reliquidación
                    txtExpediente.Text = certificadoSeleccionado.ExpedientePago;
                    txtMontoCertificado.Text = certificadoSeleccionado.MontoTotal.ToString("0.00");
                    txtFecha.Text = certificadoSeleccionado.MesAprobacion?.ToString("yyyy-MM-dd");

                    SelectDropDownListByValue(ddlTipo, certificadoSeleccionado.TipoPagoId.ToString());

                    btnAgregar.Text = "Actualizar";


                    ScriptManager.RegisterStartupScript(this, this.GetType(), "UpdateModalAndShow", @"
                                $(document).ready(function() {
                                    $('#modalAgregar .modal-title').text('Modificar Reliquidación');
                                    document.getElementById('" + btnAgregar.ClientID + @"').value = 'Actualizar';
                                    $('#autorizanteContainer').hide();
                                    $('#modalAgregar').modal('show');
                                });", true);

                    return; // Salir del método después de configurar la reliquidación
                }

                // Buscar el certificado real en BD para edición
                CertificadoEF certificadoEF;
                using (var context = new IVCdbContext())
                {
                    certificadoEF = context.Certificados.Find(certificadoSeleccionado.Id);
                }

                if (certificadoEF != null)
                {
                    Session["EditingCertificadoId"] = certificadoSeleccionado.Id;
                    txtExpediente.Text = certificadoEF.ExpedientePago;
                    txtMontoCertificado.Text = certificadoEF.MontoTotal.ToString("0.00");
                    txtFecha.Text = certificadoEF.MesAprobacion?.ToString("yyyy-MM-dd");
                    SelectDropDownListByValue(ddlTipo, certificadoEF.TipoPagoId.ToString());

                    // Actualizar el texto del botón
                    btnAgregar.Text = "Actualizar";

                    ScriptManager.RegisterStartupScript(this, this.GetType(), "UpdateModalAndShow", @"
                                $(document).ready(function() {
                                    // Cambiar título y texto del botón
                                    $('#modalAgregar .modal-title').text('Modificar Certificado');
                                    document.getElementById('" + btnAgregar.ClientID + @"').value = 'Actualizar';
                                    // Ocultar el dropdown de Autorizante y su etiqueta
                                    $('#autorizanteContainer').hide();
                                    // Mostrar el modal
                                    $('#modalAgregar').modal('show');
                                });", true);
                }
                else
                {
                    lblMensaje.Text = "Error: No se pudo cargar el certificado para edición.";
                    lblMensaje.CssClass = "alert alert-danger";
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
            dropDown.ClearSelection();
            ListItem item = dropDown.Items.FindByValue(value);
            if (item != null)
            {
                item.Selected = true;
            }
        }
        protected void gridviewRegistros_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            try
            {
                var id = Convert.ToInt32(gridviewRegistros.DataKeys[e.RowIndex].Value);
                if (negocio.Eliminar(id))
                {
                    lblMensaje.Text = "Certificado eliminado correctamente.";
                    lblMensaje.CssClass = "alert alert-success";

                    // Obtener cache actual
                    if (Session["GridData"] is List<CertificadoDTO> listaCache)
                    {
                        // Buscar el certificado en el cache antes de eliminar para conocer expedientes afectados
                        var certificadoEliminado = listaCache.FirstOrDefault(c => c.Id == id);
                        List<string> expedientesAfectados = new List<string>();

                        if (certificadoEliminado != null)
                        {
                            if (!string.IsNullOrWhiteSpace(certificadoEliminado.ExpedientePago))
                                expedientesAfectados.Add(certificadoEliminado.ExpedientePago);
                        }

                        // Remover el registro del cache
                        listaCache.RemoveAll(c => c.Id == id);

                        // Recalcular y recargar vista si corresponde
                        RecalcularYActualizarCache(expedientesAfectados, listaCache, persistirEnBD: false, recargarVista: true);
                    }
                    else
                    {
                        // Si no hay cache en memoria, limpiar cache SADE y recargar desde BD
                        CalculoRedeterminacionNegocioEF.LimpiarCacheSade();
                        Session["GridData"] = null;
                        CargarPaginaActual();
                    }
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al eliminar el certificado: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        protected void gridviewRegistros_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                var certificado = (CertificadoDTO)e.Row.DataItem;
                if (certificado.IdReliquidacion > 0)
                {
                    if (e.Row.FindControl("btnModificar") is LinkButton btnEditar) btnEditar.Visible = false;
                    if (e.Row.FindControl("btnEliminar") is LinkButton btnEliminar) btnEliminar.Visible = false;
                }
            }
        }

        private void PoblarFiltrosHeader()
        {
            // Evitar abrir contexto/consultas si el HeaderRow aún no existe
            if (gridviewRegistros.HeaderRow == null) return;

            using (var context = new IVCdbContext())
            {
                Action<string, object, string, string> bindFilter = (controlId, dataSource, textField, valueField) =>
                {
                    if (gridviewRegistros.HeaderRow?.FindControl(controlId) is TreeViewSearch control)
                    {
                        control.DataSource = dataSource;
                        control.DataTextField = textField;
                        control.DataValueField = valueField;
                        control.DataBind();
                    }
                };

                bindFilter("cblsHeaderArea", context.Areas.AsNoTracking().OrderBy(a => a.Nombre).Select(a => new { a.Id, a.Nombre }).ToList(), "Nombre", "Id");
                bindFilter("cblsHeaderObra", context.Obras.AsNoTracking().OrderBy(o => o.Descripcion).Select(o => new { o.Id, o.Descripcion }).ToList(), "Descripcion", "Id");
                bindFilter("cblsHeaderBarrio", context.Barrios.AsNoTracking().OrderBy(b => b.Nombre).Select(b => new { b.Id, b.Nombre }).ToList(), "Nombre", "Id");
                bindFilter("cblsHeaderProyecto", context.Proyectos.AsNoTracking().OrderBy(p => p.Nombre).Select(p => new { p.Id, p.Nombre }).ToList(), "Nombre", "Id");
                bindFilter("cblsHeaderEmpresa", context.Empresas.AsNoTracking().OrderBy(e => e.Nombre).Select(e => new { e.Id, e.Nombre }).ToList(), "Nombre", "Id");
                bindFilter("cblsHeaderCodigoAutorizante", context.Autorizantes.AsNoTracking().OrderBy(a => a.CodigoAutorizante).Select(a => new { a.Id, a.CodigoAutorizante }).ToList(), "CodigoAutorizante", "Id");
                bindFilter("cblsHeaderTipo", context.TiposPago.AsNoTracking().OrderBy(t => t.Nombre).Select(t => new { t.Id, t.Nombre }).ToList(), "Nombre", "Id");
                bindFilter("cblsHeaderLinea", context.LineasGestion.AsNoTracking().OrderBy(l => l.Nombre).Select(l => new { l.Id, l.Nombre }).ToList(), "Nombre", "Id");

                if (gridviewRegistros.HeaderRow?.FindControl("cblsHeaderEstado") is TreeViewSearch cblsHeaderEstado)
                {
                    // Estados de certificados: usar el texto como ID y como nombre
                    var estadosCertificados = new[]
                    {
                        new { Id = "NO INICIADO", Nombre = "NO INICIADO" },
                        new { Id = "EN TRAMITE", Nombre = "EN TRAMITE" },
                        new { Id = "DEVENGADO", Nombre = "DEVENGADO" }
                    };
                    cblsHeaderEstado.DataSource = estadosCertificados;
                    cblsHeaderEstado.DataTextField = "Nombre";
                    cblsHeaderEstado.DataValueField = "Id";
                    cblsHeaderEstado.DataBind();
                }

                if (gridviewRegistros.HeaderRow?.FindControl("cblsHeaderMesCertificado") is TreeViewSearch cblsHeaderMesCertificado)
                {
                    // 1. Se consultan todos los meses de aprobación distintos desde la base de datos.
                    var meses = context.Certificados.AsNoTracking()
                        .Where(c => c.MesAprobacion.HasValue)
                        .Select(c => c.MesAprobacion.Value)
                        .Distinct()
                        .OrderBy(d => d)
                        .ToList();

                    // 2. Se asigna la lista de fechas como fuente de datos y se enlaza al control.
                    cblsHeaderMesCertificado.DataSource = meses;
                    cblsHeaderMesCertificado.DataBind();
                }
            }
        }

        // Estos métodos deben ser adaptados para usar las clases de negocio EF.
        private void ObtenerTipos()
        {
            TipoPagoNegocioEF tipoPagoNegocio = new TipoPagoNegocioEF();
            ddlTipo.DataSource = tipoPagoNegocio.Listar();
            ddlTipo.DataTextField = "Nombre";
            ddlTipo.DataValueField = "Id";
            ddlTipo.DataBind();
        }

        private void ObtenerAutorizantes()
        {
            AutorizanteNegocioEF autorizanteNegocio = new AutorizanteNegocioEF();
            ddlAutorizante.DataSource = autorizanteNegocio.ListarParaDDL();
            ddlAutorizante.DataTextField = "CodigoAutorizante";
            ddlAutorizante.DataValueField = "Id";
            ddlAutorizante.DataBind();
        }

        private void BindDropDownList()
        {
            try
            {
                // Cargar dropdowns independientes
                ObtenerTipos();
                ObtenerAutorizantes();
            }
            catch (Exception)
            {
            }
        }

        protected void txtExpediente_TextChanged(object sender, EventArgs e)
        {
            TextBox txtBox = (TextBox)sender;
            GridViewRow row = (GridViewRow)txtBox.NamingContainer;

            try
            {
                int rowIndex = row.RowIndex;
                string nuevoExpediente = txtBox.Text.Trim();

                var datosFiltradosActuales = ObtenerDatosFiltradosActuales();
                if (datosFiltradosActuales == null)
                {
                    lblMensaje.Text = "Error: No hay datos en memoria.";
                    lblMensaje.CssClass = "alert alert-danger";
                    return;
                }

                int indiceReal = (gridviewRegistros.PageIndex * gridviewRegistros.PageSize) + rowIndex;
                if (indiceReal < 0 || indiceReal >= datosFiltradosActuales.Count)
                {
                    lblMensaje.Text = "Error: Índice fuera del rango de datos.";
                    lblMensaje.CssClass = "alert alert-danger";
                    return;
                }

                CertificadoDTO certificado = datosFiltradosActuales[indiceReal];
                string expedienteAnterior = certificado.ExpedientePago;

                List<CertificadoDTO> listaCompleta = Session["GridData"] as List<CertificadoDTO>;

                bool resultado = false;

                // --- Lógica para expediente vacío ---
                if (string.IsNullOrWhiteSpace(nuevoExpediente))
                {

                    if (certificado.TipoPagoId != 3 && certificado.Id > 0) // Certificado normal
                    {
                        // Usar la capa de negocio para certificados normales
                        var negocio = new CertificadoNegocioEF();
                        CertificadoEF entidad = negocio.ObtenerPorId(certificado.Id);

                        if (entidad != null)
                        {
                            entidad.ExpedientePago = string.Empty;
                            resultado = negocio.Modificar(entidad);
                        }

                        if (listaCompleta != null)
                        {
                            CertificadoDTO certificadoEnListaCompleta = listaCompleta.FirstOrDefault(c => c.Id == certificado.Id);
                            if (certificadoEnListaCompleta != null)
                            {
                                certificadoEnListaCompleta.ExpedientePago = nuevoExpediente;
                                certificadoEnListaCompleta.Estado = "NO INICIADO";
                                certificadoEnListaCompleta.Sigaf = null;
                                certificadoEnListaCompleta.BuzonSade = null;
                                certificadoEnListaCompleta.FechaSade = null;
                            }
                        }


                    }
                    else if (certificado.TipoPagoId == 3) // Reliquidación
                    {
                        // Usar la capa de negocio para reliquidaciones
                        var negocioReliq = new ExpedienteReliqNegocioEF();
                        resultado = negocioReliq.GuardarOActualizar(certificado.CodigoAutorizante, certificado.MesAprobacion.Value, "");

                        if (listaCompleta != null)
                        {
                            // encontrar el id correcto para editar el certificado
                            var reliquidacionEnListaCompleta = datosFiltradosActuales.FirstOrDefault(c => c.IdReliquidacion == certificado.IdReliquidacion);
                            if (reliquidacionEnListaCompleta != null)
                            {
                                reliquidacionEnListaCompleta.ExpedientePago = nuevoExpediente;
                                reliquidacionEnListaCompleta.Estado = "NO INICIADO";
                                reliquidacionEnListaCompleta.Sigaf = null;
                                reliquidacionEnListaCompleta.BuzonSade = null;
                                reliquidacionEnListaCompleta.FechaSade = null;
                            }
                        }


                    }


                    if (resultado)
                    {
                        lblMensaje.Text = "Expediente eliminado correctamente.";
                        lblMensaje.CssClass = "alert alert-info";
                    }
                    else
                    {
                        lblMensaje.Text = "No se detectaron cambios para guardar.";
                        lblMensaje.CssClass = "alert alert-warning";
                    }

                }
                // --- Fin lógica expediente vacío ---


                // Lógica para expediente no vacío
                else
                {
                    certificado.ExpedientePago = nuevoExpediente;

                    if (certificado.TipoPagoId == 3) // Reliquidación
                    {
                        if (!certificado.MesAprobacion.HasValue)
                        {
                            lblMensaje.Text = "Error: La reliquidación no tiene fecha de aprobación.";
                            lblMensaje.CssClass = "alert alert-danger";
                            return;
                        }
                        var negocioReliq = new ExpedienteReliqNegocioEF();
                        resultado = negocioReliq.GuardarOActualizar(certificado.CodigoAutorizante, certificado.MesAprobacion.Value, nuevoExpediente);
                    }
                    else // Certificado normal
                    {
                        if (certificado.Id > 0)
                        {
                            var negocio = new CertificadoNegocioEF();
                            var entidad = negocio.ObtenerPorId(certificado.Id);
                            if (entidad != null)
                            {
                                entidad.ExpedientePago = nuevoExpediente;
                                resultado = negocio.Modificar(entidad);
                            }
                        }
                    }
                }


                if (resultado)
                {
                    // Lista de expedientes afectados (anterior y nuevo)
                    var expedientesAfectados = new List<string>();

                    if (!string.IsNullOrWhiteSpace(expedienteAnterior))
                        expedientesAfectados.Add(expedienteAnterior);

                    if (!string.IsNullOrWhiteSpace(nuevoExpediente))
                        expedientesAfectados.Add(nuevoExpediente);

                    // Recalcular en memoria y recargar la vista
                    RecalcularYActualizarCache(expedientesAfectados, listaCompleta, persistirEnBD: false, recargarVista: true);

                    // Limpiar cache SADE ya que se modificó un expediente (certificado o reliquidación)
                    // (Queda incluida en RecalcularYActualizarCache)

                    string tipoRegistro = certificado.TipoPagoId == 3 ? "reliquidación" : "certificado";
                    lblMensaje.Text = $"Expediente de {tipoRegistro} actualizado correctamente.";
                    lblMensaje.CssClass = "alert alert-info";
                }
                else
                {
                    lblMensaje.Text = "Error al actualizar el expediente.";
                    lblMensaje.CssClass = "alert alert-danger";
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al actualizar el expediente: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        /// <summary>
        /// Recalcula SIGAF, actualiza BuzonSade y FechaSade para los expedientes indicados,
        /// actualiza Session["GridData"], limpia cache SADE y recarga la vista si corresponde.
        /// - expedientesAfectados: lista de números de expediente (anterior y/o nuevo)
        /// - listaCache: si se pasa, se usa la lista en memoria; si es null se carga desde Session
        /// - persistirEnBD: si true, el calculo debe persistir cambios en BD; si false solo en memoria
        /// - recargarVista: si true se llamará a CargarPaginaActual() al final
        /// </summary>
        private void RecalcularYActualizarCache(List<string> expedientesAfectados, List<CertificadoDTO> listaCache = null, bool persistirEnBD = false, bool recargarVista = true)
        {
            if (expedientesAfectados == null) return;

            // Filtrar nulos/vacíos y deduplicar
            expedientesAfectados = expedientesAfectados
                            .Where(s => !string.IsNullOrWhiteSpace(s))
                            .Select(s => s.Trim())
                            .Distinct()
                            .ToList();

            if (!expedientesAfectados.Any()) return;

            try
            {
                // Asegurar que exista la lista en memoria
                if (listaCache == null)
                {
                    if (Session["GridData"] == null)
                        CargarListaCertificadosCompleta();

                    listaCache = Session["GridData"] as List<CertificadoDTO>;
                    if (listaCache == null) return;
                }

                // Ejecutar recalculo: se espera que CalcularCertificadosPorExpedientes
                // actualice en la lista los campos Sigaf, BuzonSade y FechaSade (y demás)
                calculoRedeterminacionNegocio.CalcularCertificadosPorExpedientes(expedientesAfectados, listaCache, persistirEnBD);

                // Asegurar que los cambios queden en el cache de sesión
                Session["GridData"] = listaCache;
                Session["FilteredGridData"] = null; // <-- invalidar caché de filtrado
                totalRecords = listaCache.Count;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Error al recalcular SIGAF/buzón/fecha SADE: " + ex);
            }
            finally
            {
                // Limpiar cache SADE siempre que hubo cambios
                CalculoRedeterminacionNegocioEF.LimpiarCacheSade();

                // Opcional: volver a la primera página para evitar índices fuera de rango
                currentPageIndex = 0;
                ViewState["CurrentPageIndex"] = currentPageIndex;

                if (recargarVista)
                {
                    // Recargar página actual para que la grilla muestre los valores recalculados
                    CargarPaginaActual();
                }
            }
        }

        /// <summary>
        /// Método auxiliar para obtener los datos filtrados que se están mostrando actualmente en el grid.
        /// Replica la misma lógica de filtrado que se usa en BindGrid().
        /// </summary>
        private List<CertificadoDTO> ObtenerDatosFiltradosActuales()
        {
            // Usar caché de filtrado si existe (establecido en BindGrid)
            if (Session["FilteredGridData"] is List<CertificadoDTO> cachedFiltered)
                return cachedFiltered;

            if (Session["GridData"] == null)
                CargarListaCertificadosCompleta();

            var certificadosCompleta = (List<CertificadoDTO>)Session["GridData"];

            // Aplicar filtro de texto general (mismo que en BindGrid)
            string filtro = txtBuscar.Text.Trim().ToLower();
            if (!string.IsNullOrEmpty(filtro))
            {
                certificadosCompleta = certificadosCompleta.Where(c =>
                    (c.ExpedientePago?.ToLower().Contains(filtro) ?? false) ||
                    (c.EmpresaNombre?.ToLower().Contains(filtro) ?? false) ||
                    (c.CodigoAutorizante?.ToLower().Contains(filtro) ?? false) ||
                    (c.ObraDescripcion?.ToLower().Contains(filtro) ?? false) ||
                    (c.AreaNombre?.ToLower().Contains(filtro) ?? false) ||
                    (c.BarrioNombre?.ToLower().Contains(filtro) ?? false) ||
                    (c.TipoPagoNombre?.ToLower().Contains(filtro) ?? false)
                ).ToList();
            }

            // Aplicar filtros de las columnas (TreeView) - mismo que en BindGrid
            var certificadosFiltrada = AplicarFiltrosTreeViewEnMemoria(certificadosCompleta);

            // Guardar en sesión para reuso
            Session["FilteredGridData"] = certificadosFiltrada;

            return certificadosFiltrada;
        }


    #region Eventos de Filtrado Optimizados
    /// <summary>
    /// Aplica filtros desde el header y recarga la primera página.
    /// </summary>
        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            currentPageIndex = 0; // Reiniciar a la primera página al aplicar filtros

            // Forzar recomputo de filtros
            Session["FilteredGridData"] = null;

            CargarPaginaActual();
        }

        protected void BtnClearFilters_Click(object sender, EventArgs e)
        {
            txtBuscar.Text = string.Empty;
            TreeViewSearch.ClearAllFiltersOnPage(this.Page);
            currentPageIndex = 0;

            // Forzar recomputo de filtros (ahora sin filtros)
            Session["FilteredGridData"] = null;

            CargarPaginaActual();
        }
        #endregion

        private List<CertificadoDTO> AplicarFiltrosTreeViewEnMemoria(List<CertificadoDTO> data)
        {
            try
            {
                // Busca el control por id en toda la página (no depender del HeaderRow)
                Func<string, TreeViewSearch> find = id => FindControlRecursive(this, id) as TreeViewSearch;

                // Función auxiliar para parsear IDs de forma segura.
                Func<TreeViewSearch, List<int>> getSelectedIds = (tv) =>
                    tv?.ExpandedSelectedValues
                       .Select(s => int.TryParse(s, out int id) ? id : -1)
                       .Where(id => id != -1)
                       .ToList() ?? new List<int>();

                // Precalcular listas seleccionadas (evita recalcular por elemento)
                var cblsHeaderArea = find("cblsHeaderArea");
                var cblsHeaderObra = find("cblsHeaderObra");
                var cblsHeaderBarrio = find("cblsHeaderBarrio");
                var cblsHeaderProyecto = find("cblsHeaderProyecto");
                var cblsHeaderEmpresa = find("cblsHeaderEmpresa");
                var cblsHeaderCodigoAutorizante = find("cblsHeaderCodigoAutorizante");
                var cblsHeaderEstado = find("cblsHeaderEstado");
                var cblsHeaderTipo = find("cblsHeaderTipo");
                var cblsHeaderMesCertificado = find("cblsHeaderMesCertificado");
                var cblsHeaderLinea = find("cblsHeaderLinea");

                var selectedAreaIds = cblsHeaderArea != null && cblsHeaderArea.ExpandedSelectedValues.Any() ? getSelectedIds(cblsHeaderArea) : null;
                var selectedObraIds = cblsHeaderObra != null && cblsHeaderObra.ExpandedSelectedValues.Any() ? getSelectedIds(cblsHeaderObra) : null;
                var selectedBarrioIds = cblsHeaderBarrio != null && cblsHeaderBarrio.ExpandedSelectedValues.Any() ? getSelectedIds(cblsHeaderBarrio) : null;
                var selectedProyectoIds = cblsHeaderProyecto != null && cblsHeaderProyecto.ExpandedSelectedValues.Any() ? getSelectedIds(cblsHeaderProyecto) : null;
                var selectedEmpresaIds = cblsHeaderEmpresa != null && cblsHeaderEmpresa.ExpandedSelectedValues.Any() ? getSelectedIds(cblsHeaderEmpresa) : null;
                var selectedAutorizanteIds = cblsHeaderCodigoAutorizante != null && cblsHeaderCodigoAutorizante.ExpandedSelectedValues.Any() ? getSelectedIds(cblsHeaderCodigoAutorizante) : null;
                var selectedTipoIds = cblsHeaderTipo != null && cblsHeaderTipo.ExpandedSelectedValues.Any() ? getSelectedIds(cblsHeaderTipo) : null;
                var selectedLineaIds = cblsHeaderLinea != null && cblsHeaderLinea.ExpandedSelectedValues.Any() ? getSelectedIds(cblsHeaderLinea) : null;

                // Aplicar filtro por Área
                if (selectedAreaIds != null) data = data.Where(c => c.AreaId.HasValue && selectedAreaIds.Contains(c.AreaId.Value)).ToList();

                // Aplicar filtro por Obra
                if (selectedObraIds != null) data = data.Where(c => c.ObraId.HasValue && selectedObraIds.Contains(c.ObraId.Value)).ToList();

                // Aplicar filtro por Barrio
                if (selectedBarrioIds != null) data = data.Where(c => c.BarrioId.HasValue && selectedBarrioIds.Contains(c.BarrioId.Value)).ToList();

                // Aplicar filtro por Proyecto
                if (selectedProyectoIds != null) data = data.Where(c => c.ProyectoId.HasValue && selectedProyectoIds.Contains(c.ProyectoId.Value)).ToList();

                // Aplicar filtro por Empresa
                if (selectedEmpresaIds != null) data = data.Where(c => c.EmpresaId.HasValue && selectedEmpresaIds.Contains(c.EmpresaId.Value)).ToList();

                // Aplicar filtro por Código Autorizante
                if (selectedAutorizanteIds != null) data = data.Where(c => c.AutorizanteId.HasValue && selectedAutorizanteIds.Contains(c.AutorizanteId.Value)).ToList();

                // Aplicar filtro por Estado (texto)
                if (cblsHeaderEstado != null && cblsHeaderEstado.ExpandedSelectedValues.Any())
                {
                    var selectedEstados = cblsHeaderEstado.ExpandedSelectedValues;
                    data = data.Where(c => !string.IsNullOrEmpty(c.Estado) && selectedEstados.Contains(c.Estado)).ToList();
                }

                // Aplicar filtro por Tipo
                if (selectedTipoIds != null) data = data.Where(c => c.TipoPagoId.HasValue && selectedTipoIds.Contains(c.TipoPagoId.Value)).ToList();

                // Aplicar filtro por Mes Certificado
                if (cblsHeaderMesCertificado != null && cblsHeaderMesCertificado.SelectedValues.Any())
                {
                    var selectedDates = cblsHeaderMesCertificado.SelectedValues
                        .Select(s => DateTime.TryParse(s, out DateTime dt) ? (DateTime?)dt.Date : null)
                        .Where(d => d.HasValue).Select(d => d.Value)
                        .ToList();
                    data = data.Where(c => c.MesAprobacion.HasValue && selectedDates.Contains(c.MesAprobacion.Value.Date)).ToList();
                }

                // Aplicar filtro por Línea de Gestión
                if (selectedLineaIds != null) data = data.Where(c => c.LineaGestionId.HasValue && selectedLineaIds.Contains(c.LineaGestionId.Value)).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Error al calcular subtotal para PaginationControl: " + ex);
            }

            return data;
        }


    }
}
