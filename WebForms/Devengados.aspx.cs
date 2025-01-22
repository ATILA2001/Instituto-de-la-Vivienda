using Dominio;
using Negocio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebForms
{
    public partial class Devengados : System.Web.UI.Page
    {
        DevengadoNegocio negocio = new DevengadoNegocio();

        protected void Page_Load(object sender, EventArgs e)
        {
            CargarListaDevengados();

        }

        private void CargarListaDevengados()
        {
            try
            {
               
                Session["listaDevengado"] = negocio.ListarDevengados();
                dgvDevengados.DataSource = Session["listaDevengado"];
                dgvDevengados.DataBind();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cargar los Certificados: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

    }
}