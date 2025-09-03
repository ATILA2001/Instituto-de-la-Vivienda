using Dominio;
using Dominio.DTO;
using Negocio;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Drawing;
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
        private AutorizanteNegocioEF _autorizanteNegocioEF = new AutorizanteNegocioEF();

        /// <summary>
        /// Clase de negocio para cálculos complejos de redeterminaciones.
        /// Responsable de:
        /// - Generar redeterminaciones virtuales a partir de autorizantes
        /// - Calcular montos y porcentajes de redeterminaciones
        /// - Integrar datos SADE y SIGAF
        /// - Manejar la paginación optimizada de grandes volúmenes de datos
        /// </summary>
        private CalculoRedeterminacionNegocioEF _calculoRedeterminacionNegocioEF = new CalculoRedeterminacionNegocioEF();

        #endregion

        #region Variables de Paginación Externa

        /// <summary>
        /// Índice de página actual (base 0). Se mantiene en ViewState para persistir entre postbacks.
        /// Este sistema de paginación es independiente del GridView nativo y permite mejor control.
        /// </summary>
        private int _currentPageIndex = 0;

        /// <summary>
        /// Cantidad de registros por página. Por defecto 12, configurable por el usuario.
        /// Se mantiene en ViewState para persistir la preferencia del usuario.
        /// </summary>
        private int _pageSize = 12;

        /// <summary>
        /// Total de registros disponibles (autorizantes + redeterminaciones).
        /// Se calcula una vez y se almacena en ViewState para evitar recálculos.
        /// </summary>
        private int _totalRecords = 0;


        #endregion

        #region Eventos del Ciclo de Vida de la Página

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
            _currentPageIndex = (int)(ViewState["CurrentPageIndex"] ?? 0);
            _pageSize = (int)(ViewState["PageSize"] ?? 12);

            if (!IsPostBack)
            {
                CargarListaAutorizantesRedet();
                ObtenerDropDownLists();
                ConfigurarPaginationControl();
            }
            else
            {
                // En postback, actualizar controles de paginación
                if (FindControlRecursive(this, "paginationControl") is PaginationControl paginationControl)
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
            _currentPageIndex = 0;
            ViewState["CurrentPageIndex"] = _currentPageIndex;
            CargarPaginaActual();
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
                    { "Obra", "ObraDescripcion" },
                    { "Contrata", "Contrata" },
                    { "Empresa", "EmpresaNombre" },
                    { "Código Autorizante", "CodigoAutorizante" },
                    { "Expediente", "Expediente" },
                    { "Detalle", "Detalle" },
                    { "Monto Autorizado", "MontoAutorizado" },
                    { "Mes Aprobación", "MesAprobacioin" },
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
            LimpiarFormularioAgregar();

            ScriptManager.RegisterStartupScript(this, GetType(), "ShowAddModal", @"
                $(document).ready(function() {
                    // Mostrar el modal
                    $('#modalAgregarAutorizante').modal('show');
                });", true);

            // Limpiar cualquier estado de edición
            Session["EditingAutorizanteId"] = null;
            Session["EditingCodigoAutorizante"] = null;

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
            if (FindControlRecursive(this, "paginationControl") is PaginationControl paginationControl)
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
            if (FindControlRecursive(this, "paginationControl") is PaginationControl paginationControl)
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
            if (sender is DropDownList ddl)
            {
                if (FindControlRecursive(this, "paginationControl") is CustomControls.PaginationControl paginationControl)
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

            // Crear nuevo autorizante con todos los campos
            AutorizanteEF autorizante = new AutorizanteEF();
            // autorizante.CodigoAutorizante = txtCodigoAutorizante.Text.Trim();
            autorizante.Expediente = txtExpedienteAgregar.Text.Trim();
            autorizante.Detalle = txtDetalleAgregar.Text.Trim();
            autorizante.MontoAutorizado = Convert.ToDecimal(txtMontoAutorizadoAgregar.Text);

            // Parsear fecha si se proporciona
            if (!string.IsNullOrEmpty(txtMesAprobacionAgregar.Text))
            {
                autorizante.MesAprobacion = DateTime.Parse(txtMesAprobacionAgregar.Text);
            }

            // Parsear mes base si se proporciona
            if (!string.IsNullOrEmpty(txtMesBaseAgregar.Text))
            {
                autorizante.MesBase = DateTime.Parse(txtMesBaseAgregar.Text);
            }

            autorizante.ConceptoId = int.Parse(ddlConceptoAgregar.SelectedValue);
            autorizante.EstadoId = int.Parse(ddlEstadoAgregar.SelectedValue);

            // Solo al agregar se asigna la obra seleccionada
            autorizante.ObraId = int.Parse(ddlObraAgregar.SelectedValue);

            if (_autorizanteNegocioEF.Agregar(autorizante))
            {
                lblMensaje.Text = "Autorizante agregado exitosamente!";
                lblMensaje.CssClass = "alert alert-success";
            }
            else
            {
                lblMensaje.Text = "Hubo un problema al agregar el autorizante.";
                lblMensaje.CssClass = "alert alert-danger";
            }


            // Limpiar campos
            LimpiarFormularioAgregar();

            // Ocultar el modal
            ScriptManager.RegisterStartupScript(this, this.GetType(), "HideModal",
                "$('#modalAgregarAutorizante').modal('hide');", true);

            // Limpiar cache SADE ya que se agregó/modificó un autorizante
            CalculoRedeterminacionNegocioEF.LimpiarCacheSade();

            // Limpiar cache de datos para forzar recarga desde BD
            Session["GridDataAutorizantes"] = null;

            // Invalida cache y fuerza recarga
            ViewState["NecesitaRecarga"] = true;
            CargarPaginaActual();

        }

        protected void btnEditar_Click(object sender, EventArgs e)
        {
            // Verificar si la página es válida (todos los validadores pasaron)
            if (!Page.IsValid) return;



            // MODO EDITAR: Obtener el autorizante existente de la BD
            int autorizanteId = (int)Session["EditingAutorizanteId"];
            var autorizanteExistente = _autorizanteNegocioEF.ObtenerPorId(autorizanteId);

            if (autorizanteExistente == null)
            {
                lblMensaje.Text = "Error: No se encontró el autorizante a modificar.";
                lblMensaje.CssClass = "alert alert-danger";
                return;
            }

            autorizanteExistente.Expediente = txtExpedienteEditar.Text.Trim();
            autorizanteExistente.Detalle = txtDetalleEditar.Text.Trim();
            autorizanteExistente.MontoAutorizado = Convert.ToDecimal(txtMontoAutorizadoEditar.Text);

            // Parsear fecha si se proporciona
            if (!string.IsNullOrEmpty(txtMesAprobacionEditar.Text))
            {
                autorizanteExistente.MesAprobacion = DateTime.Parse(txtMesAprobacionEditar.Text);
            }
            else
            {
                autorizanteExistente.MesAprobacion = null;
            }

            // Parsear mes base si se proporciona
            if (!string.IsNullOrEmpty(txtMesBaseEditar.Text))
            {
                autorizanteExistente.MesBase = DateTime.Parse(txtMesBaseEditar.Text);
            }
            else
            {
                autorizanteExistente.MesBase = null;
            }

            autorizanteExistente.ConceptoId = int.Parse(ddlConceptoEditar.SelectedValue);
            autorizanteExistente.EstadoId = int.Parse(ddlEstadoEditar.SelectedValue);

            if (_autorizanteNegocioEF.Modificar(autorizanteExistente))
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


            // Limpiar campos
            LimpiarFormularioEditar();

            // Ocultar el modal
            ScriptManager.RegisterStartupScript(this, this.GetType(), "HideModal",
                "$('#modalEditarAutorizante').modal('hide');", true);

            // (Cache SADE ya se limpió arriba en caso de modificación exitosa)

            // Limpiar cache de datos para forzar recarga desde BD
            Session["GridDataAutorizantes"] = null;

            // Invalida cache y fuerza recarga
            ViewState["NecesitaRecarga"] = true;
            CargarPaginaActual();

        }


        private void LimpiarFormularioAgregar()
        {
            txtExpedienteAgregar.Text = string.Empty;
            txtDetalleAgregar.Text = string.Empty;
            txtMontoAutorizadoAgregar.Text = string.Empty;
            txtMesAprobacionAgregar.Text = string.Empty;
            txtMesBaseAgregar.Text = string.Empty;
            ddlObraAgregar.SelectedIndex = 0;
            ddlConceptoAgregar.SelectedIndex = 0;
            ddlEstadoAgregar.SelectedIndex = 0;
        }
        private void LimpiarFormularioEditar()
        {
            txtExpedienteEditar.Text = string.Empty;
            txtDetalleEditar.Text = string.Empty;
            txtMontoAutorizadoEditar.Text = string.Empty;
            txtMesAprobacionEditar.Text = string.Empty;
            txtMesBaseEditar.Text = string.Empty;
            ddlConceptoEditar.SelectedIndex = 0;
            ddlEstadoEditar.SelectedIndex = 0;
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
                bindFilter("cblsHeaderCodigoAutorizante", context.Autorizantes.AsNoTracking().OrderBy(a => a.CodigoAutorizante).Select(a => new { a.CodigoAutorizante }).Distinct().ToList(), "CodigoAutorizante", "CodigoAutorizante"
); bindFilter("cblsHeaderContrata", context.Contratas.AsNoTracking().OrderBy(c => c.Nombre).Select(c => new { c.Id, c.Nombre }).ToList(), "Nombre", "Id");

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
            ObraNegocioEF obraNegocio = new ObraNegocioEF();
            var usuario = UserHelper.GetFullCurrentUser();

            try
            {
                List<ObraEF> obras = obraNegocio.ListarParaDDL(usuario);

                ddlObraAgregar.DataSource = obras;
                ddlObraAgregar.DataTextField = "Descripcion";
                ddlObraAgregar.DataValueField = "Id";
                ddlObraAgregar.DataBind();
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
            ConceptoNegocioEF conceptoNegocio = new ConceptoNegocioEF();

            try
            {
                List<ConceptoEF> conceptos = conceptoNegocio.Listar();

                ddlConceptoAgregar.DataSource = conceptos;
                ddlConceptoAgregar.DataTextField = "Nombre";
                ddlConceptoAgregar.DataValueField = "Id";
                ddlConceptoAgregar.DataBind();

                ddlConceptoEditar.DataSource = conceptos;
                ddlConceptoEditar.DataTextField = "Nombre";
                ddlConceptoEditar.DataValueField = "Id";
                ddlConceptoEditar.DataBind();
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
            EstadoAutorizanteNegocioEF estadoNegocio = new EstadoAutorizanteNegocioEF();

            try
            {
                List<EstadoAutorizanteEF> estados = estadoNegocio.Listar();

                ddlEstadoAgregar.DataSource = estados;
                ddlEstadoAgregar.DataTextField = "Nombre";
                ddlEstadoAgregar.DataValueField = "Id";
                ddlEstadoAgregar.DataBind();

                ddlEstadoEditar.DataSource = estados;
                ddlEstadoEditar.DataTextField = "Nombre";
                ddlEstadoEditar.DataValueField = "Id";
                ddlEstadoEditar.DataBind();
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
            //ólo asegura cache completo y delega paginación/filtrado a BindGrid()
            try
            {
                List<AutorizanteDTO> todos;
                if (Session["GridDataAutorizantes"] == null || ViewState["NecesitaRecarga"] != null)
                {
                    UsuarioEF usuario = ObtenerUsuarioActual();
                    todos = _calculoRedeterminacionNegocioEF.ListarAutorizantesYRedeterminaciones(usuario);
                    Session["GridDataAutorizantes"] = todos;
                    ViewState["NecesitaRecarga"] = null;
                }
                else
                {
                    todos = (List<AutorizanteDTO>)Session["GridDataAutorizantes"];
                }

                _totalRecords = todos?.Count ?? 0;
                BindGrid();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cargar autorizantes: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        private void CargarPaginaActual()
        {
            ViewState["CurrentPageIndex"] = _currentPageIndex;
            ViewState["PageSize"] = _pageSize;
            CargarListaAutorizantesRedet();
            ConfigurarPaginationControl();
        }

        // Refresca grid desde cache aplicando filtros y paginación basada en currentPageIndex/pageSize
        private void BindGrid()
        {
            if (Session["GridDataAutorizantes"] == null) return;

            var datosEnMemoria = (List<AutorizanteDTO>)Session["GridDataAutorizantes"];

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

            datosEnMemoria = AplicarFiltrosTreeViewEnMemoria(datosEnMemoria);

            int totalFiltrados = datosEnMemoria.Count;
            gridviewRegistros.VirtualItemCount = totalFiltrados;
            gridviewRegistros.PageSize = _pageSize;
            gridviewRegistros.PageIndex = _currentPageIndex;
            gridviewRegistros.DataSource = datosEnMemoria
                .Skip(_currentPageIndex * _pageSize)
                .Take(_pageSize)
                .ToList();
            gridviewRegistros.DataBind();
            PoblarFiltrosHeader();

            _totalRecords = totalFiltrados;
            ViewState["TotalRecords"] = totalFiltrados;
            ConfigurarPaginationControl();
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
                        .Where(a => codigosSeleccionados.Any(cod =>
                            a.CodigoAutorizante == cod ||
                            (a.CodigoAutorizante != null && a.CodigoAutorizante.StartsWith(cod + "-R"))
                        ))
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


        protected void gridviewRegistros_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                int id = 0;
                string codigoAutorizante = null;
                if (gridviewRegistros.SelectedDataKey != null)
                {
                    var values = gridviewRegistros.SelectedDataKey.Values;
                    if (values != null)
                    {
                        if (values["Id"] != null) int.TryParse(values["Id"].ToString(), out id);
                        if (values["CodigoAutorizante"] != null) codigoAutorizante = values["CodigoAutorizante"].ToString();
                    }
                }

                if (!(Session["GridDataAutorizantes"] is List<AutorizanteDTO> autorizantesList))
                {
                    lblMensaje.Text = "Error: No hay datos en memoria.";
                    lblMensaje.CssClass = "alert alert-danger";
                    return;
                }

                AutorizanteDTO autorizanteSeleccionado = null;
                if (id > 0) autorizanteSeleccionado = autorizantesList.FirstOrDefault(a => a.Id == id);
                if (autorizanteSeleccionado == null && !string.IsNullOrEmpty(codigoAutorizante))
                    autorizanteSeleccionado = autorizantesList.FirstOrDefault(a => a.CodigoAutorizante == codigoAutorizante);

                if (autorizanteSeleccionado != null)
                {
                    // Cargar datos en el formulario
                    txtExpedienteEditar.Text = autorizanteSeleccionado.Expediente;
                    txtDetalleEditar.Text = autorizanteSeleccionado.Detalle;
                    txtMontoAutorizadoEditar.Text = autorizanteSeleccionado.MontoAutorizado.ToString("0.00");

                    if (autorizanteSeleccionado.MesAprobacion.HasValue)
                        txtMesAprobacionEditar.Text = autorizanteSeleccionado.MesAprobacion.Value.ToString("yyyy-MM-dd");

                    if (autorizanteSeleccionado.MesBase.HasValue)
                        txtMesBaseEditar.Text = autorizanteSeleccionado.MesBase.Value.ToString("yyyy-MM-dd");

                    // Seleccionar valores en dropdowns usando método helper
                    if (autorizanteSeleccionado.ConceptoId.HasValue)
                        SelectDropDownListByValue(ddlConceptoEditar, autorizanteSeleccionado.ConceptoId.Value.ToString());

                    if (autorizanteSeleccionado.EstadoId.HasValue)
                        SelectDropDownListByValue(ddlEstadoEditar, autorizanteSeleccionado.EstadoId.Value.ToString());

                    // Guardar estado de edición
                    Session["EditingAutorizanteId"] = autorizanteSeleccionado.Id;
                    Session["EditingCodigoAutorizante"] = autorizanteSeleccionado.CodigoAutorizante;


                    ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowEditModal", @"
                        $(document).ready(function() {
                            // Mostrar el modal
                            $('#modalEditarAutorizante').modal('show');
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
                if (gridviewRegistros.DataKeys == null || gridviewRegistros.DataKeys.Count <= e.RowIndex)
                {
                    lblMensaje.Text = "No se pudo identificar el registro.";
                    lblMensaje.CssClass = "alert alert-danger";
                    return;
                }

                var dataKey = gridviewRegistros.DataKeys[e.RowIndex];
                int id = 0; string codigoAutorizante = null;
                if (dataKey.Values["Id"] != null) int.TryParse(dataKey.Values["Id"].ToString(), out id);
                if (dataKey.Values["CodigoAutorizante"] != null) codigoAutorizante = dataKey.Values["CodigoAutorizante"].ToString();


                if (id <= 0)
                {
                    lblMensaje.Text = "No se puede eliminar: Id inválido.";
                    lblMensaje.CssClass = "alert alert-danger";
                    return;
                }

                bool eliminado = _autorizanteNegocioEF.Eliminar(id);

                if (eliminado)
                {
                    // Actualizar cache en sesión retirando el elemento
                    if (Session["GridDataAutorizantes"] is List<AutorizanteDTO> lista)
                    {
                        lista.RemoveAll(a => a.Id == id);
                        Session["GridDataAutorizantes"] = lista;
                    }

                    // Limpiar cache SADE
                    CalculoRedeterminacionNegocioEF.LimpiarCacheSade();

                    lblMensaje.Text = "Autorizante eliminado correctamente.";
                    lblMensaje.CssClass = "alert alert-success";

                    // Actualizar totales y rebind
                    _totalRecords = (Session["GridDataAutorizantes"] as List<AutorizanteDTO>)?.Count ?? 0;
                    ViewState["TotalRecords"] = _totalRecords;
                    BindGrid();
                }
                else
                {
                    lblMensaje.Text = "No se pudo eliminar (puede tener dependencias).";
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
                bool resultado = _autorizanteNegocioEF.ActualizarExpediente(codigoAutorizante, nuevoExpediente);

                if (resultado)
                {
                    // Actualizar también en la lista completa de Session
                    if (Session["GridDataAutorizantes"] is List<AutorizanteDTO> listaCompleta)
                    {
                        var autorizanteEnLista = listaCompleta.FirstOrDefault(a => a.CodigoAutorizante == codigoAutorizante);
                        if (autorizanteEnLista != null)
                        {
                            autorizanteEnLista.Expediente = nuevoExpediente;
                            // Recalcular BuzonSade y FechaSade sólo para este registro
                            if (string.IsNullOrWhiteSpace(nuevoExpediente))
                            {
                                autorizanteEnLista.BuzonSade = null;
                                autorizanteEnLista.FechaSade = null;
                            }
                            else
                            {
                                var (FechaUltimoPase, BuzonDestino) = SADEHelper.ObtenerInfoSADE(nuevoExpediente);
                                autorizanteEnLista.BuzonSade = BuzonDestino;
                                autorizanteEnLista.FechaSade = FechaUltimoPase;
                            }
                            // También reflejar en el objeto usado en esta operación
                            autorizante.BuzonSade = autorizanteEnLista.BuzonSade;
                            autorizante.FechaSade = autorizanteEnLista.FechaSade;
                        }
                    }

                    // Limpiar cache SADE ya que se modificó un expediente (autorizante o redeterminación)
                    CalculoRedeterminacionNegocioEF.LimpiarCacheSade();

                    // Rebind para que el GridView muestre inmediatamente el nuevo Buzon/Fecha
                    BindGrid();

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
                var paginationControl = FindControlRecursive(this, "paginationControl") as PaginationControl;
                paginationControl?.UpdatePaginationControls();
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
            var paginationControl = FindControlRecursive(this, "paginationControl") as PaginationControl;
            paginationControl?.UpdatePaginationControls();
        }

        #endregion

        #region Eventos del Control de Paginación

        protected void paginationControl_PageChanged(object sender, PaginationEventArgs e)
        {
            _currentPageIndex = e.PageIndex;
            CargarPaginaActual();
        }

        protected void paginationControl_PageSizeChanged(object sender, PaginationEventArgs e)
        {
            _pageSize = e.PageSize;
            ViewState["PageSize"] = _pageSize;
            _currentPageIndex = 0;
            CargarPaginaActual();
        }

        #endregion

        #region Métodos auxiliares de paginación

        /// Configura el control de paginación con los valores actuales.
        private void ConfigurarPaginationControl()
        {
            if (FindControlRecursive(this, "paginationControl") is PaginationControl paginationControl)
            {
                paginationControl.TotalRecords = _totalRecords;
                paginationControl.CurrentPageIndex = _currentPageIndex;
                paginationControl.PageSize = _pageSize;
                paginationControl.UpdatePaginationControls();
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
                List<AutorizanteDTO> todosLosRegistros;
                if (Session["GridDataAutorizantes"] != null)
                {
                    todosLosRegistros = (List<AutorizanteDTO>)Session["GridDataAutorizantes"];
                }
                else
                {
                    UsuarioEF usuario = ObtenerUsuarioActual();
                    todosLosRegistros = _calculoRedeterminacionNegocioEF.ListarAutorizantesCompleto(usuario);
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
                paginationControl?.UpdateSubtotal(totalMonto, cantidadRegistros);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en CalcularSubtotalParaPaginationControl: {ex.Message}");
                if (FindControlRecursive(this, "paginationControl") is CustomControls.PaginationControl paginationControl)
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
                // Preferir identificar por DataKeys: Id (autorizante) o IdRedeterminacion (redet)
                int nuevoEstadoId = int.Parse(ddlEstadoAutorizante.SelectedValue);

                int id = 0;
                int idRedeterminacion = 0;
                if (gridviewRegistros.DataKeys != null && gridviewRegistros.DataKeys.Count > row.RowIndex)
                {
                    var dataKey = gridviewRegistros.DataKeys[row.RowIndex];
                    if (dataKey != null && dataKey.Values != null)
                    {
                        if (dataKey.Values["Id"] != null) int.TryParse(dataKey.Values["Id"].ToString(), out id);
                        if (dataKey.Values["IdRedeterminacion"] != null) int.TryParse(dataKey.Values["IdRedeterminacion"].ToString(), out idRedeterminacion);
                    }
                }

                var listaCompleta = Session["GridDataAutorizantes"] as List<AutorizanteDTO>;
                if (listaCompleta == null)
                {
                    lblMensaje.Text = "Error: No hay datos en memoria.";
                    lblMensaje.CssClass = "alert alert-danger";
                    return;
                }

                AutorizanteDTO autorizanteDTO = null;

                // Si tenemos Id (autorizante real) buscar por Id
                if (id > 0)
                {
                    autorizanteDTO = listaCompleta.FirstOrDefault(a => a.Id == id);
                }

                // Si no se encontró y existe IdRedeterminacion, buscar por IdRedeterminacion
                if (autorizanteDTO == null && idRedeterminacion > 0)
                {
                    autorizanteDTO = listaCompleta.FirstOrDefault(a => a.IdRedeterminacion == idRedeterminacion);
                }

                // Fallback: si DataKeys no estaban disponibles o no localizaron, mapear usando lista filtrada y pageIndex
                if (autorizanteDTO == null)
                {
                    var filtrados = ObtenerDatosFiltradosActuales();
                    if (filtrados != null)
                    {
                        int pageIndex = _currentPageIndex;
                        int pageSizeLocal = _pageSize;
                        int rowIndex = row.RowIndex;
                        int indiceReal = pageIndex * pageSizeLocal + rowIndex;
                        if (indiceReal >= 0 && indiceReal < filtrados.Count)
                        {
                            var dtoEnFiltrados = filtrados[indiceReal];
                            if (dtoEnFiltrados != null)
                            {
                                // Buscar en la lista completa por Id preferentemente
                                if (dtoEnFiltrados.Id > 0)
                                    autorizanteDTO = listaCompleta.FirstOrDefault(a => a.Id == dtoEnFiltrados.Id);
                                if (autorizanteDTO == null && !string.IsNullOrEmpty(dtoEnFiltrados.CodigoAutorizante))
                                    autorizanteDTO = listaCompleta.FirstOrDefault(a => a.CodigoAutorizante == dtoEnFiltrados.CodigoAutorizante);
                            }
                        }
                    }
                }

                if (autorizanteDTO == null)
                {
                    lblMensaje.Text = "Error: no se pudo localizar el autorizante a actualizar.";
                    lblMensaje.CssClass = "alert alert-danger";
                    return;
                }

                // Ejecutar la actualización: preferir métodos por Id si existen
                bool actualizado = false;
                // Si es una redeterminación (IdRedeterminacion > 0) y existe negocio para redeterminaciones por Id
                if (autorizanteDTO.IdRedeterminacion > 0)
                {
                    // Utilizar RedeterminacionNegocio para actualizar por Id
                    var redNeg = new RedeterminacionNegocio();
                    var red = new Dominio.Redeterminacion { Id = autorizanteDTO.IdRedeterminacion, Etapa = new Dominio.EstadoRedet { Id = nuevoEstadoId } };
                    actualizado = redNeg.ActualizarEstado(red);
                }
                else if (autorizanteDTO.Id > 0)
                {
                    // Autorizante normal: usar AutorizanteNegocioEF.ObtenerPorId + Modificar o crear método especializado
                    var authNeg = new AutorizanteNegocioEF();
                    var entidad = authNeg.ObtenerPorId(autorizanteDTO.Id);
                    if (entidad != null)
                    {
                        entidad.EstadoId = nuevoEstadoId;
                        actualizado = authNeg.Modificar(entidad);
                    }
                    else
                    {
                        // Fallback a método por código si la entidad no existe por Id
                        actualizado = authNeg.ActualizarEstado(autorizanteDTO.CodigoAutorizante, nuevoEstadoId);
                    }
                }
                else
                {
                    // Último recurso: usar el método existente que acepta codigoAutorizante
                    var authNeg = new AutorizanteNegocioEF();
                    actualizado = authNeg.ActualizarEstado(autorizanteDTO.CodigoAutorizante, nuevoEstadoId);
                }

                if (actualizado)
                {
                    // Actualizar DTO en memoria
                    autorizanteDTO.EstadoId = nuevoEstadoId;
                    var estadoNegocio = new EstadoAutorizanteNegocioEF();
                    var estados = estadoNegocio.Listar();
                    var estadoSeleccionado = estados.FirstOrDefault(estado => estado.Id == nuevoEstadoId);
                    if (estadoSeleccionado != null)
                    {
                        autorizanteDTO.EstadoNombre = estadoSeleccionado.Nombre;
                    }

                    lblMensaje.Text = "Estado actualizado correctamente.";
                    lblMensaje.CssClass = "alert alert-success";
                    // Invalidate caches if needed
                    Session["GridDataAutorizantes"] = listaCompleta;
                    ViewState["NecesitaRecarga"] = null;
                    BindGrid();
                }
                else
                {
                    lblMensaje.Text = "Error al actualizar el estado.";
                    lblMensaje.CssClass = "alert alert-danger";
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
                TextBox txtExpediente = e.Row.FindControl("txtExpediente") as TextBox;

                if (ddlEstadoAutorizante != null)
                {
                    var estadoNegocio = new EstadoAutorizanteNegocioEF();
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

                    // Si es redeterminación virtual (IdRedeterminacion > 0) ocultar acciones
                    if (autorizante != null && autorizante.IdRedeterminacion > 0)
                    {
                        if (e.Row.FindControl("btnEliminar") is LinkButton btnEliminar) btnEliminar.Visible = false;
                        if (e.Row.FindControl("btnModificar") is LinkButton btnModificar) btnModificar.Visible = false;

                        // Deshabilitar controles y ajustar estilos para redeterminación virtual
                        txtExpediente.Enabled = false;
                        txtExpediente.CssClass = "form-control text-center";
                        txtExpediente.BorderStyle = BorderStyle.None;
                        txtExpediente.BackColor = Color.Transparent;

                        ddlEstadoAutorizante.Enabled = false;
                        ddlEstadoAutorizante.CssClass = "form-control text-center";
                        ddlEstadoAutorizante.BorderStyle = BorderStyle.None;
                        ddlEstadoAutorizante.BackColor = Color.Transparent;
                    }
                }
            }
        }

        #endregion
    }
}