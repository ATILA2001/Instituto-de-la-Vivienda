using Microsoft.VisualStudio.TestTools.UnitTesting;
using Negocio.Redeterminaciones;

namespace IVC.Tests
{
    /// <summary>
    /// Mapeo de etapa de redeterminación (ESTADOS_REDET) a estado general de la grilla de autorizantes.
    /// Ids según Database/vw_PowerBI_Redeterminaciones.sql: desestimado {35,36}, no iniciado {37,38},
    /// aprobado {12,22,33,39}, el resto en trámite.
    /// </summary>
    [TestClass]
    public class MapeoEstadoRedetTests
    {
        [TestMethod]
        public void EtapaNull_EsNoIniciado()
        {
            var estado = MapeoEstadoRedet.MapearEstadoDesdeId(null);

            Assert.AreEqual(4, estado.Id);
            Assert.AreEqual("NO INICIADO", estado.Nombre);
        }

        [DataTestMethod]
        [DataRow(37, 4, "NO INICIADO")]
        [DataRow(38, 4, "NO INICIADO")]
        [DataRow(12, 1, "APROBADO")]
        [DataRow(22, 1, "APROBADO")]
        [DataRow(33, 1, "APROBADO")]
        [DataRow(39, 1, "APROBADO")]
        [DataRow(35, 3, "DESESTIMADO")]
        [DataRow(36, 3, "DESESTIMADO")]
        [DataRow(1, 2, "EN TRAMITE")]
        [DataRow(99, 2, "EN TRAMITE")]
        public void EtapaConocida_MapeaAlEstadoGeneral(int etapaId, int estadoEsperado, string nombreEsperado)
        {
            var estado = MapeoEstadoRedet.MapearEstadoDesdeId(etapaId);

            Assert.AreEqual(estadoEsperado, estado.Id);
            Assert.AreEqual(nombreEsperado, estado.Nombre);
        }
    }
}
