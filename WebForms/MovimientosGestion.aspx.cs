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
    public partial class MovimientosGestion : System.Web.UI.Page
    {
        MovimientoNegocio negocio = new MovimientoNegocio();
        protected void Page_Init(object sender, EventArgs e)
        {
            cblObra.SelectedIndexChanged += OnCheckBoxListSearch_SelectedIndexChanged;
        }
        private void OnCheckBoxListSearch_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarListaMovimientos();
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {

                BindDropDownList();

                CargarListaMovimientos();
            }

        }

        protected void btnLimpiar_Click(object sender, EventArgs e)
        {
            txtMovimiento.Text = string.Empty;
            txtFecha.Text = string.Empty;
            ddlObra.SelectedIndex = -1;
        }

        private void CargarListaMovimientos(string filtro = null)
        {
            try
            {

                var selectedObras = cblObra.Items.Cast<ListItem>().Where(i => i.Selected).Select(i => i.Text).ToList();
               
                Session["listaMovimiento"] = negocio.listar( selectedObras, filtro);
                dgvMovimiento.DataSource = Session["listaMovimiento"];
                dgvMovimiento.DataBind();
              
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cargar los Movimientos: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        protected void dgvMovimiento_SelectedIndexChanged(object sender, EventArgs e)
        {
            var idSeleccionado = dgvMovimiento.SelectedDataKey.Value.ToString();
            //Response.Redirect("ModificarCertificado.aspx?codM=" + idSeleccionado);
        }
        protected void dgvMovimiento_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {

            try
            {
                var id = Convert.ToInt32(dgvMovimiento.DataKeys[e.RowIndex].Value);
                if (negocio.eliminar(id))
                {
                    lblMensaje.Text = "Movimiento eliminado correctamente.";
                    lblMensaje.CssClass = "alert alert-success";
                    CargarListaMovimientos();
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al eliminar el movimiento: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        protected void dgvMovimiento_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            try
            {
                dgvMovimiento.PageIndex = e.NewPageIndex;

                CargarListaMovimientos();
                
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cambiar de página: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        protected void btnAgregar_Click(object sender, EventArgs e)
        {
            try
            {
                MovimientoNegocio movimientoNegocio = new MovimientoNegocio();
                Movimiento movimiento = new Movimiento
                {
                    Obra = new Obra
                    {
                        Id = int.Parse(ddlObra.SelectedValue)
                    },
                    
                    Monto = decimal.Parse(txtMovimiento.Text),
                    Fecha = string.IsNullOrWhiteSpace(txtFecha.Text)
                        ? null
                        : (DateTime?)DateTime.Parse(txtFecha.Text)
                };

                movimientoNegocio.agregar(movimiento);

                lblMensaje.Text = "Movimiento agregado con éxito.";
                lblMensaje.ForeColor = System.Drawing.Color.Green;

                CargarListaMovimientos();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al agregar el movimientos: {ex.Message}";
                lblMensaje.ForeColor = System.Drawing.Color.Red;
            }
        }




        private void BindDropDownList()
        {


            var meses = Enumerable.Range(0, 36) // 36 meses entre 2024 y 2026
            .Select(i => new DateTime(2024, 1, 1).AddMonths(i))
            .Select(fecha => new
            {
                Texto = fecha.ToString("MMMM yyyy", new System.Globalization.CultureInfo("es-ES")), // Texto: "Enero 2024"
                Valor = fecha.ToString("yyyy-MM-dd")
            });

            cblFecha.DataSource = meses;
            cblFecha.DataTextField = "Texto";
            cblFecha.DataValueField = "Valor";
            cblFecha.DataBind();

            cblObra.DataSource = ObtenerObras();
            cblObra.DataTextField = "Nombre";
            cblObra.DataValueField = "Id";
            cblObra.DataBind();
            ddlObra.DataSource = ObtenerObras();
            ddlObra.DataTextField = "Nombre";
            ddlObra.DataValueField = "Id";
            ddlObra.DataBind();
        }
       
        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            string filtro = txtBuscar.Text.Trim();
            CargarListaMovimientos(filtro);
        }
        private DataTable ObtenerObras()
        {
            ObraNegocio barrioNegocio = new ObraNegocio();
            return barrioNegocio.listarddl();
        }
        protected void BtnClearFilters_Click(object sender, EventArgs e)
        {
            txtBuscar.Text = string.Empty;
            cblObra.ClearSelection();
            cblFecha.ClearSelection();
            CargarListaMovimientos();
        }

    }
}