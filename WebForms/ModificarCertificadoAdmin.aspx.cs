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
    public partial class ModificarCertificadoAdmin : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            int codM;
            CertificadoNegocio negocio = new CertificadoNegocio();

            if (!IsPostBack)
            {
                if (Request.QueryString["codM"] != null)
                {
                    // Llenar los DropDownList con datos correspondientes
                    ddlTipo.DataSource = ObtenerTiposPago();  // Método para obtener los datos de Tipos de Pago
                    ddlTipo.DataTextField = "Nombre";         // Columna que se muestra
                    ddlTipo.DataValueField = "Id";            // Columna que se almacena
                    ddlTipo.DataBind();

                    ValidationSettings.UnobtrusiveValidationMode = UnobtrusiveValidationMode.None;

                    // Obtener y mostrar los datos del certificado seleccionado
                    codM = Convert.ToInt32(Request.QueryString["codM"]);
                    List<Certificado> temp = (List<Certificado>)Session["listaCertificado"];
                    Certificado selected = temp.Find(x => x.Id == codM);

                    txtExpediente.Text = selected.ExpedientePago;
                    txtMontoAutorizado.Text = selected.MontoTotal.ToString("0.00");
                    txtFecha.Text = selected.MesAprobacion?.ToString("yyyy-MM-dd");

                    ddlTipo.SelectedValue = selected.Tipo.Id.ToString();
                    txtAutorizante.Text = selected.Autorizante.CodigoAutorizante.ToString();
                }
            }
        }

        protected void btnModificar_Click(object sender, EventArgs e)
        {
            CertificadoNegocio negocio = new CertificadoNegocio();
            try
            {
                if (
                    txtExpediente.Text.Trim() != string.Empty &&
                    txtMontoAutorizado.Text.Trim() != string.Empty &&
                    ddlTipo.SelectedIndex != -1 &&
                    txtAutorizante.Text.Trim() != string.Empty )
                {
                    // Crear el objeto modificado
                    Certificado certificadoModificado = new Certificado
                    {
                        Id = int.Parse(Request.QueryString["codM"]),
                        ExpedientePago = txtExpediente.Text.Trim(),
                        MontoTotal = decimal.Parse(txtMontoAutorizado.Text.Trim()),
                        MesAprobacion = string.IsNullOrEmpty(txtFecha.Text.Trim())
                            ? (DateTime?)null
                            : DateTime.Parse(txtFecha.Text.Trim()),
                        Tipo = new TipoPago { Id = int.Parse(ddlTipo.SelectedValue) },
                        Autorizante = new Autorizante
                        {
 
                            CodigoAutorizante = txtAutorizante.Text.Trim()
                        }
                    };

                    // Llamar al método modificar
                    if (negocio.modificar(certificadoModificado))
                    {
                        lblMensaje.Text = "Certificado modificado exitosamente!";
                        lblMensaje.CssClass = "alert alert-success";

                        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), "redirectJS",
                        "setTimeout(function() { window.location.replace('CertificadosAdmin.aspx') }, 3000);", true);
                    }
                    else
                    {
                        lblMensaje.Text = "Hubo un problema al modificar el certificado.";
                        lblMensaje.CssClass = "alert alert-danger";
                    }
                }
                else
                {
                    lblMensaje.Text = "Debe llenar todos los campos correctamente.";
                    lblMensaje.CssClass = "alert alert-danger";
                }
            }
            catch (Exception ex)
            {
                lblMensaje.Text = $"Error al modificar el certificado: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        private DataTable ObtenerTiposPago()
        {
            TipoPagoNegocio tipoPagoNegocio = new TipoPagoNegocio();
            return tipoPagoNegocio.listarddl();
        }


    }

}