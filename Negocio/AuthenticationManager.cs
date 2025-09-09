using Dominio;
using System;
using System.Data.Entity;

namespace Negocio
{
    public class AuthenticationManager
    {
        private readonly DbContext _context;

        public AuthenticationManager(DbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // AD primero, si devuelve null intenta por usuario y password.
        public UsuarioEF Authenticate(string username, string password, string domain = null)
        {
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("CUIL es requerido.", nameof(username));
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Password es requerido.",  nameof(password));

            var ad = new ActiveDirectoryAuthenticator(_context, domain);
            var userFromAd = ad.Authenticate(username, password);
            if (userFromAd != null) return userFromAd;

            var local = new LocalAuthenticator(_context);
            return local.Authenticate(username, password);
        }

    }
}
