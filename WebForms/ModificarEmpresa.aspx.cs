using Dominio;
using Negocio;
using System;
using WebForms.CustomControls;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebForms
{
    public partial class ModificarEmpresa : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            int codM;
            EmpresaNegocio negocio = new EmpresaNegocio();
            if (!IsPostBack)
            {
                if (Request.QueryString["codM"] != null)
                {
                    ValidationSettings.UnobtrusiveValidationMode = UnobtrusiveValidationMode.None;
                    codM = Convert.ToInt32(Request.QueryString["codM"]);
                    List<Empresa> temp = (List<Empresa>)Session["listaEmpresa"];
                    Empresa selected = temp.Find(x => x.Id == codM);
                    txtNombre.Text = selected.Nombre;

                }
            }

        }
        protected void btnModificar_Click(object sender, EventArgs e)
        {
            Empresa empresa = new Empresa();
            EmpresaNegocio negocio = new EmpresaNegocio();

            if (txtNombre.Text.Trim() != string.Empty)
            {
                try
                {
                    empresa.Id = int.Parse(Request.QueryString["codM"].ToString());
                    empresa.Nombre = txtNombre.Text.Trim();
                    negocio.modificar(empresa);
                    ToastService.Show(this.Page, "Se modificó la empresa exitosamente.", ToastService.ToastType.Success);

                    ScriptManager.RegisterClientScriptBlock(this, typeof(Page), "redirectJS",
                    "setTimeout(function() { window.location.replace('AbmlEmpresa.aspx') }, 3000);", true);
                }
                catch (InvalidOperationException ex)
                {
                    ToastService.Show(this.Page, ex.Message, ToastService.ToastType.Error);
                }
                catch (Exception)
                {
                    ToastService.Show(this.Page, "No se pudo modificar la empresa. Intente nuevamente.", ToastService.ToastType.Error);
                }
            }
            else
            {
                ToastService.Show(this.Page, "Tiene que llenar todos los campos.", ToastService.ToastType.Warning);
            }
        }
        protected void btnVolver_Click(object sender, EventArgs e)
        {
            Response.Redirect("AbmlEmpresa.aspx");
        }

    }
}