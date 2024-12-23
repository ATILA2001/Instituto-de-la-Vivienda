using Dominio;
using Negocio;
using System;
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

            if (txtNombre.Text.Trim() != string.Empty )
            {
                barrio.Id = int.Parse(Request.QueryString["codM"].ToString());
                barrio.Nombre = txtNombre.Text.Trim();
                negocio.modificar(barrio);
                lblMensaje.Text = "Se modificó el barrio exitosamente.";
                lblMensaje.CssClass = "alert alert-success";

                ScriptManager.RegisterClientScriptBlock(this, typeof(Page), "redirectJS",
                "setTimeout(function() { window.location.replace('Abmlbarrio.aspx') }, 3000);", true);
            }
            else
            {
                lblMensaje.Text = "Tiene que llenar todos los campos.";
                lblMensaje.CssClass = "alert alert-success";
            }
        }
        protected void btnVolver_Click(object sender, EventArgs e)
        {
            Response.Redirect("Abmlbarrio.aspx");
        }
    }
}