using Dominio;
using Negocio;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebForms.CustomControls;

namespace WebForms
{
    public partial class FormulacionEF : System.Web.UI.Page
    {
        private readonly FormulacionNegocioEF _negocio = new FormulacionNegocioEF();

        // Años del ciclo de formulación vigente (base, base+1, base+2).
        // Se usan en las cabeceras de la grilla y en las etiquetas del modal.
        protected int AnioCiclo1 => FormulacionCiclo.Anios[0];
        protected int AnioCiclo2 => FormulacionCiclo.Anios[1];
        protected int AnioCiclo3 => FormulacionCiclo.Anios[2];
        protected int BreveDescripcionMaxLength => Dominio.FormulacionEF.BreveDescripcionMaxLength;
        protected int ObservacionesMaxLength => Dominio.FormulacionEF.ObservacionesMaxLength;

        protected void Page_Load(object sender, EventArgs e)
        {
            ConfigurarLimitesTexto();

            if (!IsPostBack)
            {
                BindDropDownList();
                BindGrid();
            }
        }

        private void ConfigurarLimitesTexto()
        {
            txtBreveDescripcion.Attributes["maxlength"] = BreveDescripcionMaxLength.ToString(CultureInfo.InvariantCulture);
            txtBreveDescripcion.Attributes["oninput"] =
                $"actualizarContadorCaracteres(this, 'contadorBreveDescripcion', {BreveDescripcionMaxLength})";
            txtObservaciones.Attributes["maxlength"] = ObservacionesMaxLength.ToString(CultureInfo.InvariantCulture);
            txtObservaciones.Attributes["oninput"] =
                $"actualizarContadorCaracteres(this, 'contadorObservaciones', {ObservacionesMaxLength})";
        }

        protected void btnExportarExcel_Click(object sender, EventArgs e)
        {
            try
            {
                var usuario = UserHelper.GetFullCurrentUser();
                var lista = usuario != null ? _negocio.ListarPivotPorUsuario(usuario) : null;

                if (lista != null && lista.Any())
                {
                    var anios = FormulacionCiclo.Anios;
                    // Mismas columnas que la grilla: 1 fila por obra con 3 montos del ciclo.
                    var mapeoColumnas = new Dictionary<string, string>
                    {
                        { "Área", "ObraEF.Area.Nombre" },
                        { "Empresa", "ObraEF.Empresa.Nombre" },
                        { "Contrata", "ObraEF.Contrata.Nombre" },
                        { "Barrio", "ObraEF.Barrio.Nombre" },
                        { "Línea de Gestión", "ObraEF.Proyecto.LineaGestionEF.Nombre" },
                        { "Proyecto", "ObraEF.Proyecto.Nombre" },
                        { "Nombre de Obra", "ObraEF.Descripcion" },
                        { "Breve Descripción", "BreveDescripcion" },
                        { "Fecha Inicio", "FechaInicio" },
                        { "Fecha Fin", "FechaFin" },
                        { "PPI", "Ppi" },
                        { "Techo", "Techos" },
                        { $"Monto {anios[0]}", "MontoAnio1" },
                        { $"Monto {anios[1]}", "MontoAnio2" },
                        { $"Monto {anios[2]}", "MontoAnio3" },
                        { "Mes Base", "MesBase" },
                        { "Unidad de Medida", "UnidadMedidaEF.Nombre" },
                        { "Valor de Medida", "ValorMedida" },
                        { "Observaciones", "Observaciones" },
                        { "Prioridad", "PrioridadEF.Nombre" }
                    };
                    ExcelHelper.ExportarDatosGenericos(lista, mapeoColumnas, "FormulacionesEF");
                    ToastService.Show(this.Page, "Datos exportados exitosamente", ToastService.ToastType.Success);
                }
                else
                {
                    ToastService.Show(this.Page, "No hay datos para exportar", ToastService.ToastType.Warning);
                }
            }
            catch (Exception)
            {
                ToastService.Show(this.Page, "Error al exportar los datos. Intente nuevamente.", ToastService.ToastType.Error);
            }
        }

        protected void OnAcceptChanges(object sender, EventArgs e)
        {
            // Reiniciar a la primera página al aplicar filtros

            paginationControl.CurrentPageIndex =0;
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
                    formulacionesCompletas = _negocio.ListarPorUsuario(usuarioActual);
                    Session["formulacionesCompletas"] = formulacionesCompletas;
                }

                if (dgvFormulacion.HeaderRow.FindControl("cblsHeaderArea") is TreeViewSearch cblsHeaderArea)
                {
                    var areasUnicas = formulacionesCompletas
                        .Where(f => f.ObraEF?.Area != null)
                        .Select(f => f.ObraEF.Area)
                        .Distinct()
                        .OrderBy(a => a.Nombre)
                        .Select(a => new { Id = a.Id.ToString(), a.Nombre })
                        .ToList();
                    cblsHeaderArea.DataSource = areasUnicas;
                    cblsHeaderArea.DataBind();
                    cblsHeaderArea.AcceptChanges += OnAcceptChanges;
                }

                if (dgvFormulacion.HeaderRow.FindControl("cblsHeaderLineaGestion") is TreeViewSearch cblsHeaderLineaGestion)
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

                if (dgvFormulacion.HeaderRow.FindControl("cblsHeaderProyecto") is TreeViewSearch cblsHeaderProyecto)
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

                if (dgvFormulacion.HeaderRow.FindControl("cblsHeaderPrioridad") is TreeViewSearch cblsHeaderPrioridad)
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

                if (dgvFormulacion.HeaderRow.FindControl("cblsHeaderEmpresa") is TreeViewSearch cblsHeaderEmpresa)
                {
                    var empresasUnicas = formulacionesCompletas
                        .Where(f => f.ObraEF?.Empresa != null)
                        .Select(f => f.ObraEF.Empresa)
                        .Distinct()
                        .OrderBy(emp => emp.Nombre)
                        .Select(emp => new { Id = emp.Id.ToString(), emp.Nombre })
                        .ToList();
                    cblsHeaderEmpresa.DataSource = empresasUnicas;
                    cblsHeaderEmpresa.DataBind();
                    cblsHeaderEmpresa.AcceptChanges += OnAcceptChanges;
                }

                if (dgvFormulacion.HeaderRow.FindControl("cblsHeaderBarrio") is TreeViewSearch cblsHeaderBarrio)
                {
                    var barriosUnicos = formulacionesCompletas
                        .Where(f => f.ObraEF?.Barrio != null)
                        .Select(f => f.ObraEF.Barrio)
                        .Distinct()
                        .OrderBy(b => b.Nombre)
                        .Select(b => new { Id = b.Id.ToString(), b.Nombre })
                        .ToList();
                    cblsHeaderBarrio.DataSource = barriosUnicos;
                    cblsHeaderBarrio.DataBind();
                    cblsHeaderBarrio.AcceptChanges += OnAcceptChanges;
                }

                if (dgvFormulacion.HeaderRow.FindControl("cblsHeaderObra") is TreeViewSearch cblsHeaderObra)
                {
                    var obrasUnicas = formulacionesCompletas
                        .Where(f => f.ObraEF != null)
                        .Select(f => f.ObraEF)
                        .Distinct()
                        .OrderBy(o => o.Descripcion)
                        .Select(o => new { Id = o.Id.ToString(), Nombre = o.Descripcion })
                        .ToList();
                    cblsHeaderObra.DataSource = obrasUnicas;
                    cblsHeaderObra.DataBind();
                    cblsHeaderObra.AcceptChanges += OnAcceptChanges;
                }
            }
        }

        protected void btnShowAddModal_Click(object sender, EventArgs e)
        {
            // Preparar ddlObra para agregar nueva formulación;
            lblObra.Attributes["for"] = ddlObraAgregar.ClientID;
            ddlObraAgregar.Visible = true;
            ddlObraEditar.Visible = false;
            rfvObra.Enabled = true;


            LimpiarFormulario();
            ScriptManager.RegisterStartupScript(this, this.GetType(), "ResetModalTitleAndShow", @"
                $(document).ready(function() {
                    $('#modalAgregar .modal-title').text('Agregar Formulación');
                    document.getElementById('" + btnAgregar.ClientID + @"').value = 'Agregar';
                    $('.col-12:first').show();
                    $('#modalAgregar').modal('show');
                });", true);

            btnAgregar.Text = "Agregar";
            Session["EditingObraId"] = null;
        }

        // btnAgregar_Click: guardado compartido del modal (Agregar y Editar).
        // - Lee los campos compartidos y los 3 montos del ciclo.
        // - Llama a GuardarPivot, que hace upsert/borrado de las (hasta) 3 filas de la obra.
        // - Modo edición si Session["EditingObraId"] está presente; si no, es alta.
        // - Muestra mensaje de éxito/error, limpia el formulario y refresca la grilla.
        protected void btnAgregar_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            try
            {
                int? editingObraId = Session["EditingObraId"] as int?;
                int obraId = editingObraId ?? int.Parse(ddlObraAgregar.SelectedValue);

                var montos = new decimal?[]
                {
                    ParseMonto(txtMonto1.Text),
                    ParseMonto(txtMonto2.Text),
                    ParseMonto(txtMonto3.Text)
                };

                // Debe cargarse al menos un monto en alguno de los años del ciclo.
                if (!montos.Any(m => m.HasValue))
                {
                    ToastService.Show(this.Page, "Cargue al menos un monto para alguno de los años.", ToastService.ToastType.Warning);
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowModalWithError", "$('#modalAgregar').modal('show');", true);
                    return;
                }

                // Campos compartidos: se replican a las (hasta) 3 filas de la obra.
                var compartido = new Dominio.FormulacionEF
                {
                    MesBase = string.IsNullOrWhiteSpace(txtMesBase.Text) ? (DateTime?)null : DateTime.Parse(txtMesBase.Text),
                    Observaciones = txtObservaciones.Text,
                    Ppi = string.IsNullOrWhiteSpace(txtPpi.Text) ? (int?)null : int.Parse(txtPpi.Text),
                    Techos = string.IsNullOrWhiteSpace(txtTechos.Text) ? (decimal?)null : decimal.Parse(txtTechos.Text.Replace('.', ',')),
                    UnidadMedidaId = string.IsNullOrEmpty(ddlUnidadMedida.SelectedValue) ? (int?)null : int.Parse(ddlUnidadMedida.SelectedValue),
                    ValorMedida = string.IsNullOrWhiteSpace(txtValorMedida.Text) ? (decimal?)null : decimal.Parse(txtValorMedida.Text.Replace('.', ',')),
                    PrioridadId = string.IsNullOrEmpty(ddlPrioridades.SelectedValue) ? (int?)null : int.Parse(ddlPrioridades.SelectedValue),
                    BreveDescripcion = txtBreveDescripcion.Text,
                    FechaInicio = string.IsNullOrWhiteSpace(txtFechaInicio.Text) ? (DateTime?)null : DateTime.Parse(txtFechaInicio.Text),
                    FechaFin = string.IsNullOrWhiteSpace(txtFechaFin.Text) ? (DateTime?)null : DateTime.Parse(txtFechaFin.Text)
                };

                if (_negocio.GuardarPivot(obraId, compartido, montos))
                {
                    ToastService.Show(this.Page, editingObraId != null ? "Formulación modificada exitosamente!" : "Formulación agregada exitosamente!", ToastService.ToastType.Success);
                }
                else
                {
                    ToastService.Show(this.Page, "No se pudo guardar la formulación.", ToastService.ToastType.Error);
                }

                LimpiarFormulario();
                ScriptManager.RegisterStartupScript(this, this.GetType(), "HideModal", "$('#modalAgregar').modal('hide');", true);
                Session["EditingObraId"] = null;
                Session["formulacionesCompletas"] = null;

                // Refresca SOLO las obras (excluye la recién formulada). No rebindea unidad/prioridad
                // para no duplicarlas (tienen AppendDataBoundItems).
                BindObraDropDowns(null, UserHelper.GetFullCurrentUser());
                BindGrid();
            }
            catch (InvalidOperationException ex)
            {
                ToastService.Show(this.Page, ex.Message, ToastService.ToastType.Error);
                ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowModalWithError", "$('#modalAgregar').modal('show');", true);
            }
            catch (Exception)
            {
                ToastService.Show(this.Page, "Ocurrió un error al guardar. Intente nuevamente.", ToastService.ToastType.Error);
                ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowModalWithError", "$('#modalAgregar').modal('show');", true);
            }
        }

        // Convierte el texto de un campo de monto en decimal? (acepta '.' o ',' como separador decimal).
        private static decimal? ParseMonto(string text)
        {
            return string.IsNullOrWhiteSpace(text) ? (decimal?)null : decimal.Parse(text.Replace('.', ','));
        }

        // dgvFormulacion_SelectedIndexChanged: abre el modal en modo edición (por OBRA).
        // - Carga el pivot de la obra (sus filas del ciclo) y rellena los 3 montos + campos compartidos.
        // - Define Session["EditingObraId"] para indicar modo edición.
        // - Ajusta el texto del botón a "Actualizar" y muestra el modal.
        protected void dgvFormulacion_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                int obraId = Convert.ToInt32(dgvFormulacion.SelectedDataKey.Value);
                var pivot = _negocio.ObtenerPivotPorObra(obraId);
                if (pivot == null)
                {
                    ToastService.Show(this.Page, "No se encontró la formulación de la obra.", ToastService.ToastType.Error);
                    return;
                }

                Session["EditingObraId"] = obraId;

                lblObra.Attributes["for"] = ddlObraEditar.ClientID;
                ddlObraAgregar.Visible = false;
                ddlObraEditar.Visible = true;
                ddlObraEditar.Enabled = false;
                rfvObra.Enabled = false;

                SelectDropDownListByValue(ddlObraEditar, obraId.ToString());
                txtMonto1.Text = pivot.MontoAnio1?.ToString(CultureInfo.CurrentCulture) ?? "";
                txtMonto2.Text = pivot.MontoAnio2?.ToString(CultureInfo.CurrentCulture) ?? "";
                txtMonto3.Text = pivot.MontoAnio3?.ToString(CultureInfo.CurrentCulture) ?? "";
                txtPpi.Text = pivot.Ppi?.ToString() ?? "";
                txtTechos.Text = pivot.Techos?.ToString(CultureInfo.CurrentCulture) ?? "";
                txtMesBase.Text = pivot.MesBase?.ToString("yyyy-MM-dd") ?? "";
                SelectDropDownListByValue(ddlUnidadMedida, pivot.UnidadMedidaId?.ToString());
                txtValorMedida.Text = pivot.ValorMedida?.ToString(CultureInfo.CurrentCulture) ?? "";
                txtObservaciones.Text = pivot.Observaciones ?? "";
                SelectDropDownListByValue(ddlPrioridades, pivot.PrioridadId?.ToString());
                txtBreveDescripcion.Text = pivot.BreveDescripcion ?? "";
                txtFechaInicio.Text = pivot.FechaInicio?.ToString("yyyy-MM-dd") ?? "";
                txtFechaFin.Text = pivot.FechaFin?.ToString("yyyy-MM-dd") ?? "";

                btnAgregar.Text = "Actualizar";

                ScriptManager.RegisterStartupScript(this, this.GetType(), "UpdateModalAndShow", @"
                    $(document).ready(function() {
                        $('#modalAgregar .modal-title').text('Modificar Formulación');
                        document.getElementById('" + btnAgregar.ClientID + @"').value = 'Actualizar';
                        $('#modalAgregar').modal('show');
                    });", true);
            }
            catch (Exception)
            {
                ToastService.Show(this.Page, "Error al cargar los datos. Intente nuevamente.", ToastService.ToastType.Error);
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
                int obraId = Convert.ToInt32(dgvFormulacion.DataKeys[e.RowIndex].Value);
                if (_negocio.EliminarPorObra(obraId))
                {
                    ToastService.Show(this.Page, "Formulación eliminada correctamente.", ToastService.ToastType.Success);

                    Session["formulacionesCompletas"] = null;
                    // La obra vuelve a estar disponible para Agregar.
                    BindObraDropDowns(null, UserHelper.GetFullCurrentUser());
                    BindGrid();
                }
                else
                {
                    ToastService.Show(this.Page, "No se pudo eliminar la formulación.", ToastService.ToastType.Error);
                }
            }
            catch (InvalidOperationException ex)
            {
                ToastService.Show(this.Page, ex.Message, ToastService.ToastType.Error);
            }
            catch (Exception)
            {
                ToastService.Show(this.Page, "No se pudo eliminar la formulación. Intente nuevamente.", ToastService.ToastType.Error);
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
                var selPrioridades = GetSelectedValues("cblsHeaderPrioridad").Select(int.Parse).ToList();
                var selObras = GetSelectedValues("cblsHeaderObra").Select(int.Parse).ToList();
                var selEmpresas = GetSelectedValues("cblsHeaderEmpresa").Select(int.Parse).ToList();
                var selBarrios = GetSelectedValues("cblsHeaderBarrio").Select(int.Parse).ToList();

                int pageIndex = paginationControl.CurrentPageIndex;
                int pageSize = paginationControl.PageSize;

                // Total de OBRAS (1 fila por obra) con formulación en el ciclo vigente, según filtros
                int total = _negocio.ContarObrasConFiltros(usuario, filtroGeneral, selAreas, selLineas, selProyectos, selPrioridades, selObras, selEmpresas, selBarrios);

                if (pageIndex * pageSize >= Math.Max(1, total))
                {
                    pageIndex = 0;
                    paginationControl.CurrentPageIndex = 0;
                }
                paginationControl.TotalRecords = total;
                paginationControl.UpdatePaginationControls();

                // Página actual pivoteada (1 fila por obra con sus 3 montos)
                var pagina = _negocio.ListarPivotPaginadoConFiltros(usuario, pageIndex, pageSize, filtroGeneral, selAreas, selLineas, selProyectos, selPrioridades, selObras, selEmpresas, selBarrios);

                dgvFormulacion.DataSource = pagina;
                dgvFormulacion.DataBind();

                // Total global de montos (suma de los años del ciclo) de las obras filtradas
                decimal totalMontoGlobal = 0m;
                if (total > 0)
                {
                    var anios = FormulacionCiclo.Anios;
                    using (var context = new IVCdbContext())
                    {
                        var querySubtotal = context.Formulaciones.Where(f => anios.Contains(f.FechaPeriodo.Year));

                        if (!usuario.Tipo)
                        {
                            var filtroAreaIds = usuario.IvcAreaIds;
                            if (filtroAreaIds != null && filtroAreaIds.Count > 0)
                                querySubtotal = querySubtotal.Where(f => f.ObraEF.AreaId.HasValue && filtroAreaIds.Contains(f.ObraEF.AreaId.Value));
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
                        if (selPrioridades.Any()) querySubtotal = querySubtotal.Where(f => f.PrioridadEF != null && selPrioridades.Contains(f.PrioridadEF.Id));
                        if (selObras.Any()) querySubtotal = querySubtotal.Where(f => selObras.Contains(f.ObraId));
                        if (selEmpresas.Any()) querySubtotal = querySubtotal.Where(f => f.ObraEF.EmpresaId.HasValue && selEmpresas.Contains(f.ObraEF.EmpresaId.Value));
                        if (selBarrios.Any()) querySubtotal = querySubtotal.Where(f => f.ObraEF.BarrioId.HasValue && selBarrios.Contains(f.ObraEF.BarrioId.Value));

                        totalMontoGlobal = querySubtotal.Sum(f => (decimal?)f.Monto) ?? 0m;
                    }
                }

                paginationControl.SubtotalText = $"Total Monto: {totalMontoGlobal:C} ({total} obras)";
            }
            catch (Exception)
            {
                ToastService.Show(this.Page, "Error al cargar los datos. Intente nuevamente.", ToastService.ToastType.Error);
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

        // Bind completo: se llama una sola vez en Page_Load (!IsPostBack).
        // Unidad de medida y prioridad NO se rebindean en postbacks (tienen AppendDataBoundItems
        // y se duplicarían). Las obras se refrescan aparte con BindObraDropDowns tras guardar/eliminar.
        private void BindDropDownList()
        {
            try
            {
                UsuarioEF usuario = UserHelper.GetFullCurrentUser();
                int? obraEnEdicion = Session["EditingObraId"] as int?;

                BindObraDropDowns(obraEnEdicion, usuario);

                ddlUnidadMedida.DataSource = new UnidadMedidaNegocioEF().Listar();
                ddlUnidadMedida.DataTextField = "Nombre";
                ddlUnidadMedida.DataValueField = "Id";
                ddlUnidadMedida.DataBind();

                ddlPrioridades.DataSource = new PrioridadNegocioEF().Listar();
                ddlPrioridades.DataTextField = "Nombre";
                ddlPrioridades.DataValueField = "Id";
                ddlPrioridades.DataBind();
            }
            catch (Exception)
            {
                ToastService.Show(this.Page, "Error al cargar las listas. Intente nuevamente.", ToastService.ToastType.Error);
            }
        }

        // Rebind de los dropdowns de obra. Limpia los items antes de bindear (los combos tienen
        // AppendDataBoundItems="true"): así no se acumulan ni queda seleccionable una obra ya formulada.
        private void BindObraDropDowns(int? obraEnEdicion, UsuarioEF usuario)
        {
            var obraNegocio = new ObraNegocioEF();

            // ddlObraAgregar: solo obras sin formulación en el ciclo (más la que se edite, por las dudas).
            var obrasAgregar = obraNegocio.ListarParaDDLNoEnFormulacion(obraEnEdicion, usuario)
                .Select(o => new
                {
                    o.Id,
                    DescripcionCompleta = $"{o.Descripcion} - {o.Empresa?.Nombre ?? "Sin empresa"} - {o.Barrio?.Nombre ?? "Sin barrio"}"
                })
                .ToList();

            // ddlObraEditar: todas las obras (el combo va deshabilitado y solo muestra la seleccionada).
            var obrasEditar = obraNegocio.ListarParaDDL()
                .Select(o => new
                {
                    o.Id,
                    DescripcionCompleta = $"{o.Descripcion} - {o.Empresa?.Nombre ?? "Sin empresa"} - {o.Barrio?.Nombre ?? "Sin barrio"}"
                })
                .ToList();

            ddlObraAgregar.Items.Clear();
            ddlObraAgregar.Items.Add(new ListItem("Seleccione una obra", ""));
            ddlObraAgregar.DataSource = obrasAgregar;
            ddlObraAgregar.DataTextField = "DescripcionCompleta";
            ddlObraAgregar.DataValueField = "Id";
            ddlObraAgregar.DataBind();

            ddlObraEditar.Items.Clear();
            ddlObraEditar.Items.Add(new ListItem("Seleccione una obra", ""));
            ddlObraEditar.DataSource = obrasEditar;
            ddlObraEditar.DataTextField = "DescripcionCompleta";
            ddlObraEditar.DataValueField = "Id";
            ddlObraEditar.DataBind();
        }

        private void LimpiarFormulario()
        {
            ddlObraAgregar.SelectedIndex =0;
            ddlObraEditar.SelectedIndex =0;
            txtMonto1.Text = "";
            txtMonto2.Text = "";
            txtMonto3.Text = "";
            txtPpi.Text = "";
            txtTechos.Text = "";
            txtMesBase.Text = "";
            ddlUnidadMedida.SelectedIndex =0;
            txtValorMedida.Text = "";
            txtObservaciones.Text = "";
            txtBreveDescripcion.Text = "";
            txtFechaInicio.Text = "";
            txtFechaFin.Text = "";
            ddlPrioridades.SelectedIndex =0;
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
                    int totalRecords = _negocio.ContarPorUsuario(usuario, filtroGeneral);
                    paginationControl.Initialize(totalRecords,0, paginationControl.PageSize);
                    BindGrid();
                }
            }
            catch (Exception)
            {
                ToastService.Show(this.Page, "Error al aplicar los filtros. Intente nuevamente.", ToastService.ToastType.Error);
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
            catch (Exception)
            {
                ToastService.Show(this.Page, "Error al cambiar de página. Intente nuevamente.", ToastService.ToastType.Error);
            }
        }

        /// <summary>
        /// Calcula el total plurianual para la OBRA: suma de todos los CertificadoEF.MontoTotal
        /// y LegitimoEF.Certificado para esa misma OBRA en los años2026,2027,2028
        /// (usa MesAprobacion para filtrar año).
        /// Se acepta como parámetro el ObraId.
        /// </summary>
        protected string CalcularPlurianual(object obraIdObj)
        {
            try
            {
                if (obraIdObj == null || obraIdObj == DBNull.Value) return 0m.ToString("C");

                if (!int.TryParse(obraIdObj.ToString(), out int obraId)) return 0m.ToString("C");

                // Preferir diccionario precalculado para la página (evita consultas por fila)
                if (Context.Items["PlurianualesPagina"] is Dictionary<int, decimal> dic)
                {
                    if (dic.TryGetValue(obraId, out decimal val))
                        return val.ToString("C");
                    return 0m.ToString("C");
                }

                DateTime start2026 = new DateTime(2026,1,1);
                DateTime end2028 = new DateTime(2028,12,31);

                decimal total =0m;

                using (var ctx = new IVCdbContext())
                {
                    //1) Sumar Certificados cuyo MesAprobacion esté entre2026-01-01 y2028-12-31
                    // y que pertenezcan a la obra: relac. Certificado -> Autorizante (CodigoAutorizante) -> Autorizante.ObraId == obraId
                     var certificadosQuery = from c in ctx.Certificados
                                            where c.MesAprobacion.HasValue
                                                  && c.MesAprobacion.Value >= start2026
                                                  && c.MesAprobacion.Value <= end2028
                                            join a in ctx.Autorizantes on c.CodigoAutorizante equals a.CodigoAutorizante
                                            where a.ObraId == obraId
                                            select c.MontoTotal;

                    var sumaCertificados = certificadosQuery.Any() ? certificadosQuery.Sum() :0m;

                    //2) Sumar Legitimos donde Legitimo.ObraId == obraId y MesAprobacion en2026-2028
                    var legitimosQuery = ctx.Legitimos
                        .Where(l => l.ObraId == obraId && l.MesAprobacion.HasValue && l.MesAprobacion.Value >= start2026 && l.MesAprobacion.Value <= end2028)
                        .Select(l => l.Certificado ??0m);

                    var sumaLegitimos = legitimosQuery.Any() ? legitimosQuery.Sum() :0m;

                    total = sumaCertificados + sumaLegitimos;


                    // Si no hay certificados ni legítimos en2026-2028, se deja total =0
                }



                return total.ToString("C");
            }
            catch
            {
                return 0m.ToString("C");
            }
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
                int totalRecords = _negocio.ContarPorUsuario(usuario, filtroGeneral);
                paginationControl.Initialize(totalRecords,0, e.PageSize);
                BindGrid();
            }
        }

        #endregion

    }
}
