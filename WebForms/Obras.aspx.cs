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
    public partial class Obras : System.Web.UI.Page
    {
        private ObraNegocio negocio = new ObraNegocio();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Cargar la lista completa para el usuario y guardarla en sesión
                Usuario usuarioLogueado = (Usuario)Session["usuario"];
                if (usuarioLogueado != null && usuarioLogueado.Area != null)
                {
                    // Se asume que el método listar con solo usuario devuelve todas las obras de su área
                    // sin filtrar por empresa o barrio inicialmente.
                    List<Obra> listaCompletaUsuario = negocio.listar(usuarioLogueado, new List<string>(), new List<string>(), null);
                    Session["obrasUsuarioCompleto"] = listaCompletaUsuario;
                }
                else
                {
                    // Manejar el caso donde el usuario o su área no están definidos.
                    // Podría redirigir a login o mostrar un error.
                    Session["obrasUsuarioCompleto"] = new List<Obra>(); // Lista vacía para evitar nulls
                }

                BindDropDownList(); // Para los dropdowns del modal
                CargarListaObras();
            }
        }

        // Evento para los filtros de cabecera
        public void OnAcceptChanges(object sender, EventArgs e)
        {
            CargarListaObras();
        }

        protected void btnShowAddModal_Click(object sender, EventArgs e)
        {
            // Clear any existing data
            ClearFormFields();

            // Reset the modal title and button text to "Add" and show Obra field
            ScriptManager.RegisterStartupScript(this, this.GetType(), "ResetModalTitleAndShow", @"
                $(document).ready(function() {
                    $('#modalAgregar .modal-title').text('Agregar Obra');
                    document.getElementById('" + btnAgregar.ClientID + @"').value = 'Agregar';
                    
                    // Show the modal
                    $('#modalAgregar').modal('show');
                });", true);

            btnAgregar.Text = "Agregar";

            // Clear any editing state
            ViewState["EditingObraId"] = null;
        }

        private DataTable ObtenerEmpresas()
        {
            EmpresaNegocio empresaNegocio = new EmpresaNegocio();
            return empresaNegocio.listarddl();
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
                Usuario usuarioLogueado = (Usuario)Session["usuario"];
                if (usuarioLogueado == null || usuarioLogueado.Area == null)
                {
                    lblMensaje.Text = "No se pudo determinar el área del usuario.";
                    lblMensaje.CssClass = "alert alert-warning";
                    dgvObra.DataSource = new List<Obra>();
                    dgvObra.DataBind();
                    return;
                }

                List<Obra> listaCompletaUsuario;
                if (Session["obrasUsuarioCompleto"] == null)
                {
                    // Fallback si la sesión se pierde, aunque debería haberse cargado en Page_Load
                    listaCompletaUsuario = negocio.listar(usuarioLogueado, new List<string>(), new List<string>(), null);
                    Session["obrasUsuarioCompleto"] = listaCompletaUsuario;
                }
                else
                {
                    listaCompletaUsuario = (List<Obra>)Session["obrasUsuarioCompleto"];
                }

                IEnumerable<Obra> listaFiltrada = listaCompletaUsuario;

                // Obtener valores de los filtros de cabecera
                List<string> selectedHeaderEmpresas = new List<string>();
                List<string> selectedHeaderBarrios = new List<string>();

                if (dgvObra.HeaderRow != null)
                {
                    var cblsHeaderEmpresaControl = dgvObra.HeaderRow.FindControl("cblsHeaderEmpresa") as WebForms.CustomControls.CheckBoxListSearch;
                    if (cblsHeaderEmpresaControl != null) selectedHeaderEmpresas = cblsHeaderEmpresaControl.SelectedValues;

                    var cblsHeaderBarrioControl = dgvObra.HeaderRow.FindControl("cblsHeaderBarrio") as WebForms.CustomControls.CheckBoxListSearch;
                    if (cblsHeaderBarrioControl != null) selectedHeaderBarrios = cblsHeaderBarrioControl.SelectedValues;
                }

                // Aplicar filtro de texto general
                string filtroTexto = string.IsNullOrEmpty(filtro) ? txtBuscar.Text.Trim().ToUpper() : filtro.Trim().ToUpper();

                if (!string.IsNullOrEmpty(filtroTexto))
                {
                    listaFiltrada = listaFiltrada.Where(o =>
                        (o.Empresa?.Nombre.ToUpper().Contains(filtroTexto) ?? false) ||
                        (o.Contrata?.Nombre.ToUpper().Contains(filtroTexto) ?? false) ||
                        (o.Barrio?.Nombre.ToUpper().Contains(filtroTexto) ?? false) ||
                        (o.Descripcion?.ToUpper().Contains(filtroTexto) ?? false) ||
                        (o.Numero?.ToString().Contains(filtroTexto) ?? false) ||
                        (o.Año.ToString().Contains(filtroTexto)) ||
                        (o.Etapa.ToString().Contains(filtroTexto)) ||
                        (o.ObraNumero.ToString().Contains(filtroTexto))
                    );
                }

                // Aplicar filtros de cabecera (IDs)
                if (selectedHeaderEmpresas.Any())
                    listaFiltrada = listaFiltrada.Where(o => o.Empresa != null && selectedHeaderEmpresas.Contains(o.Empresa.Id.ToString()));
                if (selectedHeaderBarrios.Any())
                    listaFiltrada = listaFiltrada.Where(o => o.Barrio != null && selectedHeaderBarrios.Contains(o.Barrio.Id.ToString()));

                List<Obra> resultadoFinal = listaFiltrada.ToList();
                Session["listaObra"] = resultadoFinal; // Actualizar la sesión con la lista filtrada para el SelectedIndexChanged
                dgvObra.DataSource = resultadoFinal;
                dgvObra.DataBind();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cargar las obras: {ex.Message}";
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
                    // Common data for both add and update operations
                    obra.Numero = int.Parse(txtNumero.Text.Trim());
                    obra.Año = int.Parse(txtAño.Text.Trim());
                    obra.Etapa = int.Parse(txtEtapa.Text.Trim());
                    obra.ObraNumero = int.Parse(txtObra.Text.Trim());
                    obra.Descripcion = txtDescripcion.Text.Trim();
                    obra.Empresa = new Empresa { Id = int.Parse(ddlEmpresa.SelectedValue) };
                    obra.Contrata = new Contrata { Id = int.Parse(ddlContrata.SelectedValue) };
                    obra.Barrio = new Barrio { Id = int.Parse(ddlBarrio.SelectedValue) };

                    // Get area from user session
                    Usuario usuarioLogueado = (Usuario)Session["usuario"];
                    obra.Area = new Area();
                    obra.Area.Id = usuarioLogueado.Area.Id;
                    obra.Area.Nombre = usuarioLogueado.Area.Nombre;

                    // Check if we're editing or adding
                    if (ViewState["EditingObraId"] != null)
                    {
                        // We're updating an existing obra
                        obra.Id = (int)ViewState["EditingObraId"];

                        if (negocio.modificar(obra))
                        {
                            lblMensaje.Text = "Obra modificada exitosamente!";
                            lblMensaje.CssClass = "alert alert-success";

                            // Clear the editing state
                            ViewState["EditingObraId"] = null;
                        }
                        else
                        {
                            lblMensaje.Text = "Hubo un problema al modificar la obra.";
                            lblMensaje.CssClass = "alert alert-danger";
                        }
                    }
                    else
                    {
                        // We're adding a new obra
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
        }

        protected void dgvObra_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                // Get the ID of the selected row
                int idObra = Convert.ToInt32(dgvObra.SelectedDataKey.Value);

                // Get the list of obras from session
                List<Obra> listaObras = (List<Obra>)Session["listaObra"];

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

                    // Select the corresponding values in the dropdowns
                    if (obraSeleccionada.Empresa != null)
                        SelectDropDownListByValue(ddlEmpresa, obraSeleccionada.Empresa.Id.ToString());

                    if (obraSeleccionada.Contrata != null)
                        SelectDropDownListByValue(ddlContrata, obraSeleccionada.Contrata.Id.ToString());

                    if (obraSeleccionada.Barrio != null)
                        SelectDropDownListByValue(ddlBarrio, obraSeleccionada.Barrio.Id.ToString());

                    // Store the ID of the obra being edited in ViewState
                    ViewState["EditingObraId"] = idObra;

                    // Update modal title and show it
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "UpdateModalAndShow", @"
                        $(document).ready(function() {
                            // Change title and button text
                            $('#modalAgregar .modal-title').text('Modificar Obra');
                            document.getElementById('" + btnAgregar.ClientID + @"').value = 'Actualizar';
                            
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

        protected void dgvObra_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.Header)
            {
                List<Obra> obrasUsuarioCompleto = Session["obrasUsuarioCompleto"] as List<Obra>;

                if (obrasUsuarioCompleto == null || !obrasUsuarioCompleto.Any())
                {
                    return; // No hay datos para poblar los filtros.
                }

                // Poblar filtro de Empresa en cabecera
                var cblsHeaderEmpresa = e.Row.FindControl("cblsHeaderEmpresa") as WebForms.CustomControls.CheckBoxListSearch;
                if (cblsHeaderEmpresa != null)
                {
                    var empresasUnicas = obrasUsuarioCompleto
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

                // Poblar filtro de Barrio en cabecera
                var cblsHeaderBarrio = e.Row.FindControl("cblsHeaderBarrio") as WebForms.CustomControls.CheckBoxListSearch;
                if (cblsHeaderBarrio != null)
                {
                    var barriosUnicos = obrasUsuarioCompleto
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
        private void BindDropDownList()
        {
            ddlEmpresa.Items.Clear();
            ddlContrata.Items.Clear();
            ddlBarrio.Items.Clear();

            // Add empty items to each dropdown
            ddlEmpresa.Items.Add(new ListItem("Seleccione una empresa", ""));
            ddlContrata.Items.Add(new ListItem("Seleccione una contrata", ""));
            ddlBarrio.Items.Add(new ListItem("Seleccione un barrio", ""));

            // Set AppendDataBoundItems property to true for all dropdowns
            ddlEmpresa.AppendDataBoundItems = true;
            ddlContrata.AppendDataBoundItems = true;
            ddlBarrio.AppendDataBoundItems = true;

            ddlEmpresa.DataSource = ObtenerEmpresas();
            ddlEmpresa.DataTextField = "Nombre";
            ddlEmpresa.DataValueField = "Id";
            ddlEmpresa.DataBind();


            ddlContrata.DataSource = ObtenerContratas();
            ddlContrata.DataTextField = "Nombre";
            ddlContrata.DataValueField = "Id";
            ddlContrata.DataBind();

            ddlBarrio.DataSource = ObtenerBarrios();
            ddlBarrio.DataTextField = "Nombre";
            ddlBarrio.DataValueField = "Id";
            ddlBarrio.DataBind();
        }


      
        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            string filtro = txtBuscar.Text.Trim();
            CargarListaObras(filtro);
        }


        protected void BtnClearFilters_Click(object sender, EventArgs e)
        {
            txtBuscar.Text = string.Empty;

            // Limpiar filtros de cabecera
            if (dgvObra.HeaderRow != null)
            {
                ClearHeaderFilter("cblsHeaderEmpresa");
                ClearHeaderFilter("cblsHeaderBarrio");
            }

            CargarListaObras();
            ScriptManager.RegisterStartupScript(this, this.GetType(), "SetFiltersClearedFlag", "sessionStorage.setItem('filtersCleared', 'true');", true);
        }

        private void ClearHeaderFilter(string controlId)
        {
            if (dgvObra.HeaderRow != null)
            {
                var control = dgvObra.HeaderRow.FindControl(controlId) as WebForms.CustomControls.CheckBoxListSearch;
                if (control != null)
                {
                    control.ClearSelection();

                    // Lógica para limpiar la sesión/contexto si el control CheckBoxListSearch lo requiere internamente
                    string controlInstanceId = control.ID;
                    string sessionKey = $"CheckBoxListSearch_SelectedValues_{controlInstanceId}";
                    if (HttpContext.Current.Session[sessionKey] != null)
                    {
                        HttpContext.Current.Session.Remove(sessionKey);
                    }
                    string contextKey = $"CheckBoxListSearch_{controlInstanceId}_ContextSelectedValues";
                    if (HttpContext.Current.Items.Contains(contextKey))
                    {
                        HttpContext.Current.Items.Remove(contextKey);
                    }
                }
            }
        }


    }
}