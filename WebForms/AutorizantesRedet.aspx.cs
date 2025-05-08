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
    public partial class AutorizantesRedet : System.Web.UI.Page
    {

        private AutorizanteNegocio negocio = new AutorizanteNegocio();

        protected void Page_Init(object sender, EventArgs e)
        {
            cblArea.AcceptChanges += OnAcceptChanges;
            cblObra.AcceptChanges += OnAcceptChanges;
            cblEmpresa.AcceptChanges += OnAcceptChanges;
            cblConcepto.AcceptChanges += OnAcceptChanges;
            cblEstado.AcceptChanges += OnAcceptChanges;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindDropDownList();
                CargarListaAutorizantes();
                CalcularSubtotal();
            }
        }

        private void OnAcceptChanges(object sender, EventArgs e)
        {
            CargarListaAutorizantes();
        }

        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            string filtro = txtBuscar.Text.Trim(); // Obtener el texto del buscador

            CargarListaAutorizantes(filtro);
        }


        protected void BtnClearFilters_Click(object sender, EventArgs e)
        {
            txtBuscar.Text = string.Empty;
            cblArea.ClearSelection();
            cblEmpresa.ClearSelection();
            cblConcepto.ClearSelection();
            cblEstado.ClearSelection();
            cblObra.ClearSelection();
            CargarListaAutorizantes();
        }
        private void CargarListaAutorizantes(string filtro = null)
        {
            try
            {
                var selectedAreas = cblArea.SelectedValues;

                var selectedEmpresas = cblEmpresa.SelectedValues;
                var selectedConceptos = cblConcepto.SelectedValues;
                var selectedEstados = cblEstado.SelectedValues;
                var selectedObras = cblObra.SelectedValues;


                Session["listaAutorizanteRedet"] = negocio.listar(selectedAreas, selectedEstados, selectedEmpresas, selectedConceptos, selectedObras, filtro);
                dgvAutorizante.DataSource = Session["listaAutorizanteRedet"];
                dgvAutorizante.DataBind();
                CalcularSubtotal();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cargar los Autorizantes: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }
        private void CalcularSubtotal()
        {
            decimal subtotal = 0;

            foreach (GridViewRow row in dgvAutorizante.Rows)
            {
                var cellValue = row.Cells[9].Text;
                if (decimal.TryParse(cellValue, System.Globalization.NumberStyles.Currency, null, out decimal monto))
                {
                    subtotal += monto;
                }
            }

            txtSubtotal.Text = subtotal.ToString("C");
        }



        private DataTable ObtenerAreas()
        {
            AreaNegocio areaNegocio = new AreaNegocio();
            return areaNegocio.listarddl();
        }
        private void BindDropDownList()
        {
            cblArea.DataSource = ObtenerAreas();
            cblArea.DataTextField = "Nombre";
            cblArea.DataValueField = "Id";
            cblArea.DataBind();

            cblEstado.DataSource = ObtenerEstado();
            cblEstado.DataTextField = "Nombre";
            cblEstado.DataValueField = "Id";
            cblEstado.DataBind();

            cblConcepto.DataSource = ObtenerConcepto();
            cblConcepto.DataTextField = "Nombre";
            cblConcepto.DataValueField = "Id";
            cblConcepto.DataBind();

            cblEmpresa.DataSource = ObtenerEmpresas();
            cblEmpresa.DataTextField = "Nombre";
            cblEmpresa.DataValueField = "Id";
            cblEmpresa.DataBind();

            cblObra.DataSource = ObtenerObras();
            cblObra.DataTextField = "Nombre";
            cblObra.DataValueField = "Id";
            cblObra.DataBind();
        }
        private DataTable ObtenerEmpresas()
        {
            EmpresaNegocio empresaNegocio = new EmpresaNegocio();
            return empresaNegocio.listarddl();
        }
     

        
        private DataTable ObtenerEstado()
        {
            EstadoAutorizanteNegocio empresaNegocio = new EstadoAutorizanteNegocio();
            return empresaNegocio.listarddl();
        }

        private DataTable ObtenerObras()
        {
            ObraNegocio barrioNegocio = new ObraNegocio();
            return barrioNegocio.listarddl();
        }
        private DataTable ObtenerConcepto()
        {
            ConceptoNegocio empresaNegocio = new ConceptoNegocio();
            return empresaNegocio.listarddl();
        }
        
        
    }

}