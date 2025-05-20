using Dominio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negocio
{
    /// <summary>
    /// Clase estática para almacenar y compartir datos entre diferentes servicios
    /// </summary>
    public static class DatosCompartidosHelper
    {
        // Lista de certificados (incluyendo redeterminaciones) en memoria
        private static List<Certificado> _certificadosEnMemoria = new List<Certificado>();

        // Lista de legítimos abonos en memoria
        private static List<Legitimo> _legitimosEnMemoria = new List<Legitimo>();

        // Indica si los certificados ya fueron cargados
        private static bool _certificadosCargados = false;

        // Indica si los legítimos ya fueron cargados
        private static bool _legitimosCargados = false;

        /// <summary>
        /// Establece la lista de certificados compartidos
        /// </summary>
        public static void SetCertificados(List<Certificado> certificados)
        {
            _certificadosEnMemoria = certificados ?? new List<Certificado>();
            _certificadosCargados = true;
        }

        /// <summary>
        /// Establece la lista de legítimos abonos compartidos
        /// </summary>
        public static void SetLegitimos(List<Legitimo> legitimos)
        {
            _legitimosEnMemoria = legitimos ?? new List<Legitimo>();
            _legitimosCargados = true;
        }

        /// <summary>
        /// Obtiene los certificados compartidos
        /// </summary>
        public static List<Certificado> GetCertificados()
        {
            return _certificadosEnMemoria;
        }

        /// <summary>
        /// Obtiene los legítimos abonos compartidos
        /// </summary>
        public static List<Legitimo> GetLegitimos()
        {
            return _legitimosEnMemoria;
        }

        /// <summary>
        /// Indica si los certificados están cargados
        /// </summary>
        public static bool CertificadosCargados => _certificadosCargados;

        /// <summary>
        /// Indica si los legítimos están cargados
        /// </summary>
        public static bool LegitimosCargados => _legitimosCargados;

        /// <summary>
        /// Limpia todos los datos compartidos
        /// </summary>
        public static void LimpiarDatos()
        {
            _certificadosEnMemoria.Clear();
            _legitimosEnMemoria.Clear();
            _certificadosCargados = false;
            _legitimosCargados = false;
        }
    }


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
        /// 
        /// <summary>
        /// Calcula el SIGAF considerando certificados y legítimos abonos de memoria
        /// </summary>
        public static decimal? CalcularSIGAFCompartido(string expediente, decimal montoActual, bool esCertificado)
        {
            if (string.IsNullOrEmpty(expediente))
                return null;

            try
            {
                // Obtener total de devengados para el expediente
                decimal totalImporteDevengados = ObtenerTotalImporteDevengados(expediente);

                if (totalImporteDevengados <= 0)
                    return null;

                // Listas para almacenar todos los montos
                List<decimal> montosTotal = new List<decimal>();

                // Agregar montos de los certificados si están disponibles
                if (DatosCompartidosHelper.CertificadosCargados)
                {
                    var certificadosMismoExpediente = DatosCompartidosHelper.GetCertificados()
                        .Where(c => c.ExpedientePago == expediente)
                        .ToList();

                    montosTotal.AddRange(certificadosMismoExpediente.Select(c => c.MontoTotal));
                }

                // Agregar montos de los legítimos si están disponibles
                if (DatosCompartidosHelper.LegitimosCargados)
                {
                    var legitimosMismoExpediente = DatosCompartidosHelper.GetLegitimos()
                        .Where(l => l.Expediente == expediente && l.Certificado.HasValue)
                        .ToList();

                    montosTotal.AddRange(legitimosMismoExpediente.Select(l => l.Certificado.Value));
                }

                // Si no hay montos en memoria, consultar la base de datos
                if (montosTotal.Count == 0)
                {
                    return CalcularSIGAF(expediente, montoActual, new List<decimal> { montoActual });
                }

                // Si solo hay un monto (el actual), asignar todo el importe
                if (montosTotal.Count == 1)
                    return totalImporteDevengados;

                // Calcular la suma de todos los montos
                decimal sumaTotalMontos = montosTotal.Sum();

                // Calcular la proporción
                if (sumaTotalMontos > 0)
                    return totalImporteDevengados * montoActual / sumaTotalMontos;

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al calcular SIGAF compartido: {ex.Message}");
                return null;
            }
        }
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
        public static decimal ObtenerTotalImporteDevengados(string expediente)
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