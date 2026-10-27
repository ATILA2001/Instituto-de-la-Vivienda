using System;
using System.Security.Claims;

namespace Negocio.Infraestructura
{
    /// <summary>
    /// Lo único que la capa de negocio necesita del request web: la sesión del usuario, su principal
    /// autenticado y la capacidad de enviarle un archivo. Permite que Negocio no dependa de System.Web
    /// y que las reglas que usan sesión o claims se prueben con un contexto falso.
    /// </summary>
    public interface IContextoWeb
    {
        /// <summary>True si existe un estado de sesión para este request.</summary>
        bool SesionDisponible { get; }

        object ObtenerDeSesion(string clave);

        void GuardarEnSesion(string clave, object valor);

        void QuitarDeSesion(string clave);

        /// <summary>Elimina todas las entradas de la sesión (no la abandona).</summary>
        void LimpiarSesion();

        /// <summary>Principal del request; null si no hay request o no es un ClaimsPrincipal.</summary>
        ClaimsPrincipal Usuario { get; }

        /// <summary>Envía un archivo como descarga y termina la respuesta.</summary>
        void EnviarArchivo(byte[] contenido, string nombreArchivo, string contentType);
    }

    /// <summary>
    /// Punto de acceso al contexto web actual. La aplicación web registra el proveedor al iniciar
    /// (Global.Application_Start); fuera de un request, o sin registro, Actual es null y el código de
    /// negocio debe comportarse como "sin sesión".
    /// </summary>
    public static class ContextoWeb
    {
        private static Func<IContextoWeb> _proveedor;

        public static void Registrar(Func<IContextoWeb> proveedor)
        {
            _proveedor = proveedor;
        }

        public static IContextoWeb Actual => _proveedor?.Invoke();
    }
}
