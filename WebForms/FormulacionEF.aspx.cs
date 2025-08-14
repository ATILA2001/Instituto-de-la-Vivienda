using Dominio;
using Negocio;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebForms.CustomControls;

namespace WebForms
{
    public partial class FormulacionEF : System.Web.UI.Page
    {
        private FormulacionNegocioEF negocio = new FormulacionNegocioEF();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindDropDownList();
                BindGrid();
            }
        }

        protected void btnExportarExcel_Click(object sender, EventArgs e)
        {
            try
            {
                var lista = Session["formulacionesCompletas"] as List<Dominio.FormulacionEF>;
                if (lista == null)
                {
                    BindGrid();
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
            // Reiniciar a la primera página al aplicar filtros

            paginationControl.CurrentPageIndex = 0;
            paginationControl.UpdatePaginationControls();

            BindGrid(); // Cargar datos filtrados y paginados
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
                    Monto_26 = string.IsNullOrWhiteSpace(txtMonto26.Text) ? (decimal?)null : decimal.Parse(txtMonto26.Text.Replace('.', ',')),
                    Monto_27 = string.IsNullOrWhiteSpace(txtMonto27.Text) ? (decimal?)null : decimal.Parse(txtMonto27.Text.Replace('.', ',')),
                    Monto_28 = string.IsNullOrWhiteSpace(txtMonto28.Text) ? (decimal?)null : decimal.Parse(txtMonto28.Text.Replace('.', ',')),
                    MesBase = string.IsNullOrWhiteSpace(txtMesBase.Text) ? (DateTime?)null : DateTime.Parse(txtMesBase.Text),
                    Observaciones = txtObservaciones.Text,
                    UnidadMedidaId = string.IsNullOrEmpty(ddlUnidadMedida.SelectedValue) ? (int?)null : int.Parse(ddlUnidadMedida.SelectedValue),
                    ValorMedida = string.IsNullOrWhiteSpace(txtValorMedida.Text) ? (decimal?)null : decimal.Parse(txtValorMedida.Text.Replace('.', ',')),
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
                Session["formulacionesCompletas"] = null; // clear cache so selection sees fresh data
                BindDropDownList(); // refresh ddlObra options
                BindGrid();
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
                if (formulacion == null)
                {
                    var usuario = UserHelper.GetFullCurrentUser();
                    if (usuario != null)
                    {
                        lista = negocio.ListarPorUsuario(usuario);
                        Session["formulacionesCompletas"] = lista;
                        formulacion = lista?.FirstOrDefault(f => f.Id == id);
                    }
                }
                if (formulacion != null)
                {
                    // Re-bind dropdown to include the current obra in edit mode
                    Session["EditingFormulacionEFId"] = formulacion.Id;
                    BindDropDownList();
                    SelectDropDownListByValue(ddlObra, formulacion.ObraId.ToString());
                    txtMonto26.Text = formulacion.Monto_26?.ToString() ?? "";
                    txtMonto27.Text = formulacion.Monto_27?.ToString() ?? "";
                    txtMonto28.Text = formulacion.Monto_28?.ToString() ?? "";
                    txtMesBase.Text = formulacion.MesBase?.ToString("yyyy-MM-dd") ?? "";
                    SelectDropDownListByValue(ddlUnidadMedida, formulacion.UnidadMedidaId?.ToString());
                    txtValorMedida.Text = formulacion.ValorMedida?.ToString() ?? "";
                    txtObservaciones.Text = formulacion.Observaciones ?? "";
                    SelectDropDownListByValue(ddlPrioridades, formulacion.PrioridadId?.ToString());

                    // Mantener el Id en sesión hasta guardar/cancelar

                    ScriptManager.RegisterStartupScript(this, this.GetType(), "EditModal", @"
                        $(document).ready(function() {
                            $('#modalAgregar .modal-title').text('Modificar Formulación');
                            $('.col-12:first').hide();
                            $('#modalAgregar').modal('show');
                        });", true);

                    btnAgregar.Text = "Modificar";
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cargar los datos: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        private void SelectDropDownListByValue(DropDownList dropDown, string value)
        {
            if (dropDown == null) return;
            dropDown.ClearSelection();
            if (string.IsNullOrWhiteSpace(value)) return;
            var item = dropDown.Items.FindByValue(value);
            if (item != null)
            {
                item.Selected = true;
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
                    BindDropDownList();
                    BindGrid();
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

        /// <summary>
        /// Método simplificado para cargar la grilla usando el control de paginación
        /// </summary>
        private void BindGrid()
        {
            try
            {
                var usuario = UserHelper.GetFullCurrentUser();
                if (usuario == null) return;

                // Filtro texto
                string filtroGeneral = txtBuscar?.Text?.Trim();

                // Filtros discretos desde los TreeViewSearch
                var selAreas = GetSelectedValues("cblsHeaderArea").Select(int.Parse).ToList();
                var selLineas = GetSelectedValues("cblsHeaderLineaGestion").Select(int.Parse).ToList();
                var selProyectos = GetSelectedValues("cblsHeaderProyecto").Select(int.Parse).ToList();
                var selMontos26 = GetSelectedValues("cblsHeaderMonto2026").Select(s => decimal.Parse(s, CultureInfo.InvariantCulture)).ToList();
                var selPrioridades = GetSelectedValues("cblsHeaderPrioridad").Select(int.Parse).ToList();

                int pageIndex = paginationControl.CurrentPageIndex;
                int pageSize = paginationControl.PageSize;

                // Total filtrado en BD
                int total = negocio.ContarPorUsuarioConFiltros(usuario, filtroGeneral, selAreas, selLineas, selProyectos, selMontos26, selPrioridades);

                // Ajustar paginación si necesario
                if (pageIndex * pageSize >= Math.Max(1, total))
                {
                    pageIndex = 0;
                    paginationControl.CurrentPageIndex = 0;
                }
                paginationControl.TotalRecords = total;
                paginationControl.UpdatePaginationControls();

                // Página actual desde BD
                var pagina = negocio.ListarPorUsuarioPaginadoConFiltros(usuario, pageIndex, pageSize, filtroGeneral, selAreas, selLineas, selProyectos, selMontos26, selPrioridades);

                dgvFormulacion.DataSource = pagina;
                dgvFormulacion.DataBind();

                // Calcular subtotal global (requiere suma total filtrada) -> eficiencia: si se necesita exacto hacer una consulta específica
                decimal totalMonto26Global = 0;
                if (total > 0)
                {
                    // Consulta rápida para subtotal (se podría optimizar combinando con el conteo si se usa proyección)
                    // Para simplicidad se reusa BuildBaseQuery y filtros: ideal mover a un método en negocio si se usa frecuentemente
                    using (var context = new IVCdbContext())
                    {
                        var querySubtotal = context.Formulaciones.AsQueryable();
                        // Reaplicar filtros mínimos (duplicado ligero respecto a negocio; factorizable)
                        if (!usuario.Tipo)
                        {
                            if (usuario.AreaId.HasValue)
                                querySubtotal = querySubtotal.Where(f => f.ObraEF.AreaId == usuario.AreaId);
                            else if (usuario.Area != null)
                                querySubtotal = querySubtotal.Where(f => f.ObraEF.AreaId == usuario.Area.Id);
                        }
                        if (!string.IsNullOrWhiteSpace(filtroGeneral))
                        {
                            querySubtotal = querySubtotal.Where(f =>
                                f.ObraEF.Descripcion.Contains(filtroGeneral) ||
                                f.ObraEF.Empresa.Nombre.Contains(filtroGeneral) ||
                                f.ObraEF.Contrata.Nombre.Contains(filtroGeneral) ||
                                f.ObraEF.Barrio.Nombre.Contains(filtroGeneral) ||
                                (f.Observaciones != null && f.Observaciones.Contains(filtroGeneral)) ||
                                (f.PrioridadEF != null && f.PrioridadEF.Nombre.Contains(filtroGeneral)) ||
                                (f.ObraEF.Proyecto != null && f.ObraEF.Proyecto.Nombre.Contains(filtroGeneral)) ||
                                (f.ObraEF.Proyecto.LineaGestionEF != null && f.ObraEF.Proyecto.LineaGestionEF.Nombre.Contains(filtroGeneral))
                            );
                        }
                        if (selAreas.Any()) querySubtotal = querySubtotal.Where(f => f.ObraEF.Area != null && selAreas.Contains(f.ObraEF.Area.Id));
                        if (selLineas.Any()) querySubtotal = querySubtotal.Where(f => f.ObraEF.Proyecto.LineaGestionEF != null && selLineas.Contains(f.ObraEF.Proyecto.LineaGestionEF.Id));
                        if (selProyectos.Any()) querySubtotal = querySubtotal.Where(f => f.ObraEF.Proyecto != null && selProyectos.Contains(f.ObraEF.Proyecto.Id));
                        if (selMontos26.Any()) querySubtotal = querySubtotal.Where(f => f.Monto_26.HasValue && selMontos26.Contains(f.Monto_26.Value));
                        if (selPrioridades.Any()) querySubtotal = querySubtotal.Where(f => f.PrioridadEF != null && selPrioridades.Contains(f.PrioridadEF.Id));

                        totalMonto26Global = querySubtotal.Sum(f => (decimal?)f.Monto_26) ?? 0m;
                    }
                }

                paginationControl.SubtotalText = $"Total Monto 2026: {totalMonto26Global:C} ({total} registros)";
                CalcularSubtotal(pagina);

            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cargar la grilla: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        /// <summary>
        /// Obtiene los valores seleccionados de un TreeViewSearch en la cabecera del GridView.
        /// Devuelve lista vacía si no existe o aún no se ha renderizado.
        /// </summary>
        private List<string> GetSelectedValues(string controlId)
        {
            if (dgvFormulacion.HeaderRow == null) return new List<string>();
            var ctl = dgvFormulacion.HeaderRow.FindControl(controlId) as TreeViewSearch;
            return ctl?.SelectedValues ?? new List<string>();
        }

        private void BindDropDownList()
        {
            try
            {
                // ddlObra: listar solo obras que no están en Formulación (y filtrar por área si corresponde)
                UsuarioEF usuario = UserHelper.GetFullCurrentUser();
                int? obraEnEdicion = null;
                if (Session["EditingFormulacionEFId"] is int editId)
                {
                    // obtener la obra asociada a la formulación en edición para incluirla en el combo
                    using (var ctx = new IVCdbContext())
                    {
                        var form = ctx.Formulaciones.AsNoTracking().FirstOrDefault(f => f.Id == editId);
                        obraEnEdicion = form?.ObraId;
                    }
                }
                ddlObra.DataSource = new ObraNegocioEF().ListarParaDDLNoEnFormulacion(obraEnEdicion, usuario);
                ddlObra.DataTextField = "Descripcion";
                ddlObra.DataValueField = "Id";
                ddlObra.DataBind();

                ddlUnidadMedida.DataSource = new UnidadMedidaNegocioEF().Listar();
                ddlUnidadMedida.DataTextField = "Nombre";
                ddlUnidadMedida.DataValueField = "Id";
                ddlUnidadMedida.DataBind();

                ddlPrioridades.DataSource = new PrioridadNegocioEF().Listar();
                ddlPrioridades.DataTextField = "Nombre";
                ddlPrioridades.DataValueField = "Id";
                ddlPrioridades.DataBind();
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
                // Recalcular total y reinicializar paginación
                var usuario = UserHelper.GetFullCurrentUser();
                if (usuario != null)
                {
                    string filtroGeneral = txtBuscar?.Text?.Trim();
                    int totalRecords = negocio.ContarPorUsuario(usuario, filtroGeneral);
                    paginationControl.Initialize(totalRecords, 0, paginationControl.PageSize);
                    BindGrid();
                }
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
            BindGrid(); // Recargar completamente
        }

        protected void dgvFormulacion_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            try
            {
                dgvFormulacion.PageIndex = e.NewPageIndex;
                BindGrid();
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

        /// <summary>
        /// Calcula subtotales para la página actual y actualiza el control
        /// </summary>
        private void CalcularSubtotal(List<Dominio.FormulacionEF> formulacionesPagina)
        {

            decimal totalMonto26 = 0;
            int count = formulacionesPagina?.Count ?? 0;

            if (formulacionesPagina != null)
            {
                totalMonto26 = formulacionesPagina
                    .Sum(f => f.Monto_26 ?? 0);
            }

            var paginationInfo = paginationControl.GetPaginationInfo();
            paginationControl.SubtotalText = $"Total Monto 2026: {totalMonto26:C} ({paginationInfo.TotalRecords} registros)";

        }

        #region Eventos del Control de Paginación

        protected void paginationControl_PageChanged(object sender, PaginationEventArgs e)
        {
            // El control maneja automáticamente el cambio de página
            paginationControl.UpdatePaginationControls();
            BindGrid();
        }

        protected void paginationControl_PageSizeChanged(object sender, PaginationEventArgs e)
        {
            // Recalcular total cuando cambia el tamaño de página
            var usuario = UserHelper.GetFullCurrentUser();
            if (usuario != null)
            {
                string filtroGeneral = txtBuscar?.Text?.Trim();
                int totalRecords = negocio.ContarPorUsuario(usuario, filtroGeneral);
                paginationControl.Initialize(totalRecords, 0, e.PageSize);
                BindGrid();
            }
        }

        #endregion

    }
}