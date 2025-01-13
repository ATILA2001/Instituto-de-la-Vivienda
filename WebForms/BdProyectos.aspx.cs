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
    public partial class BdProyectos : System.Web.UI.Page
    {
        BdProyectoNegocio bdProyectoNegocio = new BdProyectoNegocio();
        protected void Page_Load(object sender, EventArgs e)
        {
            // Asegúrate de que el GridView se llena solo al cargar la página por primera vez
            if (!IsPostBack)
            {

                ddlObra.DataSource = ObtenerObras();  // Método para obtener los datos de Autorizantes
                ddlObra.DataTextField = "Nombre";
                ddlObra.DataValueField = "Id";
                ddlObra.DataBind();

                ddlLineaGestion.DataSource = ObtenerLineaGestion();  // Método para obtener los datos de Autorizantes
                ddlLineaGestion.DataTextField = "Nombre";
                ddlLineaGestion.DataValueField = "Id";
                ddlLineaGestion.DataBind();


                // Llamamos a la función para obtener los proyectos
                Session["listaProyectos"] = bdProyectoNegocio.Listar();

                // Asignamos la lista de proyectos al GridView
                dgvBdProyecto.DataSource = Session["listaProyectos"];
                dgvBdProyecto.DataBind();
            }
        }
        protected void dgvBdProyecto_SelectedIndexChanged(object sender, EventArgs e)
        {
            var idSeleccionado = dgvBdProyecto.SelectedDataKey.Value.ToString();
            Response.Redirect("ModificarProyecto.aspx?codM=" + idSeleccionado);
        }

        // Método para el evento de eliminación de fila
        protected void dgvBdProyecto_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            try
            {
                var id = Convert.ToInt32(dgvBdProyecto.DataKeys[e.RowIndex].Value);
                if (bdProyectoNegocio.eliminar(id))
                {
                    lblMensaje.Text = "Obra eliminada correctamente.";
                    lblMensaje.CssClass = "alert alert-success";
                    Session["listaProyectos"] = bdProyectoNegocio.Listar();

                    // Asignamos la lista de proyectos al GridView
                    dgvBdProyecto.DataSource = Session["listaProyectos"];
                    dgvBdProyecto.DataBind();
                    ddlObra.DataSource = ObtenerObras();  // Método para obtener los datos de Autorizantes
                    ddlObra.DataTextField = "Nombre";
                    ddlObra.DataValueField = "Id";
                    ddlObra.DataBind();
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al eliminar la obra: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }

        }

        // Método para el cambio de página del GridView
        protected void dgvBdProyecto_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            dgvBdProyecto.PageIndex = e.NewPageIndex;
            Session["listaProyectos"] = bdProyectoNegocio.Listar();

            // Asignamos la lista de proyectos al GridView
            dgvBdProyecto.DataSource = Session["listaProyectos"];
            dgvBdProyecto.DataBind();
        }
        protected void btnAgregar_Click(object sender, EventArgs e)
        {
            try
            {
                // Crear un nuevo objeto BdProyecto
                BdProyecto nuevoProyecto = new BdProyecto();

                // Asignar los valores ingresados en el formulario
                nuevoProyecto.Obra = new Obra(); // Creamos una nueva instancia de Obra
                nuevoProyecto.Obra.Id = Convert.ToInt32(ddlObra.SelectedValue);
                nuevoProyecto.SubProyecto = txtSubProyecto.Text; // Subproyecto del TextBox
                nuevoProyecto.Proyecto = txtProyecto.Text; // Proyecto del TextBox
                nuevoProyecto.LineaGestion = new LineaGestion(); // Creamos una nueva instancia de LineaGestion
                nuevoProyecto.LineaGestion.Id = int.Parse(ddlLineaGestion.SelectedValue);
                nuevoProyecto.LineaGestion.Nombre = ddlLineaGestion.SelectedItem.Text;
                nuevoProyecto.AutorizadoInicial = Convert.ToDecimal(txtMontoAutorizadoInicial.Text); // Autorizado inicial del TextBox

                // Llamar al método `agregar` de la clase BdProyectoNegocio para insertar el proyecto en la base de datos
                BdProyectoNegocio proyectoNegocio = new BdProyectoNegocio();
                proyectoNegocio.agregar(nuevoProyecto);

                // Mostrar mensaje de éxito
                lblMensaje.Text = "Proyecto agregado con éxito.";
                lblMensaje.CssClass = "alert alert-success"; // Establecer el estilo de mensaje de éxito
                Session["listaProyectos"] = bdProyectoNegocio.Listar();

                // Asignamos la lista de proyectos al GridView
                dgvBdProyecto.DataSource = Session["listaProyectos"];
                dgvBdProyecto.DataBind();
                ddlLineaGestion.SelectedIndex = -1;
                ddlObra.DataSource = ObtenerObras();  // Método para obtener los datos de Autorizantes
                ddlObra.DataTextField = "Nombre";
                ddlObra.DataValueField = "Id";
                ddlObra.DataBind();
                ddlObra.SelectedIndex = -1;
                txtProyecto.Text = string.Empty;
                txtSubProyecto.Text = string.Empty;
                txtMontoAutorizadoInicial.Text = string.Empty;
            }
            catch (Exception ex)
            {
                // Manejo de errores: Mostrar el mensaje de error en caso de problemas
                lblMensaje.Text = "Error al agregar el proyecto: " + ex.Message;
                lblMensaje.CssClass = "alert alert-danger"; // Estilo de mensaje de error
            }
        }
        private DataTable ObtenerObras()
        {
            ObraNegocio barrioNegocio = new ObraNegocio();
            return barrioNegocio.listarddl();
        }
        private DataTable ObtenerLineaGestion()
        {
            LineaGestionNegocio barrioNegocio = new LineaGestionNegocio();
            return barrioNegocio.listarddl();
        }
    }
}