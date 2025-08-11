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
    /// Página de administración de certificados y reliquidaciones con sistema de paginación optimizada.
    /// </summary>
    public partial class CertificadosEF : Page
    {
        #region Instancias de Negocio
        // Se instancian las clases de negocio de EF.
        CertificadoNegocioEF negocio = new CertificadoNegocioEF();
        CalculoRedeterminacionNegocioEF calculoRedeterminacionNegocio = new CalculoRedeterminacionNegocioEF();
        ExpedienteReliqNegocioEF negocioReliq = new ExpedienteReliqNegocioEF();
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
                CargarListaCertificadosCompleta();
                BindDropDownList();
                ConfigurarPaginationControl();
            }
            else
            {
                // En postback, actualizar controles de paginación
                var paginationControl = FindControlRecursive(this, "paginationControl") as CustomControls.PaginationControl;
                if (paginationControl != null)
                {
                    paginationControl.UpdatePaginationControls();
                }
            }
        }

        #endregion

        #region Métodos de Paginación
        /// <summary>
        /// Método central que carga datos paginados de certificados y reliquidaciones.
        /// 
        /// ARQUITECTURA DE PAGINACIÓN REAL:
        /// 1. Consulta totalRecords con ContarTotalCertificados()
        /// 2. Carga solo pageSize registros usando ListarCertificadosPaginados() 
        /// 3. Guarda total en ViewState["TotalRecords"] para cálculos de paginación
        /// 4. Guarda datos de página actual en Session["GridData"]
        /// 
        /// DATOS COMBINADOS:
        /// - Certificados reales de tabla Certificados
        /// - Reliquidaciones calculadas dinámicamente (virtuales)
        /// - Integración de datos SADE y SIGAF
        /// - Estados mapeados según lógica de negocio
        /// 
        /// OPTIMIZACIONES IMPLEMENTADAS:
        /// - Paginación en BD (no carga todos los registros)
        /// - Variables externas currentPageIndex y pageSize
        /// - Consulta de conteo separada (más eficiente)
        /// - Session para evitar recálculos en filtros locales
        /// </summary>
        private void CargarListaCertificadosCompleta()
        {
            try
            {
                // Obtener usuario actual
                UsuarioEF usuario = UserHelper.GetFullCurrentUser();

                // REGRESANDO A CARGA COMPLETA: necesaria para todos los campos calculados (Contrata, Proyecto, SIGAF, SADE, etc.)
                // OPTIMIZACIÓN: Solo cargar si no están en cache
                List<CertificadoDTO> todoLosCertificados;
                
                if (Session["GridData"] == null || ViewState["NecesitaRecarga"] != null)
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
                
                // Calcular total de registros
                int totalRegistros = todoLosCertificados?.Count ?? 0;
                
                // Guardar total en ViewState
                totalRecords = totalRegistros;

                // Usar BindGrid para paginación en memoria
                BindGrid();
            }
            catch (Exception ex)
            {
                
                lblMensaje.Text = $"Error al cargar certificados: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        /// <summary>
        /// Método central para refrescar la visualización del GridView.
        /// Lee los datos desde Session, aplica los filtros y la paginación actuales.
        /// EXACTAMENTE IGUAL QUE AutorizantesAdminEF.
        /// </summary>
        private void BindGrid()
        {
            if (Session["GridData"] == null) return;

            var datosEnMemoria = (List<CertificadoDTO>)Session["GridData"];

            // Aplicar filtro de texto general
            string filtro = txtBuscar.Text.Trim().ToLower();
            if (!string.IsNullOrEmpty(filtro))
            {
                datosEnMemoria = datosEnMemoria.Where(c =>
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
            datosEnMemoria = AplicarFiltrosTreeViewEnMemoria(datosEnMemoria);

            // Configurar paginación
            int totalFiltrados = datosEnMemoria.Count;
            gridviewRegistros.VirtualItemCount = totalFiltrados;
            gridviewRegistros.PageSize = pageSize;
            gridviewRegistros.PageIndex = currentPageIndex;
            gridviewRegistros.DataSource = datosEnMemoria
                                        .Skip(currentPageIndex * pageSize)
                                        .Take(pageSize)
                                        .ToList();
            gridviewRegistros.DataBind();
            PoblarFiltrosHeader();

            // Actualizar totalRecords y paginación según los datos filtrados
            totalRecords = totalFiltrados;
            ViewState["TotalRecords"] = totalFiltrados;
            ConfigurarPaginationControl();

            // Recalcular subtotal después de aplicar filtros
            CalcularSubtotal();
        }

        /// <summary>
        /// Calcula el subtotal de los certificados filtrados.
        /// Replica EXACTAMENTE la lógica de AutorizantesAdminEF:
        /// Hace consulta completa independiente para obtener TODOS los registros filtrados.
        /// </summary>
        private void CalcularSubtotal()
        {
            try
            {
                // Usa datos del caché en lugar de hacer nueva consulta DB
                List<CertificadoDTO> todosLosRegistros;
                
                // Intentar obtener datos del caché primero
                if (Session["GridData"] != null)
                {
                    todosLosRegistros = (List<CertificadoDTO>)Session["GridData"];
                }
                else
                {
                    // Solo si no hay caché, consultar BD
                    UsuarioEF usuario = UserHelper.GetFullCurrentUser();
                    todosLosRegistros = calculoRedeterminacionNegocio.ListarCertificadosYReliquidaciones(usuario);
                }
                
                // Aplica filtro de texto general si existe
                string filtro = txtBuscar.Text.Trim().ToLower();
                if (!string.IsNullOrEmpty(filtro))
                {
                    todosLosRegistros = todosLosRegistros.Where(c =>
                        (c.ExpedientePago?.ToLower().Contains(filtro) ?? false) ||
                        (c.EmpresaNombre?.ToLower().Contains(filtro) ?? false) ||
                        (c.CodigoAutorizante?.ToLower().Contains(filtro) ?? false) ||
                        (c.ObraDescripcion?.ToLower().Contains(filtro) ?? false) ||
                        (c.AreaNombre?.ToLower().Contains(filtro) ?? false) ||
                        (c.BarrioNombre?.ToLower().Contains(filtro) ?? false) ||
                        (c.TipoPagoNombre?.ToLower().Contains(filtro) ?? false)
                    ).ToList();
                }

                // Aplica filtros de TreeView (filtros de columnas)
                todosLosRegistros = AplicarFiltrosTreeViewEnMemoria(todosLosRegistros);

                // Calcular el total de los registros filtrados
                decimal totalMonto = todosLosRegistros.Sum(c => c.MontoTotal);
                int cantidadRegistros = todosLosRegistros.Count;

                // Actualizar etiqueta de subtotal en el control existente
                if (txtSubtotal != null)
                {
                    txtSubtotal.Text = totalMonto.ToString("C", CultureInfo.GetCultureInfo("es-AR"));
                }

                // Actualizar label de subtotal en paginación si existe
                if (FindControlRecursive(this, "lblSubtotalPaginacion") is Label lblSubtotalPaginacion)
                {
                    lblSubtotalPaginacion.Text = $"Total: {totalMonto:C} ({cantidadRegistros} registros)";
                }
            }
            catch (Exception)
            {
                // En caso de error, mostrar información básica
                if (txtSubtotal != null)
                {
                    txtSubtotal.Text = "$0,00";
                }

                if (FindControlRecursive(this, "lblSubtotalPaginacion") is Label lblSubtotalPaginacion)
                {
                    lblSubtotalPaginacion.Text = "Total: Error al calcular";
                }
            }
        }

        private void CargarPaginaActual()
        {
            // Guarda el estado actual en ViewState
            ViewState["CurrentPageIndex"] = currentPageIndex;
            ViewState["PageSize"] = pageSize;
            
            CargarListaCertificadosCompleta();
            
            ConfigurarPaginationControl();
        }

        /// <summary>
        /// Configura el PaginationControl con la información actual de paginación
        /// </summary>
        private void ConfigurarPaginationControl()
        {

            var paginationControl = FindControlRecursive(this, "paginationControl") as PaginationControl;
            if (paginationControl != null)
            {
                // Configura las propiedades del control
                paginationControl.TotalRecords = totalRecords;
                paginationControl.CurrentPageIndex = currentPageIndex;
                paginationControl.PageSize = pageSize;
                paginationControl.UpdatePaginationControls();
                
                // Actualizar subtotal para el control
                CalcularSubtotalParaPaginationControl();
            }
        }

        /// <summary>
        /// Calcula y actualiza el subtotal en el PaginationControl
        /// </summary>
        private void CalcularSubtotalParaPaginationControl()
        {
            try
            {
                if (FindControlRecursive(this, "paginationControl") is PaginationControl paginationControl)
                {
                    List<CertificadoDTO> datosFiltradosActuales = ObtenerDatosFiltradosActuales();
                    var subtotal = datosFiltradosActuales.Sum(c => c.MontoTotal);
                    var cantidad = datosFiltradosActuales.Count;

                    paginationControl.UpdateSubtotal(subtotal, cantidad);
                }
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error al calcular subtotal para PaginationControl: " + ex.Message);
            }
        }

        
        /// <summary>
        /// Busca un control recursivamente en la jerarquía de controles.
        /// Método auxiliar para trabajar sin designer regenerado.
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
        /// Manejador genérico para aceptar cambios en filtros TreeViewSearch.
        /// Replicado exactamente desde AutorizantesAdminEF.
        /// 
        /// FUNCIONALIDAD:
        /// - Se dispara cuando el usuario confirma cambios en filtros del header
        /// - Reinicia la paginación a la primera página
        /// - Reaplica filtros y refresca la visualización
        /// 
        /// INTEGRACIÓN:
        /// - Conectado con controles TreeViewSearch del header del GridView
        /// - Permite filtrado dinámico sin recargar datos de BD
        /// </summary>
        public void OnAcceptChanges(object sender, EventArgs e)
        {
            currentPageIndex = 0; // Reiniciar a la primera página al aplicar filtros
            ViewState["CurrentPageIndex"] = currentPageIndex; // Actualizar ViewState
            CargarPaginaActual(); // Cargar datos filtrados y paginados
        }

        #region Eventos del Control de Paginación

        protected void paginationControl_PageChanged(object sender, PaginationEventArgs e)
        {
            currentPageIndex = e.PageIndex;
            CargarPaginaActual();
        }

        protected void paginationControl_PageSizeChanged(object sender, PaginationEventArgs e)
        {
            pageSize = e.PageSize;
            ViewState["PageSize"] = pageSize; // Guardar el nuevo tamaño de página en ViewState
            currentPageIndex = 0; // Reiniciar a la primera página
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
                    // Solo si no hay caché, consultar BD
                    UsuarioEF usuario =  UserHelper.GetFullCurrentUser();
                    todosLosCertificados = calculoRedeterminacionNegocio.ListarCertificadosYReliquidaciones(usuario);
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
                
                todosLosCertificados = AplicarFiltrosTreeViewEnMemoria(todosLosCertificados);

                // Definir mapeo de columnas
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
                if (Session["EditingCertificadoId"] != null)
                {
                    // Recupera el certificado existente y modifica sus propiedades
                    int certificadoId = (int)Session["EditingCertificadoId"];
                    CertificadoEF certificadoExistente = negocio.ObtenerPorId(certificadoId);
                    
                    if (certificadoExistente == null)
                    {
                        lblMensaje.Text = "Error: No se encontró el certificado a modificar.";
                        lblMensaje.CssClass = "alert alert-danger";
                        return;
                    }

                    // Actualiza solo las propiedades editables
                    certificadoExistente.ExpedientePago = txtExpediente.Text.Trim();
                    certificadoExistente.MontoTotal = decimal.Parse(txtMontoAutorizado.Text.Trim());
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
                        ExpedientePago = txtExpediente.Text.Trim(),
                        MontoTotal = decimal.Parse(txtMontoAutorizado.Text.Trim()),
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
                    // Limpiar cache SADE ya que se agregó/modificó un certificado
                    CalculoRedeterminacionNegocioEF.LimpiarCacheSade();
                    
                    // Invalida caché para forzar recarga desde BD
                    Session["GridData"] = null;
                    ViewState["NecesitaRecarga"] = true;
                    
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "HideModal", "$('#modalAgregar').modal('hide');", true);
                    CargarPaginaActual(); // Recargará desde BD al no encontrar caché
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
            txtMontoAutorizado.Text = string.Empty;
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

                    txtMontoAutorizado.Text = certificadoSeleccionado.MontoTotal.ToString("0.00");

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

                    txtMontoAutorizado.Text = certificadoEF.MontoTotal.ToString("0.00");

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
                    
                    // Limpiar cache SADE ya que se eliminó un certificado
                    CalculoRedeterminacionNegocioEF.LimpiarCacheSade();
                    
                    // Eliminar registro específico del cache
                    if (Session["GridData"] is List<CertificadoDTO> listaCache)
                    {
                        // Remover el registro del cache
                        listaCache.RemoveAll(c => c.Id == id);
                        
                        // Actualizar totales
                        ViewState["TotalRecords"] = listaCache.Count;
                        totalRecords = listaCache.Count;
                    }
                    
                    // Recargar vista con datos actualizados
                    BindGrid();
                    ConfigurarPaginationControl();
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al eliminar el certificado: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        private void PoblarFiltrosHeader()
        {
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
                            // certEnListaCompleta = listaCompleta.FirstOrDefault(c => c.IdReliquidacion == certModificado.IdReliquidacion);
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



                    // Recalcular todos los certificados afectados y guardar en BD
                     calculoRedeterminacionNegocio.CalcularCertificadosPorExpedientes(
                        expedientesAfectados, 
                        listaCompleta,
                        persistirEnBD: false);

                    // AQUI DEBEMOS ACTUALIZAR EL Session GridData con los certificados recalculados

                    if (listaCompleta != null && datosFiltradosActuales != null)
                    {
                        foreach (var certModificado in datosFiltradosActuales)
                        {
                            CertificadoDTO certEnListaCompleta = null;
                            if (certModificado.TipoPagoId == 3) // Reliquidación
                            {
                                certEnListaCompleta = listaCompleta.FirstOrDefault(c =>
                                    c.IdReliquidacion == certModificado.IdReliquidacion);
                            }
                            else // Certificado normal
                            {
                                certEnListaCompleta = listaCompleta.FirstOrDefault(c => c.Id == certModificado.Id);
                            }

                            if (certEnListaCompleta != null)
                            {
                                certEnListaCompleta.Sigaf = certModificado.Sigaf;
                            }
                        }
                    }

                    CargarPaginaActual(); // Usar método optimizado

                    // Limpiar cache SADE ya que se modificó un expediente (certificado o reliquidación)
                    CalculoRedeterminacionNegocioEF.LimpiarCacheSade();

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
        /// Método auxiliar para obtener los datos filtrados que se están mostrando actualmente en el grid.
        /// Replica la misma lógica de filtrado que se usa en BindGrid().
        /// </summary>
        private List<CertificadoDTO> ObtenerDatosFiltradosActuales()
        {
            if (Session["GridData"] == null) return new List<CertificadoDTO>();

            var datosEnMemoria = (List<CertificadoDTO>)Session["GridData"];

            // Aplicar filtro de texto general (mismo que en BindGrid)
            string filtro = txtBuscar.Text.Trim().ToLower();
            if (!string.IsNullOrEmpty(filtro))
            {
                datosEnMemoria = datosEnMemoria.Where(c =>
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
            datosEnMemoria = AplicarFiltrosTreeViewEnMemoria(datosEnMemoria);

            return datosEnMemoria;
        }


        #region Eventos de Filtrado Optimizados
        /// <summary>
        /// Aplica filtros y reinicia la paginación.
        /// Replicado exactamente desde AutorizantesAdminEF.
        /// 
        /// COMPORTAMIENTO:
        /// - Reinicia currentPageIndex a 0 (primera página)
        /// - Ejecuta CargarPaginaActual() que aplica filtros y recarga
        /// - Los filtros se obtienen automáticamente de los controles TreeViewSearch
        /// 
        /// OPTIMIZACIÓN:
        /// - No consulta BD, trabaja con datos en Session
        /// - Filtros se aplican en memoria para rapidez
        /// </summary>
        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            currentPageIndex = 0; // Reiniciar a la primera página al aplicar filtros
            CargarPaginaActual();
        }

        /// <summary>
        /// Limpia todos los filtros aplicados y refresca la vista.
        /// Replicado exactamente desde AutorizantesAdminEF.
        /// 
        /// ACCIONES:
        /// - Limpia campo de búsqueda textual
        /// - Limpia todos los filtros TreeViewSearch de la página
        /// - Reinicia paginación a primera página
        /// - Recarga vista sin filtros
        /// 
        /// DEPENDENCIAS:
        /// - TreeViewSearch.ClearAllFiltersOnPage() para limpiar filtros
        /// - CargarPaginaActual() para refrescar vista
        /// </summary>
        protected void BtnClearFilters_Click(object sender, EventArgs e)
        {
            txtBuscar.Text = string.Empty;
            TreeViewSearch.ClearAllFiltersOnPage(this.Page);
            currentPageIndex = 0;
            CargarPaginaActual();
        }
        #endregion

        private List<CertificadoDTO> AplicarFiltrosTreeViewEnMemoria(List<CertificadoDTO> data)
        {
            if (gridviewRegistros.HeaderRow == null) return data;

            try
            {
                // Función auxiliar para parsear IDs de forma segura.
                Func<TreeViewSearch, List<int>> getSelectedIds = (tv) => tv.ExpandedSelectedValues.Select(s => int.TryParse(s, out int id) ? id : -1).Where(id => id != -1).ToList();

                // Aplicar filtro por Área
                if (gridviewRegistros.HeaderRow.FindControl("cblsHeaderArea") is TreeViewSearch cblsHeaderArea && cblsHeaderArea.ExpandedSelectedValues.Any())
                    data = data.Where(c => c.AreaId.HasValue && getSelectedIds(cblsHeaderArea).Contains(c.AreaId.Value)).ToList();

                // Aplicar filtro por Obra
                if (gridviewRegistros.HeaderRow.FindControl("cblsHeaderObra") is TreeViewSearch cblsHeaderObra && cblsHeaderObra.ExpandedSelectedValues.Any())
                    data = data.Where(c => c.ObraId.HasValue && getSelectedIds(cblsHeaderObra).Contains(c.ObraId.Value)).ToList();

                // Aplicar filtro por Barrio
                if (gridviewRegistros.HeaderRow.FindControl("cblsHeaderBarrio") is TreeViewSearch cblsHeaderBarrio && cblsHeaderBarrio.ExpandedSelectedValues.Any())
                    data = data.Where(c => c.BarrioId.HasValue && getSelectedIds(cblsHeaderBarrio).Contains(c.BarrioId.Value)).ToList();

                // Aplicar filtro por Proyecto (certificados y reliquidaciones)
                if (gridviewRegistros.HeaderRow.FindControl("cblsHeaderProyecto") is TreeViewSearch cblsHeaderProyecto && cblsHeaderProyecto.ExpandedSelectedValues.Any())
                {
                    var selectedProyectoIds = getSelectedIds(cblsHeaderProyecto);
                    
                    // Obtener todos los nombres de los proyectos seleccionados
                    using (var context = new IVCdbContext())
                    {
                        var selectedProjectNames = context.Proyectos
                            .Where(p => selectedProyectoIds.Contains(p.Id))
                            .Select(p => p.Nombre)
                            .Distinct()
                            .ToList();

                        // Obtener todos los IDs de proyectos que coincidan con esos nombres
                        var allMatchingProjectIds = context.Proyectos
                            .Where(p => selectedProjectNames.Contains(p.Nombre))
                            .Select(p => p.Id)
                            .Distinct()
                            .ToList();
                        
                        data = data.Where(c => c.ProyectoId.HasValue && allMatchingProjectIds.Contains(c.ProyectoId.Value)).ToList();
                    }
                }

                // Aplicar filtro por Empresa
                if (gridviewRegistros.HeaderRow.FindControl("cblsHeaderEmpresa") is TreeViewSearch cblsHeaderEmpresa && cblsHeaderEmpresa.ExpandedSelectedValues.Any())
                    data = data.Where(c => c.EmpresaId.HasValue && getSelectedIds(cblsHeaderEmpresa).Contains(c.EmpresaId.Value)).ToList();

                // Aplicar filtro por Código Autorizante
                if (gridviewRegistros.HeaderRow.FindControl("cblsHeaderCodigoAutorizante") is TreeViewSearch cblsHeaderCodigoAutorizante && cblsHeaderCodigoAutorizante.ExpandedSelectedValues.Any())
                    data = data.Where(c => c.AutorizanteId.HasValue && getSelectedIds(cblsHeaderCodigoAutorizante).Contains(c.AutorizanteId.Value)).ToList();

                // Aplicar filtro por Estado (ahora por texto, no por ID numérico)
                if (gridviewRegistros.HeaderRow.FindControl("cblsHeaderEstado") is TreeViewSearch cblsHeaderEstado)
                {
                    var selectedEstados = cblsHeaderEstado.ExpandedSelectedValues;
                    if (selectedEstados != null && selectedEstados.Any())
                    {
                        data = data.Where(c => !string.IsNullOrEmpty(c.Estado) && selectedEstados.Contains(c.Estado)).ToList();
                    }
                    // Si no hay filtros seleccionados, no se filtra por estado (se muestran todos)
                }

                // Aplicar filtro por Tipo
                if (gridviewRegistros.HeaderRow.FindControl("cblsHeaderTipo") is TreeViewSearch cblsHeaderTipo && cblsHeaderTipo.ExpandedSelectedValues.Any())
                    data = data.Where(c => c.TipoPagoId.HasValue && getSelectedIds(cblsHeaderTipo).Contains(c.TipoPagoId.Value)).ToList();

                // Aplicar filtro por Mes Certificado
                if (gridviewRegistros.HeaderRow.FindControl("cblsHeaderMesCertificado") is TreeViewSearch cblsHeaderMesCertificado && cblsHeaderMesCertificado.SelectedValues.Any())
                {
                    var selectedDates = cblsHeaderMesCertificado.SelectedValues
                        .Select(s => DateTime.TryParse(s, out DateTime dt) ? (DateTime?)dt.Date : null)
                        .Where(d => d.HasValue).Select(d => d.Value)
                        .ToList();
                    data = data.Where(c => c.MesAprobacion.HasValue && selectedDates.Contains(c.MesAprobacion.Value.Date)).ToList();
                }

                // Aplicar filtro por Línea de Gestión
                if (gridviewRegistros.HeaderRow.FindControl("cblsHeaderLinea") is TreeViewSearch cblsHeaderLinea && cblsHeaderLinea.ExpandedSelectedValues.Any())
                    data = data.Where(c => c.LineaGestionId.HasValue && getSelectedIds(cblsHeaderLinea).Contains(c.LineaGestionId.Value)).ToList();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine($"Error aplicando filtros en memoria: {e}");
            }

            return data;
        }


    }
}
