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
    /// Página administrativa para la gestión completa de Autorizantes y Redeterminaciones.
    /// 
    /// FUNCIONALIDADES PRINCIPALES:
    /// - Visualización unificada de autorizantes y redeterminaciones en un GridView
    /// - Paginación externa personalizada (independiente del GridView nativo)
    /// - Filtrado avanzado con TreeViewSearch en múltiples campos
    /// - Estado editable mediante DropDownList con persistencia en base de datos
    /// - Exportación a Excel con datos completos
    /// - Modal para agregar/editar autorizantes
    /// - Cálculo automático de subtotales de montos
    /// - Integración con datos SADE y SIGAF
    /// 
    /// ARQUITECTURA DE DATOS:
    /// - Los datos se cargan una vez y se almacenan en Session["GridDataAutorizantes"]
    /// - Se combinan autorizantes reales con redeterminaciones calculadas
    /// - Los filtros se aplican en memoria para mejor rendimiento
    /// - La paginación se maneja con ViewState para mantener estado entre postbacks
    /// 
    /// FLUJO DE TRABAJO:
    /// 1. Page_Load: Inicializa datos y controles de paginación
    /// 2. CargarListaAutorizantesRedet(): Obtiene todos los datos y los guarda en Session
    /// 3. BindGrid(): Aplica filtros y paginación a los datos en memoria
    /// 4. Eventos de paginación: Navegan entre páginas sin recargar datos de BD
    /// 5. Filtros TreeViewSearch: Se aplican en memoria para filtrado rápido
    /// 6. Estado editable: Se actualiza directamente en BD y se refleja en memoria
    /// </summary>
    public partial class AutorizantesEF : System.Web.UI.Page
    {
        #region Variables y Dependencias
        
        /// <summary>
        /// Clase de negocio para operaciones CRUD de autorizantes.
        /// Maneja las operaciones básicas como agregar, modificar, eliminar y consultar autorizantes.
        /// </summary>
        private AutorizanteNegocioEF autorizanteNegocio = new AutorizanteNegocioEF();
        
        /// <summary>
        /// Clase de negocio para cálculos complejos de redeterminaciones.
        /// Responsable de:
        /// - Generar redeterminaciones virtuales a partir de autorizantes
        /// - Calcular montos y porcentajes de redeterminaciones
        /// - Integrar datos SADE y SIGAF
        /// - Manejar la paginación optimizada de grandes volúmenes de datos
        /// </summary>
        private CalculoRedeterminacionNegocioEF calculoRedeterminacionNegocio = new CalculoRedeterminacionNegocioEF();

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
        /// Total de registros disponibles (autorizantes + redeterminaciones).
        /// Se calcula una vez y se almacena en ViewState para evitar recálculos.
        /// </summary>
        private int totalRecords = 0;
        
        /// <summary>
        /// Total de páginas calculado a partir de totalRecords / pageSize.
        /// Se recalcula automáticamente cuando cambian los totales o el tamaño de página.
        /// </summary>
        private int totalPages = 0;

        #endregion

        #region Eventos del Ciclo de Vida de la Página

        /// <summary>
        /// Evento PreRender: Se ejecuta justo antes de renderizar la página.
        /// 
        /// PROPÓSITO:
        /// - Controla la habilitación de validadores según el estado de edición
        /// - Evita validaciones innecesarias cuando se está editando un autorizante existente
        /// 
        /// LÓGICA:
        /// - Si Session["EditingAutorizanteId"] tiene valor, estamos editando (no validar Obra)
        /// - Si es null, estamos agregando un nuevo autorizante (validar Obra obligatoria)
        /// </summary>
        protected void Page_PreRender(object sender, EventArgs e)
        {
            // Habilitar validadores solo cuando NO estemos editando
            rfvObra.Enabled = Session["EditingAutorizanteId"] == null;
        }

        /// <summary>
        /// Evento principal de carga de la página.
        /// 
        /// FLUJO DE INICIALIZACIÓN:
        /// 1. Restaura valores de paginación desde ViewState (persistencia entre postbacks)
        /// 2. En primer carga (!IsPostBack):
        ///    - Carga todos los datos desde BD y los almacena en Session
        ///    - Inicializa dropdowns del modal de agregar/editar
        ///    - Calcula subtotales de montos
        ///    - Configura controles de paginación externa
        /// 3. En postbacks:
        ///    - Solo actualiza controles de paginación (datos ya en memoria)
        /// 
        /// OPTIMIZACIÓN:
        /// - Los datos solo se cargan una vez por sesión
        /// - ViewState mantiene estado de paginación
        /// - No hay consultas a BD en postbacks rutinarios
        /// </summary>
        protected void Page_Load(object sender, EventArgs e)
        {
            // Cargar valores de paginación desde ViewState
            currentPageIndex = (int)(ViewState["CurrentPageIndex"] ?? 0);
            pageSize = (int)(ViewState["PageSize"] ?? 12);

            if (!IsPostBack)
            {
                CargarListaAutorizantesRedet(); // Ya calcula subtotal internamente
                ObtenerDropDownLists();
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
        /// <summary>
        /// Manejador genérico para aceptar cambios en filtros TreeViewSearch.
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
        protected void OnAcceptChanges(object sender, EventArgs e)
        {
            // Reiniciar a la primera página al aplicar filtros
            var paginationControl = FindControlRecursive(this, "paginationControl") as CustomControls.PaginationControl;
            if (paginationControl != null)
            {
                paginationControl.CurrentPageIndex = 0;
                paginationControl.UpdatePaginationControls();
            }
            BindGrid(); // Cargar datos filtrados y paginados
        }

        /// <summary>
        /// Exporta los datos actuales del GridView a un archivo Excel.
        /// 
        /// CARACTERÍSTICAS:
        /// - Exporta TODOS los datos (no solo la página actual)
        /// - Aplica filtros activos al momento de la exportación
        /// - Incluye mapeo de columnas legibles para Excel
        /// - Maneja errores y muestra mensajes apropiados
        /// 
        /// DATOS EXPORTADOS:
        /// - Información completa de autorizantes y redeterminaciones
        /// - Campos calculados como SIGAF, SADE, estados mapeados
        /// - Formato amigable para análisis en Excel
        /// 
        /// DEPENDENCIAS:
        /// - Requiere Negocio.ExcelHelper para la generación del archivo
        /// - Utiliza Session["GridDataAutorizantes"] como fuente de datos
        /// </summary>
        protected void btnExportarExcel_Click(object sender, EventArgs e)
        {
            try
            {
                // Usar la misma fuente de datos que la grilla para consistencia
                if (Session["GridDataAutorizantes"] == null) return;
                var datosParaExportar = (List<AutorizanteDTO>)Session["GridDataAutorizantes"];

                // Aplicar los mismos filtros que tiene la grilla
                string filtro = txtBuscar.Text.Trim().ToLower();
                if (!string.IsNullOrEmpty(filtro))
                {
                    datosParaExportar = datosParaExportar.Where(a =>
                        (a.CodigoAutorizante?.ToLower().Contains(filtro) ?? false) ||
                        (a.Detalle?.ToLower().Contains(filtro) ?? false) ||
                        (a.Expediente?.ToLower().Contains(filtro) ?? false) ||
                        (a.EmpresaNombre?.ToLower().Contains(filtro) ?? false) ||
                        (a.ObraDescripcion?.ToLower().Contains(filtro) ?? false) ||
                        (a.AreaNombre?.ToLower().Contains(filtro) ?? false) ||
                        (a.BarrioNombre?.ToLower().Contains(filtro) ?? false) ||
                        (a.ConceptoNombre?.ToLower().Contains(filtro) ?? false) ||
                        (a.EstadoNombre?.ToLower().Contains(filtro) ?? false)
                    ).ToList();
                }

                datosParaExportar = AplicarFiltrosTreeViewEnMemoria(datosParaExportar);

                // Definir mapeo de columnas
                var mapeoColumnas = new Dictionary<string, string>
                {
                    { "Área", "AreaNombre" },
                    { "Area", "AreaNombre" }, // Versión sin acento
                    { "Obra", "ObraDescripcion" },
                    { "Contrata", "ContrataNombre" },
                    { "Empresa", "EmpresaNombre" },
                    { "Código Autorizante", "CodigoAutorizante" },
                    { "Expediente", "Expediente" },
                    { "Detalle", "Detalle" },
                    { "Monto Autorizado", "MontoAutorizado" },
                    { "Fecha", "Fecha" },
                    { "Mes Base", "MesBase" },
                    { "Concepto", "ConceptoNombre" },
                    { "Estado", "EstadoNombre" },
                    { "Barrio", "BarrioNombre" },
                    { "Proyecto", "ProyectoNombre" },
                    { "Buzón SADE", "BuzonSade" },
                    { "Fecha SADE", "FechaSade" }
                };

                Negocio.ExcelHelper.ExportarDatosGenericos(gridviewRegistros, datosParaExportar, mapeoColumnas, "Autorizantes");
            }
            catch (Exception ex)
            {
                lblMensaje.Text = "Error al exportar: " + ex.Message;
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        /// <summary>
        /// Abre el modal para agregar un nuevo autorizante.
        /// 
        /// CONFIGURACIÓN DEL MODAL:
        /// - Limpia todos los campos del formulario
        /// - Establece título como "Agregar Autorizante"
        /// - Muestra el dropdown de Obra (requerido para nuevos)
        /// - Configura botón como "Agregar"
        /// - Limpia cualquier estado de edición previo
        /// 
        /// TECNOLOGÍA:
        /// - Utiliza JavaScript/jQuery para manipular el modal Bootstrap
        /// - ScriptManager para ejecutar scripts desde servidor
        /// - Session para limpiar estados de edición
        /// </summary>
        protected void btnShowAddModal_Click(object sender, EventArgs e)
        {
            // Limpiar datos existentes
            LimpiarFormulario();

            txtCodigoAutorizante.Enabled = true; // Habilitar campo de código para nuevos autorizantes

            // Reiniciar el título del modal y texto del botón a "Agregar" y mostrar campo Obra
            ScriptManager.RegisterStartupScript(this, this.GetType(), "ResetModalTitleAndShow", @"
                $(document).ready(function() {
                    $('#modalAgregar .modal-title').text('Agregar Autorizante');
                    document.getElementById('" + Button1.ClientID + @"').value = 'Agregar';
                    
                    // Mostrar el dropdown de Obra y su etiqueta
                    $('#obraContainer').show();
                    
                    // Mostrar el modal
                    $('#modalAgregar').modal('show');
                });", true);

            Button1.Text = "Agregar";

            // Limpiar cualquier estado de edición
            Session["EditingAutorizanteId"] = null;
            Session["EditingCodigoAutorizante"] = null;

        }

        /// <summary>
        /// Limpia todos los campos del formulario modal.
        /// 
        /// CAMPOS AFECTADOS:
        /// - Campos de texto: código, expediente, detalle, monto, fecha, mes
        /// - Dropdowns: obra, concepto, estado (vuelven a opción por defecto)
        /// 
        /// USO:
        /// - Al abrir modal de agregar
        /// - Después de operaciones exitosas
        /// - Al presionar botón limpiar explícitamente
        /// </summary>
        protected void btnLimpiar_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();
        }

        /// <summary>
        /// Aplica filtros y reinicia la paginación.
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
            // Reiniciar a la primera página al aplicar filtros
            var paginationControl = FindControlRecursive(this, "paginationControl") as CustomControls.PaginationControl;
            if (paginationControl != null)
            {
                paginationControl.CurrentPageIndex = 0;
                paginationControl.UpdatePaginationControls();
            }
            BindGrid();
        }

        /// <summary>
        /// Limpia todos los filtros aplicados y refresca la vista.
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
            // Reiniciar a la primera página
            var paginationControl = FindControlRecursive(this, "paginationControl") as CustomControls.PaginationControl;
            if (paginationControl != null)
            {
                paginationControl.CurrentPageIndex = 0;
                paginationControl.UpdatePaginationControls();
            }
            BindGrid();
        }

        /// <summary>
        /// Método obsoleto mantenido por compatibilidad.
        /// 
        /// ESTADO: DEPRECADO
        /// - Redirige al método ddlPageSizeExternal_SelectedIndexChanged
        /// - Se mantiene para evitar errores en controles existentes
        /// - Nuevo código debe usar ddlPageSizeExternal_SelectedIndexChanged
        /// 
        /// FUNCIONALIDAD:
        /// - Cambia el tamaño de página según selección del usuario
        /// - Reinicia paginación a primera página
        /// </summary>
        protected void ddlPageSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Este método ahora está obsoleto, se usa ddlPageSizeExternal_SelectedIndexChanged
            // Mantenemos por compatibilidad pero redirigimos al nuevo método
            DropDownList ddl = sender as DropDownList;
            if (ddl != null)
            {
                var paginationControl = FindControlRecursive(this, "paginationControl") as CustomControls.PaginationControl;
                if (paginationControl != null)
                {
                    paginationControl.PageSize = int.Parse(ddl.SelectedValue);
                    paginationControl.CurrentPageIndex = 0; // Reiniciar a la primera página
                    paginationControl.UpdatePaginationControls();
                }
                BindGrid();
            }
        }

        #endregion

        #region Gestión de Autorizantes (CRUD)

        /// <summary>
        /// Maneja tanto la creación de nuevos autorizantes como la modificación de existentes.
        /// 
        /// LÓGICA DUAL:
        /// 1. MODO AGREGAR (Session["EditingAutorizanteId"] == null):
        ///    - Crea nuevo AutorizanteEF con datos del formulario
        ///    - Requiere selección de Obra (dropdown visible)
        ///    - Llama a autorizanteNegocio.Agregar()
        /// 
        /// 2. MODO EDITAR (Session["EditingAutorizanteId"] != null):
        ///    - Modifica autorizante existente con ID de Session
        ///    - NO requiere Obra (se mantiene la original)
        ///    - Llama a autorizanteNegocio.Modificar()
        /// 
        /// VALIDACIONES:
        /// - Page.IsValid verifica validadores del lado servidor
        /// - Campos requeridos según modo (Obra solo en agregar)
        /// - Conversión de tipos con manejo de errores
        /// 
        /// FLUJO POST-OPERACIÓN:
        /// - Limpia formulario y estado de edición
        /// - Resetea modal a modo "Agregar"
        /// - Recarga datos para reflejar cambios
        /// - Muestra mensajes de éxito/error apropiados
        /// 
        /// CAMPOS MANEJADOS:
        /// - Código Autorizante, Expediente, Detalle, Monto
        /// - Fecha de Aprobación y Mes Base (opcionales)
        /// - Concepto y Estado (requeridos)
        /// - Obra (solo en modo agregar)
        /// </summary>
        protected void btnAgregar_Click(object sender, EventArgs e)
        {
            // Verificar si la página es válida (todos los validadores pasaron)
            if (!Page.IsValid) return;

            try
            {
                // Verificar si estamos editando o agregando
                if (Session["EditingAutorizanteId"] != null)
                {
                    // MODO EDITAR: Obtener el autorizante existente de la BD
                    int autorizanteId = (int)Session["EditingAutorizanteId"];
                    var autorizanteExistente = autorizanteNegocio.ObtenerPorId(autorizanteId);
                    
                    if (autorizanteExistente == null)
                    {
                        lblMensaje.Text = "Error: No se encontró el autorizante a modificar.";
                        lblMensaje.CssClass = "alert alert-danger";
                        return;
                    }

                    // Actualizar solo los campos que se pueden modificar (NO incluir ObraId)
                    autorizanteExistente.CodigoAutorizante = txtCodigoAutorizante.Text.Trim();
                    autorizanteExistente.Expediente = txtExpediente.Text.Trim();
                    autorizanteExistente.Detalle = txtDetalle.Text.Trim();
                    autorizanteExistente.MontoAutorizado = Convert.ToDecimal(txtMontoAutorizado.Text);

                    // Parsear fecha si se proporciona
                    if (!string.IsNullOrEmpty(txtFecha.Text))
                    {
                        autorizanteExistente.MesAprobacion = DateTime.Parse(txtFecha.Text);
                    }
                    else
                    {
                        autorizanteExistente.MesAprobacion = null;
                    }

                    // Parsear mes base si se proporciona
                    if (!string.IsNullOrEmpty(txtMes.Text))
                    {
                        autorizanteExistente.MesBase = DateTime.Parse(txtMes.Text);
                    }
                    else
                    {
                        autorizanteExistente.MesBase = null;
                    }

                    autorizanteExistente.ConceptoId = int.Parse(ddlConcepto.SelectedValue);
                    autorizanteExistente.EstadoId = int.Parse(ddlEstado.SelectedValue);

                    // NO MODIFICAR autorizanteExistente.ObraId - se mantiene el original

                    if (autorizanteNegocio.Modificar(autorizanteExistente))
                    {
                        lblMensaje.Text = "Autorizante modificado exitosamente!";
                        lblMensaje.CssClass = "alert alert-success";

                        // Limpiar cache SADE ya que se modificó un autorizante
                        CalculoRedeterminacionNegocioEF.LimpiarCacheSade();

                        // Limpiar el estado de edición
                        Session["EditingAutorizanteId"] = null;
                    }
                    else
                    {
                        lblMensaje.Text = "Hubo un problema al modificar el autorizante.";
                        lblMensaje.CssClass = "alert alert-danger";
                    }
                }
                else
                {
                    // MODO AGREGAR: Crear nuevo autorizante con todos los campos
                    AutorizanteEF autorizante = new AutorizanteEF();
                    autorizante.CodigoAutorizante = txtCodigoAutorizante.Text.Trim();
                    autorizante.Expediente = txtExpediente.Text.Trim();
                    autorizante.Detalle = txtDetalle.Text.Trim();
                    autorizante.MontoAutorizado = Convert.ToDecimal(txtMontoAutorizado.Text);

                    // Parsear fecha si se proporciona
                    if (!string.IsNullOrEmpty(txtFecha.Text))
                    {
                        autorizante.MesAprobacion = DateTime.Parse(txtFecha.Text);
                    }

                    // Parsear mes base si se proporciona
                    if (!string.IsNullOrEmpty(txtMes.Text))
                    {
                        autorizante.MesBase = DateTime.Parse(txtMes.Text);
                    }

                    autorizante.ConceptoId = int.Parse(ddlConcepto.SelectedValue);
                    autorizante.EstadoId = int.Parse(ddlEstado.SelectedValue);
                    
                    // Solo al agregar se asigna la obra seleccionada
                    autorizante.ObraId = int.Parse(ddlObra.SelectedValue);

                    if (autorizanteNegocio.Agregar(autorizante))
                    {
                        lblMensaje.Text = "Autorizante agregado exitosamente!";
                        lblMensaje.CssClass = "alert alert-success";
                    }
                    else
                    {
                        lblMensaje.Text = "Hubo un problema al agregar el autorizante.";
                        lblMensaje.CssClass = "alert alert-danger";
                    }
                }

                // Limpiar campos
                LimpiarFormulario();

                // Reiniciar el título del modal y texto del botón
                ScriptManager.RegisterStartupScript(this, this.GetType(), "ResetModalTitle",
                    "$('#modalAgregar .modal-title').text('Agregar Autorizante');", true);
                Button1.Text = "Agregar";

                // Ocultar el modal
                ScriptManager.RegisterStartupScript(this, this.GetType(), "HideModal",
                    "$('#modalAgregar').modal('hide');", true);

                // Limpiar cache SADE ya que se agregó/modificó un autorizante
                CalculoRedeterminacionNegocioEF.LimpiarCacheSade();

                // Limpiar cache de datos para forzar recarga desde BD
                Session["GridDataAutorizantesTotal"] = null;

                // Refrescar la lista de autorizantes
                CargarListaAutorizantesRedet(); // Recarga desde BD
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        /// <summary>
        /// Limpia todos los campos del formulario modal de agregar/editar autorizante.
        /// 
        /// CAMPOS LIMPIADOS:
        /// - Campos de texto: CodigoAutorizante, Expediente, Detalle, MontoAutorizado, Fecha, Mes
        /// - Dropdowns: Obra, Concepto, Estado (vuelven al índice 0 - opción por defecto)
        /// 
        /// USO:
        /// - Al abrir modal para agregar nuevo autorizante
        /// - Después de operaciones exitosas (agregar/modificar)
        /// - Al presionar botón "Limpiar" explícitamente
        /// - Para resetear estado del formulario
        /// 
        /// NOTAS:
        /// - No afecta el estado de Session["EditingAutorizanteId"]
        /// - Los dropdowns mantienen sus opciones, solo cambia la selección
        /// </summary>
        private void LimpiarFormulario()
        {
            txtCodigoAutorizante.Text = string.Empty;
            txtExpediente.Text = string.Empty;
            txtDetalle.Text = string.Empty;
            txtMontoAutorizado.Text = string.Empty;
            txtFecha.Text = string.Empty;
            txtMes.Text = string.Empty;
            ddlObra.SelectedIndex = 0;
            ddlConcepto.SelectedIndex = 0;
            ddlEstado.SelectedIndex = 0;
        }

        /// <summary>
        /// Método orquestador que inicializa todos los dropdowns del formulario modal.
        /// 
        /// DROPDOWNS INICIALIZADOS:
        /// - ddlObra: Lista de obras disponibles
        /// - ddlConcepto: Lista de conceptos disponibles  
        /// - ddlEstado: Lista de estados de autorizante
        /// 
        /// PATRÓN DE DISEÑO:
        /// - Divide la responsabilidad en métodos específicos
        /// - Facilita mantenimiento y testing individual
        /// - Centraliza la inicialización de controles
        /// 
        /// LLAMADO DESDE:
        /// - Page_Load (solo en !IsPostBack)
        /// - Operaciones que requieren refrescar dropdowns
        /// </summary>
        private void ObtenerDropDownLists()
        {
            ObtenerObras();
            ObtenerConceptos();
            ObtenerEstados();
        }

        #endregion

        #region Filtros y Búsqueda

        /// <summary>
        /// Pobla los filtros TreeViewSearch del header del GridView con datos de BD.
        /// 
        /// FILTROS CONFIGURADOS:
        /// - Área, Obra, Barrio, Proyecto, Empresa, Contrata: Consultas directas a BD
        /// - Concepto: Incluye conceptos normales + "REDETERMINACION" especial
        /// - Estado: Usa CalculoRedeterminacionNegocioEF.ObtenerEstadosParaFiltro()
        /// - Mes Autorizante: Valores únicos de MesBase de autorizantes existentes
        /// 
        /// PATRÓN DE IMPLEMENTACIÓN:
        /// - Función lambda bindFilter para reutilizar lógica
        /// - Consultas AsNoTracking() para optimizar performance
        /// - Ordenamiento alfabético en la mayoría de filtros
        /// - Manejo especial para conceptos (incluye REDETERMINACION)
        /// 
        /// OPTIMIZACIÓN:
        /// - Single using context para todas las consultas
        /// - AsNoTracking() evita tracking innecesario de entities
        /// - Select anónimo para obtener solo campos necesarios
        /// 
        /// DEPENDENCIAS:
        /// - RequierE GridView.HeaderRow != null (llamar después de DataBind)
        /// - TreeViewSearch controls en template del header
        /// - CalculoRedeterminacionNegocioEF para estados especiales
        /// </summary>
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
                bindFilter("cblsHeaderCodigoAutorizante", context.Autorizantes.AsNoTracking().OrderBy(a => a.CodigoAutorizante).Select(a => new { a.CodigoAutorizante }).Distinct().ToList(),"CodigoAutorizante","CodigoAutorizante"
);                bindFilter("cblsHeaderContrata", context.Contratas.AsNoTracking().OrderBy(c => c.Nombre).Select(c => new { c.Id, c.Nombre }).ToList(), "Nombre", "Id");
                
                // Filtro de Concepto con "REDETERMINACION" incluida
                if (gridviewRegistros.HeaderRow?.FindControl("cblsHeaderConcepto") is TreeViewSearch cblsHeaderConcepto)
                {
                    var conceptos = context.Conceptos.AsNoTracking().OrderBy(c => c.Nombre).Select(c => new { Id = c.Id.ToString(), Nombre = c.Nombre }).ToList();
                    // Agregar "REDETERMINACION" como concepto especial
                    conceptos.Add(new { Id = "REDETERMINACION", Nombre = "REDETERMINACION" });
                    conceptos = conceptos.OrderBy(c => c.Nombre).ToList();
                    
                    cblsHeaderConcepto.DataSource = conceptos;
                    cblsHeaderConcepto.DataTextField = "Nombre";
                    cblsHeaderConcepto.DataValueField = "Id";
                    cblsHeaderConcepto.DataBind();
                }

                if (gridviewRegistros.HeaderRow?.FindControl("cblsHeaderEstado") is TreeViewSearch cblsHeaderEstado)
                {
                    var negocio = new CalculoRedeterminacionNegocioEF();
                    cblsHeaderEstado.DataSource = negocio.ObtenerEstadosParaFiltro();
                    cblsHeaderEstado.DataTextField = "Nombre";
                    cblsHeaderEstado.DataValueField = "Id";
                    cblsHeaderEstado.DataBind();
                }

                if (gridviewRegistros.HeaderRow?.FindControl("cblsHeaderMesAutorizante") is TreeViewSearch cblsHeaderMesAutorizante)
                {
                    var meses = context.Autorizantes.AsNoTracking()
                        .Where(a => a.MesBase.HasValue)
                        .Select(a => a.MesBase.Value)
                        .Distinct()
                        .OrderBy(d => d)
                        .ToList();
                    cblsHeaderMesAutorizante.DataSource = meses;
                    cblsHeaderMesAutorizante.DataBind();
                }
            }
        }

        /// <summary>
        /// Carga y configura el dropdown de obras disponibles para el formulario modal.
        /// 
        /// FUNCIONALIDAD:
        /// - Consulta todas las obras usando ObraNegocioEF.ListarParaDDL()
        /// - Configura DataTextField como "Descripcion" y DataValueField como "Id"
        /// - Agrega opción por defecto "-- Seleccionar Obra --" con valor "0"
        /// 
        /// USO:
        /// - Se ejecuta en inicialización de página (!IsPostBack)
        /// - Obra es requerida solo al AGREGAR autorizantes (no al editar)
        /// - El dropdown se oculta en modo edición
        /// 
        /// MANEJO DE ERRORES:
        /// - Captura excepciones y muestra mensaje en lblMensaje
        /// - Permite que la página continúe funcionando aunque falle la carga
        /// </summary>
        private void ObtenerObras()
        {
            try
            {
                ObraNegocioEF obraNegocio = new ObraNegocioEF();
                ddlObra.DataSource = obraNegocio.ListarParaDDL();
                ddlObra.DataTextField = "Descripcion";
                ddlObra.DataValueField = "Id";
                ddlObra.DataBind();

                // Agregar opción por defecto
                ddlObra.Items.Insert(0, new ListItem("-- Seleccionar Obra --", "0"));
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cargar obras: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        /// <summary>
        /// Carga y configura el dropdown de conceptos disponibles para el formulario modal.
        /// 
        /// FUNCIONALIDAD:
        /// - Consulta todos los conceptos usando ConceptoNegocioEF.Listar()
        /// - Configura DataTextField como "Nombre" y DataValueField como "Id"
        /// - Agrega opción por defecto "-- Seleccionar Concepto --" con valor "0"
        /// 
        /// USO:
        /// - Se ejecuta en inicialización de página (!IsPostBack)
        /// - Concepto es requerido tanto para agregar como editar autorizantes
        /// - Los conceptos son definidos en la tabla Conceptos de BD
        /// 
        /// DIFERENCIACIÓN:
        /// - Este dropdown es para autorizantes manuales
        /// - Las redeterminaciones automáticamente usan "REDETERMINACION" como concepto
        /// 
        /// MANEJO DE ERRORES:
        /// - Captura excepciones y muestra mensaje en lblMensaje
        /// - Permite que la página continúe funcionando aunque falle la carga
        /// </summary>
        private void ObtenerConceptos()
        {
            try
            {
                ConceptoNegocioEF conceptoNegocio = new ConceptoNegocioEF();
                ddlConcepto.DataSource = conceptoNegocio.Listar();
                ddlConcepto.DataTextField = "Nombre";
                ddlConcepto.DataValueField = "Id";
                ddlConcepto.DataBind();

                // Agregar opción por defecto
                ddlConcepto.Items.Insert(0, new ListItem("-- Seleccionar Concepto --", "0"));
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cargar conceptos: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        /// <summary>
        /// Carga y configura el dropdown de estados disponibles para el formulario modal.
        /// 
        /// FUNCIONALIDAD:
        /// - Consulta todos los estados usando EstadoAutorizanteNegocioEF.Listar()
        /// - Configura DataTextField como "Nombre" y DataValueField como "Id"
        /// - Agrega opción por defecto "-- Seleccionar Estado --" con valor "0"
        /// 
        /// USO:
        /// - Se ejecuta en inicialización de página (!IsPostBack)
        /// - Estado es requerido tanto para agregar como editar autorizantes
        /// - Los estados se usan también en redeterminaciones (campo editable)
        /// 
        /// ESTADOS TÍPICOS:
        /// - APROBADO, EN TRAMITE, DESESTIMADO, NO INICIADO (según mapeo en CalculoRedeterminacionNegocioEF)
        /// 
        /// MANEJO DE ERRORES:
        /// - Captura excepciones y muestra mensaje en lblMensaje
        /// - Permite que la página continúe funcionando aunque falle la carga
        /// </summary>
        private void ObtenerEstados()
        {
            try
            {
                EstadoAutorizanteNegocioEF estadoNegocio = new EstadoAutorizanteNegocioEF();
                ddlEstado.DataSource = estadoNegocio.Listar();
                ddlEstado.DataTextField = "Nombre";
                ddlEstado.DataValueField = "Id";
                ddlEstado.DataBind();

                // Agregar opción por defecto
                ddlEstado.Items.Insert(0, new ListItem("-- Seleccionar Estado --", "0"));
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cargar estados: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        #endregion

        #region Carga de Datos Principal

        /// <summary>
        /// Método central que carga datos paginados de autorizantes y redeterminaciones.
        /// 
        /// ARQUITECTURA DE PAGINACIÓN REAL:
        /// 1. Consulta totalRecords con ContarTotalAutorizantes() (count optimizado)
        /// 2. Carga solo pageSize registros usando ListarAutorizantesPaginados() 
        /// 3. Guarda total en ViewState["TotalRecords"] para cálculos de paginación
        /// 4. Guarda datos de página actual en Session["GridDataAutorizantes"]
        /// 
        /// DATOS COMBINADOS:
        /// - Autorizantes reales de tabla Autorizantes
        /// - Redeterminaciones calculadas dinámicamente (virtuales)
        /// - Integración de datos SADE y SIGAF
        /// - Estados mapeados según lógica de negocio
        /// 
        /// OPTIMIZACIONES IMPLEMENTADAS:
        /// - Paginación en BD (no carga todos los registros)
        /// - Variables externas currentPageIndex y pageSize
        /// - Consulta de conteo separada (más eficiente)
        /// - Session para evitar recálculos en filtros locales
        /// 
        /// FLUJO POST-CARGA:
        /// 1. DataBind() al GridView con datos paginados
        /// 2. PoblarFiltrosHeader() para filtros TreeViewSearch
        /// 3. CalcularSubtotal() para totales de montos
        /// 
        /// DEPENDENCIAS:
        /// - CalculoRedeterminacionNegocioEF para datos combinados
        /// - ObtenerUsuarioActual() para filtro por área
        /// - Variables currentPageIndex, pageSize (paginación externa)
        /// 
        /// DIFERENCIAS CON OTROS MÓDULOS:
        /// - Otros módulos cargan todo en memoria y paginan en cliente
        /// - Este módulo pagina en servidor para manejar grandes volúmenes
        /// - Session contiene solo datos de página actual (no todo el dataset)
        /// </summary>
        private void CargarListaAutorizantesRedet()
        {
            try
            {
                // Obtener información de paginación del control
                var paginationControl = FindControlRecursive(this, "paginationControl") as CustomControls.PaginationControl;
                int actualPageIndex = 0;
                int actualPageSize = 12;
                
                if (paginationControl != null)
                {
                    var paginationInfo = paginationControl.GetPaginationInfo();
                    actualPageIndex = paginationInfo.CurrentPageIndex;
                    actualPageSize = paginationInfo.PageSize;
                    
                    // Sincronizar variables de clase
                    currentPageIndex = actualPageIndex;
                    pageSize = actualPageSize;
                }

                // Usar la lista completa de la sesión
                List<AutorizanteDTO> listaCompleta = Session["GridDataAutorizantesTotal"] as List<AutorizanteDTO>;
                if (listaCompleta == null)
                {
                    // Recargar si por algún motivo no está en sesión
                    UsuarioEF usuario = ObtenerUsuarioActual();
                    listaCompleta = calculoRedeterminacionNegocio.ListarAutorizantesYRedeterminaciones(usuario);
                    Session["GridDataAutorizantesTotal"] = listaCompleta;
                }

                // Aplicar filtro de búsqueda
                string filtro = txtBuscar.Text.Trim().ToLower();
                var datosFiltrados = listaCompleta;
                if (!string.IsNullOrEmpty(filtro))
                {
                    datosFiltrados = datosFiltrados.Where(a =>
                        (a.CodigoAutorizante?.ToLower().Contains(filtro) ?? false) ||
                        (a.Detalle?.ToLower().Contains(filtro) ?? false) ||
                        (a.Expediente?.ToLower().Contains(filtro) ?? false) ||
                        (a.EmpresaNombre?.ToLower().Contains(filtro) ?? false) ||
                        (a.ObraDescripcion?.ToLower().Contains(filtro) ?? false) ||
                        (a.AreaNombre?.ToLower().Contains(filtro) ?? false) ||
                        (a.BarrioNombre?.ToLower().Contains(filtro) ?? false) ||
                        (a.ConceptoNombre?.ToLower().Contains(filtro) ?? false) ||
                        (a.EstadoNombre?.ToLower().Contains(filtro) ?? false)
                    ).ToList();
                }

                // Aplicar filtros de TreeView
                datosFiltrados = AplicarFiltrosTreeViewEnMemoria(datosFiltrados);



                // Guardar en sesión la lista filtrada
                Session["GridDataAutorizantes"] = datosFiltrados;
                Session["TotalRegistros"] = datosFiltrados.Count;

                // Paginación manual usando valores actualizados
                int totalFiltrados = datosFiltrados.Count;
                totalRecords = totalFiltrados;
                ViewState["TotalRecords"] = totalFiltrados;

                // Usar las variables sincronizadas
                var paginaActual = datosFiltrados
                    .Skip(actualPageIndex * actualPageSize)
                    .Take(actualPageSize)
                    .ToList();

                // Actualizar el control con el total correcto ANTES del DataBind
                if (paginationControl != null)
                {
                    paginationControl.TotalRecords = totalFiltrados;
                    paginationControl.UpdatePaginationControls();
                }

                gridviewRegistros.DataSource = paginaActual;
                gridviewRegistros.DataBind();

                PoblarFiltrosHeader();
                CalcularSubtotalParaPaginationControl();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cargar autorizantes: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        /// <summary>
        /// Método central para refrescar la visualización del GridView.
        /// Lee los datos desde Session, aplica los filtros y la paginación actuales.
        /// </summary>
        private void BindGrid()
        {
            if (Session["GridDataAutorizantes"] == null) return;

            var datosEnMemoria = (List<AutorizanteDTO>)Session["GridDataAutorizantes"];

            // Aplicar filtro de texto general
            string filtro = txtBuscar.Text.Trim().ToLower();
            if (!string.IsNullOrEmpty(filtro))
            {
                datosEnMemoria = datosEnMemoria.Where(a =>
                    (a.CodigoAutorizante?.ToLower().Contains(filtro) ?? false) ||
                    (a.Detalle?.ToLower().Contains(filtro) ?? false) ||
                    (a.Expediente?.ToLower().Contains(filtro) ?? false) ||
                    (a.EmpresaNombre?.ToLower().Contains(filtro) ?? false) ||
                    (a.ObraDescripcion?.ToLower().Contains(filtro) ?? false) ||
                    (a.AreaNombre?.ToLower().Contains(filtro) ?? false) ||
                    (a.BarrioNombre?.ToLower().Contains(filtro) ?? false) ||
                    (a.ConceptoNombre?.ToLower().Contains(filtro) ?? false) ||
                    (a.EstadoNombre?.ToLower().Contains(filtro) ?? false)
                ).ToList();
            }

            // Aplicar filtros de las columnas (TreeView)
            datosEnMemoria = AplicarFiltrosTreeViewEnMemoria(datosEnMemoria);

            // Obtener información de paginación del control
            var paginationControl = FindControlRecursive(this, "paginationControl") as CustomControls.PaginationControl;
            if (paginationControl != null)
            {
                var paginationInfo = paginationControl.GetPaginationInfo();
                
                // Configurar paginación en el GridView
                int totalFiltrados = datosEnMemoria.Count;
                gridviewRegistros.VirtualItemCount = totalFiltrados;
                gridviewRegistros.PageSize = paginationInfo.PageSize;
                gridviewRegistros.PageIndex = paginationInfo.CurrentPageIndex;
                gridviewRegistros.DataSource = datosEnMemoria
                                            .Skip(paginationInfo.Skip)
                                            .Take(paginationInfo.Take)
                                            .ToList();
                
                // Actualizar total de registros en el control
                paginationControl.TotalRecords = totalFiltrados;
                paginationControl.UpdatePaginationControls();
            }
            else
            {
                // Fallback si no se encuentra el control
                gridviewRegistros.DataSource = datosEnMemoria.Take(12).ToList();
            }
            
            gridviewRegistros.DataBind();
            PoblarFiltrosHeader();
            
            // Recalcular subtotal después de aplicar filtros
            CalcularSubtotalParaPaginationControl();
        }

        /// <summary>
        /// Método para vincular datos paginados directamente al GridView
        /// No aplica filtros en memoria ya que los datos vienen paginados de la base
        /// </summary>
        private List<AutorizanteDTO> AplicarFiltrosTreeViewEnMemoria(List<AutorizanteDTO> autorizantes)
        {
            try
            {
                // Aplicar filtro de Área
                if (gridviewRegistros.HeaderRow?.FindControl("cblsHeaderArea") is TreeViewSearch cblsHeaderArea && 
                    cblsHeaderArea.SelectedValues?.Any() == true)
                {
                    var areasSeleccionadas = cblsHeaderArea.SelectedValues.Select(int.Parse).ToList();
                    autorizantes = autorizantes.Where(a => a.AreaId.HasValue && areasSeleccionadas.Contains(a.AreaId.Value)).ToList();
                }

                // Aplicar filtro de Obra
                if (gridviewRegistros.HeaderRow?.FindControl("cblsHeaderObra") is TreeViewSearch cblsHeaderObra && 
                    cblsHeaderObra.SelectedValues?.Any() == true)
                {
                    var obrasSeleccionadas = cblsHeaderObra.SelectedValues.Select(int.Parse).ToList();
                    autorizantes = autorizantes.Where(a => a.ObraId.HasValue && obrasSeleccionadas.Contains(a.ObraId.Value)).ToList();
                }

                // Aplicar filtro de Empresa
                if (gridviewRegistros.HeaderRow?.FindControl("cblsHeaderEmpresa") is TreeViewSearch cblsHeaderEmpresa && 
                    cblsHeaderEmpresa.SelectedValues?.Any() == true)
                {
                    var empresasSeleccionadas = cblsHeaderEmpresa.SelectedValues.Select(int.Parse).ToList();
                    autorizantes = autorizantes.Where(a => a.EmpresaId.HasValue && empresasSeleccionadas.Contains(a.EmpresaId.Value)).ToList();
                }

                // Aplicar filtro de Código Autorizante
                if (gridviewRegistros.HeaderRow?.FindControl("cblsHeaderCodigoAutorizante") is TreeViewSearch cblsHeaderCodigoAutorizante && cblsHeaderCodigoAutorizante.SelectedValues?.Any() == true)
                {
                    var codigosSeleccionados = cblsHeaderCodigoAutorizante.SelectedValues;
                    autorizantes = autorizantes
                        .Where(a => codigosSeleccionados.Contains(a.CodigoAutorizante))
                        .ToList();
                }

                // Aplicar filtro de Concepto
                if (gridviewRegistros.HeaderRow?.FindControl("cblsHeaderConcepto") is TreeViewSearch cblsHeaderConcepto && 
                    cblsHeaderConcepto.SelectedValues?.Any() == true)
                {
                    var conceptosSeleccionados = cblsHeaderConcepto.SelectedValues.ToList();
                    autorizantes = autorizantes.Where(a => 
                    {
                        // Si se seleccionó "REDETERMINACION", filtrar por ConceptoNombre
                        if (conceptosSeleccionados.Contains("REDETERMINACION") && a.ConceptoNombre == "REDETERMINACION")
                        {
                            return true;
                        }
                        
                        // Si se seleccionaron conceptos normales, filtrar por ConceptoId
                        var conceptosNumericos = conceptosSeleccionados.Where(c => int.TryParse(c, out _)).Select(int.Parse).ToList();
                        if (conceptosNumericos.Any() && a.ConceptoId.HasValue && conceptosNumericos.Contains(a.ConceptoId.Value))
                        {
                            return true;
                        }
                        
                        return false;
                    }).ToList();
                }

                // Aplicar filtro de Estado
                if (gridviewRegistros.HeaderRow?.FindControl("cblsHeaderEstado") is TreeViewSearch cblsHeaderEstado && 
                    cblsHeaderEstado.SelectedValues?.Any() == true)
                {
                    var estadosSeleccionados = cblsHeaderEstado.SelectedValues.Select(int.Parse).ToList();
                    autorizantes = autorizantes.Where(a => a.EstadoId.HasValue && estadosSeleccionados.Contains(a.EstadoId.Value)).ToList();
                }

                // Aplicar filtro de Barrio (si existe)
                if (gridviewRegistros.HeaderRow?.FindControl("cblsHeaderBarrio") is TreeViewSearch cblsHeaderBarrio && 
                    cblsHeaderBarrio.SelectedValues?.Any() == true)
                {
                    var barriosSeleccionados = cblsHeaderBarrio.SelectedValues.Select(int.Parse).ToList();
                    autorizantes = autorizantes.Where(a => a.BarrioId.HasValue && barriosSeleccionados.Contains(a.BarrioId.Value)).ToList();
                }

                // Aplicar filtro de Contrata (si existe)
                if (gridviewRegistros.HeaderRow?.FindControl("cblsHeaderContrata") is TreeViewSearch cblsHeaderContrata && 
                    cblsHeaderContrata.SelectedValues?.Any() == true)
                {
                    var contratasSeleccionadas = cblsHeaderContrata.SelectedValues.Select(int.Parse).ToList();
                    autorizantes = autorizantes.Where(a => a.ContrataId.HasValue && contratasSeleccionadas.Contains(a.ContrataId.Value)).ToList();
                }

                // Aplicar filtro de Línea de Gestión (si existe)
                if (gridviewRegistros.HeaderRow?.FindControl("cblsHeaderLineaGestion") is TreeViewSearch cblsHeaderLineaGestion && 
                    cblsHeaderLineaGestion.SelectedValues?.Any() == true)
                {
                    var lineasGestionSeleccionadas = cblsHeaderLineaGestion.SelectedValues.Select(int.Parse).ToList();
                    autorizantes = autorizantes.Where(a => a.LineaGestionId.HasValue && lineasGestionSeleccionadas.Contains(a.LineaGestionId.Value)).ToList();
                }

                // Aplicar filtro de Proyecto (si existe)
                if (gridviewRegistros.HeaderRow?.FindControl("cblsHeaderProyecto") is TreeViewSearch cblsHeaderProyecto && 
                    cblsHeaderProyecto.SelectedValues?.Any() == true)
                {
                    var proyectosSeleccionados = cblsHeaderProyecto.SelectedValues.Select(int.Parse).ToList();
                    autorizantes = autorizantes.Where(a => a.ProyectoId.HasValue && proyectosSeleccionados.Contains(a.ProyectoId.Value)).ToList();
                }

                return autorizantes;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en AplicarFiltrosTreeViewEnMemoria: {ex.Message}");
                return autorizantes; // Devolver lista original en caso de error
            }
        }

        private UsuarioEF ObtenerUsuarioActual()
        {
            // Implementar según tu lógica de sesión
            // Por ejemplo:
            if (Session["Usuario"] != null)
            {
                var usuarioTradicional = (Usuario)Session["Usuario"];
                return new UsuarioEF 
                { 
                    Id = usuarioTradicional.Id,
                    Nombre = usuarioTradicional.Nombre,
                    Correo = usuarioTradicional.Correo,
                    Tipo = usuarioTradicional.Tipo,
                    Estado = usuarioTradicional.Estado,
                    AreaId = usuarioTradicional.Area?.Id ?? 0,
                };
            }
            
            // Si no hay usuario en sesión, devolver un usuario por defecto que permita ver todos los datos
            // (sin filtro de área)
            return new UsuarioEF 
            {
                Id = 0,
                Nombre = "Usuario null",
                Correo = null,
                Tipo = false, // Usuario normal por defecto
                Estado = false,
                AreaId = 0, // 0 significa sin filtro de área
            };
        }

        //private void CalcularSubtotal()
        //{
        //    try
        //    {
        //        // Siempre usar la lista completa (no la paginada)
        //        List<AutorizanteDTO> todosLosRegistros;
        //        if (Session["GridDataAutorizantesTotal"] != null)
        //        {
        //            todosLosRegistros = (List<AutorizanteDTO>)Session["GridDataAutorizantesTotal"];
        //        }
        //        else
        //        {
        //            UsuarioEF usuario = ObtenerUsuarioActual();
        //            todosLosRegistros = calculoRedeterminacionNegocio.ListarAutorizantesCompleto(usuario);
        //        }

        //        // Aplicar filtro de búsqueda (txtBuscar)
        //        string filtro = txtBuscar.Text.Trim().ToLower();
        //        if (!string.IsNullOrEmpty(filtro))
        //        {
        //            todosLosRegistros = todosLosRegistros.Where(a =>
        //                (a.CodigoAutorizante?.ToLower().Contains(filtro) ?? false) ||
        //                (a.Detalle?.ToLower().Contains(filtro) ?? false) ||
        //                (a.Expediente?.ToLower().Contains(filtro) ?? false) ||
        //                (a.EmpresaNombre?.ToLower().Contains(filtro) ?? false) ||
        //                (a.ObraDescripcion?.ToLower().Contains(filtro) ?? false) ||
        //                (a.AreaNombre?.ToLower().Contains(filtro) ?? false) ||
        //                (a.BarrioNombre?.ToLower().Contains(filtro) ?? false) ||
        //                (a.ConceptoNombre?.ToLower().Contains(filtro) ?? false) ||
        //                (a.EstadoNombre?.ToLower().Contains(filtro) ?? false)
        //            ).ToList();
        //        }

        //        // Aplicar filtros de TreeView (filtros de columnas)
        //        todosLosRegistros = AplicarFiltrosTreeViewEnMemoria(todosLosRegistros);

        //        // Calcular el total de los registros filtrados
        //        decimal totalMonto = todosLosRegistros.Sum(a => a.MontoAutorizado);
        //        int cantidadRegistros = todosLosRegistros.Count;

        //        // Actualizar etiquetas de subtotal
        //        if (lblSubtotalPaginacion != null)
        //        {
        //            lblSubtotalPaginacion.Text = $"Total: {totalMonto:C} ({cantidadRegistros} registros)";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Diagnostics.Debug.WriteLine($"Error en CalcularSubtotal: {ex.Message}");
        //        if (lblSubtotalPaginacion != null)
        //        {
        //            lblSubtotalPaginacion.Text = "Total: Error al calcular";
        //        }
        //    }
        //}

        protected void gridviewRegistros_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                string codigoAutorizante = gridviewRegistros.SelectedDataKey.Value.ToString();
                
                // Obtener datos de la sesión filtrada en lugar de consultar BD directamente
                List<AutorizanteDTO> autorizantesList = Session["GridDataAutorizantes"] as List<AutorizanteDTO>;
                if (autorizantesList == null)
                {
                    lblMensaje.Text = "Error: No hay datos en memoria.";
                    lblMensaje.CssClass = "alert alert-danger";
                    return;
                }

                // Buscar el autorizante seleccionado en la lista filtrada
                AutorizanteDTO autorizanteSeleccionado = autorizantesList.FirstOrDefault(a => a.CodigoAutorizante == codigoAutorizante);

                if (autorizanteSeleccionado != null)
                {
                    // Cambiar texto del botón a "Actualizar"
                    Button1.Text = "Actualizar";

                    txtCodigoAutorizante.Enabled = false; // Código no editable en edición

                    // Cargar datos en el formulario
                    txtCodigoAutorizante.Text = autorizanteSeleccionado.CodigoAutorizante;
                    txtExpediente.Text = autorizanteSeleccionado.Expediente;
                    txtDetalle.Text = autorizanteSeleccionado.Detalle;
                    txtMontoAutorizado.Text = autorizanteSeleccionado.MontoAutorizado.ToString("0.00");
                    
                    if (autorizanteSeleccionado.MesAprobacion.HasValue)
                        txtFecha.Text = autorizanteSeleccionado.MesAprobacion.Value.ToString("yyyy-MM-dd");
                    
                    if (autorizanteSeleccionado.MesBase.HasValue)
                        txtMes.Text = autorizanteSeleccionado.MesBase.Value.ToString("yyyy-MM-dd");

                    // Seleccionar valores en dropdowns usando método helper
                    if (autorizanteSeleccionado.ConceptoId.HasValue)
                        SelectDropDownListByValue(ddlConcepto, autorizanteSeleccionado.ConceptoId.Value.ToString());
                    
                    if (autorizanteSeleccionado.EstadoId.HasValue)
                        SelectDropDownListByValue(ddlEstado, autorizanteSeleccionado.EstadoId.Value.ToString());

                    // Guardar estado de edición
                    Session["EditingAutorizanteId"] = autorizanteSeleccionado.Id;
                    Session["EditingCodigoAutorizante"] = autorizanteSeleccionado.CodigoAutorizante;

                    // Actualizar modal con JavaScript completo similar a AutorizantesAdmin
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "UpdateModalAndShow", @"
                        $(document).ready(function() {
                            // Cambiar título y texto del botón
                            $('#modalAgregar .modal-title').text('Modificar Autorizante');
                            document.getElementById('" + Button1.ClientID + @"').value = 'Actualizar';
                            
                            // Ocultar el dropdown de Obra y su etiqueta
                            $('#obraContainer').hide();
                            
                            // Mostrar el modal
                            $('#modalAgregar').modal('show');
                        });", true);
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cargar los datos del autorizante: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        /// <summary>
        /// Método helper que selecciona un elemento en un DropDownList por su valor.
        /// Limpia la selección actual y busca el elemento por el valor dado.
        /// Si encuentra el elemento, lo selecciona.
        /// </summary>
        private void SelectDropDownListByValue(DropDownList dropDown, string value)
        {
            // Limpiar selección actual
            dropDown.ClearSelection();

            // Intentar encontrar y seleccionar el elemento
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
                string codigoAutorizante = gridviewRegistros.DataKeys[e.RowIndex].Value.ToString();
                
                if (autorizanteNegocio.Eliminar(codigoAutorizante))
                {
                    lblMensaje.Text = "Autorizante eliminado exitosamente!";
                    lblMensaje.CssClass = "alert alert-success";
                    
                    // Limpiar cache SADE ya que se eliminó un autorizante
                    CalculoRedeterminacionNegocioEF.LimpiarCacheSade();
                    
                    CargarListaAutorizantesRedet(); // Ya calcula subtotal internamente
                }
                else
                {
                    lblMensaje.Text = "Hubo un problema al eliminar el autorizante.";
                    lblMensaje.CssClass = "alert alert-danger";
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        protected void gridviewRegistros_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            // Este método ahora está obsoleto ya que usamos paginación externa
            // Mantenemos por compatibilidad pero no hacemos nada
            e.Cancel = true;
        }

        protected void txtExpediente_TextChanged(object sender, EventArgs e)
        {
            TextBox txtBox = (TextBox)sender;
            GridViewRow row = (GridViewRow)txtBox.NamingContainer;

            try
            {
                int rowIndex = row.RowIndex;
                string nuevoExpediente = txtBox.Text.Trim();

                // Obtener datos filtrados actuales similar a CertificadosAdminEF
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

                AutorizanteDTO autorizante = datosFiltradosActuales[indiceReal];
                string codigoAutorizante = autorizante.CodigoAutorizante;

                // Actualizar en la base de datos
                bool resultado = autorizanteNegocio.ActualizarExpediente(codigoAutorizante, nuevoExpediente);

                if (resultado)
                {
                    // Actualizar también en la lista completa de Session
                    List<AutorizanteDTO> listaCompleta = Session["GridDataAutorizantes"] as List<AutorizanteDTO>;
                    if (listaCompleta != null)
                    {
                        var autorizanteEnLista = listaCompleta.FirstOrDefault(a => a.CodigoAutorizante == codigoAutorizante);
                        if (autorizanteEnLista != null)
                        {
                            autorizanteEnLista.Expediente = nuevoExpediente;
                        }
                    }

                    // Limpiar cache SADE ya que se modificó un expediente (autorizante o redeterminación)
                    CalculoRedeterminacionNegocioEF.LimpiarCacheSade();

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
                lblMensaje.Text = $"Error: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        private List<AutorizanteDTO> ObtenerDatosFiltradosActuales()
        {
            if (Session["GridDataAutorizantes"] == null) return null;
            
            var autorizantes = (List<AutorizanteDTO>)Session["GridDataAutorizantes"];

            // Aplicar filtro de búsqueda
            string filtro = txtBuscar.Text.Trim().ToLower();
            if (!string.IsNullOrEmpty(filtro))
            {
                autorizantes = autorizantes.Where(a =>
                    (a.CodigoAutorizante?.ToLower().Contains(filtro) ?? false) ||
                    (a.Detalle?.ToLower().Contains(filtro) ?? false) ||
                    (a.Expediente?.ToLower().Contains(filtro) ?? false) ||
                    (a.EmpresaNombre?.ToLower().Contains(filtro) ?? false) ||
                    (a.ObraDescripcion?.ToLower().Contains(filtro) ?? false) ||
                    (a.AreaNombre?.ToLower().Contains(filtro) ?? false) ||
                    (a.ConceptoNombre?.ToLower().Contains(filtro) ?? false) ||
                    (a.EstadoNombre?.ToLower().Contains(filtro) ?? false)
                ).ToList();
            }

            // Aplicar filtros de TreeView
            autorizantes = AplicarFiltrosTreeViewEnMemoria(autorizantes);

            return autorizantes;
        }



        protected void gridviewRegistros_DataBound(object sender, EventArgs e)
        {
            try
            {
                // Actualizar controles de paginación externa después del DataBind
                var paginationControl = FindControlRecursive(this, "paginationControl") as CustomControls.PaginationControl;
                if (paginationControl != null)
                {
                    paginationControl.UpdatePaginationControls();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en DataBound: {ex.Message}");
            }
        }


        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            // Asegurar que el GridView está configurado correctamente
            gridviewRegistros.DataBound += gridviewRegistros_DataBound;
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            // Actualizar controles de paginación externa antes del render
            var paginationControl = FindControlRecursive(this, "paginationControl") as CustomControls.PaginationControl;
            if (paginationControl != null)
            {
                paginationControl.UpdatePaginationControls();
            }
        }

        #endregion

        #region Eventos del Control de Paginación

        protected void paginationControl_PageChanged(object sender, PaginationEventArgs e)
        {
            // El control maneja automáticamente el cambio de página
            var paginationControl = FindControlRecursive(this, "paginationControl") as CustomControls.PaginationControl;
            if (paginationControl != null)
            {
                paginationControl.UpdatePaginationControls();
            }
            CargarListaAutorizantesRedet();
        }

        protected void paginationControl_PageSizeChanged(object sender, PaginationEventArgs e)
        {
            // Recalcular total cuando cambia el tamaño de página
            var usuario = ObtenerUsuarioActual();
            if (usuario != null)
            {
                var paginationControl = FindControlRecursive(this, "paginationControl") as CustomControls.PaginationControl;
                if (paginationControl != null)
                {
                    var todosLosRegistros = calculoRedeterminacionNegocio.ListarAutorizantesCompleto(usuario);
                    paginationControl.TotalRecords = todosLosRegistros.Count;
                    paginationControl.UpdatePaginationControls();
                }
                CargarListaAutorizantesRedet();
            }
        }

        #endregion

        #region Métodos auxiliares de paginación

        /// <summary>
        /// Configura los valores del control de paginación basándose en los datos actuales
        /// </summary>
        private void ConfigurarPaginationControl()
        {
            var usuario = ObtenerUsuarioActual();
            if (usuario != null)
            {
                // Obtener total de registros contando la lista completa
                var todosLosRegistros = calculoRedeterminacionNegocio.ListarAutorizantesCompleto(usuario);
                var totalRegistros = todosLosRegistros.Count;
                
                // Configurar el control de paginación
                var paginationControl = FindControlRecursive(this, "paginationControl") as CustomControls.PaginationControl;
                if (paginationControl != null)
                {
                    paginationControl.TotalRecords = totalRegistros;
                    paginationControl.CurrentPageIndex = 0; // Reiniciar a primera página
                    paginationControl.PageSize = 12; // Valor por defecto
                    paginationControl.UpdatePaginationControls();
                }
                
                // Calcular y mostrar subtotal
                CalcularSubtotalParaPaginationControl();
            }
        }

        /// <summary>
        /// Calcula el subtotal para el control de paginación
        /// </summary>
        private void CalcularSubtotalParaPaginationControl()
        {
            try
            {
                // Usar la misma lógica que CalcularSubtotal() pero actualizar el control de paginación
                List<AutorizanteDTO> todosLosRegistros;
                if (Session["GridDataAutorizantesTotal"] != null)
                {
                    todosLosRegistros = (List<AutorizanteDTO>)Session["GridDataAutorizantesTotal"];
                }
                else
                {
                    UsuarioEF usuario = ObtenerUsuarioActual();
                    todosLosRegistros = calculoRedeterminacionNegocio.ListarAutorizantesCompleto(usuario);
                }

                // Aplicar filtro de búsqueda (txtBuscar)
                string filtro = txtBuscar.Text.Trim().ToLower();
                if (!string.IsNullOrEmpty(filtro))
                {
                    todosLosRegistros = todosLosRegistros.Where(a =>
                        (a.CodigoAutorizante?.ToLower().Contains(filtro) ?? false) ||
                        (a.Detalle?.ToLower().Contains(filtro) ?? false) ||
                        (a.Expediente?.ToLower().Contains(filtro) ?? false) ||
                        (a.EmpresaNombre?.ToLower().Contains(filtro) ?? false) ||
                        (a.ObraDescripcion?.ToLower().Contains(filtro) ?? false) ||
                        (a.AreaNombre?.ToLower().Contains(filtro) ?? false) ||
                        (a.BarrioNombre?.ToLower().Contains(filtro) ?? false) ||
                        (a.ConceptoNombre?.ToLower().Contains(filtro) ?? false) ||
                        (a.EstadoNombre?.ToLower().Contains(filtro) ?? false)
                    ).ToList();
                }

                // Aplicar filtros de TreeView (filtros de columnas)
                todosLosRegistros = AplicarFiltrosTreeViewEnMemoria(todosLosRegistros);

                // Calcular el total de los registros filtrados
                decimal totalMonto = todosLosRegistros.Sum(a => a.MontoAutorizado);
                int cantidadRegistros = todosLosRegistros.Count;

                // Actualizar el subtotal en el control de paginación
                var paginationControl = FindControlRecursive(this, "paginationControl") as CustomControls.PaginationControl;
                if (paginationControl != null)
                {
                    paginationControl.UpdateSubtotal(totalMonto, cantidadRegistros);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en CalcularSubtotalParaPaginationControl: {ex.Message}");
                var paginationControl = FindControlRecursive(this, "paginationControl") as CustomControls.PaginationControl;
                if (paginationControl != null)
                {
                    paginationControl.UpdateSubtotal(0, 0);
                }
            }
        }

        #endregion

        #region Métodos auxiliares para búsqueda recursiva de controles

        /// <summary>
        /// Busca un control recursivamente en la jerarquía de controles.
        /// Método auxiliar para trabajar sin designer regenerado.
        /// </summary>
        private Control FindControlRecursive(Control root, string id)
        {
            if (root.ID == id)
                return root;

            foreach (Control control in root.Controls)
            {
                Control found = FindControlRecursive(control, id);
                if (found != null)
                    return found;
            }

            return null;
        }

        protected void ddlEstadoAutorizante_SelectedIndexChanged(object sender, EventArgs e)
        {
            DropDownList ddlEstadoAutorizante = (DropDownList)sender;
            GridViewRow row = (GridViewRow)ddlEstadoAutorizante.NamingContainer;

            try
            {
                // Obtener el índice de la fila actual
                int rowIndex = row.RowIndex;
                
                // Obtener los datos de la sesión
                List<AutorizanteDTO> listaAutorizantes = (List<AutorizanteDTO>)Session["GridDataAutorizantes"];
                if (listaAutorizantes == null || rowIndex >= listaAutorizantes.Count)
                {
                    lblMensaje.Text = "Error: No se pudieron obtener los datos del autorizante.";
                    lblMensaje.CssClass = "alert alert-danger";
                    return;
                }
                
                AutorizanteDTO autorizanteDTO = listaAutorizantes[rowIndex];
                int nuevoEstadoId = int.Parse(ddlEstadoAutorizante.SelectedValue);
                
                // Actualizar el estado en la base de datos
                AutorizanteNegocioEF negocio = new AutorizanteNegocioEF();
                if (negocio.ActualizarEstado(autorizanteDTO.CodigoAutorizante, nuevoEstadoId))
                {
                    // Actualizar el DTO en memoria
                    autorizanteDTO.EstadoId = nuevoEstadoId;
                    
                    // Obtener el nombre del estado para actualizar también en memoria
                    EstadoAutorizanteNegocioEF estadoNegocio = new EstadoAutorizanteNegocioEF();
                    var estados = estadoNegocio.Listar();
                    var estadoSeleccionado = estados.FirstOrDefault(estado => estado.Id == nuevoEstadoId);
                    if (estadoSeleccionado != null)
                    {
                        autorizanteDTO.EstadoNombre = estadoSeleccionado.Nombre;
                    }
                    
                    // Los cambios se reflejan automáticamente por referencia
                    // Session["GridDataAutorizantes"] = listaAutorizantes; // Línea innecesaria
                    
                    lblMensaje.Text = "Estado actualizado correctamente.";
                    lblMensaje.CssClass = "alert alert-success";
                    
                    // Recargar solo si es necesario (para mantener la paginación actual)
                    BindGrid();
                }
                else
                {
                    lblMensaje.Text = "Error al actualizar el estado.";
                    lblMensaje.CssClass = "alert alert-danger";
                    
                    // Restaurar el valor anterior en el desplegable
                    ddlEstadoAutorizante.SelectedValue = autorizanteDTO.EstadoId.ToString();
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al actualizar el estado: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        protected void gridviewRegistros_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                DropDownList ddlEstadoAutorizante = (DropDownList)e.Row.FindControl("ddlEstadoAutorizante");
                if (ddlEstadoAutorizante != null)
                {
                    // Cargar estados
                    EstadoAutorizanteNegocioEF estadoNegocio = new EstadoAutorizanteNegocioEF();
                    var estados = estadoNegocio.Listar();
                    
                    ddlEstadoAutorizante.DataSource = estados;
                    ddlEstadoAutorizante.DataTextField = "Nombre";
                    ddlEstadoAutorizante.DataValueField = "Id";
                    ddlEstadoAutorizante.DataBind();

                    // Establecer el valor seleccionado
                    AutorizanteDTO autorizante = (AutorizanteDTO)e.Row.DataItem;
                    if (autorizante != null && autorizante.EstadoId.HasValue)
                    {
                        ListItem item = ddlEstadoAutorizante.Items.FindByValue(autorizante.EstadoId.ToString());
                        if (item != null)
                        {
                            ddlEstadoAutorizante.SelectedValue = autorizante.EstadoId.ToString();
                        }
                    }
                }
            }
        }

        #endregion
    }
}
