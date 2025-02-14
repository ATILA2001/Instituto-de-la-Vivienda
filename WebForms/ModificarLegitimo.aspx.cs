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
    public partial class ModificarLegitimo : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            int codM;
            LegitimoNegocio negocio = new LegitimoNegocio();

            if (!IsPostBack)
            {
                if (Request.QueryString["codM"] != null)
                {

                    ValidationSettings.UnobtrusiveValidationMode = UnobtrusiveValidationMode.None;
                    codM = Convert.ToInt32(Request.QueryString["codM"]);
                    List<Legitimo> temp = (List<Legitimo>)Session["listaLegitimos"];
                    Legitimo selected = new Legitimo();
                    selected =  temp.Find(x => x.Id == codM);

                    txtExpediente.Text = selected.Expediente;
                    txtInicioEjecucion.Text = selected.InicioEjecucion?.ToString("yyyy-MM-dd");
                    txtFinEjecucion.Text = selected.FinEjecucion?.ToString("yyyy-MM-dd");
                    txtCertificado.Text = selected.Certificado.ToString();
                    txtMesAprobacion.Text = selected.MesAprobacion.ToString();

                    txtObra.Text = selected.Obra.Descripcion.ToString();
                    txtCodigoAutorizante.Text = selected.CodigoAutorizante.ToString();
                }
            }
        }

        protected void btnModificar_Click(object sender, EventArgs e)
        {
            LegitimoNegocio negocio = new LegitimoNegocio();
            try
            {
                if (
                    txtExpediente.Text.Trim() != string.Empty &&
                    txtObra.Text.Trim() != string.Empty &&
                    txtInicioEjecucion.Text.Trim() != string.Empty &&
                    txtFinEjecucion.Text.Trim() != string.Empty &&
                    txtCertificado.Text.Trim() != string.Empty &&
                    txtMesAprobacion.Text.Trim() != string.Empty)
                {
                    Legitimo legitimoModificado = new Legitimo
                    {
                        Id = int.Parse(Request.QueryString["codM"]),
                        CodigoAutorizante = txtCodigoAutorizante.Text.Trim(),
                        Obra = new Obra { Descripcion = txtObra.Text.Trim() },
                        Expediente = txtExpediente.Text.Trim(),
                        InicioEjecucion = DateTime.Parse(txtInicioEjecucion.Text.Trim()),
                        FinEjecucion = DateTime.Parse(txtFinEjecucion.Text.Trim()),
                        Certificado = decimal.Parse(txtCertificado.Text.Trim()),
                        MesAprobacion = DateTime.Parse(txtMesAprobacion.Text.Trim())
                    };

                    if (negocio.modificar(legitimoModificado))
                    {
                        lblMensaje.Text = "Legítimo modificado exitosamente!";
                        lblMensaje.CssClass = "alert alert-success";

                        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), "redirectJS",
                        "setTimeout(function() { window.location.replace('Legitimos.aspx') }, 3000);", true);
                    }
                    else
                    {
                        lblMensaje.Text = "Hubo un problema al modificar el legítimo.";
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
                lblMensaje.Text = $"Error al modificar el legítimo: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }

        private DataTable ObtenerObras()
        {
            ObraNegocio obraNegocio = new ObraNegocio();
            return obraNegocio.listarddl();
        }

    }
}
