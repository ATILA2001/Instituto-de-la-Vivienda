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
    public partial class ObrasRedet : System.Web.UI.Page
    {
        private ObraNegocio negocio = new ObraNegocio();

        protected void Page_Init(object sender, EventArgs e)
        {
            cblArea.AcceptChanges += OnAcceptChanges;
            cblEmpresa.AcceptChanges += OnAcceptChanges;
            cblBarrio.AcceptChanges += OnAcceptChanges;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindDropDownList();

                CargarListaObras();
            }
        }

        private void OnAcceptChanges(object sender, EventArgs e)
        {
            CargarListaObras();
        }

        protected void BtnClearFilters_Click(object sender, EventArgs e)
        {
            txtBuscar.Text = string.Empty;
            cblArea.ClearSelection();
            cblBarrio.ClearSelection();
            cblEmpresa.ClearSelection();
            CargarListaObras();
        }

        private DataTable ObtenerEmpresas()
        {
            EmpresaNegocio empresaNegocio = new EmpresaNegocio();
            return empresaNegocio.listarddl();
        }
        private DataTable ObtenerAreas()
        {
            AreaNegocio areaNegocio = new AreaNegocio();
            return areaNegocio.listarddl();
        }

        private DataTable ObtenerContratas()
        {
            ContrataNegocio contrataNegocio = new ContrataNegocio();
            return contrataNegocio.listarddl();
        }

        private DataTable ObtenerBarrios()
        {
            BarrioNegocio barrioNegocio = new BarrioNegocio();
            return barrioNegocio.listarddl();
        }

        private void CargarListaObras(string filtro = null)
        {
            try
            {
                var selectedEmpresas = cblEmpresa.SelectedValues;

                var selectedBarrios = cblBarrio.SelectedValues;
                var selectedAreas = cblArea.SelectedValues;

                Session["listaObra"] = negocio.listar(selectedBarrios, selectedEmpresas, selectedAreas, filtro);
                dgvObra.DataSource = Session["listaObra"];
                dgvObra.DataBind();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cargar las obras: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

      
        private void BindDropDownList()
        {

     

            cblBarrio.DataSource = ObtenerBarrios();
            cblBarrio.DataTextField = "Nombre";
            cblBarrio.DataValueField = "Id";
            cblBarrio.DataBind();


            cblEmpresa.DataSource = ObtenerEmpresas();
            cblEmpresa.DataTextField = "Nombre";
            cblEmpresa.DataValueField = "Id";
            cblEmpresa.DataBind();

            cblArea.DataSource = ObtenerAreas();
            cblArea.DataTextField = "Nombre";
            cblArea.DataValueField = "Id";
            cblArea.DataBind();


        }

      
        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            string filtro = txtBuscar.Text.Trim();
            CargarListaObras(filtro);
        }
    }
}