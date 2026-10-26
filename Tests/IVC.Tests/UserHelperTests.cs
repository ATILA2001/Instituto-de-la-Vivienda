using System.Collections.Generic;
using Dominio;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Negocio;

namespace IVC.Tests
{
    [TestClass]
    public class UserHelperTests
    {
        [TestMethod]
        public void AreaIdsParaFiltro_UsuarioNull_DevuelveListaVacia()
        {
            var ids = UserHelper.AreaIdsParaFiltro(null);

            Assert.IsNotNull(ids);
            Assert.AreEqual(0, ids.Count);
        }

        [TestMethod]
        public void AreaIdsParaFiltro_SinAreasResueltas_DevuelveListaVacia()
        {
            var usuario = new UsuarioEF { Tipo = false, IvcAreaIds = null };

            var ids = UserHelper.AreaIdsParaFiltro(usuario);

            Assert.IsNotNull(ids);
            Assert.AreEqual(0, ids.Count, "Un usuario sin áreas debe filtrar con lista vacía (fail-closed), nunca con null");
        }

        [TestMethod]
        public void AreaIdsParaFiltro_ConAreas_DevuelveLasMismas()
        {
            var usuario = new UsuarioEF { Tipo = false, IvcAreaIds = new List<int> { 16, 19 } };

            var ids = UserHelper.AreaIdsParaFiltro(usuario);

            CollectionAssert.AreEqual(new List<int> { 16, 19 }, ids);
        }
    }
}
