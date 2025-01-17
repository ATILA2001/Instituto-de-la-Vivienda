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
            if (!IsPostBack)
            {

                ddlObra.DataSource = ObtenerObras();
                ddlObra.DataTextField = "Nombre";
                ddlObra.DataValueField = "Id";
                ddlObra.DataBind();

                ddlLineaGestion.DataSource = ObtenerLineaGestion();  
                ddlLineaGestion.DataTextField = "Nombre";
                ddlLineaGestion.DataValueField = "Id";
                ddlLineaGestion.DataBind();

                Session["listaProyectos"] = bdProyectoNegocio.Listar();

                dgvBdProyecto.DataSource = Session["listaProyectos"];
                dgvBdProyecto.DataBind();
                CalcularSubtotal();
                CalcularSubtotal1();
            }
        }

        private void CalcularSubtotal()
        {
            decimal subtotal = 0;

            foreach (GridViewRow row in dgvBdProyecto.Rows)
            {
                var cellValue = row.Cells[5].Text;
                if (decimal.TryParse(cellValue, System.Globalization.NumberStyles.Currency, null, out decimal monto))
                {
                    subtotal += monto;
                }
            }

            txtSubtotal.Text = subtotal.ToString("C");
        }
        private void CalcularSubtotal1()
        {
            decimal subtotal = 0;

            foreach (GridViewRow row in dgvBdProyecto.Rows)
            {
                var cellValue = row.Cells[6].Text;
                if (decimal.TryParse(cellValue, System.Globalization.NumberStyles.Currency, null, out decimal monto))
                {
                    subtotal += monto;
                }
            }

            txtSubtotal1.Text = subtotal.ToString("C");
        }


        protected void dgvBdProyecto_SelectedIndexChanged(object sender, EventArgs e)
        {
            var idSeleccionado = dgvBdProyecto.SelectedDataKey.Value.ToString();
            Response.Redirect("ModificarProyecto.aspx?codM=" + idSeleccionado);
        }
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

                    dgvBdProyecto.DataSource = Session["listaProyectos"];
                    dgvBdProyecto.DataBind();
                    ddlObra.DataSource = ObtenerObras(); 
                    ddlObra.DataTextField = "Nombre";
                    ddlObra.DataValueField = "Id";
                    ddlObra.DataBind();
                    CalcularSubtotal();
                    CalcularSubtotal1();
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al eliminar la obra: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }

        }
        protected void dgvBdProyecto_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            dgvBdProyecto.PageIndex = e.NewPageIndex;
            Session["listaProyectos"] = bdProyectoNegocio.Listar();

            dgvBdProyecto.DataSource = Session["listaProyectos"];
            dgvBdProyecto.DataBind();
            CalcularSubtotal();
            CalcularSubtotal1();
        }
        protected void btnAgregar_Click(object sender, EventArgs e)
        {
            try
            {
                BdProyecto nuevoProyecto = new BdProyecto();

                nuevoProyecto.Obra = new Obra();
                nuevoProyecto.Obra.Id = Convert.ToInt32(ddlObra.SelectedValue);
                nuevoProyecto.SubProyecto = txtSubProyecto.Text;
                nuevoProyecto.Proyecto = txtProyecto.Text; 
                nuevoProyecto.LineaGestion = new LineaGestion(); 
                nuevoProyecto.LineaGestion.Id = int.Parse(ddlLineaGestion.SelectedValue);
                nuevoProyecto.LineaGestion.Nombre = ddlLineaGestion.SelectedItem.Text;
                nuevoProyecto.AutorizadoInicial = Convert.ToDecimal(txtMontoAutorizadoInicial.Text); 
                BdProyectoNegocio proyectoNegocio = new BdProyectoNegocio();
                proyectoNegocio.agregar(nuevoProyecto);

                lblMensaje.Text = "Proyecto agregado con éxito.";
                lblMensaje.CssClass = "alert alert-success";
                Session["listaProyectos"] = bdProyectoNegocio.Listar();

                dgvBdProyecto.DataSource = Session["listaProyectos"];
                dgvBdProyecto.DataBind();
                ddlLineaGestion.SelectedIndex = -1;
                ddlObra.DataSource = ObtenerObras();  
                ddlObra.DataTextField = "Nombre";
                ddlObra.DataValueField = "Id";
                ddlObra.DataBind();
                ddlObra.SelectedIndex = -1;
                txtProyecto.Text = string.Empty;
                txtSubProyecto.Text = string.Empty;
                txtMontoAutorizadoInicial.Text = string.Empty;
                CalcularSubtotal();
                CalcularSubtotal1();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = "Error al agregar el proyecto: " + ex.Message;
                lblMensaje.CssClass = "alert alert-danger";
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