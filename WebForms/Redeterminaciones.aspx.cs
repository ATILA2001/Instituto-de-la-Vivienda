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
    public partial class Redeterminaciones : System.Web.UI.Page
    {
        RedeterminacionNegocio negocio = new RedeterminacionNegocio();

        protected void Page_Init(object sender, EventArgs e)
        {
            cblObra.AcceptChanges += OnAcceptChanges;
            cblAutorizante.AcceptChanges += OnAcceptChanges;
            cblEtapa.AcceptChanges += OnAcceptChanges;
        }

        private void OnAcceptChanges(object sender, EventArgs e)
        {
            CargarListaRedeterminacion();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindDropDownList();
                CargarListaRedeterminacion();
            }
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            // Configure validators if we're in editing mode
            if (ViewState["EditingRedeterminacionId"] != null)
            {
                // Disable the Autorizante validator since the field is hidden
                rfvAutorizante.Enabled = false;
            }
            else
            {
                // Enable validators in add mode
                rfvAutorizante.Enabled = true;
            }
        }

        protected void btnShowAddModal_Click(object sender, EventArgs e)
        {
            // Clear any existing data
            LimpiarFormulario();

            // Reset the modal title and button text to "Add" and show Autorizante field
            ScriptManager.RegisterStartupScript(this, this.GetType(), "ResetModalTitleAndShow", @"
        $(document).ready(function() {
            $('#modalAgregar .modal-title').text('Agregar Redeterminación');
            document.getElementById('" + btnAgregar.ClientID + @"').value = 'Agregar';
            
            // Show the Autorizante dropdown and its label
            $('#autorizanteContainer').show();
            
            // Show the modal
            $('#modalAgregar').modal('show');
        });", true);

            btnAgregar.Text = "Agregar";

            // Clear any editing state
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
            cblObra.ClearSelection();
            cblEtapa.ClearSelection();
            cblAutorizante.ClearSelection();
            CargarListaRedeterminacion();
        }

        private void CargarListaRedeterminacion(string filtro = null)
        {
            try
            {
                var selectedObras = cblObra.SelectedValues;
                var selectedAutorizantes = cblAutorizante.SelectedValues;
                var selectedEtapas = cblEtapa.SelectedValues;

                Session["listaRedeterminacion"] = negocio.listar(selectedEtapas, selectedAutorizantes, selectedObras, filtro);
                dgvRedeterminacion.DataSource = Session["listaRedeterminacion"];
                dgvRedeterminacion.DataBind();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cargar las redeterminaciones: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        protected void dgvRedeterminacion_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                // Get the ID of the selected row
                int idRedeterminacion = Convert.ToInt32(dgvRedeterminacion.SelectedDataKey.Value);

                // Get the list of redeterminaciones from session
                List<Redeterminacion> listaRedeterminaciones = (List<Redeterminacion>)Session["listaRedeterminacion"];

                // Find the selected redeterminacion
                Redeterminacion redeterminacionSeleccionada = listaRedeterminaciones.FirstOrDefault(r => r.Id == idRedeterminacion);

                if (redeterminacionSeleccionada != null)
                {
                    // Set button text to "Actualizar"
                    btnAgregar.Text = "Actualizar";

                    // Load the redeterminacion data into the form fields
                    txtExpediente.Text = redeterminacionSeleccionada.Expediente;

                    if (redeterminacionSeleccionada.Salto.HasValue)
                        txtSalto.Text = redeterminacionSeleccionada.Salto.Value.ToString("yyyy-MM-dd");

                    if (redeterminacionSeleccionada.Nro.HasValue)
                        txtNro.Text = redeterminacionSeleccionada.Nro.Value.ToString();

                    txtTipo.Text = redeterminacionSeleccionada.Tipo;

                    if (redeterminacionSeleccionada.Porcentaje.HasValue)
                        txtPorcentaje.Text = redeterminacionSeleccionada.Porcentaje.Value.ToString();

                    txtObservacion.Text = redeterminacionSeleccionada.Observaciones;

                    // Select the corresponding values in the dropdowns
                    if (redeterminacionSeleccionada.Etapa != null)
                        SelectDropDownListByValue(ddlEtapa, redeterminacionSeleccionada.Etapa.Id.ToString());

                    // Store the IDs for update
                    ViewState["EditingRedeterminacionId"] = idRedeterminacion;

                    if (redeterminacionSeleccionada.Autorizante != null)
                    {
                        ViewState["EditingAutorizanteId"] = redeterminacionSeleccionada.Autorizante.Id;
                        ViewState["EditingCodigoAutorizante"] = redeterminacionSeleccionada.CodigoRedet;
                    }

                    // Update modal title and hide the Autorizante field
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "UpdateModalAndShow", @"
                $(document).ready(function() {
                    // Change title and button text
                    $('#modalAgregar .modal-title').text('Modificar Redeterminación');
                    document.getElementById('" + btnAgregar.ClientID + @"').value = 'Actualizar';
                    
                    // Hide the Autorizante dropdown and its label
                    $('#autorizanteContainer').hide();
                    
                    // Show the modal
                    $('#modalAgregar').modal('show');
                });", true);
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cargar los datos de la redeterminación: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        protected void dgvRedeterminacion_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            try
            {
                var id = Convert.ToInt32(dgvRedeterminacion.DataKeys[e.RowIndex].Value);
                if (negocio.eliminar(id))
                {
                    lblMensaje.Text = "Redeterminación eliminada correctamente.";
                    lblMensaje.CssClass = "alert alert-success";
                    CargarListaRedeterminacion();
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al eliminar la redeterminación: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        protected void dgvRedeterminacion_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            try
            {
                dgvRedeterminacion.PageIndex = e.NewPageIndex;
                CargarListaRedeterminacion();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cambiar de página: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        protected void btnAgregar_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            try
            {
                RedeterminacionNegocio redeterminacionNegocio = new RedeterminacionNegocio();
                Redeterminacion redeterminacion = new Redeterminacion();

                // Common data for both add and update operations
                redeterminacion.Expediente = txtExpediente.Text.Trim();
                redeterminacion.Salto = string.IsNullOrWhiteSpace(txtSalto.Text)
                    ? null
                    : (DateTime?)DateTime.Parse(txtSalto.Text);
                redeterminacion.Nro = string.IsNullOrWhiteSpace(txtNro.Text)
                    ? null
                    : (int?)int.Parse(txtNro.Text);
                redeterminacion.Tipo = txtTipo.Text.Trim();
                redeterminacion.Etapa = new EstadoRedet { Id = int.Parse(ddlEtapa.SelectedValue) };
                redeterminacion.Observaciones = txtObservacion.Text.Trim();
                redeterminacion.Porcentaje = decimal.Parse(txtPorcentaje.Text);

                // Check if we're editing an existing redeterminacion or adding a new one
                if (ViewState["EditingRedeterminacionId"] != null)
                {
                    // We're updating an existing redeterminacion
                    redeterminacion.Id = (int)ViewState["EditingRedeterminacionId"];

                    // Use the stored Autorizante info for the update
                    if (ViewState["EditingAutorizanteId"] != null && ViewState["EditingCodigoAutorizante"] != null)
                    {
                        redeterminacion.Autorizante = new Autorizante { Id = (int)ViewState["EditingAutorizanteId"] };
                        redeterminacion.CodigoRedet = ViewState["EditingCodigoAutorizante"].ToString();
                    }

                    if (redeterminacionNegocio.modificar(redeterminacion))
                    {
                        lblMensaje.Text = "Redeterminación modificada exitosamente!";
                        lblMensaje.CssClass = "alert alert-success";

                        // Clear the editing state
                        ViewState["EditingRedeterminacionId"] = null;
                        ViewState["EditingAutorizanteId"] = null;
                        ViewState["EditingCodigoAutorizante"] = null;
                    }
                    else
                    {
                        lblMensaje.Text = "Hubo un problema al modificar la redeterminación.";
                        lblMensaje.CssClass = "alert alert-danger";
                    }
                }
                else
                {
                    // We're adding a new redeterminacion
                    redeterminacion.Autorizante = new Autorizante { CodigoAutorizante = ddlAutorizante.SelectedItem.Text };
                    redeterminacion.CodigoRedet = ddlAutorizante.SelectedItem.Text;

                    if (redeterminacionNegocio.agregar(redeterminacion))
                    {
                        lblMensaje.Text = "Redeterminación agregada exitosamente!";
                        lblMensaje.CssClass = "alert alert-success";
                    }
                    else
                    {
                        lblMensaje.Text = "Hubo un problema al agregar la redeterminación.";
                        lblMensaje.CssClass = "alert alert-danger";
                    }
                }

                // Clear fields
                LimpiarFormulario();

                // Reset the modal title and button text
                ScriptManager.RegisterStartupScript(this, this.GetType(), "ResetModalTitle",
                    "$('#modalAgregar .modal-title').text('Agregar Redeterminación');", true);
                btnAgregar.Text = "Agregar";

                // Hide the modal
                ScriptManager.RegisterStartupScript(this, this.GetType(), "HideModal",
                    "$('#modalAgregar').modal('hide');", true);

                // Refresh the redeterminaciones list
                CargarListaRedeterminacion();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        // Helper method to select dropdown item by value
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

        private DataTable ObtenerTipos()
        {
            EstadoRedetNegocio tipoPagNegocio = new EstadoRedetNegocio();
            return tipoPagNegocio.listarddl();
        }

        private DataTable ObtenerAutorizantes()
        {
            AutorizanteNegocio autorizanteNegocio = new AutorizanteNegocio();
            return autorizanteNegocio.listarddl();
        }

        private void BindDropDownList()
        {
            try
            {
                // Cargar etapas
                DataTable tiposEtapas = ObtenerTipos();
                if (tiposEtapas != null && tiposEtapas.Rows.Count > 0)
                {
                    ddlEtapa.DataSource = tiposEtapas;
                    ddlEtapa.DataTextField = "Nombre";
                    ddlEtapa.DataValueField = "Id";
                    ddlEtapa.DataBind();

                    // Añadir opción predeterminada
                    ddlEtapa.Items.Insert(0, new ListItem("Seleccione una etapa", ""));

                    cblEtapa.DataSource = tiposEtapas;
                    cblEtapa.DataTextField = "Nombre";
                    cblEtapa.DataValueField = "Id";
                    cblEtapa.DataBind();
                }

                // Cargar autorizantes
                DataTable autorizantes = ObtenerAutorizantes();
                if (autorizantes != null && autorizantes.Rows.Count > 0)
                {
                    ddlAutorizante.DataSource = autorizantes;
                    ddlAutorizante.DataTextField = "Nombre";
                    ddlAutorizante.DataValueField = "Id";
                    ddlAutorizante.DataBind();

                    // Añadir opción predeterminada
                    ddlAutorizante.Items.Insert(0, new ListItem("Seleccione un autorizante", ""));

                    cblAutorizante.DataSource = autorizantes;
                    cblAutorizante.DataTextField = "Nombre";
                    cblAutorizante.DataValueField = "Id";
                    cblAutorizante.DataBind();
                }

                // Cargar obras
                DataTable obras = ObtenerObras();
                if (obras != null && obras.Rows.Count > 0)
                {
                    cblObra.DataSource = obras;
                    cblObra.DataTextField = "Nombre";
                    cblObra.DataValueField = "Id";
                    cblObra.DataBind();
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cargar los datos iniciales: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        private DataTable ObtenerObras()
        {
            ObraNegocio empresaNegocio = new ObraNegocio();
            return empresaNegocio.listarddl();
        }

        private DataRow CrearFilaTodos(DataTable table)
        {
            DataRow row = table.NewRow();
            row["Id"] = 0;
            row["Nombre"] = "Todos";
            return row;
        }

        protected void txtExpediente_TextChanged(object sender, EventArgs e)
        {
            // TextBox event handler implementation
        }

        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            string filtro = txtBuscar.Text.Trim();
            CargarListaRedeterminacion(filtro);
        }

        protected void ddlEtapas_SelectedIndexChanged(object sender, EventArgs e)
        {
            DropDownList ddlEtapas = (DropDownList)sender;
            GridViewRow row = (GridViewRow)ddlEtapas.NamingContainer;

            List<Redeterminacion> listaRedeterminaciones = (List<Redeterminacion>)Session["listaRedeterminacion"];
            int id = int.Parse(dgvRedeterminacion.DataKeys[row.RowIndex].Value.ToString());
            Redeterminacion redeterminacion = listaRedeterminaciones.Find(r => r.Id == id);

            if (redeterminacion != null)
            {
                redeterminacion.Etapa.Id = int.Parse(ddlEtapas.SelectedValue);
                RedeterminacionNegocio negocio = new RedeterminacionNegocio();
                negocio.ActualizarEstado(redeterminacion);
                CargarListaRedeterminacion();

                lblMensaje.Text = "Etapa actualizada correctamente.";
                lblMensaje.CssClass = "alert alert-success";
            }
        }

        protected void dgvRedeterminacion_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                DropDownList ddlEtapas = (DropDownList)e.Row.FindControl("ddlEtapas");

                if (ddlEtapas != null)
                {
                    DataTable estados = ObtenerTipos();
                    ddlEtapas.DataSource = estados;
                    ddlEtapas.DataTextField = "Nombre";
                    ddlEtapas.DataValueField = "Id";
                    ddlEtapas.DataBind();

                    string estadoActual = DataBinder.Eval(e.Row.DataItem, "Etapa.Id").ToString();
                    ddlEtapas.SelectedValue = estadoActual;
                }
            }
        }
    }
}