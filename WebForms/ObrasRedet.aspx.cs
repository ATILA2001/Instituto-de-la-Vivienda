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

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Cargar la lista completa de obras una vez y guardarla en Session.
                // Se asume que llamar a negocio.listar con todos los filtros null/vacíos devuelve todas las obras.
                List<Obra> obrasCompletas = negocio.listar(new List<string>(), new List<string>(), new List<string>(), null);
                Session["obrasCompletasRedet"] = obrasCompletas;

                CargarListaObras();
            }
        }

        protected void OnAcceptChanges(object sender, EventArgs e)
        {
            CargarListaObras();
        }

        protected void BtnClearFilters_Click(object sender, EventArgs e)
        {
            txtBuscar.Text = string.Empty;
            ClearHeaderFilter("cblsHeaderArea");
            ClearHeaderFilter("cblsHeaderEmpresa");
            ClearHeaderFilter("cblsHeaderBarrio");
            CargarListaObras();
            ScriptManager.RegisterStartupScript(this, this.GetType(), "SetFiltersClearedFlag", "sessionStorage.setItem('filtersCleared', 'true');", true);
        }

        private void ClearHeaderFilter(string controlId)
        {
            if (dgvObra.HeaderRow != null)
            {
                var control = dgvObra.HeaderRow.FindControl(controlId) as WebForms.CustomControls.CheckBoxListSearch;
                if (control != null)
                {
                    control.ClearSelection();
                    // Limpiar estado de sesión/contexto si el control lo usa internamente
                    string controlInstanceId = control.ID;
                    string sessionKey = $"CheckBoxListSearch_SelectedValues_{controlInstanceId}";
                    if (HttpContext.Current.Session[sessionKey] != null)
                    {
                        HttpContext.Current.Session.Remove(sessionKey);
                    }
                    string contextKey = $"CheckBoxListSearch_{controlInstanceId}_ContextSelectedValues";
                    if (HttpContext.Current.Items.Contains(contextKey))
                    {
                        HttpContext.Current.Items.Remove(contextKey);
                    }
                }
            }
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
                List<Obra> obrasCompletas = Session["obrasCompletasRedet"] as List<Obra>;
                if (obrasCompletas == null) // Fallback si la sesión se pierde
                {
                    obrasCompletas = negocio.listar(new List<string>(), new List<string>(), new List<string>(), null);
                    Session["obrasCompletasRedet"] = obrasCompletas;
                }

                IEnumerable<Obra> listaFiltrada = obrasCompletas;

                // Aplicar filtro de texto general
                string filtroGeneral = string.IsNullOrEmpty(filtro) ? txtBuscar.Text.Trim().ToUpper() : filtro.Trim().ToUpper();
                if (!string.IsNullOrEmpty(filtroGeneral))
                {
                    listaFiltrada = listaFiltrada.Where(o =>
                        (o.Area?.Nombre?.ToUpper().Contains(filtroGeneral) ?? false) ||
                        (o.Empresa?.Nombre?.ToUpper().Contains(filtroGeneral) ?? false) ||
                        (o.Contrata?.Nombre?.ToUpper().Contains(filtroGeneral) ?? false) ||
                        (o.Numero?.ToString().Contains(filtroGeneral) ?? false) ||
                        (o.Año?.ToString().Contains(filtroGeneral) ?? false) ||
                        (o.Barrio?.Nombre?.ToUpper().Contains(filtroGeneral) ?? false) ||
                        (o.Descripcion?.ToUpper().Contains(filtroGeneral) ?? false) ||
                        (o.LineaGestion?.Nombre?.ToUpper().Contains(filtroGeneral) ?? false) ||
                        (o.Proyecto?.Proyecto?.ToUpper().Contains(filtroGeneral) ?? false)
                    );
                }

                // Aplicar filtros de cabecera
                if (dgvObra.HeaderRow != null)
                {
                    var cblsHeaderArea = dgvObra.HeaderRow.FindControl("cblsHeaderArea") as WebForms.CustomControls.CheckBoxListSearch;
                    var filtroHeaderArea = cblsHeaderArea?.SelectedValues;
                    if (filtroHeaderArea != null && filtroHeaderArea.Any())
                    {
                        listaFiltrada = listaFiltrada.Where(o => o.Area != null && filtroHeaderArea.Contains(o.Area.Id.ToString()));
                    }

                    var cblsHeaderEmpresa = dgvObra.HeaderRow.FindControl("cblsHeaderEmpresa") as WebForms.CustomControls.CheckBoxListSearch;
                    var filtroHeaderEmpresa = cblsHeaderEmpresa?.SelectedValues;
                    if (filtroHeaderEmpresa != null && filtroHeaderEmpresa.Any())
                    {
                        listaFiltrada = listaFiltrada.Where(o => o.Empresa != null && filtroHeaderEmpresa.Contains(o.Empresa.Id.ToString()));
                    }

                    var cblsHeaderBarrio = dgvObra.HeaderRow.FindControl("cblsHeaderBarrio") as WebForms.CustomControls.CheckBoxListSearch;
                    var filtroHeaderBarrio = cblsHeaderBarrio?.SelectedValues;
                    if (filtroHeaderBarrio != null && filtroHeaderBarrio.Any())
                    {
                        listaFiltrada = listaFiltrada.Where(o => o.Barrio != null && filtroHeaderBarrio.Contains(o.Barrio.Id.ToString()));
                    }
                }

                List<Obra> resultadoFinal = listaFiltrada.ToList();
                Session["listaObraRedet"] = resultadoFinal; // Guardar lista filtrada para otras operaciones si es necesario
                dgvObra.DataSource = resultadoFinal;
                dgvObra.DataBind();

                if (!resultadoFinal.Any())
                {
                    lblMensaje.Text = "No se encontraron obras con los filtros aplicados.";
                    lblMensaje.CssClass = "alert alert-info";
                }
                else
                {
                    lblMensaje.Text = "";
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cargar las obras: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        protected void dgvObra_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.Header)
            {
                List<Obra> obrasCompletas = Session["obrasCompletasRedet"] as List<Obra>;
                if (obrasCompletas == null) return; // No hay datos para poblar filtros

                // Poblar filtro de Área
                var cblsHeaderArea = e.Row.FindControl("cblsHeaderArea") as WebForms.CustomControls.CheckBoxListSearch;
                if (cblsHeaderArea != null)
                {
                    var areasUnicas = obrasCompletas
                        .Where(o => o.Area != null && !string.IsNullOrEmpty(o.Area.Nombre))
                        .Select(o => new { Id = o.Area.Id.ToString(), Nombre = o.Area.Nombre })
                        .Distinct()
                        .OrderBy(a => a.Nombre)
                        .ToList();
                    cblsHeaderArea.DataSource = areasUnicas;
                    cblsHeaderArea.DataBind();
                }

                // Poblar filtro de Empresa
                var cblsHeaderEmpresa = e.Row.FindControl("cblsHeaderEmpresa") as WebForms.CustomControls.CheckBoxListSearch;
                if (cblsHeaderEmpresa != null)
                {
                    var empresasUnicas = obrasCompletas
                        .Where(o => o.Empresa != null && !string.IsNullOrEmpty(o.Empresa.Nombre))
                        .Select(o => new { Id = o.Empresa.Id.ToString(), Nombre = o.Empresa.Nombre })
                        .Distinct()
                        .OrderBy(em => em.Nombre)
                        .ToList();
                    cblsHeaderEmpresa.DataSource = empresasUnicas;
                    cblsHeaderEmpresa.DataBind();
                }

                // Poblar filtro de Barrio
                var cblsHeaderBarrio = e.Row.FindControl("cblsHeaderBarrio") as WebForms.CustomControls.CheckBoxListSearch;
                if (cblsHeaderBarrio != null)
                {
                    var barriosUnicos = obrasCompletas
                        .Where(o => o.Barrio != null && !string.IsNullOrEmpty(o.Barrio.Nombre))
                        .Select(o => new { Id = o.Barrio.Id.ToString(), Nombre = o.Barrio.Nombre })
                        .Distinct()
                        .OrderBy(b => b.Nombre)
                        .ToList();
                    cblsHeaderBarrio.DataSource = barriosUnicos;
                    cblsHeaderBarrio.DataBind();
                }
            }
        }


        private void BindDropDownList()
        {

     

            //cblBarrio.DataSource = ObtenerBarrios();
            //cblBarrio.DataTextField = "Nombre";
            //cblBarrio.DataValueField = "Id";
            //cblBarrio.DataBind();


            //cblEmpresa.DataSource = ObtenerEmpresas();
            //cblEmpresa.DataTextField = "Nombre";
            //cblEmpresa.DataValueField = "Id";
            //cblEmpresa.DataBind();

            //cblArea.DataSource = ObtenerAreas();
            //cblArea.DataTextField = "Nombre";
            //cblArea.DataValueField = "Id";
            //cblArea.DataBind();


        }

      
        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            string filtro = txtBuscar.Text.Trim();
            CargarListaObras(filtro);
        }
    }
}