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
            // Check if page is valid before proceeding
            if (!Page.IsValid)
            {
                lblMensaje.Text = "Por favor complete todos los campos correctamente.";
                lblMensaje.CssClass = "alert alert-danger";
                return;
            }

            UsuarioNegocio negocio = new UsuarioNegocio();
            Usuario nuevo = new Usuario();
            nuevo.Correo = txtEmail.Text.Trim();
            nuevo.Contrasenia = txtPass.Text;
            nuevo.Nombre = txtNombre.Text.Trim(); 
            nuevo.Area = new Area();
            nuevo.Area.Id = int.Parse(ddlAreas.SelectedValue);
            nuevo.Area.Nombre = ddlAreas.SelectedItem.Text;

            try
            {
                nuevo.Nombre = negocio.registrarUsuario(nuevo);
                negocio.Logear(nuevo);
                Session["usuario"] = nuevo;

                if (nuevo.Tipo & nuevo.Estado) 
                { 
                    Response.Redirect("BdProyectos.aspx", false);
                }
                else
                {
                    if (((Dominio.Usuario)Session["Usuario"]).Estado == true)
                    {
                        Response.Redirect("Obras.aspx", false);
                    }
                    else
                    {
                        lblMensaje.Text = "Usuario registrado. Pendiente de habilitación.";
                        lblMensaje.CssClass = "alert alert-info";
                    }
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = "Error al registrar el usuario. Por favor intente nuevamente." + ex;
                lblMensaje.CssClass = "alert alert-danger";
            }
        }
        private void CargarDropDownList()
        {
            AreaNegocio negocio = new AreaNegocio();

            try
            {
                List<Area> listaAreas = negocio.listar();

                ddlAreas.DataSource = listaAreas;
                ddlAreas.DataTextField = "Nombre";
                ddlAreas.DataValueField = "Id";
                ddlAreas.DataBind();

                ddlAreas.Items.Insert(0, new ListItem("Seleccione un área", "0"));
            }
            catch (Exception ex)
            {
                lblMensaje.Text = "Error al cargar las áreas: " + ex.Message;
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        protected void btnVolver_Click(object sender, EventArgs e)
        {
            Response.Redirect("Login.aspx");
        }
    }
}