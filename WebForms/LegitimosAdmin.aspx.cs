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
    public partial class LegitimosAdmin : System.Web.UI.Page
    {
        private LegitimoNegocio negocio = new LegitimoNegocio();

        protected void Page_Init(object sender, EventArgs e)
        {
            cblArea.SelectedIndexChanged += OnCheckBoxListSearch_SelectedIndexChanged;
            cblEmpresa.SelectedIndexChanged += OnCheckBoxListSearch_SelectedIndexChanged;
            cblAutorizante.SelectedIndexChanged += OnCheckBoxListSearch_SelectedIndexChanged;
            cblFecha.SelectedIndexChanged += OnCheckBoxListSearch_SelectedIndexChanged;
            cblLinea.SelectedIndexChanged += OnCheckBoxListSearch_SelectedIndexChanged;
            cblEstadoExpediente.SelectedIndexChanged += OnCheckBoxListSearch_SelectedIndexChanged;
        }

        private void OnCheckBoxListSearch_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarListaLegitimos();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindDropDownList();
                CargarListaLegitimos();
            }
            else
            {
                // If we're in edit mode and returning from a postback
                if (ViewState["EditingLegitimoId"] != null && ViewState["EditingObraId"] != null)
                {
                    // Make sure the obra dropdown is correctly set
                    int obraId = (int)ViewState["EditingObraId"];

                    ddlObra.ClearSelection();
                    ListItem item = ddlObra.Items.FindByValue(obraId.ToString());
                    if (item != null)
                    {
                        item.Selected = true;
                    }

                    // Keep the dropdown disabled
                    ddlObra.Enabled = false;
                }
            }
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            // Configure validators if we're in editing mode
            if (ViewState["EditingLegitimoId"] != null)
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
            $('#modalAgregar .modal-title').text('Agregar Legítimo');
            document.getElementById('" + btnAgregar.ClientID + @"').value = 'Agregar';
            
            // Show the Obra dropdown and its label
            $('#obraContainer').show();
            
            // Show the modal
            $('#modalAgregar').modal('show');
        });", true);

            btnAgregar.Text = "Agregar";

            // Clear any editing state
            ViewState["EditingLegitimoId"] = null;
            ViewState["EditingObraId"] = null;
        }

        protected void btnLimpiar_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();
        }

        protected void btnAgregar_Click(object sender, EventArgs e)
        {
            if (Page.IsValid)
            {
                try
                {
                    LegitimoNegocio negocio = new LegitimoNegocio();
                    Legitimo legitimo = new Legitimo();

                    // Common data for both add and update operations
                    legitimo.CodigoAutorizante = txtAutorizante.Text.Trim();
                    legitimo.Expediente = txtExpediente.Text.Trim();
                    legitimo.InicioEjecucion = DateTime.Parse(txtInicioEjecucion.Text);
                    legitimo.FinEjecucion = DateTime.Parse(txtFinEjecucion.Text);
                    legitimo.Certificado = decimal.Parse(txtCertificado.Text);
                    legitimo.MesAprobacion = DateTime.Parse(txtMesAprobacion.Text);

                    // Check if we're editing an existing legitimo or adding a new one
                    if (ViewState["EditingLegitimoId"] != null)
                    {
                        // We're updating an existing legitimo
                        legitimo.Id = (int)ViewState["EditingLegitimoId"];

                        // Use the stored Obra ID from ViewState for the update
                        if (ViewState["EditingObraId"] != null)
                        {
                            legitimo.Obra = new Obra { Id = (int)ViewState["EditingObraId"] };
                        }

                        if (negocio.modificar(legitimo))
                        {
                            lblMensaje.Text = "Legítimo modificado exitosamente!";
                            lblMensaje.CssClass = "alert alert-success";

                            // Clear the editing state
                            ViewState["EditingLegitimoId"] = null;
                            ViewState["EditingObraId"] = null;
                        }
                        else
                        {
                            lblMensaje.Text = "Hubo un problema al modificar el legítimo.";
                            lblMensaje.CssClass = "alert alert-danger";
                        }
                    }
                    else
                    {
                        // We're adding a new legitimo - use the selected value from the dropdown
                        legitimo.Obra = new Obra { Id = int.Parse(ddlObra.SelectedValue) };

                        if (negocio.agregar(legitimo))
                        {
                            lblMensaje.Text = "Legítimo agregado exitosamente!";
                            lblMensaje.CssClass = "alert alert-success";
                        }
                        else
                        {
                            lblMensaje.Text = "Hubo un problema al agregar el legítimo.";
                            lblMensaje.CssClass = "alert alert-danger";
                        }
                    }

                    // Clear fields
                    LimpiarFormulario();

                    // Reset the modal title and button text
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "ResetModalTitle",
                        "$('#modalAgregar .modal-title').text('Agregar Legítimo');", true);
                    btnAgregar.Text = "Agregar";

                    // Hide the modal
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "HideModal",
                        "$('#modalAgregar').modal('hide');", true);

                    // Refresh the legitimos list
                    CargarListaLegitimos();
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
            txtAutorizante.Text = string.Empty;
            txtExpediente.Text = string.Empty;
            txtInicioEjecucion.Text = string.Empty;
            txtFinEjecucion.Text = string.Empty;
            txtCertificado.Text = string.Empty;
            txtMesAprobacion.Text = string.Empty;
            ddlObra.SelectedIndex = 0;

            // Make sure the dropdown is enabled
            ddlObra.Enabled = true;
        }

        private DataTable ObtenerAreas()
        {
            AreaNegocio areaNegocio = new AreaNegocio();
            return areaNegocio.listarddl();
        }

        private void CargarListaLegitimos(string filtro = null)
        {
            try
            {
                var selectedAreas = cblArea.Items.Cast<ListItem>().Where(i => i.Selected).Select(i => i.Text).ToList();
                var selectedLineas = cblLinea.Items.Cast<ListItem>().Where(i => i.Selected).Select(i => i.Text).ToList();

                var selectedEmpresas = cblEmpresa.Items.Cast<ListItem>().Where(i => i.Selected).Select(i => i.Text).ToList();
                var selectedAutorizantes = cblAutorizante.Items.Cast<ListItem>().Where(i => i.Selected).Select(i => i.Text).ToList();
                var selectedFechas = cblFecha.Items.Cast<ListItem>().Where(i => i.Selected).Select(i => i.Value).ToList();
                var selectedEstadoExpedientes = cblEstadoExpediente.Items.Cast<ListItem>().Where(i => i.Selected).Select(i => i.Value).ToList();
                Session["listaLegitimos"] = negocio.listarFiltro(selectedLineas, selectedAreas, selectedFechas, selectedEmpresas, selectedAutorizantes, selectedEstadoExpedientes, filtro);

                dgvLegitimos.DataSource = Session["listaLegitimos"];
                dgvLegitimos.DataBind();
                CalcularSubtotal();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cargar los legítimos: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        protected void dgvLegitimos_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                // Get the ID of the selected row
                int idLegitimo = Convert.ToInt32(dgvLegitimos.SelectedDataKey.Value);

                // Get the list of legitimos from session
                List<Legitimo> legitimosList = (List<Legitimo>)Session["listaLegitimos"];

                // Find the selected legitimo
                Legitimo legitimoSeleccionado = legitimosList.FirstOrDefault(l => l.Id == idLegitimo);

                if (legitimoSeleccionado != null)
                {
                    // Set button text to "Actualizar"
                    btnAgregar.Text = "Actualizar";

                    // Fill form data
                    txtAutorizante.Text = legitimoSeleccionado.CodigoAutorizante;
                    txtExpediente.Text = legitimoSeleccionado.Expediente;

                    if (legitimoSeleccionado.InicioEjecucion.HasValue)
                        txtInicioEjecucion.Text = legitimoSeleccionado.InicioEjecucion.Value.ToString("yyyy-MM-dd");

                    if (legitimoSeleccionado.FinEjecucion.HasValue)
                        txtFinEjecucion.Text = legitimoSeleccionado.FinEjecucion.Value.ToString("yyyy-MM-dd");

                    txtCertificado.Text = legitimoSeleccionado.Certificado.ToString();

                    if (legitimoSeleccionado.MesAprobacion.HasValue)
                        txtMesAprobacion.Text = legitimoSeleccionado.MesAprobacion.Value.ToString("yyyy-MM-dd");

                    // Store the ID of the legitimo being edited in ViewState
                    ViewState["EditingLegitimoId"] = idLegitimo;

                    // Also store the Obra ID so we can use it later in the update process
                    if (legitimoSeleccionado.Obra != null)
                        ViewState["EditingObraId"] = legitimoSeleccionado.Obra.Id;

                    // Update modal title and hide the Obra field
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "UpdateModalAndShow", @"
                $(document).ready(function() {
                    // Change title and button text
                    $('#modalAgregar .modal-title').text('Modificar Legítimo');
                    document.getElementById('" + btnAgregar.ClientID + @"').value = 'Actualizar';
                    
                    // Hide the Obra dropdown and its label (assuming they're in the same div with class mb-3)
                    $('#obraContainer').hide();
                    
                    // Show the modal
                    $('#modalAgregar').modal('show');
                });", true);
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cargar los datos del legítimo: {ex.Message}";
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

        protected void dgvLegitimos_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            try
            {
                var id = dgvLegitimos.DataKeys[e.RowIndex].Value.ToString();
                if (negocio.eliminar(id))
                {
                    lblMensaje.Text = "Legítimo eliminado correctamente.";
                    lblMensaje.CssClass = "alert alert-success";
                    CargarListaLegitimos();
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al eliminar el legítimo: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        protected void dgvLegitimos_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            try
            {
                dgvLegitimos.PageIndex = e.NewPageIndex;
                CargarListaLegitimos();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cambiar de página: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        private DataTable ObtenerObras()
        {
            ObraNegocio barrioNegocio = new ObraNegocio();
            return barrioNegocio.listarddl();
        }

        private DataTable ObtenerEstadosExpedientes()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("ID", typeof(int));
            dt.Columns.Add("NOMBRE", typeof(string));

            DataRow _1 = dt.NewRow();
            _1["ID"] = 0;
            _1["NOMBRE"] = "NO INICIADO";
            dt.Rows.Add(_1);

            DataRow _2 = dt.NewRow();
            _2["ID"] = 1;
            _2["NOMBRE"] = "EN TRAMITE";
            dt.Rows.Add(_2);

            DataRow _3 = dt.NewRow();
            _3["ID"] = 2;
            _3["NOMBRE"] = "DEVENGADO";
            dt.Rows.Add(_3);

            return dt;
        }

        protected void txtExpediente_TextChanged(object sender, EventArgs e)
        {
            TextBox txtExpediente = (TextBox)sender;
            GridViewRow row = (GridViewRow)txtExpediente.NamingContainer;

            int id = (int)dgvLegitimos.DataKeys[row.RowIndex].Value;

            string nuevoExpediente = txtExpediente.Text;

            try
            {
                LegitimoNegocio negocio = new LegitimoNegocio();
                negocio.ActualizarExpediente(id, nuevoExpediente);

                lblMensaje.Text = "Expediente actualizado correctamente.";
                CargarListaLegitimos();
                CalcularSubtotal();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = "Error al actualizar el expediente: " + ex.Message;
            }
        }

        private void CalcularSubtotal()
        {
            decimal subtotal = 0;

            foreach (GridViewRow row in dgvLegitimos.Rows)
            {
                var cellValue = row.Cells[7].Text;
                if (decimal.TryParse(cellValue, System.Globalization.NumberStyles.Currency, null, out decimal monto))
                {
                    subtotal += monto;
                }
            }

            txtSubtotal.Text = subtotal.ToString("C");
        }

        private void BindDropDownList()
        {
            // Clear existing items
            ddlObra.Items.Clear();

            // Set AppendDataBoundItems to true
            ddlObra.AppendDataBoundItems = true;

            // Add empty option
            ddlObra.Items.Add(new ListItem("Seleccione una obra", ""));

            ddlObra.DataSource = ObtenerObras();
            ddlObra.DataTextField = "Nombre";
            ddlObra.DataValueField = "Id";
            ddlObra.DataBind();

            cblArea.DataSource = ObtenerAreas();
            cblArea.DataTextField = "Nombre";
            cblArea.DataValueField = "Id";
            cblArea.DataBind();

            cblEmpresa.DataSource = ObtenerEmpresas();
            cblEmpresa.DataTextField = "Nombre";
            cblEmpresa.DataValueField = "Id";
            cblEmpresa.DataBind();

            cblAutorizante.DataSource = ObtenerLegitimos();
            cblAutorizante.DataTextField = "Nombre";
            cblAutorizante.DataValueField = "Id";
            cblAutorizante.DataBind();

            var meses = Enumerable.Range(0, 36) // 36 meses entre 2024 y 2026
            .Select(i => new DateTime(2024, 1, 1).AddMonths(i))
            .Select(fecha => new
            {
                Texto = fecha.ToString("MMMM yyyy", new System.Globalization.CultureInfo("es-ES")), // Texto: "Enero 2024"
                Valor = fecha.ToString("yyyy-MM-dd")
            });

            cblFecha.DataSource = meses;
            cblFecha.DataTextField = "Texto";
            cblFecha.DataValueField = "Valor";
            cblFecha.DataBind();

            cblLinea.DataSource = ObtenerLineaGestion();
            cblLinea.DataTextField = "Nombre";
            cblLinea.DataValueField = "Id";
            cblLinea.DataBind();

            cblEstadoExpediente.DataSource = ObtenerEstadosExpedientes();
            cblEstadoExpediente.DataTextField = "Nombre";
            cblEstadoExpediente.DataValueField = "Id";
            cblEstadoExpediente.DataBind();
        }

        private DataTable ObtenerLegitimos()
        {
            LegitimoNegocio barrioNegocio = new LegitimoNegocio();
            return barrioNegocio.listarddl();
        }

        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            string filtro = txtBuscar.Text.Trim();
            CargarListaLegitimos(filtro);
        }

        private DataTable ObtenerEmpresas()
        {
            EmpresaNegocio empresaNegocio = new EmpresaNegocio();
            return empresaNegocio.listarddl();
        }

        private DataTable ObtenerLineaGestion()
        {
            LineaGestionNegocio barrioNegocio = new LineaGestionNegocio();
            return barrioNegocio.listarddl();
        }

        protected void BtnClearFilters_Click(object sender, EventArgs e)
        {
            txtBuscar.Text = string.Empty;
            cblArea.ClearSelection();
            cblEmpresa.ClearSelection();
            cblAutorizante.ClearSelection();
            cblFecha.ClearSelection();
            cblLinea.ClearSelection();
            cblEstadoExpediente.ClearSelection();
            CargarListaLegitimos();
        }
    }
}
