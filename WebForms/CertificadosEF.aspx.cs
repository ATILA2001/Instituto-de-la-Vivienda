using Dominio;
using Dominio.DTO;
using Negocio;
using Negocio.Redeterminaciones;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebForms.CustomControls;
using WebForms.Infraestructura;

namespace WebForms
{
    /// <summary>
    /// Certificados y reliquidaciones. La paginación, el filtro de texto, los filtros de cabecera y la
    /// exportación vienen de GridPageBase; acá queda lo propio: qué se carga, cómo se filtra, el alta,
    /// la edición (modal e inline), la baja y el recálculo de SIGAF/SADE sobre el snapshot.
    /// </summary>
    public partial class CertificadosEF : GridPageBase<CertificadoDTO>
    {
        private readonly CertificadoNegocioEF negocio = new CertificadoNegocioEF();
        private readonly CalculoRedeterminacionNegocioEF calculoRedeterminacionNegocio = new CalculoRedeterminacionNegocioEF();

        private bool? _puedeModificar;
        /// <summary>Evaluado una sola vez por request: la regla consulta Session y la base.</summary>
        private bool PuedeModificar =>
            _puedeModificar ?? (_puedeModificar = UserHelper.PuedeModificarPlanificacion()).Value;

        private static bool EsReliquidacion(CertificadoDTO c) => c.TipoPagoId == ReglasRedeterminaciones.TipoPagoReliquidacionId;

        // ------------------------------------------------------------------
        // GridPageBase
        // ------------------------------------------------------------------

        protected override GridView Grilla => gridviewRegistros;
        protected override TextBox CajaBusqueda => txtBuscar;
        protected override PaginationControl Paginador => paginationControl;
        protected override string ClaveSesion => "CertificadosCompleto";
        protected override int IdDe(CertificadoDTO item) => item.Id;
        protected override string NombreClaveVirtual => "ClaveReliquidacion";
        protected override string ClaveVirtualDe(CertificadoDTO item) => item.ClaveReliquidacion;
        protected override string MensajeErrorCarga => "Error al cargar los certificados. Intente nuevamente.";

        protected override List<CertificadoDTO> CargarListaCompleta() =>
            calculoRedeterminacionNegocio.ListarCertificadosYReliquidaciones(UserHelper.GetFullCurrentUser());

        protected override bool CoincideTexto(CertificadoDTO c, string filtro) =>
            (c.ExpedientePago?.ToUpper().Contains(filtro) ?? false) ||
            (c.EmpresaNombre?.ToUpper().Contains(filtro) ?? false) ||
            (c.CodigoAutorizante?.ToUpper().Contains(filtro) ?? false) ||
            (c.ObraDescripcion?.ToUpper().Contains(filtro) ?? false) ||
            (c.AreaNombre?.ToUpper().Contains(filtro) ?? false) ||
            (c.BarrioNombre?.ToUpper().Contains(filtro) ?? false) ||
            (c.TipoPagoNombre?.ToUpper().Contains(filtro) ?? false);

        protected override List<CertificadoDTO> AplicarFiltrosHeader(List<CertificadoDTO> datos)
        {
            var areas = IdsSeleccionados("cblsHeaderArea");
            var obras = IdsSeleccionados("cblsHeaderObra");
            var barrios = IdsSeleccionados("cblsHeaderBarrio");
            var proyectos = IdsSeleccionados("cblsHeaderProyecto");
            var empresas = IdsSeleccionados("cblsHeaderEmpresa");
            var tipos = IdsSeleccionados("cblsHeaderTipo");
            var lineas = IdsSeleccionados("cblsHeaderLinea");
            var estadosAutorizante = IdsSeleccionados("cblsHeaderAutorizanteEstado");
            var codigos = ValoresSeleccionados("cblsHeaderCodigoAutorizante");
            var estados = ValoresSeleccionados("cblsHeaderEstado");
            var meses = FechasSeleccionadas("cblsHeaderMesCertificado");

            if (areas != null) datos = datos.Where(c => c.AreaId.HasValue && areas.Contains(c.AreaId.Value)).ToList();
            if (obras != null) datos = datos.Where(c => c.ObraId.HasValue && obras.Contains(c.ObraId.Value)).ToList();
            if (barrios != null) datos = datos.Where(c => c.BarrioId.HasValue && barrios.Contains(c.BarrioId.Value)).ToList();
            if (proyectos != null) datos = datos.Where(c => c.ProyectoId.HasValue && proyectos.Contains(c.ProyectoId.Value)).ToList();
            if (empresas != null) datos = datos.Where(c => c.EmpresaId.HasValue && empresas.Contains(c.EmpresaId.Value)).ToList();
            if (codigos != null) datos = datos.Where(c => !string.IsNullOrEmpty(c.CodigoAutorizante) && codigos.Contains(c.CodigoAutorizante)).ToList();
            if (estados != null) datos = datos.Where(c => !string.IsNullOrEmpty(c.Estado) && estados.Contains(c.Estado)).ToList();
            if (tipos != null) datos = datos.Where(c => c.TipoPagoId.HasValue && tipos.Contains(c.TipoPagoId.Value)).ToList();
            if (meses != null) datos = datos.Where(c => c.MesAprobacion.HasValue && meses.Contains(c.MesAprobacion.Value.Date)).ToList();
            if (lineas != null) datos = datos.Where(c => c.LineaGestionId.HasValue && lineas.Contains(c.LineaGestionId.Value)).ToList();

            if (estadosAutorizante != null)
            {
                // El DTO trae el nombre del estado del autorizante, no el Id: se traducen los Ids elegidos a nombres.
                List<string> nombres;
                using (var context = new IVCdbContext())
                {
                    nombres = context.EstadosAutorizante.AsNoTracking()
                        .Where(e => estadosAutorizante.Contains(e.Id))
                        .Select(e => e.Nombre)
                        .ToList();
                }
                datos = datos.Where(c => !string.IsNullOrEmpty(c.AutorizanteEstado) && nombres.Contains(c.AutorizanteEstado)).ToList();
            }

            return datos;
        }

        protected override void PoblarFiltrosHeader(List<CertificadoDTO> certificados)
        {
            if (Grilla.HeaderRow == null) return;

            // Área, obra y proyecto salen del snapshot del usuario, no de la tabla completa (no mostrar otras áreas).
            BindFiltroHeader("cblsHeaderArea", FiltrosHeader.Distintos(certificados, c => c.AreaId, c => c.AreaNombre));
            BindFiltroHeader("cblsHeaderObra", FiltrosHeader.Distintos(certificados, c => c.ObraId, c => c.ObraDescripcion));
            BindFiltroHeader("cblsHeaderProyecto", FiltrosHeader.Distintos(certificados, c => c.ProyectoId, c => c.ProyectoNombre));

            var codigos = certificados
                .Select(c => new { c.CodigoAutorizante })
                .Distinct()
                .OrderBy(c => c.CodigoAutorizante)
                .ToList();
            BindFiltroHeader("cblsHeaderCodigoAutorizante", codigos, "CodigoAutorizante", "CodigoAutorizante");

            // Estados de certificados: el texto es a la vez Id y nombre.
            var estados = new[] { "NO INICIADO", "EN TRAMITE", "DEVENGADO" }
                .Select(e => new { Id = e, Nombre = e })
                .ToList();
            BindFiltroHeader("cblsHeaderEstado", estados);

            using (var context = new IVCdbContext())
            {
                BindFiltroHeader("cblsHeaderBarrio", context.Barrios.AsNoTracking().OrderBy(b => b.Nombre).Select(b => new { b.Id, b.Nombre }).ToList());
                BindFiltroHeader("cblsHeaderEmpresa", context.Empresas.AsNoTracking().OrderBy(e => e.Nombre).Select(e => new { e.Id, e.Nombre }).ToList());
                BindFiltroHeader("cblsHeaderTipo", context.TiposPago.AsNoTracking().OrderBy(t => t.Nombre).Select(t => new { t.Id, t.Nombre }).ToList());
                BindFiltroHeader("cblsHeaderLinea", context.LineasGestion.AsNoTracking().OrderBy(l => l.Nombre).Select(l => new { l.Id, l.Nombre }).ToList());
                BindFiltroHeader("cblsHeaderAutorizanteEstado", context.EstadosAutorizante.AsNoTracking().OrderBy(e => e.Nombre).Select(e => new { e.Id, Estado = e.Nombre }).ToList(), "Estado", "Id");

                // Meses de aprobación distintos: lista de fechas sin campos de texto/valor.
                var meses = context.Certificados.AsNoTracking()
                    .Where(c => c.MesAprobacion.HasValue)
                    .Select(c => c.MesAprobacion.Value)
                    .Distinct()
                    .OrderBy(d => d)
                    .ToList();
                if (FiltroHeader("cblsHeaderMesCertificado") is TreeViewSearch filtroMes)
                {
                    filtroMes.DataSource = meses;
                    filtroMes.DataBind();
                }
            }
        }

        protected override decimal Subtotal(List<CertificadoDTO> certificados) => certificados.Sum(c => c.MontoTotal);

        // ------------------------------------------------------------------
        // Ciclo de vida
        // ------------------------------------------------------------------

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindDropDownList();
                CargarPaginaActual();
            }
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            rfvAutorizante.Enabled = ViewState["EditingCertificadoId"] == null;
        }

        protected void btnExportarExcel_Click(object sender, EventArgs e)
        {
            var mapeo = new Dictionary<string, string>
            {
                { "Área", "AreaNombre" },
                { "Contrata", "Contrata" },
                { "Obra", "ObraDescripcion" },
                { "Barrio", "BarrioNombre" },
                { "Proyecto", "ProyectoNombre" },
                { "Empresa", "EmpresaNombre" },
                { "Código Autorizante", "CodigoAutorizante" },
                { "Estado Autorizante", "AutorizanteEstado" },
                { "Expediente", "ExpedientePago" },
                { "Estado", "Estado" },
                { "Tipo", "TipoPagoNombre" },
                { "Monto Certificado", "MontoTotal" },
                { "Mes Certificado", "MesAprobacion" },
                { "Línea", "LineaGestionNombre" },
                { "Porcentaje", "Porcentaje" },
                { "Sigaf", "Sigaf" },
                { "Buzón SADE", "BuzonSade" },
                { "Fecha SADE", "FechaSade" },
            };

            ExportarExcel(mapeo, "Certificados");
        }

        // ------------------------------------------------------------------
        // Recálculo de SIGAF / SADE sobre el snapshot
        // ------------------------------------------------------------------

        /// <summary>
        /// Recalcula SIGAF, buzón y fecha SADE de los expedientes indicados sobre el snapshot en sesión,
        /// limpia la caché SADE y vuelve a la primera página. Sin expedientes, sólo rebindea la página actual.
        /// </summary>
        private void RecalcularYRecargar(IEnumerable<string> expedientesAfectados)
        {
            var expedientes = (expedientesAfectados ?? Enumerable.Empty<string>())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Trim())
                .Distinct()
                .ToList();

            if (expedientes.Count == 0)
            {
                CargarPaginaActual();
                return;
            }

            try
            {
                calculoRedeterminacionNegocio.CalcularCertificadosPorExpedientes(expedientes, ListaCompleta, persistirEnBD: false);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[CertificadosEF] Error al recalcular SIGAF/buzón/fecha SADE: " + ex);
                ToastService.Show(this.Page, "No se pudieron recalcular SIGAF y SADE. Los datos mostrados pueden estar desactualizados; recargue la página.", ToastService.ToastType.Error);
            }
            finally
            {
                CalculoRedeterminacionNegocioEF.LimpiarCacheSade();
            }

            RecargarDesdePrimeraPagina();
        }

        // ------------------------------------------------------------------
        // Alta y edición por modal
        // ------------------------------------------------------------------

        private void BindDropDownList()
        {
            try
            {
                ddlTipo.DataSource = new TipoPagoNegocioEF().Listar();
                ddlTipo.DataTextField = "Nombre";
                ddlTipo.DataValueField = "Id";
                ddlTipo.DataBind();

                var autorizantes = new AutorizanteNegocioEF().ListarParaDDL(UserHelper.GetFullCurrentUser());
                ddlAutorizante.DataSource = autorizantes;
                ddlAutorizante.DataTextField = "CodigoAutorizante";
                ddlAutorizante.DataValueField = "Id";
                ddlAutorizante.DataBind();
                foreach (ListItem item in ddlAutorizante.Items)
                {
                    var autorizante = autorizantes.FirstOrDefault(a => a.Id.ToString() == item.Value);
                    if (autorizante?.Obra != null)
                    {
                        string sufijo = $"{autorizante.Obra.Empresa?.Nombre ?? "Sin empresa"} - {autorizante.Obra.Barrio?.Nombre ?? "Sin barrio"}";
                        item.Text = $"{autorizante.CodigoAutorizante} - {TextoCombo.Acortar(autorizante.Obra.Descripcion)} - {sufijo}";
                        item.Attributes["title"] = $"{autorizante.CodigoAutorizante} - {autorizante.Obra.Descripcion} - {sufijo}";
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[CertificadosEF] Error al cargar las listas del formulario: " + ex);
                ToastService.Show(this.Page, "Error al cargar los datos del formulario.", ToastService.ToastType.Error);
            }
        }

        private void LimpiarFormulario()
        {
            txtExpediente.Text = string.Empty;
            txtMontoCertificado.Text = string.Empty;
            txtFecha.Text = string.Empty;
            ddlAutorizante.SelectedIndex = 0;
            ddlTipo.SelectedIndex = 0;
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

            ViewState["EditingCertificadoId"] = null;
        }

        protected void btnAgregar_Click(object sender, EventArgs e)
        {
            if (!UserHelper.PuedeModificarPlanificacion())
            {
                ToastService.Show(this.Page, "No tiene permisos para realizar esta acción.", ToastService.ToastType.Error);
                return;
            }

            if (!Page.IsValid) return;

            try
            {
                bool resultado;
                string expedienteAnterior = null;
                string nuevoExpediente = txtExpediente.Text.Trim();
                decimal nuevoMonto = decimal.Parse(txtMontoCertificado.Text.Trim());
                DateTime? nuevoMes = string.IsNullOrWhiteSpace(txtFecha.Text) ? (DateTime?)null : DateTime.Parse(txtFecha.Text);
                int nuevoTipoPagoId = int.Parse(ddlTipo.SelectedValue);

                int? editingId = ViewState["EditingCertificadoId"] as int?;

                if (editingId != null)
                {
                    CertificadoEF certificadoExistente = negocio.ObtenerPorId(editingId.Value);
                    if (certificadoExistente == null)
                    {
                        ToastService.Show(this.Page, "Error: No se encontró el certificado a modificar.", ToastService.ToastType.Error);
                        return;
                    }

                    expedienteAnterior = certificadoExistente.ExpedientePago;

                    certificadoExistente.ExpedientePago = nuevoExpediente;
                    certificadoExistente.MontoTotal = nuevoMonto;
                    certificadoExistente.MesAprobacion = nuevoMes;
                    certificadoExistente.TipoPagoId = nuevoTipoPagoId;

                    resultado = negocio.Modificar(certificadoExistente);
                    ToastService.Show(this.Page,
                        resultado ? "Certificado modificado exitosamente!" : "Hubo un problema al modificar el certificado.",
                        resultado ? ToastService.ToastType.Success : ToastService.ToastType.Error);
                }
                else
                {
                    var nuevoCertificado = new CertificadoEF
                    {
                        ExpedientePago = nuevoExpediente,
                        MontoTotal = nuevoMonto,
                        MesAprobacion = nuevoMes,
                        TipoPagoId = nuevoTipoPagoId,
                        CodigoAutorizante = new AutorizanteNegocioEF().ObtenerPorId(int.Parse(ddlAutorizante.SelectedValue)).CodigoAutorizante
                    };

                    resultado = negocio.Agregar(nuevoCertificado);
                    ToastService.Show(this.Page,
                        resultado ? "Certificado agregado exitosamente!" : "Hubo un problema al agregar el certificado.",
                        resultado ? ToastService.ToastType.Success : ToastService.ToastType.Error);
                }

                if (!resultado) return;

                if (editingId == null)
                {
                    // Alta: el registro nuevo no está en el snapshot, se recarga desde la base.
                    InvalidarListaCompleta();
                }
                else
                {
                    // Edición: reflejar los campos editables en el DTO del snapshot antes del recálculo.
                    var dto = ListaCompleta.FirstOrDefault(c => c.Id == editingId.Value);
                    if (dto != null)
                    {
                        dto.ExpedientePago = nuevoExpediente;
                        dto.MontoTotal = nuevoMonto;
                        dto.MesAprobacion = nuevoMes;
                        dto.TipoPagoId = nuevoTipoPagoId;
                        dto.TipoPagoNombre = ddlTipo.SelectedItem?.Text ?? dto.TipoPagoNombre;
                    }
                }

                var expedientesAfectados = new[] { expedienteAnterior, nuevoExpediente }.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                if (expedientesAfectados.Count > 0)
                {
                    RecalcularYRecargar(expedientesAfectados);
                }
                else
                {
                    InvalidarListaCompleta();
                    CalculoRedeterminacionNegocioEF.LimpiarCacheSade();
                    CargarPaginaActual();
                }

                ViewState["EditingCertificadoId"] = null;
                ScriptManager.RegisterStartupScript(this, this.GetType(), "HideModal", "$('#modalAgregar').modal('hide');", true);
            }
            catch (InvalidOperationException ex)
            {
                ToastService.Show(this.Page, ex.Message, ToastService.ToastType.Error);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[CertificadosEF] Error al guardar el certificado: " + ex);
                ToastService.Show(this.Page, "Ocurrió un error al guardar. Intente nuevamente.", ToastService.ToastType.Error);
            }
        }

        /// <summary>
        /// Abre el modal en modo edición. Las reliquidaciones (TipoPagoId 3) son virtuales y se editan desde el
        /// DTO calculado; los certificados reales se leen de la base.
        /// </summary>
        protected void gridviewRegistros_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                CertificadoDTO seleccionado = ItemDeFila(gridviewRegistros.SelectedIndex);
                if (seleccionado == null)
                {
                    ToastService.Show(this.Page, "Error: Registro no encontrado.", ToastService.ToastType.Error);
                    return;
                }

                if (EsReliquidacion(seleccionado))
                {
                    txtExpediente.Text = seleccionado.ExpedientePago;
                    txtMontoCertificado.Text = seleccionado.MontoTotal.ToString("0.00");
                    txtFecha.Text = seleccionado.MesAprobacion?.ToString("yyyy-MM-dd");
                    SeleccionarValor(ddlTipo, seleccionado.TipoPagoId?.ToString());
                    btnAgregar.Text = "Actualizar";

                    ScriptManager.RegisterStartupScript(this, this.GetType(), "UpdateModalAndShow", @"
                                $(document).ready(function() {
                                    $('#modalAgregar .modal-title').text('Modificar Reliquidación');
                                    document.getElementById('" + btnAgregar.ClientID + @"').value = 'Actualizar';
                                    $('#autorizanteContainer').hide();
                                    $('#modalAgregar').modal('show');
                                });", true);
                    return;
                }

                CertificadoEF certificadoEF = negocio.ObtenerPorId(seleccionado.Id);
                if (certificadoEF == null)
                {
                    ToastService.Show(this.Page, "Error: No se pudo cargar el certificado para edición.", ToastService.ToastType.Error);
                    return;
                }

                ViewState["EditingCertificadoId"] = seleccionado.Id;
                txtExpediente.Text = certificadoEF.ExpedientePago;
                txtMontoCertificado.Text = certificadoEF.MontoTotal.ToString("0.00");
                txtFecha.Text = certificadoEF.MesAprobacion?.ToString("yyyy-MM-dd");
                SeleccionarValor(ddlTipo, certificadoEF.TipoPagoId.ToString());
                btnAgregar.Text = "Actualizar";

                ScriptManager.RegisterStartupScript(this, this.GetType(), "UpdateModalAndShow", @"
                $(document).ready(function() {
                    $('#modalAgregar .modal-title').text('Modificar Certificado');
                    document.getElementById('" + btnAgregar.ClientID + @"').value = 'Actualizar';
                    $('#autorizanteContainer').hide();
                    $('#modalAgregar').modal('show');
                });", true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[CertificadosEF] Error al cargar el certificado para edición: " + ex);
                ToastService.Show(this.Page, "Error al cargar los datos del certificado. Intente nuevamente.", ToastService.ToastType.Error);
            }
        }

        // ------------------------------------------------------------------
        // Baja
        // ------------------------------------------------------------------

        protected void gridviewRegistros_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            if (!UserHelper.PuedeModificarPlanificacion())
            {
                e.Cancel = true;
                ToastService.Show(this.Page, "No tiene permisos para realizar esta acción.", ToastService.ToastType.Error);
                return;
            }

            try
            {
                var id = Convert.ToInt32(gridviewRegistros.DataKeys[e.RowIndex].Value);
                if (!negocio.Eliminar(id))
                    return;

                ToastService.Show(this.Page, "Certificado eliminado correctamente.", ToastService.ToastType.Success);

                // Quitar el registro del snapshot y recalcular el expediente que lo contenía.
                var lista = ListaCompleta;
                var eliminado = lista.FirstOrDefault(c => c.Id == id);
                lista.RemoveAll(c => c.Id == id);

                RecalcularYRecargar(new[] { eliminado?.ExpedientePago });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[CertificadosEF] Error al eliminar el certificado: " + ex);
                ToastService.Show(this.Page, "No se pudo eliminar el certificado. Intente nuevamente.", ToastService.ToastType.Error);
            }
        }

        protected void gridviewRegistros_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;

            var certificado = (CertificadoDTO)e.Row.DataItem;
            if (certificado.IdReliquidacion > 0)
            {
                if (e.Row.FindControl("btnModificar") is LinkButton btnEditar) btnEditar.Visible = false;
                if (e.Row.FindControl("btnEliminar") is LinkButton btnEliminar) btnEliminar.Visible = false;
                if (e.Row.FindControl("txtMontoInline") is TextBox txtMontoRel) txtMontoRel.Enabled = false;
            }
            else if (e.Row.FindControl("txtMontoInline") is TextBox txtMonto)
            {
                txtMonto.Enabled = !string.IsNullOrWhiteSpace(certificado.ExpedientePago);
            }

            // Misma regla que verifican los handlers inline: sin permiso, los controles se ven deshabilitados.
            if (!PuedeModificar)
            {
                foreach (var id in new[] { "txtExpediente", "txtMontoInline", "txtPorcEjecFisicaInline" })
                {
                    if (e.Row.FindControl(id) is TextBox txt) txt.Enabled = false;
                }
            }
        }

        // ------------------------------------------------------------------
        // Edición inline
        // ------------------------------------------------------------------

        protected void txtExpediente_TextChanged(object sender, EventArgs e)
        {
            if (!UserHelper.PuedeModificarPlanificacion())
            {
                ToastService.Show(this.Page, "No tiene permisos para realizar esta acción.", ToastService.ToastType.Error);
                CargarPaginaActual();
                return;
            }

            try
            {
                string nuevoExpediente = ((TextBox)sender).Text.Trim();

                CertificadoDTO certificado = ItemDelControlInline(sender);
                if (certificado == null)
                {
                    ToastService.Show(this.Page, "Error: Índice fuera del rango de datos.", ToastService.ToastType.Error);
                    return;
                }

                string expedienteAnterior = certificado.ExpedientePago;
                bool resultado = false;

                if (string.IsNullOrWhiteSpace(nuevoExpediente))
                {
                    // Expediente vacío: se borra en la base y el DTO vuelve a NO INICIADO sin datos SIGAF/SADE.
                    if (EsReliquidacion(certificado))
                    {
                        resultado = new ExpedienteReliqNegocioEF().GuardarOActualizar(certificado.CodigoAutorizante, certificado.MesAprobacion.Value, "");
                    }
                    else if (certificado.Id > 0)
                    {
                        CertificadoEF entidad = negocio.ObtenerPorId(certificado.Id);
                        if (entidad != null)
                        {
                            entidad.ExpedientePago = string.Empty;
                            resultado = negocio.Modificar(entidad);
                        }
                    }

                    certificado.ExpedientePago = nuevoExpediente;
                    certificado.Estado = "NO INICIADO";
                    certificado.Sigaf = null;
                    certificado.BuzonSade = null;
                    certificado.FechaSade = null;

                    ToastService.Show(this.Page,
                        resultado ? "Expediente eliminado correctamente." : "No se detectaron cambios para guardar.",
                        resultado ? ToastService.ToastType.Info : ToastService.ToastType.Warning);
                }
                else
                {
                    certificado.ExpedientePago = nuevoExpediente;

                    if (EsReliquidacion(certificado))
                    {
                        if (!certificado.MesAprobacion.HasValue)
                        {
                            ToastService.Show(this.Page, "Error: La reliquidación no tiene fecha de aprobación.", ToastService.ToastType.Error);
                            return;
                        }
                        resultado = new ExpedienteReliqNegocioEF().GuardarOActualizar(certificado.CodigoAutorizante, certificado.MesAprobacion.Value, nuevoExpediente);
                    }
                    else if (certificado.Id > 0)
                    {
                        var entidad = negocio.ObtenerPorId(certificado.Id);
                        if (entidad != null)
                        {
                            entidad.ExpedientePago = nuevoExpediente;
                            resultado = negocio.Modificar(entidad);
                        }
                    }
                }

                if (!resultado)
                {
                    ToastService.Show(this.Page, "Error al actualizar el expediente.", ToastService.ToastType.Error);
                    return;
                }

                RecalcularYRecargar(new[] { expedienteAnterior, nuevoExpediente });

                string tipoRegistro = EsReliquidacion(certificado) ? "reliquidación" : "certificado";
                ToastService.Show(this.Page, $"Expediente de {tipoRegistro} actualizado correctamente.", ToastService.ToastType.Info);
            }
            catch (InvalidOperationException ex)
            {
                ToastService.Show(this.Page, ex.Message, ToastService.ToastType.Error);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[CertificadosEF] Error al actualizar el expediente: " + ex);
                ToastService.Show(this.Page, "Error al actualizar el expediente. Intente nuevamente.", ToastService.ToastType.Error);
            }
        }

        protected void txtMontoInline_TextChanged(object sender, EventArgs e)
        {
            if (!UserHelper.PuedeModificarPlanificacion())
            {
                ToastService.Show(this.Page, "No tiene permisos para realizar esta acción.", ToastService.ToastType.Error);
                CargarPaginaActual();
                return;
            }

            try
            {
                string nuevoValorStr = ((TextBox)sender).Text.Trim();
                if (string.IsNullOrWhiteSpace(nuevoValorStr)) return;

                if (!MontoHelper.TryParseEsAr(nuevoValorStr, out decimal nuevoMonto))
                {
                    ToastService.Show(this.Page, "Error: El monto ingresado no es válido. Use coma para los decimales (ej.: 1.234,56).", ToastService.ToastType.Error);
                    // Volver a bindear para que el textbox muestre el valor guardado y no el texto rechazado
                    CargarPaginaActual();
                    return;
                }

                CertificadoDTO certificado = ItemDelControlInline(sender);
                if (certificado == null)
                {
                    ToastService.Show(this.Page, "Error: Índice fuera del rango de datos.", ToastService.ToastType.Error);
                    return;
                }

                if (EsReliquidacion(certificado) || certificado.Id <= 0)
                {
                    ToastService.Show(this.Page, "Error: No se puede modificar el monto de este registro.", ToastService.ToastType.Error);
                    return;
                }

                CertificadoEF entidad = negocio.ObtenerPorId(certificado.Id);
                if (entidad == null)
                {
                    ToastService.Show(this.Page, "Error: No se encontró el certificado.", ToastService.ToastType.Error);
                    return;
                }

                entidad.MontoTotal = nuevoMonto;
                if (!negocio.Modificar(entidad))
                {
                    ToastService.Show(this.Page, "Error al actualizar el monto.", ToastService.ToastType.Error);
                    return;
                }

                certificado.MontoTotal = nuevoMonto;
                RecalcularYRecargar(new[] { certificado.ExpedientePago });

                ToastService.Show(this.Page, "Monto actualizado correctamente.", ToastService.ToastType.Info);
            }
            catch (InvalidOperationException ex)
            {
                ToastService.Show(this.Page, ex.Message, ToastService.ToastType.Error);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[CertificadosEF] Error al actualizar el monto: " + ex);
                ToastService.Show(this.Page, "Error al actualizar el monto. Intente nuevamente.", ToastService.ToastType.Error);
            }
        }

        protected void txtPorcEjecFisicaInline_TextChanged(object sender, EventArgs e)
        {
            if (!UserHelper.PuedeModificarPlanificacion())
            {
                ToastService.Show(this.Page, "No tiene permisos para realizar esta acción.", ToastService.ToastType.Error);
                CargarPaginaActual();
                return;
            }

            try
            {
                string nuevoValorStr = ((TextBox)sender).Text.Trim();

                CertificadoDTO dto = ItemDelControlInline(sender);
                if (dto == null)
                {
                    ToastService.Show(this.Page, "Error: Índice fuera del rango de datos.", ToastService.ToastType.Error);
                    return;
                }

                if (dto.IdReliquidacion != 0 || dto.Id <= 0)
                {
                    ToastService.Show(this.Page, "La ejecución física solo se puede cargar en certificados reales.", ToastService.ToastType.Warning);
                    return;
                }

                decimal? nuevoPorcentaje = null;
                if (!string.IsNullOrWhiteSpace(nuevoValorStr))
                {
                    nuevoValorStr = nuevoValorStr.Replace(",", ".");
                    if (!decimal.TryParse(nuevoValorStr, System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out decimal parsed))
                    {
                        ToastService.Show(this.Page, "Error: El porcentaje ingresado no es válido.", ToastService.ToastType.Error);
                        CargarPaginaActual();
                        return;
                    }
                    nuevoPorcentaje = parsed;
                }

                negocio.ActualizarEjecFisica(dto.Id, nuevoPorcentaje);
                dto.PorcEjecFisica = nuevoPorcentaje;

                // Invalidar los snapshots de las páginas que agregan ejecución por autorizante/obra.
                Session["ObrasCompleto"] = null;
                Session["GridDataAutorizantes"] = null;
                CargarPaginaActual();

                ToastService.Show(this.Page, "Ejecución física actualizada.", ToastService.ToastType.Info);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[CertificadosEF] Error al actualizar la ejecución física: " + ex);
                ToastService.Show(this.Page, "No se pudo actualizar la ejecución física. Intente nuevamente.", ToastService.ToastType.Error);
            }
        }
    }
}
