using Dominio;
using Negocio;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebForms
{
    public partial class FormulacionEF : System.Web.UI.Page
    {
        private FormulacionNegocioEF negocio;

        /// <summary>
        /// Índice de la página actual en el sistema de paginación manual.
        /// Este sistema de paginación es independiente del GridView nativo y permite mejor control.
        /// </summary>
        private int currentPageIndex = 0;
        
        /// <summary>
        /// Cantidad de registros por página. Por defecto 12, configurable por el usuario.
        /// Se mantiene en ViewState para persistir la preferencia del usuario.
        /// </summary>
        private int pageSize = 12;
        
        /// <summary>
        /// Total de registros filtrados. Se usa para calcular el número de páginas.
        /// </summary>
        private int totalRecords = 0;

        protected void Page_Init(object sender, EventArgs e)
        {
            negocio = new FormulacionNegocioEF(new IVCdbContext());
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // Cargar valores de paginación desde ViewState
            currentPageIndex = (int)(ViewState["CurrentPageIndex"] ?? 0);
            pageSize = (int)(ViewState["PageSize"] ?? 12);

            if (!IsPostBack)
            {
                BindDropDownList();
                CargarListaFormulaciones();
            }
        }

        protected void btnExportarExcel_Click(object sender, EventArgs e)
        {
            try
            {
                var lista = Session["formulacionesCompletas"] as List<Dominio.FormulacionEF>;
                if (lista == null)
                {
                    CargarListaFormulaciones();
                    lista = Session["formulacionesCompletas"] as List<Dominio.FormulacionEF>;
                }

                if (lista != null && lista.Any())
                {
                    var mapeoColumnas = new Dictionary<string, string>
                    {
                        { "Área", "ObraEF.Area.Nombre" },
                        { "Empresa", "ObraEF.Empresa.Nombre" },
                        { "Contrata", "ObraEF.Contrata.Nombre" },
                        { "Barrio", "ObraEF.Barrio.Nombre" },
                        { "Nombre de Obra", "ObraEF.Descripcion" },
                        { "Monto 2026", "Monto_26" },
                        { "Monto 2027", "Monto_27" },
                        { "Monto 2028", "Monto_28" },
                        { "Mes Base", "MesBase" },
                        { "Unidad de Medida", "UnidadMedidaEF.Nombre" },
                        { "Valor de Medida", "ValorMedida" },
                        { "Observaciones", "Observaciones" },
                        { "Prioridad", "PrioridadEF.Nombre" }
                    };
                    ExcelHelper.ExportarDatosGenericos(dgvFormulacion, lista, mapeoColumnas, "FormulacionesEF");
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

        protected void OnAcceptChanges(object sender, EventArgs e)
        {
            CargarListaFormulaciones();
        }

        protected void dgvFormulacion_DataBound(object sender, EventArgs e)
        {
            if (dgvFormulacion.HeaderRow != null)
            {
                UsuarioEF usuarioActual = UserHelper.GetFullCurrentUser();
                if (!(Session["formulacionesCompletas"] is List<Dominio.FormulacionEF> formulacionesCompletas))
                {
                    formulacionesCompletas = negocio.ListarPorUsuario(usuarioActual);
                    Session["formulacionesCompletas"] = formulacionesCompletas;
                }

                if (dgvFormulacion.HeaderRow.FindControl("cblsHeaderArea") is CustomControls.TreeViewSearch cblsHeaderArea)
                {
                    var areasUnicas = formulacionesCompletas
                        .Where(f => f.ObraEF?.Area != null)
                        .Select(f => f.ObraEF.Area)
                        .Distinct()
                        .OrderBy(a => a.Nombre)
                        .Select(a => new { Id = a.Id.ToString(), Nombre = a.Nombre })
                        .ToList();
                    cblsHeaderArea.DataSource = areasUnicas;
                    cblsHeaderArea.DataBind();
                    cblsHeaderArea.AcceptChanges += OnAcceptChanges;
                }

                if (dgvFormulacion.HeaderRow.FindControl("cblsHeaderLineaGestion") is CustomControls.TreeViewSearch cblsHeaderLineaGestion)
                {
                    var lineasUnicas = formulacionesCompletas
                        .Where(f => f.ObraEF?.Proyecto?.LineaGestionEF != null)
                        .Select(f => f.ObraEF.Proyecto.LineaGestionEF)
                        .Distinct()
                        .OrderBy(lg => lg.Nombre)
                        .Select(lg => new { Id = lg.Id.ToString(), lg.Nombre })
                        .ToList();
                    cblsHeaderLineaGestion.DataSource = lineasUnicas;
                    cblsHeaderLineaGestion.DataBind();
                    cblsHeaderLineaGestion.AcceptChanges += OnAcceptChanges;
                }

                if (dgvFormulacion.HeaderRow.FindControl("cblsHeaderProyecto") is CustomControls.TreeViewSearch cblsHeaderProyecto)
                {
                    var proyectosUnicos = formulacionesCompletas
                        .Where(f => f.ObraEF?.Proyecto != null)
                        .Select(f => f.ObraEF.Proyecto)
                        .Distinct()
                        .OrderBy(p => p.Nombre)
                        .Select(p => new { Id = p.Id.ToString(), p.Nombre })
                        .ToList();
                    cblsHeaderProyecto.DataSource = proyectosUnicos;
                    cblsHeaderProyecto.DataBind();
                    cblsHeaderProyecto.AcceptChanges += OnAcceptChanges;
                }

                if (dgvFormulacion.HeaderRow.FindControl("cblsHeaderMonto2026") is CustomControls.TreeViewSearch cblsHeaderMonto2026)
                {
                    var montosUnicos = formulacionesCompletas
                        .Where(f => f.Monto_26.HasValue)
                        .Select(f => f.Monto_26.Value)
                        .Distinct()
                        .OrderBy(m => m)
                        .Select(m => new { Id = m.ToString(CultureInfo.InvariantCulture), Nombre = m.ToString("N2") }) // Use "N2" or "C" for display
                        .ToList();
                    cblsHeaderMonto2026.DataSource = montosUnicos;
                    cblsHeaderMonto2026.DataBind();
                    cblsHeaderMonto2026.AcceptChanges += OnAcceptChanges;
                }

                if (dgvFormulacion.HeaderRow.FindControl("cblsHeaderPrioridad") is CustomControls.TreeViewSearch cblsHeaderPrioridad)
                {
                    var prioridadesUnicas = formulacionesCompletas
                        .Where(f => f.PrioridadEF != null)
                        .Select(f => f.PrioridadEF)
                        .Distinct()
                        .OrderBy(p => p.Nombre)
                        .Select(p => new { Id = p.Id.ToString(), p.Nombre })
                        .ToList();
                    cblsHeaderPrioridad.DataSource = prioridadesUnicas;
                    cblsHeaderPrioridad.DataBind();
                    cblsHeaderPrioridad.AcceptChanges += OnAcceptChanges;
                }
            }
        }

        protected void btnShowAddModal_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();
            ScriptManager.RegisterStartupScript(this, this.GetType(), "ResetModalTitleAndShow", @"
                $(document).ready(function() {
                    $('#modalAgregar .modal-title').text('Agregar Formulación');
                    document.getElementById('" + btnAgregar.ClientID + @"').value = 'Agregar';
                    $('.col-12:first').show();
                    $('#modalAgregar').modal('show');
                });", true);

            btnAgregar.Text = "Agregar";
            Session["EditingFormulacionEFId"] = null;
        }

        protected void btnAgregar_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            try
            {
                var formulacion = new Dominio.FormulacionEF
                {
                    ObraId = int.Parse(ddlObra.SelectedValue),
                    Monto_26 = string.IsNullOrWhiteSpace(txtMonto26.Text) ? (decimal?)null : decimal.Parse(txtMonto26.Text),
                    Monto_27 = string.IsNullOrWhiteSpace(txtMonto27.Text) ? (decimal?)null : decimal.Parse(txtMonto27.Text),
                    Monto_28 = string.IsNullOrWhiteSpace(txtMonto28.Text) ? (decimal?)null : decimal.Parse(txtMonto28.Text),
                    MesBase = string.IsNullOrWhiteSpace(txtMesBase.Text) ? (DateTime?)null : DateTime.Parse(txtMesBase.Text),
                    Observaciones = txtObservaciones.Text,
                    UnidadMedidaId = string.IsNullOrEmpty(ddlUnidadMedida.SelectedValue) ? (int?)null : int.Parse(ddlUnidadMedida.SelectedValue),
                    ValorMedida = string.IsNullOrWhiteSpace(txtValorMedida.Text) ? (decimal?)null : decimal.Parse(txtValorMedida.Text),
                    PrioridadId = string.IsNullOrEmpty(ddlPrioridades.SelectedValue) ? (int?)null : int.Parse(ddlPrioridades.SelectedValue)
                };

                if (Session["EditingFormulacionEFId"] != null)
                {
                    formulacion.Id = (int)Session["EditingFormulacionEFId"];
                    negocio.Modificar(formulacion);
                    lblMensaje.Text = "Formulación modificada exitosamente!";
                }
                else
                {
                    negocio.Agregar(formulacion);
                    lblMensaje.Text = "Formulación agregada exitosamente!";
                }

                lblMensaje.CssClass = "alert alert-success";
                LimpiarFormulario();
                ScriptManager.RegisterStartupScript(this, this.GetType(), "HideModal", "$('#modalAgregar').modal('hide');", true);
                Session["EditingFormulacionEFId"] = null;
                CargarListaFormulaciones();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
                ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowModalWithError", "$('#modalAgregar').modal('show');", true);
            }
        }

        protected void dgvFormulacion_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                int id = Convert.ToInt32(dgvFormulacion.SelectedDataKey.Value);
                var lista = Session["formulacionesCompletas"] as List<Dominio.FormulacionEF>;
                var formulacion = lista?.FirstOrDefault(f => f.Id == id);
                if (formulacion != null)
                {
                    ddlObra.SelectedValue = formulacion.ObraId.ToString();
                    txtMonto26.Text = formulacion.Monto_26?.ToString() ?? "";
                    txtMonto27.Text = formulacion.Monto_27?.ToString() ?? "";
                    txtMonto28.Text = formulacion.Monto_28?.ToString() ?? "";
                    txtMesBase.Text = formulacion.MesBase?.ToString("yyyy-MM-dd") ?? "";
                    ddlUnidadMedida.SelectedValue = formulacion.UnidadMedidaId?.ToString() ?? "";
                    txtValorMedida.Text = formulacion.ValorMedida?.ToString() ?? "";
                    txtObservaciones.Text = formulacion.Observaciones ?? "";
                    ddlPrioridades.SelectedValue = formulacion.PrioridadId?.ToString() ?? "";

                    Session["EditingFormulacionEFId"] = formulacion.Id;

                    ScriptManager.RegisterStartupScript(this, this.GetType(), "EditModal", @"
                        $('#modalAgregar .modal-title').text('Modificar Formulación');
                        $('.col-12:first').hide();
                        $('#modalAgregar').modal('show');", true);

                    btnAgregar.Text = "Modificar";
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cargar los datos: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        protected void dgvFormulacion_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            try
            {
                int id = Convert.ToInt32(dgvFormulacion.DataKeys[e.RowIndex].Value);
                if (negocio.Eliminar(id))
                {
                    lblMensaje.Text = "Formulación eliminada correctamente.";
                    lblMensaje.CssClass = "alert alert-success";
                    CargarListaFormulaciones();
                }
                else
                {
                    lblMensaje.Text = "No se pudo eliminar la formulación.";
                    lblMensaje.CssClass = "alert alert-danger";
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al eliminar: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        private void CargarListaFormulaciones()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[FormulacionEF] Iniciando CargarListaFormulaciones");
                
                UsuarioEF usuarioActual = UserHelper.GetFullCurrentUser();
                if (usuarioActual == null)
                {
                    System.Diagnostics.Debug.WriteLine("[FormulacionEF] ERROR: usuarioActual es null");
                    lblMensaje.Text = "Error: No se pudo obtener el usuario actual";
                    lblMensaje.CssClass = "alert alert-danger";
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"[FormulacionEF] Usuario obtenido:");
                System.Diagnostics.Debug.WriteLine($"  - ID: {usuarioActual.Id}");
                System.Diagnostics.Debug.WriteLine($"  - NombreUsuario: {usuarioActual.Nombre}");
                System.Diagnostics.Debug.WriteLine($"  - Tipo (Es Admin): {usuarioActual.Tipo}");
                System.Diagnostics.Debug.WriteLine($"  - AreaId: {usuarioActual.AreaId}");
                System.Diagnostics.Debug.WriteLine($"  - Area es null: {usuarioActual.Area == null}");

                // Si es administrador (Tipo = true), no necesita área
                if (!usuarioActual.Tipo && usuarioActual.Area == null && !usuarioActual.AreaId.HasValue)
                {
                    System.Diagnostics.Debug.WriteLine("[FormulacionEF] ERROR: Usuario no administrador sin área asignada");
                    lblMensaje.Text = "Error: El usuario no tiene área asignada y no es administrador";
                    lblMensaje.CssClass = "alert alert-danger";
                    return;
                }

                // Guardar usuario en sesión para uso en BindGrid
                Session["UsuarioLogueado"] = usuarioActual;

                // Reiniciar paginación
                currentPageIndex = 0;

                // Usar nuevo método de paginación (no cargar todos los datos en memoria)
                BindGrid();

                System.Diagnostics.Debug.WriteLine("[FormulacionEF] CargarListaFormulaciones completado exitosamente con paginación");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FormulacionEF] ERROR en CargarListaFormulaciones: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[FormulacionEF] StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[FormulacionEF] InnerException: {ex.InnerException.Message}");
                }
                
                lblMensaje.Text = $"Error al cargar las formulaciones: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
                
                // En caso de error, limpiar la grilla
                dgvFormulacion.DataSource = new List<Dominio.FormulacionEF>();
                dgvFormulacion.DataBind();
            }
        }
            
        

        /// <summary>
        /// Método central para refrescar la visualización del GridView.
        /// Utiliza paginación a nivel de base de datos para mejor rendimiento.
        /// </summary>
        private void BindGrid()
        {
            try
            {
                var usuario = (UsuarioEF)Session["UsuarioLogueado"];
                if (usuario == null) return;

                using (var context = new IVCdbContext())
                {
                    var negocio = new FormulacionNegocioEF(context);

                    // Obtener filtros de búsqueda
                    string filtroGeneral = txtBuscar?.Text?.Trim();
                    
                    // Contar total de registros con filtros
                    totalRecords = negocio.ContarPorUsuario(usuario, filtroGeneral);

                    // Obtener página actual con paginación a nivel de DB
                    var formulacionesPagina = negocio.ListarPorUsuarioPaginado(usuario, currentPageIndex, pageSize, filtroGeneral);

                    // Configurar ViewState para mantener el estado
                    ViewState["TotalRecords"] = totalRecords;
                    ViewState["CurrentPageIndex"] = currentPageIndex;
                    ViewState["PageSize"] = pageSize;

                    // Bind a la grilla
                    dgvFormulacion.DataSource = formulacionesPagina;
                    dgvFormulacion.DataBind();

                    // Actualizar controles de paginación
                    ActualizarControlesPaginacion();

                    // Calcular subtotales para la página actual
                    CalcularSubtotal(formulacionesPagina);
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cargar la grilla: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        private void BindDropDownList()
        {
            try
            {
                ddlObra.DataSource = new ObraNegocioEF().ListarParaDDL();
                ddlObra.DataTextField = "Descripcion";
                ddlObra.DataValueField = "Id";
                ddlObra.DataBind();
                ddlObra.Items.Insert(0, new ListItem("-- Seleccione una obra --", ""));

                ddlUnidadMedida.DataSource = new UnidadMedidaNegocioEF().Listar();
                ddlUnidadMedida.DataTextField = "Nombre";
                ddlUnidadMedida.DataValueField = "Id";
                ddlUnidadMedida.DataBind();
                ddlUnidadMedida.Items.Insert(0, new ListItem("-- Seleccione una unidad --", ""));

                ddlPrioridades.DataSource = new PrioridadNegocioEF().Listar();
                ddlPrioridades.DataTextField = "Nombre";
                ddlPrioridades.DataValueField = "Id";
                ddlPrioridades.DataBind();
                ddlPrioridades.Items.Insert(0, new ListItem("-- Seleccione una prioridad --", ""));
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cargar listas: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        private void LimpiarFormulario()
        {
            ddlObra.SelectedIndex = 0;
            txtMonto26.Text = "";
            txtMonto27.Text = "";
            txtMonto28.Text = "";
            txtMesBase.Text = "";
            ddlUnidadMedida.SelectedIndex = 0;
            txtValorMedida.Text = "";
            txtObservaciones.Text = "";
            ddlPrioridades.SelectedIndex = 0;
        }

        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            try
            {
                // Reiniciar paginación al aplicar filtros
                currentPageIndex = 0;
                
                // Llamar a BindGrid que aplicará los filtros automáticamente
                BindGrid();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al filtrar: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        protected void BtnClearFilters_Click(object sender, EventArgs e)
        {
            txtBuscar.Text = string.Empty;

            CustomControls.TreeViewSearch.ClearAllFiltersOnPage(this.Page);

            // Reiniciar paginación al limpiar filtros
            currentPageIndex = 0;
            
            // Recargar datos sin filtros
            BindGrid();
        }

        protected void dgvFormulacion_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            try
            {
                dgvFormulacion.PageIndex = e.NewPageIndex;
                CargarListaFormulaciones();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cambiar de página: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        /// <summary>
        /// Calcula el total plurianual (suma de Monto_26, Monto_27 y Monto_28)
        /// </summary>
        protected string CalcularPlurianual(object monto26, object monto27, object monto28)
        {
            try
            {
                decimal total = 0;
                
                if (monto26 != null && monto26 != DBNull.Value)
                    total += Convert.ToDecimal(monto26);
                    
                if (monto27 != null && monto27 != DBNull.Value)
                    total += Convert.ToDecimal(monto27);
                    
                if (monto28 != null && monto28 != DBNull.Value)
                    total += Convert.ToDecimal(monto28);
                
                return total.ToString("C");
            }
            catch
            {
                return "$0.00";
            }
        }

        #region Métodos de Paginación

        /// <summary>
        /// Actualiza la visibilidad y estado de los controles de paginación
        /// </summary>
        private void ActualizarControlesPaginacion()
        {
            int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            
            // Habilitar/deshabilitar botones de navegación
            lnkFirst.Enabled = currentPageIndex > 0;
            lnkPrev.Enabled = currentPageIndex > 0;
            lnkNext.Enabled = currentPageIndex < totalPages - 1;
            lnkLast.Enabled = currentPageIndex < totalPages - 1;
            
            // Actualizar info de página
            lblPaginaInfo.Text = $"Página {currentPageIndex + 1} de {Math.Max(totalPages, 1)}";
            
            // Configurar botones de página
            ConfigurarBotonesPagina(totalPages);
        }

        private void ConfigurarBotonesPagina(int totalPages)
        {
            var botonesPagina = new[] { lnkPage1, lnkPage2, lnkPage3, lnkPage4, lnkPage5 };
            
            // Calcular rango de páginas a mostrar
            int startPage = Math.Max(0, currentPageIndex - 2);
            int endPage = Math.Min(totalPages - 1, startPage + 4);
            
            // Ajustar startPage si hay menos de 5 páginas al final
            if (endPage - startPage < 4)
                startPage = Math.Max(0, endPage - 4);
            
            for (int i = 0; i < botonesPagina.Length; i++)
            {
                int pageIndex = startPage + i;
                var boton = botonesPagina[i];
                
                if (pageIndex <= endPage && pageIndex < totalPages)
                {
                    boton.Visible = true;
                    boton.Text = (pageIndex + 1).ToString();
                    boton.CommandArgument = pageIndex.ToString();
                    boton.CssClass = pageIndex == currentPageIndex 
                        ? "btn btn-sm btn-primary mx-1" 
                        : "btn btn-sm btn-outline-primary mx-1";
                }
                else
                {
                    boton.Visible = false;
                }
            }
        }

        private void CalcularSubtotal(List<Dominio.FormulacionEF> formulacionesPagina)
        {
            try
            {
                decimal totalPluranual = 0;
                int count = formulacionesPagina?.Count ?? 0;

                if (formulacionesPagina != null)
                {
                    totalPluranual = formulacionesPagina
                        .Sum(f => (f.Monto_26 ?? 0) + (f.Monto_27 ?? 0) + (f.Monto_28 ?? 0));
                }

                lblSubtotalPaginacion.Text = $"Total: {totalPluranual:C} ({count} registros)";
            }
            catch (Exception ex)
            {
                lblSubtotalPaginacion.Text = $"Error en cálculo: {ex.Message}";
            }
        }

        #endregion

        #region Eventos de Paginación

        protected void lnkFirst_Click(object sender, EventArgs e)
        {
            currentPageIndex = 0;
            BindGrid();
        }

        protected void lnkPrev_Click(object sender, EventArgs e)
        {
            if (currentPageIndex > 0)
            {
                currentPageIndex--;
                BindGrid();
            }
        }

        protected void lnkNext_Click(object sender, EventArgs e)
        {
            int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            if (currentPageIndex < totalPages - 1)
            {
                currentPageIndex++;
                BindGrid();
            }
        }

        protected void lnkLast_Click(object sender, EventArgs e)
        {
            int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            currentPageIndex = Math.Max(0, totalPages - 1);
            BindGrid();
        }

        protected void lnkPage_Click(object sender, EventArgs e)
        {
            if (sender is LinkButton btn && int.TryParse(btn.CommandArgument, out int pageIndex))
            {
                currentPageIndex = pageIndex;
                BindGrid();
            }
        }

        protected void ddlPageSizeExternal_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (int.TryParse(ddlPageSizeExternal.SelectedValue, out int newPageSize))
            {
                pageSize = newPageSize;
                currentPageIndex = 0; // Reiniciar a la primera página
                BindGrid();
            }
        }

        #endregion

    }
}