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
    public partial class ObrasAdmin : System.Web.UI.Page
    {
        private ObraNegocio negocio = new ObraNegocio();

        public void OnAcceptChanges(object sender, EventArgs e)
        {
            CargarListaObras();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Cargar la lista completa una vez y guardarla en sesión
                List<Obra> listaCompleta = negocio.listar(new List<string>(), new List<string>(), new List<string>(), null);
                Session["obrasCompleto"] = listaCompleta;

                BindDropDownList();
                CargarListaObras();
            }
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            // Configure validators if we're in editing mode
            if (ViewState["EditingObraId"] != null)
            {
                // Disable the Area validator since the field is hidden
                rfvArea.Enabled = false;
            }
            else
            {
                // Enable validators in add mode
                rfvArea.Enabled = true;
            }
        }

        protected void btnShowAddModal_Click(object sender, EventArgs e)
        {
            // Clear any existing data
            ClearFormFields();

            // Reset the modal title and button text to "Add" and show Area field
            ScriptManager.RegisterStartupScript(this, this.GetType(), "ResetModalTitleAndShow", @"
                $(document).ready(function() {
                    $('#modalAgregar .modal-title').text('Agregar Obra');
                    document.getElementById('" + btnAgregar.ClientID + @"').value = 'Agregar';
                    
                    // Show the Area dropdown and its container
                    $('#areaContainer').show();
                    
                    // Show the modal
                    $('#modalAgregar').modal('show');
                });", true);

            btnAgregar.Text = "Agregar";

            // Clear any editing state
            ViewState["EditingObraId"] = null;
            ViewState["EditingAreaId"] = null;
            ViewState["EditingAreaNombre"] = null;
        }

        protected void ddlFiltroEmpresa_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarListaObras();
        }

        protected void ddlBarrioFiltro_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarListaObras();
        }

        protected void ddlAreaFiltro_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarListaObras();
        }

        protected void btnLimpiar_Click(object sender, EventArgs e)
        {
            ClearFormFields();
        }

        private DataTable ObtenerEmpresas()
        {
            EmpresaNegocio empresaNegocio = new EmpresaNegocio();
            return empresaNegocio.listarddl();
        }

        private DataTable ObtenerAreas()
        {
            AreaNegocio areaNegocio = new AreaNegocio();
            return areaNegocio.listarddl();
        }

        private DataTable ObtenerContratas()
        {
            ContrataNegocio contrataNegocio = new ContrataNegocio();
            return contrataNegocio.listarddl();
        }

        private DataTable ObtenerBarrios()
        {
            BarrioNegocio barrioNegocio = new BarrioNegocio();
            return barrioNegocio.listarddl();
        }

        private void CargarListaObras(string filtro = null)
        {
            try
            {
                List<Obra> listaCompleta;
                if (Session["obrasCompleto"] == null)
                {
                    // Fallback si la sesión se pierde
                    listaCompleta = negocio.listar(new List<string>(), new List<string>(), new List<string>(), null);
                    Session["obrasCompleto"] = listaCompleta;
                }
                else
                {
                    listaCompleta = (List<Obra>)Session["obrasCompleto"];
                }

                IEnumerable<Obra> listaFiltrada = listaCompleta;

                // Obtener valores de los filtros laterales existentes
                var selectedAreas = cblArea.SelectedValues;
                var selectedEmpresas = cblEmpresa.SelectedValues;
                var selectedBarrios = cblBarrio.SelectedValues;

                // Obtener valores de los filtros de cabecera
                List<string> selectedHeaderAreas = new List<string>();
                List<string> selectedHeaderEmpresas = new List<string>();
                List<string> selectedHeaderBarrios = new List<string>();
                //List<string> selectedHeaderLineasGestion = new List<string>();
                //List<string> selectedHeaderProyectos = new List<string>();

                if (dgvObra.HeaderRow != null)
                {
                    var cblsHeaderAreaControl = dgvObra.HeaderRow.FindControl("cblsHeaderArea") as WebForms.CustomControls.TreeViewSearch;
                    if (cblsHeaderAreaControl != null) selectedHeaderAreas = cblsHeaderAreaControl.SelectedValues;

                    var cblsHeaderEmpresaControl = dgvObra.HeaderRow.FindControl("cblsHeaderEmpresa") as WebForms.CustomControls.TreeViewSearch;
                    if (cblsHeaderEmpresaControl != null) selectedHeaderEmpresas = cblsHeaderEmpresaControl.SelectedValues;

                    var cblsHeaderBarrioControl = dgvObra.HeaderRow.FindControl("cblsHeaderBarrio") as WebForms.CustomControls.TreeViewSearch;
                    if (cblsHeaderBarrioControl != null) selectedHeaderBarrios = cblsHeaderBarrioControl.SelectedValues;

                    //var cblsHeaderLineaGestionControl = dgvObra.HeaderRow.FindControl("cblsHeaderLineaGestion") as WebForms.CustomControls.TreeViewSearch;
                    //if (cblsHeaderLineaGestionControl != null) selectedHeaderLineasGestion = cblsHeaderLineaGestionControl.SelectedValues;

                    //var cblsHeaderProyectoControl = dgvObra.HeaderRow.FindControl("cblsHeaderProyecto") as WebForms.CustomControls.TreeViewSearch;
                    //if (cblsHeaderProyectoControl != null) selectedHeaderProyectos = cblsHeaderProyectoControl.SelectedValues;
                }

                // Aplicar filtro de texto general
                string filtroTextoGeneral = string.IsNullOrEmpty(filtro) ? txtBuscar.Text.Trim().ToUpper() : filtro.Trim().ToUpper();

                if (!string.IsNullOrEmpty(filtroTextoGeneral))
                {
                    listaFiltrada = listaFiltrada.Where(o =>
                        (o.Area?.Nombre.ToUpper().Contains(filtroTextoGeneral) ?? false) ||
                        (o.Empresa?.Nombre.ToUpper().Contains(filtroTextoGeneral) ?? false) ||
                        (o.Contrata?.Nombre.ToUpper().Contains(filtroTextoGeneral) ?? false) ||
                        (o.Barrio?.Nombre.ToUpper().Contains(filtroTextoGeneral) ?? false) ||
                        (o.Descripcion?.ToUpper().Contains(filtroTextoGeneral) ?? false) ||
                        (o.LineaGestion?.Nombre?.ToUpper().Contains(filtroTextoGeneral) ?? false) ||
                        (o.Proyecto?.Proyecto?.ToUpper().Contains(filtroTextoGeneral) ?? false) ||
                        (o.Numero?.ToString().Contains(filtroTextoGeneral) ?? false) ||
                        (o.Año.ToString().Contains(filtroTextoGeneral)) ||
                        (o.Etapa.ToString().Contains(filtroTextoGeneral)) ||
                        (o.ObraNumero.ToString().Contains(filtroTextoGeneral))
                    );
                }

                // Aplicar filtros laterales existentes
                if (selectedAreas.Any())
                    listaFiltrada = listaFiltrada.Where(o => !string.IsNullOrEmpty(o.Area.Nombre) && selectedAreas.Contains(o.Area.Id.ToString()));
                if (selectedEmpresas.Any())
                    listaFiltrada = listaFiltrada.Where(o => !string.IsNullOrEmpty(o.Empresa.Nombre) && selectedEmpresas.Contains(o.Empresa.Id.ToString()));
                if (selectedBarrios.Any())
                    listaFiltrada = listaFiltrada.Where(o => !string.IsNullOrEmpty(o.Barrio.Nombre) && selectedBarrios.Contains(o.Barrio.Id.ToString()));

                // Aplicar filtros de cabecera
                if (selectedHeaderAreas.Any())
                    listaFiltrada = listaFiltrada.Where(o => !string.IsNullOrEmpty(o.Area.Nombre) && selectedHeaderAreas.Contains(o.Area.Id.ToString()));
                if (selectedHeaderEmpresas.Any())
                    listaFiltrada = listaFiltrada.Where(o => !string.IsNullOrEmpty(o.Empresa.Nombre) && selectedHeaderEmpresas.Contains(o.Empresa.Id.ToString()));
                if (selectedHeaderBarrios.Any())
                    listaFiltrada = listaFiltrada.Where(o => !string.IsNullOrEmpty(o.Barrio.Nombre) && selectedHeaderBarrios.Contains(o.Barrio.Id.ToString()));
                //if (selectedHeaderLineasGestion.Any())
                //    listaFiltrada = listaFiltrada.Where(o => o.LineaGestion != null && selectedHeaderLineasGestion.Contains(o.LineaGestion.Id.ToString()));
                //if (selectedHeaderProyectos.Any())
                //    listaFiltrada = listaFiltrada.Where(o => o.Proyecto != null && selectedHeaderProyectos.Contains(o.Proyecto.Id.ToString()));

                List<Obra> resultadoFinal = listaFiltrada.ToList();
                Session["listaObraAdmin"] = resultadoFinal;
                dgvObra.DataSource = resultadoFinal;
                dgvObra.DataBind();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cargar las obras: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }

        }

        protected void dgvObra_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.Header)
            {
                List<Obra> obrasCompleto = Session["obrasCompleto"] as List<Obra>;

                if (obrasCompleto == null)
                {
                    obrasCompleto = negocio.listar(new List<string>(), new List<string>(), new List<string>(), null);
                    Session["obrasCompleto"] = obrasCompleto;
                }

                if (obrasCompleto == null || !obrasCompleto.Any())
                {
                    return; // No hay datos para poblar los filtros.
                }

                // Poblar filtro de Área
                var cblsHeaderArea = e.Row.FindControl("cblsHeaderArea") as WebForms.CustomControls.TreeViewSearch;
                if (cblsHeaderArea != null)
                {
                    var areasUnicas = obrasCompleto
                        .Where(o => o.Area != null && !string.IsNullOrEmpty(o.Area.Nombre))
                        .Select(o => new { Id = o.Area.Id, Nombre = o.Area.Nombre })
                        .Distinct()
                        .OrderBy(x => x.Nombre)
                        .ToList();
                    cblsHeaderArea.DataTextField = "Nombre";
                    cblsHeaderArea.DataValueField = "Id";
                    cblsHeaderArea.DataSource = areasUnicas;
                    cblsHeaderArea.DataBind();
                }

                // Poblar filtro de Empresa
                var cblsHeaderEmpresa = e.Row.FindControl("cblsHeaderEmpresa") as WebForms.CustomControls.TreeViewSearch;
                if (cblsHeaderEmpresa != null)
                {
                    var empresasUnicas = obrasCompleto
                        .Where(o => o.Empresa != null && !string.IsNullOrEmpty(o.Empresa.Nombre))
                        .Select(o => new { Id = o.Empresa.Id, Nombre = o.Empresa.Nombre })
                        .Distinct()
                        .OrderBy(x => x.Nombre)
                        .ToList();
                    cblsHeaderEmpresa.DataTextField = "Nombre";
                    cblsHeaderEmpresa.DataValueField = "Id";
                    cblsHeaderEmpresa.DataSource = empresasUnicas;
                    cblsHeaderEmpresa.DataBind();
                }

                // Poblar filtro de Barrio
                var cblsHeaderBarrio = e.Row.FindControl("cblsHeaderBarrio") as WebForms.CustomControls.TreeViewSearch;
                if (cblsHeaderBarrio != null)
                {
                    var barriosUnicos = obrasCompleto
                        .Where(o => o.Barrio != null && !string.IsNullOrEmpty(o.Barrio.Nombre))
                        .Select(o => new { Id = o.Barrio.Id, Nombre = o.Barrio.Nombre })
                        .Distinct()
                        .OrderBy(x => x.Nombre)
                        .ToList();
                    cblsHeaderBarrio.DataTextField = "Nombre";
                    cblsHeaderBarrio.DataValueField = "Id";
                    cblsHeaderBarrio.DataSource = barriosUnicos;
                    cblsHeaderBarrio.DataBind();
                }

                // Poblar filtro de Línea de Gestión
                var cblsHeaderLineaGestion = e.Row.FindControl("cblsHeaderLineaGestion") as WebForms.CustomControls.TreeViewSearch;
                if (cblsHeaderLineaGestion != null)
                {
                    var lineasUnicas = obrasCompleto
                        .Where(o => o.LineaGestion != null && !string.IsNullOrEmpty(o.LineaGestion.Nombre))
                        .Select(o => new { Id = o.LineaGestion.Id, Nombre = o.LineaGestion.Nombre })
                        .Distinct()
                        .OrderBy(x => x.Nombre)
                        .ToList();
                    cblsHeaderLineaGestion.DataTextField = "Nombre";
                    cblsHeaderLineaGestion.DataValueField = "Id";
                    cblsHeaderLineaGestion.DataSource = lineasUnicas;
                    cblsHeaderLineaGestion.DataBind();
                }

                // Poblar filtro de Proyecto
                var cblsHeaderProyecto = e.Row.FindControl("cblsHeaderProyecto") as WebForms.CustomControls.TreeViewSearch;
                if (cblsHeaderProyecto != null)
                {
                    var proyectosUnicos = obrasCompleto
                        .Where(o => o.Proyecto != null && !string.IsNullOrEmpty(o.Proyecto.Proyecto))
                        .Select(o => new { Id = o.Proyecto.Id, Nombre = o.Proyecto.Proyecto })
                        .Distinct()
                        .OrderBy(x => x.Nombre)
                        .ToList();
                    cblsHeaderProyecto.DataTextField = "Nombre";
                    cblsHeaderProyecto.DataValueField = "Id";
                    cblsHeaderProyecto.DataSource = proyectosUnicos;
                    cblsHeaderProyecto.DataBind();
                }
            }
        }


        protected void dgvObra_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                // Get the ID of the selected row
                int idObra = Convert.ToInt32(dgvObra.SelectedDataKey.Value);

                // Get the list of obras from session
                List<Obra> listaObras = (List<Obra>)Session["listaObraAdmin"];

                // Find the selected obra
                Obra obraSeleccionada = listaObras.FirstOrDefault(o => o.Id == idObra);

                if (obraSeleccionada != null)
                {
                    // Set button text to "Actualizar"
                    btnAgregar.Text = "Actualizar";

                    // Load the obra data into the form fields
                    txtNumero.Text = obraSeleccionada.Numero?.ToString();
                    txtAño.Text = obraSeleccionada.Año.ToString();
                    txtEtapa.Text = obraSeleccionada.Etapa.ToString();
                    txtObra.Text = obraSeleccionada.ObraNumero.ToString();
                    txtDescripcion.Text = obraSeleccionada.Descripcion;

                    // Select the corresponding values in the dropdowns (except Area which will be hidden)
                    if (obraSeleccionada.Empresa != null)
                        SelectDropDownListByValue(ddlEmpresa, obraSeleccionada.Empresa.Id.ToString());

                    if (obraSeleccionada.Contrata != null)
                        SelectDropDownListByValue(ddlContrata, obraSeleccionada.Contrata.Id.ToString());

                    if (obraSeleccionada.Barrio != null)
                        SelectDropDownListByValue(ddlBarrio, obraSeleccionada.Barrio.Id.ToString());

                    // Store the ID of the obra being edited in ViewState
                    ViewState["EditingObraId"] = idObra;

                    // Store the Area data for use during update
                    if (obraSeleccionada.Area != null)
                    {
                        ViewState["EditingAreaId"] = obraSeleccionada.Area.Id;
                        ViewState["EditingAreaNombre"] = obraSeleccionada.Area.Nombre;
                    }

                    // Update modal title and hide the Area field
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "UpdateModalAndShow", @"
                        $(document).ready(function() {
                            // Change title and button text
                            $('#modalAgregar .modal-title').text('Modificar Obra');
                            document.getElementById('" + btnAgregar.ClientID + @"').value = 'Actualizar';
                            
                            // Hide the Area dropdown and its container
                            $('#areaContainer').hide();
                            
                            // Show the modal
                            $('#modalAgregar').modal('show');
                        });", true);
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cargar los datos de la obra: {ex.Message}";
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

        protected void dgvObra_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            try
            {
                var id = Convert.ToInt32(dgvObra.DataKeys[e.RowIndex].Value);
                if (negocio.eliminar(id))
                {
                    lblMensaje.Text = "Obra eliminada correctamente.";
                    lblMensaje.CssClass = "alert alert-success";
                    CargarListaObras(); // Actualizar el GridView
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al eliminar la obra: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        protected void dgvObra_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            try
            {
                // Cambiar el índice de la página
                dgvObra.PageIndex = e.NewPageIndex;

                // Refrescar el listado de empresas
                CargarListaObras();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cambiar de página: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        protected void btnAgregar_Click(object sender, EventArgs e)
        {
            // If page is valid (all validators passed), proceed with creating the new object
            if (Page.IsValid)
            {
                ObraNegocio negocio = new ObraNegocio();
                Obra obra = new Obra();

                try
                {
                    // Assign common values to the obra
                    obra.Numero = int.Parse(txtNumero.Text.Trim());
                    obra.Año = int.Parse(txtAño.Text.Trim());
                    obra.Etapa = int.Parse(txtEtapa.Text.Trim());
                    obra.ObraNumero = int.Parse(txtObra.Text.Trim());
                    obra.Descripcion = txtDescripcion.Text.Trim();
                    obra.Empresa = new Empresa { Id = int.Parse(ddlEmpresa.SelectedValue) };
                    obra.Contrata = new Contrata { Id = int.Parse(ddlContrata.SelectedValue) };
                    obra.Barrio = new Barrio { Id = int.Parse(ddlBarrio.SelectedValue) };

                    // Check if we're editing an existing obra or adding a new one
                    if (ViewState["EditingObraId"] != null)
                    {
                        // We're updating an existing obra
                        obra.Id = (int)ViewState["EditingObraId"];

                        // Use the stored Area data for the update
                        if (ViewState["EditingAreaId"] != null)
                        {
                            obra.Area = new Area
                            {
                                Id = (int)ViewState["EditingAreaId"],
                                Nombre = ViewState["EditingAreaNombre"]?.ToString()
                            };
                        }

                        if (negocio.modificar(obra))
                        {
                            lblMensaje.Text = "Obra modificada exitosamente!";
                            lblMensaje.CssClass = "alert alert-success";

                            // Clear the editing state
                            ViewState["EditingObraId"] = null;
                            ViewState["EditingAreaId"] = null;
                            ViewState["EditingAreaNombre"] = null;
                        }
                        else
                        {
                            lblMensaje.Text = "Hubo un problema al modificar la obra.";
                            lblMensaje.CssClass = "alert alert-danger";
                        }
                    }
                    else
                    {
                        // We're adding a new obra - use the selected Area
                        obra.Area = new Area
                        {
                            Id = int.Parse(ddlArea.SelectedValue),
                            Nombre = ddlArea.SelectedItem.Text
                        };

                        if (negocio.agregar(obra))
                        {
                            lblMensaje.Text = "Obra agregada exitosamente!";
                            lblMensaje.CssClass = "alert alert-success";
                        }
                        else
                        {
                            lblMensaje.Text = "Hubo un problema al agregar la obra.";
                            lblMensaje.CssClass = "alert alert-danger";
                        }
                    }

                    // Clear fields
                    ClearFormFields();

                    // Reset the modal title and button text
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "ResetModalTitle",
                        "$('#modalAgregar .modal-title').text('Agregar Obra');", true);
                    btnAgregar.Text = "Agregar";

                    // Hide the modal
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "HideModal",
                        "$('#modalAgregar').modal('hide');", true);

                    // Refresh the works list
                    CargarListaObras();
                }
                catch (Exception ex)
                {
                    lblMensaje.Text = $"Error: {ex.Message}";
                    lblMensaje.CssClass = "alert alert-danger";
                }
            }
        }

        private void ClearFormFields()
        {
            txtNumero.Text = string.Empty;
            txtAño.Text = string.Empty;
            txtEtapa.Text = string.Empty;
            txtObra.Text = string.Empty;
            txtDescripcion.Text = string.Empty;
            ddlEmpresa.SelectedIndex = 0;
            ddlContrata.SelectedIndex = 0;
            ddlBarrio.SelectedIndex = 0;
            ddlArea.SelectedIndex = 0;
        }
        private void BindDropDownList()
        {
            ddlEmpresa.Items.Clear();
            ddlArea.Items.Clear();
            ddlContrata.Items.Clear();
            ddlBarrio.Items.Clear();

            // Add empty items to each dropdown
            ddlEmpresa.Items.Add(new ListItem("Seleccione una empresa", ""));
            ddlArea.Items.Add(new ListItem("Seleccione un área", ""));
            ddlContrata.Items.Add(new ListItem("Seleccione una contrata", ""));
            ddlBarrio.Items.Add(new ListItem("Seleccione un barrio", ""));

            // Set AppendDataBoundItems property to true for all dropdowns
            ddlEmpresa.AppendDataBoundItems = true;
            ddlArea.AppendDataBoundItems = true;
            ddlContrata.AppendDataBoundItems = true;
            ddlBarrio.AppendDataBoundItems = true;

            ddlEmpresa.DataSource = ObtenerEmpresas();
            ddlEmpresa.DataTextField = "Nombre";
            ddlEmpresa.DataValueField = "Id";
            ddlEmpresa.DataBind();

            ddlArea.DataSource = ObtenerAreas();    // Método para obtener los datos de los Barrios
            ddlArea.DataTextField = "Nombre";
            ddlArea.DataValueField = "Id";
            ddlArea.DataBind();


            ddlContrata.DataSource = ObtenerContratas();
            ddlContrata.DataTextField = "Nombre";
            ddlContrata.DataValueField = "Id";
            ddlContrata.DataBind();

            ddlBarrio.DataSource = ObtenerBarrios();
            ddlBarrio.DataTextField = "Nombre";
            ddlBarrio.DataValueField = "Id";
            ddlBarrio.DataBind();

            cblBarrio.DataSource = ObtenerBarrios();
            cblBarrio.DataTextField = "Nombre";
            cblBarrio.DataValueField = "Id";
            cblBarrio.DataBind();


            cblEmpresa.DataSource = ObtenerEmpresas();
            cblEmpresa.DataTextField = "Nombre";
            cblEmpresa.DataValueField = "Id";
            cblEmpresa.DataBind();

            cblArea.DataSource = ObtenerAreas();
            cblArea.DataTextField = "Nombre";
            cblArea.DataValueField = "Id";
            cblArea.DataBind();


        }

        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            string filtro = txtBuscar.Text.Trim();
            CargarListaObras(filtro);
        }

        protected void BtnClearFilters_Click(object sender, EventArgs e)
        {
            txtBuscar.Text = string.Empty;

            // Limpiar filtros laterales existentes
            cblArea.ClearSelection();
            cblBarrio.ClearSelection();
            cblEmpresa.ClearSelection();

            // Limpiar filtros de cabecera
            if (dgvObra.HeaderRow != null)
            {
                ClearFilter("cblsHeaderArea");
                ClearFilter("cblsHeaderEmpresa");
                ClearFilter("cblsHeaderBarrio");
                ClearFilter("cblsHeaderLineaGestion");
                ClearFilter("cblsHeaderProyecto");
            }

            CargarListaObras();
            ScriptManager.RegisterStartupScript(this, this.GetType(), "SetFiltersClearedFlag", "sessionStorage.setItem('filtersCleared', 'true');", true);
        }

        private void ClearFilter(string controlId)
        {
            if (dgvObra.HeaderRow != null)
            {
                var control = dgvObra.HeaderRow.FindControl(controlId) as WebForms.CustomControls.TreeViewSearch;
                if (control != null)
                {
                    control.ClearSelection();

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