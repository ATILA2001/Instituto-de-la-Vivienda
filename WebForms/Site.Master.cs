using Dominio;
using Negocio;
using System;
using System.Collections.Generic;
using System.IO;
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
                Response.Redirect("Startup.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                Response.End();
            }
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            UsuarioEF currentUser = UserHelper.GetFullCurrentUser();

            // Obtenemos si el usuario es administrador.
            bool isAdmin = currentUser?.Tipo == true;
            // Determinar si el usuario pertenece al área de Redeterminaciones
            bool isRedeterminacionesUser = currentUser?.AreaId == 16;
            bool isSecretariaUser = currentUser?.AreaId == 19;


            // Aplicar la configuración de navegación según el tipo de usuario
            if (isAdmin)
            {
                // Caso 1: Usuario administrador - Mostrar todos los enlaces admin
                ShowOrHideAdminNavItem(true);
                ShowOrHideRedeterminacionesNavItems(false);
                ShowOrHideTechosAndPpiTextBoxes(true);
            }
            else if (isRedeterminacionesUser)
            {
                // Caso 2: Usuario del área Redeterminaciones - Solo mostrar enlaces a Obras y Autorizantes
                ShowOrHideAdminNavItem(false);
                ShowOrHideRedeterminacionesNavItems(true);
                ShowOrHideTechosAndPpiTextBoxes(false);
            }
            else
            {
                // Caso 3: Usuario normal - Mostrar enlaces básicos
                ShowOrHideAdminNavItem(false);
                ShowOrHideRedeterminacionesNavItems(false);
                ShowOrHideTechosAndPpiTextBoxes(false);
                lnkFormulacion.Visible = true;

                // Configuración adicional para usuarios normales
                if (isSecretariaUser)
                {
                    // Para usuarios del área 19, forzamos que todos los controles estén ocultos
                    HideAllUserControls();
                }
                else
                {
                    // Para el resto de usuarios normales, aplicamos la configuración estándar
                    ShowOrHideUserControlsByPlanningOrFormulationStatus();
                }
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                chkIsPlanningOpen.Checked = ABMPlaniNegocio.GetIsPlanningOpen();
                chkIsFormulationOpen.Checked = ABMPlaniNegocio.GetIsFormulationOpen();
            }
        }

        protected void ShowOrHideRedeterminacionesNavItems(bool visible)
        {
            lnkFormulacion.Visible = false;
            if (FindControl("liPlaniNav") is Control liPlaniNav)
            {
                liPlaniNav.Visible = !visible;
            }
            if (FindControl("lnkObras") is Control lnkObras)
            {
                lnkObras.Visible = visible;
            }

            if (FindControl("lnkAutorizantes") is Control lnkAutorizantes)
            {
                lnkAutorizantes.Visible = visible;
            }
            if (FindControl("lnkRedeterminaciones") is Control lnkRedeterminaciones)
            {
                lnkRedeterminaciones.Visible = visible;
            }



        }

        protected void ShowOrHideAdminNavItem(bool visible)
        {
            // Si el usuario es administrador, mostramos los enlaces del dropdown Admin.
            liAdminNav.Visible = visible;
            dropdownGestion.Visible = visible;
            lnkFormulacion.Visible = false;


        }

        protected void ShowOrHideTechosAndPpiTextBoxes(bool isVisible)
        {
            var content = ContentPlaceHolder1;
            if (content == null) return;
            if (content.FindControl("panelShowTechosAndPpiTextBoxes") is Panel panelShowTechosAndPpiTextBoxes)
            {
                panelShowTechosAndPpiTextBoxes.Visible = isVisible;
            }

            // Controlar visibilidad de columnas en el GridView dgvFormulacion
            // En vista admin (isVisible == true) mostramos PPI y Techos; en usuario normal mostramos solo Techos.
            if (content.FindControl("dgvFormulacion") is GridView gv)
            {
                var ppiColumn = gv.Columns.OfType<DataControlField>().FirstOrDefault(c => string.Equals(c.HeaderText, "PPI", StringComparison.OrdinalIgnoreCase));
                var techosColumn = gv.Columns.OfType<DataControlField>().FirstOrDefault(c => string.Equals(c.HeaderText, "Techos 2026", StringComparison.OrdinalIgnoreCase));

                if (ppiColumn != null)
                    ppiColumn.Visible = isVisible; // Admin ve PPI, User no

                if (techosColumn != null)
                    techosColumn.Visible = true; // Visible para ambos

            }
        }

        protected void ShowOrHideUserControlsByPlanningOrFormulationStatus()
        {
            bool isPlanningOpen = ABMPlaniNegocio.GetIsPlanningOpen();
            bool isFormulationOpen = ABMPlaniNegocio.GetIsFormulationOpen();

            // Use the common method with the dynamic values
            ConfigureUserControls(isPlanningOpen, isFormulationOpen);
        }

        /// <summary>
        /// Oculta todos los controles de usuario, independientemente del estado de planificación o formulación
        /// </summary>
        protected void HideAllUserControls()
        {
            // Use the common method with all values set to false
            ConfigureUserControls(false, false);
        }

        /// <summary>
        /// Configura la visibilidad de los controles de usuario según los parámetros especificados
        /// </summary>
        /// <param name="showPlanningControls">Si es true, muestra los controles de planificación</param>
        /// <param name="showFormulationControls">Si es true, muestra los controles de formulación</param>
        private void ConfigureUserControls(bool showPlanningControls, bool showFormulationControls)
        {
            var content = ContentPlaceHolder1;
            if (content == null) return;

            // Configurar columna de acciones en gridviewRegistros
            if (content.FindControl("gridviewRegistros") is GridView grid)
            {
                var actionsColumn = grid.Columns
                    .OfType<DataControlField>()
                    .FirstOrDefault(c => c.HeaderText == "Acciones");

                if (actionsColumn != null)
                {
                    actionsColumn.Visible = showPlanningControls;
                }
            }

            // Configurar columna de acciones en dgvFormulacion
            if (content.FindControl("dgvFormulacion") is GridView formulationGrid)
            {
                var actionsColumn = formulationGrid.Columns
                    .OfType<DataControlField>()
                    .FirstOrDefault(c => c.HeaderText == "Acciones");

                if (actionsColumn != null)
                {
                    actionsColumn.Visible = showFormulationControls;
                }
            }

            // Configurar panel de botón de agregar para planificación
            if (content.FindControl("panelShowAddButton") is Panel panelShowAddButton)
            {
                panelShowAddButton.Visible = showPlanningControls;
            }

            // Configurar panel de botón de agregar para formulación
            if (content.FindControl("panelFormulationShowAddButton") is Panel panelFormulationShowAddButton)
            {
                panelFormulationShowAddButton.Visible = showFormulationControls;
            }
        }


        protected void btnCerrarSesion_Click(object sender, EventArgs e) // codigo duplicado en admin.master.cs
        {
            Session.Clear();
            Context.Request.Cookies.Clear();
            Response.Redirect("Startup.aspx", false);
        }

        protected void chkIsPlanningOpen_ServerChange(object sender, EventArgs e)
        {
            Negocio.ABMPlaniNegocio.SetIsPlanningOpen(chkIsPlanningOpen.Checked);
        }

        protected void chkIsFormulationOpen_ServerChange(object sender, EventArgs e)
        {
            ABMPlaniNegocio.SetIsFormulationOpen(chkIsFormulationOpen.Checked);
        }
        /// <summary>
        /// Obtiene el nombre de la página actual sin la extensión .aspx
        /// </summary>
        /// <returns>Nombre de la página actual</returns>
        public string GetCurrentPageName()
        {
            string pageName = Path.GetFileNameWithoutExtension(Request.Path);

            // Convertir algunos nombres técnicos a nombres más amigables
            switch (pageName.ToLower())
            {
                case "formulacionef":
                    return "Formulación 2026";
                case "proyectosef":
                    return "Proyectos";
                case "movimientosgestionef":
                    return "Movimientos";
                case "lineasgestionef":
                    return "Líneas de Gestión";
                case "lineasgestionffef":
                    return "Líneas de Gestión con FF";
                case "autorizantesef":
                    return "Autorizantes";
                case "certificadosef":
                    return "Certificados";
                case "legitimosef":
                    return "Legítimos Abonos";
                case "redeterminacionesef":
                    return "Redeterminaciones";
                case "obrasef":
                    return "Obras";
                default:
                    return pageName;
            }
        }
    }
}