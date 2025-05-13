using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negocio
{
    /// <summary>
    /// Clase helper para cálculos relacionados con SIGAF
    /// </summary>
    public static class SIGAFHelper
    {
        /// <summary>
        /// Calcula el valor SIGAF para un expediente y monto específicos
        /// </summary>
        /// <param name="expediente">Número de expediente</param>
        /// <param name="montoActual">Monto del certificado actual</param>
        /// <param name="todosLosMontos">Lista de todos los montos asociados al mismo expediente</param>
        /// <returns>Valor SIGAF calculado o null si no es posible calcularlo</returns>
        public static decimal? CalcularSIGAF(string expediente, decimal montoActual, List<decimal> todosLosMontos)
        {
            if (string.IsNullOrEmpty(expediente))
                return null;

            try
            {
                // Obtener importe total de devengados para este expediente
                decimal totalImporteDevengados = ObtenerTotalImporteDevengados(expediente);

                // Si no hay importe devengado, no hay SIGAF
                if (totalImporteDevengados <= 0)
                    return null;

                // Si solo hay un monto, asignar todo el importe
                if (todosLosMontos.Count == 1)
                    return totalImporteDevengados;

                // Calcular la suma total de los montos
                decimal montoTotalCertificados = todosLosMontos.Sum();

                // Si hay más de un monto y la suma total es mayor que cero, calcular la proporción
                if (montoTotalCertificados > 0)
                    return totalImporteDevengados * montoActual / montoTotalCertificados;

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al calcular SIGAF: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Método para obtener el total de importes devengados para un expediente
        /// </summary>
        /// <param name="expediente">Número de expediente</param>
        /// <returns>Total de importes devengados</returns>
        private static decimal ObtenerTotalImporteDevengados(string expediente)
        {
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("SELECT SUM(IMPORTE_PP) AS TotalImporte FROM DEVENGADOS WHERE EE_FINANCIERA = @EXPEDIENTE");
                datos.agregarParametro("@EXPEDIENTE", expediente);
                datos.ejecutarLectura();

                decimal totalImporte = 0;
                if (datos.Lector.Read() && datos.Lector["TotalImporte"] != DBNull.Value)
                {
                    totalImporte = Convert.ToDecimal(datos.Lector["TotalImporte"]);
                }

                return totalImporte;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        /// <summary>
        /// Determina el estado de un expediente basado en si existe y si tiene registros en DEVENGADOS
        /// </summary>
        /// <param name="expediente">Número de expediente</param>
        /// <returns>Estado del expediente (NO INICIADO, EN TRAMITE, DEVENGADO)</returns>
        public static string DeterminarEstadoExpediente(string expediente)
        {
            if (string.IsNullOrWhiteSpace(expediente))
                return "NO INICIADO";

            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("SELECT 1 FROM DEVENGADOS WHERE EE_FINANCIERA = @EXPEDIENTE");
                datos.agregarParametro("@EXPEDIENTE", expediente);
                datos.ejecutarLectura();

                if (datos.Lector.Read())
                    return "DEVENGADO";
                else
                    return "EN TRAMITE";
            }
            finally
            {
                datos.cerrarConexion();
            }
        }
    }

    /// <summary>
    /// Clase helper para información relacionada con SADE (Sistema de Administración de Documentos Electrónicos)
    /// </summary>
    public static class SADEHelper
    {
        /// <summary>
        /// Obtiene la información de SADE para un expediente específico
        /// </summary>
        /// <param name="expediente">Número de expediente</param>
        /// <returns>Tupla con fecha del último pase y buzón destino</returns>
        public static (DateTime? FechaUltimoPase, string BuzonDestino) ObtenerInfoSADE(string expediente)
        {
            if (string.IsNullOrEmpty(expediente))
                return (null, null);

            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("SELECT [BUZON DESTINO], [FECHA ULTIMO PASE] FROM PASES_SADE WHERE EXPEDIENTE = @EXPEDIENTE COLLATE Modern_Spanish_CI_AS");
                datos.agregarParametro("@EXPEDIENTE", expediente);
                datos.ejecutarLectura();

                if (datos.Lector.Read())
                {
                    DateTime? fechaUltimoPase = datos.Lector["FECHA ULTIMO PASE"] != DBNull.Value
                        ? (DateTime?)Convert.ToDateTime(datos.Lector["FECHA ULTIMO PASE"])
                        : null;

                    string buzonDestino = datos.Lector["BUZON DESTINO"]?.ToString();

                    return (fechaUltimoPase, buzonDestino);
                }

                return (null, null);
            }
            finally
            {
                datos.cerrarConexion();
            }
        }
    }
}