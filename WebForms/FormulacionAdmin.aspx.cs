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
    public partial class FormulacionAdmin : System.Web.UI.Page
    {
        private FormulacionNegocio negocio = new FormulacionNegocio();

        protected void Page_Init(object sender, EventArgs e)
        {
            cblObra.AcceptChanges += OnAcceptChanges;
        }

        private void OnAcceptChanges(object sender, EventArgs e)
        {
            CargarListaFormulaciones();
        }
        protected void Page_PreRender(object sender, EventArgs e)
        {
            // Configurar validadores según si estamos en modo edición
            if (ViewState["EditingFormulacionId"] != null)
            {
                // Desactivar el validador de obra en modo edición ya que el campo está oculto
                rfvObra.Enabled = false;
            }
            else
            {
                // Activar validadores en modo agregar
                rfvObra.Enabled = true;
            }
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindDropDownList();
                CargarListaFormulaciones();
            }
        }

        protected void btnShowAddModal_Click(object sender, EventArgs e)
        {
            // Limpiar datos existentes
            ClearFormFields();

            // Restablecer el título del modal y el texto del botón a "Agregar" y mostrar el campo de obra
            ScriptManager.RegisterStartupScript(this, this.GetType(), "ResetModalTitleAndShow", @"
                $(document).ready(function() {
                    $('#modalAgregar .modal-title').text('Agregar Formulación');
                    document.getElementById('" + btnAgregar.ClientID + @"').value = 'Agregar';
                    
                    // Mostrar el campo de obra
                    $('.col-12:first').show();
                    
                    $('#modalAgregar').modal('show');
                });", true);

            btnAgregar.Text = "Agregar";

            // Limpiar cualquier estado de edición
            ViewState["EditingFormulacionId"] = null;
            ViewState["EditingObraId"] = null;
        }

        private DataTable ObtenerObras()
        {
            ObraNegocio obraNegocio = new ObraNegocio();
            return obraNegocio.listarddlFormulacion();
        }

        private DataTable ObtenerUnidadesMedida()
        {
            UnidadMedidaNegocio unidadMedidaNegocio = new UnidadMedidaNegocio();
            return unidadMedidaNegocio.listarddl();
        }

        private void CargarListaFormulaciones(string filtro = null)
        {
            try
            {
                var selectedObras = cblObra.SelectedValues;

                // Si hay obras seleccionadas, filtrar por ellas
                List<Formulacion> listaFormulaciones;
                if (selectedObras.Count > 0)
                {
                    listaFormulaciones = new List<Formulacion>();
                    foreach (var idObra in selectedObras)
                    {
                        if (int.TryParse(idObra, out int obraId))
                        {
                            listaFormulaciones.AddRange(negocio.listar());
                        }
                    }
                }
                else
                {
                    // Obtener todas las formulaciones
                    listaFormulaciones = negocio.listar();
                }



                Session["listaFormulacionAdmin"] = listaFormulaciones;
                dgvFormulacion.DataSource = listaFormulaciones;
                dgvFormulacion.DataBind();
                BindDropDownList();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cargar las formulaciones: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        protected void dgvFormulacion_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                // Obtener el ID de la fila seleccionada
                int idFormulacion = Convert.ToInt32(dgvFormulacion.SelectedDataKey.Value);

                // Obtener la lista de formulaciones de la sesión
                List<Formulacion> listaFormulaciones = (List<Formulacion>)Session["listaFormulacionAdmin"];

                // Encontrar la formulación seleccionada
                Formulacion formulacionSeleccionada = listaFormulaciones.FirstOrDefault(f => f.Id == idFormulacion);

                if (formulacionSeleccionada != null)
                {
                    // Establecer el texto del botón a "Actualizar"
                    btnAgregar.Text = "Actualizar";

                    // Cargar los datos de la formulación en los campos del formulario
                    if (formulacionSeleccionada.Obra != null)
                    {
                        SelectDropDownListByValue(ddlObra, formulacionSeleccionada.Obra.Id.ToString());
                        // Guardar el ID de la obra en el ViewState para usarlo en la actualización
                        ViewState["EditingObraId"] = formulacionSeleccionada.Obra.Id;
                    }

                    txtMonto26.Text = formulacionSeleccionada.Monto_26.ToString();
                    txtMonto27.Text = formulacionSeleccionada.Monto_27.ToString();
                    txtMonto28.Text = formulacionSeleccionada.Monto_28.ToString();
                    txtPPI.Text = formulacionSeleccionada.Ppi.ToString();
                    txtTechos.Text = formulacionSeleccionada.Techos2026.ToString();
                    txtMesBase.Text = formulacionSeleccionada.MesBase?.ToString("yyyy-MM-dd") ?? string.Empty;

                    if (formulacionSeleccionada.UnidadMedida != null)
                    {
                        SelectDropDownListByValue(ddlUnidadMedida, formulacionSeleccionada.UnidadMedida.Id.ToString());
                    }
                    txtValorMedida.Text = formulacionSeleccionada.ValorMedida.ToString();
                    txtObservaciones.Text = formulacionSeleccionada.Observacion;
                    if (formulacionSeleccionada.Prioridad != null)
                    {
                        SelectDropDownListByValue(ddlPrioridades, formulacionSeleccionada.Prioridad.Id.ToString());
                    }
                    else
                    {
                        ddlPrioridades.SelectedIndex = 0; // Seleccionar el primer elemento si no hay prioridad
                    }

                    // Almacenar el ID de la formulación que se está editando en ViewState
                    ViewState["EditingFormulacionId"] = idFormulacion;

                    // Actualizar el título del modal
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "UpdateModalAndShow", @"
                        $(document).ready(function() {
                            // Cambiar título y texto del botón
                            $('#modalAgregar .modal-title').text('Modificar Formulación');
                            document.getElementById('" + btnAgregar.ClientID + @"').value = 'Actualizar';
                            
                            // Ocultar el campo de obra (el primer div col-12)
                            $('.col-12:first').hide();
                            
                            // Mostrar el modal
                            $('#modalAgregar').modal('show');
                        });", true);
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cargar los datos de la formulación: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        // Método auxiliar para seleccionar un elemento de un DropDownList por su valor
        private void SelectDropDownListByValue(DropDownList dropDown, string value)
        {
            // Limpiar cualquier selección actual
            dropDown.ClearSelection();

            // Intentar encontrar y seleccionar el elemento
            ListItem item = dropDown.Items.FindByValue(value);
            if (item != null)
            {
                item.Selected = true;
            }
        }

        protected void dgvFormulacion_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            try
            {
                var id = Convert.ToInt32(dgvFormulacion.DataKeys[e.RowIndex].Value);
                if (negocio.eliminar(id))
                {
                    lblMensaje.Text = "Formulación eliminada correctamente.";
                    lblMensaje.CssClass = "alert alert-success";
                    CargarListaFormulaciones(); // Actualizar el GridView
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al eliminar la formulación: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        protected void btnAgregar_Click(object sender, EventArgs e)
        {
            // Si la página es válida (todos los validadores pasaron), proceder con la creación del nuevo objeto
            if (Page.IsValid)
            {
                FormulacionNegocio negocio = new FormulacionNegocio();
                Formulacion formulacion = new Formulacion();

                try
                {
                    // Verificar que la unidad de medida esté seleccionada
                    if (string.IsNullOrEmpty(ddlUnidadMedida.SelectedValue) || ddlUnidadMedida.SelectedValue == "")
                    {
                        lblMensaje.Text = "Error: Debe seleccionar una unidad de medida.";
                        lblMensaje.CssClass = "alert alert-danger";
                        ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowModalWithError",
                            "$('#modalAgregar').modal('show');", true);
                        return;
                    }

                    // Asignar valores comunes a la formulación
                    if (ViewState["EditingFormulacionId"] != null) // Modo edición
                    {
                        formulacion.Id = (int)ViewState["EditingFormulacionId"];

                        // Verificar que exista el ID de obra para edición
                        if (ViewState["EditingObraId"] == null)
                        {
                            lblMensaje.Text = "Error: No se pudo determinar la obra a editar.";
                            lblMensaje.CssClass = "alert alert-danger";
                            ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowModalWithError",
                                "$('#modalAgregar').modal('show');", true);
                            return;
                        }

                        formulacion.Obra = new Obra { Id = (int)ViewState["EditingObraId"] };
                    }
                    else // Modo agregar
                    {
                        // Verificar que se haya seleccionado una obra
                        if (string.IsNullOrEmpty(ddlObra.SelectedValue))
                        {
                            lblMensaje.Text = "Error: Debe seleccionar una obra.";
                            lblMensaje.CssClass = "alert alert-danger";
                            ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowModalWithError",
                                "$('#modalAgregar').modal('show');", true);
                            return;
                        }

                        formulacion.Obra = new Obra { Id = int.Parse(ddlObra.SelectedValue) };
                    }

                    // Asignar el resto de valores comunes
                    formulacion.Monto_26 = decimal.Parse(txtMonto26.Text.Trim());
                    formulacion.Monto_27 = decimal.Parse(txtMonto27.Text.Trim());
                    formulacion.Monto_28 = decimal.Parse(txtMonto28.Text.Trim());
                    formulacion.Ppi = int.Parse(txtPPI.Text.Trim());
                    formulacion.Techos2026 = decimal.Parse(txtTechos.Text.Trim());

                    // Manejar valores que pueden ser nulos
                    formulacion.MesBase = !string.IsNullOrWhiteSpace(txtMesBase.Text.Trim())
                        ? DateTime.Parse(txtMesBase.Text.Trim())
                        : (DateTime?)null;

                    formulacion.Observacion = !string.IsNullOrWhiteSpace(txtObservaciones.Text.Trim())
                        ? txtObservaciones.Text.Trim()
                        : null;

                    // Asignar prioridad si se seleccionó
                    if (!string.IsNullOrEmpty(ddlPrioridades.SelectedValue) && ddlPrioridades.SelectedValue != "")
                    {
                        formulacion.Prioridad = new Prioridad { Id = int.Parse(ddlPrioridades.SelectedValue) };
                    }

                    // Asignar unidad de medida
                    formulacion.UnidadMedida = new UnidadMedida { Id = int.Parse(ddlUnidadMedida.SelectedValue) };

                    if (!string.IsNullOrEmpty(txtValorMedida.Text))
                        formulacion.ValorMedida = decimal.Parse(txtValorMedida.Text.Trim());
                    else
                        formulacion.ValorMedida = 0;

                    // Ejecutar operación según modo
                    if (ViewState["EditingFormulacionId"] != null)
                    {
                        negocio.modificar(formulacion);
                        lblMensaje.Text = "Formulación modificada exitosamente!";
                    }
                    else
                    {
                        negocio.agregar(formulacion);
                        lblMensaje.Text = "Formulación agregada exitosamente!";
                    }

                    lblMensaje.CssClass = "alert alert-success";

                    // Limpiar estados
                    ViewState["EditingFormulacionId"] = null;
                    ViewState["EditingObraId"] = null;
                    ClearFormFields();

                    // Restablecer UI
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "ResetModalAndUI", @"
                $('#modalAgregar .modal-title').text('Agregar Formulación');
                document.getElementById('" + btnAgregar.ClientID + @"').value = 'Agregar';
                $('.col-12:first').show();
                $('#modalAgregar').modal('hide');
            ", true);

                    btnAgregar.Text = "Agregar";

                    // Actualizar la lista de formulaciones
                    CargarListaFormulaciones();
                }
                catch (Exception ex)
                {
                    lblMensaje.Text = $"Error: {ex.Message}";
                    lblMensaje.CssClass = "alert alert-danger";
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "KeepModalOpen",
                        "$('#modalAgregar').modal('show');", true);
                }
            }
        }

        private void ClearFormFields()
        {
            ddlObra.SelectedIndex = 0;
            txtMonto26.Text = string.Empty;
            txtMonto27.Text = string.Empty;
            txtMonto28.Text = string.Empty;
            txtPPI.Text = string.Empty;
            ddlUnidadMedida.SelectedIndex = 0;
            txtValorMedida.Text = string.Empty;
            txtTechos.Text = string.Empty;
            txtMesBase.Text = string.Empty;
            txtObservaciones.Text = string.Empty;
            ddlPrioridades.SelectedIndex = 0; // Seleccionar el primer elemento si no hay prioridad
        }

        private void BindDropDownList()
        {
            ddlObra.Items.Clear();
            ddlUnidadMedida.Items.Clear();
            ddlPrioridades.Items.Clear(); // Añadir esta línea para limpiar las prioridades


            // Agregar elementos vacíos a cada dropdown
            ddlObra.Items.Add(new ListItem("Seleccione una obra", ""));
            ddlUnidadMedida.Items.Add(new ListItem("Seleccione una unidad de medida", ""));
            ddlPrioridades.Items.Add(new ListItem("Seleccione una prioridad", "")); // Añadir esta línea


            // Establecer la propiedad AppendDataBoundItems a true para todos los dropdowns
            ddlObra.AppendDataBoundItems = true;
            ddlUnidadMedida.AppendDataBoundItems = true;
            ddlPrioridades.AppendDataBoundItems = true; // Añadir esta línea

            // Cargar obras
            ddlObra.DataSource = ObtenerObras();
            ddlObra.DataTextField = "NOMBRE";
            ddlObra.DataValueField = "ID";
            ddlObra.DataBind();

            // Cargar unidades de medida
            ddlUnidadMedida.DataSource = ObtenerUnidadesMedida();
            ddlUnidadMedida.DataTextField = "NOMBRE";
            ddlUnidadMedida.DataValueField = "ID";
            ddlUnidadMedida.DataBind();

            // Cargar el control de filtro de obras
            cblObra.DataSource = ObtenerObras();
            cblObra.DataTextField = "NOMBRE";
            cblObra.DataValueField = "ID";
            cblObra.DataBind();

            ddlPrioridades.DataSource = listarPrioridades();
            ddlPrioridades.DataTextField = "NOMBRE";
            ddlPrioridades.DataValueField = "ID";
            ddlPrioridades.DataBind();
        }
        private DataTable listarPrioridades()
        {
            PrioridadNegocio prioridadNegocio = new PrioridadNegocio();
            return prioridadNegocio.listarddl();
        }
        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            string filtro = txtBuscar.Text.Trim();
            CargarListaFormulaciones(filtro);
        }

        protected void BtnClearFilters_Click(object sender, EventArgs e)
        {
            txtBuscar.Text = string.Empty;
            cblObra.ClearSelection();
            CargarListaFormulaciones();
        }
    }
}