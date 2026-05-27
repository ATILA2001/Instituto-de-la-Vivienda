using Dominio;
using Negocio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebForms
{
    public partial class Site : System.Web.UI.MasterPage
    {
        public class AppLinkItem
        {
            public string Label { get; set; }
            public string Url { get; set; }
        }

        protected List<AppLinkItem> OtherApps { get; private set; } = new List<AppLinkItem>();
        protected string AdminPanelUrl { get; private set; } = "/admin";

        protected void Page_Init(object sender, EventArgs e)
        {
            // Comprobación básica de usuario logueado
            if (Session["Usuario"] == null)
            {
                UserHelper.EnsureSessionUserFromClaims();
            }

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

            // Build app switcher list from available_app claims
            var principal = HttpContext.Current?.User as ClaimsPrincipal;
            bool isAdmin = currentUser?.Tipo == true;
            if (principal != null)
            {
                var authWebBase = (Environment.GetEnvironmentVariable("AuthWeb__BaseUrl")
                    ?? System.Configuration.ConfigurationManager.AppSettings["AuthWebBaseUrl"]
                    ?? string.Empty).TrimEnd('/');
                AdminPanelUrl = string.IsNullOrWhiteSpace(authWebBase) ? "/admin" : authWebBase + "/admin";
                const string currentClientId = "PlaniLocal";
                var availableAppIds = principal.Claims
                    .Where(c => c.Type == "available_app"
                                && !string.IsNullOrWhiteSpace(c.Value))
                    .Select(c => c.Value)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                OtherApps = availableAppIds
                    .Where(clientId => isAdmin || !string.Equals(clientId, currentClientId, StringComparison.OrdinalIgnoreCase))
                    .Select(clientId => new AppLinkItem
                    {
                        Label = GetAppDisplayName(clientId),
                        Url = authWebBase + "/connect/switch-app?clientId=" + Uri.EscapeDataString(clientId)
                    })
                    .ToList();

                if (phAppSwitcher != null)
                {
                    phAppSwitcher.Visible = isAdmin || availableAppIds.Count > 1;
                }
            }

            if (rptOtherApps != null)
            {
                rptOtherApps.DataSource = OtherApps;
                rptOtherApps.DataBind();
            }

            if (phAppSwitcher != null)
            {
                phAppSwitcher.Visible = isAdmin || phAppSwitcher.Visible;
            }
            if (phAdminPanelDivider != null)
            {
                phAdminPanelDivider.Visible = isAdmin && OtherApps.Count > 0;
            }
            if (phAdminPanelLink != null)
            {
                phAdminPanelLink.Visible = isAdmin;
            }
            if (lnkAdminPanel != null)
            {
                lnkAdminPanel.NavigateUrl = AdminPanelUrl;
            }

            // Obtenemos si el usuario es administrador.
            bool isRedeterminacionesUser = UserHelper.IsUserInArea(IvcAreaIds.Redeterminaciones);
            bool isSecretariaUser = UserHelper.IsUserInArea(IvcAreaIds.Secretaria);


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
            UsuarioEF currentUser = UserHelper.GetFullCurrentUser();
            bool isPlanningOpen = ABMPlaniNegocio.GetIsPlanningOpen() || currentUser.IsPlanningOpenOverride;
            bool isFormulationOpen = ABMPlaniNegocio.GetIsFormulationOpen();
            ConfigureUserControls(isPlanningOpen, isFormulationOpen);
        }

        protected void HideAllUserControls()
        {
            ConfigureUserControls(false, false);
        }

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
                    actionsColumn.Visible = showPlanningControls;
            }

            // Configurar columna de acciones en dgvFormulacion
            if (content.FindControl("dgvFormulacion") is GridView formulationGrid)
            {
                var actionsColumn = formulationGrid.Columns
                    .OfType<DataControlField>()
                    .FirstOrDefault(c => c.HeaderText == "Acciones");

                if (actionsColumn != null)
                    actionsColumn.Visible = showFormulationControls;
            }

            // Configurar panel de botón de agregar
            if (content.FindControl("panelShowAddButton") is Panel panelShowAddButton)
                panelShowAddButton.Visible = showPlanningControls;

            // Configurar panel de botón de agregar para formulación
            if (content.FindControl("panelFormulationShowAddButton") is Panel panelFormulationShowAddButton)
                panelFormulationShowAddButton.Visible = showFormulationControls;
        }


        protected void btnCerrarSesion_Click(object sender, EventArgs e)
        {
            IvcLogoutHelper.SignOutAndRedirect(Context);
        }

        private static string GetAppDisplayName(string clientId)
        {
            switch (clientId)
            {
                case "sai":
                    return "Sistema de Administracion de Inventario";
                case "PlaniLocal":
                    return "Administracion Financiera";
                default:
                    return clientId;
            }
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
