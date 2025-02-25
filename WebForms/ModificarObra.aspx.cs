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
    public partial class ModificarObra : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {


            int codM;
            ObraNegocio negocio = new ObraNegocio();
            if (!IsPostBack)
            {
                if (Request.QueryString["codM"] != null)
                {

                    ddlEmpresa.DataSource = ObtenerEmpresas();
                    ddlEmpresa.DataTextField = "Nombre";
                    ddlEmpresa.DataValueField = "Id";
                    ddlEmpresa.DataBind();

                    ddlContrata.DataSource = ObtenerContratas();
                    ddlContrata.DataTextField = "Nombre";
                    ddlContrata.DataValueField = "Id";
                    ddlContrata.DataBind();

                    ddlBarrio.DataSource = ObtenerBarrios();
                    ddlBarrio.DataTextField = "Nombre";
                    ddlBarrio.DataValueField = "Id";
                    ddlBarrio.DataBind();

                    ValidationSettings.UnobtrusiveValidationMode = UnobtrusiveValidationMode.None;
                    codM = Convert.ToInt32(Request.QueryString["codM"]);
                    List<Obra> temp = (List<Obra>)Session["listaObraAdmin"];
                    Obra selected = temp.Find(x => x.Id == codM);
        

                    txtNumero.Text = selected.Numero?.ToString();
                    txtAño.Text = selected.Año.ToString();
                    txtEtapa.Text = selected.Etapa.ToString();
                    txtObra.Text = selected.ObraNumero.ToString();
                    txtDescripcion.Text = selected.Descripcion;

                    ddlEmpresa.SelectedValue = selected.Empresa.Id.ToString();
                    ddlContrata.SelectedValue = selected.Contrata.Id.ToString();
                    ddlBarrio.SelectedValue = selected.Barrio.Id.ToString();
                     }
            }
        }
        protected void btnModificar_Click(object sender, EventArgs e)
        {
            ObraNegocio negocio = new ObraNegocio();
            try
            {
                if (txtNumero.Text.Trim() != string.Empty &&
                    txtAño.Text.Trim() != string.Empty &&
                    txtEtapa.Text.Trim() != string.Empty &&
                    txtObra.Text.Trim() != string.Empty &&
                    ddlEmpresa.SelectedIndex != -1 &&
                    ddlContrata.SelectedIndex != -1 &&
                    ddlBarrio.SelectedIndex != -1)
                {
                    // Recuperar la obra a modificar
                    Obra obraModificada = new Obra
                    {
                        Id = int.Parse(Request.QueryString["codM"]),
                        Numero = int.Parse(txtNumero.Text.Trim()),
                        Año = int.Parse(txtAño.Text.Trim()),
                        Etapa = int.Parse(txtEtapa.Text.Trim()),
                        ObraNumero = int.Parse(txtObra.Text.Trim()),
                        Descripcion = txtDescripcion.Text.Trim(),
                        Empresa = new Empresa { Id = int.Parse(ddlEmpresa.SelectedValue) },
                        Contrata = new Contrata { Id = int.Parse(ddlContrata.SelectedValue) },
                        Barrio = new Barrio { Id = int.Parse(ddlBarrio.SelectedValue) }
                    };

                    // Llamar al método modificar de ObraNegocio
                    if (negocio.modificar(obraModificada))
                    {
                        lblMensaje.Text = "Obra modificada exitosamente!";
                        lblMensaje.CssClass = "alert alert-success";

                        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), "redirectJS",
 "setTimeout(function() { window.location.replace('ObrasAdmin.aspx') }, 3000);", true);

                    }
                    else
                    {
                        lblMensaje.Text = "Hubo un problema al modificar la obra.";
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
                lblMensaje.Text = $"Error al modificar la obra: {ex.Message}";
                lblMensaje.CssClass = "alert alert-danger";
            }
        }
        private DataTable ObtenerEmpresas()
        {
            EmpresaNegocio empresaNegocio = new EmpresaNegocio();
            return empresaNegocio.listarddl();
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
    }
}