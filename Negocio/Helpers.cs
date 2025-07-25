﻿using Dominio;
using Dominio.DTO;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI.WebControls;

namespace Negocio
{
    /// <summary>
    /// Clase helper para exportar datos a Excel (.xlsx)
    /// </summary>
    public static class ExcelHelper
    {
        /// <summary>
        /// Exporta datos de cualquier tipo a Excel, manejando propiedades anidadas
        /// </summary>
        public static void ExportarDatosGenericos<T>(GridView gridView, IEnumerable<T> datos,
            Dictionary<string, string> mapeoColumnas, string fileName)
        {
            if (gridView == null)
                throw new ArgumentException("El GridView es nulo");

            if (datos == null || !datos.Any())
                throw new ArgumentException("No hay datos para exportar");

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Datos");
                int fila = 1;
                int columnaActual = 1;

                // Normalizar mapeo para facilitar comparaciones
                Dictionary<string, string> mapeoNormalizado = new Dictionary<string, string>();
                foreach (var kvp in mapeoColumnas)
                {
                    // Convertir claves a minúsculas y sin acentos para comparación más flexible
                    string claveNormalizada = NormalizarTexto(kvp.Key);
                    mapeoNormalizado[claveNormalizada] = kvp.Value;
                }

                // Depuración: imprimir todas las columnas del GridView
                System.Diagnostics.Debug.WriteLine("Columnas en GridView:");
                foreach (DataControlField column in gridView.Columns)
                {
                    System.Diagnostics.Debug.WriteLine($"- HeaderText: '{column.HeaderText}' | Tipo: {column.GetType().Name} | Visible: {column.Visible}");
                }

                // Depuración: imprimir mapeo normalizado
                System.Diagnostics.Debug.WriteLine("Mapeo normalizado:");
                foreach (var kvp in mapeoNormalizado)
                {
                    System.Diagnostics.Debug.WriteLine($"- Clave: '{kvp.Key}' | Ruta: '{kvp.Value}'");
                }

                // Agregar encabezados desde el GridView
                foreach (DataControlField column in gridView.Columns)
                {
                    if (column.Visible && !(column is ButtonField) && !(column is CommandField) &&
                        column.HeaderText != "Acciones")
                    {
                        worksheet.Cells[fila, columnaActual].Value = column.HeaderText;
                        FormatearEncabezado(worksheet.Cells[fila, columnaActual]);
                        columnaActual++;
                    }
                }

                // Agregar datos
                fila++;
                foreach (var item in datos)
                {
                    columnaActual = 1;
                    foreach (DataControlField column in gridView.Columns)
                    {
                        if (column.Visible && !(column is ButtonField) && !(column is CommandField) &&
                            column.HeaderText != "Acciones")
                        {
                            // Obtener expresión de propiedad para esta columna
                            string headerText = column.HeaderText;
                            string headerTextNormalizado = NormalizarTexto(headerText);

                            object valor = null;
                            string rutaPropiedad = null;

                            // Si hay mapeo específico para esta columna, usarlo
                            if (mapeoNormalizado.TryGetValue(headerTextNormalizado, out rutaPropiedad))
                            {
                                valor = ObtenerValorPorRuta(item, rutaPropiedad);
                                System.Diagnostics.Debug.WriteLine($"Columna: '{headerText}' (norm: '{headerTextNormalizado}') → Ruta: '{rutaPropiedad}' → Valor: {valor}");
                            }
                            else
                            {
                                // Intentar con BoundField
                                if (column is BoundField boundField && !string.IsNullOrEmpty(boundField.DataField))
                                {
                                    valor = ObtenerValorPorRuta(item, boundField.DataField);
                                    System.Diagnostics.Debug.WriteLine($"Columna: '{headerText}' (BoundField) → DataField: '{boundField.DataField}' → Valor: {valor}");
                                }
                                else
                                {
                                    System.Diagnostics.Debug.WriteLine($"Columna: '{headerText}' → No se encontró mapeo");
                                }
                            }

                            // Formatear según tipo de dato
                            if (valor is decimal || valor is double || valor is float)
                            {
                                worksheet.Cells[fila, columnaActual].Value = Convert.ToDouble(valor);
                                worksheet.Cells[fila, columnaActual].Style.Numberformat.Format = "#,##0.00";
                            }
                            else if (valor is DateTime)
                            {
                                worksheet.Cells[fila, columnaActual].Value = (DateTime)valor;
                                worksheet.Cells[fila, columnaActual].Style.Numberformat.Format = "dd-MM-yyyy";
                            }
                            else
                            {
                                worksheet.Cells[fila, columnaActual].Value = valor?.ToString();
                            }

                            columnaActual++;
                        }
                    }
                    fila++;
                }

                AjustarAnchosColumnas(worksheet, columnaActual - 1);
                DescargarArchivo(package, fileName);
            }
        }

        /// <summary>
        /// Normaliza un texto para comparaciones consistentes: minúsculas y sin acentos
        /// </summary>
        private static string NormalizarTexto(string texto)
        {
            if (string.IsNullOrEmpty(texto))
                return string.Empty;

            // Convertir a minúsculas
            texto = texto.ToLowerInvariant();

            // Eliminar acentos
            string normalizedString = texto.Normalize(NormalizationForm.FormD);
            StringBuilder stringBuilder = new StringBuilder();

            foreach (char c in normalizedString)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    stringBuilder.Append(c);
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        /// <summary>
        /// Obtiene el valor de una propiedad anidada usando una ruta de acceso
        /// Ejemplo: "Autorizante.Obra.Area.Nombre"
        /// </summary>
        /// <param name="obj">Objeto base</param>
        /// <param name="rutaPropiedad">Ruta de propiedades separadas por puntos</param>
        /// <returns>Valor de la propiedad o null si no se encuentra</returns>
        private static object ObtenerValorPorRuta(object obj, string rutaPropiedad)
        {
            if (obj == null || string.IsNullOrEmpty(rutaPropiedad))
                return null;

            string[] propiedades = rutaPropiedad.Split('.');
            object resultado = obj;

            // Navegar por cada nivel de propiedad
            foreach (string nombrePropiedad in propiedades)
            {
                if (resultado == null)
                    return null;

                Type tipo = resultado.GetType();
                var propiedad = tipo.GetProperty(nombrePropiedad);

                if (propiedad == null)
                    return null;

                // Obtener el valor de esta propiedad y continuar
                resultado = propiedad.GetValue(resultado);
            }

            return resultado;
        }
        /// <summary>
        /// Formatea una celda como encabezado
        /// </summary>
        private static void FormatearEncabezado(ExcelRange cell)
        {
            cell.Style.Font.Bold = true;
            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
        }

        /// <summary>
        /// Ajusta el ancho de las columnas en función del contenido
        /// </summary>
        private static void AjustarAnchosColumnas(ExcelWorksheet worksheet, int columnCount)
        {
            for (int i = 1; i <= columnCount; i++)
            {
                worksheet.Column(i).AutoFit();
            }
        }

        /// <summary>
        /// Descarga el archivo Excel generado
        /// </summary>
        private static void DescargarArchivo(ExcelPackage package, string fileName)
        {
            HttpContext context = HttpContext.Current;
            if (context == null)
                throw new InvalidOperationException("Esta función solo puede usarse en un contexto web.");

            context.Response.Clear();
            context.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            context.Response.AddHeader("content-disposition", $"attachment;  filename={fileName}.xlsx");
            context.Response.BinaryWrite(package.GetAsByteArray());
            context.Response.End();
        }
    }

    /// <summary>
    /// Clase estática para almacenar y compartir datos entre diferentes servicios
    /// </summary>
    public static class DatosCompartidosHelper
    {
        // Lista de certificados (incluyendo redeterminaciones) en memoria
        private static List<CertificadoDTO> _certificadosEnMemoria = new List<CertificadoDTO>();

        // Lista de legítimos abonos en memoria
        private static List<Legitimo> _legitimosEnMemoria = new List<Legitimo>();

        // Indica si los certificados ya fueron cargados
        private static bool _certificadosCargados = false;

        // Indica si los legítimos ya fueron cargados
        private static bool _legitimosCargados = false;

        /// <summary>
        /// Establece la lista de certificados compartidos
        /// </summary>
        public static void SetCertificados(List<CertificadoDTO> certificados)
        {
            _certificadosEnMemoria = certificados ?? new List<CertificadoDTO>();
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
        public static List<CertificadoDTO> GetCertificados()
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
        public static decimal? CalcularSIGAFCompartido(string expediente, decimal montoActual)
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