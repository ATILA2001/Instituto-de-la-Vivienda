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
    public partial class ModificarAutorizanteAdmin : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Request.QueryString["codM"] != null)
                {
                    var codM = Request.QueryString["codM"];
                    AutorizanteNegocio negocio = new AutorizanteNegocio();
                    ddlObra.DataSource = ObtenerObras();
                    ddlObra.DataTextField = "Nombre";
                    ddlObra.DataValueField = "Id";
                    ddlObra.DataBind();
                    ddlConcepto.DataSource = ObtenerConcepto();
                    ddlConcepto.DataTextField = "Nombre";
                    ddlConcepto.DataValueField = "Id";
                    ddlConcepto.DataBind();

                    ValidationSettings.UnobtrusiveValidationMode = UnobtrusiveValidationMode.None;

                    List<Autorizante> temp = (List<Autorizante>)Session["listaAutorizanteAdmin"];
                    Autorizante selected = temp?.Find(x => x.CodigoAutorizante == codM);

                    if (selected != null)
                    {

                        ddlConcepto.SelectedValue = selected.Concepto?.Id.ToString();
                        txtDetalle.Text = selected.Detalle ?? string.Empty;
                        txtMontoAutorizado.Text = selected.MontoAutorizado.ToString("0.00");
                        txtFecha.Text = selected.Fecha.HasValue
                            ? selected.Fecha.Value.ToString("yyyy-MM-dd")
                            : string.Empty;

                        txtMes.Text = selected.MesBase.HasValue
                            ? selected.MesBase.Value.ToString("yyyy-MM-dd")
                            : string.Empty;
                        ddlObra.SelectedValue = selected.Obra?.Id.ToString();
                    }
                    else
                    {
                        lblMensaje.Text = "El autorizante no existe.";
                        lblMensaje.CssClass = "alert alert-danger";
                    }
                }
            }
        }

        protected void btnModificar_Click(object sender, EventArgs e)
        {
            AutorizanteNegocio negocio = new AutorizanteNegocio();
            try
            {
                if (ValidarCampos())
                {
                    var codM = Request.QueryString["codM"];

                    Autorizante autorizadoModificado = new Autorizante
                    {
                        CodigoAutorizante = codM,
                        Obra = new Obra { Id = int.Parse(ddlObra.SelectedValue), Descripcion = ddlObra.SelectedItem.Text },
                        Concepto = new Concepto { Id = int.Parse(ddlConcepto.SelectedValue), Nombre = ddlConcepto.SelectedItem.Text },
                        Detalle = txtDetalle.Text.Trim(),
                        MontoAutorizado = decimal.Parse(txtMontoAutorizado.Text.Trim()),
                        Fecha = string.IsNullOrEmpty(txtFecha.Text.Trim())
                            ? (DateTime?)null
                            : DateTime.Parse(txtFecha.Text.Trim()),
                        MesBase = string.IsNullOrEmpty(txtMes.Text.Trim())
                            ? (DateTime?)null
                            : DateTime.Parse(txtMes.Text.Trim()),

                    };

                    if (negocio.modificar(autorizadoModificado))
                    {
                        lblMensaje.Text = "Autorizante modificado exitosamente.";
                        lblMensaje.CssClass = "alert alert-success";

                        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), "redirectJS",
                            "setTimeout(function() { window.location.replace('AutorizantesAdmin.aspx') }, 3000);", true);
                    }
                    else
                    {
                        lblMensaje.Text = "Hubo un problema al modificar el autorizante.";
                        lblMensaje.CssClass = "alert alert-danger";
                    }
                }
                else
                {
                    lblMensaje.Text = "Debe completar todos los campos requeridos.";
                    lblMensaje.CssClass = "alert alert-danger";
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al modificar el autorizante: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }
        private DataTable ObtenerConcepto()
        {
            ConceptoNegocio empresaNegocio = new ConceptoNegocio();
            return empresaNegocio.listarddl();
        }
        private DataTable ObtenerEstado()
        {
            EstadoAutorizanteNegocio estadoNegocio = new EstadoAutorizanteNegocio();
            return estadoNegocio.listarddl();
        }
        private DataTable ObtenerObras()
        {
            ObraNegocio barrioNegocio = new ObraNegocio();
            return barrioNegocio.listarddl();
        }

        private bool ValidarCampos()
        {
            return ddlObra.SelectedIndex != -1 &&
                   !string.IsNullOrWhiteSpace(ddlConcepto.Text) &&
                   !string.IsNullOrWhiteSpace(txtDetalle.Text) &&
                   !string.IsNullOrWhiteSpace(txtMontoAutorizado.Text);
        }
    }
}