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
            ddlObra.DataSource = ObtenerObras();  // Método para obtener los datos de Autorizantes
            ddlObra.DataTextField = "Nombre";
            ddlObra.DataValueField = "Id";
            ddlObra.DataBind();

            ddlLineaGestion.DataSource = ObtenerLineaGestion();  // Método para obtener los datos de Autorizantes
            ddlLineaGestion.DataTextField = "Nombre";
            ddlLineaGestion.DataValueField = "Id";
            ddlLineaGestion.DataBind();

            {
                // Llamamos a la función para obtener los proyectos
                List<BdProyecto> proyectos = bdProyectoNegocio.Listar();

                // Asignamos la lista de proyectos al GridView
                dgvBdProyecto.DataSource = proyectos;
                dgvBdProyecto.DataBind();
            }
        }
        protected void dgvBdProyecto_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Lógica para manejar la selección de filas (si es necesario)
            string codigoAutorizante = dgvBdProyecto.SelectedDataKey["CodigoAutorizante"].ToString();
            lblMensaje.Text = "Proyecto seleccionado: " + codigoAutorizante;
        }

        // Método para el evento de eliminación de fila
        protected void dgvBdProyecto_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            // Lógica para manejar la eliminación de filas (si es necesario)
            int idProyecto = Convert.ToInt32(dgvBdProyecto.DataKeys[e.RowIndex].Value);
            // Lógica para eliminar proyecto...
        }

        // Método para el cambio de página del GridView
        protected void dgvBdProyecto_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            dgvBdProyecto.PageIndex = e.NewPageIndex;
            List<BdProyecto> proyectos = bdProyectoNegocio.Listar();
            dgvBdProyecto.DataSource = proyectos;
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
                nuevoProyecto.LineaGestion.Id = Convert.ToInt32(ddlLineaGestion.SelectedValue); // Valor seleccionado del DropDownList
                nuevoProyecto.AutorizadoInicial = Convert.ToDecimal(txtMontoAutorizadoInicial.Text); // Autorizado inicial del TextBox

                // Llamar al método `agregar` de la clase BdProyectoNegocio para insertar el proyecto en la base de datos
                BdProyectoNegocio proyectoNegocio = new BdProyectoNegocio();
                proyectoNegocio.agregar(nuevoProyecto);

                // Mostrar mensaje de éxito
                lblMensaje.Text = "Proyecto agregado con éxito.";
                lblMensaje.CssClass = "alert alert-success"; // Establecer el estilo de mensaje de éxito

                // Actualizar la lista de proyectos y recargar el GridView
                List<BdProyecto> proyectos = bdProyectoNegocio.Listar(); // Obtener los proyectos actualizados
                dgvBdProyecto.DataSource = proyectos; // Asignar la lista actualizada de proyectos al GridView
                dgvBdProyecto.DataBind(); // Recargar el GridView con la nueva lista de proyectos
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