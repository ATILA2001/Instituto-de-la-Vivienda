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
    public partial class Legitimos : System.Web.UI.Page
    {
        private LegitimoNegocio negocio = new LegitimoNegocio();

        protected void Page_Init(object sender, EventArgs e)
        {
            cblEmpresa.SelectedIndexChanged += OnCheckBoxListSearch_SelectedIndexChanged;
            cblAutorizante.SelectedIndexChanged += OnCheckBoxListSearch_SelectedIndexChanged;
            cblFecha.SelectedIndexChanged += OnCheckBoxListSearch_SelectedIndexChanged;
        }

        private void OnCheckBoxListSearch_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarListaLegitimos();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindDropDownList();
                CargarListaLegitimos();

            }
        }
        protected void dgvLegitimos_SelectedIndexChanged(object sender, EventArgs e)
        {
            var idSeleccionado = dgvLegitimos.SelectedDataKey.Value.ToString();
            Response.Redirect("ModificarLegitimo.aspx?codM=" + idSeleccionado);
        }
        //protected void btnLimpiar_Click(object sender, EventArgs e)
        //{
        //    txtAutorizante.Text = string.Empty;
        //    txtExpediente.Text = string.Empty;
        //    txtInicioEjecucion.Text = string.Empty;
        //    txtFinEjecucion.Text = string.Empty;
        //    txtCertificado.Text = string.Empty;
        //    txtMesAprobacion.Text = string.Empty;
        //    ddlObra.SelectedIndex = -1;
        //}

        private void CargarListaLegitimos(string filtro = null)
        {
            try
            {
                Usuario usuarioLogueado = (Usuario)Session["usuario"];


                var selectedEmpresas = cblEmpresa.Items.Cast<ListItem>().Where(i => i.Selected).Select(i => i.Text).ToList();
                var selectedAutorizantes = cblAutorizante.Items.Cast<ListItem>().Where(i => i.Selected).Select(i => i.Text).ToList();
                var selectedFechas = cblFecha.Items.Cast<ListItem>().Where(i => i.Selected).Select(i => i.Value).ToList();

                Session["listaLegitimos"] = negocio.listarFiltro(usuarioLogueado, selectedFechas, selectedEmpresas, selectedAutorizantes,filtro);

                dgvLegitimos.DataSource = Session["listaLegitimos"];
                dgvLegitimos.DataBind();
                CalcularSubtotal();

            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cargar los legítimos: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }



      

        protected void dgvLegitimos_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            try
            {
                var id = dgvLegitimos.DataKeys[e.RowIndex].Value.ToString();
                if (negocio.eliminar(id))
                {
                    lblMensaje.Text = "Legítimo eliminado correctamente.";
                    lblMensaje.CssClass = "alert alert-success";
                    CargarListaLegitimos();
                    CalcularSubtotal();

                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al eliminar el legítimo: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        protected void dgvLegitimos_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            try
            {
                dgvLegitimos.PageIndex = e.NewPageIndex;
                CargarListaLegitimos();
                CalcularSubtotal();

            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cambiar de página: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        //protected void btnAgregar_Click(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        Legitimo nuevoLegitimo = new Legitimo
        //        {
        //            CodigoAutorizante = txtAutorizante.Text,
        //            Expediente = txtExpediente.Text,
        //            InicioEjecucion = DateTime.Parse(txtInicioEjecucion.Text),
        //            FinEjecucion = DateTime.Parse(txtFinEjecucion.Text),
        //            Certificado = decimal.Parse(txtCertificado.Text),
        //            MesAprobacion = DateTime.Parse(txtMesAprobacion.Text)
        //        };
        //        nuevoLegitimo.Obra = new Obra
        //        {
        //            Id = int.Parse(ddlObra.SelectedValue)
        //        };

        //        if (negocio.agregar(nuevoLegitimo))
        //        {
        //            lblMensaje.Text = "Legítimo agregado con éxito.";
        //            lblMensaje.CssClass = "alert alert-success";
        //            CargarListaLegitimos();
        //            CalcularSubtotal();

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        lblMensaje.Text = $"Error al agregar el legítimo: {ex.Message}";
        //        lblMensaje.CssClass = "alert alert-danger";
        //    }
        //}

        private DataTable ObtenerObras()
        {
            ObraNegocio barrioNegocio = new ObraNegocio();
            Usuario usuarioLogueado = (Usuario)Session["usuario"];
            return barrioNegocio.listarddl(usuarioLogueado);
        }
        private DataTable ObtenerLegitimos()
        {
            LegitimoNegocio barrioNegocio = new LegitimoNegocio();
            Usuario usuarioLogueado = (Usuario)Session["usuario"];
            return barrioNegocio.listarddl(usuarioLogueado);
        }
        protected void txtExpediente_TextChanged(object sender, EventArgs e)
        {
            TextBox txtExpediente = (TextBox)sender;
            GridViewRow row = (GridViewRow)txtExpediente.NamingContainer;

            int id = (int)dgvLegitimos.DataKeys[row.RowIndex].Value;

            string nuevoExpediente = txtExpediente.Text;

            try
            {
                LegitimoNegocio negocio = new LegitimoNegocio();
                negocio.ActualizarExpediente(id, nuevoExpediente);

                lblMensaje.Text = "Expediente actualizado correctamente.";
                CargarListaLegitimos();
                CalcularSubtotal();


            }
            catch (Exception ex)
            {
                lblMensaje.Text = "Error al actualizar el expediente: " + ex.Message;
            }
        }
        private void CalcularSubtotal()
        {
            decimal subtotal = 0;

            foreach (GridViewRow row in dgvLegitimos.Rows)
            {
                var cellValue = row.Cells[6].Text;
                if (decimal.TryParse(cellValue, System.Globalization.NumberStyles.Currency, null, out decimal monto))
                {
                    subtotal += monto;
                }
            }

            txtSubtotal.Text = subtotal.ToString("C");
        }
        private void BindDropDownList()
        {

            //ddlObra.DataSource = ObtenerObras();
            //ddlObra.DataTextField = "Nombre";
            //ddlObra.DataValueField = "Id";
            //ddlObra.DataBind();

            cblEmpresa.DataSource = ObtenerEmpresas();
            cblEmpresa.DataTextField = "Nombre";
            cblEmpresa.DataValueField = "Id";
            cblEmpresa.DataBind();

            cblAutorizante.DataSource = ObtenerLegitimos();
            cblAutorizante.DataTextField = "Nombre";
            cblAutorizante.DataValueField = "Id";
            cblAutorizante.DataBind();

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


        }
        private DataTable ObtenerEmpresas()
        {
            EmpresaNegocio empresaNegocio = new EmpresaNegocio();
            return empresaNegocio.listarddl();
        }
        private DataRow CrearFilaTodos(DataTable table)
        {
            DataRow row = table.NewRow();
            row["Id"] = 0;
            row["Nombre"] = "Todos";
            return row;
        }
        protected void btnFiltrar_Click(object sender, EventArgs e)
        {

            string filtro = txtBuscar.Text.Trim();
            CargarListaLegitimos(filtro);
        }

        protected void BtnClearFilters_Click(object sender, EventArgs e)
        {
            txtBuscar.Text = string.Empty;
            cblAutorizante.DeselectAllAndReload();
            cblEmpresa.DeselectAllAndReload();
            cblFecha.DeselectAllAndReload();
        }

    }

}