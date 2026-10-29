using System.Security.Claims;
using System.Web;
using Negocio.Infraestructura;

namespace WebForms.Infraestructura
{
    /// <summary>
    /// Implementación de IContextoWeb sobre HttpContext. Se registra en Global.Application_Start.
    /// </summary>
    public sealed class HttpContextoWeb : IContextoWeb
    {
        private readonly HttpContext _context;

        private HttpContextoWeb(HttpContext context)
        {
            _context = context;
        }

        /// <summary>Contexto del request actual, o null fuera de un request (por ejemplo, dentro de Task.Run).</summary>
        public static IContextoWeb DelRequestActual()
        {
            var context = HttpContext.Current;
            return context == null ? null : new HttpContextoWeb(context);
        }

        public bool SesionDisponible => _context.Session != null;

        public object ObtenerDeSesion(string clave) => _context.Session?[clave];

        public void GuardarEnSesion(string clave, object valor)
        {
            if (_context.Session != null)
                _context.Session[clave] = valor;
        }

        public void QuitarDeSesion(string clave) => _context.Session?.Remove(clave);

        public void LimpiarSesion() => _context.Session?.Clear();

        public ClaimsPrincipal Usuario => _context.User as ClaimsPrincipal;
    }
}
