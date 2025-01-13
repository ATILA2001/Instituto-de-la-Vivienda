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
    public partial class ModificarProyecto : System.Web.UI.Page
    {
        int codM;
        BdProyectoNegocio negocio = new BdProyectoNegocio();

        protected void Page_Load(object sender, EventArgs e)
        {
            // Asegúrate de que el GridView se llena solo al cargar la página por primera vez
            if (!IsPostBack)
            {
                if (Request.QueryString["codM"] != null)
                {
                    ddlObra.DataSource = ObtenerObras();  // Método para obtener los datos de Autorizantes
                    ddlObra.DataTextField = "Nombre";
                    ddlObra.DataValueField = "Id";
                    ddlObra.DataBind();

                    ddlLineaGestion.DataSource = ObtenerLineaGestion();  // Método para obtener los datos de Autorizantes
                    ddlLineaGestion.DataTextField = "Nombre";
                    ddlLineaGestion.DataValueField = "Id";
                    ddlLineaGestion.DataBind();

                    ValidationSettings.UnobtrusiveValidationMode = UnobtrusiveValidationMode.None;

                    // Obtener y mostrar los datos del certificado seleccionado
                    codM = Convert.ToInt32(Request.QueryString["codM"]);
                    List<BdProyecto> temp = (List<BdProyecto>)Session["listaProyectos"];
                    BdProyecto selected = temp.Find(x => x.Id == codM);

                    txtSubProyecto.Text = selected.SubProyecto;
                    txtProyecto.Text = selected.Proyecto;
                    txtMontoAutorizadoInicial.Text = selected.AutorizadoInicial.ToString();

                    ddlObra.SelectedValue = selected.Obra.Id.ToString();
                    ddlLineaGestion.SelectedValue = selected.LineaGestion.Id.ToString();
                }
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
        protected void btnModificar_Click(object sender, EventArgs e)
        {
            BdProyectoNegocio negocio = new BdProyectoNegocio();
            try
            {
                if (
                    txtProyecto.Text.Trim() != string.Empty &&
                    txtSubProyecto.Text.Trim() != string.Empty &&
                    ddlLineaGestion.SelectedIndex != -1 &&
                    txtMontoAutorizadoInicial.Text.Trim() != string.Empty)
                {
                    // Crear el objeto modificado
                    BdProyecto Modificado = new BdProyecto
                    {
                        Id = int.Parse(Request.QueryString["codM"]),
                        Proyecto = txtProyecto.Text.Trim(),
                        AutorizadoInicial = decimal.Parse(txtMontoAutorizadoInicial.Text.Trim()),
                        SubProyecto = txtSubProyecto.Text.Trim(),
                        Obra = new Obra { Id = int.Parse(ddlObra.SelectedValue) },
                        LineaGestion = new LineaGestion
                        {

                            Id = int.Parse(ddlLineaGestion.SelectedValue)
                        }
                    };

                    // Llamar al método modificar
                    if (negocio.modificar(Modificado))
                    {
                        lblMensaje.Text = "Certificado modificado exitosamente!";
                        lblMensaje.CssClass = "alert alert-success";

                        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), "redirectJS",
                        "setTimeout(function() { window.location.replace('BdProyectos.aspx') }, 3000);", true);
                    }
                    else
                    {
                        lblMensaje.Text = "Hubo un problema al modificar el certificado.";
                        lblMensaje.CssClass = "alert alert-danger";
                    }
                }
                else
                {
                    lblMensaje.Text = "Debe llenar todos los campos correctamente.";
                    lblMensaje.CssClass = "alert alert-danger";
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al modificar el certificado: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }


    }
}