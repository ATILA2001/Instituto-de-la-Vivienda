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
    public partial class Autorizantes : System.Web.UI.Page
    {
        private AutorizanteNegocio negocio = new AutorizanteNegocio();

        protected void Page_Init(object sender, EventArgs e)
        {
            cblEmpresa.SelectedIndexChanged += OnCheckBoxListSearch_SelectedIndexChanged;
            cblConcepto.SelectedIndexChanged += OnCheckBoxListSearch_SelectedIndexChanged;
            cblEstado.SelectedIndexChanged += OnCheckBoxListSearch_SelectedIndexChanged;
            cblObra.SelectedIndexChanged += OnCheckBoxListSearch_SelectedIndexChanged;
        }

        private void OnCheckBoxListSearch_SelectedIndexChanged(object sender, EventArgs e)
        {
            CargarListaAutorizantes();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindDropDownList();
                CargarListaAutorizantes();
            }
        }

        protected void btnLimpiar_Click(object sender, EventArgs e)
        {
            //COMENTADO POR CIERRE PLANIFICACION
            ddlConcepto.SelectedIndex = -1;
            txtDetalle.Text = string.Empty;
            txtMontoAutorizado.Text = string.Empty;
            txtExpediente.Text = string.Empty;
            txtFecha.Text = string.Empty;
            ddlObra.SelectedIndex = -1;
            ddlEstado.SelectedIndex = -1;

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
        private void CargarListaAutorizantes(string filtro = null)
        {
            try
            {
                Usuario usuarioLogueado = (Usuario)Session["usuario"];


                var selectedEmpresas = cblEmpresa.Items.Cast<ListItem>().Where(i => i.Selected).Select(i => i.Text).ToList();
                var selectedConceptos = cblConcepto.Items.Cast<ListItem>().Where(i => i.Selected).Select(i => i.Text).ToList();
                var selectedEstados = cblEstado.Items.Cast<ListItem>().Where(i => i.Selected).Select(i => i.Text).ToList();
                var selectedObras = cblObra.Items.Cast<ListItem>().Where(i => i.Selected).Select(i => i.Value).ToList();


                Session["listaAutorizante"] = negocio.listar(usuarioLogueado, selectedEstados, selectedEmpresas, selectedConceptos, selectedObras,filtro);
                dgvAutorizante.DataSource = Session["listaAutorizante"];
                dgvAutorizante.DataBind();
                CalcularSubtotal();
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cargar los Autorizantes: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }
        
        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            string filtro = txtBuscar.Text.Trim(); // Obtener el texto del buscador

            CargarListaAutorizantes(filtro);
        }


        protected void dgvAutorizante_SelectedIndexChanged(object sender, EventArgs e)
        {
            var idSeleccionado = dgvAutorizante.SelectedDataKey.Value.ToString();
            Response.Redirect("ModificarAutorizante.aspx?codM=" + idSeleccionado);
        }
        protected void dgvAutorizante_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {

            try
            {
                var id = dgvAutorizante.DataKeys[e.RowIndex].Value.ToString();
                if (negocio.eliminar(id))
                {
                    lblMensaje.Text = "Autorizante eliminado correctamente.";
                    lblMensaje.CssClass = "alert alert-success";
                    CargarListaAutorizantes();
                    CalcularSubtotal();
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al eliminar el Autorizante: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
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
        protected void btnAgregar_Click(object sender, EventArgs e)
        {
            AutorizanteNegocio autorizanteNegocio = new AutorizanteNegocio();
            Autorizante nuevoAutorizante = new Autorizante();

            nuevoAutorizante.Obra = new Obra();
            nuevoAutorizante.Obra.Id = int.Parse(ddlObra.SelectedValue); // COMENTADO POR CIERRE PLANIFICACION
            nuevoAutorizante.Concepto = new Concepto();
            nuevoAutorizante.Concepto.Id = int.Parse(ddlConcepto.SelectedValue);// COMENTADO POR CIERRE PLANIFICACION
            nuevoAutorizante.Detalle = txtDetalle.Text;// COMENTADO POR CIERRE PLANIFICACION
            nuevoAutorizante.Expediente = txtExpediente.Text;// COMENTADO POR CIERRE PLANIFICACION
            nuevoAutorizante.Estado = new EstadoAutorizante();
            nuevoAutorizante.Estado.Id = int.Parse(ddlEstado.SelectedValue);// COMENTADO POR CIERRE PLANIFICACION
            nuevoAutorizante.MontoAutorizado = decimal.Parse(txtMontoAutorizado.Text);// COMENTADO POR CIERRE PLANIFICACION
            nuevoAutorizante.Fecha = DateTime.Parse(txtFecha.Text);// COMENTADO POR CIERRE PLANIFICACION
            nuevoAutorizante.MesBase = string.IsNullOrWhiteSpace(txtMes.Text) ? (DateTime?)null : DateTime.Parse(txtMes.Text);// COMENTADO POR CIERRE PLANIFICACION
            autorizanteNegocio.agregar(nuevoAutorizante);

            lblMensaje.Text = "Autorizante agregado con éxito.";
            CargarListaAutorizantes();
            CalcularSubtotal();
        }
        private void BindDropDownList()
        {
            //COMENTADO POR CIERRE PLANIFICACION
            ddlEstado.DataSource = ObtenerEstado();
            ddlEstado.DataTextField = "Nombre";
            ddlEstado.DataValueField = "Id";
            ddlEstado.DataBind();

            ddlConcepto.DataSource = ObtenerConcepto();
            ddlConcepto.DataTextField = "Nombre";
            ddlConcepto.DataValueField = "Id";
            ddlConcepto.DataBind();

            ddlObra.DataSource = ObtenerObras();
            ddlObra.DataTextField = "Nombre";
            ddlObra.DataValueField = "Id";
            ddlObra.DataBind();

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
        private DataRow CrearFilaTodos(DataTable table)
        {
            DataRow row = table.NewRow();
            row["Id"] = 0;
            row["Nombre"] = "Todos";
            return row;
        }

        private DataTable ObtenerEstado()
        {
            EstadoAutorizanteNegocio empresaNegocio = new EstadoAutorizanteNegocio();
            return empresaNegocio.listarddl();
        }
        private DataTable ObtenerConcepto()
        {
            ConceptoNegocio empresaNegocio = new ConceptoNegocio();
            return empresaNegocio.listarddl();
        }

        private DataTable ObtenerObras()
        {
            ObraNegocio barrioNegocio = new ObraNegocio();
            Usuario usuarioLogueado = (Usuario)Session["usuario"];
            return barrioNegocio.listarddl(usuarioLogueado);
        }

        protected void dgvAutorizante_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                DropDownList ddlEstadoAutorizante = (DropDownList)e.Row.FindControl("ddlEstadoAutorizante");

                if (ddlEstadoAutorizante != null)
                {
                    DataTable estados = ObtenerEstado();
                    ddlEstadoAutorizante.DataSource = estados;
                    ddlEstadoAutorizante.DataTextField = "Nombre";
                    ddlEstadoAutorizante.DataValueField = "Id";
                    ddlEstadoAutorizante.DataBind();

                    string estadoActual = DataBinder.Eval(e.Row.DataItem, "Estado.Id").ToString();
                    ddlEstadoAutorizante.SelectedValue = estadoActual;
                }
            }
        }
        protected void ddlEstadoAutorizante_SelectedIndexChanged(object sender, EventArgs e)
        {
            DropDownList ddlEstadoAutorizante = (DropDownList)sender;
            GridViewRow row = (GridViewRow)ddlEstadoAutorizante.NamingContainer;

            List<Autorizante> listaAutorizantes = (List<Autorizante>)Session["listaAutorizante"];
            string codAutorizante = dgvAutorizante.DataKeys[row.RowIndex].Value.ToString();
            Autorizante autorizante = listaAutorizantes.Find(a => a.CodigoAutorizante == codAutorizante);

            if (autorizante != null)
            {
                autorizante.Estado.Id = int.Parse(ddlEstadoAutorizante.SelectedValue);
                AutorizanteNegocio negocio = new AutorizanteNegocio();
                negocio.ActualizarEstado(autorizante);
                CargarListaAutorizantes();

                lblMensaje.Text = "Estado actualizado correctamente.";
                lblMensaje.CssClass = "alert alert-success";
            }
        }

        protected void txtExpediente_TextChanged(object sender, EventArgs e)
        {
            // Identifica el TextBox modificado
            TextBox txtExpediente = (TextBox)sender;
            GridViewRow row = (GridViewRow)txtExpediente.NamingContainer;

            // Obtiene la clave del registro desde DataKeyNames
            string codigoAutorizante = dgvAutorizante.DataKeys[row.RowIndex].Value.ToString();

            // Nuevo valor del expediente
            string nuevoExpediente = txtExpediente.Text;

            // Actualiza en la base de datos
            try
            {
                // Llama al método del negocio para actualizar el expediente
                AutorizanteNegocio negocio = new AutorizanteNegocio();
                negocio.ActualizarExpediente(codigoAutorizante, nuevoExpediente);

                // Mensaje de éxito o retroalimentación opcional
                lblMensaje.Text = "Expediente actualizado correctamente.";
                CargarListaAutorizantes();
                CalcularSubtotal();

            }
            catch (Exception ex)
            {
                // Manejo de errores
                lblMensaje.Text = "Error al actualizar el expediente: " + ex.Message;
            }
        }


        protected void BtnClearFilters_Click(object sender, EventArgs e)
        {
            txtBuscar.Text = string.Empty;
            cblEmpresa.ClearSelection();
            cblConcepto.ClearSelection();
            cblEstado.ClearSelection();
            cblObra.ClearSelection();
            CargarListaAutorizantes();
        }

    }
}