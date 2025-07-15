using Dominio;
using Dominio.DTO;
using Negocio;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebForms.CustomControls;

namespace WebForms
{
    public partial class CertificadosAdminEF : Page
    {
        // Se instancian las clases de negocio de EF.
        CertificadoNegocioEF negocio = new CertificadoNegocioEF();
        CalculoRedeterminacionNegocioEF calculoRedeterminacionNegocio = new CalculoRedeterminacionNegocioEF();
        ExpedienteReliqNegocioEF negocioReliq = new ExpedienteReliqNegocioEF();

        protected void Page_PreRender(object sender, EventArgs e)
        {
            rfvAutorizante.Enabled = Session["EditingCertificadoId"] == null;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarGrillaCompleta();
                BindDropDownList();
            }
        }

        public void OnAcceptChanges(object sender, EventArgs e)
        {
            dgvCertificado.PageIndex = 0;
            BindGrid();
        }

        protected void btnExportarExcel_Click(object sender, EventArgs e)
        {
            try
            {
                // CORRECCIÓN: Se utiliza la misma fuente de datos que la grilla (Session) para asegurar consistencia.
                if (Session["GridData"] == null) return;
                var datosParaExportar = (List<CertificadoDTO>)Session["GridData"];

                // Se aplican los mismos filtros que tiene la grilla.
                string filtro = txtBuscar.Text.Trim().ToLower();
                if (!string.IsNullOrEmpty(filtro))
                {
                    datosParaExportar = datosParaExportar.Where(c =>
                        (c.ExpedientePago?.ToLower().Contains(filtro) ?? false) ||
                        (c.EmpresaNombre?.ToLower().Contains(filtro) ?? false) ||
                        (c.CodigoAutorizante?.ToLower().Contains(filtro) ?? false) ||
                        (c.ObraDescripcion?.ToLower().Contains(filtro) ?? false) ||
                        (c.AreaNombre?.ToLower().Contains(filtro) ?? false) ||
                        (c.BarrioNombre?.ToLower().Contains(filtro) ?? false) ||
                        (c.TipoPagoNombre?.ToLower().Contains(filtro) ?? false)
                    ).ToList();
                }
                datosParaExportar = AplicarFiltrosTreeViewEnMemoria(datosParaExportar);

                // Se define el mapeo de columnas.
                var mapeoColumnas = new Dictionary<string, string>
                {
                    { "Área", "AreaNombre" },
                    { "Obra", "ObraDescripcion" },
                    { "Barrio", "BarrioNombre" },
                    { "Proyecto", "ProyectoNombre" },
                    { "Empresa", "EmpresaNombre" },
                    { "Código Autorizante", "CodigoAutorizante" },
                    { "Expediente", "ExpedientePago" },
                    { "Estado", "Estado" },
                    { "Tipo", "TipoCertificado" }, // Se usa el tipo calculado (Certificado/Reliquidación)
                    { "Monto", "MontoTotal" },
                    { "Mes Certificado", "MesAprobacion" },
                    { "Línea", "LineaGestionNombre" }
                };

                ExcelHelper.ExportarDatosGenericos(dgvCertificado, datosParaExportar, mapeoColumnas, "Certificados");
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

            ScriptManager.RegisterStartupScript(this, this.GetType(), "ResetModalTitleAndShow", @"
            $(document).ready(function() {
            $('#modalAgregar .modal-title').text('Agregar Certificado');
            document.getElementById('" + btnAgregar.ClientID + @"').value = 'Agregar';
            
            $('#autorizanteContainer').show();
            
            $('#modalAgregar').modal('show');
            });", true);

            btnAgregar.Text = "Agregar";

            Session["EditingCertificadoId"] = null;
            Session["EditingAutorizanteId"] = null;
            Session["EditingCodigoAutorizante"] = null;
        }

        protected void btnLimpiar_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();
        }

        protected void btnAgregar_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            try
            {
                CertificadoEF certificado = new CertificadoEF
                {
                    ExpedientePago = txtExpediente.Text.Trim(),
                    MontoTotal = decimal.Parse(txtMontoAutorizado.Text.Trim()),
                    MesAprobacion = string.IsNullOrWhiteSpace(txtFecha.Text) ? (DateTime?)null : DateTime.Parse(txtFecha.Text),
                    TipoPagoId = int.Parse(ddlTipo.SelectedValue)
                };

                bool resultado;
                if (Session["EditingCertificadoId"] != null)
                {
                    certificado.Id = (int)Session["EditingCertificadoId"];
                    resultado = negocio.Modificar(certificado);
                    lblMensaje.Text = resultado ? "Certificado modificado exitosamente!" : "Hubo un problema al modificar el certificado.";
                }
                else
                {
                    certificado.CodigoAutorizante = ddlAutorizante.SelectedItem.Text;
                    resultado = negocio.Agregar(certificado);
                    lblMensaje.Text = resultado ? "Certificado agregado exitosamente!" : "Hubo un problema al agregar el certificado.";
                }

                lblMensaje.CssClass = resultado ? "alert alert-success" : "alert alert-danger";

                if (resultado)
                {
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "HideModal", "$('#modalAgregar').modal('hide');", true);
                    CargarGrillaCompleta();
                    //PoblarFiltrosHeader();
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }


        private void LimpiarFormulario()
        {
            txtExpediente.Text = string.Empty;
            txtMontoAutorizado.Text = string.Empty;
            txtFecha.Text = string.Empty;
            ddlAutorizante.SelectedIndex = 0;
            ddlTipo.SelectedIndex = 0;
        }

        /// <summary>
        /// Método central para cargar los datos desde la capa de negocio y guardarlos en Session.
        /// Se llama solo una vez en Page_Load o cuando se necesita recargar todos los datos (ej. después de agregar/eliminar).
        /// </summary>
        private void CargarGrillaCompleta()
        {
            // Se llama al método principal que devuelve una lista completa en memoria.
            var todosLosDatos = calculoRedeterminacionNegocio.ListarCertificadosYReliquidaciones();
            Session["GridData"] = todosLosDatos;

            // Se bindean los datos a la grilla por primera vez.
            BindGrid();

        }

        /// <summary>
        /// Método central para refrescar la visualización del GridView.
        /// Lee los datos desde Session, aplica los filtros y la paginación actuales.
        /// </summary>
        private void BindGrid()
        {
            if (Session["GridData"] == null) return;

            var datosEnMemoria = (List<CertificadoDTO>)Session["GridData"];

            // Aplicar filtro de texto general.
            string filtro = txtBuscar.Text.Trim().ToLower();
            if (!string.IsNullOrEmpty(filtro))
            {
                datosEnMemoria = datosEnMemoria.Where(c =>
                    (c.ExpedientePago?.ToLower().Contains(filtro) ?? false) ||
                    (c.EmpresaNombre?.ToLower().Contains(filtro) ?? false) ||
                    (c.CodigoAutorizante?.ToLower().Contains(filtro) ?? false) ||
                    (c.ObraDescripcion?.ToLower().Contains(filtro) ?? false) ||
                    (c.AreaNombre?.ToLower().Contains(filtro) ?? false) ||
                    (c.BarrioNombre?.ToLower().Contains(filtro) ?? false) ||
                    (c.TipoPagoNombre?.ToLower().Contains(filtro) ?? false)
                ).ToList();
            }

            // Aplicar filtros de las columnas (TreeView).
            datosEnMemoria = AplicarFiltrosTreeViewEnMemoria(datosEnMemoria);

            // Calcular subtotal sobre los datos ya filtrados.
            decimal subtotal = datosEnMemoria.Sum(c => c.MontoTotal);
            txtSubtotal.Text = subtotal.ToString("C", CultureInfo.GetCultureInfo("es-AR"));

            // Configurar paginación.
            dgvCertificado.VirtualItemCount = datosEnMemoria.Count;
            dgvCertificado.DataSource = datosEnMemoria
                                        .Skip(dgvCertificado.PageIndex * dgvCertificado.PageSize)
                                        .Take(dgvCertificado.PageSize)
                                        .ToList();
            dgvCertificado.DataBind();
            PoblarFiltrosHeader();
        }




        protected void dgvCertificado_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                int idCertificado = Convert.ToInt32(dgvCertificado.SelectedDataKey.Value);
                CertificadoEF certificadoSeleccionado;
                using (var context = new IVCdbContext())
                {
                    // Se busca el certificado directamente en la base de datos.
                    certificadoSeleccionado = context.Certificados.Find(idCertificado);
                }

                if (certificadoSeleccionado != null)
                {
                    Session["EditingCertificadoId"] = idCertificado;
                    txtExpediente.Text = certificadoSeleccionado.ExpedientePago;
                    txtMontoAutorizado.Text = certificadoSeleccionado.MontoTotal.ToString("0.00");
                    txtFecha.Text = certificadoSeleccionado.MesAprobacion?.ToString("yyyy-MM-dd");
                    SelectDropDownListByValue(ddlTipo, certificadoSeleccionado.TipoPagoId.ToString());

                    ScriptManager.RegisterStartupScript(this, this.GetType(), "UpdateModalAndShow", @"
                    $('#modalAgregar .modal-title').text('Modificar Certificado');
                    $('#autorizanteContainer').hide();
                    $('#modalAgregar').modal('show');", true);
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
            dropDown.ClearSelection();
            ListItem item = dropDown.Items.FindByValue(value);
            if (item != null)
            {
                item.Selected = true;
            }
        }
        protected void dgvCertificado_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            try
            {
                var id = Convert.ToInt32(dgvCertificado.DataKeys[e.RowIndex].Value);
                if (negocio.Eliminar(id))
                {
                    lblMensaje.Text = "Certificado eliminado correctamente.";
                    lblMensaje.CssClass = "alert alert-success";
                    // Se recarga toda la grilla para reflejar la eliminación.
                    CargarGrillaCompleta();
                    //PoblarFiltrosHeader();
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al eliminar el certificado: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        private void PoblarFiltrosHeader()
        {
            using (var context = new IVCdbContext())
            {
                Action<string, object, string, string> bindFilter = (controlId, dataSource, textField, valueField) =>
                {
                    if (dgvCertificado.HeaderRow?.FindControl(controlId) is TreeViewSearch control)
                    {
                        control.DataSource = dataSource;
                        control.DataTextField = textField;
                        control.DataValueField = valueField;
                        control.DataBind();
                    }
                };

                bindFilter("cblsHeaderArea", context.Areas.AsNoTracking().OrderBy(a => a.Nombre).Select(a => new { a.Id, a.Nombre }).ToList(), "Nombre", "Id");
                bindFilter("cblsHeaderObra", context.Obras.AsNoTracking().OrderBy(o => o.Descripcion).Select(o => new { o.Id, o.Descripcion }).ToList(), "Descripcion", "Id");
                bindFilter("cblsHeaderBarrio", context.Barrios.AsNoTracking().OrderBy(b => b.Nombre).Select(b => new { b.Id, b.Nombre }).ToList(), "Nombre", "Id");
                bindFilter("cblsHeaderProyecto", context.Proyectos.AsNoTracking().OrderBy(p => p.Nombre).Select(p => new { p.Id, p.Nombre }).ToList(), "Nombre", "Id");
                bindFilter("cblsHeaderEmpresa", context.Empresas.AsNoTracking().OrderBy(e => e.Nombre).Select(e => new { e.Id, e.Nombre }).ToList(), "Nombre", "Id");
                bindFilter("cblsHeaderCodigoAutorizante", context.Autorizantes.AsNoTracking().OrderBy(a => a.CodigoAutorizante).Select(a => new { a.Id, a.CodigoAutorizante }).ToList(), "CodigoAutorizante", "Id");
                bindFilter("cblsHeaderTipo", context.TiposPago.AsNoTracking().OrderBy(t => t.Nombre).Select(t => new { t.Id, t.Nombre }).ToList(), "Nombre", "Id");
                bindFilter("cblsHeaderLinea", context.LineasGestion.AsNoTracking().OrderBy(l => l.Nombre).Select(l => new { l.Id, l.Nombre }).ToList(), "Nombre", "Id");

                if (dgvCertificado.HeaderRow?.FindControl("cblsHeaderEstado") is TreeViewSearch cblsHeaderEstado)
                {
                    cblsHeaderEstado.DataSource = calculoRedeterminacionNegocio.ObtenerEstadosParaFiltro();
                    cblsHeaderEstado.DataTextField = "Nombre";
                    cblsHeaderEstado.DataValueField = "Id";
                    cblsHeaderEstado.DataBind();
                }

                if (dgvCertificado.HeaderRow?.FindControl("cblsHeaderMesCertificado") is TreeViewSearch cblsHeaderMesCertificado)
                {
                    // 1. Se consultan todos los meses de aprobación distintos desde la base de datos.
                    var meses = context.Certificados.AsNoTracking()
                        .Where(c => c.MesAprobacion.HasValue)
                        .Select(c => c.MesAprobacion.Value)
                        .Distinct()
                        .OrderBy(d => d)
                        .ToList();

                    // 2. Se asigna la lista de fechas como fuente de datos y se enlaza al control.
                    cblsHeaderMesCertificado.DataSource = meses;
                    cblsHeaderMesCertificado.DataBind();
                }
            }
        }

        // Estos métodos deben ser adaptados para usar las clases de negocio EF.
        private void ObtenerTipos()
        {
            TipoPagoNegocioEF tipoPagoNegocio = new TipoPagoNegocioEF();
            ddlTipo.DataSource = tipoPagoNegocio.Listar();
            ddlTipo.DataTextField = "Nombre";
            ddlTipo.DataValueField = "Id";
            ddlTipo.DataBind();
        }        
        
        private void ObtenerAutorizantes()
        {
            AutorizanteNegocioEF autorizanteNegocio = new AutorizanteNegocioEF();
            ddlAutorizante.DataSource = autorizanteNegocio.ListarParaDDL();
            ddlAutorizante.DataTextField = "CodigoAutorizante"; // Corregido: usar CodigoAutorizante en lugar de Nombre
            ddlAutorizante.DataValueField = "Id";
            ddlAutorizante.DataBind();
        }        
        
        private void BindDropDownList()
        {
            try
            {
                // Cargar dropdowns independientes
                ObtenerTipos();
                ObtenerAutorizantes();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en BindDropDownList: {ex.Message}");
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
                    lblMensaje.Text = "Error: No hay datos en memoria.";
                    lblMensaje.CssClass = "alert alert-danger";
                    return;
                }

                int indiceReal = (dgvCertificado.PageIndex * dgvCertificado.PageSize) + rowIndex;
                if (indiceReal < 0 || indiceReal >= datosFiltradosActuales.Count)
                {
                    lblMensaje.Text = "Error: Índice fuera del rango de datos.";
                    lblMensaje.CssClass = "alert alert-danger";
                    return;
                }

                CertificadoDTO certificado = datosFiltradosActuales[indiceReal];
                string expedienteAnterior = certificado.ExpedientePago;

                List<CertificadoDTO> listaCompleta = Session["GridData"] as List<CertificadoDTO>;

                bool resultado = false;
                
                // --- Lógica para expediente vacío ---
                if (string.IsNullOrWhiteSpace(nuevoExpediente))
                {

                    if (certificado.TipoPagoId != 3 && certificado.Id > 0) // Certificado normal
                    {
                        // Usar la capa de negocio para certificados normales
                        var negocio = new CertificadoNegocioEF();
                        CertificadoEF entidad = negocio.ObtenerPorId(certificado.Id);

                        if (entidad != null)
                        {
                            entidad.ExpedientePago = string.Empty;
                            resultado = negocio.Modificar(entidad);
                        }

                        if (listaCompleta != null)
                        {
                            CertificadoDTO certificadoEnListaCompleta = listaCompleta.FirstOrDefault(c => c.Id == certificado.Id);
                            if (certificadoEnListaCompleta != null)
                            {
                                certificadoEnListaCompleta.ExpedientePago = nuevoExpediente;
                                certificadoEnListaCompleta.Estado = "NO INICIADO";
                                certificadoEnListaCompleta.Sigaf = null;
                                certificadoEnListaCompleta.BuzonSade = null;
                                certificadoEnListaCompleta.FechaSade = null;
                            }
                        }


                    }
                    else if (certificado.TipoPagoId == 3) // Reliquidación
                    {
                        // Usar la capa de negocio para reliquidaciones
                        var negocioReliq = new ExpedienteReliqNegocioEF();
                        resultado = negocioReliq.GuardarOActualizar(certificado.CodigoAutorizante, certificado.MesAprobacion.Value, "");

                        if (listaCompleta != null)
                        {
                            // certEnListaCompleta = listaCompleta.FirstOrDefault(c => c.IdReliquidacion == certModificado.IdReliquidacion);
                            // encontrar el id correcto para editar el certificado

                            var reliquidacionEnListaCompleta = datosFiltradosActuales.FirstOrDefault(c => c.IdReliquidacion == certificado.IdReliquidacion);
                            if (reliquidacionEnListaCompleta != null)
                            {
                                reliquidacionEnListaCompleta.ExpedientePago = nuevoExpediente;
                                reliquidacionEnListaCompleta.Estado = "NO INICIADO";
                                reliquidacionEnListaCompleta.Sigaf = null;
                                reliquidacionEnListaCompleta.BuzonSade = null;
                                reliquidacionEnListaCompleta.FechaSade = null;
                            }
                        }


                    }


                    if (resultado)
                    {
                        lblMensaje.Text = "Expediente eliminado correctamente.";
                        lblMensaje.CssClass = "alert alert-info";
                    }
                    else
                    {
                        lblMensaje.Text = "No se detectaron cambios para guardar.";
                        lblMensaje.CssClass = "alert alert-warning";
                    }

                }
                // --- Fin lógica expediente vacío ---


                // Lógica para expediente no vacío
                else
                {
                    certificado.ExpedientePago = nuevoExpediente;

                    if (certificado.TipoPagoId == 3) // Reliquidación
                    {
                        if (!certificado.MesAprobacion.HasValue)
                        {
                            lblMensaje.Text = "Error: La reliquidación no tiene fecha de aprobación.";
                            lblMensaje.CssClass = "alert alert-danger";
                            return;
                        }
                        var negocioReliq = new ExpedienteReliqNegocioEF();
                        resultado = negocioReliq.GuardarOActualizar(certificado.CodigoAutorizante, certificado.MesAprobacion.Value, nuevoExpediente);
                    }
                    else // Certificado normal
                    {
                        if (certificado.Id > 0)
                        {
                            var negocio = new CertificadoNegocioEF();
                            var entidad = negocio.ObtenerPorId(certificado.Id);
                            if (entidad != null)
                            {
                                entidad.ExpedientePago = nuevoExpediente;
                                resultado = negocio.Modificar(entidad);
                            }
                        }
                    }
                }
            

                if (resultado)
                {
                    // Lista de expedientes afectados (anterior y nuevo)
                    var expedientesAfectados = new List<string>();

                    if (!string.IsNullOrWhiteSpace(expedienteAnterior))
                        expedientesAfectados.Add(expedienteAnterior);

                    if (!string.IsNullOrWhiteSpace(nuevoExpediente))
                        expedientesAfectados.Add(nuevoExpediente);




                    // Recalcular todos los certificados afectados y guardar en BD
                     calculoRedeterminacionNegocio.CalcularCertificadosPorExpedientes(
                        expedientesAfectados, 
                        listaCompleta,
                        persistirEnBD: false);

                    // AQUI DEBEMOS ACTUALIZAR EL Session GridData con los certificados recalculados

                    if (listaCompleta != null && datosFiltradosActuales != null)
                    {
                        foreach (var certModificado in datosFiltradosActuales)
                        {
                            CertificadoDTO certEnListaCompleta = null;
                            if (certModificado.TipoPagoId == 3) // Reliquidación
                            {
                                certEnListaCompleta = listaCompleta.FirstOrDefault(c =>
                                    c.IdReliquidacion == certModificado.IdReliquidacion);
                            }
                            else // Certificado normal
                            {
                                certEnListaCompleta = listaCompleta.FirstOrDefault(c => c.Id == certModificado.Id);
                            }

                            if (certEnListaCompleta != null)
                            {
                                certEnListaCompleta.Sigaf = certModificado.Sigaf;
                            }
                        }
                    }

                    BindGrid();

                    string tipoRegistro = certificado.TipoPagoId == 3 ? "reliquidación" : "certificado";
                    lblMensaje.Text = $"Expediente de {tipoRegistro} actualizado correctamente.";
                    lblMensaje.CssClass = "alert alert-info";
                }
                else
                {
                    lblMensaje.Text = "Error al actualizar el expediente.";
                    lblMensaje.CssClass = "alert alert-danger";
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al actualizar el expediente: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        /// <summary>
        /// Método auxiliar para obtener los datos filtrados que se están mostrando actualmente en el grid.
        /// Replica la misma lógica de filtrado que se usa en BindGrid().
        /// </summary>
        private List<CertificadoDTO> ObtenerDatosFiltradosActuales()
        {
            if (Session["GridData"] == null) return new List<CertificadoDTO>();

            var datosEnMemoria = (List<CertificadoDTO>)Session["GridData"];

            // Aplicar filtro de texto general (mismo que en BindGrid)
            string filtro = txtBuscar.Text.Trim().ToLower();
            if (!string.IsNullOrEmpty(filtro))
            {
                datosEnMemoria = datosEnMemoria.Where(c =>
                    (c.ExpedientePago?.ToLower().Contains(filtro) ?? false) ||
                    (c.EmpresaNombre?.ToLower().Contains(filtro) ?? false) ||
                    (c.CodigoAutorizante?.ToLower().Contains(filtro) ?? false) ||
                    (c.ObraDescripcion?.ToLower().Contains(filtro) ?? false) ||
                    (c.AreaNombre?.ToLower().Contains(filtro) ?? false) ||
                    (c.BarrioNombre?.ToLower().Contains(filtro) ?? false) ||
                    (c.TipoPagoNombre?.ToLower().Contains(filtro) ?? false)
                ).ToList();
            }

            // Aplicar filtros de las columnas (TreeView) - mismo que en BindGrid
            datosEnMemoria = AplicarFiltrosTreeViewEnMemoria(datosEnMemoria);

            return datosEnMemoria;
        }


        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            //string filtro = txtBuscar.Text.Trim();
            dgvCertificado.PageIndex = 0; // Reiniciar a la primera página al aplicar un filtro
            BindGrid();
        }


        protected void BtnClearFilters_Click(object sender, EventArgs e)
        {
            txtBuscar.Text = string.Empty;
            TreeViewSearch.ClearAllFiltersOnPage(this.Page);
            dgvCertificado.PageIndex = 0;
            BindGrid();
        }

        protected void dgvCertificado_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            dgvCertificado.PageIndex = e.NewPageIndex;
            BindGrid();
        }

        private List<CertificadoDTO> AplicarFiltrosTreeViewEnMemoria(List<CertificadoDTO> data)
        {
            if (dgvCertificado.HeaderRow == null) return data;

            try
            {
                // Función auxiliar para parsear IDs de forma segura.
                Func<TreeViewSearch, List<int>> getSelectedIds = (tv) => tv.ExpandedSelectedValues.Select(s => int.TryParse(s, out int id) ? id : -1).Where(id => id != -1).ToList();

                // Aplicar filtro por Área
                if (dgvCertificado.HeaderRow.FindControl("cblsHeaderArea") is TreeViewSearch cblsHeaderArea && cblsHeaderArea.ExpandedSelectedValues.Any())
                    data = data.Where(c => c.AreaId.HasValue && getSelectedIds(cblsHeaderArea).Contains(c.AreaId.Value)).ToList();

                // Aplicar filtro por Obra
                if (dgvCertificado.HeaderRow.FindControl("cblsHeaderObra") is TreeViewSearch cblsHeaderObra && cblsHeaderObra.ExpandedSelectedValues.Any())
                    data = data.Where(c => c.ObraId.HasValue && getSelectedIds(cblsHeaderObra).Contains(c.ObraId.Value)).ToList();

                // Aplicar filtro por Barrio
                if (dgvCertificado.HeaderRow.FindControl("cblsHeaderBarrio") is TreeViewSearch cblsHeaderBarrio && cblsHeaderBarrio.ExpandedSelectedValues.Any())
                    data = data.Where(c => c.BarrioId.HasValue && getSelectedIds(cblsHeaderBarrio).Contains(c.BarrioId.Value)).ToList();

                // Aplicar filtro por Proyecto
                if (dgvCertificado.HeaderRow.FindControl("cblsHeaderProyecto") is TreeViewSearch cblsHeaderProyecto && cblsHeaderProyecto.ExpandedSelectedValues.Any())
                    data = data.Where(c => c.ProyectoId.HasValue && getSelectedIds(cblsHeaderProyecto).Contains(c.ProyectoId.Value)).ToList();

                // Aplicar filtro por Empresa
                if (dgvCertificado.HeaderRow.FindControl("cblsHeaderEmpresa") is TreeViewSearch cblsHeaderEmpresa && cblsHeaderEmpresa.ExpandedSelectedValues.Any())
                    data = data.Where(c => c.EmpresaId.HasValue && getSelectedIds(cblsHeaderEmpresa).Contains(c.EmpresaId.Value)).ToList();

                // Aplicar filtro por Código Autorizante
                if (dgvCertificado.HeaderRow.FindControl("cblsHeaderCodigoAutorizante") is TreeViewSearch cblsHeaderCodigoAutorizante && cblsHeaderCodigoAutorizante.ExpandedSelectedValues.Any())
                    data = data.Where(c => c.AutorizanteId.HasValue && getSelectedIds(cblsHeaderCodigoAutorizante).Contains(c.AutorizanteId.Value)).ToList();

                // Aplicar filtro por Estado
                if (dgvCertificado.HeaderRow.FindControl("cblsHeaderEstado") is TreeViewSearch cblsHeaderEstado && cblsHeaderEstado.ExpandedSelectedValues.Any())
                {
                    var selectedGroupIds = getSelectedIds(cblsHeaderEstado);
                    data = data.Where(c => selectedGroupIds.Contains(calculoRedeterminacionNegocio.MapearEstadoDesdeId(c.EstadoRedetId).Id)).ToList();
                }

                // Aplicar filtro por Tipo
                if (dgvCertificado.HeaderRow.FindControl("cblsHeaderTipo") is TreeViewSearch cblsHeaderTipo && cblsHeaderTipo.ExpandedSelectedValues.Any())
                    data = data.Where(c => c.TipoPagoId.HasValue && getSelectedIds(cblsHeaderTipo).Contains(c.TipoPagoId.Value)).ToList();

                // Aplicar filtro por Mes Certificado
                if (dgvCertificado.HeaderRow.FindControl("cblsHeaderMesCertificado") is TreeViewSearch cblsHeaderMesCertificado && cblsHeaderMesCertificado.SelectedValues.Any())
                {
                    // CORRECCIÓN: Se reemplaza DbFunctions.TruncateTime por .Date, que es el equivalente para LINQ to Objects.
                    var selectedDates = cblsHeaderMesCertificado.SelectedValues
                        .Select(s => DateTime.TryParse(s, out DateTime dt) ? (DateTime?)dt.Date : null)
                        .Where(d => d.HasValue).Select(d => d.Value)
                        .ToList();
                    data = data.Where(c => c.MesAprobacion.HasValue && selectedDates.Contains(c.MesAprobacion.Value.Date)).ToList();
                }

                // Aplicar filtro por Línea de Gestión
                if (dgvCertificado.HeaderRow.FindControl("cblsHeaderLinea") is TreeViewSearch cblsHeaderLinea && cblsHeaderLinea.ExpandedSelectedValues.Any())
                    data = data.Where(c => c.LineaGestionId.HasValue && getSelectedIds(cblsHeaderLinea).Contains(c.LineaGestionId.Value)).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en AplicarFiltrosTreeViewEnMemoria: {ex.Message}");
            }

            return data;
        }


    }
}
