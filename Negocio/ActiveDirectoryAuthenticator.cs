using Dominio;
using System;
using System.Data.Entity;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Security.Principal;

namespace Negocio
{
    public class ActiveDirectoryAuthenticator
    {
        private readonly DbContext _context;
        private readonly string _domain;

        public ActiveDirectoryAuthenticator(DbContext context, string domain = null)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _domain = domain;
        }

        // Devuelve UsuarioEF si AD valida y el usuario existe en BD; null si credenciales inválidas o usuario no existe.
        public UsuarioEF Authenticate(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username)) return null;
            if (password == null) return null;

            try
            {
                using (var principalContext = string.IsNullOrWhiteSpace(_domain)
                    ? new PrincipalContext(ContextType.Domain)
                    : new PrincipalContext(ContextType.Domain, _domain))
                {
                    bool isValidCredentials;
                    try
                    {
                       isValidCredentials = principalContext.ValidateCredentials(username, password);
                    }
                    catch (PrincipalServerDownException)
                    {
                        // AD inaccesible -> tratar como no autenticado
                        return null;
                    }

                    UserPrincipal user = UserPrincipal.FindByIdentity(principalContext, username);

                    if (!isValidCredentials) return null;

                    return new UsuarioNegocioEF(_context).GetByEmailOrCUIL(user.EmailAddress);
                }
            }
            catch
            {
                // No propagar errores; Manager decide siguiente paso
                return null;
            }
        }

    }
}
