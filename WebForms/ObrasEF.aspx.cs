using Dominio;
using Dominio.DTO;
using Negocio;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebForms
{
    public partial class ObrasEF : Page
    {
        private readonly ObraNegocioEF _negocio = new ObraNegocioEF();

        readonly bool IsPlanningOpen = ABMPlaniNegocio.GetIsPlanningOpen();

        // Paginación externa (control personalizado)
        private int currentPageIndex = 0;
        private int pageSize = 12;
        private int totalRecords = 0;

        private readonly int AreaIdRedet = 16; // Id del Área Redeterminaciones en la BD.

        protected void Page_Load(object sender, EventArgs e)
        {
            // cargar paginación desde ViewState
            currentPageIndex = (int)(ViewState["CurrentPageIndex"] ?? 0);
            pageSize = (int)(ViewState["PageSize"] ?? 12);

            if (!IsPostBack)
            {
                BindDropDownList();
                BindGrid();
            }
        }

        /// <summary>
        /// Reutiliza la lista completa de obras (DTO) en Session["ObrasCompleto"] o la carga nuevamente si es necesario.
        /// </summary>
        private void CargarListaObrasCompleta()
        {
            try
            {
                List<ObraDTO> todasLasObras;

                if (Session["ObrasCompleto"] == null)
                {
                    // Cargar la lista completa de obras o filtrarla por área.
                    if (UserHelper.IsUserAdmin() || UserHelper.IsUserInArea(AreaIdRedet))
                        todasLasObras = _negocio.ListarTodo();
                    else
                        todasLasObras = _negocio.ListarPorArea(UserHelper.GetUserAreaId());

                    Session["ObrasCompleto"] = todasLasObras;
                }
                else
                {
                    // Reutilizar la lista existente
                    todasLasObras = (List<ObraDTO>)Session["ObrasCompleto"];
                }

                totalRecords = todasLasObras?.Count ?? 0;
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cargar obras: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        private void BindGrid()
        {
            try
            {
                // Cargar o reutilizar la lista completa de DTOs en Session["ObrasCompleto"]
                if (Session["ObrasCompleto"] == null)
                    CargarListaObrasCompleta();

                var ObrasCompleto = (List<ObraDTO>)Session["ObrasCompleto"];

                // Aplicar filtro de texto general
                string filtro = txtBuscar.Text.Trim().ToUpper();
                var lista = string.IsNullOrEmpty(filtro) ? ObrasCompleto : ObrasCompleto.Where(o =>
                    (o.Descripcion?.ToUpper().Contains(filtro) ?? false) ||
                    (o.Empresa?.ToUpper().Contains(filtro) ?? false) ||
                    (o.Barrio?.ToUpper().Contains(filtro) ?? false)
                ).ToList();

                // Aplicar filtros de las columnas (TreeView)
                var listaFiltrada = AplicarFiltrosTreeViewEnMemoria(lista);
                Session["FilteredObrasCompleto"] = listaFiltrada;

                // Configurar paginación usando la lista ya filtrada
                int totalFiltrados = listaFiltrada.Count;
                dgvObra.VirtualItemCount = totalFiltrados;
                dgvObra.PageSize = pageSize;
                dgvObra.PageIndex = currentPageIndex;
                dgvObra.DataSource = listaFiltrada.Skip(currentPageIndex * pageSize).Take(pageSize).ToList();
                dgvObra.DataBind();

                // Poblar filtros del header si corresponde
                PoblarFiltrosHeader();

                // Actualizar totalRecords y paginación según los datos filtrados
                totalRecords = totalFiltrados;
                ConfigurarPaginationControl(listaFiltrada);
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error en BindGrid: {ex.Message}" + (ex.InnerException != null ? " - " + ex.InnerException.Message : "");
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        private void CargarPaginaActual()
        {
            // Guarda el estado actual en ViewState
            ViewState["CurrentPageIndex"] = currentPageIndex;
            ViewState["PageSize"] = pageSize;

            var datosFiltrados = ObtenerDatosFiltradosActuales();

            // Rebind de la grilla usando la lista ya filtrada
            totalRecords = datosFiltrados.Count;
            dgvObra.VirtualItemCount = totalRecords;
            dgvObra.PageSize = pageSize;
            dgvObra.PageIndex = currentPageIndex;
            dgvObra.DataSource = datosFiltrados.Skip(currentPageIndex * pageSize).Take(pageSize).ToList();
            dgvObra.DataBind();

            // Asegurar que los filtros del header estén poblados
            PoblarFiltrosHeader();

            // Actualizar paginador y subtotal usando la lista filtrada
            ConfigurarPaginationControl(datosFiltrados);
        }


        private void ConfigurarPaginationControl(List<ObraDTO> datosFiltrados)
        {
            if (FindControlRecursive(this, "paginationControl") is CustomControls.PaginationControl paginationControl)
            {
                paginationControl.TotalRecords = totalRecords;
                paginationControl.CurrentPageIndex = currentPageIndex;
                paginationControl.PageSize = pageSize;
                paginationControl.UpdatePaginationControls();


                CalcularSubtotalParaPaginationControl(datosFiltrados);
            }
        }


        private void CalcularSubtotalParaPaginationControl(List<ObraDTO> datosFiltrados)
        {
            try
            {
                if (FindControlRecursive(this, "paginationControl") is CustomControls.PaginationControl paginationControl)
                {
                    var subtotal = datosFiltrados.Sum(o => o.MontoCertificado).GetValueOrDefault();
                    var cantidad = datosFiltrados.Count;
                    paginationControl.UpdateSubtotal(subtotal, cantidad);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Error CalcularSubtotalParaPaginationControl (ObrasEF): " + ex);
            }
        }

        private Control FindControlRecursive(Control root, string id)
        {
            if (root == null) return null;
            if (root.ID == id) return root;
            foreach (Control child in root.Controls.Cast<Control>())
            {
                var found = FindControlRecursive(child, id);
                if (found != null) return found;
            }
            return null;

        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            // Aplicar la visibilidad de columnas justo antes del render
            try
            {
                panelShowAddButton.Visible = !UserHelper.IsUserInArea(AreaIdRedet);
                // incluir tanto los DataField como los HeaderText para cubrir todos los casos
                SetColumnsVisibilityForRedet(UserHelper.IsUserInArea(AreaIdRedet),
                    // DataField names
                    "AutorizadoNuevo",
                    "MontoCertificado",
                    "Porcentaje",
                    "MontoInicial",
                    "MontoActual",
                    "MontoFaltante",
                    // Header text / display names
                    "Disponible Actual",
                    "Planificacion 2025",
                    "Ejecucion presupuesto 2025",
                    "Monto de Obra inicial",
                    "Monto de Obra actual",
                    "Faltante de Obra",
                    // acciones (template field header)
                    "Acciones");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Error OnPreRender SetColumnsVisibilityForRedet: " + ex);
            }
        }

        private void SetColumnsVisibilityForRedet(bool isRedet, params string[] columnsToHide)
        {
            if (dgvObra == null || dgvObra.Columns == null) return;

            var hideSet = new HashSet<string>(columnsToHide ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);

            // Encontrar columnas cuyo DataField (BoundField) o HeaderText coincida con la lista
            var matches = dgvObra.Columns
                .Cast<DataControlField>()
                .Where(col =>
                {
                    if (col is BoundField bf && !string.IsNullOrWhiteSpace(bf.DataField))
                        return hideSet.Contains(bf.DataField);
                    if (!string.IsNullOrWhiteSpace(col.HeaderText))
                        return hideSet.Contains(col.HeaderText);

                    return false;
                })
                .ToList();

            // Aplicar visibilidad (si isRedet=true -> ocultar)
            matches.ForEach(c => c.Visible = !isRedet);
        }


        // Manejador invocado cuando los TreeViewSearch del header aceptan cambios
        protected void OnAcceptChanges(object sender, EventArgs e)
        {
            currentPageIndex = 0; // Reiniciar a la primera página al aplicar filtros
            ViewState["CurrentPageIndex"] = currentPageIndex;

            // Invalida la caché de filtrado para forzar que se apliquen los nuevos filtros
            Session["FilteredObrasCompleto"] = null;

            CargarPaginaActual(); // Cargar datos filtrados y paginados
        }


        protected void paginationControl_PageChanged(object sender, WebForms.CustomControls.PaginationEventArgs e)
        {
            currentPageIndex = e.PageIndex;
            ViewState["CurrentPageIndex"] = currentPageIndex;
            CargarPaginaActual();
        }

        protected void paginationControl_PageSizeChanged(object sender, WebForms.CustomControls.PaginationEventArgs e)
        {
            pageSize = e.PageSize;
            currentPageIndex = 0;
            ViewState["PageSize"] = pageSize;
            ViewState["CurrentPageIndex"] = currentPageIndex;
            CargarPaginaActual();
        }

        protected void btnExportarExcel_Click(object sender, EventArgs e)
        {
            try
            {
                var obrasCompleto = Session["ObrasCompleto"] as List<ObraDTO>;
                if (obrasCompleto == null)
                {
                    CargarListaObrasCompleta();
                    Session["ObrasCompleto"] = obrasCompleto;
                }

                if (obrasCompleto != null && obrasCompleto.Any())
                {
                    var mapeo = new Dictionary<string, string>
                    {
                         { "Área", "Area" },
                        { "Empresa", "Empresa" },
                        { "Contrata", "Contrata" },
                        { "Barrio", "Barrio" },
                        { "Nombre de Obra", "Descripcion" },
                        { "Línea de Gestión", "LineaGestionNombre" },
                        { "Proyecto", "ProyectoNombre" },
                        { "Disponible Actual", "AutorizadoNuevo" },
                        { "Monto Certificado", "MontoCertificado" },
                        { "Porcentaje", "Porcentaje" },
                        { "Monto Inicial", "MontoInicial" },
                        { "Monto Actual", "MontoActual" },
                        { "Monto Faltante", "MontoFaltante" },
                        { "Fecha Inicio", "FechaInicio" },
                        { "Fecha Fin", "FechaFin" }
                    };

                    ExcelHelper.ExportarDatosGenericos(obrasCompleto, mapeo, "ObrasEF");
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

        protected void btnShowAddModal_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();

            if (!UserHelper.IsUserAdmin())
            {
                ddlArea.Enabled = false;
                ddlArea.SelectedValue = UserHelper.GetFullCurrentUser().AreaId.ToString();
            }
            else
            {
                ddlArea.Enabled = true;
            }


            btnAgregar.Text = "Agregar";
            Session["EditingObraId"] = null;
            ScriptManager.RegisterStartupScript(this, this.GetType(), "ResetModalTitleAndShow", @"
                $(document).ready(function() {
                    $('#modalAgregar .modal-title').text('Agregar Obra');
                    document.getElementById('" + btnAgregar.ClientID + @"').value = 'Agregar';
                    $('#areaContainer').show();
                    $('#modalAgregar').modal('show');
                });", true);
        }

        protected void btnAgregar_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
                // Mostrar errores de validación para ayudar al debugg
                var errorList = Page.Validators.Cast<System.Web.UI.IValidator>().Where(v => !v.IsValid).Select(v => v.ErrorMessage).ToList();
                lblMensaje.Text = "Errores de validación: " + string.Join("; ", errorList);
                lblMensaje.CssClass = "alert alert-danger";
                return;
            }

            int? editingId = Session["EditingObraId"] as int?;

            // Crear o cargar la obra según si es edición o nueva
            ObraEF obra = editingId != null ? _negocio.ObtenerPorId(editingId.GetValueOrDefault()) : new ObraEF();

            int.TryParse(txtNumero.Text.Trim(), out int numero);
            int.TryParse(txtAnio.Text.Trim(), out int anio);

            obra.Descripcion = txtDescripcion.Text.Trim();
            obra.Numero = numero;
            obra.Anio = anio;

            if (ddlArea.SelectedIndex > 0 && int.TryParse(ddlArea.SelectedValue, out int areaId))
            {
                obra.AreaId = areaId;
            }

            obra.EmpresaId = int.Parse(ddlEmpresa.SelectedValue);
            obra.ContrataId = int.Parse(ddlContrata.SelectedValue);
            obra.BarrioId = int.Parse(ddlBarrio.SelectedValue);

            obra.Etapa = int.Parse(txtEtapa.Text.Trim());
            obra.ObraNumero = int.Parse(txtObraNumero.Text.Trim());


            if (editingId != null)
            {

                if (_negocio.Modificar(obra))
                {
                    lblMensaje.Text = "Obra modificada exitosamente!";
                    lblMensaje.CssClass = "alert alert-success";
                    Session["EditingObraId"] = null;
                }
                else
                {
                    lblMensaje.Text = "Hubo un problema al modificar la obra.";
                    lblMensaje.CssClass = "alert alert-danger";
                }

            }
            else
            {
                // Nueva obra
                if (_negocio.Agregar(obra))
                {
                    lblMensaje.Text = "Obra agregada exitosamente!";
                    lblMensaje.CssClass = "alert alert-success";
                }
                else
                {
                    lblMensaje.Text = "Hubo un problema al agregar la obra.";
                    lblMensaje.CssClass = "alert alert-danger";
                }

            }



            LimpiarFormulario();
            ScriptManager.RegisterStartupScript(this, this.GetType(), "HideModal", "$('#modalAgregar').modal('hide');", true);

            // Forzar recarga de la lista completa
            Session["ObrasCompleto"] = null;
            CargarListaObrasCompleta();
            CargarPaginaActual();


        }

        private void LimpiarFormulario()
        {
            txtNumero.Text = string.Empty;
            txtAnio.Text = string.Empty;
            txtEtapa.Text = string.Empty;
            txtObraNumero.Text = string.Empty;
            txtDescripcion.Text = string.Empty;
            ddlEmpresa.SelectedIndex = 0;
            ddlContrata.SelectedIndex = 0;
            ddlBarrio.SelectedIndex = 0;
            ddlArea.SelectedIndex = 0;
        }

        protected void dgvObra_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                int rowIndex = dgvObra.SelectedIndex;

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

                ObraDTO obraSeleccionada = datosFiltradosActuales[indiceReal];

                // Buscar el obra real en BD para edición
                ObraEF obra;
                using (var context = new IVCdbContext())
                {
                    obra = context.Obras.Find(obraSeleccionada.Id);
                }

                if (obra != null)
                {
                    // Guardar id para edición
                    Session["EditingObraId"] = obraSeleccionada.Id;

                    // Poblar modal con los campos que realmente existen en el markup de ObrasEF.aspx
                    txtNumero.Text = obraSeleccionada.Numero?.ToString() ?? string.Empty;
                    txtAnio.Text = obraSeleccionada.Anio?.ToString() ?? string.Empty;
                    txtEtapa.Text = obraSeleccionada.Etapa.ToString();
                    txtObraNumero.Text = obraSeleccionada.ObraNumero.ToString();
                    txtDescripcion.Text = obraSeleccionada.Descripcion ?? string.Empty;

                    // Seleccionar valores en los dropdowns si existen
                    if (obraSeleccionada.EmpresaId.HasValue) SelectDropDownListByValue(ddlEmpresa, obraSeleccionada.EmpresaId.Value.ToString()); else ddlEmpresa.ClearSelection();
                    if (obraSeleccionada.ContrataId.HasValue) SelectDropDownListByValue(ddlContrata, obraSeleccionada.ContrataId.Value.ToString()); else ddlContrata.ClearSelection();
                    if (obraSeleccionada.BarrioId.HasValue) SelectDropDownListByValue(ddlBarrio, obraSeleccionada.BarrioId.Value.ToString()); else ddlBarrio.ClearSelection();

                    // Area puede venir del DTO
                    if (obraSeleccionada.AreaId.HasValue)
                    {
                        SelectDropDownListByValue(ddlArea, obraSeleccionada.AreaId.Value.ToString());
                        ViewState["EditingAreaIdEF"] = obraSeleccionada.AreaId.Value;
                        ViewState["EditingAreaNombreEF"] = obraSeleccionada.Area;
                    }

                    // Actualizar el texto del botón
                    btnAgregar.Text = "Actualizar";

                    ddlArea.Enabled = false; // No permitir cambiar el área al editar

                    ScriptManager.RegisterStartupScript(this, this.GetType(), "UpdateModalAndShow", @"
                                $(document).ready(function() {
                                    // Cambiar título y texto del botón
                                    $('#modalAgregar .modal-title').text('Modificar Obra');
                                    document.getElementById('" + btnAgregar.ClientID + @"').value = 'Actualizar';
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
            if (dropDown == null) return;
            dropDown.ClearSelection();
            ListItem item = dropDown.Items.FindByValue(value);
            if (item != null) item.Selected = true;
        }

        protected void dgvObra_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            try
            {
                var id = Convert.ToInt32(dgvObra.DataKeys[e.RowIndex].Value);
                if (_negocio.Eliminar(id))
                {
                    lblMensaje.Text = "Obra eliminada exitosamente";
                    lblMensaje.CssClass = "alert alert-success";
                }
                else
                {
                    lblMensaje.Text = "No fue posible eliminar la obra";
                    lblMensaje.CssClass = "alert alert-danger";
                }

                // Forzar recarga de la lista completa
                Session["ObrasCompleto"] = null;
                CargarListaObrasCompleta();
                CargarPaginaActual();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = "Error: " + ex.Message;
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        protected void dgvObra_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                if (e.Row.FindControl("btnEliminar") is LinkButton btnEliminar) btnEliminar.Visible = UserHelper.IsUserAdmin() || IsPlanningOpen;
            }
        }

        private void PoblarFiltrosHeader()
        {
            if (dgvObra.HeaderRow == null) return;

            Action<string, object, string, string> bindFilter = (controlId, dataSource, textField, valueField) =>
            {
                if (dgvObra.HeaderRow.FindControl(controlId) is CustomControls.TreeViewSearch control)
                {
                    control.DataSource = dataSource;
                    control.DataTextField = textField;
                    control.DataValueField = valueField;
                    control.DataBind();
                }
            };

            List<ObraDTO> obrasCompleto = (List<ObraDTO>)Session["ObrasCompleto"];

            bindFilter("cblsHeaderArea", _negocio.ListarAreas().Select(a => new { a.Id, a.Nombre }).ToList(), "Nombre", "Id");
            bindFilter("cblsHeaderEmpresa", _negocio.ListarEmpresas().Select(e => new { e.Id, e.Nombre }).ToList(), "Nombre", "Id");
            // bindFilter("cblsHeaderBarrio", Negocio.ListarBarrios().Select(b => new { b.Id, b.Nombre }).ToList(), "Nombre", "Id");
            var barriosDesdeObras = obrasCompleto
                    .Select(o => new
                    {
                        Id = o.BarrioId ?? 0, // usar 0 para los vacíos
                        Nombre = string.IsNullOrWhiteSpace(o.Barrio) ? string.Empty : o.Barrio
                    })
                    .GroupBy(b => new { b.Id, b.Nombre })
                    .Select(g => new { g.Key.Id, g.Key.Nombre })
                    .OrderBy(b => b.Nombre)
                    .ToList();

            bindFilter("cblsHeaderBarrio", barriosDesdeObras, "Nombre", "Id");

        }

        private void BindDropDownList()
        {
            try
            {
                using (var ctx = new IVCdbContext())
                {
                    var empresas = ctx.Empresas.AsNoTracking().OrderBy(e => e.Nombre).Select(e => new { e.Id, e.Nombre }).ToList();
                    ddlEmpresa.DataSource = empresas;
                    ddlEmpresa.DataTextField = "Nombre";
                    ddlEmpresa.DataValueField = "Id";
                    ddlEmpresa.DataBind();
                }

                using (var ctx = new IVCdbContext())
                {
                    var areas = ctx.Areas.AsNoTracking().OrderBy(a => a.Nombre).Select(a => new { a.Id, a.Nombre }).ToList();
                    ddlArea.DataSource = areas;
                    ddlArea.DataTextField = "Nombre";
                    ddlArea.DataValueField = "Id";
                    ddlArea.DataBind();
                }

                using (var ctx = new IVCdbContext())
                {
                    var contratas = ctx.Contratas.AsNoTracking().OrderBy(c => c.Nombre).Select(c => new { c.Id, c.Nombre }).ToList();
                    ddlContrata.DataSource = contratas;
                    ddlContrata.DataTextField = "Nombre";
                    ddlContrata.DataValueField = "Id";
                    ddlContrata.DataBind();
                }

                using (var ctx = new IVCdbContext())
                {
                    var barrios = ctx.Barrios.AsNoTracking().OrderBy(b => b.Nombre).Select(b => new { b.Id, b.Nombre }).ToList();
                    ddlBarrio.DataSource = barrios;
                    ddlBarrio.DataTextField = "Nombre";
                    ddlBarrio.DataValueField = "Id";
                    ddlBarrio.DataBind();
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cargar listas: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }


        private List<ObraDTO> ObtenerDatosFiltradosActuales()
        {
            if (Session["ObrasCompleto"] == null)
                CargarListaObrasCompleta();

            List<ObraDTO> obrasCompleto = (List<ObraDTO>)Session["ObrasCompleto"];

            // Aplicar filtro de texto
            string filtro = txtBuscar.Text.Trim().ToUpper();

            if (!string.IsNullOrEmpty(filtro))
            {
                obrasCompleto = obrasCompleto.Where(o =>
                    (o.Area?.ToUpper().Contains(filtro) ?? false) ||
                    (o.Empresa?.ToUpper().Contains(filtro) ?? false) ||
                    (o.Contrata?.ToUpper().Contains(filtro) ?? false) ||
                    (o.Barrio?.ToUpper().Contains(filtro) ?? false) ||
                    (o.Descripcion?.ToUpper().Contains(filtro) ?? false) ||
                    (o.LineaGestionNombre?.ToUpper().Contains(filtro) ?? false) ||
                    (o.ProyectoNombre?.ToUpper().Contains(filtro) ?? false) ||
                    (o.Numero?.ToString().Contains(filtro) ?? false) ||
                    (o.Anio?.ToString().Contains(filtro) ?? false)
                ).ToList();
            }

            // Aplicar filtros de columnas
            var obrasFiltrada = AplicarFiltrosTreeViewEnMemoria(obrasCompleto);

            Session["FilteredObrasCompleto"] = obrasFiltrada;
            return obrasFiltrada;
        }


        private List<ObraDTO> AplicarFiltrosTreeViewEnMemoria(List<ObraDTO> data)
        {
            try
            {
                // Busca el control por id en toda la página (no depender del HeaderRow)
                Func<string, CustomControls.TreeViewSearch> find = id => FindControlRecursive(this, id) as WebForms.CustomControls.TreeViewSearch;

                // Función auxiliar para parsear IDs de forma segura.
                Func<WebForms.CustomControls.TreeViewSearch, List<int>> getSelectedIds = (tv) =>
                    tv?.ExpandedSelectedValues
                       .Select(s => int.TryParse(s, out int id) ? id : -1)
                       .Where(id => id != -1)
                       .ToList() ?? new List<int>();

                var cblsHeaderArea = find("cblsHeaderArea");
                var cblsHeaderEmpresa = find("cblsHeaderEmpresa");
                var cblsHeaderBarrio = find("cblsHeaderBarrio");

                var selectedAreaIds = cblsHeaderArea != null && cblsHeaderArea.ExpandedSelectedValues.Any() ? getSelectedIds(cblsHeaderArea) : null;
                var selectedEmpresaIds = cblsHeaderEmpresa != null && cblsHeaderEmpresa.ExpandedSelectedValues.Any() ? getSelectedIds(cblsHeaderEmpresa) : null;
                var selectedBarrioIds = cblsHeaderBarrio != null && cblsHeaderBarrio.ExpandedSelectedValues.Any() ? getSelectedIds(cblsHeaderBarrio) : null;

                if (selectedAreaIds != null) data = data.Where(c => c.AreaId.HasValue && selectedAreaIds.Contains(c.AreaId.Value)).ToList();
                if (selectedEmpresaIds != null) data = data.Where(c => c.EmpresaId.HasValue && selectedEmpresaIds.Contains(c.EmpresaId.Value)).ToList();
                if (selectedBarrioIds != null) data = data.Where(c => c.BarrioId.HasValue && selectedBarrioIds.Contains(c.BarrioId.Value)).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Error al aplicar filtros de cabecera (ObrasEF): " + ex);
            }

            return data;
        }






        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            currentPageIndex = 0; // Reiniciar a la primera página al aplicar filtros

            // Forzar recomputo de filtros
            Session["FilteredObrasCompleto"] = null;

            CargarPaginaActual();
        }

        protected void BtnClearFilters_Click(object sender, EventArgs e)
        {
            txtBuscar.Text = string.Empty;
            WebForms.CustomControls.TreeViewSearch.ClearAllFiltersOnPage(this.Page);
            currentPageIndex = 0;

            // Forzar recomputo de filtros (ahora sin filtros)
            Session["FilteredObrasCompleto"] = null;

            CargarPaginaActual();
        }

        protected void dgvObra_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            try
            {
                // Update current page index and rebind using our pagination helper
                currentPageIndex = e.NewPageIndex;
                ViewState["CurrentPageIndex"] = currentPageIndex;
                CargarPaginaActual();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cambiar de página: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }




    }
}