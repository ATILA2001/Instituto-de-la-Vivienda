using Dominio;
using Negocio;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebForms
{
    public partial class Autorizantes : System.Web.UI.Page
    {
        private AutorizanteNegocio negocio = new AutorizanteNegocio();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                Usuario usuarioLogueado = (Usuario)Session["usuario"];
                if (usuarioLogueado != null && usuarioLogueado.Area != null)
                {
                    // Cargar la lista completa de autorizantes para el área del usuario y guardarla en sesión
                    // Se pasan listas vacías para los filtros para obtener todos los del área.
                    List<Autorizante> listaCompletaUsuario = negocio.listar(usuarioLogueado,
                        new List<string>(), new List<string>(), new List<string>(), new List<string>(), null);
                    Session["autorizantesUsuarioCompleto"] = listaCompletaUsuario;
                }
                else
                {
                    // Manejar el caso donde el usuario o su área no están definidos.
                    Session["autorizantesUsuarioCompleto"] = new List<Autorizante>(); // Lista vacía para evitar nulls
                    lblMensaje.Text = "No se pudo determinar el área del usuario. No se pueden cargar los autorizantes.";
                    lblMensaje.CssClass = "alert alert-warning";
                }

                BindDropDownList(); // Para los dropdowns del modal
                CargarListaAutorizantes(); // Carga inicial (puede estar vacía si no hay usuario/área)
            }
        }

        protected void OnAcceptChanges(object sender, EventArgs e)
        {
            CargarListaAutorizantes();
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

        private void CalcularSubtotal()
        {
            decimal subtotal = 0;
            List<Autorizante> dataSource = dgvAutorizante.DataSource as List<Autorizante>;

            if (dataSource != null) // Si el DataSource fue asignado directamente
            {
                subtotal = dataSource.Sum(a => a.MontoAutorizado);
            }
            else if (Session["listaAutorizante"] != null) // Fallback a la lista en Session
            {
                dataSource = (List<Autorizante>)Session["listaAutorizante"];
                subtotal = dataSource.Sum(a => a.MontoAutorizado);
            }
            // Si ninguno de los dos, subtotal permanece 0

            txtSubtotal.Text = subtotal.ToString("C");
        }

        private void CargarListaAutorizantes(string filtro = null)
        {
            try
            {
                List<Autorizante> listaBase;

                    
                    Usuario usuarioLogueado = (Usuario)Session["usuario"];
                    if (usuarioLogueado != null && usuarioLogueado.Area != null)
                    {
                        listaBase = negocio.listar(usuarioLogueado, new List<string>(), new List<string>(), new List<string>(), new List<string>(), null);
                        Session["autorizantesUsuarioCompleto"] = listaBase;
                    }
                    else
                    {
                        dgvAutorizante.DataSource = new List<Autorizante>();
                        dgvAutorizante.DataBind();
                        CalcularSubtotal();
                        return;
                    }
                

                IEnumerable<Autorizante> listaFiltrada = listaBase;

                // Obtener valores de los filtros de cabecera
                List<string> selectedHeaderObras = new List<string>();
                List<string> selectedHeaderEmpresas = new List<string>();
                List<string> selectedHeaderConceptos = new List<string>();
                List<string> selectedHeaderEstados = new List<string>();

                if (dgvAutorizante.HeaderRow != null)
                {
                    var cblsHeaderObraCtrl = dgvAutorizante.HeaderRow.FindControl("cblsHeaderObra") as WebForms.CustomControls.TreeViewSearch;
                    if (cblsHeaderObraCtrl != null) selectedHeaderObras = cblsHeaderObraCtrl.SelectedValues;

                    var cblsHeaderEmpresaCtrl = dgvAutorizante.HeaderRow.FindControl("cblsHeaderEmpresa") as WebForms.CustomControls.TreeViewSearch;
                    if (cblsHeaderEmpresaCtrl != null) selectedHeaderEmpresas = cblsHeaderEmpresaCtrl.SelectedValues;

                    var cblsHeaderConceptoCtrl = dgvAutorizante.HeaderRow.FindControl("cblsHeaderConcepto") as WebForms.CustomControls.TreeViewSearch;
                    if (cblsHeaderConceptoCtrl != null) selectedHeaderConceptos = cblsHeaderConceptoCtrl.SelectedValues;

                    var cblsHeaderEstadoCtrl = dgvAutorizante.HeaderRow.FindControl("cblsHeaderEstado") as WebForms.CustomControls.TreeViewSearch;
                    if (cblsHeaderEstadoCtrl != null) selectedHeaderEstados = cblsHeaderEstadoCtrl.SelectedValues;
                }

                // Aplicar filtro de texto general
                string filtroTexto = string.IsNullOrEmpty(filtro) ? txtBuscar.Text.Trim().ToUpper() : filtro.Trim().ToUpper();

                if (!string.IsNullOrEmpty(filtroTexto))
                {
                    listaFiltrada = listaFiltrada.Where(a =>
                        (a.Obra?.Descripcion?.ToUpper().Contains(filtroTexto) ?? false) ||
                        (a.Obra?.Contrata?.Nombre?.ToUpper().Contains(filtroTexto) ?? false) ||
                        (a.Empresa?.ToUpper().Contains(filtroTexto) ?? false) || // Autorizante.Empresa (string)
                        (a.CodigoAutorizante?.ToUpper().Contains(filtroTexto) ?? false) ||
                        (a.Concepto?.Nombre?.ToUpper().Contains(filtroTexto) ?? false) ||
                        (a.Detalle?.ToUpper().Contains(filtroTexto) ?? false) ||
                        (a.Expediente?.ToUpper().Contains(filtroTexto) ?? false) ||
                        (a.Estado?.Nombre?.ToUpper().Contains(filtroTexto) ?? false) ||
                        (a.BuzonSade?.ToUpper().Contains(filtroTexto) ?? false)
                    );
                }

                // Aplicar filtros de cabecera (IDs)
                if (selectedHeaderObras.Any())
                    listaFiltrada = listaFiltrada.Where(a => a.Obra != null && selectedHeaderObras.Contains(a.Obra.Id.ToString()));

                if (selectedHeaderEmpresas.Any()) // Filtra por Autorizante.Obra.Empresa.Id
                    listaFiltrada = listaFiltrada.Where(a => a.Obra?.Empresa != null && selectedHeaderEmpresas.Contains(a.Obra.Empresa.Id.ToString()));

                if (selectedHeaderConceptos.Any())
                    listaFiltrada = listaFiltrada.Where(a => a.Concepto != null && selectedHeaderConceptos.Contains(a.Concepto.Id.ToString()));

                if (selectedHeaderEstados.Any())
                    listaFiltrada = listaFiltrada.Where(a => a.Estado != null && selectedHeaderEstados.Contains(a.Estado.Id.ToString()));

                List<Autorizante> resultadoFinal = listaFiltrada.ToList();
                Session["listaAutorizante"] = resultadoFinal; // Actualizar la sesión con la lista filtrada
                dgvAutorizante.DataSource = resultadoFinal;
                dgvAutorizante.DataBind();
                CalcularSubtotal();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cargar los Autorizantes: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
                dgvAutorizante.DataSource = new List<Autorizante>(); // Evitar error si falla la carga
                dgvAutorizante.DataBind();
                CalcularSubtotal(); // Asegurar que el subtotal se actualice a 0 o el valor correcto
            }

        }

        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            string filtro = txtBuscar.Text.Trim();
            CargarListaAutorizantes(filtro);
        }

        protected void dgvAutorizante_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                // Get the ID of the selected row
                string codigoAutorizante = dgvAutorizante.SelectedDataKey.Value.ToString();

                // Get the list of autorizantes from session
                List<Autorizante> autorizantesList = (List<Autorizante>)Session["listaAutorizante"];

                // Find the selected autorizante
                Autorizante autorizanteSeleccionado = autorizantesList.FirstOrDefault(a => a.CodigoAutorizante == codigoAutorizante);

                if (autorizanteSeleccionado != null)
                {
                    // Set button text to "Actualizar"
                    Button1.Text = "Actualizar";

                    // Load the autorizante data into the form fields
                    txtDetalle.Text = autorizanteSeleccionado.Detalle ?? string.Empty;
                    txtExpediente.Text = autorizanteSeleccionado.Expediente ?? string.Empty;
                    txtMontoAutorizado.Text = autorizanteSeleccionado.MontoAutorizado.ToString("0.00");

                    if (autorizanteSeleccionado.Fecha.HasValue)
                        txtMes.Text = autorizanteSeleccionado.Fecha.Value.ToString("yyyy-MM-dd");

                    if (autorizanteSeleccionado.MesBase.HasValue)
                        txtFecha.Text = autorizanteSeleccionado.MesBase.Value.ToString("yyyy-MM-dd");

                    // Select the corresponding values in the dropdowns (except Obra which will be hidden)
                    if (autorizanteSeleccionado.Concepto != null)
                        SelectDropDownListByValue(ddlConcepto, autorizanteSeleccionado.Concepto.Id.ToString());

                    if (autorizanteSeleccionado.Estado != null)
                        SelectDropDownListByValue(ddlEstado, autorizanteSeleccionado.Estado.Id.ToString());

                    // Store the ID for the obra and the codigo autorizante for update
                    if (autorizanteSeleccionado.Obra != null)
                        ViewState["EditingAutorizanteId"] = autorizanteSeleccionado.Obra.Id;

                    ViewState["EditingCodigoAutorizante"] = autorizanteSeleccionado.CodigoAutorizante;

                    // Update modal title and hide the Obra field
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
                    CargarListaAutorizantes();
                    CalcularSubtotal();
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al eliminar el Autorizante: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        protected void dgvAutorizante_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            try
            {
                dgvAutorizante.PageIndex = e.NewPageIndex;
                CargarListaAutorizantes();
                CalcularSubtotal();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cambiar de página: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        protected void dgvAutorizante_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                // Lógica existente para el DropDownList de estado en la fila
                DropDownList ddlEstadoAutorizante = (DropDownList)e.Row.FindControl("ddlEstadoAutorizante");
                if (ddlEstadoAutorizante != null)
                {
                    DataTable estados = ObtenerEstado();
                    ddlEstadoAutorizante.DataSource = estados;
                    ddlEstadoAutorizante.DataTextField = "Nombre";
                    ddlEstadoAutorizante.DataValueField = "Id";
                    ddlEstadoAutorizante.DataBind();

                    Autorizante autorizante = (Autorizante)e.Row.DataItem;
                    if (autorizante != null && autorizante.Estado != null)
                    {
                        ListItem item = ddlEstadoAutorizante.Items.FindByValue(autorizante.Estado.Id.ToString());
                        if (item != null)
                        {
                            item.Selected = true;
                        }
                    }
                }
            }
            else if (e.Row.RowType == DataControlRowType.Header)
            {
                List<Autorizante> autorizantesUsuarioCompleto = Session["autorizantesUsuarioCompleto"] as List<Autorizante>;

                if (autorizantesUsuarioCompleto == null || !autorizantesUsuarioCompleto.Any())
                {
                    return; // No hay datos para poblar los filtros.
                }

                // Poblar filtro de Obra en cabecera
                var cblsHeaderObra = e.Row.FindControl("cblsHeaderObra") as WebForms.CustomControls.TreeViewSearch;
                if (cblsHeaderObra != null)
                {
                    var obrasUnicas = autorizantesUsuarioCompleto
                        .Where(a => a.Obra != null)
                        .Select(a => new { Id = a.Obra.Id, Descripcion = a.Obra.Descripcion })
                        .Distinct()
                        .OrderBy(o => o.Descripcion)
                        .ToList();
                    cblsHeaderObra.DataTextField = "Descripcion";
                    cblsHeaderObra.DataValueField = "Id";
                    cblsHeaderObra.DataSource = obrasUnicas;
                    cblsHeaderObra.DataBind();
                }

                // Poblar filtro de Empresa en cabecera
                var cblsHeaderEmpresa = e.Row.FindControl("cblsHeaderEmpresa") as WebForms.CustomControls.TreeViewSearch;
                if (cblsHeaderEmpresa != null)
                {
                    var empresasUnicas = autorizantesUsuarioCompleto
                        .Where(a => a.Obra?.Empresa != null)
                        .Select(a => new { Id = a.Obra.Empresa.Id, Nombre = a.Obra.Empresa.Nombre })
                        .Distinct()
                        .OrderBy(em => em.Nombre)
                        .ToList();
                    cblsHeaderEmpresa.DataTextField = "Nombre";
                    cblsHeaderEmpresa.DataValueField = "Id";
                    cblsHeaderEmpresa.DataSource = empresasUnicas;
                    cblsHeaderEmpresa.DataBind();
                }

                // Poblar filtro de Concepto en cabecera
                var cblsHeaderConcepto = e.Row.FindControl("cblsHeaderConcepto") as WebForms.CustomControls.TreeViewSearch;
                if (cblsHeaderConcepto != null)
                {
                    var conceptosUnicos = autorizantesUsuarioCompleto
                        .Where(a => a.Concepto != null)
                        .Select(a => new { Id = a.Concepto.Id, Nombre = a.Concepto.Nombre })
                        .Distinct()
                        .OrderBy(c => c.Nombre)
                        .ToList();
                    cblsHeaderConcepto.DataTextField = "Nombre";
                    cblsHeaderConcepto.DataValueField = "Id";
                    cblsHeaderConcepto.DataSource = conceptosUnicos;
                    cblsHeaderConcepto.DataBind();
                }

                // Poblar filtro de Estado en cabecera
                var cblsHeaderEstado = e.Row.FindControl("cblsHeaderEstado") as WebForms.CustomControls.TreeViewSearch;
                if (cblsHeaderEstado != null)
                {
                    var estadosUnicos = autorizantesUsuarioCompleto
                        .Where(a => a.Estado != null)
                        .Select(a => new { Id = a.Estado.Id, Nombre = a.Estado.Nombre })
                        .Distinct()
                        .OrderBy(es => es.Nombre)
                        .ToList();
                    cblsHeaderEstado.DataTextField = "Nombre";
                    cblsHeaderEstado.DataValueField = "Id";
                    cblsHeaderEstado.DataSource = estadosUnicos;
                    cblsHeaderEstado.DataBind();
                }
            }
        }


        protected void btnAgregar_Click(object sender, EventArgs e)
        {
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
                        autorizante.Obra = new Obra { Id = Convert.ToInt32(ViewState["EditingAutorizanteId"]) };

                        if (autorizanteNegocio.modificar(autorizante))
                        {
                            lblMensaje.Text = "Autorizante modificado exitosamente!";
                            lblMensaje.CssClass = "alert alert-success";

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
                        // We're adding a new autorizante
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
                    CargarListaAutorizantes();
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
       

        private DataTable ObtenerEstado()
        {
            EstadoAutorizanteNegocio empresaNegocio = new EstadoAutorizanteNegocio();
            return empresaNegocio.listarddl();
        }
        private DataTable ObtenerConcepto()
        {
            ConceptoNegocio empresaNegocio = new ConceptoNegocio();
            return empresaNegocio.listarddl();
        }

        private DataTable ObtenerObras()
        {
            ObraNegocio barrioNegocio = new ObraNegocio();
            Usuario usuarioLogueado = (Usuario)Session["usuario"];
            return barrioNegocio.listarddl(usuarioLogueado);
        }

        protected void ddlEstadoAutorizante_SelectedIndexChanged(object sender, EventArgs e)
        {
            DropDownList ddlEstadoAutorizante = (DropDownList)sender;
            GridViewRow row = (GridViewRow)ddlEstadoAutorizante.NamingContainer;

            try
            {
                List<Autorizante> listaAutorizantes = (List<Autorizante>)Session["listaAutorizante"];
                string codAutorizante = dgvAutorizante.DataKeys[row.RowIndex].Value.ToString();
                Autorizante autorizante = listaAutorizantes.Find(a => a.CodigoAutorizante == codAutorizante);

                if (autorizante != null)
                {
                    autorizante.Estado.Id = int.Parse(ddlEstadoAutorizante.SelectedValue);
                    AutorizanteNegocio negocio = new AutorizanteNegocio();

                    if (negocio.ActualizarEstado(autorizante))
                    {
                        // Refrescar la sesión completa del usuario
                        Usuario usuarioLogueado = (Usuario)Session["usuario"];
                        if (usuarioLogueado != null && usuarioLogueado.Area != null)
                        {
                            List<Autorizante> listaCompletaActualizada = negocio.listar(usuarioLogueado,
                                new List<string>(), new List<string>(), new List<string>(), new List<string>(), null);
                            Session["autorizantesUsuarioCompleto"] = listaCompletaActualizada;
                        }

                        // Recargar la lista filtrada
                        CargarListaAutorizantes();

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

                    CargarListaAutorizantes();
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


        protected void BtnClearFilters_Click(object sender, EventArgs e)
        {
            txtBuscar.Text = string.Empty;

            WebForms.CustomControls.TreeViewSearch.ClearAllFiltersOnPage(this.Page);

            CargarListaAutorizantes();
        }

        private void ClearHeaderFilter(string controlId)
        {
            if (dgvAutorizante.HeaderRow != null)
            {
                var control = dgvAutorizante.HeaderRow.FindControl(controlId) as WebForms.CustomControls.TreeViewSearch;
                if (control != null)
                {
                    control.ClearSelection();
                    // Lógica para limpiar la sesión/contexto si el control TreeViewSearch lo requiere internamente
                    // (similar a AutorizantesAdmin.aspx.cs)
                    string controlInstanceId = control.ID;
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
        }

    }
}