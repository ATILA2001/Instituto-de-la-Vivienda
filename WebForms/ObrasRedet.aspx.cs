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
                // Force complete reload on initial page load
                CargarListaObras(null, true);
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
            ClearHeaderFilter("cblsHeaderContrata");
            ClearHeaderFilter("cblsHeaderBarrio");
            ClearHeaderFilter("cblsHeaderNombreObra");
            CargarListaObras();
            ScriptManager.RegisterStartupScript(this, this.GetType(), "SetFiltersClearedFlag", "sessionStorage.setItem('filtersCleared', 'true');", true);
        }

        private void ClearHeaderFilter(string controlId)
        {
            if (dgvObra.HeaderRow != null)
            {
                var control = dgvObra.HeaderRow.FindControl(controlId) as WebForms.CustomControls.TreeViewSearch;
                if (control != null)
                {
                    control.ClearSelection();
                    // Limpiar estado de sesión/contexto si el control lo usa internamente
                    string controlInstanceId = control.ID;
                    string sessionKey = $"TreeViewSearch_SelectedValues_{controlInstanceId}";
                    if (HttpContext.Current.Session[sessionKey] != null)
                    {
                        HttpContext.Current.Session.Remove(sessionKey);
                    }
                    string contextKey = $"TreeViewSearch_{controlInstanceId}_ContextSelectedValues";
                    if (HttpContext.Current.Items.Contains(contextKey))
                    {
                        HttpContext.Current.Items.Remove(contextKey);
                    }
                }
            }
        }
        protected void dgvObra_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            try
            {
                // Cambiar el índice de la página
                dgvObra.PageIndex = e.NewPageIndex;

                // Refrescar el listado de empresas
                CargarListaObras();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cambiar de página: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }
        //private DataTable ObtenerEmpresas()
        //{
        //    EmpresaNegocio empresaNegocio = new EmpresaNegocio();
        //    return empresaNegocio.listarddl();
        //}
        //private DataTable ObtenerAreas()
        //{
        //    AreaNegocio areaNegocio = new AreaNegocio();
        //    return areaNegocio.listarddl();
        //}

        //private DataTable ObtenerContratas()
        //{
        //    ContrataNegocio contrataNegocio = new ContrataNegocio();
        //    return contrataNegocio.listarddl();
        //}

        //private DataTable ObtenerBarrios()
        //{
        //    BarrioNegocio barrioNegocio = new BarrioNegocio();
        //    return barrioNegocio.listarddl();
        //}

        private void CargarListaObras(string filtro = null, bool forzarRecargaCompleta = false)
        {
            try
            {
                List<Obra> obrasCompletas;

                if (forzarRecargaCompleta || Session["obrasCompletasRedet"] == null)
                {
                    // Only load from database when forced or data doesn't exist in session
                    obrasCompletas = negocio.listar(new List<string>(), new List<string>(), new List<string>(), null);
                    Session["obrasCompletasRedet"] = obrasCompletas;
                }
                else
                {
                    // Use cached data from session
                    obrasCompletas = (List<Obra>)Session["obrasCompletasRedet"];
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
                        (o.ContrataFormateada?.ToUpper().Contains(filtroGeneral) ?? false) || // Incluir búsqueda en texto completo
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
                    var cblsHeaderArea = dgvObra.HeaderRow.FindControl("cblsHeaderArea") as WebForms.CustomControls.TreeViewSearch;
                    var filtroHeaderArea = cblsHeaderArea?.SelectedValues;
                    if (filtroHeaderArea != null && filtroHeaderArea.Any())
                    {
                        listaFiltrada = listaFiltrada.Where(o => o.Area != null && filtroHeaderArea.Contains(o.Area.Id.ToString()));
                    }

                    var cblsHeaderEmpresa = dgvObra.HeaderRow.FindControl("cblsHeaderEmpresa") as WebForms.CustomControls.TreeViewSearch;
                    var filtroHeaderEmpresa = cblsHeaderEmpresa?.SelectedValues;
                    if (filtroHeaderEmpresa != null && filtroHeaderEmpresa.Any())
                    {
                        listaFiltrada = listaFiltrada.Where(o => o.Empresa != null && filtroHeaderEmpresa.Contains(o.Empresa.Id.ToString()));
                    }

                    // Filtro para Contrata (modificado para usar texto completo)
                    var cblsHeaderContrata = dgvObra.HeaderRow.FindControl("cblsHeaderContrata") as WebForms.CustomControls.TreeViewSearch;
                    var filtroHeaderContrata = cblsHeaderContrata?.SelectedValues;
                    if (filtroHeaderContrata != null && filtroHeaderContrata.Any())
                    {
                        listaFiltrada = listaFiltrada.Where(o => !string.IsNullOrEmpty(o.ContrataFormateada) &&
                            filtroHeaderContrata.Contains(o.ContrataFormateada));
                    }

                    var cblsHeaderBarrio = dgvObra.HeaderRow.FindControl("cblsHeaderBarrio") as WebForms.CustomControls.TreeViewSearch;
                    var filtroHeaderBarrio = cblsHeaderBarrio?.SelectedValues;
                    if (filtroHeaderBarrio != null && filtroHeaderBarrio.Any())
                    {
                        listaFiltrada = listaFiltrada.Where(o => o.Barrio != null && filtroHeaderBarrio.Contains(o.Barrio.Id.ToString()));
                    }

                    // Nuevo filtro para Nombre de Obra (Descripcion)
                    var cblsHeaderNombreObra = dgvObra.HeaderRow.FindControl("cblsHeaderNombreObra") as WebForms.CustomControls.TreeViewSearch;
                    var filtroHeaderNombreObra = cblsHeaderNombreObra?.SelectedValues;
                    if (filtroHeaderNombreObra != null && filtroHeaderNombreObra.Any())
                    {
                        listaFiltrada = listaFiltrada.Where(o => !string.IsNullOrEmpty(o.Descripcion) && filtroHeaderNombreObra.Contains(o.Descripcion));
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
                var cblsHeaderArea = e.Row.FindControl("cblsHeaderArea") as WebForms.CustomControls.TreeViewSearch;
                if (cblsHeaderArea != null)
                {
                    var areasUnicas = obrasCompletas
                        .Where(o => o.Area != null)
                        .Select(o => new { Id = o.Area.Id.ToString(), Nombre = o.Area.Nombre })
                        .Distinct()
                        .OrderBy(a => a.Nombre)
                        .ToList();
                    cblsHeaderArea.DataSource = areasUnicas;
                    cblsHeaderArea.DataBind();
                }

                // Poblar filtro de Empresa
                var cblsHeaderEmpresa = e.Row.FindControl("cblsHeaderEmpresa") as WebForms.CustomControls.TreeViewSearch;
                if (cblsHeaderEmpresa != null)
                {
                    var empresasUnicas = obrasCompletas
                        .Where(o => o.Empresa != null)
                        .Select(o => new { Id = o.Empresa.Id.ToString(), Nombre = o.Empresa.Nombre })
                        .Distinct()
                        .OrderBy(em => em.Nombre)
                        .ToList();
                    cblsHeaderEmpresa.DataSource = empresasUnicas;
                    cblsHeaderEmpresa.DataBind();
                }

                // Poblar filtro de Contrata (modificado para usar texto completo)
                var cblsHeaderContrata = e.Row.FindControl("cblsHeaderContrata") as WebForms.CustomControls.TreeViewSearch;
                if (cblsHeaderContrata != null)
                {
                    var contratasUnicas = obrasCompletas
                        .Where(o => !string.IsNullOrEmpty(o.ContrataFormateada))
                        .Select(o => new { ContrataFormateada = o.ContrataFormateada })
                        .Distinct()
                        .OrderBy(c => c.ContrataFormateada)
                        .ToList();
                    cblsHeaderContrata.DataTextField = "ContrataFormateada";
                    cblsHeaderContrata.DataValueField = "ContrataFormateada"; // Usar el texto completo como valor
                    cblsHeaderContrata.DataSource = contratasUnicas;
                    cblsHeaderContrata.DataBind();
                }

                // Poblar filtro de Barrio
                var cblsHeaderBarrio = e.Row.FindControl("cblsHeaderBarrio") as WebForms.CustomControls.TreeViewSearch;
                if (cblsHeaderBarrio != null)
                {
                    var barriosUnicos = obrasCompletas
                        .Where(o => o.Barrio != null)
                        .Select(o => new { Id = o.Barrio.Id.ToString(), Nombre = o.Barrio.Nombre })
                        .Distinct()
                        .OrderBy(b => b.Nombre)
                        .ToList();
                    cblsHeaderBarrio.DataSource = barriosUnicos;
                    cblsHeaderBarrio.DataBind();
                }

                // Poblar filtro de Nombre de Obra (nuevo)
                var cblsHeaderNombreObra = e.Row.FindControl("cblsHeaderNombreObra") as WebForms.CustomControls.TreeViewSearch;
                if (cblsHeaderNombreObra != null)
                {
                    var nombresObrasUnicos = obrasCompletas
                        .Where(o => !string.IsNullOrEmpty(o.Descripcion))
                        .Select(o => new { Descripcion = o.Descripcion })
                        .Distinct()
                        .OrderBy(n => n.Descripcion)
                        .ToList();
                    cblsHeaderNombreObra.DataSource = nombresObrasUnicos;
                    cblsHeaderNombreObra.DataBind();
                }
            }
        }


        //private void BindDropDownList()
        //{



        //    //cblBarrio.DataSource = ObtenerBarrios();
        //    //cblBarrio.DataTextField = "Nombre";
        //    //cblBarrio.DataValueField = "Id";
        //    //cblBarrio.DataBind();


        //    //cblEmpresa.DataSource = ObtenerEmpresas();
        //    //cblEmpresa.DataTextField = "Nombre";
        //    //cblEmpresa.DataValueField = "Id";
        //    //cblEmpresa.DataBind();

        //    //cblArea.DataSource = ObtenerAreas();
        //    //cblArea.DataTextField = "Nombre";
        //    //cblArea.DataValueField = "Id";
        //    //cblArea.DataBind();


        //}


        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            string filtro = txtBuscar.Text.Trim();

            WebForms.CustomControls.TreeViewSearch.ClearAllFiltersOnPage(this.Page);

            CargarListaObras(filtro);
        }
    }
}