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
    public partial class Redeterminaciones : System.Web.UI.Page
    {
        RedeterminacionNegocio negocio = new RedeterminacionNegocio();
        protected void Page_Load(object sender, EventArgs e)
        {

            if (!IsPostBack)
            {

                BindDropDownList();

                CargarListaRedeterminacion();
            }
        }

        protected void btnLimpiar_Click(object sender, EventArgs e)
        {
            //txtMontoAutorizado.Text = string.Empty;
            //txtExpediente.Text = string.Empty;
            //txtFecha.Text = string.Empty;
            //ddlAutorizante.SelectedIndex = -1;
            //ddlTipo.SelectedIndex = -1;
        }
        protected void BtnClearFilters_Click(object sender, EventArgs e)
        {
            txtBuscar.Text = string.Empty;
            cblObra.ClearSelection();
            cblEtapa.ClearSelection();
            cblAutorizante.ClearSelection();
            CargarListaRedeterminacion();
        }
        private void CargarListaRedeterminacion(string filtro = null)
        {
            try
            {

                var selectedObras = cblObra.Items.Cast<ListItem>().Where(i => i.Selected).Select(i => i.Text).ToList();
                var selectedAutorizantes = cblAutorizante.Items.Cast<ListItem>().Where(i => i.Selected).Select(i => i.Text).ToList();
                var selectedEtapas = cblEtapa.Items.Cast<ListItem>().Where(i => i.Selected).Select(i => i.Text).ToList();

                //var selectedFechas = cblFecha.Items.Cast<ListItem>().Where(i => i.Selected).Select(i => i.Value).ToList();

                Session["listaRedeterminacion"] = negocio.listar(selectedEtapas, selectedAutorizantes, selectedObras, filtro);
                dgvRedeterminacion.DataSource = Session["listaRedeterminacion"];
                dgvRedeterminacion.DataBind();

            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al cargar las redeterminaciones: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        protected void dgvRedeterminacion_SelectedIndexChanged(object sender, EventArgs e)
        {
            var idSeleccionado = dgvRedeterminacion.SelectedDataKey.Value.ToString();
            Response.Redirect("ModificarCertificado.aspx?codM=" + idSeleccionado);
        }
        protected void dgvRedeterminacion_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {

            try
            {
                var id = Convert.ToInt32(dgvRedeterminacion.DataKeys[e.RowIndex].Value);
                if (negocio.eliminar(id))
                {
                    lblMensaje.Text = "Certificado eliminado correctamente.";
                    lblMensaje.CssClass = "alert alert-success";
                    CargarListaRedeterminacion();
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al eliminar el certificado: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        protected void dgvRedeterminacion_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            try
            {
                dgvRedeterminacion.PageIndex = e.NewPageIndex;

                CargarListaRedeterminacion();
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
        //        RedeterminacionNegocio redeterminacionNegocio = new RedeterminacionNegocio();
        //        Redeterminacion nuevaRedet = new Redeterminacion
        //        {
        //            Autorizante = new Autorizante
        //            {
        //                CodigoAutorizante = ddlAutorizante.SelectedItem.Text
        //            },
        //            Expediente = string.IsNullOrWhiteSpace(txtExpediente.Text) ? null : txtExpediente.Text,
        //            Salto = string.IsNullOrWhiteSpace(txtSalto.Text)
        //                ? (DateTime?)null
        //                : DateTime.Parse(txtSalto.Text),
        //            Nro = string.IsNullOrWhiteSpace(txtNro.Text)
        //                ? (int?)null
        //                : int.Parse(txtNro.Text),
        //            Tipo = string.IsNullOrWhiteSpace(txtTipo.Text) ? null : txtTipo.Text,
        //            Etapa = new EstadoRedet
        //            {
        //                Id = int.Parse(ddlEtapa.SelectedValue)
        //            },
        //            Observaciones = string.IsNullOrWhiteSpace(txtObservacion.Text) ? null : txtObservacion.Text,
        //            Porcentaje = string.IsNullOrWhiteSpace(txtPorcentaje.Text)
        //                ? (int?)null
        //                : int.Parse(txtPorcentaje.Text)
        //        };

        //        if (redeterminacionNegocio.agregar(nuevaRedet))
        //        {
        //            lblMensaje.Text = "Redeterminación agregada con éxito.";
        //            lblMensaje.ForeColor = System.Drawing.Color.Green;

        //            // Aquí puedes recargar la lista o limpiar los campos si es necesario
        //            CargarListaRedeterminacion();
        //            // LimpiarCampos();
        //        }
        //        else
        //        {
        //            lblMensaje.Text = "Hubo un problema al agregar la redeterminación.";
        //            lblMensaje.ForeColor = System.Drawing.Color.Red;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        lblMensaje.Text = $"Error al agregar la redeterminación: {ex.Message}";
        //        lblMensaje.ForeColor = System.Drawing.Color.Red;
        //    }
        //}



        private DataTable ObtenerTipos()
        {
            EstadoRedetNegocio tipoPagNegocio = new EstadoRedetNegocio();
            return tipoPagNegocio.listarddl();
        }

        private DataTable ObtenerAutorizantes()
        {
            AutorizanteNegocio autorizanteNegocio = new AutorizanteNegocio();
            return autorizanteNegocio.listarddl();
        }
        private void BindDropDownList()
        {
            //ddlEtapa.DataSource = ObtenerTipos();
            //ddlEtapa.DataTextField = "Nombre";
            //ddlEtapa.DataValueField = "Id";
            //ddlEtapa.DataBind();


            cblEtapa.DataSource = ObtenerTipos();
            cblEtapa.DataTextField = "Nombre";
            cblEtapa.DataValueField = "Id";
            cblEtapa.DataBind();

            cblObra.DataSource = ObtenerObras();
            cblObra.DataTextField = "Nombre";
            cblObra.DataValueField = "Id";
            cblObra.DataBind();


            //ddlAutorizante.DataSource = ObtenerAutorizantes();
            //ddlAutorizante.DataTextField = "Nombre";
            //ddlAutorizante.DataValueField = "Id";
            //ddlAutorizante.DataBind();

            cblAutorizante.DataSource = ObtenerAutorizantes();
            cblAutorizante.DataTextField = "Nombre";
            cblAutorizante.DataValueField = "Id";
            cblAutorizante.DataBind();

            //var meses = Enumerable.Range(0, 36) // 36 meses entre 2024 y 2026
            //.Select(i => new DateTime(2024, 1, 1).AddMonths(i))
            //.Select(fecha => new
            //{
            //    Texto = fecha.ToString("MMMM yyyy", new System.Globalization.CultureInfo("es-ES")), // Texto: "Enero 2024"
            //    Valor = fecha.ToString("yyyy-MM-dd")
            //});

            //cblFecha.DataSource = meses;
            //cblFecha.DataTextField = "Texto";
            //cblFecha.DataValueField = "Valor";
            //cblFecha.DataBind();

        }
        private DataTable ObtenerObras()
        {
            ObraNegocio empresaNegocio = new ObraNegocio();
            return empresaNegocio.listarddl();
        }
        private DataRow CrearFilaTodos(DataTable table)
        {
            DataRow row = table.NewRow();
            row["Id"] = 0;
            row["Nombre"] = "Todos";
            return row;
        }


        protected void txtExpediente_TextChanged(object sender, EventArgs e)
        {
            //// Identifica el TextBox modificado
            //TextBox txtExpediente = (TextBox)sender;
            //GridViewRow row = (GridViewRow)txtExpediente.NamingContainer;

            //// Obtiene la clave del registro desde DataKeyNames
            //int id = int.Parse(dgvRedeterminacion.DataKeys[row.RowIndex].Value.ToString());

            //// Nuevo valor del expediente
            //string nuevoExpediente = txtExpediente.Text;

            //// Actualiza en la base de datos
            //try
            //{
            //    // Llama al método del negocio para actualizar el expediente
            //    CertificadoNegocio negocio = new CertificadoNegocio();
            //    negocio.ActualizarExpediente(id, nuevoExpediente);

            //    // Mensaje de éxito o retroalimentación opcional
            //    lblMensaje.Text = "Expediente actualizado correctamente.";
            //    CargarListaCertificados();
            //    CalcularSubtotal();

            //}
            //catch (Exception ex)
            //{
            //    // Manejo de errores
            //    lblMensaje.Text = "Error al actualizar el expediente: " + ex.Message;
            //}
        }
        protected void btnFiltrar_Click(object sender, EventArgs e)
        {
            string filtro = txtBuscar.Text.Trim();
            CargarListaRedeterminacion(filtro);
        }

        protected void ddlEtapas_SelectedIndexChanged(object sender, EventArgs e)
        {
            DropDownList ddlEtapas = (DropDownList)sender;
            GridViewRow row = (GridViewRow)ddlEtapas.NamingContainer;

            List<Redeterminacion> listaRedeterminaciones = (List<Redeterminacion>)Session["listaRedeterminacion"];
            int id = int.Parse(dgvRedeterminacion.DataKeys[row.RowIndex].Value.ToString());
            Redeterminacion autorizante = listaRedeterminaciones.Find(a => a.Id == id);

            if (autorizante != null)
            {
                autorizante.Etapa.Id = int.Parse(ddlEtapas.SelectedValue);
                RedeterminacionNegocio negocio = new RedeterminacionNegocio();
                negocio.ActualizarEstado(autorizante);
                CargarListaRedeterminacion();

                lblMensaje.Text = "Etapa actualizada correctamente.";
                lblMensaje.CssClass = "alert alert-success";
            }
        }

        protected void dgvRedeterminacion_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                DropDownList ddlEtapas = (DropDownList)e.Row.FindControl("ddlEtapas");

                if (ddlEtapas != null)
                {
                    DataTable estados = ObtenerTipos();
                    ddlEtapas.DataSource = estados;
                    ddlEtapas.DataTextField = "Nombre";
                    ddlEtapas.DataValueField = "Id";
                    ddlEtapas.DataBind();

                    string estadoActual = DataBinder.Eval(e.Row.DataItem, "Etapa.Id").ToString();
                    ddlEtapas.SelectedValue = estadoActual;
                }
            }
        }
    }

}