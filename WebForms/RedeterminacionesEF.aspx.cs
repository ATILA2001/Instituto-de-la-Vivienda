﻿using Dominio;
using Negocio;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebForms.CustomControls;

namespace WebForms
{
    public partial class RedeterminacionesEF : System.Web.UI.Page
    {
        readonly RedeterminacionNegocioEF _negocio = new RedeterminacionNegocioEF();
        CalculoRedeterminacionNegocioEF _calculo = new CalculoRedeterminacionNegocioEF();

        protected void OnAcceptChanges(object sender, EventArgs e)
        {
            // Reiniciar a la primera página al aplicar filtros

            paginationControl.CurrentPageIndex = 0;
            paginationControl.UpdatePaginationControls();

            BindGrid(); // Cargar datos filtrados y paginados
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindDropDownList();
                BindGrid();
                Debug.WriteLine("Page_Load: First load, dropdowns and grid bound.");
            }
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            rfvAutorizante.Enabled = ViewState["EditingRedeterminacionId"] == null;
        }

        protected void btnShowAddModal_Click(object sender, EventArgs e)
        {
            // Limpiar formulario y mostrar modal (patrón similar a LegitimosEF)
            LimpiarFormulario();

            ScriptManager.RegisterStartupScript(this, this.GetType(), "ResetModalTitleAndShow", @"
                $(document).ready(function() {
                    $('#modalAgregar .modal-title').text('Agregar Redeterminación');
                    document.getElementById('" + btnAgregar.ClientID + @"').value = 'Agregar';
                    $('#autorizanteContainer').show();
                    $('#modalAgregar').modal('show');
                });", true);

            btnAgregar.Text = "Agregar";

            // Resetear estado de edición
            ViewState["EditingRedeterminacionId"] = null;
            ViewState["EditingAutorizanteId"] = null;
            ViewState["EditingCodigoAutorizante"] = null;
        }
        protected void btnLimpiar_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();
        }

        private void LimpiarFormulario()
        {
            txtExpediente.Text = string.Empty;
            txtSalto.Text = string.Empty;
            txtNro.Text = string.Empty;
            txtTipo.Text = string.Empty;
            txtPorcentaje.Text = string.Empty;
            txtObservacion.Text = string.Empty;

            if (ddlAutorizante.Items.Count > 0)
                ddlAutorizante.SelectedIndex = 0;

            if (ddlEtapa.Items.Count > 0)
                ddlEtapa.SelectedIndex = 0;
        }

        protected void BtnClearFilters_Click(object sender, EventArgs e)
        {
            txtBuscar.Text = string.Empty;
            // Limpiar filtros de cabecera
            TreeViewSearch.ClearAllFiltersOnPage(this.Page);
            ClearHeaderFilter("cblsHeaderObra");
            ClearHeaderFilter("cblsHeaderAutorizante");
            ClearHeaderFilter("cblsHeaderEstado");
            ClearHeaderFilter("cblsHeaderUsuario");
            ClearHeaderFilter("cblsHeaderEmpresa");
            ClearHeaderFilter("cblsHeaderArea");
            // Reset filtros especiales
            ViewState["BuzonFilterValue"] = "all";
            ShowOnlyMismatchedRecords = false;
            if (chkShowMismatchOnly != null)
                chkShowMismatchOnly.Checked = false;
            if (paginationControl != null)
            {
                paginationControl.CurrentPageIndex = 0;
                paginationControl.UpdatePaginationControls();
            }
            BindGrid();

            // Resetear dropdown principal de Días x Buzón (volver al primer ítem)
            if (ddlFiltroBuzon != null)
            {
                ddlFiltroBuzon.SelectedIndex = 0; // elemento cero
            }

            // Sincronizar también cliente (checkbox y dropdown)
            ScriptManager.RegisterStartupScript(this, this.GetType(), "ResetSpecialToggles", @"$(function(){ $('#" + chkShowMismatchOnly.ClientID + @"').prop('checked', false); $('#" + ddlFiltroBuzon.ClientID + @"').prop('selectedIndex',0); });", true);
            ScriptManager.RegisterStartupScript(this, this.GetType(), "SetFiltersClearedFlag", "sessionStorage.setItem('filtersCleared', 'true');", true);
        }

        private void ClearHeaderFilter(string controlId)
        {
            if (dgvRedeterminacion.HeaderRow != null)
            {
                if (dgvRedeterminacion.HeaderRow.FindControl(controlId) is TreeViewSearch control)
                {
                    control.ClearSelection();
                    string controlInstanceId = control.ID;
                    string sessionKey = $"TreeViewSearch_SelectedValues_{controlInstanceId}";
                    if (HttpContext.Current.Session[sessionKey] != null) HttpContext.Current.Session.Remove(sessionKey);
                    string contextKey = $"TreeViewSearch_{controlInstanceId}_ContextSelectedValues";
                    if (HttpContext.Current.Items.Contains(contextKey)) HttpContext.Current.Items.Remove(contextKey);
                }
            }
        }

        protected string FormatDaysWithColor(double days)
        {
            int dayCount = (int)Math.Floor(days);
            string color;

            if (dayCount < 7)
            {
                color = "green"; // Verde para menos de 7 días
            }
            else if (dayCount >= 7 && dayCount <= 14)
            {
                color = "orange"; // Amarillo para entre 7 y 14 días
            }
            else
            {
                color = "red"; // Rojo para más de 14 días
            }

            return $"<span style='color: {color}; font-weight: bold;'>{dayCount}</span>";
        }
        protected void BtnToggleMismatch_ServerChange(object sender, EventArgs e)
        {
            ShowOnlyMismatchedRecords = chkShowMismatchOnly.Checked;
            BindGrid();
        }

        protected bool ShowOnlyMismatchedRecords
        {
            get { return ViewState["ShowOnlyMismatchedRecords"] as bool? ?? false; }
            set { ViewState["ShowOnlyMismatchedRecords"] = value; }
        }
        protected bool IsBuzonEtapaMismatch(string etapaNombre, string buzonSade)
        {
            if (string.IsNullOrEmpty(etapaNombre) || string.IsNullOrEmpty(buzonSade))
                return false;

            // Diccionario de correspondencias entre etapas y buzones
            var correspondencias = new Dictionary<string, List<string>>
            {
                {"RD-01/11-Subsanacion Empresa", new List<string>{"IVC-4010 DEPTO REDETERMINACIONES"}},
                {"RD-02/11-Análisis Tecnica", new List<string>{
                    "IVC-3300 GO PLANEAMIENTO Y EVALUACIÓN",
                    "IVC-3430 DEPTO OBRAS 1",
                    "IVC-3400 GO INSPECCIION Y AUDITORIA DE OBRAS",
                    "11000",
                    "IVC-2600 GO PLANIFICACION Y CONTROL",
                    "VLMOHAREM",
                    "IVC-12400 GO LOGISTICA",
                    "IVC-3000 DG OBRAS",
                    "IVC-3420 DEPTO AUDITORIA 2",
                    "IVC-9500 GO MODERNIZACION"
        }},
        {"RD-03/11-Análisis DGAyF", new List<string>{"IVC-4010 DEPTO REDETERMINACIONES"}},
        {"RD-04/11-Dgtal-Dictamen", new List<string>{
            "IVC-5210 DEPTO OBRAS PUBLICAS",
            "IVC-5220 DEPTO SUMINISTROS Y OBRAS MENORES",
            "IVC-5200 GO ASESORAMIENTO Y CONTROL DE LEGALIDAD OBRA PUBLICA Y SUMINISTROS"
        }},
        // Mantener las demás correspondencias...
            };

            // Verificar si el buzón corresponde a la etapa
            if (correspondencias.ContainsKey(etapaNombre))
            {
                bool coincide = correspondencias[etapaNombre].Any(b => buzonSade.Contains(b));
                return !coincide; // Retorna true si NO coincide (hay mismatch)
            }

            return false;
        }

        protected void ddlFiltroBuzon_SelectedIndexChanged(object sender, EventArgs e)
        {
            DropDownList ddlFiltroBuzon = (DropDownList)sender;
            string selectedValue = ddlFiltroBuzon.SelectedValue;

            ViewState["BuzonFilterValue"] = selectedValue;
            // Reaplicar filtros y paginación
            if (paginationControl != null)
            {
                paginationControl.CurrentPageIndex = 0;
                paginationControl.UpdatePaginationControls();
            }
            BindGrid();
        }

        protected void dgvRedeterminacion_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                // Lógica existente para poblar el DropDownList de Etapas en cada fila
                DropDownList ddlEtapas = (DropDownList)e.Row.FindControl("ddlEtapas");
                if (ddlEtapas != null)
                {
                    // Modernizado: usar lista fuertemente tipada en lugar de List<EstadoRedetEF>
                    List<EstadoRedetEF> estados = ObtenerTipos(); // Devuelve List<EstadoRedetEF>
                    ddlEtapas.DataSource = estados;
                    ddlEtapas.DataTextField = "Nombre";
                    ddlEtapas.DataValueField = "Id";
                    ddlEtapas.DataBind();

                    // Establecer el valor seleccionado para el DropDownList de la fila
                    var redetItem = e.Row.DataItem as RedeterminacionEF;
                    if (redetItem != null)
                    {
                        // Preferir la navegación cargada (Etapa) y si no, usar la FK (EstadoRedetEFId)
                        string selectedEtapaValue = redetItem.Etapa != null
                            ? redetItem.Etapa.Id.ToString()
                            : redetItem.EstadoRedetEFId.ToString();

                        ListItem item = ddlEtapas.Items.FindByValue(selectedEtapaValue);
                        if (item != null)
                        {
                            ddlEtapas.ClearSelection();
                            ddlEtapas.SelectedValue = selectedEtapaValue;
                        }
                    }
                }
            }

            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                // Lógica existente para poblar el DropDownList de Usuario en cada fila
                DropDownList ddlUsuario = (DropDownList)e.Row.FindControl("ddlUsuario");
                if (ddlUsuario != null)
                {
                    List<UsuarioEF> usuarios = ObtenerUsuarios();
                    ddlUsuario.DataSource = usuarios;
                    ddlUsuario.DataTextField = "Nombre";
                    ddlUsuario.DataValueField = "Id";
                    ddlUsuario.DataBind();

                    // Añadir opción "Sin Usuario" si no existe
                    if (!ddlUsuario.Items.Cast<ListItem>().Any(x => x.Value == "-1"))
                    {
                        ddlUsuario.Items.Insert(0, new ListItem("Sin Usuario", "-1"));
                    }

                    // Establecer el valor seleccionado para el DropDownList de la fila
                    var redetItem = e.Row.DataItem as RedeterminacionEF;
                    if (redetItem != null)
                    {
                        string selectedUsuarioValue = redetItem.UsuarioId.HasValue
                            ? redetItem.UsuarioId.Value.ToString()
                            : "-1";

                        ListItem item = ddlUsuario.Items.FindByValue(selectedUsuarioValue);
                        if (item != null)
                        {
                            ddlUsuario.ClearSelection();
                            ddlUsuario.SelectedValue = selectedUsuarioValue;
                        }
                        else
                        {
                            ddlUsuario.SelectedValue = "-1";
                        }
                    }
                }
            }

            else if (e.Row.RowType == DataControlRowType.Header)
            {
                // Filtros del header se cargan en PoblarFiltrosHeader() tras el DataBind del grid
            }
        }

        private List<UsuarioEF> ObtenerUsuarios()
        {
            using (var ctx = new IVCdbContext())
            {
                var negocio = new UsuarioNegocioEF(ctx);
                return negocio.ListarDdlRedet() ?? new List<UsuarioEF>();
            }
        }

        protected void dgvRedeterminacion_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (dgvRedeterminacion.SelectedDataKey == null) return;
                int idRedet = Convert.ToInt32(dgvRedeterminacion.SelectedDataKey.Value);

                // Obtener entidad EF directamente (patrón similar a FormulacionEF: consulta puntual)
                var redet = _negocio.ObtenerPorId(idRedet);
                if (redet == null)
                {
                    ToastService.Show(this.Page, "No se encontró la redeterminación seleccionada.", ToastService.ToastType.Warning);
                    return;
                }

                ViewState["EditingRedeterminacionId"] = redet.Id;
                ViewState["EditingAutorizanteId"] = null; // No se usa directamente en EF
                ViewState["EditingCodigoAutorizante"] = redet.CodigoAutorizante;

                // Poblar controles
                txtExpediente.Text = redet.Expediente;
                txtSalto.Text = redet.Salto?.ToString("yyyy-MM-dd") ?? string.Empty;
                txtNro.Text = redet.Nro?.ToString() ?? string.Empty;
                txtTipo.Text = redet.Tipo;
                txtPorcentaje.Text = redet.Porcentaje?.ToString() ?? string.Empty;
                txtObservacion.Text = redet.Observaciones;

                if (ddlEtapa.Items.Count == 0)
                {
                    // asegurar que ddlEtapa esté cargado si por algún motivo no lo está
                    var estados = ObtenerTipos();
                    ddlEtapa.DataSource = estados;
                    ddlEtapa.DataTextField = "NOMBRE";
                    ddlEtapa.DataValueField = "ID";
                    ddlEtapa.DataBind();
                    ddlEtapa.Items.Insert(0, new ListItem("Seleccione una etapa", ""));
                }
                if (redet.EstadoRedetEFId.HasValue)
                    SelectDropDownListByValue(ddlEtapa, redet.EstadoRedetEFId.ToString());

                btnAgregar.Text = "Actualizar";

                // Mostrar modal en modo edición ocultando Autorizante
                ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowEditModal", @"
                    $(document).ready(function() {
                        $('#modalAgregar .modal-title').text('Modificar Redeterminación');
                        $('#autorizanteContainer').hide();
                        $('#modalAgregar').modal('show');
                    });", true);
            }
            catch (Exception ex)
            {
                ToastService.Show(this.Page, "Error al cargar datos: " + ex.Message, ToastService.ToastType.Error);
            }
        }

        protected void dgvRedeterminacion_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            try
            {
                var id = Convert.ToInt32(dgvRedeterminacion.DataKeys[e.RowIndex].Value);
                if (_negocio.Eliminar(id))
                {
                    ToastService.Show(this.Page, "Redeterminación eliminada correctamente.", ToastService.ToastType.Success);
                    BindGrid();
                }
            }
            catch (Exception ex)
            {
                ToastService.Show(this.Page, $"Error al eliminar la redeterminación: {ex.Message}", ToastService.ToastType.Error);
            }
        }

        private List<string> GetSelectedValues(string controlId)
        {
            if (dgvRedeterminacion.HeaderRow == null) return new List<string>();
            var ctl = dgvRedeterminacion.HeaderRow.FindControl(controlId) as TreeViewSearch;
            return ctl?.SelectedValues ?? new List<string>();
        }

        protected void btnAgregar_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            try
            {
                var redeterminacionNegocio = _negocio;

                if (ViewState["EditingRedeterminacionId"] != null)
                {
                    // Update

                    RedeterminacionEF redetExistente = redeterminacionNegocio.ObtenerPorId((int)ViewState["EditingRedeterminacionId"]);
                    //EstadoRedetEF 

                    if (redetExistente == null)
                    {
                        ToastService.Show(this.Page, "Error: No se encontró la redet a modificar.", ToastService.ToastType.Error);
                        return;
                    }

                    // Actualizar campos
                    redetExistente.Expediente = txtExpediente.Text.Trim();
                    redetExistente.Salto = string.IsNullOrWhiteSpace(txtSalto.Text)
                        ? null
                        : (DateTime?)DateTime.Parse(txtSalto.Text);
                    redetExistente.Nro = string.IsNullOrWhiteSpace(txtNro.Text)
                        ? null
                        : (int?)int.Parse(txtNro.Text);
                    redetExistente.Tipo = txtTipo.Text.Trim();
                    redetExistente.EstadoRedetEFId = int.Parse(ddlEtapa.SelectedValue);
                    redetExistente.Observaciones = txtObservacion.Text.Trim();
                    redetExistente.Porcentaje = string.IsNullOrWhiteSpace(txtPorcentaje.Text) ? (decimal?)null : decimal.Parse(txtPorcentaje.Text);


                    if (redeterminacionNegocio.Modificar(redetExistente))
                    {
                        ToastService.Show(this.Page, "Redeterminación modificada exitosamente!", ToastService.ToastType.Success);

                        ViewState["EditingRedeterminacionId"] = null;
                        ViewState["EditingAutorizanteId"] = null;
                        ViewState["EditingCodigoAutorizante"] = null;
                    }
                    else
                    {
                        ToastService.Show(this.Page, "Hubo un problema al modificar la redeterminación.", ToastService.ToastType.Error);
                    }
                }
                else
                {
                    RedeterminacionEF redetNueva = new RedeterminacionEF();

                    redetNueva.Expediente = txtExpediente.Text.Trim();
                    redetNueva.Salto = string.IsNullOrWhiteSpace(txtSalto.Text)
                        ? null
                        : (DateTime?)DateTime.Parse(txtSalto.Text);
                    redetNueva.Nro = string.IsNullOrWhiteSpace(txtNro.Text)
                        ? null
                        : (int?)int.Parse(txtNro.Text);
                    redetNueva.Tipo = txtTipo.Text.Trim();
                    redetNueva.EstadoRedetEFId = int.Parse(ddlEtapa.SelectedValue);
                    redetNueva.Observaciones = txtObservacion.Text.Trim();
                    redetNueva.Porcentaje = string.IsNullOrWhiteSpace(txtPorcentaje.Text) ? (decimal?)null : decimal.Parse(txtPorcentaje.Text);

                    redetNueva.CodigoAutorizante = new AutorizanteNegocioEF().ObtenerPorId(int.Parse(ddlAutorizante.SelectedValue)).CodigoAutorizante;

                    if (_negocio.Agregar(redetNueva))
                    {
                        ToastService.Show(this.Page, "Redeterminación agregada exitosamente!", ToastService.ToastType.Success);
                    }
                    else
                    {
                        ToastService.Show(this.Page, "Hubo un problema al agregar la redeterminación.", ToastService.ToastType.Error);
                    }
                }

                LimpiarFormulario();

                ScriptManager.RegisterStartupScript(this, this.GetType(), "ResetModalTitle",
                    "$('#modalAgregar .modal-title').text('Agregar Redeterminación');", true);
                btnAgregar.Text = "Agregar";

                ScriptManager.RegisterStartupScript(this, this.GetType(), "HideModal",
                    "$('#modalAgregar').modal('hide');", true);



                BindGrid();
            }
            catch (Exception ex)
            {
                ToastService.Show(this.Page, $"Error: {ex.Message}", ToastService.ToastType.Error);
            }
        }

        private void SelectDropDownListByValue(DropDownList dropDown, string value)
        {
            if (dropDown != null && dropDown.Items.Count > 0)
            {
                // Clear any current selection
                dropDown.ClearSelection();

                // Try to find and select the item
                ListItem item = dropDown.Items.FindByValue(value);
                if (item != null)
                {
                    item.Selected = true;
                }
            }
        }

        private List<EstadoRedetEF> ObtenerTipos()
        {
            EstadoRedetNegocioEF tipoPagNegocio = new EstadoRedetNegocioEF();
            return tipoPagNegocio.ListarParaDDL();
        }

        private List<AutorizanteEF> ObtenerAutorizantes()
        {
            AutorizanteNegocioEF autorizanteNegocio = new AutorizanteNegocioEF();
            return autorizanteNegocio.ListarParaDDL();
        }

        private void BindDropDownList()
        {
            try
            {
                // Cargar etapas
                List<EstadoRedetEF> tiposEtapas = ObtenerTipos();
                if (tiposEtapas != null && tiposEtapas.Count > 0)
                {
                    ddlEtapa.DataSource = tiposEtapas;
                    ddlEtapa.DataTextField = "NOMBRE";
                    ddlEtapa.DataValueField = "ID";
                    ddlEtapa.DataBind();

                    // Añadir opción predeterminada
                    ddlEtapa.Items.Insert(0, new ListItem("Seleccione una etapa", ""));
                }

                // Cargar autorizantes
                List<AutorizanteEF> autorizantes = ObtenerAutorizantes();
                if (autorizantes != null)
                {
                    // Usar una propiedad existente (CodigoAutorizante) para evitar el error de DataBinding sobre 'Nombre'
                    ddlAutorizante.DataSource = autorizantes;
                    ddlAutorizante.DataTextField = "CodigoAutorizante"; // antes: "Nombre" (no existe en AutorizanteEF)
                    ddlAutorizante.DataValueField = "Id";
                    ddlAutorizante.DataBind();

                    // Añadir opción predeterminada
                    ddlAutorizante.Items.Insert(0, new ListItem("Seleccione un autorizante", ""));

                    // Personalizar el texto mostrado combinando varias propiedades
                    foreach (ListItem item in ddlAutorizante.Items.Cast<ListItem>().ToList())
                    {
                        // Saltar el item placeholder
                        if (string.IsNullOrEmpty(item.Value)) continue;

                        var autorizante = autorizantes.FirstOrDefault(a => a.Id.ToString() == item.Value);
                        if (autorizante != null)
                        {
                            string obraDesc = autorizante.Obra?.Descripcion ?? "(Sin Obra)";
                            string empresaNombre = autorizante.Obra?.Empresa?.Nombre ?? "(Sin Empresa)";
                            string detalle = autorizante.Detalle ?? string.Empty;
                            item.Text = $"{autorizante.CodigoAutorizante} - {obraDesc} - {empresaNombre} - {detalle}".Trim().Trim('-').Trim();
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                ToastService.Show(this.Page, $"Error al cargar los datos iniciales: {ex.Message}", ToastService.ToastType.Error);
            }
        }



        protected void txtExpediente_TextChanged(object sender, EventArgs e)
        {
            TextBox txtExpediente = (TextBox)sender;
            GridViewRow row = (GridViewRow)txtExpediente.NamingContainer;

            int idRedeterminacion = Convert.ToInt32(dgvRedeterminacion.DataKeys[row.RowIndex].Value);
            string nuevoExpediente = txtExpediente.Text;

            try
            {
                if (_negocio.ActualizarExpediente(idRedeterminacion, nuevoExpediente))
                {

                    BindGrid();

                    ToastService.Show(this.Page, "Expediente actualizado correctamente.", ToastService.ToastType.Success);
                }
                else
                {
                    ToastService.Show(this.Page, "Error al actualizar el expediente.", ToastService.ToastType.Error);
                }
            }
            catch (Exception ex)
            {
                ToastService.Show(this.Page, "Error al actualizar el expediente: " + ex.Message, ToastService.ToastType.Error);
            }
        }

        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            // Reiniciar paginación y recargar con el filtro de texto
            if (paginationControl != null)
            {
                paginationControl.CurrentPageIndex = 0;
                paginationControl.UpdatePaginationControls();
            }
            BindGrid();
        }

        protected void ddlEtapas_SelectedIndexChanged(object sender, EventArgs e)
        {
            DropDownList ddlEtapas = (DropDownList)sender;
            GridViewRow row = (GridViewRow)ddlEtapas.NamingContainer;

            int id = int.Parse(dgvRedeterminacion.DataKeys[row.RowIndex].Value.ToString());
            int nuevaEtapaId = int.Parse(ddlEtapas.SelectedValue);

            _negocio.ActualizarEstado(id, nuevaEtapaId);
            BindGrid();

            ToastService.Show(this.Page, "Etapa actualizada correctamente.", ToastService.ToastType.Success);
        }

        protected void ddlUsuario_SelectedIndexChanged(object sender, EventArgs e)
        {
            DropDownList ddlUsuario = (DropDownList)sender;
            GridViewRow row = (GridViewRow)ddlUsuario.NamingContainer;

            int id = int.Parse(dgvRedeterminacion.DataKeys[row.RowIndex].Value.ToString());
            int? nuevoUsuarioId = ddlUsuario.SelectedValue == "-1" ? (int?)null : int.Parse(ddlUsuario.SelectedValue);

            _negocio.ActualizarUsuario(id, nuevoUsuarioId);
            BindGrid();

            ToastService.Show(this.Page, "Usuario actualizado correctamente.", ToastService.ToastType.Success);
        }


        protected void btnExportarExcel_Click(object sender, EventArgs e)
        {
            try
            {
                // Obtener datos para la exportación
                List<RedeterminacionEF> redeterminaciones;

                // Verificar si hay datos en caché
                if (Session["RedeterminacionesCompleto"] != null)
                {
                    redeterminaciones = (List<RedeterminacionEF>)Session["RedeterminacionesCompleto"];
                }
                else
                {
                    // Si no hay caché, cargamos datos frescos
                    var negocio = new RedeterminacionNegocioEF();
                    redeterminaciones = negocio.Listar();

                    // Almacenar en sesión para futuros usos
                    Session["RedeterminacionesCompleto"] = redeterminaciones;
                }

                // Definir mapeo de columnas: Nombre visible en Excel -> Propiedad en objeto
                var mapeoColumnas = new Dictionary<string, string>
        {   /*{ "Usuario", "Usuario.Nombre" },*/
            { "Código Redeterminación", "CodigoRedet" },
            { "Código Autorizante", "CodigoAutorizante" },
            { "Obra", "Autorizante.Obra.Descripcion" },
            { "Empresa", "Empresa" },
            { "Área", "Area" },
            { "Expediente", "Expediente" },
            { "Salto", "Salto" },
            { "Tipo", "Tipo" },
            { "Etapa", "Etapa.Nombre" },
            { "Porcentaje", "Porcentaje" },
            { "Observaciones", "Observaciones" },
            { "Buzón SADE", "BuzonSade" },
            { "Fecha SADE", "FechaSade" }
        };

                // Llamar al helper de exportación
                ExcelHelper.ExportarDatosGenericos(redeterminaciones, mapeoColumnas, "Redeterminaciones");

                ToastService.Show(this.Page, "Exportación completada con éxito.", ToastService.ToastType.Success);
            }
            catch (Exception ex)
            {
                ToastService.Show(this.Page, "Error al exportar: " + ex.Message, ToastService.ToastType.Error);
            }
        }

        private void BindGrid()
        {
            string filtroGeneral = txtBuscar?.Text?.Trim();
            var selObra = SafeParseIntList(GetSelectedValues("cblsHeaderObra"));
            var selAutorizante = SafeParseIntList(GetSelectedValues("cblsHeaderAutorizante"));
            var selEstado = SafeParseIntList(GetSelectedValues("cblsHeaderEstado"));
            var selUsuario = SafeParseIntList(GetSelectedValues("cblsHeaderUsuario"));
            var selEmpresa = SafeParseIntList(GetSelectedValues("cblsHeaderEmpresa"));
            var selArea = SafeParseIntList(GetSelectedValues("cblsHeaderArea"));


            string buzonFilter = ViewState["BuzonFilterValue"] as string ?? "all";
            bool mismatch = ShowOnlyMismatchedRecords;

            int pageIndex = paginationControl.CurrentPageIndex;
            int pageSize = paginationControl.PageSize;

            List<RedeterminacionEF> fullList;

            bool requiresInMemory = mismatch || buzonFilter != "all" || selEmpresa.Count > 0 || selArea.Count > 0;
            if (requiresInMemory)
            {
                // Cargar lista completa filtrada por criterios básicos desde BD
                fullList = _negocio.Listar(selEstado, selAutorizante, selObra, filtroGeneral);

                // Aplicar filtro de usuario si es necesario
                if (selUsuario.Count > 0)
                {
                    fullList = fullList.Where(r => r.UsuarioId.HasValue && selUsuario.Contains(r.UsuarioId.Value)).ToList();
                }

                // Aplicar filtro de empresa si es necesario
                if (selEmpresa.Count > 0)
                {
                    fullList = fullList.Where(r => r.Autorizante?.Obra?.EmpresaId.HasValue == true &&
                                                 selEmpresa.Contains(r.Autorizante.Obra.EmpresaId.Value)).ToList();
                }

                // Aplicar filtro de área si es necesario
                if (selArea.Count > 0)
                {
                    fullList = fullList.Where(r => r.Autorizante?.Obra?.AreaId.HasValue == true &&
                                                 selArea.Contains(r.Autorizante.Obra.AreaId.Value)).ToList();
                }

                // Aplicar filtros especiales (mismatch / buzón)
                fullList = AplicarFiltrosEspeciales(fullList, buzonFilter, mismatch);

                int total = fullList.Count;
                if (pageIndex * pageSize >= Math.Max(1, total))
                {
                    pageIndex = 0;
                    paginationControl.CurrentPageIndex = 0;
                }
                paginationControl.TotalRecords = total;
                paginationControl.UpdatePaginationControls();
                paginationControl.SubtotalText = $"Total: {total} registros";

                var pagina = fullList
                    .Skip(pageIndex * pageSize)
                    .Take(pageSize)
                    .ToList();

                dgvRedeterminacion.DataSource = pagina;
                dgvRedeterminacion.DataBind();
            }
            else
            {
                // Usar paginación en BD
                int total = _negocio.ContarConFiltros(filtroGeneral, selObra, selAutorizante, selEstado, selUsuario);
                if (pageIndex * pageSize >= Math.Max(1, total))
                {
                    pageIndex = 0;
                    paginationControl.CurrentPageIndex = 0;
                }
                paginationControl.TotalRecords = total;
                paginationControl.UpdatePaginationControls();
                paginationControl.SubtotalText = $"Total: {total} registros";

                var pagina = _negocio.ListarPaginadoConFiltros(pageIndex, pageSize, filtroGeneral, selObra, selAutorizante, selEstado, selUsuario);
                dgvRedeterminacion.DataSource = pagina;
                dgvRedeterminacion.DataBind();
            }
            PoblarFiltrosHeader();
        }

        private void PoblarFiltrosHeader()
        {
            if (dgvRedeterminacion.HeaderRow == null) return;

            using (var context = new IVCdbContext())
            {
                Action<string, object, string, string> bindFilter = (controlId, dataSource, textField, valueField) =>
                {
                    if (dgvRedeterminacion.HeaderRow?.FindControl(controlId) is TreeViewSearch control)
                    {
                        control.DataSource = dataSource;
                        control.DataTextField = textField;
                        control.DataValueField = valueField;
                        control.DataBind();
                        control.AcceptChanges += OnAcceptChanges;
                    }
                };

                // Obra: Id, Descripcion
                bindFilter("cblsHeaderObra",
                    context.Obras.AsNoTracking().OrderBy(o => o.Descripcion).Select(o => new { o.Id, o.Descripcion }).ToList(),
                    "Descripcion", "Id");

                // Autorizante: Id, CodigoAutorizante
                bindFilter("cblsHeaderAutorizante",
                    context.Autorizantes.AsNoTracking().OrderBy(a => a.CodigoAutorizante).Select(a => new { a.Id, a.CodigoAutorizante }).ToList(),
                    "CodigoAutorizante", "Id");

                // Estado (Etapa Redet): Id, Nombre
                bindFilter("cblsHeaderEstado",
                    context.EstadosRedet.AsNoTracking().OrderBy(e => e.Nombre).Select(e => new { e.Id, e.Nombre }).ToList(),
                    "Nombre", "Id");

                // Usuario: Id, Nombre
                bindFilter("cblsHeaderUsuario",
           context.Usuarios.AsNoTracking()
               .Where(u => u.Estado && u.AreaId == 16)  // Solo usuarios activos del área 16
               .OrderBy(u => u.Nombre)
               .Select(u => new { u.Id, u.Nombre })
               .ToList(),
           "Nombre", "Id");//nuevo bind

                // Empresa: Id, Nombre
                bindFilter("cblsHeaderEmpresa",
                    context.Empresas.AsNoTracking()
                        .OrderBy(e => e.Nombre)
                        .Select(e => new { e.Id, e.Nombre })
                        .ToList(),
                    "Nombre", "Id");

                // Area: Id, Nombre
                bindFilter("cblsHeaderArea",
                    context.Areas.AsNoTracking()
                        .OrderBy(a => a.Nombre)
                        .Select(a => new { a.Id, a.Nombre })
                        .ToList(),
                    "Nombre", "Id");

                // Reaplicar selección del dropdown de Días x Buzón si existe en el header
                if (dgvRedeterminacion.HeaderRow.FindControl("ddlFiltroBuzon") is DropDownList ddlBuzon && ViewState["BuzonFilterValue"] != null)
                {
                    ddlBuzon.SelectedValue = ViewState["BuzonFilterValue"].ToString();
                }
            }
        }


        private List<int> SafeParseIntList(List<string> raw)
        {
            var list = new List<int>();
            foreach (var s in raw)
            {
                if (int.TryParse(s, out int v)) list.Add(v);
            }
            return list;
        }

        private List<RedeterminacionEF> AplicarFiltrosEspeciales(List<RedeterminacionEF> lista, string buzonFilter, bool mismatch)
        {
            if (lista == null) return new List<RedeterminacionEF>();
            IEnumerable<RedeterminacionEF> q = lista;

            // Filtro de mismatch (control de estados)
            if (mismatch)
            {
                q = q.Where(r => IsBuzonEtapaMismatch(r.Etapa?.Nombre, r.BuzonSade));
            }

            // Filtro por días en buzón
            if (buzonFilter != "all")
            {
                List<int> etapasExcluidas = new List<int> { 12, 22, 33, 34, 35, 36, 37, 38, 39 };
                if (buzonFilter == "0")
                {
                    q = q.Where(r => r.FechaSade.HasValue && r.Etapa != null && !etapasExcluidas.Contains(r.Etapa.Id) && (DateTime.Now - r.FechaSade.Value).TotalDays < 7);
                }
                else if (buzonFilter == "1")
                {
                    q = q.Where(r => r.FechaSade.HasValue && r.Etapa != null && !etapasExcluidas.Contains(r.Etapa.Id) && (DateTime.Now - r.FechaSade.Value).TotalDays >= 7 && (DateTime.Now - r.FechaSade.Value).TotalDays <= 14);
                }
                else if (buzonFilter == "2")
                {
                    q = q.Where(r => r.FechaSade.HasValue && r.Etapa != null && !etapasExcluidas.Contains(r.Etapa.Id) && (DateTime.Now - r.FechaSade.Value).TotalDays > 14);
                }
            }

            return q.ToList();
        }

        // Eliminado: el poblamiento de filtros de header se realiza en PoblarFiltrosHeader()

        protected void paginationControl_PageChanged(object sender, PaginationEventArgs e)
        {
            paginationControl.CurrentPageIndex = e.PageIndex;
            paginationControl.UpdatePaginationControls();
            BindGrid();
        }

        protected void paginationControl_PageSizeChanged(object sender, PaginationEventArgs e)
        {
            paginationControl.CurrentPageIndex = 0;
            paginationControl.UpdatePaginationControls();
            BindGrid();
        }

    }
}