using Dominio;
using Negocio;
using Negocio.Negocio;
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
        private CalculoRedeterminacionNegocio calculoRedeterminacionNegocio = new CalculoRedeterminacionNegocio();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Cargar la lista completa de autorizantes usando CalculoRedeterminacionNegocio
                List<Autorizante> listaCompleta = calculoRedeterminacionNegocio.listarAutRedet();
                Session["autorizantesCompletosRedet"] = listaCompleta;

                CargarListaAutorizantes();
                // CalcularSubtotal se llama dentro de CargarListaAutorizantes
            }
        }

        protected void OnAcceptChanges(object sender, EventArgs e)
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

            WebForms.CustomControls.TreeViewSearch.ClearAllFiltersOnPage(this.Page);

            CargarListaAutorizantes();
        }


        private void CargarListaAutorizantes(string filtro = null)
        {
            try
            {
                List<Autorizante> listaCompleta;
                if (Session["autorizantesCompletosRedet"] == null)
                {
                    listaCompleta = calculoRedeterminacionNegocio.listarAutRedet();
                    Session["autorizantesCompletosRedet"] = listaCompleta;
                }
                else
                {
                    listaCompleta = (List<Autorizante>)Session["autorizantesCompletosRedet"];
                }

                if (listaCompleta == null)
                {
                    lblMensaje.Text = "No se pudieron cargar los datos de los autorizantes.";
                    lblMensaje.CssClass = "alert alert-warning";
                    dgvAutorizante.DataSource = null;
                    dgvAutorizante.DataBind();
                    txtSubtotal.Text = 0.ToString("C");
                    return;
                }

                IEnumerable<Autorizante> listaFiltrada = listaCompleta;

                // Obtener IDs seleccionados desde los controles de cabecera del GridView
                List<string> selectedAreas = new List<string>();
                List<string> selectedObras = new List<string>();
                List<string> selectedEmpresas = new List<string>(); // Para Obra.Empresa
                List<string> selectedConceptos = new List<string>();
                List<string> selectedEstados = new List<string>();

                if (dgvAutorizante.HeaderRow != null)
                {
                    var cblsHeaderAreaControl = dgvAutorizante.HeaderRow.FindControl("cblsHeaderArea") as WebForms.CustomControls.TreeViewSearch;
                    if (cblsHeaderAreaControl != null) selectedAreas = cblsHeaderAreaControl.SelectedValues;

                    var cblsHeaderObraControl = dgvAutorizante.HeaderRow.FindControl("cblsHeaderObra") as WebForms.CustomControls.TreeViewSearch;
                    if (cblsHeaderObraControl != null) selectedObras = cblsHeaderObraControl.SelectedValues;

                    var cblsHeaderEmpresaControl = dgvAutorizante.HeaderRow.FindControl("cblsHeaderEmpresa") as WebForms.CustomControls.TreeViewSearch;
                    if (cblsHeaderEmpresaControl != null) selectedEmpresas = cblsHeaderEmpresaControl.SelectedValues;

                    var cblsHeaderConceptoControl = dgvAutorizante.HeaderRow.FindControl("cblsHeaderConcepto") as WebForms.CustomControls.TreeViewSearch;
                    if (cblsHeaderConceptoControl != null) selectedConceptos = cblsHeaderConceptoControl.SelectedValues;

                    var cblsHeaderEstadoControl = dgvAutorizante.HeaderRow.FindControl("cblsHeaderEstado") as WebForms.CustomControls.TreeViewSearch;
                    if (cblsHeaderEstadoControl != null) selectedEstados = cblsHeaderEstadoControl.SelectedValues;
                }

                // Aplicar filtro de texto general
                string filtroTextoGeneral = string.IsNullOrEmpty(filtro) ? txtBuscar.Text.Trim().ToUpper() : filtro.Trim().ToUpper();
                if (!string.IsNullOrEmpty(filtroTextoGeneral))
                {
                    listaFiltrada = listaFiltrada.Where(a =>
                        (a.Obra?.Area?.Nombre?.ToUpper().Contains(filtroTextoGeneral) ?? false) ||
                        (a.Obra?.Descripcion?.ToUpper().Contains(filtroTextoGeneral) ?? false) ||
                        (a.Obra?.Contrata?.Nombre?.ToUpper().Contains(filtroTextoGeneral) ?? false) ||
                        (a.Empresa?.ToUpper().Contains(filtroTextoGeneral) ?? false) || // Campo Empresa directo de Autorizante (string)
                        (a.Obra?.Empresa?.Nombre?.ToUpper().Contains(filtroTextoGeneral) ?? false) || // Empresa de la Obra
                        (a.CodigoAutorizante?.ToUpper().Contains(filtroTextoGeneral) ?? false) ||
                        (a.Concepto?.Nombre?.ToUpper().Contains(filtroTextoGeneral) ?? false) ||
                        (a.Detalle?.ToUpper().Contains(filtroTextoGeneral) ?? false) ||
                        (a.Expediente?.ToUpper().Contains(filtroTextoGeneral) ?? false) ||
                        (a.Estado?.Nombre?.ToUpper().Contains(filtroTextoGeneral) ?? false)
                    );
                }

                // Aplicar filtros específicos LINQ
                if (selectedAreas.Any())
                    listaFiltrada = listaFiltrada.Where(a => a.Obra?.Area != null && selectedAreas.Contains(a.Obra.Area.Id.ToString()));
                if (selectedObras.Any())
                    listaFiltrada = listaFiltrada.Where(a => a.Obra != null && selectedObras.Contains(a.Obra.Id.ToString()));
                if (selectedEmpresas.Any()) // Filtrar por Obra.Empresa.Id
                    listaFiltrada = listaFiltrada.Where(a => a.Obra?.Empresa != null && selectedEmpresas.Contains(a.Obra.Empresa.Id.ToString()));
                if (selectedConceptos.Any())
                    listaFiltrada = listaFiltrada.Where(a => a.Concepto != null && selectedConceptos.Contains(a.Concepto.Id.ToString()));
                if (selectedEstados.Any())
                    listaFiltrada = listaFiltrada.Where(a => a.Estado != null && selectedEstados.Contains(a.Estado.Id.ToString()));

                List<Autorizante> resultadoFinal = listaFiltrada.ToList();
                // Session["listaAutorizanteRedetFiltrada"] = resultadoFinal; // Opcional, si se necesita para otras operaciones
                dgvAutorizante.DataSource = resultadoFinal;
                dgvAutorizante.DataBind();
                CalcularSubtotal(); // Calcular después de filtrar y enlazar
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cargar/filtrar los Autorizantes: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
                dgvAutorizante.DataSource = new List<Autorizante>(); // Evitar null reference en DataBind
                dgvAutorizante.DataBind();
                txtSubtotal.Text = 0.ToString("C");
            }

        }
        private void CalcularSubtotal()
        {
            decimal subtotal = 0;

            if (dgvAutorizante.DataSource is List<Autorizante> dataSource)
            {
                subtotal = dataSource.Sum(a => a.MontoAutorizado);
            }
            else if (dgvAutorizante.Rows.Count > 0 && dgvAutorizante.DataSource != null)
            {
                var boundData = dgvAutorizante.DataSource as IEnumerable<Autorizante>;
                if (boundData != null)
                {
                    subtotal = boundData.Sum(a => a.MontoAutorizado);
                }
            }
            txtSubtotal.Text = subtotal.ToString("C");
        }

        protected void dgvAutorizante_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.Header)
            {
                List<Autorizante> autorizantesCompletos;
                if (Session["autorizantesCompletosRedet"] == null)
                {
                    autorizantesCompletos = calculoRedeterminacionNegocio.listarAutRedet();
                    Session["autorizantesCompletosRedet"] = autorizantesCompletos;
                }
                else
                {
                    autorizantesCompletos = (List<Autorizante>)Session["autorizantesCompletosRedet"];
                }

                if (autorizantesCompletos == null || !autorizantesCompletos.Any()) return;

                // Poblar filtro de Área
                var cblsHeaderArea = e.Row.FindControl("cblsHeaderArea") as WebForms.CustomControls.TreeViewSearch;
                if (cblsHeaderArea != null)
                {
                    var items = autorizantesCompletos
                        .Where(a => a.Obra?.Area != null && !string.IsNullOrEmpty(a.Obra.Area.Nombre))
                        .Select(a => new { Id = a.Obra.Area.Id, Nombre = a.Obra.Area.Nombre })
                        .Distinct()
                        .OrderBy(a => a.Nombre)
                        .ToList();
                    cblsHeaderArea.DataSource = items;
                    cblsHeaderArea.DataBind();
                }

                // Poblar filtro de Obra
                var cblsHeaderObra = e.Row.FindControl("cblsHeaderObra") as WebForms.CustomControls.TreeViewSearch;
                if (cblsHeaderObra != null)
                {
                    var items = autorizantesCompletos
                        .Where(a => a.Obra != null && !string.IsNullOrEmpty(a.Obra.Descripcion))
                        .Select(a => new { Id = a.Obra.Id, Nombre = a.Obra.Descripcion })
                        .Distinct()
                        .OrderBy(o => o.Nombre)
                        .ToList();
                    cblsHeaderObra.DataSource = items;
                    cblsHeaderObra.DataBind();
                }

                // Poblar filtro de Empresa (basado en Obra.Empresa)
                var cblsHeaderEmpresa = e.Row.FindControl("cblsHeaderEmpresa") as WebForms.CustomControls.TreeViewSearch;
                if (cblsHeaderEmpresa != null)
                {
                    var items = autorizantesCompletos
                        .Where(a => a.Obra?.Empresa != null && !string.IsNullOrEmpty(a.Obra.Empresa.Nombre))
                        .Select(a => new { Id = a.Obra.Empresa.Id, Nombre = a.Obra.Empresa.Nombre })
                        .Distinct()
                        .OrderBy(em => em.Nombre)
                        .ToList();
                    cblsHeaderEmpresa.DataSource = items;
                    cblsHeaderEmpresa.DataBind();
                }

                // Poblar filtro de Concepto
                var cblsHeaderConcepto = e.Row.FindControl("cblsHeaderConcepto") as WebForms.CustomControls.TreeViewSearch;
                if (cblsHeaderConcepto != null)
                {
                    var items = autorizantesCompletos
                        .Where(a => a.Concepto != null && !string.IsNullOrEmpty(a.Concepto.Nombre))
                        .Select(a => new { Id = a.Concepto.Id, Nombre = a.Concepto.Nombre })
                        .Distinct()
                        .OrderBy(c => c.Nombre)
                        .ToList();
                    cblsHeaderConcepto.DataSource = items;
                    cblsHeaderConcepto.DataBind();
                }

                // Poblar filtro de Estado
                var cblsHeaderEstado = e.Row.FindControl("cblsHeaderEstado") as WebForms.CustomControls.TreeViewSearch;
                if (cblsHeaderEstado != null)
                {
                    var items = autorizantesCompletos
                        .Where(a => a.Estado != null && !string.IsNullOrEmpty(a.Estado.Nombre))
                        .Select(a => new { Id = a.Estado.Id, Nombre = a.Estado.Nombre })
                        .Distinct()
                        .OrderBy(es => es.Nombre)
                        .ToList();
                    cblsHeaderEstado.DataSource = items;
                    cblsHeaderEstado.DataBind();
                }
            }
        }

        protected void dgvAutorizante_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            try
            {
                dgvAutorizante.PageIndex = e.NewPageIndex;
                CargarListaAutorizantes();
                CalcularSubtotal();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cambiar de página: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

    }

}