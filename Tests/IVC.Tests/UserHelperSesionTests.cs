using System.Security.Claims;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Negocio;
using Negocio.Infraestructura;

namespace IVC.Tests
{
    /// <summary>
    /// Caché del usuario en sesión: se reutiliza sólo para la misma identidad efectiva (persona, roles, áreas,
    /// versión de permisos) y cualquier cambio descarta toda la sesión.
    /// </summary>
    [TestClass]
    public class UserHelperSesionTests
    {
        [TestCleanup]
        public void Limpiar() => ContextoWeb.Registrar(null);

        [TestMethod]
        public void SinContextoWeb_DevuelveNull()
        {
            ContextoWeb.Registrar(null);

            Assert.IsNull(UserHelper.EnsureSessionUserFromClaims());
        }

        [TestMethod]
        public void SinPrincipalAutenticado_DevuelveNullYLimpiaLaSesionSiHabiaUsuario()
        {
            var ctx = new ContextoWebFalso { Usuario = new ClaimsPrincipal(new ClaimsIdentity()) }.Registrar();
            ctx.Sesion["Usuario"] = new Dominio.UsuarioEF();
            ctx.Sesion["GridDataAutorizantes"] = new object();

            var usuario = UserHelper.EnsureSessionUserFromClaims();

            Assert.IsNull(usuario);
            Assert.AreEqual(0, ctx.Sesion.Count);
            Assert.AreEqual(1, ctx.VecesLimpiada);
        }

        [TestMethod]
        public void PrimeraLlamada_ConstruyeYCacheaElUsuario()
        {
            var ctx = new ContextoWebFalso { Usuario = ContextoWebFalso.Principal("Ana", "Admin") }.Registrar();

            var usuario = UserHelper.EnsureSessionUserFromClaims();

            Assert.IsNotNull(usuario);
            Assert.AreEqual("Ana", usuario.Nombre);
            Assert.IsTrue(usuario.Tipo, "el rol Admin debe marcar al usuario como administrador");
            Assert.AreSame(usuario, ctx.Sesion["Usuario"]);
        }

        [TestMethod]
        public void MismaIdentidad_ReutilizaElUsuarioCacheado()
        {
            var ctx = new ContextoWebFalso { Usuario = ContextoWebFalso.Principal("Ana", "Admin") }.Registrar();
            var primero = UserHelper.EnsureSessionUserFromClaims();
            ctx.Sesion["GridDataAutorizantes"] = new object();

            var segundo = UserHelper.EnsureSessionUserFromClaims();

            Assert.AreSame(primero, segundo);
            Assert.IsTrue(ctx.Sesion.ContainsKey("GridDataAutorizantes"), "la sesión no debe limpiarse");
            Assert.AreEqual(0, ctx.VecesLimpiada);
        }

        [TestMethod]
        public void CambioDeRoles_DescartaElUsuarioYLimpiaLaSesion()
        {
            var ctx = new ContextoWebFalso { Usuario = ContextoWebFalso.Principal("Ana", "Admin") }.Registrar();
            var admin = UserHelper.EnsureSessionUserFromClaims();
            ctx.Sesion["CertificadosCompleto"] = new object();

            ctx.Usuario = ContextoWebFalso.Principal("Ana"); // mismo nombre, sin rol Admin
            var normal = UserHelper.EnsureSessionUserFromClaims();

            Assert.AreNotSame(admin, normal);
            Assert.IsFalse(normal.Tipo);
            Assert.IsFalse(ctx.Sesion.ContainsKey("CertificadosCompleto"), "las listas cacheadas eran de la identidad anterior");
            Assert.AreEqual(1, ctx.VecesLimpiada);
        }

        [TestMethod]
        public void OtraPersona_DescartaElUsuarioYLimpiaLaSesion()
        {
            var ctx = new ContextoWebFalso { Usuario = ContextoWebFalso.Principal("Ana") }.Registrar();
            UserHelper.EnsureSessionUserFromClaims();

            ctx.Usuario = ContextoWebFalso.Principal("Bruno");
            var usuario = UserHelper.EnsureSessionUserFromClaims();

            Assert.AreEqual("Bruno", usuario.Nombre);
            Assert.AreEqual(1, ctx.VecesLimpiada);
        }

        [TestMethod]
        public void SinSesionDisponible_DevuelveNull()
        {
            new ContextoWebFalso { Usuario = ContextoWebFalso.Principal("Ana"), SesionDisponible = false }.Registrar();

            Assert.IsNull(UserHelper.EnsureSessionUserFromClaims());
        }
    }
}
