using Dominio;
using System;
using System.Data.Entity;
using System.Linq;

namespace Negocio
{
    public class LocalAuthenticator
    {
        private readonly DbContext _context;
        private readonly UsuarioNegocioEF _usuarioNegocio;

        public LocalAuthenticator(DbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _usuarioNegocio = new UsuarioNegocioEF(_context);
        }

        // Devuelve UsuarioEF si credenciales locales son válidas; null caso contrario.
        public UsuarioEF Authenticate(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username)) return null;
            if (password == null) return null;

            UsuarioEF usuario = null;
            try
            {
                usuario = _usuarioNegocio.GetByEmailOrCUIL(username);
            }
            catch
            {
                // en caso de error en consulta, devolver null
                return null;
            }

            if (usuario == null) return null;


            try
            {
                var stored = usuario.Contrasenia;
                if (!string.IsNullOrEmpty(stored) && stored == password)
                {
                    _context.Entry(usuario).Reference(u => u.Area).Load();
                    return usuario;
                }
            }
            catch
            {
                // Si falla la lectura, no autenticar
                return null;
            }

            return null;
        }
    }
}
