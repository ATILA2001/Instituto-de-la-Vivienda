using Dominio;
using Dominio.DTO;
using Negocio;
using Negocio.Redeterminaciones;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebForms.CustomControls;
using WebForms.Infraestructura;

namespace WebForms
{
    /// <summary>
    /// Autorizantes y redeterminaciones en una misma grilla. La paginación, el filtro de texto, los filtros
    /// de cabecera y la exportación vienen de GridPageBase; acá queda lo propio: qué se carga, cómo se
    /// filtra, el alta y la edición por modal, la baja y la edición inline de expediente y estado.
    /// Las filas de redeterminación son virtuales (IdRedeterminacion > 0) y no se editan desde acá.
    /// </summary>
    public partial class AutorizantesEF : GridPageBase<AutorizanteDTO>
    {
        private readonly AutorizanteNegocioEF _autorizanteNegocioEF = new AutorizanteNegocioEF();
        private readonly CalculoRedeterminacionNegocioEF _calculoRedeterminacionNegocioEF = new CalculoRedeterminacionNegocioEF();
        private readonly DatosExternosNegocioEF _datosExternos = new DatosExternosNegocioEF();

        private bool? _puedeEditarSeguimiento;
        /// <summary>Evaluado una sola vez por request: la regla consulta Session y la base.</summary>
        private bool PuedeEditarSeguimiento =>
            _puedeEditarSeguimiento ?? (_puedeEditarSeguimiento = UserHelper.PuedeEditarInline()).Value;

        private List<EstadoAutorizanteEF> _estadosAutorizante;
        /// <summary>Catálogo de estados, leído una sola vez por request (se usa en cada fila de la grilla).</summary>
        private List<EstadoAutorizanteEF> EstadosAutorizante =>
            _estadosAutorizante ?? (_estadosAutorizante = new EstadoAutorizanteNegocioEF().Listar());

        // ------------------------------------------------------------------
        // GridPageBase
        // ------------------------------------------------------------------

        protected override GridView Grilla => gridviewRegistros;
        protected override TextBox CajaBusqueda => txtBuscar;
        protected override PaginationControl Paginador => paginationControl;
        protected override string ClaveSesion => "GridDataAutorizantes";
        protected override int IdDe(AutorizanteDTO item) => item.Id;
        protected override string NombreClaveVirtual => "IdRedeterminacion";
        protected override string ClaveVirtualDe(AutorizanteDTO item) =>
            item.IdRedeterminacion > 0 ? item.IdRedeterminacion.ToString() : null;
        protected override string MensajeErrorCarga => "No se pudieron cargar los autorizantes. Intente nuevamente.";

        protected override List<AutorizanteDTO> CargarListaCompleta()
        {
            // Redeterminaciones ve todos los autorizantes sin filtro de área: null omite el filtro.
            UsuarioEF usuario = UserHelper.IsUserInArea(IvcAreaIds.Redeterminaciones) ? null : UserHelper.GetFullCurrentUser();
            return _calculoRedeterminacionNegocioEF.ListarAutorizantesYRedeterminaciones(usuario);
        }

        protected override bool CoincideTexto(AutorizanteDTO a, string filtro) =>
            (a.CodigoAutorizante?.ToUpper().Contains(filtro) ?? false) ||
            (a.Detalle?.ToUpper().Contains(filtro) ?? false) ||
            (a.Expediente?.ToUpper().Contains(filtro) ?? false) ||
            (a.EmpresaNombre?.ToUpper().Contains(filtro) ?? false) ||
            (a.ObraDescripcion?.ToUpper().Contains(filtro) ?? false) ||
            (a.AreaNombre?.ToUpper().Contains(filtro) ?? false) ||
            (a.BarrioNombre?.ToUpper().Contains(filtro) ?? false) ||
            (a.ConceptoNombre?.ToUpper().Contains(filtro) ?? false) ||
            (a.EstadoNombre?.ToUpper().Contains(filtro) ?? false);

        protected override List<AutorizanteDTO> AplicarFiltrosHeader(List<AutorizanteDTO> datos)
        {
            var areas = IdsSeleccionados("cblsHeaderArea");
            var obras = IdsSeleccionados("cblsHeaderObra");
            var empresas = IdsSeleccionados("cblsHeaderEmpresa");
            var estados = IdsSeleccionados("cblsHeaderEstado");
            var codigos = ValoresSeleccionados("cblsHeaderCodigoAutorizante");
            var conceptos = ValoresSeleccionados("cblsHeaderConcepto");
            // Contrata filtra por texto: el valor del nodo es el nombre y puede contener comas, por eso no se expande.
            var contratas = FiltroHeader("cblsHeaderContrata")?.SelectedValues;

            if (areas != null) datos = datos.Where(a => a.AreaId.HasValue && areas.Contains(a.AreaId.Value)).ToList();
            if (obras != null) datos = datos.Where(a => a.ObraId.HasValue && obras.Contains(a.ObraId.Value)).ToList();
            if (empresas != null) datos = datos.Where(a => a.EmpresaId.HasValue && empresas.Contains(a.EmpresaId.Value)).ToList();
            if (codigos != null) datos = datos.Where(a => !string.IsNullOrEmpty(a.CodigoAutorizante) && codigos.Contains(a.CodigoAutorizante)).ToList();
            if (estados != null) datos = datos.Where(a => a.EstadoId.HasValue && estados.Contains(a.EstadoId.Value)).ToList();

            if (conceptos != null)
            {
                // Conceptos por Id; "REDETERMINACION" es un concepto virtual que se compara por nombre.
                var conceptoIds = conceptos.Select(c => int.TryParse(c, out int id) ? id : (int?)null).Where(id => id.HasValue).Select(id => id.Value).ToList();
                bool incluirRedet = conceptos.Contains("REDETERMINACION");
                datos = datos.Where(a =>
                    (incluirRedet && a.ConceptoNombre == "REDETERMINACION") ||
                    (a.ConceptoId.HasValue && conceptoIds.Contains(a.ConceptoId.Value))).ToList();
            }

            if (contratas != null && contratas.Count > 0)
            {
                var nombres = new HashSet<string>(contratas, StringComparer.OrdinalIgnoreCase);
                datos = datos.Where(a =>
                    (!string.IsNullOrWhiteSpace(a.Contrata) && nombres.Contains(a.Contrata)) ||
                    (!string.IsNullOrWhiteSpace(a.ContrataNombre) && nombres.Contains(a.ContrataNombre))).ToList();
            }

            return datos;
        }

        protected override void PoblarFiltrosHeader(List<AutorizanteDTO> autorizantes)
        {
            if (Grilla.HeaderRow == null) return;

            // Área y obra salen del snapshot del usuario, no de la tabla completa (no mostrar otras áreas).
            BindFiltroHeader("cblsHeaderArea", FiltrosHeader.Distintos(autorizantes, a => a.AreaId, a => a.AreaNombre));
            BindFiltroHeader("cblsHeaderObra", FiltrosHeader.Distintos(autorizantes, a => a.ObraId, a => a.ObraDescripcion));

            var codigos = autorizantes
                .Select(a => new { a.CodigoAutorizante })
                .Distinct()
                .OrderBy(c => c.CodigoAutorizante)
                .ToList();
            BindFiltroHeader("cblsHeaderCodigoAutorizante", codigos, "CodigoAutorizante", "CodigoAutorizante");

            // Contrata: texto "NOMBRE NUMERO/AÑO" del DTO, como valor y como etiqueta.
            var contratas = autorizantes
                .Select(a => a.Contrata)
                .Distinct()
                .OrderBy(c => c)
                .Select(c => new { Nombre = c })
                .ToList();
            BindFiltroHeader("cblsHeaderContrata", contratas, "Nombre", "Nombre");

            BindFiltroHeader("cblsHeaderEstado", MapeoEstadoRedet.ObtenerEstadosParaFiltro());

            using (var context = new IVCdbContext())
            {
                BindFiltroHeader("cblsHeaderEmpresa", context.Empresas.AsNoTracking().OrderBy(e => e.Nombre).Select(e => new { e.Id, e.Nombre }).ToList());
                BindFiltroHeader("cblsHeaderConcepto", context.Conceptos.AsNoTracking().OrderBy(c => c.Nombre).Select(c => new { c.Id, c.Nombre }).ToList());
            }
        }

        protected override decimal Subtotal(List<AutorizanteDTO> autorizantes) => autorizantes.Sum(a => a.MontoAutorizado);

        // ------------------------------------------------------------------
        // Ciclo de vida
        // ------------------------------------------------------------------

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                ObtenerDropDownLists();
                CargarPaginaActual();
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            try
            {
                bool esRolRedet = UserHelper.IsUserInArea(IvcAreaIds.Redeterminaciones);
                bool esRolSecretaria = UserHelper.IsUserInArea(IvcAreaIds.Secretaria);
                bool isPlanningOpen = ABMPlaniNegocio.GetIsPlanningOpen() || UserHelper.GetFullCurrentUser().IsPlanningOpenOverride;
                bool canModify = !esRolRedet && !esRolSecretaria && (UserHelper.IsUserAdmin() || isPlanningOpen);

                panelShowAddButton.Visible = canModify;

                // Columnas de seguimiento SADE: ocultas sólo para el rol Redeterminaciones.
                MostrarColumnas(!esRolRedet, "MesBase", "BuzonSade", "FechaSade", "Mes Base", "Buzón SADE", "Fecha SADE");

                // Columna Acciones: oculta si no puede modificar (Redeterminaciones o planificación cerrada).
                MostrarColumnas(canModify, "Acciones");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[AutorizantesEF] Error al aplicar la visibilidad de columnas: " + ex);
            }
        }

        protected void btnExportarExcel_Click(object sender, EventArgs e)
        {
            var mapeo = new Dictionary<string, string>
            {
                { "Área", "AreaNombre" },
                { "Obra", "ObraDescripcion" },
                { "Contrata", "Contrata" },
                { "Empresa", "EmpresaNombre" },
                { "Barrio", "BarrioNombre" },
                { "Código Autorizante", "CodigoAutorizante" },
                { "Expediente", "Expediente" },
                { "Detalle", "Detalle" },
                { "Concepto", "ConceptoNombre" },
                { "Monto Autorizado", "MontoAutorizado" },
                { "Mes Aprobación", "MesAprobacion" },
                { "Mes Base", "MesBase" },
                { "Estado", "EstadoNombre" },
                { "Buzón SADE", "BuzonSade" },
                { "Fecha SADE", "FechaSade" },
                { "GEDO", "Acdir" }
            };

            ExportarExcel(mapeo, "Autorizantes");
        }

        protected void gridviewRegistros_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;

            var ddlEstadoAutorizante = e.Row.FindControl("ddlEstadoAutorizante") as DropDownList;
            var txtExpediente = e.Row.FindControl("txtExpediente") as TextBox;
            if (ddlEstadoAutorizante == null) return;

            ddlEstadoAutorizante.DataSource = EstadosAutorizante;
            ddlEstadoAutorizante.DataTextField = "Nombre";
            ddlEstadoAutorizante.DataValueField = "Id";
            ddlEstadoAutorizante.DataBind();

            var autorizante = (AutorizanteDTO)e.Row.DataItem;
            if (autorizante?.EstadoId != null && ddlEstadoAutorizante.Items.FindByValue(autorizante.EstadoId.ToString()) != null)
            {
                ddlEstadoAutorizante.SelectedValue = autorizante.EstadoId.ToString();
            }

            if (autorizante != null && autorizante.IdRedeterminacion > 0)
            {
                // Redeterminación virtual: sin acciones y con los controles inline como texto plano.
                if (e.Row.FindControl("btnEliminar") is LinkButton btnEliminar) btnEliminar.Visible = false;
                if (e.Row.FindControl("btnModificar") is LinkButton btnModificar) btnModificar.Visible = false;

                if (txtExpediente != null)
                {
                    txtExpediente.Enabled = false;
                    txtExpediente.CssClass = "form-control text-center";
                    txtExpediente.BorderStyle = BorderStyle.None;
                    txtExpediente.BackColor = Color.Transparent;
                }

                ddlEstadoAutorizante.Enabled = false;
                ddlEstadoAutorizante.CssClass = "form-control text-center";
                ddlEstadoAutorizante.BorderStyle = BorderStyle.None;
                ddlEstadoAutorizante.BackColor = Color.Transparent;
            }
            else
            {
                // Misma regla que verifican los handlers: si el usuario no puede editar, que no vea
                // el control habilitado y reciba un rechazo recién al cambiarlo.
                if (txtExpediente != null) txtExpediente.Enabled = PuedeEditarSeguimiento;
                ddlEstadoAutorizante.Enabled = PuedeEditarSeguimiento;
            }
        }

        // ------------------------------------------------------------------
        // Alta y edición por modal
        // ------------------------------------------------------------------

        private void ObtenerDropDownLists()
        {
            ObtenerObras();
            ObtenerConceptos();
            ObtenerEstados();
        }

        private void ObtenerObras()
        {
            try
            {
                List<ObraEF> obras = new ObraNegocioEF().ListarParaDDL();

                ddlObraAgregar.DataSource = obras;
                ddlObraAgregar.DataTextField = "Descripcion";
                ddlObraAgregar.DataValueField = "Id";
                ddlObraAgregar.DataBind();

                foreach (ListItem item in ddlObraAgregar.Items)
                {
                    var obra = obras.FirstOrDefault(a => a.Id.ToString() == item.Value);
                    if (obra != null)
                    {
                        item.Text = $"{TextoCombo.Acortar(obra.Descripcion)} - {obra.Barrio?.Nombre} - {obra.Empresa?.Nombre}";
                        item.Attributes["title"] = $"{obra.Descripcion} - {obra.Barrio?.Nombre} - {obra.Empresa?.Nombre}";
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[AutorizantesEF] Error al cargar las obras del formulario: " + ex);
                ToastService.Show(this.Page, "No se pudieron cargar las obras. Intente nuevamente.", ToastService.ToastType.Error);
            }
        }

        private void ObtenerConceptos()
        {
            try
            {
                List<ConceptoEF> conceptos = new ConceptoNegocioEF().Listar();

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
                System.Diagnostics.Trace.TraceError("[AutorizantesEF] Error al cargar los conceptos del formulario: " + ex);
                ToastService.Show(this.Page, "No se pudieron cargar los conceptos. Intente nuevamente.", ToastService.ToastType.Error);
            }
        }

        private void ObtenerEstados()
        {
            try
            {
                ddlEstadoAgregar.DataSource = EstadosAutorizante;
                ddlEstadoAgregar.DataTextField = "Nombre";
                ddlEstadoAgregar.DataValueField = "Id";
                ddlEstadoAgregar.DataBind();

                ddlEstadoEditar.DataSource = EstadosAutorizante;
                ddlEstadoEditar.DataTextField = "Nombre";
                ddlEstadoEditar.DataValueField = "Id";
                ddlEstadoEditar.DataBind();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[AutorizantesEF] Error al cargar los estados del formulario: " + ex);
                ToastService.Show(this.Page, "No se pudieron cargar los estados. Intente nuevamente.", ToastService.ToastType.Error);
            }
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

        protected void btnShowAddModal_Click(object sender, EventArgs e)
        {
            LimpiarFormularioAgregar();

            ScriptManager.RegisterStartupScript(this, GetType(), "ShowAddModal", @"
                $(document).ready(function() {
                    $('#modalAgregarAutorizante').modal('show');
                });", true);

            ViewState["EditingAutorizanteId"] = null;
            ViewState["EditingCodigoAutorizante"] = null;
        }

        protected void btnAgregar_Click(object sender, EventArgs e)
        {
            if (!UserHelper.PuedeModificarPlanificacion())
            {
                ToastService.Show(this.Page, "No tiene permisos para realizar esta acción.", ToastService.ToastType.Error);
                return;
            }

            if (!Page.IsValid) return;

            var autorizante = new AutorizanteEF
            {
                Expediente = txtExpedienteAgregar.Text.Trim(),
                Detalle = txtDetalleAgregar.Text.Trim(),
                MontoAutorizado = Convert.ToDecimal(txtMontoAutorizadoAgregar.Text),
                MesAprobacion = string.IsNullOrEmpty(txtMesAprobacionAgregar.Text) ? (DateTime?)null : DateTime.Parse(txtMesAprobacionAgregar.Text),
                MesBase = string.IsNullOrEmpty(txtMesBaseAgregar.Text) ? (DateTime?)null : DateTime.Parse(txtMesBaseAgregar.Text),
                ConceptoId = int.Parse(ddlConceptoAgregar.SelectedValue),
                EstadoId = int.Parse(ddlEstadoAgregar.SelectedValue),
                // Sólo al agregar se asigna la obra seleccionada.
                ObraId = int.Parse(ddlObraAgregar.SelectedValue)
            };

            if (_autorizanteNegocioEF.Agregar(autorizante))
            {
                ToastService.Show(this.Page, "Autorizante agregado exitosamente!", ToastService.ToastType.Success);
            }
            else
            {
                ToastService.Show(this.Page, "Hubo un problema al agregar el autorizante.", ToastService.ToastType.Error);
            }

            LimpiarFormularioAgregar();
            ScriptManager.RegisterStartupScript(this, this.GetType(), "HideModal", "$('#modalAgregarAutorizante').modal('hide');", true);

            CalculoRedeterminacionNegocioEF.LimpiarCacheSade();
            RecargarTrasCambio();
        }

        protected void btnEditar_Click(object sender, EventArgs e)
        {
            if (!UserHelper.PuedeModificarPlanificacion())
            {
                ToastService.Show(this.Page, "No tiene permisos para realizar esta acción.", ToastService.ToastType.Error);
                return;
            }

            if (!Page.IsValid) return;

            int? autorizanteId = ViewState["EditingAutorizanteId"] as int?;
            var autorizanteExistente = autorizanteId.HasValue ? _autorizanteNegocioEF.ObtenerPorId(autorizanteId.Value) : null;
            if (autorizanteExistente == null)
            {
                ToastService.Show(this.Page, "Error: No se encontró el autorizante a modificar.", ToastService.ToastType.Error);
                return;
            }

            autorizanteExistente.Expediente = txtExpedienteEditar.Text.Trim();
            autorizanteExistente.Detalle = txtDetalleEditar.Text.Trim();
            autorizanteExistente.MontoAutorizado = Convert.ToDecimal(txtMontoAutorizadoEditar.Text);
            autorizanteExistente.MesAprobacion = string.IsNullOrEmpty(txtMesAprobacionEditar.Text) ? (DateTime?)null : DateTime.Parse(txtMesAprobacionEditar.Text);
            autorizanteExistente.MesBase = string.IsNullOrEmpty(txtMesBaseEditar.Text) ? (DateTime?)null : DateTime.Parse(txtMesBaseEditar.Text);
            autorizanteExistente.ConceptoId = int.Parse(ddlConceptoEditar.SelectedValue);
            autorizanteExistente.EstadoId = int.Parse(ddlEstadoEditar.SelectedValue);

            try
            {
                if (_autorizanteNegocioEF.Modificar(autorizanteExistente))
                {
                    ToastService.Show(this.Page, "Autorizante modificado exitosamente!", ToastService.ToastType.Success);
                    CalculoRedeterminacionNegocioEF.LimpiarCacheSade();
                    ViewState["EditingAutorizanteId"] = null;
                }
                else
                {
                    ToastService.Show(this.Page, "No se pudo guardar el cambio. Intente nuevamente.", ToastService.ToastType.Error);
                }
            }
            catch (InvalidOperationException ex)
            {
                ToastService.Show(this.Page, ex.Message, ToastService.ToastType.Error);
                return;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[AutorizantesEF] Error al modificar el autorizante: " + ex);
                ToastService.Show(this.Page, "Ocurrió un error al guardar. Intente nuevamente.", ToastService.ToastType.Error);
                return;
            }

            LimpiarFormularioEditar();
            ScriptManager.RegisterStartupScript(this, this.GetType(), "HideModal", "$('#modalEditarAutorizante').modal('hide');", true);

            RecargarTrasCambio();
        }

        protected void gridviewRegistros_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                int id = 0;
                var valorId = gridviewRegistros.SelectedDataKey?.Values["Id"];
                if (valorId != null) int.TryParse(valorId.ToString(), out id);

                var seleccionado = id > 0 ? ListaCompleta.FirstOrDefault(a => a.Id == id) : null;
                if (seleccionado == null) return;

                txtExpedienteEditar.Text = seleccionado.Expediente;
                txtDetalleEditar.Text = seleccionado.Detalle;
                txtMontoAutorizadoEditar.Text = seleccionado.MontoAutorizado.ToString("0.00");
                txtMesAprobacionEditar.Text = seleccionado.MesAprobacion?.ToString("yyyy-MM-dd") ?? txtMesAprobacionEditar.Text;
                txtMesBaseEditar.Text = seleccionado.MesBase?.ToString("yyyy-MM-dd") ?? txtMesBaseEditar.Text;

                if (seleccionado.ConceptoId.HasValue)
                    SeleccionarValor(ddlConceptoEditar, seleccionado.ConceptoId.Value.ToString());

                if (seleccionado.EstadoId.HasValue)
                    SeleccionarValor(ddlEstadoEditar, seleccionado.EstadoId.Value.ToString());

                ViewState["EditingAutorizanteId"] = seleccionado.Id;
                ViewState["EditingCodigoAutorizante"] = seleccionado.CodigoAutorizante;

                ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowEditModal", @"
                    $(document).ready(function() {
                        $('#modalEditarAutorizante').modal('show');
                    });", true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[AutorizantesEF] Error al cargar el autorizante para edición: " + ex);
                ToastService.Show(this.Page, "No se pudieron cargar los datos del autorizante. Intente nuevamente.", ToastService.ToastType.Error);
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
                if (gridviewRegistros.DataKeys == null || gridviewRegistros.DataKeys.Count <= e.RowIndex)
                {
                    ToastService.Show(this.Page, "No se pudo identificar el registro.", ToastService.ToastType.Error);
                    return;
                }

                int id = 0;
                var valorId = gridviewRegistros.DataKeys[e.RowIndex].Values["Id"];
                if (valorId != null) int.TryParse(valorId.ToString(), out id);

                if (id <= 0)
                {
                    ToastService.Show(this.Page, "No se puede eliminar: Id inválido.", ToastService.ToastType.Error);
                    return;
                }

                if (!_autorizanteNegocioEF.Eliminar(id))
                {
                    ToastService.Show(this.Page, "No se pudo eliminar (puede tener dependencias).", ToastService.ToastType.Warning);
                    return;
                }

                ListaCompleta.RemoveAll(a => a.Id == id);
                CalculoRedeterminacionNegocioEF.LimpiarCacheSade();

                ToastService.Show(this.Page, "Autorizante eliminado correctamente.", ToastService.ToastType.Success);
                CargarPaginaActual();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[AutorizantesEF] Error al eliminar el autorizante: " + ex);
                ToastService.Show(this.Page, "No se pudo eliminar el autorizante. Intente nuevamente.", ToastService.ToastType.Error);
            }
        }

        // ------------------------------------------------------------------
        // Edición inline (expediente y estado)
        // ------------------------------------------------------------------

        protected void txtExpediente_TextChanged(object sender, EventArgs e)
        {
            if (!PuedeEditarSeguimiento)
            {
                ToastService.Show(this.Page, "No tiene permisos para realizar esta acción.", ToastService.ToastType.Error);
                CargarPaginaActual();
                return;
            }

            try
            {
                string nuevoExpediente = ((TextBox)sender).Text.Trim();

                var autorizante = ItemDelControlInline(sender);
                if (autorizante == null)
                {
                    ToastService.Show(this.Page, "Error: Índice fuera del rango de datos.", ToastService.ToastType.Error);
                    return;
                }

                if (!_autorizanteNegocioEF.ActualizarExpediente(autorizante.CodigoAutorizante, nuevoExpediente))
                {
                    ToastService.Show(this.Page, "Error al actualizar el expediente.", ToastService.ToastType.Error);
                    return;
                }

                // Reflejar en el snapshot y recalcular buzón y fecha SADE sólo para este registro.
                autorizante.Expediente = nuevoExpediente;
                if (string.IsNullOrWhiteSpace(nuevoExpediente))
                {
                    autorizante.BuzonSade = null;
                    autorizante.FechaSade = null;
                }
                else
                {
                    var pase = _datosExternos.ObtenerUltimoPaseSade(nuevoExpediente);
                    autorizante.BuzonSade = pase.Buzon;
                    autorizante.FechaSade = pase.Fecha;
                }

                CalculoRedeterminacionNegocioEF.LimpiarCacheSade();
                CargarPaginaActual();

                ToastService.Show(this.Page, "Expediente actualizado correctamente.", ToastService.ToastType.Success);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[AutorizantesEF] Error al actualizar el expediente: " + ex);
                ToastService.Show(this.Page, "No se pudo actualizar el expediente. Intente nuevamente.", ToastService.ToastType.Error);
            }
        }

        protected void ddlEstadoAutorizante_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!PuedeEditarSeguimiento)
            {
                ToastService.Show(this.Page, "No tiene permisos para realizar esta acción.", ToastService.ToastType.Error);
                CargarPaginaActual();
                return;
            }

            try
            {
                int nuevoEstadoId = int.Parse(((DropDownList)sender).SelectedValue);

                var autorizante = ItemDelControlInline(sender);
                if (autorizante == null)
                {
                    ToastService.Show(this.Page, "Error: no se pudo localizar el autorizante a actualizar.", ToastService.ToastType.Error);
                    return;
                }

                // Las filas de redeterminación muestran un estado general derivado de su etapa
                // (ESTADOS_REDET). El combo de esta grilla lista ESTADOS_AUTORIZANTES, así que su
                // valor no puede escribirse en REDETERMINACIONES.ETAPA (FK a otra tabla).
                if (autorizante.IdRedeterminacion > 0)
                {
                    ToastService.Show(this.Page, "La etapa de una redeterminación se modifica desde la página Redeterminaciones.", ToastService.ToastType.Warning);
                    CargarPaginaActual();
                    return;
                }

                bool actualizado;
                var entidad = autorizante.Id > 0 ? _autorizanteNegocioEF.ObtenerPorId(autorizante.Id) : null;
                if (entidad != null)
                {
                    entidad.EstadoId = nuevoEstadoId;
                    actualizado = _autorizanteNegocioEF.Modificar(entidad);
                }
                else
                {
                    actualizado = _autorizanteNegocioEF.ActualizarEstado(autorizante.CodigoAutorizante, nuevoEstadoId);
                }

                if (!actualizado)
                {
                    ToastService.Show(this.Page, "Error al actualizar el estado.", ToastService.ToastType.Error);
                    return;
                }

                autorizante.EstadoId = nuevoEstadoId;
                autorizante.EstadoNombre = EstadosAutorizante.FirstOrDefault(estado => estado.Id == nuevoEstadoId)?.Nombre ?? autorizante.EstadoNombre;

                ToastService.Show(this.Page, "Estado actualizado correctamente.", ToastService.ToastType.Success);
                CargarPaginaActual();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[AutorizantesEF] Error al actualizar el estado: " + ex);
                ToastService.Show(this.Page, "No se pudo actualizar el estado. Intente nuevamente.", ToastService.ToastType.Error);
            }
        }
    }
}
