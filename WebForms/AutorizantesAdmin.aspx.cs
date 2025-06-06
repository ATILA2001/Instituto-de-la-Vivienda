using Dominio;
using Negocio;
using Negocio.Negocio;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebForms
{
    public partial class AutorizantesAdmin : System.Web.UI.Page
    {
        private AutorizanteNegocio negocio = new AutorizanteNegocio();
        private CalculoRedeterminacionNegocio calculoRedeterminacionNegocio = new CalculoRedeterminacionNegocio();

        public void OnAcceptChanges(object sender, EventArgs e)
        {
            CargarListaAutorizantesRedet();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindDropDownList();
                CargarListaAutorizantesRedet();
                CalcularSubtotal();
            }
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            // Configure validators if we're in editing mode
            if (ViewState["EditingAutorizanteId"] != null)
            {
                // Disable the Obra validator since the field is hidden
                rfvObra.Enabled = false;
            }
            else
            {
                // Enable validators in add mode
                rfvObra.Enabled = true;
            }
        }

        protected void btnShowAddModal_Click(object sender, EventArgs e)
        {
            // Clear any existing data
            LimpiarFormulario();

            // Reset the modal title and button text to "Add" and show Obra field
            ScriptManager.RegisterStartupScript(this, this.GetType(), "ResetModalTitleAndShow", @"
                $(document).ready(function() {
                    $('#modalAgregar .modal-title').text('Agregar Autorizante');
                    document.getElementById('" + Button1.ClientID + @"').value = 'Agregar';
                    
                    // Show the Obra dropdown and its label
                    $('#obraContainer').show();
                    
                    // Show the modal
                    $('#modalAgregar').modal('show');
                });", true);

            Button1.Text = "Agregar";

            // Clear any editing state
            ViewState["EditingAutorizanteId"] = null;
            ViewState["EditingCodigoAutorizante"] = null;
        }

        protected void btnLimpiar_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();
        }

        private DataTable ObtenerAreas()
        {
            AreaNegocio areaNegocio = new AreaNegocio();
            return areaNegocio.listarddl();
        }

        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            string filtro = txtBuscar.Text.Trim(); // Obtener el texto del buscador

            //CargarListaAutorizantes(filtro);
        }

        protected void btnAgregar_Click(object sender, EventArgs e)
        {
            // Check if page is valid (all validators passed)
            if (Page.IsValid)
            {
                try
                {
                    AutorizanteNegocio autorizanteNegocio = new AutorizanteNegocio();
                    Autorizante autorizante = new Autorizante();

                    // Common data for both add and update operations
                    autorizante.Concepto = new Concepto();
                    autorizante.Concepto.Id = int.Parse(ddlConcepto.SelectedValue);
                    autorizante.Detalle = txtDetalle.Text;
                    autorizante.Expediente = txtExpediente.Text;
                    autorizante.Estado = new EstadoAutorizante();
                    autorizante.Estado.Id = int.Parse(ddlEstado.SelectedValue);
                    autorizante.MontoAutorizado = decimal.Parse(txtMontoAutorizado.Text);
                    autorizante.Fecha = string.IsNullOrWhiteSpace(txtMes.Text)
                        ? (DateTime?)null
                        : DateTime.Parse(txtMes.Text);
                    autorizante.MesBase = string.IsNullOrWhiteSpace(txtFecha.Text)
                        ? (DateTime?)null
                        : DateTime.Parse(txtFecha.Text);

                    // Check if we're editing an existing autorizante or adding a new one
                    if (ViewState["EditingAutorizanteId"] != null && ViewState["EditingCodigoAutorizante"] != null)
                    {
                        // We're updating an existing autorizante
                        autorizante.CodigoAutorizante = ViewState["EditingCodigoAutorizante"].ToString();
                        autorizante.Obra = new Obra { Id = (int)ViewState["EditingAutorizanteId"] };

                        if (autorizanteNegocio.modificar(autorizante))
                        {
                            lblMensaje.Text = "Autorizante modificado exitosamente!";
                            lblMensaje.CssClass = "alert alert-success";

                            //CargarListaAutorizantes();
                            CargarListaAutorizantesRedet();
                            CalcularSubtotal();

                            // Clear the editing state
                            ViewState["EditingAutorizanteId"] = null;
                            ViewState["EditingCodigoAutorizante"] = null;
                        }
                        else
                        {
                            lblMensaje.Text = "Hubo un problema al modificar el autorizante.";
                            lblMensaje.CssClass = "alert alert-danger";
                        }
                    }
                    else
                    {
                        // We're adding a new autorizante - use the selected obra
                        autorizante.Obra = new Obra();
                        autorizante.Obra.Id = int.Parse(ddlObra.SelectedValue);

                        if (autorizanteNegocio.agregar(autorizante))
                        {
                            lblMensaje.Text = "Autorizante agregado exitosamente!";
                            lblMensaje.CssClass = "alert alert-success";
                        }
                        else
                        {
                            lblMensaje.Text = "Hubo un problema al agregar el autorizante.";
                            lblMensaje.CssClass = "alert alert-danger";
                        }
                    }

                    // Clear fields
                    LimpiarFormulario();

                    // Reset the modal title and button text
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "ResetModalTitle",
                        "$('#modalAgregar .modal-title').text('Agregar Autorizante');", true);
                    Button1.Text = "Agregar";

                    // Hide the modal
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "HideModal",
                        "$('#modalAgregar').modal('hide');", true);

                    // Refresh the autorizantes list
                    CargarListaAutorizantesRedet();
                    CalcularSubtotal();
                }
                catch (Exception ex)
                {
                    lblMensaje.Text = $"Error: {ex.Message}";
                    lblMensaje.CssClass = "alert alert-danger";
                }
            }
        }

        private void LimpiarFormulario()
        {
            txtExpediente.Text = string.Empty;
            txtDetalle.Text = string.Empty;
            txtMontoAutorizado.Text = string.Empty;
            txtFecha.Text = string.Empty;
            txtMes.Text = string.Empty;
            ddlObra.SelectedIndex = 0;
            ddlConcepto.SelectedIndex = 0;
            ddlEstado.SelectedIndex = 0;
        }

        private void CargarListaAutorizantesRedet(string filtro = null)
        {
            try
            {
                // 1. Obtener la lista completa
                List<Autorizante> listaCompleta;
                if (Session["autorizantesCompleto"] == null)
                {
                    listaCompleta = calculoRedeterminacionNegocio.listarAutRedet();
                    Session["autorizantesCompleto"] = listaCompleta;
                }
                else
                {
                    listaCompleta = (List<Autorizante>)Session["autorizantesCompleto"];
                }

                IEnumerable<Autorizante> listaFiltrada = listaCompleta;

                // Obtener IDs seleccionados desde los controles de cabecera del GridView
                List<string> selectedAreas = new List<string>();
                List<string> selectedObras = new List<string>();
                List<string> selectedEmpresas = new List<string>();
                List<string> selectedConceptos = new List<string>();
                List<string> selectedEstados = new List<string>();

                if (dgvAutorizante.HeaderRow != null)
                {
                    var cblsHeaderAreaControl = dgvAutorizante.HeaderRow.FindControl("cblsHeaderArea") as WebForms.CustomControls.TreeViewSearch;
                    if (cblsHeaderAreaControl != null) selectedAreas = cblsHeaderAreaControl.SelectedValues;

                    var cblsHeaderObraControl = dgvAutorizante.HeaderRow.FindControl("cblsHeaderObra") as WebForms.CustomControls.TreeViewSearch;
                    if (cblsHeaderObraControl != null) selectedObras = cblsHeaderObraControl.SelectedValues;

                    var cblsHeaderEmpresaControl = dgvAutorizante.HeaderRow.FindControl("cblsHeaderEmpresa") as WebForms.CustomControls.TreeViewSearch;
                    if (cblsHeaderEmpresaControl != null) selectedEmpresas = cblsHeaderEmpresaControl.SelectedValues;

                    var cblsHeaderConceptoControl = dgvAutorizante.HeaderRow.FindControl("cblsHeaderConcepto") as WebForms.CustomControls.TreeViewSearch;
                    if (cblsHeaderConceptoControl != null) selectedConceptos = cblsHeaderConceptoControl.SelectedValues;

                    var cblsHeaderEstadoControl = dgvAutorizante.HeaderRow.FindControl("cblsHeaderEstado") as WebForms.CustomControls.TreeViewSearch;
                    if (cblsHeaderEstadoControl != null) selectedEstados = cblsHeaderEstadoControl.SelectedValues;
                }

                // 3. Aplicar filtro de texto general
                string filtroTextoGeneral = string.IsNullOrEmpty(filtro) ? txtBuscar.Text.Trim().ToUpper() : filtro.Trim().ToUpper();
                if (!string.IsNullOrEmpty(filtroTextoGeneral))
                {
                    listaFiltrada = listaFiltrada.Where(a =>
                        (a.Obra?.Area?.Nombre?.ToUpper().Contains(filtroTextoGeneral) ?? false) ||
                        (a.Obra?.Descripcion?.ToUpper().Contains(filtroTextoGeneral) ?? false) ||
                        (a.Obra?.Contrata?.Nombre?.ToUpper().Contains(filtroTextoGeneral) ?? false) ||
                        (a.Empresa?.ToUpper().Contains(filtroTextoGeneral) ?? false) || // Autorizante.Empresa (string)
                        (a.Obra?.Empresa?.Nombre?.ToUpper().Contains(filtroTextoGeneral) ?? false) || // Autorizante.Obra.Empresa.Nombre
                        (a.CodigoAutorizante?.ToUpper().Contains(filtroTextoGeneral) ?? false) ||
                        (a.Concepto?.Nombre?.ToUpper().Contains(filtroTextoGeneral) ?? false) ||
                        (a.Detalle?.ToUpper().Contains(filtroTextoGeneral) ?? false) ||
                        (a.Expediente?.ToUpper().Contains(filtroTextoGeneral) ?? false) ||
                        (a.Estado?.Nombre?.ToUpper().Contains(filtroTextoGeneral) ?? false) ||
                        (a.BuzonSade?.ToUpper().Contains(filtroTextoGeneral) ?? false)
                    );
                }

                // 4. Aplicar filtros específicos LINQ
                if (selectedAreas != null && selectedAreas.Any())
                {
                    listaFiltrada = listaFiltrada.Where(a => a.Obra?.Area != null && selectedAreas.Contains(a.Obra.Area.Id.ToString()));
                }
                if (selectedObras != null && selectedObras.Any())
                {
                    listaFiltrada = listaFiltrada.Where(a => a.Obra != null && selectedObras.Contains(a.Obra.Id.ToString()));
                }
                if (selectedEmpresas != null && selectedEmpresas.Any())
                {
                    listaFiltrada = listaFiltrada.Where(a => a.Obra?.Empresa != null && selectedEmpresas.Contains(a.Obra.Empresa.Id.ToString()));
                }
                if (selectedConceptos != null && selectedConceptos.Any())
                {
                    listaFiltrada = listaFiltrada.Where(a => a.Concepto != null && selectedConceptos.Contains(a.Concepto.Id.ToString()));
                }
                if (selectedEstados != null && selectedEstados.Any())
                {
                    listaFiltrada = listaFiltrada.Where(a => a.Estado != null && selectedEstados.Contains(a.Estado.Id.ToString()));
                }

                // 5. Guardar y enlazar la lista filtrada
                List<Autorizante> resultadoFinal = listaFiltrada.ToList();
                Session["listaAutorizanteAdmin"] = resultadoFinal;
                dgvAutorizante.DataSource = resultadoFinal;
                dgvAutorizante.DataBind();
                CalcularSubtotal(); // Calcular después de filtrar y enlazar
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cargar/filtrar los Autorizantes: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
                dgvAutorizante.DataSource = null;
                dgvAutorizante.DataBind();
                txtSubtotal.Text = 0.ToString("C");
            }
        }

        private void CalcularSubtotal()
        {
            decimal subtotal = 0;
            List<Autorizante> dataSource = dgvAutorizante.DataSource as List<Autorizante>;

            if (dataSource != null)
            {
                subtotal = dataSource.Sum(a => a.MontoAutorizado);
            }
            else if (Session["listaAutorizanteAdmin"] != null)
            {
                dataSource = (List<Autorizante>)Session["listaAutorizanteAdmin"];
                subtotal = dataSource.Sum(a => a.MontoAutorizado);
            }


            txtSubtotal.Text = subtotal.ToString("C");
        }

        protected void dgvAutorizante_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                // Get the code of the selected row
                string codigoAutorizante = dgvAutorizante.SelectedDataKey.Value.ToString();

                // Get the list of autorizantes from session
                List<Autorizante> autorizantesList = (List<Autorizante>)Session["listaAutorizanteAdmin"];

                // Find the selected autorizante
                Autorizante autorizanteSeleccionado = autorizantesList.FirstOrDefault(a => a.CodigoAutorizante == codigoAutorizante);

                if (autorizanteSeleccionado != null)
                {
                    // Set button text to "Actualizar"
                    Button1.Text = "Actualizar";

                    // Load the autorizante data into the form fields
                    txtDetalle.Text = autorizanteSeleccionado.Detalle;
                    txtExpediente.Text = autorizanteSeleccionado.Expediente;
                    txtMontoAutorizado.Text = autorizanteSeleccionado.MontoAutorizado.ToString("0.00");

                    if (autorizanteSeleccionado.Fecha.HasValue)
                        txtMes.Text = autorizanteSeleccionado.Fecha.Value.ToString("yyyy-MM-dd");

                    if (autorizanteSeleccionado.MesBase.HasValue)
                        txtFecha.Text = autorizanteSeleccionado.MesBase.Value.ToString("yyyy-MM-dd");

                    // Select the corresponding value in the dropdowns (except Obra that will be hidden)
                    if (autorizanteSeleccionado.Concepto != null)
                        SelectDropDownListByValue(ddlConcepto, autorizanteSeleccionado.Concepto.Id.ToString());

                    if (autorizanteSeleccionado.Estado != null)
                        SelectDropDownListByValue(ddlEstado, autorizanteSeleccionado.Estado.Id.ToString());

                    // Store the IDs for update
                    if (autorizanteSeleccionado.Obra != null)
                        ViewState["EditingAutorizanteId"] = autorizanteSeleccionado.Obra.Id;

                    ViewState["EditingCodigoAutorizante"] = autorizanteSeleccionado.CodigoAutorizante;

                    // Update modal title, button text, and hide the Obra field
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "UpdateModalAndShow", @"
                        $(document).ready(function() {
                            // Change title and button text
                            $('#modalAgregar .modal-title').text('Modificar Autorizante');
                            document.getElementById('" + Button1.ClientID + @"').value = 'Actualizar';
                            
                            // Hide the Obra dropdown and its label
                            $('#obraContainer').hide();
                            
                            // Show the modal
                            $('#modalAgregar').modal('show');
                        });", true);
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cargar los datos del autorizante: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        // Helper method to select dropdown item by value
        private void SelectDropDownListByValue(DropDownList dropDown, string value)
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

        protected void dgvAutorizante_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            try
            {
                var id = dgvAutorizante.DataKeys[e.RowIndex].Value.ToString();
                if (negocio.eliminar(id))
                {
                    lblMensaje.Text = "Autorizante eliminado correctamente.";
                    lblMensaje.CssClass = "alert alert-success";
                    //CargarListaAutorizantes();
                    CargarListaAutorizantesRedet();
                    CalcularSubtotal();
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al eliminar el autorizante: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        private void BindDropDownList()
        {// Clear existing items first
            ddlEstado.Items.Clear();
            ddlConcepto.Items.Clear();
            ddlObra.Items.Clear();

            // Set AppendDataBoundItems to true
            ddlEstado.AppendDataBoundItems = true;
            ddlConcepto.AppendDataBoundItems = true;
            ddlObra.AppendDataBoundItems = true;

            // Add empty items
            ddlEstado.Items.Add(new ListItem("Seleccione un estado", ""));
            ddlConcepto.Items.Add(new ListItem("Seleccione un concepto", ""));
            ddlObra.Items.Add(new ListItem("Seleccione una obra", ""));

            ddlEstado.DataSource = ObtenerEstado();
            ddlEstado.DataTextField = "Nombre";
            ddlEstado.DataValueField = "Id";
            ddlEstado.DataBind();

            ddlConcepto.DataSource = ObtenerConcepto();
            ddlConcepto.DataTextField = "Nombre";
            ddlConcepto.DataValueField = "Id";
            ddlConcepto.DataBind();

            ddlObra.DataSource = ObtenerObras();
            ddlObra.DataTextField = "Nombre";
            ddlObra.DataValueField = "Id";
            ddlObra.DataBind();
        }
        private DataTable ObtenerEmpresas()
        {
            EmpresaNegocio empresaNegocio = new EmpresaNegocio();
            return empresaNegocio.listarddl();
        }

        protected void txtExpediente_TextChanged(object sender, EventArgs e)
        {
            TextBox txtExpediente = (TextBox)sender;
            GridViewRow row = (GridViewRow)txtExpediente.NamingContainer;

            string codigoAutorizante = dgvAutorizante.DataKeys[row.RowIndex].Value.ToString();
            string nuevoExpediente = txtExpediente.Text;

            try
            {
                AutorizanteNegocio negocio = new AutorizanteNegocio();

                if (negocio.ActualizarExpediente(codigoAutorizante, nuevoExpediente))
                {
                    // Refrescar la sesión completa del usuario
                    Usuario usuarioLogueado = (Usuario)Session["usuario"];
                    if (usuarioLogueado != null && usuarioLogueado.Area != null)
                    {
                        List<Autorizante> listaCompletaActualizada = negocio.listar(usuarioLogueado,
                            new List<string>(), new List<string>(), new List<string>(), new List<string>(), null);
                        Session["autorizantesUsuarioCompleto"] = listaCompletaActualizada;
                    }

                    CargarListaAutorizantesRedet();
                    CalcularSubtotal();

                    lblMensaje.Text = "Expediente actualizado correctamente.";
                    lblMensaje.CssClass = "alert alert-success";
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = "Error al actualizar el expediente: " + ex.Message;
                lblMensaje.CssClass = "alert alert-danger";
            }
        }
        private DataTable ObtenerEstado()
        {
            EstadoAutorizanteNegocio empresaNegocio = new EstadoAutorizanteNegocio();
            return empresaNegocio.listarddl();
        }

        private DataTable ObtenerObras()
        {
            ObraNegocio barrioNegocio = new ObraNegocio();
            return barrioNegocio.listarddl();
        }
        private DataTable ObtenerConcepto()
        {
            ConceptoNegocio empresaNegocio = new ConceptoNegocio();
            return empresaNegocio.listarddl();
        }
        protected void ddlEstadoAutorizante_SelectedIndexChanged(object sender, EventArgs e)
        {
            DropDownList ddlEstadoAutorizante = (DropDownList)sender;
            GridViewRow row = (GridViewRow)ddlEstadoAutorizante.NamingContainer;

            try
            {
                List<Autorizante> listaAutorizantes = (List<Autorizante>)Session["listaAutorizanteAdmin"];
                string codAutorizante = dgvAutorizante.DataKeys[row.RowIndex].Value.ToString();
                Autorizante autorizante = listaAutorizantes.Find(a => a.CodigoAutorizante == codAutorizante);

                if (autorizante != null)
                {
                    autorizante.Estado.Id = int.Parse(ddlEstadoAutorizante.SelectedValue);
                    AutorizanteNegocio negocio = new AutorizanteNegocio();

                    if (negocio.ActualizarEstado(autorizante))
                    {
                        // Refrescar la sesión completa de administrador
                        List<Autorizante> listaCompletaActualizada = calculoRedeterminacionNegocio.listarAutRedet();
                        Session["autorizantesCompleto"] = listaCompletaActualizada;

                        // Recargar la lista filtrada
                        CargarListaAutorizantesRedet();

                        lblMensaje.Text = "Estado actualizado correctamente.";
                        lblMensaje.CssClass = "alert alert-success";
                    }
                    else
                    {
                        lblMensaje.Text = "Error al actualizar el estado.";
                        lblMensaje.CssClass = "alert alert-danger";
                    }
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al actualizar el estado: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }

        }
        protected void dgvAutorizante_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                DropDownList ddlEstadoAutorizante = (DropDownList)e.Row.FindControl("ddlEstadoAutorizante");
                if (ddlEstadoAutorizante != null)
                {
                    DataTable estados = ObtenerEstado(); // Asume que esto obtiene todos los estados posibles
                    ddlEstadoAutorizante.DataSource = estados;
                    ddlEstadoAutorizante.DataTextField = "Nombre";
                    ddlEstadoAutorizante.DataValueField = "Id";
                    ddlEstadoAutorizante.DataBind();

                    // Establecer el valor seleccionado para el DropDownList en la fila
                    Autorizante autorizante = (Autorizante)e.Row.DataItem;
                    if (autorizante != null && autorizante.Estado != null)
                    {
                        ListItem item = ddlEstadoAutorizante.Items.FindByValue(autorizante.Estado.Id.ToString());
                        if (item != null)
                        {
                            ddlEstadoAutorizante.SelectedValue = autorizante.Estado.Id.ToString();
                        }
                    }
                }
            }
            else if (e.Row.RowType == DataControlRowType.Header)
            {
                List<Autorizante> autorizantesCompleto;
                if (Session["autorizantesCompleto"] == null)
                {
                    autorizantesCompleto = calculoRedeterminacionNegocio.listarAutRedet();
                    Session["autorizantesCompleto"] = autorizantesCompleto;
                }
                else
                {
                    autorizantesCompleto = (List<Autorizante>)Session["autorizantesCompleto"];
                }

                var cblsHeaderArea = e.Row.FindControl("cblsHeaderArea") as WebForms.CustomControls.TreeViewSearch;
                if (cblsHeaderArea != null && autorizantesCompleto != null)
                {
                    var areasUnicas = autorizantesCompleto
                        .Where(a => a.Obra?.Area != null && !string.IsNullOrEmpty(a.Obra.Area.Nombre))
                        .Select(a => a.Obra.Area)
                        .GroupBy(area => area.Id)
                        .Select(g => g.First())
                        .OrderBy(area => area.Nombre)
                        .ToList();
                    cblsHeaderArea.DataSource = areasUnicas;
                    cblsHeaderArea.DataBind();
                }

                var cblsHeaderObra = e.Row.FindControl("cblsHeaderObra") as WebForms.CustomControls.TreeViewSearch;
                if (cblsHeaderObra != null && autorizantesCompleto != null)
                {
                    var obrasUnicas = autorizantesCompleto
                        .Where(a => a.Obra != null && !string.IsNullOrEmpty(a.Obra.Descripcion))
                        .Select(a => new { Id = a.Obra.Id, Nombre = a.Obra.Descripcion }) // Nombre aquí es Descripcion
                        .Distinct()
                        .OrderBy(o => o.Nombre)
                        .ToList();
                    cblsHeaderObra.DataTextField = "Nombre"; // Corresponde al 'Nombre' del objeto anónimo
                    cblsHeaderObra.DataValueField = "Id";
                    cblsHeaderObra.DataSource = obrasUnicas;
                    cblsHeaderObra.DataBind();
                }

                var cblsHeaderEmpresa = e.Row.FindControl("cblsHeaderEmpresa") as WebForms.CustomControls.TreeViewSearch;
                if (cblsHeaderEmpresa != null && autorizantesCompleto != null)
                {
                    var empresasUnicas = autorizantesCompleto
                        .Where(a => a.Obra?.Empresa != null && !string.IsNullOrEmpty(a.Obra.Empresa.Nombre))
                        .Select(a => a.Obra.Empresa)
                        .GroupBy(emp => emp.Id)
                        .Select(g => g.First())
                        .OrderBy(emp => emp.Nombre)
                        .ToList();
                    cblsHeaderEmpresa.DataSource = empresasUnicas;
                    cblsHeaderEmpresa.DataBind();
                }

                var cblsHeaderConcepto = e.Row.FindControl("cblsHeaderConcepto") as WebForms.CustomControls.TreeViewSearch;
                if (cblsHeaderConcepto != null && autorizantesCompleto != null)
                {
                    var conceptosUnicos = autorizantesCompleto
                        .Where(a => a.Concepto != null && !string.IsNullOrEmpty(a.Concepto.Nombre))
                        .Select(a => a.Concepto)
                        .GroupBy(con => con.Id)
                        .Select(g => g.First())
                        .OrderBy(con => con.Nombre)
                        .ToList();
                    cblsHeaderConcepto.DataSource = conceptosUnicos;
                    cblsHeaderConcepto.DataBind();
                }

                var cblsHeaderEstado = e.Row.FindControl("cblsHeaderEstado") as WebForms.CustomControls.TreeViewSearch;
                if (cblsHeaderEstado != null && autorizantesCompleto != null)
                {
                    var estadosUnicos = autorizantesCompleto
                        .Where(a => a.Estado != null && !string.IsNullOrEmpty(a.Estado.Nombre))
                        .Select(a => a.Estado)
                        .GroupBy(est => est.Id)
                        .Select(g => g.First())
                        .OrderBy(est => est.Nombre)
                        .ToList();
                    cblsHeaderEstado.DataSource = estadosUnicos;
                    cblsHeaderEstado.DataBind();
                }
            }
        }

        protected void BtnClearFilters_Click(object sender, EventArgs e)
        {
            txtBuscar.Text = string.Empty;

            WebForms.CustomControls.TreeViewSearch.ClearAllFiltersOnPage(this.Page);


            CargarListaAutorizantesRedet();
        }

        private void ClearFilter(string controlId)
        {
            if (dgvAutorizante.HeaderRow != null)
            {
                var control = dgvAutorizante.HeaderRow.FindControl(controlId) as WebForms.CustomControls.TreeViewSearch;
                control?.ClearSelection();

                string controlInstanceId = control.ID; // Usar el ID del control para la clave de sesión/contexto.

                string sessionKey = $"TreeViewSearch_SelectedValues_{controlInstanceId}";
                if (HttpContext.Current.Session[sessionKey] != null)
                {
                    HttpContext.Current.Session.Remove(sessionKey);
                }

                string contextKey = $"TreeViewSearch_{controlInstanceId}_ContextSelectedValues";
                if (HttpContext.Current.Items.Contains(contextKey))
                {
                    HttpContext.Current.Items.Remove(contextKey);
                }
            }
        }
        protected void dgvAutorizante_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            try
            {
                dgvAutorizante.PageIndex = e.NewPageIndex;
                CargarListaAutorizantesRedet();
                CalcularSubtotal();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cambiar de página: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }
    }
}