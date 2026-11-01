using Dominio;
using Dominio.DTO;
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
    /// Grilla de obras. La paginación, el filtro de texto, los filtros de cabecera y la exportación
    /// vienen de GridPageBase; acá queda lo propio de obras: qué se carga, cómo se filtra, el alta,
    /// la edición y la baja.
    /// </summary>
    public partial class ObrasEF : GridPageBase<ObraDTO>
    {
        private readonly ObraNegocioEF _negocio = new ObraNegocioEF();

        private bool IsPlanningOpen =>
            ABMPlaniNegocio.GetIsPlanningOpen() || UserHelper.GetFullCurrentUser().IsPlanningOpenOverride;

        // ------------------------------------------------------------------
        // GridPageBase
        // ------------------------------------------------------------------

        protected override GridView Grilla => dgvObra;
        protected override TextBox CajaBusqueda => txtBuscar;
        protected override PaginationControl Paginador => paginationControl;
        protected override string ClaveSesion => "ObrasCompleto";
        protected override string MensajeErrorCarga => "Error al cargar las obras. Intente nuevamente.";

        protected override List<ObraDTO> CargarListaCompleta()
        {
            // Admin y Redeterminaciones ven todas las obras; el resto, las de sus áreas.
            if (UserHelper.IsUserAdmin() || UserHelper.IsUserInArea(IvcAreaIds.Redeterminaciones))
                return _negocio.ListarTodo();

            return _negocio.ListarPorAreaIds(UserHelper.GetFullCurrentUser().IvcAreaIds);
        }

        protected override bool CoincideTexto(ObraDTO o, string filtro) =>
            (o.Area?.ToUpper().Contains(filtro) ?? false) ||
            (o.Empresa?.ToUpper().Contains(filtro) ?? false) ||
            (o.Contrata?.ToUpper().Contains(filtro) ?? false) ||
            (o.Barrio?.ToUpper().Contains(filtro) ?? false) ||
            (o.Descripcion?.ToUpper().Contains(filtro) ?? false) ||
            (o.LineaGestionNombre?.ToUpper().Contains(filtro) ?? false) ||
            (o.ProyectoNombre?.ToUpper().Contains(filtro) ?? false) ||
            (o.Numero?.ToString().Contains(filtro) ?? false) ||
            (o.Anio?.ToString().Contains(filtro) ?? false);

        protected override List<ObraDTO> AplicarFiltrosHeader(List<ObraDTO> datos)
        {
            var areas = IdsSeleccionados("cblsHeaderArea");
            var empresas = IdsSeleccionados("cblsHeaderEmpresa");
            var barrios = IdsSeleccionados("cblsHeaderBarrio");

            if (areas != null) datos = datos.Where(o => o.AreaId.HasValue && areas.Contains(o.AreaId.Value)).ToList();
            if (empresas != null) datos = datos.Where(o => o.EmpresaId.HasValue && empresas.Contains(o.EmpresaId.Value)).ToList();
            if (barrios != null) datos = datos.Where(o => o.BarrioId.HasValue && barrios.Contains(o.BarrioId.Value)).ToList();

            return datos;
        }

        protected override void PoblarFiltrosHeader(List<ObraDTO> obras)
        {
            // Las opciones salen del snapshot del usuario, no de las tablas completas (no mostrar otras áreas).
            BindFiltroHeader("cblsHeaderArea", FiltrosHeader.Distintos(obras, o => o.AreaId, o => o.Area));
            BindFiltroHeader("cblsHeaderEmpresa", FiltrosHeader.Distintos(obras, o => o.EmpresaId, o => o.Empresa));
            BindFiltroHeader("cblsHeaderBarrio", FiltrosHeader.Distintos(obras, o => o.BarrioId ?? 0, o => o.Barrio));
        }

        protected override decimal Subtotal(List<ObraDTO> obras) =>
            obras.Sum(o => o.MontoCertificado).GetValueOrDefault();

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

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            bool esRolRedet = UserHelper.IsUserInArea(IvcAreaIds.Redeterminaciones);
            bool esRolSecretaria = UserHelper.IsUserInArea(IvcAreaIds.Secretaria);
            bool canModify = !esRolRedet && !esRolSecretaria && (UserHelper.IsUserAdmin() || IsPlanningOpen);

            panelShowAddButton.Visible = canModify;

            // Columnas financieras: ocultas sólo para el rol Redeterminaciones.
            MostrarColumnas(!esRolRedet,
                "Autorizado2026",
                "MontoCertificado",
                "Porcentaje",
                "MontoInicial",
                "MontoActual",
                "MontoFaltante",
                "Disponible Actual",
                "Planificacion 2025",
                "Ejecucion Presupuesto 2025",
                "Monto de Obra inicial",
                "Monto de Obra actual",
                "Faltante de Obra");

            MostrarColumnas(canModify, "Acciones");
        }

        protected void btnExportarExcel_Click(object sender, EventArgs e)
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
                { "Disponible Actual", "Autorizado2026" },
                { "Monto Certificado", "MontoCertificado" },
                { "Porcentaje", "Porcentaje" },
                { "Monto Inicial", "MontoInicial" },
                { "Monto Actual", "MontoActual" },
                { "Monto Faltante", "MontoFaltante" },
                { "Fecha Inicio", "FechaInicio" },
                { "Fecha Fin", "FechaFin" }
            };

            ExportarExcel(mapeo, "ObrasEF");
        }

        // ------------------------------------------------------------------
        // Alta y edición
        // ------------------------------------------------------------------

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
                    var areasQuery = ctx.Areas.AsNoTracking().AsQueryable();

                    // Para usuarios no admin, restringir a sus áreas (IvcAreaIds), igual que ObraNegocioEF.ListarParaDDL
                    if (!UserHelper.IsUserAdmin())
                    {
                        var filtroAreaIds = UserHelper.AreaIdsParaFiltro(UserHelper.GetFullCurrentUser());
                        areasQuery = areasQuery.Where(a => filtroAreaIds.Contains(a.Id));
                    }

                    var areas = areasQuery.OrderBy(a => a.Nombre).Select(a => new { a.Id, a.Nombre }).ToList();
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
                System.Diagnostics.Trace.TraceError("[ObrasEF] Error al cargar las listas del formulario: " + ex);
                ToastService.Show(this.Page, "Error al cargar las listas. Intente nuevamente.", ToastService.ToastType.Error);
            }
        }

        protected void btnShowAddModal_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();

            if (!UserHelper.IsUserAdmin())
            {
                // El ddlArea ya viene filtrado a las áreas del usuario desde BindDropDownList (IvcAreaIds)
                var userAreaIds = UserHelper.GetFullCurrentUser().IvcAreaIds ?? new List<int>();
                ddlArea.ClearSelection();

                if (userAreaIds.Count == 1)
                {
                    // Una sola área: fijarla y bloquear
                    var item = ddlArea.Items.FindByValue(userAreaIds[0].ToString());
                    if (item != null) item.Selected = true;
                    ddlArea.Enabled = false;
                }
                else
                {
                    // Varias áreas: permitir elegir entre las disponibles
                    ddlArea.Enabled = true;
                }
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
            if (!UserHelper.PuedeModificarPlanificacion())
            {
                ToastService.Show(this.Page, "No tiene permisos para realizar esta acción.", ToastService.ToastType.Error);
                return;
            }

            if (!Page.IsValid)
            {
                var errorList = Page.Validators.Cast<IValidator>().Where(v => !v.IsValid).Select(v => v.ErrorMessage).ToList();
                ToastService.Show(this.Page, "Errores de validación: " + string.Join("; ", errorList), ToastService.ToastType.Error);
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
                    ToastService.Show(this.Page, "Obra modificada exitosamente!", ToastService.ToastType.Success);
                    Session["EditingObraId"] = null;
                }
                else
                {
                    ToastService.Show(this.Page, "Hubo un problema al modificar la obra.", ToastService.ToastType.Error);
                }
            }
            else
            {
                if (_negocio.Agregar(obra))
                {
                    ToastService.Show(this.Page, "Obra agregada exitosamente!", ToastService.ToastType.Success);
                }
                else
                {
                    ToastService.Show(this.Page, "Hubo un problema al agregar la obra.", ToastService.ToastType.Error);
                }
            }

            LimpiarFormulario();
            ScriptManager.RegisterStartupScript(this, this.GetType(), "HideModal", "$('#modalAgregar').modal('hide');", true);

            RecargarTrasCambio();
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
                ObraDTO obraSeleccionada = ItemDeFila(dgvObra.SelectedIndex);
                if (obraSeleccionada == null)
                {
                    ToastService.Show(this.Page, "Error: Registro no encontrado.", ToastService.ToastType.Error);
                    return;
                }

                // Verificar que la obra siga existiendo en la base antes de editarla
                bool existe;
                using (var context = new IVCdbContext())
                {
                    existe = context.Obras.Find(obraSeleccionada.Id) != null;
                }

                if (!existe)
                {
                    ToastService.Show(this.Page, "Error: No se pudo cargar la obra para edición.", ToastService.ToastType.Error);
                    return;
                }

                Session["EditingObraId"] = obraSeleccionada.Id;

                txtNumero.Text = obraSeleccionada.Numero?.ToString() ?? string.Empty;
                txtAnio.Text = obraSeleccionada.Anio?.ToString() ?? string.Empty;
                txtEtapa.Text = obraSeleccionada.Etapa.ToString();
                txtObraNumero.Text = obraSeleccionada.ObraNumero.ToString();
                txtDescripcion.Text = obraSeleccionada.Descripcion ?? string.Empty;

                SeleccionarValor(ddlEmpresa, obraSeleccionada.EmpresaId?.ToString());
                SeleccionarValor(ddlContrata, obraSeleccionada.ContrataId?.ToString());
                SeleccionarValor(ddlBarrio, obraSeleccionada.BarrioId?.ToString());

                if (obraSeleccionada.AreaId.HasValue)
                {
                    SeleccionarValor(ddlArea, obraSeleccionada.AreaId.Value.ToString());
                    ViewState["EditingAreaIdEF"] = obraSeleccionada.AreaId.Value;
                    ViewState["EditingAreaNombreEF"] = obraSeleccionada.Area;
                }

                btnAgregar.Text = "Actualizar";
                ddlArea.Enabled = false; // No permitir cambiar el área al editar

                ScriptManager.RegisterStartupScript(this, this.GetType(), "UpdateModalAndShow", @"
                                $(document).ready(function() {
                                    $('#modalAgregar .modal-title').text('Modificar Obra');
                                    document.getElementById('" + btnAgregar.ClientID + @"').value = 'Actualizar';
                                    $('#modalAgregar').modal('show');
                                });", true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[ObrasEF] Error al cargar la obra para edición: " + ex);
                ToastService.Show(this.Page, "Error al cargar los datos de la obra. Intente nuevamente.", ToastService.ToastType.Error);
            }
        }

        // ------------------------------------------------------------------
        // Baja
        // ------------------------------------------------------------------

        protected void dgvObra_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            if (!UserHelper.PuedeModificarPlanificacion())
            {
                e.Cancel = true;
                ToastService.Show(this.Page, "No tiene permisos para realizar esta acción.", ToastService.ToastType.Error);
                return;
            }

            try
            {
                var id = Convert.ToInt32(dgvObra.DataKeys[e.RowIndex].Value);
                if (_negocio.Eliminar(id))
                {
                    ToastService.Show(this.Page, "Obra eliminada exitosamente", ToastService.ToastType.Success);
                }
                else
                {
                    ToastService.Show(this.Page, "No fue posible eliminar la obra", ToastService.ToastType.Error);
                }

                RecargarTrasCambio();
            }
            catch (InvalidOperationException ex)
            {
                ToastService.Show(this.Page, ex.Message, ToastService.ToastType.Error);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("[ObrasEF] Error al eliminar la obra: " + ex);
                ToastService.Show(this.Page, "No se pudo eliminar la obra. Intente nuevamente.", ToastService.ToastType.Error);
            }
        }

        protected void dgvObra_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                if (e.Row.FindControl("btnEliminar") is LinkButton btnEliminar)
                {
                    btnEliminar.Visible = UserHelper.IsUserAdmin() || IsPlanningOpen;
                }
            }
        }
    }
}
