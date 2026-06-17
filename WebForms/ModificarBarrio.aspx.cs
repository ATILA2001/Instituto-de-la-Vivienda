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
    public partial class ModificarBarrio : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            int codM;
            BarrioNegocio negocio = new BarrioNegocio();
            if (!IsPostBack)
            {
                if (Request.QueryString["codM"] != null)
                {
                    ValidationSettings.UnobtrusiveValidationMode = UnobtrusiveValidationMode.None;
                    codM = Convert.ToInt32(Request.QueryString["codM"]);
                    List<Barrio> temp = (List<Barrio>)Session["listaBarrio"];
                    Barrio selected = temp.Find(x => x.Id == codM);
                    txtNombre.Text = selected.Nombre;

                }
            }

        }
        protected void btnModificar_Click(object sender, EventArgs e)
        {
            Barrio barrio = new Barrio();
            BarrioNegocio negocio = new BarrioNegocio();

            if (txtNombre.Text.Trim() != string.Empty)
            {
                try
                {
                    barrio.Id = int.Parse(Request.QueryString["codM"].ToString());
                    barrio.Nombre = txtNombre.Text.Trim();
                    negocio.modificar(barrio);
                    ToastService.Show(this.Page, "Se modificó el barrio exitosamente.", ToastService.ToastType.Success);

                    ScriptManager.RegisterClientScriptBlock(this, typeof(Page), "redirectJS",
                    "setTimeout(function() { window.location.replace('Abmlbarrio.aspx') }, 3000);", true);
                }
                catch (InvalidOperationException ex)
                {
                    ToastService.Show(this.Page, ex.Message, ToastService.ToastType.Error);
                }
                catch (Exception)
                {
                    ToastService.Show(this.Page, "No se pudo modificar el barrio. Intente nuevamente.", ToastService.ToastType.Error);
                }
            }
            else
            {
                ToastService.Show(this.Page, "Tiene que llenar todos los campos.", ToastService.ToastType.Warning);
            }
        }
        protected void btnVolver_Click(object sender, EventArgs e)
        {
            Response.Redirect("Abmlbarrio.aspx");
        }
    }
}