using Dominio;
using Negocio;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebForms.CustomControls;

namespace WebForms
{
    public partial class LegitimosEF : System.Web.UI.Page
    {
        private LegitimoNegocioEF negocio = new LegitimoNegocioEF();
        CalculoRedeterminacionNegocioEF calculoRedeterminacionNegocio = new CalculoRedeterminacionNegocioEF();

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

        protected void btnShowAddModal_Click(object sender, EventArgs e)
        {
            // Seguir patrón de CertificadosEF: limpiar formulario, ajustar título y mostrar modal en cliente
            LimpiarFormulario();

            ScriptManager.RegisterStartupScript(this, this.GetType(), "ResetModalTitleAndShow",
                "$(document).ready(function() { $('#modalAgregar .modal-title').text('Agregar Legítimo'); document.getElementById('" + btnAgregar.ClientID + "').value = 'Agregar'; $('#modalAgregar').modal('show'); });",
                true);

            // Asegurar texto del botón y limpiar id de edición
            btnAgregar.Text = "Agregar";
            ddlObra.Enabled = true; // Permitir seleccionar obra al agregar
            Session["EditingLegitimoEFId"] = null;
        }

        protected void btnAgregar_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
                return;
            try
            {
                var legitimo = new LegitimoEF
                {
                    ObraId = string.IsNullOrEmpty(ddlObra.SelectedValue) ? 0 : int.Parse(ddlObra.SelectedValue),
                    CodigoAutorizante = txtAutorizante.Text,
                    Expediente = txtExpediente.Text,
                    InicioEjecucion = string.IsNullOrWhiteSpace(txtInicioEjecucion.Text) ? (DateTime?)null : DateTime.Parse(txtInicioEjecucion.Text),
                    FinEjecucion = string.IsNullOrWhiteSpace(txtFinEjecucion.Text) ? (DateTime?)null : DateTime.Parse(txtFinEjecucion.Text),
                    Certificado = string.IsNullOrWhiteSpace(txtCertificado.Text) ? (decimal?)null : decimal.Parse(txtCertificado.Text.Replace('.', ',')),
                    MesAprobacion = string.IsNullOrWhiteSpace(txtMesAprobacion.Text) ? (DateTime?)null : DateTime.Parse(txtMesAprobacion.Text),
                };

                // Si hay un id en sesión, es edición
                if (Session["EditingLegitimoEFId"] != null)
                {
                    legitimo.Id = (int)Session["EditingLegitimoEFId"];
                    negocio.Modificar(legitimo);
                    ToastService.Show(this.Page, "Legítimo modificado exitosamente.", ToastService.ToastType.Success);
                }
                else
                {
                    negocio.Agregar(legitimo);
                    ToastService.Show(this.Page, "Legítimo agregado exitosamente.", ToastService.ToastType.Success);
                }

                LimpiarFormulario();
                ScriptManager.RegisterStartupScript(this, this.GetType(), "HideModal", "$('#modalAgregar').modal('hide');", true);
                Session["EditingLegitimoEFId"] = null;
                Session["legitimosCompletos"] = null;
                BindDropDownList();
                BindGrid();
            }
            catch (Exception ex)
            {
                ToastService.Show(this.Page, "Error: " + ex.Message, ToastService.ToastType.Error);
                ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowModalWithError", "$('#modalAgregar').modal('show');", true);
            }
        }

        private void SelectDropDownListByValue(DropDownList dropDown, string value)
        {
            if (dropDown == null) return;
            if (string.IsNullOrWhiteSpace(value)) return;
            var item = dropDown.Items.FindByValue(value);
            if (item != null)
            {
                dropDown.ClearSelection();
                item.Selected = true;
            }
        }

        protected void gridviewRegistros_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            var key = gridviewRegistros.DataKeys[e.RowIndex].Value;
            if (key == null)
            {
                ToastService.Show(this.Page, "Id inválido.", ToastService.ToastType.Error);
                return;
            }
            if (!int.TryParse(key.ToString(), out int id))
            {
                ToastService.Show(this.Page, "Id inválido.", ToastService.ToastType.Error);
                return;
            }

            try
            {
                // Get the legitimo being deleted and its expediente first
                var listaCompleta = Session["legitimosCompletos"] as List<Dominio.LegitimoEF>;
                var legitimoAEliminar = listaCompleta?.FirstOrDefault(l => l.Id == id);
                string expedienteAfectado = legitimoAEliminar?.Expediente;

                // Delete from database
                var ok = negocio.Eliminar(id);

                if (ok)
                {
                    ToastService.Show(this.Page, "Legítimo eliminado exitosamente.", ToastService.ToastType.Success);

                    // Update the in-memory cache if it exists
                    if (listaCompleta != null)
                    {
                        // Remove the deleted item from the in-memory list
                        listaCompleta.RemoveAll(l => l.Id == id);

                        // Update session with modified list
                        Session["legitimosCompletos"] = listaCompleta;

                        // Invalidate filtered data cache to force recalculation
                        Session["FilteredLegitimos"] = null;

                        // Recalculate SIGAF/SADE data for affected expedientes
                        if (!string.IsNullOrWhiteSpace(expedienteAfectado))
                        {
                            List<string> expedientesAfectados = new List<string> { expedienteAfectado };
                            RecalcularYActualizarCache(expedientesAfectados, listaCompleta, false, true);
                        }
                        else
                        {
                            // If no expediente to recalculate, just reload the current page
                            currentPageIndex = 0; // Reset to first page on delete
                            ViewState["CurrentPageIndex"] = currentPageIndex;
                            CargarPaginaActual();
                        }
                    }
                    else
                    {
                        // If no cache exists, invalidate and reload from DB
                        Session["legitimosCompletos"] = null;
                        Session["FilteredLegitimos"] = null;
                        CalculoRedeterminacionNegocioEF.LimpiarCacheSade();

                        currentPageIndex = 0; // Reset to first page on delete
                        ViewState["CurrentPageIndex"] = currentPageIndex;
                        BindGrid();
                    }
                }
                else
                {
                    // If no cache exists, invalidate and reload from DB
                    Session["legitimosCompletos"] = null;
                    Session["FilteredLegitimos"] = null;
                    CalculoRedeterminacionNegocioEF.LimpiarCacheSade();

                    currentPageIndex = 0; // Reset to first page on delete
                    ViewState["CurrentPageIndex"] = currentPageIndex;
                    BindGrid();
                }
            }
            catch (Exception ex)
            {
                ToastService.Show(this.Page, "Error al eliminar: " + ex.Message, ToastService.ToastType.Error);
                BindGrid();
            }
        }

        protected void gridviewRegistros_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                int id = Convert.ToInt32(gridviewRegistros.SelectedDataKey.Value);
                var lista = Session["legitimosCompletos"] as List<Dominio.LegitimoEF>;
                var legitimo = lista?.FirstOrDefault(l => l.Id == id);
                if (legitimo == null)
                {
                    var usuario = UserHelper.GetFullCurrentUser();
                    if (usuario != null)
                    {
                        lista = negocio.ListarPorUsuarioConFiltrosCompleto(usuario, null, new List<int>(), new List<int>(), new List<string>(), new List<string>(), new List<DateTime?>());
                        Session["legitimosCompletos"] = lista;
                        legitimo = lista?.FirstOrDefault(l => l.Id == id);
                    }
                }

                if (legitimo != null)
                {
                    Session["EditingLegitimoEFId"] = legitimo.Id;
                    // Rebind dropdowns to include current obra
                    BindDropDownList();
                    SelectDropDownListByValue(ddlObra, legitimo.ObraId.ToString());
                    txtAutorizante.Text = legitimo.CodigoAutorizante ?? string.Empty;
                    txtExpediente.Text = legitimo.Expediente ?? string.Empty;
                    txtInicioEjecucion.Text = legitimo.InicioEjecucion?.ToString("yyyy-MM-dd") ?? string.Empty;
                    txtFinEjecucion.Text = legitimo.FinEjecucion?.ToString("yyyy-MM-dd") ?? string.Empty;
                    txtCertificado.Text = legitimo.Certificado?.ToString() ?? string.Empty;
                    txtMesAprobacion.Text = legitimo.MesAprobacion?.ToString("yyyy-MM-dd") ?? string.Empty;

                    ScriptManager.RegisterStartupScript(this, this.GetType(), "EditModal", @"
                        $(document).ready(function() {
                            $('#modalAgregar .modal-title').text('Modificar Legítimo');
                            $('#modalAgregar').modal('show');
                        });", true);

                    ddlObra.Enabled = false; // No permitir cambiar obra al editar
                    btnAgregar.Text = "Modificar";
                }
            }
            catch (Exception ex)
            {
                ToastService.Show(this.Page, $"Error al cargar los datos: {ex.Message}", ToastService.ToastType.Error);
            }
        }

        /// <summary>
        /// Cargar la grilla usando el control de paginación
        /// </summary>
        private void BindGrid()
        {
            var usuario = UserHelper.GetFullCurrentUser();
            if (usuario == null) return;

            if (!(Session["legitimosCompletos"] is List<Dominio.LegitimoEF> listaCompleta))
            {
                try
                {
                    listaCompleta = negocio.ListarPorUsuarioConFiltrosCompleto(usuario, null, new List<int>(), new List<int>(), new List<string>(), new List<string>(), new List<DateTime?>());
                }
                catch
                {
                    listaCompleta = new List<Dominio.LegitimoEF>();
                }
                Session["legitimosCompletos"] = listaCompleta;
            }

            // Aplicar filtro de texto general
            string filtro = txtBuscar?.Text?.Trim().ToLower();
            var listaFiltrada = listaCompleta;
            if (!string.IsNullOrEmpty(filtro))
            {
                listaFiltrada = listaFiltrada.Where(l =>
                    (l.Expediente?.ToLower().Contains(filtro) ?? false) ||
                    (l.ObraEF?.Empresa?.Nombre?.ToLower().Contains(filtro) ?? false) ||
                    (l.CodigoAutorizante?.ToLower().Contains(filtro) ?? false) ||
                    (l.ObraEF?.Descripcion?.ToLower().Contains(filtro) ?? false) ||
                    (l.ObraEF?.Area?.Nombre?.ToLower().Contains(filtro) ?? false) ||
                    (l.ObraEF?.Proyecto?.LineaGestionEF?.Nombre?.ToLower().Contains(filtro) ?? false)
                ).ToList();
            }

            // Aplicar filtros de columnas (TreeViewSearch) en memoria
            listaFiltrada = AplicarFiltrosTreeViewEnMemoria(listaFiltrada);

            // Guardar lista filtrada opcionalmente en sesión para reuso
            Session["FilteredLegitimos"] = listaFiltrada;

            totalRecords = listaFiltrada.Count;
            int page = currentPageIndex;
            int size = pageSize;

            if (page * size >= Math.Max(1, totalRecords))
            {
                page = 0;
                this.paginationControl.CurrentPageIndex = 0;
            }

            this.paginationControl.Initialize(totalRecords, page, size);
            this.paginationControl.TotalRecords = totalRecords;
            this.paginationControl.UpdatePaginationControls();

            var items = listaFiltrada.Skip(page * size).Take(size).ToList();

            gridviewRegistros.DataSource = items;
            gridviewRegistros.DataBind();

            // calculando subtotal en memoria
            var subtotalGlobal = listaFiltrada.Sum(l => l.Certificado ?? 0m);
            // Actualizar subtotal en el control de paginación
            if (FindControlRecursive(this, "paginationControl") is PaginationControl paginationControl)
            {
                paginationControl.UpdateSubtotal(subtotalGlobal, totalRecords);
            }

        }

        private List<string> GetSelectedValues(string controlId)
        {
            if (gridviewRegistros.HeaderRow == null) return new List<string>();
            var control = gridviewRegistros.HeaderRow.FindControl(controlId) as TreeViewSearch;
            if (control == null) return new List<string>();
            return control.SelectedValues ?? new List<string>();
        }

        private void BindDropDownList()
        {
            try
            {
                var usuario = UserHelper.GetFullCurrentUser();
                // Cargar obras disponibles; si estamos editando incluir la obra en edición
                int? obraEnEdicion = Session["EditingLegitimoEFId"] as int?;
                int? includeObraId = null;
                if (obraEnEdicion.HasValue)
                {
                    // obtener la legitimo y su obra
                    var lista = Session["legitimosCompletos"] as List<Dominio.LegitimoEF>;
                    var actual = lista?.FirstOrDefault(x => x.Id == obraEnEdicion.Value);
                    if (actual != null) includeObraId = actual.ObraId;
                }

                var obras = new ObraNegocioEF().ListarParaDDL();
                ddlObra.Items.Clear();
                ddlObra.Items.Add(new ListItem("Seleccione una obra", ""));
                foreach (var o in obras)
                {
                    ddlObra.Items.Add(new ListItem(o.Descripcion, o.Id.ToString()));
                }
            }
            catch (Exception ex)
            {
                ToastService.Show(this.Page, "Error al cargar obras: " + ex.Message, ToastService.ToastType.Error);
            }
        }

        /// <summary>
        /// Obtiene los datos completos (posiblemente en caché) y aplica los filtros actuales en memoria.
        /// Útil para exportar o calcular subtotales que requieren la colección completa filtrada.
        /// </summary>
        private List<LegitimoEF> ObtenerDatosFiltradosActuales()
        {
            var lista = Session["legitimosCompletos"] as List<LegitimoEF>;
            if (lista == null)
            {
                var usuario = UserHelper.GetFullCurrentUser();
                if (usuario == null) return new List<LegitimoEF>();
                try
                {
                    lista = negocio.ListarPorUsuarioConFiltrosCompleto(usuario, null, new List<int>(), new List<int>(), new List<string>(), new List<string>(), new List<DateTime?>());

                    // Asegurar que todos los registros tengan un Estado coherente (NO INICIADO por defecto)
                    foreach (var l in lista)
                    {
                        try
                        {
                            // Misma lógica que Certificados: sin expediente => NO INICIADO,
                            // con expediente y sin SIGAF => EN TRAMITE, si SIGAF>0 => DEVENGADO
                            if (string.IsNullOrWhiteSpace(l.Expediente))
                                l.Estado = "NO INICIADO";
                            else if (!l.Sigaf.HasValue || l.Sigaf == 0)
                                l.Estado = "EN TRAMITE";
                            else
                                l.Estado = "DEVENGADO";
                        }
                        catch { }
                    }

                    Session["legitimosCompletos"] = lista;
                }
                catch
                {
                    return new List<Dominio.LegitimoEF>();
                }
            }

            // Filtro de texto general
            string filtro = txtBuscar?.Text?.Trim().ToLower();
            if (!string.IsNullOrEmpty(filtro))
            {
                lista = lista.Where(l =>
                    (l.Expediente?.ToLower().Contains(filtro) ?? false) ||
                    (l.ObraEF?.Empresa?.Nombre?.ToLower().Contains(filtro) ?? false) ||
                    (l.CodigoAutorizante?.ToLower().Contains(filtro) ?? false) ||
                    (l.ObraEF?.Descripcion?.ToLower().Contains(filtro) ?? false) ||
                    (l.ObraEF?.Area?.Nombre?.ToLower().Contains(filtro) ?? false) ||
                    (l.ObraEF?.Proyecto?.LineaGestionEF?.Nombre?.ToLower().Contains(filtro) ?? false)
                ).ToList();
            }

            // Aplicar filtros de columnas (TreeViewSearch)
            lista = AplicarFiltrosTreeViewEnMemoria(lista);

            return lista;
        }

        private List<Dominio.LegitimoEF> AplicarFiltrosTreeViewEnMemoria(List<Dominio.LegitimoEF> data)
        {
            if (gridviewRegistros.HeaderRow == null) return data;

            try
            {
                if (gridviewRegistros.HeaderRow.FindControl("cblsHeaderArea") is TreeViewSearch cblsHeaderArea && cblsHeaderArea.ExpandedSelectedValues.Any())
                {
                    var selected = cblsHeaderArea.ExpandedSelectedValues.Select(s => { int v; return int.TryParse(s, out v) ? (int?)v : null; }).Where(v => v.HasValue).Select(v => v.Value).ToList();
                    if (selected.Any()) data = data.Where(d => d.ObraEF?.Area != null && selected.Contains(d.ObraEF.Area.Id)).ToList();
                }

                if (gridviewRegistros.HeaderRow.FindControl("cblsHeaderEmpresa") is TreeViewSearch cblsHeaderEmpresa && cblsHeaderEmpresa.ExpandedSelectedValues.Any())
                {
                    var selected = cblsHeaderEmpresa.ExpandedSelectedValues.Select(s => { int v; return int.TryParse(s, out v) ? (int?)v : null; }).Where(v => v.HasValue).Select(v => v.Value).ToList();
                    if (selected.Any()) data = data.Where(d => d.ObraEF?.Empresa != null && selected.Contains(d.ObraEF.Empresa.Id)).ToList();
                }

                if (gridviewRegistros.HeaderRow.FindControl("cblsHeaderAutorizante") is TreeViewSearch cblsHeaderAutorizante && cblsHeaderAutorizante.ExpandedSelectedValues.Any())
                {
                    var selected = cblsHeaderAutorizante.ExpandedSelectedValues;
                    if (selected.Any()) data = data.Where(d => !string.IsNullOrEmpty(d.CodigoAutorizante) && selected.Contains(d.CodigoAutorizante)).ToList();
                }

                if (gridviewRegistros.HeaderRow.FindControl("cblsHeaderEstado") is TreeViewSearch cblsHeaderEstado && cblsHeaderEstado.ExpandedSelectedValues.Any())
                {
                    var selected = cblsHeaderEstado.ExpandedSelectedValues;
                    if (selected.Any()) data = data.Where(d => !string.IsNullOrEmpty(d.Estado) && selected.Contains(d.Estado)).ToList();
                }

                if (gridviewRegistros.HeaderRow.FindControl("cblsHeaderMesAprobacion") is TreeViewSearch cblsHeaderMes && cblsHeaderMes.SelectedValues.Any())
                {
                    var selectedDates = cblsHeaderMes.SelectedValues.Select(s => { DateTime dt; return DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt) ? (DateTime?)dt.Date : null; }).Where(d => d.HasValue).Select(d => d.Value).ToList();
                    if (selectedDates.Any()) data = data.Where(d => d.MesAprobacion.HasValue && selectedDates.Contains(d.MesAprobacion.Value.Date)).ToList();
                }

                if (gridviewRegistros.HeaderRow.FindControl("cblsHeaderLineaGestion") is TreeViewSearch cblsHeaderLinea && cblsHeaderLinea.ExpandedSelectedValues.Any())
                {
                    var selected = cblsHeaderLinea.ExpandedSelectedValues.Select(s => { int v; return int.TryParse(s, out v) ? (int?)v : null; }).Where(v => v.HasValue).Select(v => v.Value).ToList();
                    if (selected.Any()) data = data.Where(d => d.ObraEF?.Proyecto?.LineaGestionEF != null && selected.Contains(d.ObraEF.Proyecto.LineaGestionEF.Id)).ToList();
                }
            }
            catch (Exception ex)
            {
                ToastService.Show(this.Page, "Error al aplicar filtros: " + ex.Message, ToastService.ToastType.Error);
            }

            return data;
        }

        private void LimpiarFormulario()
        {
            try
            {
                txtAutorizante.Text = string.Empty;
                txtExpediente.Text = string.Empty;
                txtInicioEjecucion.Text = string.Empty;
                txtFinEjecucion.Text = string.Empty;
                txtCertificado.Text = string.Empty;
                txtMesAprobacion.Text = string.Empty;
                ddlObra.ClearSelection();
                Session["EditingLegitimoEFId"] = null;
            }
            catch (Exception ex)
            {
                ToastService.Show(this.Page, "Error al limpiar formulario: " + ex.Message, ToastService.ToastType.Error);
            }
        }

        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            paginationControl.CurrentPageIndex = 0;
            paginationControl.UpdatePaginationControls();
            BindGrid();
        }

        protected void BtnClearFilters_Click(object sender, EventArgs e)
        {
            // Limpiar filtros de texto y de columnas, reiniciar paginación y forzar recomputo
            txtBuscar.Text = string.Empty;

            // Limpia cualquier TreeViewSearch en la página (método utilitario del control)
            TreeViewSearch.ClearAllFiltersOnPage(this.Page);



            paginationControl.CurrentPageIndex = 0;
            paginationControl.UpdatePaginationControls();


            // Invalidar caché de datos filtrados en sesión si existe
            Session["FilteredLegitimos"] = null;

            BindGrid();
        }

        protected void gridviewRegistros_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gridviewRegistros.PageIndex = e.NewPageIndex;
            currentPageIndex = e.NewPageIndex;
            ViewState["CurrentPageIndex"] = currentPageIndex;

            var listaFiltrada = Session["FilteredLegitimos"] as List<Dominio.LegitimoEF>;
            if (listaFiltrada != null)
            {
                CargarPaginaActual();
            }
            else
            {
                BindGrid();
            }
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

        protected void gridviewRegistros_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.Header)
            {
                // Poblar filtros del header usando la lista completa almacenada en sesión
                var usuarioActual = UserHelper.GetFullCurrentUser();
                var legitimosCompletos = Session["legitimosCompletos"] as List<Dominio.LegitimoEF>;
                if (legitimosCompletos == null)
                {
                    try
                    {
                        legitimosCompletos = negocio.ListarPorUsuarioConFiltrosCompleto(usuarioActual, null, new List<int>(), new List<int>(), new List<string>(), new List<string>(), new List<DateTime?>());
                    }
                    catch { legitimosCompletos = new List<Dominio.LegitimoEF>(); }
                    Session["legitimosCompletos"] = legitimosCompletos;
                }

                if (e.Row.FindControl("cblsHeaderArea") is TreeViewSearch cblsHeaderArea)
                {
                    var areasUnicas = legitimosCompletos
                        .Where(l => l.ObraEF?.Area != null)
                        .Select(l => l.ObraEF.Area)
                        .Distinct()
                        .OrderBy(a => a.Nombre)
                        .Select(a => new { Id = a.Id.ToString(), Nombre = a.Nombre })
                        .ToList();
                    cblsHeaderArea.DataSource = areasUnicas;
                    cblsHeaderArea.DataBind();
                    cblsHeaderArea.AcceptChanges += OnAcceptChanges;
                }

                if (e.Row.FindControl("cblsHeaderAutorizante") is TreeViewSearch cblsHeaderAutorizante)
                {
                    var autorizantesUnicos = legitimosCompletos
                        .Where(l => !string.IsNullOrEmpty(l.CodigoAutorizante))
                        .Select(l => l.CodigoAutorizante)
                        .Distinct()
                        .OrderBy(a => a)
                        .Select(a => new { Id = a, Nombre = a })
                        .ToList();
                    cblsHeaderAutorizante.DataSource = autorizantesUnicos;
                    cblsHeaderAutorizante.DataBind();
                    cblsHeaderAutorizante.AcceptChanges += OnAcceptChanges;
                }

                if (e.Row.FindControl("cblsHeaderEmpresa") is TreeViewSearch cblsHeaderEmpresa)
                {
                    var empresasUnicas = legitimosCompletos
                        .Where(l => l.ObraEF?.Empresa != null)
                        .Select(l => l.ObraEF.Empresa)
                        .Distinct()
                        .OrderBy(emp => emp.Nombre)
                        .Select(emp => new { Id = emp.Id.ToString(), Nombre = emp.Nombre })
                        .ToList();
                    cblsHeaderEmpresa.DataSource = empresasUnicas;
                    cblsHeaderEmpresa.DataBind();
                    cblsHeaderEmpresa.AcceptChanges += OnAcceptChanges;
                }

                if (e.Row.FindControl("cblsHeaderLineaGestion") is TreeViewSearch cblsHeaderLineaGestion)
                {
                    var lineasUnicas = legitimosCompletos
                        .Where(l => l.ObraEF?.Proyecto?.LineaGestionEF != null)
                        .Select(l => l.ObraEF.Proyecto.LineaGestionEF)
                        .Distinct()
                        .OrderBy(lg => lg.Nombre)
                        .Select(lg => new { Id = lg.Id.ToString(), lg.Nombre })
                        .ToList();
                    cblsHeaderLineaGestion.DataSource = lineasUnicas;
                    cblsHeaderLineaGestion.DataBind();
                    cblsHeaderLineaGestion.AcceptChanges += OnAcceptChanges;
                }

                if (e.Row.FindControl("cblsHeaderMesAprobacion") is TreeViewSearch cblsHeaderMes)
                {
                    var meses = legitimosCompletos
                        .Where(l => l.MesAprobacion != null)
                        .Select(l => l.MesAprobacion.Value)
                        .Distinct()
                        .OrderBy(m => m)
                        .ToList();

                    cblsHeaderMes.DataSource = meses;
                    cblsHeaderMes.DataBind();
                    cblsHeaderMes.AcceptChanges += OnAcceptChanges;
                }

                if (e.Row.FindControl("cblsHeaderEstado") is TreeViewSearch cblsHeaderEstado)
                {
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
                    cblsHeaderEstado.AcceptChanges += OnAcceptChanges;
                }
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
                    ToastService.Show(this.Page, "Error: No hay datos en memoria.", ToastService.ToastType.Error);
                    return;
                }

                int indiceReal = (currentPageIndex * pageSize) + rowIndex;
                if (indiceReal < 0 || indiceReal >= datosFiltradosActuales.Count)
                {
                    ToastService.Show(this.Page, "Error: Índice fuera del rango de datos.", ToastService.ToastType.Error);
                    return;
                }

                var legitimo = datosFiltradosActuales[indiceReal];
                string expedienteAnterior = legitimo.Expediente;

                bool resultado = false;

                // Si nuevo expediente vacío, limpiar; si no, actualizar.
                if (legitimo.Id > 0)
                {
                    var entidad = negocio.ObtenerPorId(legitimo.Id);
                    if (entidad != null)
                    {
                        entidad.Expediente = string.IsNullOrWhiteSpace(nuevoExpediente) ? string.Empty : nuevoExpediente;
                        negocio.Modificar(entidad);
                        resultado = true;

                        // sincronizar en la lista en memoria (si existe)
                        var listaCompleta = Session["legitimosCompletos"] as List<Dominio.LegitimoEF>;
                        if (listaCompleta != null)
                        {
                            var enLista = listaCompleta.FirstOrDefault(x => x.Id == entidad.Id);
                            if (enLista != null)
                            {
                                enLista.Expediente = entidad.Expediente;
                                enLista.Sigaf = entidad.Sigaf;
                                enLista.BuzonSade = entidad.BuzonSade;
                                enLista.FechaSade = entidad.FechaSade;

                                // Ajustar Estado coherente
                                if (string.IsNullOrWhiteSpace(enLista.Expediente))
                                {
                                    enLista.Estado = "NO INICIADO";
                                    enLista.Sigaf = null;
                                    enLista.BuzonSade = null;
                                    enLista.FechaSade = null;
                                }
                                else if (!enLista.Sigaf.HasValue || enLista.Sigaf == 0)
                                {
                                    enLista.Estado = "EN TRAMITE";
                                }
                                else
                                {
                                    enLista.Estado = "DEVENGADO";
                                }
                            }
                        }
                    }
                }

                if (resultado)
                {
                    var expedientesAfectados = new List<string>();
                    if (!string.IsNullOrWhiteSpace(expedienteAnterior)) expedientesAfectados.Add(expedienteAnterior);
                    if (!string.IsNullOrWhiteSpace(nuevoExpediente)) expedientesAfectados.Add(nuevoExpediente);

                    // Recalcular datos relacionados (SIGAF/SADE) y actualizar cache
                    RecalcularYActualizarCache(expedientesAfectados, persistirEnBD: false, recargarVista: true);

                    ToastService.Show(this.Page, "Expediente actualizado correctamente.", ToastService.ToastType.Info);
                }
                else
                {
                    ToastService.Show(this.Page, "No se detectaron cambios para guardar.", ToastService.ToastType.Warning);
                }
            }
            catch (Exception ex)
            {
                ToastService.Show(this.Page, "Error al actualizar el expediente: " + ex.Message, ToastService.ToastType.Error);
            }
        }

        /// <summary>
        /// Recalcula SIGAF/SADE para expedientes y actualiza la sesión y vista.
        /// </summary>
        private void RecalcularYActualizarCache(List<string> expedientesAfectados, List<Dominio.LegitimoEF> listaCache = null, bool persistirEnBD = false, bool recargarVista = true)
        {
            if (expedientesAfectados == null) return;

            expedientesAfectados = expedientesAfectados
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Trim())
                .Distinct()
                .ToList();

            if (!expedientesAfectados.Any()) return;

            try
            {
                if (listaCache == null)
                {
                    listaCache = Session["legitimosCompletos"] as List<Dominio.LegitimoEF> ?? ObtenerDatosFiltradosActuales();
                }

                if (listaCache == null) return;

                // Obtener datos SADE y SIGAF para los expedientes modificados
                var datosSade = calculoRedeterminacionNegocio.ObtenerDatosSadeBulk(expedientesAfectados);
                var datosSigaf = calculoRedeterminacionNegocio.ObtenerSigafBulk(expedientesAfectados);

                // Actualizar cada elemento de la cache que tenga expediente en la lista afectada
                foreach (var l in listaCache)
                {
                    if (string.IsNullOrWhiteSpace(l.Expediente)) continue;
                    var exp = l.Expediente.Trim();
                    if (!expedientesAfectados.Contains(exp)) continue;

                    if (datosSade != null && datosSade.TryGetValue(exp, out var sadeInfo))
                    {
                        l.BuzonSade = sadeInfo.Buzon;
                        l.FechaSade = sadeInfo.Fecha;
                    }

                    if (datosSigaf != null && datosSigaf.TryGetValue(exp, out var sigafVal))
                    {
                        l.Sigaf = sigafVal;
                    }

                    if (string.IsNullOrWhiteSpace(l.Expediente))
                        l.Estado = "NO INICIADO";
                    else if (!l.Sigaf.HasValue || l.Sigaf == 0)
                        l.Estado = "EN TRAMITE";
                    else
                        l.Estado = "DEVENGADO";
                }

                // Guardar cambios en sesión e invalidar filtrado
                Session["legitimosCompletos"] = listaCache;
                Session["FilteredLegitimos"] = null;

                if (recargarVista)
                {
                    currentPageIndex = 0;
                    ViewState["CurrentPageIndex"] = currentPageIndex;
                    CargarPaginaActual();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Error al recalcular SIGAF/buzon/fecha SADE para legitimos: " + ex);
            }
            finally
            {
                CalculoRedeterminacionNegocioEF.LimpiarCacheSade();
            }
        }

        public void OnAcceptChanges(object sender, EventArgs e)
        {
            // Reiniciar a la primera página al aplicar filtros
            currentPageIndex = 0;
            ViewState["CurrentPageIndex"] = currentPageIndex;

            // Invalida la caché de filtrado para forzar que se apliquen los nuevos filtros
            Session["FilteredLegitimos"] = null;

            // Recalcular y cargar la primera página
            var datosFiltrados = ObtenerDatosFiltradosActuales();
            Session["FilteredLegitimos"] = datosFiltrados;
            CargarPaginaActual();
        }

        protected void btnExportarExcel_Click(object sender, EventArgs e)
        {
            try
            {
                var todos = (List<LegitimoEF>)Session["legitimosCompletos"];


                var mapeo = new Dictionary<string, string>
                {
                    { "Área", "ObraEF.Area.Nombre" },
                    { "Obra", "ObraEF.Descripcion" },
                    { "Empresa", "Empresa" },
                    { "Línea Gestión", "Linea" },
                    { "Código Autorizante", "CodigoAutorizante" },
                    { "Expediente", "Expediente" },
                    { "Inicio Ejecución", "InicioEjecucion" },
                    { "Fin Ejecución", "FinEjecucion" },
                    { "Certificado", "Certificado" },
                    { "Mes Aprobación", "MesAprobacion" },
                    { "Estado", "Estado" },
                    { "Sigaf", "Sigaf" },
                    { "Buzón SADE", "BuzonSade" },
                    { "Fecha SADE", "FechaSade" },
                };

                ExcelHelper.ExportarDatosGenericos(todos, mapeo, "Legitimos");
                ToastService.Show(this.Page, "Datos exportados exitosamente", ToastService.ToastType.Success);
            }
            catch (Exception ex)
            {
                ToastService.Show(this.Page, "Error al exportar: " + ex.Message, ToastService.ToastType.Error);
            }
        }

        /// <summary>
        /// Reconstituye y hace DataBind de la página actual usando la lista filtrada
        /// </summary>
        private void CargarPaginaActual()
        {
            // Reconstruir la lista filtrada si no existe en sesión (igual que en CertificadosEF)
            var listaFiltrada = Session["FilteredLegitimos"] as List<Dominio.LegitimoEF>;
            if (listaFiltrada == null)
            {
                listaFiltrada = ObtenerDatosFiltradosActuales();
                Session["FilteredLegitimos"] = listaFiltrada;
            }

            int page = currentPageIndex;
            int size = pageSize;

            totalRecords = listaFiltrada.Count;

            if (page * size >= Math.Max(1, totalRecords))
            {
                page = 0;
                currentPageIndex = 0;
                ViewState["CurrentPageIndex"] = 0;
            }

            paginationControl.Initialize(totalRecords, page, size);
            paginationControl.TotalRecords = totalRecords;
            paginationControl.UpdatePaginationControls();

            var items = listaFiltrada.Skip(page * size).Take(size).ToList();

            gridviewRegistros.DataSource = items;
            gridviewRegistros.DataBind();

            var subtotalGlobal = listaFiltrada.Sum(l => l.Certificado ?? 0m);
            if (FindControlRecursive(this, "paginationControl") is PaginationControl pag)
            {
                pag.UpdateSubtotal(subtotalGlobal, totalRecords);
            }
        }

        /// <summary>
        /// Busca un control recursivamente en la jerarquía de controles.
        /// Útil cuando el designer no expone el control directamente.
        /// </summary>
        private Control FindControlRecursive(Control root, string id)
        {
            if (root == null) return null;
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

    }
}