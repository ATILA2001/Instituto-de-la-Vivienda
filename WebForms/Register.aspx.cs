using Dominio;
using Negocio;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebForms
{
    public partial class Register : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarDropDownList();
            }
        }

        protected void btnRegistrar_Click(object sender, EventArgs e)
        {
            UsuarioNegocio negocio = new UsuarioNegocio();
            Usuario nuevo = new Usuario();
            nuevo.Correo = txtEmail.Text;
            nuevo.Contrasenia = txtPass.Text;
            nuevo.Nombre = txtNombre.Text; 
            nuevo.Area = new Area(); // Inicializa la propiedad Area
            nuevo.Area.Id = int.Parse(ddlAreas.SelectedValue);
            nuevo.Area.Nombre = ddlAreas.SelectedItem.Text;

            try
            {
                int areaSeleccionadaId = int.Parse(ddlAreas.SelectedValue);

                if (areaSeleccionadaId == 0)
                {
                    lblError.Text = "Por favor selecciona un área.";


                    lblMensaje.CssClass = "alert alert-danger";
                }
                else if (txtNombre.Text.Trim() == string.Empty)
                {
                    lblMensaje.Text = "Tiene que escribir un nombre";
                    lblMensaje.CssClass = "alert alert-danger";
                }
             
                else if (txtEmail.Text.Trim() == string.Empty)
                {
                    lblMensaje.Text = "Tiene que escribir un Email";
                    lblMensaje.CssClass = "alert alert-danger";
                }
                else if (txtEmailRep.Text.Trim() == string.Empty)
                {
                    lblMensaje.Text = "Tiene que repetir el Email";
                    lblMensaje.CssClass = "alert alert-danger";
                }
                else if (txtPass.Text.Trim() == string.Empty)
                {
                    lblMensaje.Text = "Tiene que escribir una contraseña";
                    lblMensaje.CssClass = "alert alert-danger";
                }
                else if (txtPassRep.Text.Trim() == string.Empty)
                {
                    lblMensaje.Text = "Tiene que repetir la contraseña";
                    lblMensaje.CssClass = "alert alert-danger";
                }
                else if (txtEmail.Text == txtEmailRep.Text && txtPass.Text == txtPassRep.Text)
                {
                    nuevo.Nombre = negocio.registrarUsuario(nuevo);
                    negocio.Logear(nuevo);
                    Session["usuario"] = nuevo;
                    if (nuevo.Tipo) { 
                    Response.Redirect("HomeAdmin.aspx", false);
                    }else
                    {
                        Response.Redirect("HomeUser.aspx", false);
                    }
                }

            }
            catch (Exception)
            {

                throw;
            }

        }

        protected void txtEmailRep_TextChanged(object sender, EventArgs e)
        {
            if (txtEmailRep.Text.Trim() != txtEmail.Text.Trim())
            {
                lblErrorMail.Text = "No coinciden los Mails";
                lblErrorMail.CssClass = "text-danger";
            }
            else
            {
                lblErrorMail.Text = string.Empty; lblErrorMail.CssClass = string.Empty;
            }
        }

        protected void txtPassRep_TextChanged(object sender, EventArgs e)
        {
            if (txtPass.Text.Trim() != txtPassRep.Text.Trim())
            {
                lblErrorPass.Text = "No coinciden las contraseñas";
                lblErrorPass.CssClass = "text-danger";
            }
            else
            {
                lblErrorPass.Text = string.Empty; lblErrorPass.CssClass = string.Empty;
            }
        }

        private void CargarDropDownList()
        {
            AreaNegocio negocio = new AreaNegocio();

            try
            {
                // Obtener la lista de áreas desde la base de datos
                List<Area> listaAreas = negocio.listar();

                // Asignar los datos al DropDownList
                ddlAreas.DataSource = listaAreas;
                ddlAreas.DataTextField = "Nombre"; // Mostrar el nombre del área
                ddlAreas.DataValueField = "Id";   // El valor será el ID del área
                ddlAreas.DataBind();

                // Opción por defecto (opcional)
                ddlAreas.Items.Insert(0, new ListItem("-- Seleccionar Área --", "0"));
            }
            catch (Exception ex)
            {
                // Manejo de errores (opcionalmente mostrar un mensaje de error)
                lblError.Text = "Error al cargar las áreas: " + ex.Message;
            }
        }
        protected void btnVolver_Click(object sender, EventArgs e)
        {
            Response.Redirect("Login.aspx");
        }




    }
}