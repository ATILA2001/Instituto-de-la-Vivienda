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
        private CalculoRedeterminacionNegocio calculoRedeterminacionNegocio = new CalculoRedeterminacionNegocio();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CargarListaAutorizantes(null, true);

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


        private void CargarListaAutorizantes(string filtro = null, bool forzarRecargaCompleta = false)
        {
            try
            {
                List<Autorizante> listaCompleta;

                if (forzarRecargaCompleta || Session["autorizantesCompletosRedet"] == null)
                {
                    // Only load from database when forced or data doesn't exist in session
                    listaCompleta = calculoRedeterminacionNegocio.listarAutRedet();
                    Session["autorizantesCompletosRedet"] = listaCompleta;
                }
                else
                {
                    // Use cached data from session
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
                List<string> selectedContratas = new List<string>(); // Filtro para Contratas (texto completo)
                List<string> selectedCodigosAutorizante = new List<string>(); // Nuevo filtro para Códigos Autorizante



                if (dgvAutorizante.HeaderRow != null)
                {
                    var cblsHeaderAreaControl = dgvAutorizante.HeaderRow.FindControl("cblsHeaderArea") as WebForms.CustomControls.TreeViewSearch;
                    if (cblsHeaderAreaControl != null) selectedAreas = cblsHeaderAreaControl.SelectedValues;

                    var cblsHeaderObraControl = dgvAutorizante.HeaderRow.FindControl("cblsHeaderObra") as WebForms.CustomControls.TreeViewSearch;
                    if (cblsHeaderObraControl != null) selectedObras = cblsHeaderObraControl.SelectedValues;

                    var cblsHeaderContrataControl = dgvAutorizante.HeaderRow.FindControl("cblsHeaderContrata") as WebForms.CustomControls.TreeViewSearch;
                    if (cblsHeaderContrataControl != null) selectedContratas = cblsHeaderContrataControl.SelectedValues;


                    var cblsHeaderEmpresaControl = dgvAutorizante.HeaderRow.FindControl("cblsHeaderEmpresa") as WebForms.CustomControls.TreeViewSearch;
                    if (cblsHeaderEmpresaControl != null) selectedEmpresas = cblsHeaderEmpresaControl.SelectedValues;

                    var cblsHeaderCodigoAutorizanteControl = dgvAutorizante.HeaderRow.FindControl("cblsHeaderCodigoAutorizante") as WebForms.CustomControls.TreeViewSearch;
                    if (cblsHeaderCodigoAutorizanteControl != null) selectedCodigosAutorizante = cblsHeaderCodigoAutorizanteControl.SelectedValues;

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
                if (selectedContratas.Any()) // Filtro para Contratas por texto completo (Contrata + Numero + Año)
                    listaFiltrada = listaFiltrada.Where(a => a.Obra?.Contrata != null &&
                        !string.IsNullOrEmpty(a.Obra.Contrata.Nombre) &&
                        selectedContratas.Contains(a.Obra.Contrata.Nombre));
                if (selectedEmpresas.Any()) // Filtrar por Obra.Empresa.Id
                    listaFiltrada = listaFiltrada.Where(a => a.Obra?.Empresa != null && selectedEmpresas.Contains(a.Obra.Empresa.Id.ToString()));
                if (selectedCodigosAutorizante.Any()) // Nuevo filtro para Códigos Autorizante
                    listaFiltrada = listaFiltrada.Where(a => !string.IsNullOrEmpty(a.CodigoAutorizante) && selectedCodigosAutorizante.Contains(a.CodigoAutorizante));
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
                List<Autorizante> autorizantesCompletos = (List<Autorizante>)Session["autorizantesCompletosRedet"];
                if (autorizantesCompletos == null || !autorizantesCompletos.Any())
                {
                    // If session data is missing (shouldn't happen if our pattern is followed), 
                    // load it but this is a fallback
                    autorizantesCompletos = calculoRedeterminacionNegocio.listarAutRedet();
                    Session["autorizantesCompletosRedet"] = autorizantesCompletos;
                    if (autorizantesCompletos == null || !autorizantesCompletos.Any()) return;
                }

                // Poblar filtro de Área
                var cblsHeaderArea = e.Row.FindControl("cblsHeaderArea") as WebForms.CustomControls.TreeViewSearch;
                if (cblsHeaderArea != null)
                {
                    var items = autorizantesCompletos
                        .Where(a => a.Obra?.Area != null)
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
                        .Where(a => a.Obra != null)
                        .Select(a => new { Id = a.Obra.Id, Nombre = a.Obra.Descripcion })
                        .Distinct()
                        .OrderBy(o => o.Nombre)
                        .ToList();
                    cblsHeaderObra.DataSource = items;
                    cblsHeaderObra.DataBind();
                }

                // Poblar filtro de Contrata (modificado para usar texto completo)
                var cblsHeaderContrata = e.Row.FindControl("cblsHeaderContrata") as WebForms.CustomControls.TreeViewSearch;
                if (cblsHeaderContrata != null)
                {
                    var items = autorizantesCompletos
                        .Where(a => a.Obra?.Contrata != null && !string.IsNullOrEmpty(a.Obra.Contrata.Nombre))
                        .Select(a => new { Nombre = a.Obra.Contrata.Nombre }) // Usar texto completo como clave y valor
                        .Distinct()
                        .OrderBy(c => c.Nombre)
                        .ToList();
                    cblsHeaderContrata.DataTextField = "Nombre";
                    cblsHeaderContrata.DataValueField = "Nombre"; // Usar el texto completo como valor
                    cblsHeaderContrata.DataSource = items;
                    cblsHeaderContrata.DataBind();
                }

                // Poblar filtro de Empresa (basado en Obra.Empresa)
                var cblsHeaderEmpresa = e.Row.FindControl("cblsHeaderEmpresa") as WebForms.CustomControls.TreeViewSearch;
                if (cblsHeaderEmpresa != null)
                {
                    var items = autorizantesCompletos
                        .Where(a => a.Obra?.Empresa != null)
                        .Select(a => new { Id = a.Obra.Empresa.Id, Nombre = a.Obra.Empresa.Nombre })
                        .Distinct()
                        .OrderBy(em => em.Nombre)
                        .ToList();
                    cblsHeaderEmpresa.DataSource = items;
                    cblsHeaderEmpresa.DataBind();
                }

                // Poblar filtro de Código Autorizante (nuevo)
                var cblsHeaderCodigoAutorizante = e.Row.FindControl("cblsHeaderCodigoAutorizante") as WebForms.CustomControls.TreeViewSearch;
                if (cblsHeaderCodigoAutorizante != null)
                {
                    var items = autorizantesCompletos
                        .Where(a => !string.IsNullOrEmpty(a.CodigoAutorizante))
                        .Select(a => new { CodigoAutorizante = a.CodigoAutorizante })
                        .Distinct()
                        .OrderBy(c => c.CodigoAutorizante)
                        .ToList();
                    cblsHeaderCodigoAutorizante.DataSource = items;
                    cblsHeaderCodigoAutorizante.DataBind();
                }

                // Poblar filtro de Concepto
                var cblsHeaderConcepto = e.Row.FindControl("cblsHeaderConcepto") as WebForms.CustomControls.TreeViewSearch;
                if (cblsHeaderConcepto != null)
                {
                    var items = autorizantesCompletos
                        .Where(a => a.Concepto != null)
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
                        .Where(a => a.Estado != null)
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
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cambiar de página: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

    }

}