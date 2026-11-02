using System.Web.UI;

namespace WebForms.Infraestructura
{
    /// <summary>
    /// Ata el ViewState y la validación de eventos de cada página a la sesión del usuario.
    /// Sin esto, el MAC del __VIEWSTATE sólo depende de la machineKey: un usuario con cuenta puede copiar el
    /// ViewState de una página y armar un formulario en otro sitio que un administrador envíe sin saberlo
    /// (CSRF). La cookie compartida viaja con SameSite=None, así que el navegador la adjunta.
    /// </summary>
    public static class ProteccionCsrf
    {
        private const string ClaveMarcadorSesion = "__ProteccionCsrf";

        /// <summary>
        /// Llamar en Page_Init de cada master (antes de cualquier Response.End): la propiedad debe fijarse
        /// antes de que la página cargue el ViewState.
        /// </summary>
        public static void Aplicar(Page pagina)
        {
            var sesion = pagina?.Session;
            if (sesion == null) return;

            // Una sesión vacía no persiste: ASP.NET no manda la cookie ASP.NET_SessionId y el SessionID
            // cambia en cada request, con lo que el postback fallaría la validación del ViewState.
            if (sesion.Count == 0)
                sesion[ClaveMarcadorSesion] = true;

            pagina.ViewStateUserKey = sesion.SessionID;
        }
    }
}
