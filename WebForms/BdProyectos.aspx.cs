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
                BindDropDownList();
                CargarListaProyectos();
                CalcularSubtotal();
                CalcularSubtotal1();
            }
        }
        private void CargarListaProyectos(string filtro = null)
        {
            try
            {
                string area = ddlAreaFiltro.SelectedValue == "0" ? null : ddlAreaFiltro.SelectedItem.Text;

                string linea = ddlLinea.SelectedValue == "0" ? null : ddlLinea.SelectedItem.Text;
                string proyecto = ddlProyecto.SelectedValue == "0" ? null : ddlProyecto.SelectedItem.Text;
                Session["listaProyectos"] = bdProyectoNegocio.Listar(linea,proyecto, area, filtro);
                dgvBdProyecto.DataSource = Session["listaProyectos"];
                dgvBdProyecto.DataBind();
                CalcularSubtotal();
                CalcularSubtotal1();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cargar los Autorizantes: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }
        protected void btnBuscar_Click(object sender, EventArgs e)
        {
            string filtro = txtBuscar.Text.Trim(); // Obtener el texto del buscador
            CargarListaProyectos(filtro);
        }
        protected void ddlAreaFiltro_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarListaProyectos();
            CalcularSubtotal();
            CalcularSubtotal1();
        }
        private DataTable ObtenerAreas()
        {
            AreaNegocio areaNegocio = new AreaNegocio();
            return areaNegocio.listarddl();
        }
        protected void ddlLinea_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarListaProyectos();
            CalcularSubtotal();
            CalcularSubtotal1();
        }
        protected void ddlProyecto_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarListaProyectos();
            CalcularSubtotal();
            CalcularSubtotal1();
        }
        private void CalcularSubtotal()
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

            txtSubtotal.Text = subtotal.ToString("C");
        }
        private void CalcularSubtotal1()
        {
            decimal subtotal = 0;

            foreach (GridViewRow row in dgvBdProyecto.Rows)
            {
                var cellValue = row.Cells[7].Text;
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
                    CargarListaProyectos();
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
            CargarListaProyectos();
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
                CargarListaProyectos();
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
        private void BindDropDownList()
        {
            var proyectoFiltro = ObtenerProyecto();
            proyectoFiltro.Rows.InsertAt(CrearFilaTodos(proyectoFiltro), 0);
            ddlProyecto.DataSource = proyectoFiltro;
            ddlProyecto.DataTextField = "Nombre";
            ddlProyecto.DataValueField = "Id";
            ddlProyecto.DataBind();

            ddlObra.DataSource = ObtenerObras();
            ddlObra.DataTextField = "Nombre";
            ddlObra.DataValueField = "Id";
            ddlObra.DataBind();

            var area = ObtenerAreas();
            area.Rows.InsertAt(CrearFilaTodos(area), 0);
            ddlAreaFiltro.DataSource = area;
            ddlAreaFiltro.DataTextField = "Nombre";
            ddlAreaFiltro.DataValueField = "Id";
            ddlAreaFiltro.DataBind();

            ddlLineaGestion.DataSource = ObtenerLineaGestion();
            ddlLineaGestion.DataTextField = "Nombre";
            ddlLineaGestion.DataValueField = "Id";
            ddlLineaGestion.DataBind();

            var lineasFiltro = ObtenerLineaGestion();
            lineasFiltro.Rows.InsertAt(CrearFilaTodos(lineasFiltro), 0);
            ddlLinea.DataSource = lineasFiltro;
            ddlLinea.DataTextField = "Nombre";
            ddlLinea.DataValueField = "Id";
            ddlLinea.DataBind();
        }
        private DataRow CrearFilaTodos(DataTable table)
        {
            DataRow row = table.NewRow();
            row["Id"] = 0;
            row["Nombre"] = "Todos";
            return row;
        }
        private DataTable ObtenerObras()
        {
            ObraNegocio barrioNegocio = new ObraNegocio();
            return barrioNegocio.listarddlProyecto();
        }
        private DataTable ObtenerProyecto()
        {
            BdProyectoNegocio barrioNegocio = new BdProyectoNegocio();
            return barrioNegocio.listarddl();
        }
        private DataTable ObtenerLineaGestion()
        {
            LineaGestionNegocio barrioNegocio = new LineaGestionNegocio();
            return barrioNegocio.listarddl();
        }
    }
}