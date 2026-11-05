using System.Security.Claims;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Negocio;
using Negocio.Infraestructura;

namespace IVC.Tests
{
    /// <summary>
    /// Los campos inline de las grillas de planificación (expediente, monto, ejecución física, estado)
    /// los edita cualquier usuario autenticado: no dependen del estado de la planificación ni del área.
    /// </summary>
    [TestClass]
    [DoNotParallelize] // registra un contexto web global al proceso
    public class UserHelperEdicionInlineTests
    {
        [TestCleanup]
        public void Limpiar() => ContextoWeb.Registrar(null);

        [TestMethod]
        public void SinContextoWeb_NoPuedeEditarInline()
        {
            ContextoWeb.Registrar(null);

            Assert.IsFalse(UserHelper.PuedeEditarInline());
        }

        [TestMethod]
        public void SinPrincipalAutenticado_NoPuedeEditarInline()
        {
            new ContextoWebFalso { Usuario = new ClaimsPrincipal(new ClaimsIdentity()) }.Registrar();

            Assert.IsFalse(UserHelper.PuedeEditarInline());
        }

        [TestMethod]
        public void UsuarioNormalAutenticado_PuedeEditarInline()
        {
            // Sin rol Admin y sin consultar la planificación: el campo inline no depende de ella.
            new ContextoWebFalso { Usuario = ContextoWebFalso.Principal("Ana") }.Registrar();

            Assert.IsTrue(UserHelper.PuedeEditarInline());
        }

        [TestMethod]
        public void Administrador_PuedeEditarInline()
        {
            new ContextoWebFalso { Usuario = ContextoWebFalso.Principal("Ana", "Admin") }.Registrar();

            Assert.IsTrue(UserHelper.PuedeEditarInline());
        }
    }
}
