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

        protected void Page_Init(object sender, EventArgs e)
        {
            cblArea.SelectedIndexChanged += OnCheckBoxListSearch_SelectedIndexChanged;
            cblEmpresa.SelectedIndexChanged += OnCheckBoxListSearch_SelectedIndexChanged;
            cblConcepto.SelectedIndexChanged += OnCheckBoxListSearch_SelectedIndexChanged;
            cblEstado.SelectedIndexChanged += OnCheckBoxListSearch_SelectedIndexChanged;
            cblObra.SelectedIndexChanged += OnCheckBoxListSearch_SelectedIndexChanged;
        }

        private void OnCheckBoxListSearch_SelectedIndexChanged(object sender, EventArgs e)
        {
            //CargarListaAutorizantes();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindDropDownList();
                //CargarListaAutorizantes();
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

        private void CargarListaAutorizantes(string filtro = null)
        {
            try
            {
                var selectedAreas = cblArea.Items.Cast<ListItem>().Where(i => i.Selected).Select(i => i.Text).ToList();
                var selectedEmpresas = cblEmpresa.Items.Cast<ListItem>().Where(i => i.Selected).Select(i => i.Text).ToList();
                var selectedConceptos = cblConcepto.Items.Cast<ListItem>().Where(i => i.Selected).Select(i => i.Text).ToList();
                var selectedEstados = cblEstado.Items.Cast<ListItem>().Where(i => i.Selected).Select(i => i.Text).ToList();
                var selectedObras = cblObra.Items.Cast<ListItem>().Where(i => i.Selected).Select(i => i.Value).ToList();

                Session["listaAutorizanteAdmin"] = negocio.listar(selectedAreas, selectedEstados, selectedEmpresas, selectedConceptos, selectedObras, filtro);
                dgvAutorizante.DataSource = Session["listaAutorizanteAdmin"];
                dgvAutorizante.DataBind();
                CalcularSubtotal();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cargar los Autorizantes: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        private void CargarListaAutorizantesRedet(string filtro = null)
        {
            try
            {
                Session["listaAutorizanteAdmin"] = calculoRedeterminacionNegocio.listarAutRedet();
                dgvAutorizante.DataSource = Session["listaAutorizanteAdmin"];
                dgvAutorizante.DataBind();
                //CalcularSubtotal();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cargar los Autorizantes: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        private void CalcularSubtotal()
        {
            decimal subtotal = 0;

            foreach (GridViewRow row in dgvAutorizante.Rows)
            {
                var cellValue = row.Cells[10].Text;
                if (decimal.TryParse(cellValue, System.Globalization.NumberStyles.Currency, null, out decimal monto))
                {
                    subtotal += monto;
                }
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
            
            cblArea.DataSource = ObtenerAreas();
            cblArea.DataTextField = "Nombre";
            cblArea.DataValueField = "Id";
            cblArea.DataBind();
            cblEstado.DataSource = ObtenerEstado();
            cblEstado.DataTextField = "Nombre";
            cblEstado.DataValueField = "Id";
            cblEstado.DataBind();

            cblConcepto.DataSource = ObtenerConcepto();
            cblConcepto.DataTextField = "Nombre";
            cblConcepto.DataValueField = "Id";
            cblConcepto.DataBind();

            cblEmpresa.DataSource = ObtenerEmpresas();
            cblEmpresa.DataTextField = "Nombre";
            cblEmpresa.DataValueField = "Id";
            cblEmpresa.DataBind();

            cblObra.DataSource = ObtenerObras();
            cblObra.DataTextField = "Nombre";
            cblObra.DataValueField = "Id";
            cblObra.DataBind();
        }
        private DataTable ObtenerEmpresas()
        {
            EmpresaNegocio empresaNegocio = new EmpresaNegocio();
            return empresaNegocio.listarddl();
        }
      
        protected void txtExpediente_TextChanged(object sender, EventArgs e)
        {
            // Identifica el TextBox modificado
            TextBox txtExpediente = (TextBox)sender;
            GridViewRow row = (GridViewRow)txtExpediente.NamingContainer;

            // Obtiene la clave del registro desde DataKeyNames
            string codigoAutorizante = dgvAutorizante.DataKeys[row.RowIndex].Value.ToString();

            // Nuevo valor del expediente
            string nuevoExpediente = txtExpediente.Text;

            // Actualiza en la base de datos
            try
            {
                // Llama al método del negocio para actualizar el expediente
                AutorizanteNegocio negocio = new AutorizanteNegocio();
                negocio.ActualizarExpediente(codigoAutorizante, nuevoExpediente);

                // Mensaje de éxito o retroalimentación opcional
                lblMensaje.Text = "Expediente actualizado correctamente.";
                //CargarListaAutorizantes();
                CalcularSubtotal();

            }
            catch (Exception ex)
            {
                // Manejo de errores
                lblMensaje.Text = "Error al actualizar el expediente: " + ex.Message;
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

            List<Autorizante> listaAutorizantes = (List<Autorizante>)Session["listaAutorizante"];
            string codAutorizante = dgvAutorizante.DataKeys[row.RowIndex].Value.ToString();
            Autorizante autorizante = listaAutorizantes.Find(a => a.CodigoAutorizante == codAutorizante);

            if (autorizante != null)
            {
                autorizante.Estado.Id = int.Parse(ddlEstadoAutorizante.SelectedValue);
                AutorizanteNegocio negocio = new AutorizanteNegocio();
                negocio.ActualizarEstado(autorizante);
                //CargarListaAutorizantes();

                lblMensaje.Text = "Estado actualizado correctamente.";
                lblMensaje.CssClass = "alert alert-success";
            }
        }
        protected void dgvAutorizante_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                DropDownList ddlEstadoAutorizante = (DropDownList)e.Row.FindControl("ddlEstadoAutorizante");

                if (ddlEstadoAutorizante != null)
                {
                    DataTable estados = ObtenerEstado();
                    ddlEstadoAutorizante.DataSource = estados;
                    ddlEstadoAutorizante.DataTextField = "Nombre";
                    ddlEstadoAutorizante.DataValueField = "Id";
                    ddlEstadoAutorizante.DataBind();

                    string estadoActual = DataBinder.Eval(e.Row.DataItem, "Estado.Id").ToString();
                    ddlEstadoAutorizante.SelectedValue = estadoActual;
                }
            }
        }

        protected void BtnClearFilters_Click(object sender, EventArgs e) 
        {
            txtBuscar.Text = string.Empty;
            cblArea.ClearSelection();
            cblEmpresa.ClearSelection();
            cblConcepto.ClearSelection();
            cblEstado.ClearSelection();
            cblObra.ClearSelection();
            //CargarListaAutorizantes();
        }

    }

}