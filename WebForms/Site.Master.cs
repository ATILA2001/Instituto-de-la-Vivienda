using Dominio;
using Negocio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebForms
{
    public partial class Site : System.Web.UI.MasterPage
    {
        protected void Page_Init(object sender, EventArgs e)
        {
            // Comprobación básica de usuario logueado
            if (Session["Usuario"] == null)
            {
                Response.Redirect("Login.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                Response.End();
            }
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            UsuarioEF currentUser = UserHelper.GetFullCurrentUser();

            // Obtenemos si el usuario es administrador.
            bool isAdmin = currentUser?.Tipo == true;

            ShowOrHideAdminNavItem(isAdmin);
            ShowOrHideTechosAndPpiTextBoxes(isAdmin);

            if (!isAdmin)
                ShowOrHideUserControlsByPlanningOrFormulationStatus();

        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                chkIsPlanningOpen.Checked = ABMPlaniNegocio.GetIsPlanningOpen();
                chkIsFormulationOpen.Checked = ABMPlaniNegocio.GetIsFormulationOpen();
            }
        }

        protected void ShowOrHideAdminNavItem(bool visible)
        {
            // Si el usuario es administrador, mostramos los enlaces del dropdown Admin.
            liAdminNav.Visible = visible;
            dropdownGestion.Visible = visible;

            // Si el usuario es administrador, no mostramos el enlace basico de Formulación.
            lnkFormulacion.Visible = !visible;

            // Como aun no hemos migrado Obras y Legitimos a la nueva lógica con EF,
            // utilizamos los anteriores, segun el tipo de usuario.

            lnkObras.HRef = visible ? "ObrasAdmin.aspx" : "Obras.aspx";
            lnkLegitimos.HRef = visible ? "LegitimosAdmin.aspx" : "Legitimos.aspx";
        }

        protected void ShowOrHideTechosAndPpiTextBoxes(bool isVisible)
        {
            var content = ContentPlaceHolder1;
            if (content == null) return;
            if (content.FindControl("panelShowTechosAndPpiTextBoxes") is Panel panelShowTechosAndPpiTextBoxes)
            {
                panelShowTechosAndPpiTextBoxes.Visible = isVisible;
            }
        }

        protected void ShowOrHideUserControlsByPlanningOrFormulationStatus()
        {
            bool isPlanningOpen = ABMPlaniNegocio.GetIsPlanningOpen();
            bool isFormulationOpen = ABMPlaniNegocio.GetIsFormulationOpen();

            var content = ContentPlaceHolder1;

            if (content == null) return;

            if (content.FindControl("gridviewRegistros") is GridView grid)
            {
                var actionsColumn = grid.Columns
                    .OfType<DataControlField>()
                    .FirstOrDefault(c => c.HeaderText == "Acciones");

                if (actionsColumn != null)
                {
                    actionsColumn.Visible = isPlanningOpen;
                }
            }

            if (content.FindControl("dgvFormulacion") is GridView formulationGrid)
            {
                var actionsColumn = formulationGrid.Columns
                    .OfType<DataControlField>()
                    .FirstOrDefault(c => c.HeaderText == "Acciones");

                if (actionsColumn != null)
                {
                    actionsColumn.Visible = isFormulationOpen;
                }
            }

            if (content.FindControl("panelShowAddButton") is Panel panelShowAddButton)
            {
                panelShowAddButton.Visible = isPlanningOpen;
            }

            if (content.FindControl("panelFormulationShowAddButton") is Panel panelFormulationShowAddButton)
            {
                panelFormulationShowAddButton.Visible = isFormulationOpen;
            }

        }


        protected void btnCerrarSesion_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Response.Redirect("Login.aspx", false);
        }

        protected void chkIsPlanningOpen_ServerChange(object sender, EventArgs e)
        {
            Negocio.ABMPlaniNegocio.SetIsPlanningOpen(chkIsPlanningOpen.Checked);
        }

        protected void chkIsFormulationOpen_ServerChange(object sender, EventArgs e)
        {
            ABMPlaniNegocio.SetIsFormulationOpen(chkIsFormulationOpen.Checked);
        }
    }
}