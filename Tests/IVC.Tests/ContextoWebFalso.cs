using System.Collections.Generic;
using System.Security.Claims;
using Negocio.Infraestructura;

namespace IVC.Tests
{
    /// <summary>Contexto web en memoria para probar código de Negocio que usa sesión y claims.</summary>
    public sealed class ContextoWebFalso : IContextoWeb
    {
        public Dictionary<string, object> Sesion { get; } = new Dictionary<string, object>();
        public bool SesionDisponible { get; set; } = true;
        public ClaimsPrincipal Usuario { get; set; }
        public int VecesLimpiada { get; private set; }
        public string UltimoArchivoEnviado { get; private set; }

        public object ObtenerDeSesion(string clave) => Sesion.TryGetValue(clave, out var v) ? v : null;

        public void GuardarEnSesion(string clave, object valor) => Sesion[clave] = valor;

        public void QuitarDeSesion(string clave) => Sesion.Remove(clave);

        public void LimpiarSesion()
        {
            Sesion.Clear();
            VecesLimpiada++;
        }

        public void EnviarArchivo(byte[] contenido, string nombreArchivo, string contentType) => UltimoArchivoEnviado = nombreArchivo;

        /// <summary>Registra este contexto como el actual para el código de Negocio.</summary>
        public ContextoWebFalso Registrar()
        {
            ContextoWeb.Registrar(() => this);
            return this;
        }

        /// <summary>
        /// Principal autenticado sin NameIdentifier ni claim "area": UserHelper no consulta la base para resolverlo.
        /// </summary>
        public static ClaimsPrincipal Principal(string nombre, params string[] roles)
        {
            var claims = new List<Claim> { new Claim(ClaimTypes.Name, nombre) };
            foreach (var rol in roles)
                claims.Add(new Claim(ClaimTypes.Role, rol));
            return new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
        }
    }
}
