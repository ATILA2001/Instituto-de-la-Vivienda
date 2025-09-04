using System;
using System.Web;
using System.Web.UI;

namespace WebForms
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            // Configurar el modo de validación no intrusiva
            ValidationSettings.UnobtrusiveValidationMode = UnobtrusiveValidationMode.WebForms;

            // Agregar el mapping de jQuery
            ScriptManager.ScriptResourceMapping.AddDefinition("jquery",
                new ScriptResourceDefinition
                {
                    Path = "~/Scripts/jquery-3.6.0.min.js",
                    DebugPath = "~/Scripts/jquery-3.6.0.js",
                    CdnPath = "https://code.jquery.com/jquery-3.6.0.min.js",
                    CdnDebugPath = "https://code.jquery.com/jquery-3.6.0.js"
                });
        }
    }
}