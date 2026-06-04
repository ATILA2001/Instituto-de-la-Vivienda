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
                        { "Línea de Gestión", "ObraEF.Proyecto.LineaGestionEF.Nombre" },
                        { "Proyecto", "ObraEF.Proyecto.Nombre" },
                        { "Nombre de Obra", "ObraEF.Descripcion" },
                        { "Breve Descripción", "BreveDescripcion" },
                        { "Fecha Inicio", "FechaInicio" },
                        { "Fecha Fin", "FechaFin" },
                        { "Año", "FechaPeriodo" },
                        { "PPI", "Ppi" },
                        { "Techo", "Techos" },
                        { "Monto", "Monto" },
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

                if (dgvFormulacion.HeaderRow.FindControl("cblsHeaderMonto") is TreeViewSearch cblsHeaderMonto)
                {
                    var montosUnicos = formulacionesCompletas
                        .Where(f => f.Monto.HasValue)
                        .Select(f => f.Monto.Value)
                        .Distinct()
                        .OrderBy(m => m)
                        .Select(m => new { Id = m.ToString(CultureInfo.InvariantCulture), Nombre = m })
                        .ToList();
                    cblsHeaderMonto.DataSource = montosUnicos;
                    cblsHeaderMonto.DataBind();
                    cblsHeaderMonto.AcceptChanges += OnAcceptChanges;
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

                if (dgvFormulacion.HeaderRow.FindControl("cblsHeaderAnio") is TreeViewSearch cblsHeaderAnio)
                {
                    var aniosUnicos = formulacionesCompletas
                        .Select(f => f.FechaPeriodo.Year)
                        .Distinct()
                        .OrderBy(a => a)
                        .Select(a => new { Id = a.ToString(), Nombre = a.ToString() })
                        .ToList();
                    cblsHeaderAnio.DataSource = aniosUnicos;
                    cblsHeaderAnio.DataBind();
                    cblsHeaderAnio.AcceptChanges += OnAcceptChanges;
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
            Session["EditingFormulacionId"] = null;
        }

        // btnAgregar_Click: controlador de guardado compartido para el modal (Agregar y Editar).
        // - Lee los valores del formulario y arma una entidad FormulacionEF.
        // - Si Session["EditingFormulacionId"] está presente, actualiza (Modificar); si no, agrega (Agregar).
        // - Muestra mensaje de éxito/error, limpia el formulario y refresca la grilla.
        protected void btnAgregar_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            try
            {
                int? editingId = Session["EditingFormulacionId"] as int?;

                Dominio.FormulacionEF formu = editingId != null ? _negocio.ObtenerPorId(editingId.GetValueOrDefault()) : new Dominio.FormulacionEF();


                int obraIdSeleccionada = editingId != null ? int.Parse(ddlObraEditar.SelectedValue) : int.Parse(ddlObraAgregar.SelectedValue);
                int anioSeleccionado = int.Parse(ddlAnio.SelectedValue);

                formu.ObraId = obraIdSeleccionada;
                formu.FechaPeriodo = new DateTime(anioSeleccionado, 1, 1);
                formu.Monto = string.IsNullOrWhiteSpace(txtMonto.Text) ? (decimal?)null : decimal.Parse(txtMonto.Text.Replace('.', ','));
                formu.MesBase = string.IsNullOrWhiteSpace(txtMesBase.Text) ? (DateTime?)null : DateTime.Parse(txtMesBase.Text);
                formu.Observaciones = txtObservaciones.Text;
                formu.Ppi = string.IsNullOrWhiteSpace(txtPpi.Text) ? (int?)null : int.Parse(txtPpi.Text);
                formu.Techos = string.IsNullOrWhiteSpace(txtTechos.Text) ? (decimal?)null : decimal.Parse(txtTechos.Text.Replace('.', ','));
                formu.UnidadMedidaId = string.IsNullOrEmpty(ddlUnidadMedida.SelectedValue) ? (int?)null : int.Parse(ddlUnidadMedida.SelectedValue);
                formu.ValorMedida = string.IsNullOrWhiteSpace(txtValorMedida.Text) ? (decimal?)null : decimal.Parse(txtValorMedida.Text.Replace('.', ','));
                formu.PrioridadId = string.IsNullOrEmpty(ddlPrioridades.SelectedValue) ? (int?)null : int.Parse(ddlPrioridades.SelectedValue);
                formu.BreveDescripcion = txtBreveDescripcion.Text;
                formu.FechaInicio = string.IsNullOrWhiteSpace(txtFechaInicio.Text) ? (DateTime?)null : DateTime.Parse(txtFechaInicio.Text);
                formu.FechaFin = string.IsNullOrWhiteSpace(txtFechaFin.Text) ? (DateTime?)null : DateTime.Parse(txtFechaFin.Text);



                if (editingId != null)
                {
                    if (_negocio.Modificar(formu))
                    {
                        ToastService.Show(this.Page, "Formulación modificada exitosamente!", ToastService.ToastType.Success);
                        Session["EditingFormulacionId"] = null;
                    }
                    else
                    {
                        ToastService.Show(this.Page, "No se pudo modificar la formulación.", ToastService.ToastType.Error);
                    }
                }
                else
                {
                    if(_negocio.Agregar(formu))
                    {
                        ToastService.Show(this.Page, "Formulación agregada exitosamente!", ToastService.ToastType.Success);
                    }
                    else
                    {
                        ToastService.Show(this.Page, "No se pudo agregar la formulación.", ToastService.ToastType.Error);
                    }
                }

                LimpiarFormulario();
                ScriptManager.RegisterStartupScript(this, this.GetType(), "HideModal", "$('#modalAgregar').modal('hide');", true);
                Session["EditingFormulacionId"] = null;
                Session["formulacionesCompletas"] = null;

                BindGrid();
            }
            catch (DbUpdateException ex) when (ex.InnerException?.InnerException?.Message.Contains("UQ_FORMULACION_BASE_PERIODO") == true ||
                                                ex.InnerException?.Message.Contains("UQ_FORMULACION_BASE_PERIODO") == true)
            {
                int anio = int.Parse(ddlAnio.SelectedValue);
                ToastService.Show(this.Page, $"Ya existe una formulación para esa obra en el año {anio}.", ToastService.ToastType.Error);
                ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowModalWithError", "$('#modalAgregar').modal('show');", true);
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

        // dgvFormulacion_SelectedIndexChanged:abre el modal en modo edición.
        // - Carga la FormulacionEF seleccionada (desde sesión o desde la BD si falta).
        // - Define Session["EditingFormulacionId"] para indicar modo edición.
        // - Re-carga los dropdowns para incluir la obra vinculada (ddlObra permanecerá oculta en la UI).
        // - Rellena los controles del modal, ajusta el texto del botón a "Modificar" y muestra el modal.
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
                        lista = _negocio.ListarPorUsuario(usuario);
                        Session["formulacionesCompletas"] = lista;
                        formulacion = lista?.FirstOrDefault(f => f.Id == id);
                    }
                }
                if (formulacion != null)
                {
                    // MARCAR MODO EDICIÓN y asegurar que los dropdowns incluyan la obra en edición
                    Session["EditingFormulacionId"] = formulacion.Id;

                    lblObra.Attributes["for"] = ddlObraEditar.ClientID;
                    ddlObraAgregar.Visible = false;
                    ddlObraEditar.Visible = true;
                    ddlObraEditar.Enabled = false;
                    rfvObra.Enabled = false;

                    SelectDropDownListByValue(ddlObraEditar, formulacion.ObraId.ToString());
                    SelectDropDownListByValue(ddlAnio, formulacion.FechaPeriodo.Year.ToString());
                    txtMonto.Text = formulacion.Monto?.ToString(CultureInfo.CurrentCulture) ?? "";
                    txtPpi.Text = formulacion.Ppi?.ToString() ?? "";
                    txtTechos.Text = formulacion.Techos?.ToString(CultureInfo.CurrentCulture) ?? "";
                    txtMesBase.Text = formulacion.MesBase?.ToString("yyyy-MM-dd") ?? "";
                    SelectDropDownListByValue(ddlUnidadMedida, formulacion.UnidadMedidaId?.ToString());
                    txtValorMedida.Text = formulacion.ValorMedida?.ToString(CultureInfo.CurrentCulture) ?? "";
                    txtObservaciones.Text = formulacion.Observaciones ?? "";
                    SelectDropDownListByValue(ddlPrioridades, formulacion.PrioridadId?.ToString());
                    txtBreveDescripcion.Text = formulacion.BreveDescripcion ?? "";
                    txtFechaInicio.Text = formulacion.FechaInicio?.ToString("yyyy-MM-dd") ?? "";
                    txtFechaFin.Text = formulacion.FechaFin?.ToString("yyyy-MM-dd") ?? "";

                    btnAgregar.Text = "Actualizar";

                    // Mostrar modal en modo edición. No hacemos visible ddlObra, solo aseguramos que su item exista.
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "UpdateModalAndShow", @"
                        $(document).ready(function() {
                            $('#modalAgregar .modal-title').text('Modificar Formulación');
                            document.getElementById('" + btnAgregar.ClientID + @"').value = 'Actualizar';



                            //$('.col-12:first').hide();
                            $('#modalAgregar').modal('show');
                        });", true);

                }
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
                int id = Convert.ToInt32(dgvFormulacion.DataKeys[e.RowIndex].Value);
                if (_negocio.Eliminar(id))
                {
                    ToastService.Show(this.Page, "Formulación eliminada correctamente.", ToastService.ToastType.Success);

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
                var selMontos = GetSelectedValues("cblsHeaderMonto").Select(s => decimal.Parse(s, CultureInfo.InvariantCulture)).ToList();
                var selPrioridades = GetSelectedValues("cblsHeaderPrioridad").Select(int.Parse).ToList();
                var selObras = GetSelectedValues("cblsHeaderObra").Select(int.Parse).ToList();
                var selAnios = GetSelectedValues("cblsHeaderAnio").Select(int.Parse).ToList();
                var selEmpresas = GetSelectedValues("cblsHeaderEmpresa").Select(int.Parse).ToList();
                var selBarrios = GetSelectedValues("cblsHeaderBarrio").Select(int.Parse).ToList();

                int pageIndex = paginationControl.CurrentPageIndex;
                int pageSize = paginationControl.PageSize;

                // Total filtrado en BD
                int total = _negocio.ContarPorUsuarioConFiltros(usuario, filtroGeneral, selAreas, selLineas, selProyectos, selMontos, selPrioridades, selObras, selAnios, selEmpresas, null, selBarrios);


                if (pageIndex * pageSize >= Math.Max(1, total))
                {
                    pageIndex =0;
                    paginationControl.CurrentPageIndex =0;
                }
                paginationControl.TotalRecords = total;
                paginationControl.UpdatePaginationControls();

                // Página actual desde BD
                var pagina = _negocio.ListarPorUsuarioPaginadoConFiltros(usuario, pageIndex, pageSize, filtroGeneral, selAreas, selLineas, selProyectos, selMontos, selPrioridades, selObras, selAnios, selEmpresas, null, selBarrios);

                dgvFormulacion.DataSource = pagina;
                dgvFormulacion.DataBind();

                // Precalcular PLURIANUALES para las obras de la página (evita N+1)
                try
                {
                    var obraIds = pagina.Select(f => f.ObraId).Where(id => id !=0).Distinct().ToList();
                    var plurianuales = new Dictionary<int, decimal>();
                    if (obraIds.Any())
                    {
                        DateTime start = new DateTime(2026,1,1);
                        DateTime end = new DateTime(2028,12,31);
                        using (var ctx = new IVCdbContext())
                        {
                            var certificadosPorObra = (from c in ctx.Certificados
                                                       where c.MesAprobacion.HasValue && c.MesAprobacion.Value >= start && c.MesAprobacion.Value <= end
                                                       join a in ctx.Autorizantes on c.CodigoAutorizante equals a.CodigoAutorizante
                                                       where obraIds.Contains(a.ObraId)
                                                       group c by a.ObraId into g
                                                       select new { ObraId = g.Key, Sum = g.Sum(x => (decimal?)x.MontoTotal) ??0m })
                                                      .ToList();

                            var legitimosPorObra = ctx.Legitimos
                                .Where(l => obraIds.Contains(l.ObraId) && l.MesAprobacion.HasValue && l.MesAprobacion.Value >= start && l.MesAprobacion.Value <= end)
                                .GroupBy(l => l.ObraId)
                                .Select(g => new { ObraId = g.Key, Sum = g.Sum(x => (decimal?)x.Certificado) ??0m })
                                .ToList();

                            // Inicializar
                            foreach (var id in obraIds) plurianuales[id] =0m;

                            foreach (var c in certificadosPorObra)
                            {
                                if (plurianuales.ContainsKey(c.ObraId))
                                    plurianuales[c.ObraId] += c.Sum;
                            }

                            foreach (var l in legitimosPorObra)
                            {
                                if (plurianuales.ContainsKey(l.ObraId))
                                    plurianuales[l.ObraId] += l.Sum;
                            }
                        }
                    }
                    Context.Items["PlurianualesPagina"] = plurianuales;
                }
                catch
                {
                    // En caso de error, aseguramos que el diccionario exista para evitar excepciones en el binding
                    Context.Items["PlurianualesPagina"] = new Dictionary<int, decimal>();
                }


                decimal totalMontoGlobal = 0;
                if (total >0)
                {

                    using (var context = new IVCdbContext())
                    {
                        var querySubtotal = context.Formulaciones.AsQueryable();

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
                        if (selMontos.Any()) querySubtotal = querySubtotal.Where(f => f.Monto.HasValue && selMontos.Contains(f.Monto.Value));
                        if (selPrioridades.Any()) querySubtotal = querySubtotal.Where(f => f.PrioridadEF != null && selPrioridades.Contains(f.PrioridadEF.Id));
                        if (selObras.Any()) querySubtotal = querySubtotal.Where(f => selObras.Contains(f.ObraId));
                        if (selAnios.Any()) querySubtotal = querySubtotal.Where(f => selAnios.Contains(f.FechaPeriodo.Year));
                        if (selEmpresas.Any()) querySubtotal = querySubtotal.Where(f => f.ObraEF.EmpresaId.HasValue && selEmpresas.Contains(f.ObraEF.EmpresaId.Value));
                        if (selBarrios.Any()) querySubtotal = querySubtotal.Where(f => f.ObraEF.BarrioId.HasValue && selBarrios.Contains(f.ObraEF.BarrioId.Value));

                        totalMontoGlobal = querySubtotal.Sum(f => (decimal?)f.Monto) ?? 0m;
                    }
                }

                paginationControl.SubtotalText = $"Total Monto: {totalMontoGlobal:C} ({total} registros)";
                CalcularSubtotal(pagina);

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

        private void BindDropDownList()
        {
            try
            {
                // ddlObra: listar solo obras que no están en Formulación (y filtrar por área si corresponde)
                UsuarioEF usuario = UserHelper.GetFullCurrentUser();
                int? obraEnEdicion = null;
                if (Session["EditingFormulacionId"] is int editId)
                {
                    // obtener la obra asociada a la formulación en edición para incluirla en el combo
                    using (var ctx = new IVCdbContext())
                    {
                        var form = ctx.Formulaciones.AsNoTracking().FirstOrDefault(f => f.Id == editId);
                        obraEnEdicion = form?.ObraId;
                    }
                }
                ddlObraAgregar.DataSource = new ObraNegocioEF().ListarParaDDL();
                ddlObraAgregar.DataTextField = "Descripcion";
                ddlObraAgregar.DataValueField = "Id";
                ddlObraAgregar.DataBind();

                ddlObraEditar.DataSource = new ObraNegocioEF().ListarParaDDL();
                ddlObraEditar.DataTextField = "Descripcion";
                ddlObraEditar.DataValueField = "Id";
                ddlObraEditar.DataBind();


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

        private void LimpiarFormulario()
        {
            ddlObraAgregar.SelectedIndex =0;
            ddlObraEditar.SelectedIndex =0;
            ddlAnio.SelectedIndex = 0;
            txtMonto.Text = "";
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

        /// <summary>
        /// Calcula subtotales para la página actual y actualiza el control
        /// </summary>
        private void CalcularSubtotal(List<Dominio.FormulacionEF> formulacionesPagina)
        {

            decimal totalMonto = 0;
            int count = formulacionesPagina?.Count ?? 0;

            if (formulacionesPagina != null)
            {
                totalMonto = formulacionesPagina
                    .Sum(f => f.Monto ?? 0);
            }

            var paginationInfo = paginationControl.GetPaginationInfo();
            paginationControl.SubtotalText = $"Total Monto: {totalMonto:C} ({paginationInfo.TotalRecords} registros)";

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