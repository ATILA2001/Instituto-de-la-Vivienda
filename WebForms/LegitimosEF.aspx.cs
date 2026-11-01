using Dominio;
using Negocio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebForms.CustomControls;
using WebForms.Infraestructura;

namespace WebForms
{
    /// <summary>
    /// Legítimos abonos. La paginación, el filtro de texto, los filtros de cabecera y la exportación
    /// vienen de GridPageBase; acá queda lo propio: qué se carga, cómo se filtra, el alta, la edición
    /// (modal e inline), la baja y el recálculo de SIGAF/SADE sobre el snapshot.
    /// </summary>
    public partial class LegitimosEF : GridPageBase<LegitimoEF>
    {
        private readonly LegitimoNegocioEF negocio = new LegitimoNegocioEF();
        private readonly CalculoRedeterminacionNegocioEF calculoRedeterminacionNegocio = new CalculoRedeterminacionNegocioEF();

        private bool? _puedeModificar;
        /// <summary>Evaluado una sola vez por request: la regla consulta Session y la base.</summary>
        private bool PuedeModificar =>
            _puedeModificar ?? (_puedeModificar = UserHelper.PuedeModificarPlanificacion()).Value;

        // ------------------------------------------------------------------
        // GridPageBase
        // ------------------------------------------------------------------

        protected override GridView Grilla => gridviewRegistros;
        protected override TextBox CajaBusqueda => txtBuscar;
        protected override PaginationControl Paginador => paginationControl;
        protected override string ClaveSesion => "legitimosCompletos";
        protected override string MensajeErrorCarga => "Error al cargar los legítimos. Intente nuevamente.";

        protected override List<LegitimoEF> CargarListaCompleta() =>
            // Sin filtros: el negocio devuelve la lista completa del usuario con Empresa, Línea, SIGAF, SADE y Estado.
            negocio.ListarPorUsuarioConFiltrosCompleto(UserHelper.GetFullCurrentUser(), null,
                new List<int>(), new List<int>(), new List<string>(), new List<string>(), new List<DateTime?>());

        protected override bool CoincideTexto(LegitimoEF l, string filtro) =>
            (l.Expediente?.ToUpper().Contains(filtro) ?? false) ||
            (l.ObraEF?.Empresa?.Nombre?.ToUpper().Contains(filtro) ?? false) ||
            (l.CodigoAutorizante?.ToUpper().Contains(filtro) ?? false) ||
            (l.ObraEF?.Descripcion?.ToUpper().Contains(filtro) ?? false) ||
            (l.ObraEF?.Area?.Nombre?.ToUpper().Contains(filtro) ?? false) ||
            (l.ObraEF?.Barrio?.Nombre?.ToUpper().Contains(filtro) ?? false) ||
            (l.ObraEF?.Proyecto?.LineaGestionEF?.Nombre?.ToUpper().Contains(filtro) ?? false);

        protected override List<LegitimoEF> AplicarFiltrosHeader(List<LegitimoEF> datos)
        {
            var areas = IdsSeleccionados("cblsHeaderArea");
            var empresas = IdsSeleccionados("cblsHeaderEmpresa");
            var lineas = IdsSeleccionados("cblsHeaderLineaGestion");
            var autorizantes = ValoresSeleccionados("cblsHeaderAutorizante");
            var estados = ValoresSeleccionados("cblsHeaderEstado");
            var meses = FechasSeleccionadas("cblsHeaderMesAprobacion");

            if (areas != null) datos = datos.Where(l => l.ObraEF?.Area != null && areas.Contains(l.ObraEF.Area.Id)).ToList();
            if (empresas != null) datos = datos.Where(l => l.ObraEF?.Empresa != null && empresas.Contains(l.ObraEF.Empresa.Id)).ToList();
            if (autorizantes != null) datos = datos.Where(l => !string.IsNullOrEmpty(l.CodigoAutorizante) && autorizantes.Contains(l.CodigoAutorizante)).ToList();
            if (estados != null) datos = datos.Where(l => !string.IsNullOrEmpty(l.Estado) && estados.Contains(l.Estado)).ToList();
            if (meses != null) datos = datos.Where(l => l.MesAprobacion.HasValue && meses.Contains(l.MesAprobacion.Value.Date)).ToList();
            if (lineas != null) datos = datos.Where(l => l.ObraEF?.Proyecto?.LineaGestionEF != null && lineas.Contains(l.ObraEF.Proyecto.LineaGestionEF.Id)).ToList();

            return datos;
        }

        protected override void PoblarFiltrosHeader(List<LegitimoEF> legitimos)
        {
            // Todas las opciones salen del snapshot del usuario.
            BindFiltroHeader("cblsHeaderArea", FiltrosHeader.Distintos(legitimos, l => l.ObraEF?.Area?.Id, l => l.ObraEF?.Area?.Nombre));
            BindFiltroHeader("cblsHeaderEmpresa", FiltrosHeader.Distintos(legitimos, l => l.ObraEF?.Empresa?.Id, l => l.ObraEF?.Empresa?.Nombre));
            BindFiltroHeader("cblsHeaderLineaGestion", FiltrosHeader.Distintos(legitimos, l => l.ObraEF?.Proyecto?.LineaGestionEF?.Id, l => l.ObraEF?.Proyecto?.LineaGestionEF?.Nombre));

            var autorizantes = legitimos
                .Where(l => !string.IsNullOrEmpty(l.CodigoAutorizante))
                .Select(l => l.CodigoAutorizante)
                .Distinct()
                .OrderBy(a => a)
                .Select(a => new { Id = a, Nombre = a })
                .ToList();
            BindFiltroHeader("cblsHeaderAutorizante", autorizantes);

            var estados = new[] { "NO INICIADO", "EN TRAMITE", "DEVENGADO" }
                .Select(e => new { Id = e, Nombre = e })
                .ToList();
            BindFiltroHeader("cblsHeaderEstado", estados);

            // Meses de aprobación distintos: lista de fechas, el control arma el árbol año/mes/día.
            if (FiltroHeader("cblsHeaderMesAprobacion") is TreeViewSearch filtroMes)
            {
                filtroMes.DataSource = legitimos
                    .Where(l => l.MesAprobacion.HasValue)
                    .Select(l => l.MesAprobacion.Value)
                    .Distinct()
                    .OrderBy(m => m)
                    .ToList();
                filtroMes.DataBind();
            }
        }

        protected override decimal Subtotal(List<LegitimoEF> legitimos) => legitimos.Sum(l => l.Certificado ?? 0m);

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

        protected void btnExportarExcel_Click(object sender, EventArgs e)
        {
            var mapeo = new Dictionary<string, string>
            {
                { "Área", "ObraEF.Area.Nombre" },
                { "Obra", "ObraEF.Descripcion" },
                { "Barrio", "ObraEF.Barrio.Nombre" },
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

            ExportarExcel(mapeo, "Legitimos");
        }

        protected void gridviewRegistros_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;

            var legitimo = (LegitimoEF)e.Row.DataItem;
            if (e.Row.FindControl("txtCertificadoInline") is TextBox txtCert)
            {
                txtCert.Enabled = !string.IsNullOrWhiteSpace(legitimo.Expediente) && PuedeModificar;
            }
            // Misma regla que verifican los handlers inline: sin permiso, el control se ve deshabilitado.
            if (e.Row.FindControl("txtExpediente") is TextBox txtExp)
            {
                txtExp.Enabled = PuedeModificar;
            }
        }

        // ------------------------------------------------------------------
        // Recálculo de SIGAF / SADE sobre el snapshot
        // ------------------------------------------------------------------

        /// <summary>Estado derivado del expediente y de SIGAF: sin expediente NO INICIADO, sin SIGAF EN TRAMITE, si no DEVENGADO.</summary>
        private static void AplicarEstado(LegitimoEF l)
        {
            if (string.IsNullOrWhiteSpace(l.Expediente))
            {
                l.Estado = "NO INICIADO";
                l.Sigaf = null;
                l.BuzonSade = null;
                l.FechaSade = null;
            }
            else if (!l.Sigaf.HasValue || l.Sigaf == 0)
            {
                l.Estado = "EN TRAMITE";
            }
            else
            {
                l.Estado = "DEVENGADO";
            }
        }

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
                var datosSade = calculoRedeterminacionNegocio.ObtenerDatosSadeBulk(expedientes);
                var datosSigaf = calculoRedeterminacionNegocio.ObtenerSigafBulk(expedientes);

                foreach (var l in ListaCompleta)
                {
                    if (string.IsNullOrWhiteSpace(l.Expediente)) continue;
                    var exp = l.Expediente.Trim();
                    if (!expedientes.Contains(exp)) continue;

                    if (datosSade != null && datosSade.TryGetValue(exp, out var sadeInfo))
                    {
                        l.BuzonSade = sadeInfo.Buzon;
                        l.FechaSade = sadeInfo.Fecha;
                    }

                    if (datosSigaf != null && datosSigaf.TryGetValue(exp, out var sigafVal))
                    {
                        l.Sigaf = sigafVal;
                    }

                    AplicarEstado(l);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[LegitimosEF] Error al recalcular SIGAF/buzón/fecha SADE: " + ex);
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
                var obras = new ObraNegocioEF().ListarParaDDL();
                ddlObra.Items.Clear();
                ddlObra.Items.Add(new ListItem("Seleccione una obra", ""));
                foreach (var o in obras)
                {
                    string barrio = o.Barrio?.Nombre ?? "Sin barrio";
                    var item = new ListItem($"{TextoCombo.Acortar(o.Descripcion)} - {barrio}", o.Id.ToString());
                    item.Attributes["title"] = $"{o.Descripcion} - {barrio}";
                    ddlObra.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[LegitimosEF] Error al cargar las obras del formulario: " + ex);
                ToastService.Show(this.Page, "Error al cargar las obras. Intente nuevamente.", ToastService.ToastType.Error);
            }
        }

        private void LimpiarFormulario()
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

        protected void btnShowAddModal_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();

            ScriptManager.RegisterStartupScript(this, this.GetType(), "ResetModalTitleAndShow",
                "$(document).ready(function() { $('#modalAgregar .modal-title').text('Agregar Legítimo'); document.getElementById('" + btnAgregar.ClientID + "').value = 'Agregar'; $('#modalAgregar').modal('show'); });",
                true);

            btnAgregar.Text = "Agregar";
            ddlObra.Enabled = true; // Permitir seleccionar obra al agregar
            Session["EditingLegitimoEFId"] = null;
        }

        protected void btnAgregar_Click(object sender, EventArgs e)
        {
            if (!UserHelper.PuedeModificarPlanificacion())
            {
                ToastService.Show(this.Page, "No tiene permisos para realizar esta acción.", ToastService.ToastType.Error);
                return;
            }

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

                // Si hay un id en sesión, es edición: se modifica la entidad cargada de la base,
                // copiando sólo los campos del formulario. Modificar() marca toda la entidad como
                // Modified, así que guardar el objeto nuevo pondría en NULL las columnas que el
                // formulario no edita (PORC_EJEC_FISICA).
                if (Session["EditingLegitimoEFId"] != null)
                {
                    var entidad = negocio.ObtenerPorId((int)Session["EditingLegitimoEFId"]);
                    if (entidad == null)
                    {
                        ToastService.Show(this.Page, "Error: No se encontró el legítimo a modificar.", ToastService.ToastType.Error);
                        return;
                    }

                    entidad.ObraId = legitimo.ObraId;
                    entidad.CodigoAutorizante = legitimo.CodigoAutorizante;
                    entidad.Expediente = legitimo.Expediente;
                    entidad.InicioEjecucion = legitimo.InicioEjecucion;
                    entidad.FinEjecucion = legitimo.FinEjecucion;
                    entidad.Certificado = legitimo.Certificado;
                    entidad.MesAprobacion = legitimo.MesAprobacion;
                    // ObtenerPorId trae la navegación ObraEF cargada; si cambió ObraId, adjuntar el grafo en el
                    // contexto nuevo de Modificar lanzaría una violación de integridad referencial de EF.
                    entidad.ObraEF = null;

                    negocio.Modificar(entidad);
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
                BindDropDownList();
                RecargarTrasCambio();
            }
            catch (InvalidOperationException ex)
            {
                ToastService.Show(this.Page, ex.Message, ToastService.ToastType.Error);
                ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowModalWithError", "$('#modalAgregar').modal('show');", true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[LegitimosEF] Error al guardar el legítimo: " + ex);
                ToastService.Show(this.Page, "Ocurrió un error al guardar. Intente nuevamente.", ToastService.ToastType.Error);
                ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowModalWithError", "$('#modalAgregar').modal('show');", true);
            }
        }

        protected void gridviewRegistros_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                int legitimoId = Convert.ToInt32(gridviewRegistros.SelectedDataKey.Value);
                var legitimo = negocio.ObtenerPorId(legitimoId);
                if (legitimo == null)
                {
                    ToastService.Show(this.Page, "Error: No se pudo cargar el legítimo para edición.", ToastService.ToastType.Error);
                    return;
                }

                Session["EditingLegitimoEFId"] = legitimoId;
                BindDropDownList();
                SeleccionarValor(ddlObra, legitimo.ObraId.ToString());
                txtAutorizante.Text = legitimo.CodigoAutorizante ?? string.Empty;
                txtExpediente.Text = legitimo.Expediente ?? string.Empty;
                txtInicioEjecucion.Text = legitimo.InicioEjecucion?.ToString("yyyy-MM-dd") ?? string.Empty;
                txtFinEjecucion.Text = legitimo.FinEjecucion?.ToString("yyyy-MM-dd") ?? string.Empty;
                txtCertificado.Text = legitimo.Certificado?.ToString() ?? string.Empty;
                txtMesAprobacion.Text = legitimo.MesAprobacion?.ToString("yyyy-MM-dd") ?? string.Empty;
                ddlObra.Enabled = false;
                btnAgregar.Text = "Actualizar";

                ScriptManager.RegisterStartupScript(this, this.GetType(), "EditModal", @"
                $(document).ready(function() {
                    $('#modalAgregar .modal-title').text('Modificar Legítimo');
                    $('#modalAgregar').modal('show');
                });", true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[LegitimosEF] Error al cargar el legítimo para edición: " + ex);
                ToastService.Show(this.Page, "Error al cargar los datos del legítimo. Intente nuevamente.", ToastService.ToastType.Error);
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

            var key = gridviewRegistros.DataKeys[e.RowIndex].Value;
            if (key == null || !int.TryParse(key.ToString(), out int id))
            {
                ToastService.Show(this.Page, "Id inválido.", ToastService.ToastType.Error);
                return;
            }

            try
            {
                var lista = ListaCompleta;
                string expedienteAfectado = lista.FirstOrDefault(l => l.Id == id)?.Expediente;

                if (negocio.Eliminar(id))
                {
                    ToastService.Show(this.Page, "Legítimo eliminado exitosamente.", ToastService.ToastType.Success);
                    lista.RemoveAll(l => l.Id == id);
                    RecalcularYRecargar(new[] { expedienteAfectado });
                }
                else
                {
                    InvalidarListaCompleta();
                    CalculoRedeterminacionNegocioEF.LimpiarCacheSade();
                    RecargarDesdePrimeraPagina();
                }
            }
            catch (InvalidOperationException ex)
            {
                ToastService.Show(this.Page, ex.Message, ToastService.ToastType.Error);
                CargarPaginaActual();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[LegitimosEF] Error al eliminar el legítimo: " + ex);
                ToastService.Show(this.Page, "No se pudo eliminar el registro. Intente nuevamente.", ToastService.ToastType.Error);
                CargarPaginaActual();
            }
        }

        // ------------------------------------------------------------------
        // Edición inline
        // ------------------------------------------------------------------

        /// <summary>Elemento del snapshot que corresponde a la fila del control inline que disparó el evento.</summary>
        private LegitimoEF ItemDelControlInline(object sender)
        {
            var fila = (sender as Control)?.NamingContainer as GridViewRow;
            return fila == null ? null : ItemDeFila(fila.RowIndex);
        }

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

                var legitimo = ItemDelControlInline(sender);
                if (legitimo == null)
                {
                    ToastService.Show(this.Page, "Error: Índice fuera del rango de datos.", ToastService.ToastType.Error);
                    return;
                }

                string expedienteAnterior = legitimo.Expediente;

                var entidad = legitimo.Id > 0 ? negocio.ObtenerPorId(legitimo.Id) : null;
                if (entidad == null)
                {
                    ToastService.Show(this.Page, "No se detectaron cambios para guardar.", ToastService.ToastType.Warning);
                    return;
                }

                entidad.Expediente = string.IsNullOrWhiteSpace(nuevoExpediente) ? string.Empty : nuevoExpediente;
                negocio.Modificar(entidad);

                // Sincronizar el snapshot: el expediente nuevo arranca sin SIGAF/SADE hasta el recálculo.
                legitimo.Expediente = entidad.Expediente;
                legitimo.Sigaf = entidad.Sigaf;
                legitimo.BuzonSade = entidad.BuzonSade;
                legitimo.FechaSade = entidad.FechaSade;
                AplicarEstado(legitimo);

                RecalcularYRecargar(new[] { expedienteAnterior, nuevoExpediente });

                ToastService.Show(this.Page, "Expediente actualizado correctamente.", ToastService.ToastType.Info);
            }
            catch (InvalidOperationException ex)
            {
                ToastService.Show(this.Page, ex.Message, ToastService.ToastType.Error);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[LegitimosEF] Error al actualizar el expediente: " + ex);
                ToastService.Show(this.Page, "Error al actualizar el expediente. Intente nuevamente.", ToastService.ToastType.Error);
            }
        }

        protected void txtCertificadoInline_TextChanged(object sender, EventArgs e)
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

                if (!MontoHelper.TryParseEsAr(nuevoValorStr, out decimal nuevoCertificado))
                {
                    ToastService.Show(this.Page, "Error: El monto ingresado no es válido. Use coma para los decimales (ej.: 1.234,56).", ToastService.ToastType.Error);
                    // Volver a bindear para que el textbox muestre el valor guardado y no el texto rechazado
                    CargarPaginaActual();
                    return;
                }

                var legitimo = ItemDelControlInline(sender);
                if (legitimo == null)
                {
                    ToastService.Show(this.Page, "Error: Índice fuera del rango de datos.", ToastService.ToastType.Error);
                    return;
                }

                if (legitimo.Id <= 0)
                {
                    ToastService.Show(this.Page, "Error: No se puede modificar este registro.", ToastService.ToastType.Error);
                    return;
                }

                var entidad = negocio.ObtenerPorId(legitimo.Id);
                if (entidad == null)
                {
                    ToastService.Show(this.Page, "Error: No se encontró el legítimo.", ToastService.ToastType.Error);
                    return;
                }

                entidad.Certificado = nuevoCertificado;
                negocio.Modificar(entidad);
                legitimo.Certificado = nuevoCertificado;

                RecalcularYRecargar(new[] { legitimo.Expediente });

                ToastService.Show(this.Page, "Certificado actualizado correctamente.", ToastService.ToastType.Info);
            }
            catch (InvalidOperationException ex)
            {
                ToastService.Show(this.Page, ex.Message, ToastService.ToastType.Error);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[LegitimosEF] Error al actualizar el certificado: " + ex);
                ToastService.Show(this.Page, "Error al actualizar el certificado. Intente nuevamente.", ToastService.ToastType.Error);
            }
        }
    }
}
