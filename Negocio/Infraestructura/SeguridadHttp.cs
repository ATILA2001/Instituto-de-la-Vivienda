using System.Linq;

namespace Negocio.Infraestructura
{
    /// <summary>
    /// Reglas puras (sin System.Web) que usa la capa web para validar entradas HTTP.
    /// </summary>
    public static class SeguridadHttp
    {
        /// <summary>
        /// Un returnUrl con caracteres de control o espacios internos (TAB, LF, CR, NUL...) no se acepta.
        /// Los navegadores descartan esos caracteres al parsear la URL de un Location, así que
        /// "/&lt;TAB&gt;/evil.com" se convierte en "//evil.com" y sale del host. Auth.Web aplica la misma regla.
        /// </summary>
        public static bool TieneCaracteresInvalidosParaUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) return false;
            return url.Any(c => char.IsControl(c) || char.IsWhiteSpace(c));
        }
    }
}
