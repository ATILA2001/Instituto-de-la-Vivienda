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
    public partial class CertificadosAdminEF : Page
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
        
        /// <summary>
        /// Total de páginas calculado a partir de totalRecords / pageSize.
        /// Se recalcula automáticamente cuando cambian los totales o el tamaño de página.
        /// </summary>
        private int totalPages = 0;

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
                //CalcularSubtotal();
                ActualizarControlesPaginacion();
            }
            else
            {
                // En postback, actualizar controles de paginación
                ActualizarControlesPaginacion();
            }
        }
        #endregion

        #region Métodos de Paginación (replicados exactamente desde AutorizantesAdminEF)
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
                UsuarioEF usuario = ObtenerUsuarioActual();

                // REGRESANDO A CARGA COMPLETA: necesaria para todos los campos calculados (Contrata, Proyecto, SIGAF, SADE, etc.)
                // OPTIMIZACIÓN: Solo cargar si no están en cache
                List<CertificadoDTO> todoLosCertificados;
                
                if (Session["GridData"] == null || ViewState["NecesitaRecarga"] != null)
                {
                    UsuarioEF usuarioActual = ObtenerUsuarioActual();
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
                ViewState["TotalRecords"] = totalRegistros;
                totalRecords = totalRegistros;
                Session["TotalRegistros"] = totalRegistros;

                // Usar BindGrid para paginación en memoria (mantiene todos los campos)
                BindGrid();
            }
            catch (Exception ex)
            {
                
                lblMensaje.Text = $"Error al cargar certificados: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        private UsuarioEF ObtenerUsuarioActual()
        {
            // Implementar según la lógica de sesión del sistema
            // Replicado desde AutorizantesAdminEF
            if (Session["Usuario"] != null)
            {
                var usuarioTradicional = (Usuario)Session["Usuario"];
                return new UsuarioEF 
                { 
                    Id = usuarioTradicional.Id,
                    Nombre = usuarioTradicional.Nombre,
                    Correo = usuarioTradicional.Correo,
                    Tipo = usuarioTradicional.Tipo, // true: Administrador, false: Usuario normal
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

            // Configurar paginación - EXACTAMENTE IGUAL que AutorizantesAdminEF
            int totalFiltrados = datosEnMemoria.Count;
            dgvCertificado.VirtualItemCount = totalFiltrados;
            dgvCertificado.PageSize = pageSize;
            dgvCertificado.PageIndex = currentPageIndex;
            dgvCertificado.DataSource = datosEnMemoria
                                        .Skip(currentPageIndex * pageSize)
                                        .Take(pageSize)
                                        .ToList();
            dgvCertificado.DataBind();
            PoblarFiltrosHeader();

            // Actualizar totalRecords y paginación según los datos filtrados
            totalRecords = totalFiltrados;
            ViewState["TotalRecords"] = totalFiltrados;
            ActualizarControlesPaginacion();

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
                // OPTIMIZACIÓN: Usar datos del caché en lugar de hacer nueva consulta DB
                List<CertificadoDTO> todosLosRegistros;
                
                // Intentar obtener datos del caché primero
                if (Session["GridData"] != null)
                {
                    todosLosRegistros = (List<CertificadoDTO>)Session["GridData"];
                }
                else
                {
                    // Solo si no hay caché, consultar BD (caso excepcional)
                    UsuarioEF usuario = ObtenerUsuarioActual();
                    todosLosRegistros = calculoRedeterminacionNegocio.ListarCertificadosYReliquidaciones(usuario);
                }
                
                // Aplicar filtro de texto general si existe
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

                // Aplicar filtros de TreeView (filtros de columnas)
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
                var lblSubtotalPaginacion = FindControlRecursive(this, "lblSubtotalPaginacion") as Label;
                if (lblSubtotalPaginacion != null)
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
                
                var lblSubtotalPaginacion = FindControlRecursive(this, "lblSubtotalPaginacion") as Label;
                if (lblSubtotalPaginacion != null)
                {
                    lblSubtotalPaginacion.Text = "Total: Error al calcular";
                }
            }
        }

        private void CargarPaginaActual()
        {
            // Guardar el estado actual en ViewState
            ViewState["CurrentPageIndex"] = currentPageIndex;
            ViewState["PageSize"] = pageSize;
            
            // Cargar los datos
            CargarListaCertificadosCompleta();
            
            // Actualizar controles de paginación
            ActualizarControlesPaginacion();
        }

        /// <summary>
        /// Actualiza todos los controles de paginación externa basándose en el estado actual.
        /// Replicado exactamente desde AutorizantesAdminEF.
        /// </summary>
        private void ActualizarControlesPaginacion()
        {
            // Obtener valores actuales
            totalRecords = (int)(ViewState["TotalRecords"] ?? 0);
            totalPages = totalRecords > 0 ? (int)Math.Ceiling((double)totalRecords / pageSize) : 1;
            
            // Buscar y actualizar controles dinámicamente hasta que se regenere el designer
            var lblPaginaInfo = FindControlRecursive(this, "lblPaginaInfo") as Label;
            if (lblPaginaInfo != null)
            {
                lblPaginaInfo.Text = $"Página {currentPageIndex + 1} de {totalPages}";
            }
            
            var ddlPageSizeExternal = FindControlRecursive(this, "ddlPageSizeExternal") as DropDownList;
            if (ddlPageSizeExternal != null)
            {
                ddlPageSizeExternal.SelectedValue = pageSize.ToString();
            }
            
            // Actualizar el label de subtotal en la paginación si existe
            // NOTA: No calcular subtotal aquí porque solo tenemos los datos de la página actual.
            // El subtotal correcto se calcula en CalcularSubtotal() que usa todos los registros.
            
            // Controlar visibilidad y estado de botones
            var lnkFirst = FindControlRecursive(this, "lnkFirst") as LinkButton;
            if (lnkFirst != null)
            {
                lnkFirst.Enabled = currentPageIndex > 0;
                lnkFirst.CssClass = currentPageIndex > 0 ? "btn btn-sm btn-outline-primary" : "btn btn-sm btn-outline-secondary disabled";
            }
            
            var lnkPrev = FindControlRecursive(this, "lnkPrev") as LinkButton;
            if (lnkPrev != null)
            {
                lnkPrev.Enabled = currentPageIndex > 0;
                lnkPrev.CssClass = currentPageIndex > 0 ? "btn btn-sm btn-outline-primary" : "btn btn-sm btn-outline-secondary disabled";
            }
            
            var lnkNext = FindControlRecursive(this, "lnkNext") as LinkButton;
            if (lnkNext != null)
            {
                lnkNext.Enabled = currentPageIndex < totalPages - 1;
                lnkNext.CssClass = currentPageIndex < totalPages - 1 ? "btn btn-sm btn-outline-primary" : "btn btn-sm btn-outline-secondary disabled";
            }
            
            var lnkLast = FindControlRecursive(this, "lnkLast") as LinkButton;
            if (lnkLast != null)
            {
                lnkLast.Enabled = currentPageIndex < totalPages - 1;
                lnkLast.CssClass = currentPageIndex < totalPages - 1 ? "btn btn-sm btn-outline-primary" : "btn btn-sm btn-outline-secondary disabled";
            }
            
            // Configurar botones numerados
            ConfigurarBotonesNumerados();
        }

        /// <summary>
        /// Configura los botones numerados de paginación.
        /// Actualizado para funcionar dinámicamente con cualquier cantidad de botones.
        /// </summary>
        private void ConfigurarBotonesNumerados()
        {
            string[] pageButtonIds = { "lnkPage1", "lnkPage2", "lnkPage3", "lnkPage4", "lnkPage5" };
            int totalButtons = pageButtonIds.Length;
            
            // Calcular rango de páginas a mostrar
            int buttonsToShow = Math.Min(totalButtons, totalPages);
            int startPage, endPage;
            
            if (totalPages <= totalButtons)
            {
                // Si hay menos páginas que botones, mostrar todas
                startPage = 0;
                endPage = totalPages - 1;
            }
            else
            {
                // Lógica de centrado: intentar poner el botón actual en el centro
                int centerPosition = totalButtons / 2; // Posición central calculada dinámicamente
                startPage = Math.Max(0, currentPageIndex - centerPosition);
                endPage = Math.Min(totalPages - 1, startPage + totalButtons - 1);
                
                // Ajustar si nos quedamos cortos al final
                if (endPage - startPage + 1 < totalButtons && totalPages >= totalButtons)
                {
                    startPage = Math.Max(0, endPage - totalButtons + 1);
                }
            }
            
            // Configurar cada botón
            for (int i = 0; i < totalButtons; i++)
            {
                var pageButton = FindControlRecursive(this, pageButtonIds[i]) as LinkButton;
                if (pageButton != null)
                {
                    int pageIndex = startPage + i;
                    
                    if (i < buttonsToShow && pageIndex <= endPage && pageIndex < totalPages)
                    {
                        pageButton.Text = (pageIndex + 1).ToString();
                        pageButton.CommandArgument = pageIndex.ToString();
                        pageButton.Visible = true;
                        
                        if (pageIndex == currentPageIndex)
                        {
                            pageButton.CssClass = "btn btn-sm btn-primary mx-1";
                        }
                        else
                        {
                            pageButton.CssClass = "btn btn-sm btn-outline-primary mx-1";
                        }
                    }
                    else
                    {
                        pageButton.Visible = false;
                    }
                }
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

            foreach (Control control in root.Controls)
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
            // Reiniciar a primera página cuando se aplican filtros
            currentPageIndex = 0;
            ViewState["CurrentPageIndex"] = currentPageIndex;
            
            // OPTIMIZACIÓN: No recargar datos desde BD, solo aplicar filtros en memoria
            // Los datos ya están completos en Session
            BindGrid();
            
            // Actualizar controles de paginación
            ActualizarControlesPaginacion();
        }

        #region Eventos de Navegación de Paginación Externa (replicados desde AutorizantesAdminEF)

        protected void lnkFirst_Click(object sender, EventArgs e)
        {
            currentPageIndex = 0;
            CargarPaginaActual();
        }

        protected void lnkPrev_Click(object sender, EventArgs e)
        {
            if (currentPageIndex > 0)
            {
                currentPageIndex--;
                CargarPaginaActual();
            }
        }

        protected void lnkNext_Click(object sender, EventArgs e)
        {
            if (currentPageIndex < totalPages - 1)
            {
                currentPageIndex++;
                CargarPaginaActual();
            }
        }

        protected void lnkLast_Click(object sender, EventArgs e)
        {
            currentPageIndex = totalPages - 1;
            CargarPaginaActual();
        }

        protected void lnkPage_Click(object sender, EventArgs e)
        {
            LinkButton btn = (LinkButton)sender;
            int pageIndex = int.Parse(btn.CommandArgument);
            currentPageIndex = pageIndex;
            CargarPaginaActual();
        }

        protected void ddlPageSizeExternal_SelectedIndexChanged(object sender, EventArgs e)
        {
            var ddlPageSizeExternal = FindControlRecursive(this, "ddlPageSizeExternal") as DropDownList;
            if (ddlPageSizeExternal != null)
            {
                pageSize = int.Parse(ddlPageSizeExternal.SelectedValue);
                currentPageIndex = 0; // Reiniciar a la primera página
                CargarPaginaActual();
            }
        }

        #endregion

        protected void btnExportarExcel_Click(object sender, EventArgs e)
        {
            try
            {
                // OPTIMIZACIÓN: Usar datos del caché para exportación
                List<CertificadoDTO> todosLosCertificados;
                
                if (Session["GridData"] != null)
                {
                    todosLosCertificados = (List<CertificadoDTO>)Session["GridData"];
                }
                else
                {
                    // Solo si no hay caché, consultar BD
                    UsuarioEF usuario = ObtenerUsuarioActual();
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

                ExcelHelper.ExportarDatosGenericos(dgvCertificado, todosLosCertificados, mapeoColumnas, "Certificados");
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
                CertificadoEF certificado = new CertificadoEF
                {
                    ExpedientePago = txtExpediente.Text.Trim(),
                    MontoTotal = decimal.Parse(txtMontoAutorizado.Text.Trim()),
                    MesAprobacion = string.IsNullOrWhiteSpace(txtFecha.Text) ? (DateTime?)null : DateTime.Parse(txtFecha.Text),
                    TipoPagoId = int.Parse(ddlTipo.SelectedValue)
                };

                bool resultado;
                if (Session["EditingCertificadoId"] != null)
                {
                    certificado.Id = (int)Session["EditingCertificadoId"];
                    resultado = negocio.Modificar(certificado);
                    lblMensaje.Text = resultado ? "Certificado modificado exitosamente!" : "Hubo un problema al modificar el certificado.";
                }
                else
                {
                    certificado.CodigoAutorizante = ddlAutorizante.SelectedItem.Text;
                    resultado = negocio.Agregar(certificado);
                    lblMensaje.Text = resultado ? "Certificado agregado exitosamente!" : "Hubo un problema al agregar el certificado.";
                }

                lblMensaje.CssClass = resultado ? "alert alert-success" : "alert alert-danger";

                if (resultado)
                {
                    // Limpiar cache SADE ya que se agregó/modificó un certificado
                    CalculoRedeterminacionNegocioEF.LimpiarCacheSade();
                    
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "HideModal", "$('#modalAgregar').modal('hide');", true);
                    CargarPaginaActual(); // Usar método optimizado
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

        #region Métodos Obsoletos Reemplazados por Paginación Optimizada
        /// <summary>
        /// OBSOLETO: Reemplazado por CargarPaginaActual() para paginación optimizada.
        /// Se mantiene para compatibilidad con código existente que aún lo llame.
        /// </summary>
        [Obsolete("Usar CargarPaginaActual() para mejor rendimiento con paginación real en BD")]
        private void CargarGrillaCompleta()
        {
            CargarPaginaActual();
        }

        /// <summary>
        protected void dgvCertificado_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            // Este método ahora está obsoleto ya que usamos paginación externa
            // Mantenemos por compatibilidad pero no hacemos nada
            e.Cancel = true;
        }
        #endregion




        protected void dgvCertificado_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                int idCertificado = Convert.ToInt32(dgvCertificado.SelectedDataKey.Value);
                CertificadoEF certificadoSeleccionado;
                using (var context = new IVCdbContext())
                {
                    // Se busca el certificado directamente en la base de datos.
                    certificadoSeleccionado = context.Certificados.Find(idCertificado);
                }

                if (certificadoSeleccionado != null)
                {
                    Session["EditingCertificadoId"] = idCertificado;
                    txtExpediente.Text = certificadoSeleccionado.ExpedientePago;
                    txtMontoAutorizado.Text = certificadoSeleccionado.MontoTotal.ToString("0.00");
                    txtFecha.Text = certificadoSeleccionado.MesAprobacion?.ToString("yyyy-MM-dd");
                    SelectDropDownListByValue(ddlTipo, certificadoSeleccionado.TipoPagoId.ToString());

                    ScriptManager.RegisterStartupScript(this, this.GetType(), "UpdateModalAndShow", @"
                    $('#modalAgregar .modal-title').text('Modificar Certificado');
                    $('#autorizanteContainer').hide();
                    $('#modalAgregar').modal('show');", true);
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
        protected void dgvCertificado_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            try
            {
                var id = Convert.ToInt32(dgvCertificado.DataKeys[e.RowIndex].Value);
                if (negocio.Eliminar(id))
                {
                    lblMensaje.Text = "Certificado eliminado correctamente.";
                    lblMensaje.CssClass = "alert alert-success";
                    
                    // Limpiar cache SADE ya que se eliminó un certificado
                    CalculoRedeterminacionNegocioEF.LimpiarCacheSade();
                    
                    // Se recarga toda la grilla para reflejar la eliminación.
                    CargarPaginaActual(); // Usar método optimizado
                    //PoblarFiltrosHeader();
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
                    if (dgvCertificado.HeaderRow?.FindControl(controlId) is TreeViewSearch control)
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

                if (dgvCertificado.HeaderRow?.FindControl("cblsHeaderEstado") is TreeViewSearch cblsHeaderEstado)
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

                if (dgvCertificado.HeaderRow?.FindControl("cblsHeaderMesCertificado") is TreeViewSearch cblsHeaderMesCertificado)
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
            ddlAutorizante.DataTextField = "CodigoAutorizante"; // Corregido: usar CodigoAutorizante en lugar de Nombre
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

                int indiceReal = (dgvCertificado.PageIndex * dgvCertificado.PageSize) + rowIndex;
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
            if (dgvCertificado.HeaderRow == null) return data;

            try
            {
                // Función auxiliar para parsear IDs de forma segura.
                Func<TreeViewSearch, List<int>> getSelectedIds = (tv) => tv.ExpandedSelectedValues.Select(s => int.TryParse(s, out int id) ? id : -1).Where(id => id != -1).ToList();

                // Aplicar filtro por Área
                if (dgvCertificado.HeaderRow.FindControl("cblsHeaderArea") is TreeViewSearch cblsHeaderArea && cblsHeaderArea.ExpandedSelectedValues.Any())
                    data = data.Where(c => c.AreaId.HasValue && getSelectedIds(cblsHeaderArea).Contains(c.AreaId.Value)).ToList();

                // Aplicar filtro por Obra
                if (dgvCertificado.HeaderRow.FindControl("cblsHeaderObra") is TreeViewSearch cblsHeaderObra && cblsHeaderObra.ExpandedSelectedValues.Any())
                    data = data.Where(c => c.ObraId.HasValue && getSelectedIds(cblsHeaderObra).Contains(c.ObraId.Value)).ToList();

                // Aplicar filtro por Barrio
                if (dgvCertificado.HeaderRow.FindControl("cblsHeaderBarrio") is TreeViewSearch cblsHeaderBarrio && cblsHeaderBarrio.ExpandedSelectedValues.Any())
                    data = data.Where(c => c.BarrioId.HasValue && getSelectedIds(cblsHeaderBarrio).Contains(c.BarrioId.Value)).ToList();

                // Aplicar filtro por Proyecto (certificados y reliquidaciones)
                if (dgvCertificado.HeaderRow.FindControl("cblsHeaderProyecto") is TreeViewSearch cblsHeaderProyecto && cblsHeaderProyecto.ExpandedSelectedValues.Any())
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
                if (dgvCertificado.HeaderRow.FindControl("cblsHeaderEmpresa") is TreeViewSearch cblsHeaderEmpresa && cblsHeaderEmpresa.ExpandedSelectedValues.Any())
                    data = data.Where(c => c.EmpresaId.HasValue && getSelectedIds(cblsHeaderEmpresa).Contains(c.EmpresaId.Value)).ToList();

                // Aplicar filtro por Código Autorizante
                if (dgvCertificado.HeaderRow.FindControl("cblsHeaderCodigoAutorizante") is TreeViewSearch cblsHeaderCodigoAutorizante && cblsHeaderCodigoAutorizante.ExpandedSelectedValues.Any())
                    data = data.Where(c => c.AutorizanteId.HasValue && getSelectedIds(cblsHeaderCodigoAutorizante).Contains(c.AutorizanteId.Value)).ToList();

                // Aplicar filtro por Estado (ahora por texto, no por ID numérico)
                if (dgvCertificado.HeaderRow.FindControl("cblsHeaderEstado") is TreeViewSearch cblsHeaderEstado)
                {
                    var selectedEstados = cblsHeaderEstado.ExpandedSelectedValues;
                    if (selectedEstados != null && selectedEstados.Any())
                    {
                        data = data.Where(c => !string.IsNullOrEmpty(c.Estado) && selectedEstados.Contains(c.Estado)).ToList();
                    }
                    // Si no hay filtros seleccionados, no se filtra por estado (se muestran todos)
                }

                // Aplicar filtro por Tipo
                if (dgvCertificado.HeaderRow.FindControl("cblsHeaderTipo") is TreeViewSearch cblsHeaderTipo && cblsHeaderTipo.ExpandedSelectedValues.Any())
                    data = data.Where(c => c.TipoPagoId.HasValue && getSelectedIds(cblsHeaderTipo).Contains(c.TipoPagoId.Value)).ToList();

                // Aplicar filtro por Mes Certificado
                if (dgvCertificado.HeaderRow.FindControl("cblsHeaderMesCertificado") is TreeViewSearch cblsHeaderMesCertificado && cblsHeaderMesCertificado.SelectedValues.Any())
                {
                    // CORRECCIÓN: Se reemplaza DbFunctions.TruncateTime por .Date, que es el equivalente para LINQ to Objects.
                    var selectedDates = cblsHeaderMesCertificado.SelectedValues
                        .Select(s => DateTime.TryParse(s, out DateTime dt) ? (DateTime?)dt.Date : null)
                        .Where(d => d.HasValue).Select(d => d.Value)
                        .ToList();
                    data = data.Where(c => c.MesAprobacion.HasValue && selectedDates.Contains(c.MesAprobacion.Value.Date)).ToList();
                }

                // Aplicar filtro por Línea de Gestión
                if (dgvCertificado.HeaderRow.FindControl("cblsHeaderLinea") is TreeViewSearch cblsHeaderLinea && cblsHeaderLinea.ExpandedSelectedValues.Any())
                    data = data.Where(c => c.LineaGestionId.HasValue && getSelectedIds(cblsHeaderLinea).Contains(c.LineaGestionId.Value)).ToList();
            }
            catch (Exception)
            {
            }

            return data;
        }


    }
}
