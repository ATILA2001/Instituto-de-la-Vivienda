using Dominio;
using Dominio.Enums;
using Negocio.Negocio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Negocio
{
    public class LegitimoNegocio
    {

        public bool modificar(Legitimo legitimoModificado)
        {
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta(@"
            UPDATE LEGITIMOS_ABONOS 
            SET 
                CODIGO_AUTORIZANTE = @CODIGO,
                EXPEDIENTE = @EXPEDIENTE,
                INICIO_EJECUCION = @INICIO_EJECUCION,
                FIN_EJECUCION = @FIN_EJECUCION,
                CERTIFICADO = @CERTIFICADO,
                MES_APROBACION = @MES_APROBACION
            WHERE 
                ID = @ID");
                datos.agregarParametro("@CODIGO", legitimoModificado.CodigoAutorizante);
                datos.agregarParametro("@EXPEDIENTE", legitimoModificado.Expediente);
                datos.agregarParametro("@INICIO_EJECUCION", legitimoModificado.InicioEjecucion);
                datos.agregarParametro("@FIN_EJECUCION", legitimoModificado.FinEjecucion);
                datos.agregarParametro("@CERTIFICADO", legitimoModificado.Certificado);
                datos.agregarParametro("@MES_APROBACION", legitimoModificado.MesAprobacion);
                datos.agregarParametro("@ID", legitimoModificado.Id);

                datos.ejecutarAccion();

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al modificar el legítimo abono.", ex);
            }
            finally
            {
                datos.cerrarConexion();
            }
        }
        public bool agregar(Legitimo nuevoLegitimo)
        {
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta(@"
            INSERT INTO LEGITIMOS_ABONOS 
            (CODIGO_AUTORIZANTE, OBRA, EXPEDIENTE, INICIO_EJECUCION, FIN_EJECUCION, CERTIFICADO, MES_APROBACION)
            VALUES 
            (@CODIGO_AUTORIZANTE, @OBRA, @EXPEDIENTE, @INICIO_EJECUCION, @FIN_EJECUCION, @CERTIFICADO, @MES_APROBACION)");

                datos.agregarParametro("@CODIGO_AUTORIZANTE", nuevoLegitimo.CodigoAutorizante);
                datos.agregarParametro("@OBRA", nuevoLegitimo.Obra.Id);
                datos.agregarParametro("@EXPEDIENTE", nuevoLegitimo.Expediente);
                datos.agregarParametro("@INICIO_EJECUCION", nuevoLegitimo.InicioEjecucion);
                datos.agregarParametro("@FIN_EJECUCION", nuevoLegitimo.FinEjecucion);
                datos.agregarParametro("@CERTIFICADO", nuevoLegitimo.Certificado);
                datos.agregarParametro("@MES_APROBACION", nuevoLegitimo.MesAprobacion);

                datos.ejecutarAccion();

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al agregar un legítimo.", ex);
            }
            finally
            {
                datos.cerrarConexion();
            }
        }
        public bool eliminar(string codigo)
        {
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("DELETE FROM LEGITIMOS_ABONOS WHERE ID = @ID");

                datos.agregarParametro("@ID", codigo);

                datos.ejecutarAccion();

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al eliminar el legítimo.", ex);
            }
            finally
            {
                datos.cerrarConexion();
            }
        }
        public DataTable listarddl(Usuario usuario)
        {
            DataTable dt = new DataTable();
            AccesoDatos datos = new AccesoDatos();

            try
            {
                // Validación: usuario y su área no pueden ser nulos
                if (usuario == null || usuario.Area == null)
                {
                    throw new ArgumentNullException("El usuario o su área no pueden ser nulos.");
                }

                // Configurar la consulta
                datos.setearConsulta(@"
            SELECT  ROW_NUMBER() OVER (ORDER BY L.CODIGO_AUTORIZANTE) AS ID, L.CODIGO_AUTORIZANTE FROM LEGITIMOS_ABONOS AS L INNER JOIN OBRAS AS O ON L.OBRA = O.ID WHERE O.AREA = @area AND L.CODIGO_AUTORIZANTE IS NOT NULL GROUP BY L.CODIGO_AUTORIZANTE ");
                datos.agregarParametro("@area", usuario.Area.Id);

                // Ejecutar la lectura
                datos.ejecutarLectura();

                // Crear columnas para el DataTable
                dt.Columns.Add("ID", typeof(int));
                dt.Columns.Add("NOMBRE", typeof(string));

                // Poblar el DataTable con los datos leídos
                while (datos.Lector.Read())
                {
                    DataRow row = dt.NewRow();
                    row["ID"] = Convert.ToInt32(datos.Lector["ID"]);
                    row["NOMBRE"] = datos.Lector["CODIGO_AUTORIZANTE"] as string;
                    dt.Rows.Add(row);
                }

                return dt; // Devolver el DataTable
            }
            catch (Exception ex)
            {
                // Manejo de errores
                throw new ApplicationException("Hubo un problema al obtener los códigos autorizantes.", ex);
            }
            finally
            {
                // Asegurarse de cerrar la conexión
                datos.cerrarConexion();
            }
        }

        public DataTable listarddl()
        {
            DataTable dt = new DataTable();
            AccesoDatos datos = new AccesoDatos();

            try
            {

                // Configurar la consulta
                datos.setearConsulta(@"
            SELECT  ROW_NUMBER() OVER (ORDER BY L.CODIGO_AUTORIZANTE) AS ID, L.CODIGO_AUTORIZANTE FROM LEGITIMOS_ABONOS AS L INNER JOIN OBRAS AS O ON L.OBRA = O.ID WHERE  L.CODIGO_AUTORIZANTE IS NOT NULL GROUP BY L.CODIGO_AUTORIZANTE ");

                // Ejecutar la lectura
                datos.ejecutarLectura();

                // Crear columnas para el DataTable
                dt.Columns.Add("ID", typeof(int));
                dt.Columns.Add("NOMBRE", typeof(string));

                // Poblar el DataTable con los datos leídos
                while (datos.Lector.Read())
                {
                    DataRow row = dt.NewRow();
                    row["ID"] = Convert.ToInt32(datos.Lector["ID"]);
                    row["NOMBRE"] = datos.Lector["CODIGO_AUTORIZANTE"] as string;
                    dt.Rows.Add(row);
                }

                return dt; // Devolver el DataTable
            }
            catch (Exception ex)
            {
                // Manejo de errores
                throw new ApplicationException("Hubo un problema al obtener los códigos autorizantes.", ex);
            }
            finally
            {
                // Asegurarse de cerrar la conexión
                datos.cerrarConexion();
            }
        }
        public List<Legitimo> listarFiltro(Usuario usuario, List<string> mesAprobacion, List<string> empresa, List<string> autorizante, List<string> estadoExpediente, string filtro = null)
        {
            var lista = new List<Legitimo>();
            var datos = new AccesoDatos();

            try
            {
                // Consulta básica para obtener los legítimos abonos sin calcular SIGAF
                string query = @"SELECT 
            L.ID, 
            CONCAT(O.DESCRIPCION, ' - ', BA.NOMBRE) AS OBRA,
            L.CODIGO_AUTORIZANTE, 
            L.EXPEDIENTE, 
            L.INICIO_EJECUCION, 
            L.FIN_EJECUCION,
            L.CERTIFICADO, 
            L.MES_APROBACION, 
            EM.NOMBRE AS EMPRESA
        FROM LEGITIMOS_ABONOS AS L 
        INNER JOIN OBRAS AS O ON L.OBRA = O.ID 
        INNER JOIN BARRIOS AS BA ON O.BARRIO = BA.ID 
        INNER JOIN EMPRESAS AS EM ON O.EMPRESA = EM.ID 
        WHERE O.AREA = @area";

                // Aplicar filtros si están presentes
                if (empresa != null && empresa.Count > 0)
                {
                    string empresasParam = string.Join(",", empresa.Select((e, i) => $"@empresa{i}"));
                    query += $" AND EM.NOMBRE IN ({empresasParam})";
                    for (int i = 0; i < empresa.Count; i++)
                    {
                        datos.setearParametros($"@empresa{i}", empresa[i]);
                    }
                }

                if (autorizante != null && autorizante.Count > 0)
                {
                    string AutorizanteParam = string.Join(",", autorizante.Select((e, i) => $"@autorizante{i}"));
                    query += $" AND L.CODIGO_AUTORIZANTE IN ({AutorizanteParam})";
                    for (int i = 0; i < autorizante.Count; i++)
                    {
                        datos.setearParametros($"@autorizante{i}", autorizante[i]);
                    }
                }

                if (mesAprobacion != null && mesAprobacion.Count > 0)
                {
                    try
                    {
                        // Dividir y validar el formato de las fechas
                        var mesesAnios = mesAprobacion
                            .Where(ma => ma.Contains("-"))
                            .Select(ma => ma.Split('-'))
                            .Select(parts => new { Año = parts[0], Mes = parts[1] });

                        // Construir los filtros dinámicos
                        string filtrosMesAño = string.Join(" OR ", mesesAnios.Select((ma, i) => $"(MONTH(L.MES_APROBACION) = @Mes{i} AND YEAR(L.MES_APROBACION) = @Año{i})"));
                        query += $" AND ({filtrosMesAño})";

                        // Asignar los parámetros
                        int index = 0;
                        foreach (var ma in mesesAnios)
                        {
                            if (int.TryParse(ma.Mes, out int mes) && int.TryParse(ma.Año, out int año))
                            {
                                datos.setearParametros($"@Mes{index}", mes);
                                datos.setearParametros($"@Año{index}", año);
                                index++;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Error al procesar el filtro de fechas.", ex);
                    }
                }

                // Filtro por estado de expediente
                if (estadoExpediente != null && estadoExpediente.Count > 0)
                {
                    var condicionesSql = new Dictionary<EstadoExpediente, string>
                    {
                        [EstadoExpediente.NoIniciado] = "(L.EXPEDIENTE IS NULL OR LTRIM(RTRIM(L.EXPEDIENTE)) = '')",
                        [EstadoExpediente.EnTramite] = @"(L.EXPEDIENTE IS NOT NULL 
                    AND LTRIM(RTRIM(L.EXPEDIENTE)) != '' 
                    AND NOT EXISTS (SELECT 1 FROM DEVENGADOS D 
                    WHERE D.EE_FINANCIERA = L.EXPEDIENTE))",
                        [EstadoExpediente.Devengado] = @"(L.EXPEDIENTE IS NOT NULL 
                    AND LTRIM(RTRIM(L.EXPEDIENTE)) != '' 
                    AND EXISTS (SELECT 1 FROM DEVENGADOS D 
                    WHERE D.EE_FINANCIERA = L.EXPEDIENTE))"
                    };

                    var condiciones = estadoExpediente
                        .Select(e => int.Parse(e))
                        .Where(e => Enum.IsDefined(typeof(EstadoExpediente), e))
                        .Select(e => condicionesSql[(EstadoExpediente)e]);

                    if (condiciones.Any())
                    {
                        query += $" AND ({string.Join(" OR ", condiciones)})";
                    }
                }

                // Filtro de búsqueda general
                if (!string.IsNullOrEmpty(filtro))
                {
                    query += " AND (O.DESCRIPCION LIKE @filtro OR BA.NOMBRE LIKE @filtro OR L.CODIGO_AUTORIZANTE LIKE @filtro OR L.EXPEDIENTE LIKE @filtro OR L.CERTIFICADO LIKE @filtro OR L.MES_APROBACION LIKE @filtro OR EM.NOMBRE LIKE @filtro) ";
                    datos.setearParametros("@filtro", $"%{filtro}%");
                }

                query += " ORDER BY O.DESCRIPCION, L.CODIGO_AUTORIZANTE, L.MES_APROBACION";
                datos.setearConsulta(query);
                datos.agregarParametro("@area", usuario.Area.Id);
                datos.ejecutarLectura();

                // Primera pasada: Recopilar todos los expedientes y montos de los legítimos
                Dictionary<string, List<decimal>> legitimosPorExpediente = new Dictionary<string, List<decimal>>();

                while (datos.Lector.Read())
                {
                    string expediente = datos.Lector["EXPEDIENTE"]?.ToString();
                    if (!string.IsNullOrEmpty(expediente) && datos.Lector["CERTIFICADO"] != DBNull.Value)
                    {
                        decimal certificado = Convert.ToDecimal(datos.Lector["CERTIFICADO"]);

                        if (!legitimosPorExpediente.ContainsKey(expediente))
                            legitimosPorExpediente[expediente] = new List<decimal>();

                        legitimosPorExpediente[expediente].Add(certificado);
                    }
                }

                // Obtener los certificados (incluyendo redeterminaciones) para combinar con los legítimos
                Dictionary<string, List<decimal>> certificadosPorExpediente = new Dictionary<string, List<decimal>>();

                // Intentar obtener los datos de certificados desde CalculoRedeterminacionNegocio
                try
                {
                    var calculoRedeterminacionNegocio = new CalculoRedeterminacionNegocio();
                    var certificados = calculoRedeterminacionNegocio.listarCertReliq();

                    // Agrupar los certificados por expediente
                    foreach (var cert in certificados)
                    {
                        if (!string.IsNullOrEmpty(cert.ExpedientePago))
                        {
                            if (!certificadosPorExpediente.ContainsKey(cert.ExpedientePago))
                                certificadosPorExpediente[cert.ExpedientePago] = new List<decimal>();

                            certificadosPorExpediente[cert.ExpedientePago].Add(cert.MontoTotal);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Si hay error al obtener los certificados, continuamos solo con los legítimos
                    System.Diagnostics.Debug.WriteLine($"Error al obtener certificados: {ex.Message}");
                }

                // Combinar los montos de legítimos y certificados
                Dictionary<string, List<decimal>> montosCombinados = new Dictionary<string, List<decimal>>();

                // Primero agregamos todos los legítimos
                foreach (var kvp in legitimosPorExpediente)
                {
                    string expediente = kvp.Key;
                    List<decimal> montos = kvp.Value;

                    montosCombinados[expediente] = new List<decimal>(montos);
                }

                // Luego agregamos los certificados
                foreach (var kvp in certificadosPorExpediente)
                {
                    string expediente = kvp.Key;
                    List<decimal> montos = kvp.Value;

                    if (!montosCombinados.ContainsKey(expediente))
                        montosCombinados[expediente] = new List<decimal>();

                    montosCombinados[expediente].AddRange(montos);
                }

                // Reiniciar la consulta para procesar los objetos legítimos
                datos.cerrarConexion();
                datos = new AccesoDatos();
                datos.setearConsulta(query);
                datos.agregarParametro("@area", usuario.Area.Id);

                // Reaplicar todos los parámetros
                if (empresa != null && empresa.Count > 0)
                {
                    for (int i = 0; i < empresa.Count; i++)
                    {
                        datos.setearParametros($"@empresa{i}", empresa[i]);
                    }
                }

                if (autorizante != null && autorizante.Count > 0)
                {
                    for (int i = 0; i < autorizante.Count; i++)
                    {
                        datos.setearParametros($"@autorizante{i}", autorizante[i]);
                    }
                }

                if (mesAprobacion != null && mesAprobacion.Count > 0)
                {
                    try
                    {
                        var mesesAnios = mesAprobacion
                            .Where(ma => ma.Contains("-"))
                            .Select(ma => ma.Split('-'))
                            .Select(parts => new { Año = parts[0], Mes = parts[1] });

                        int index = 0;
                        foreach (var ma in mesesAnios)
                        {
                            if (int.TryParse(ma.Mes, out int mes) && int.TryParse(ma.Año, out int año))
                            {
                                datos.setearParametros($"@Mes{index}", mes);
                                datos.setearParametros($"@Año{index}", año);
                                index++;
                            }
                        }
                    }
                    catch { /* Ya manejamos esta excepción antes */ }
                }

                if (!string.IsNullOrEmpty(filtro))
                {
                    datos.setearParametros("@filtro", $"%{filtro}%");
                }

                datos.ejecutarLectura();

                // Procesar los resultados y crear los objetos Legitimo
                while (datos.Lector.Read())
                {
                    string expediente = datos.Lector["EXPEDIENTE"]?.ToString();
                    decimal? certificado = datos.Lector["CERTIFICADO"] != DBNull.Value
                        ? Convert.ToDecimal(datos.Lector["CERTIFICADO"])
                        : (decimal?)null;

                    // Calcular SIGAF usando los montos combinados
                    decimal? sigaf = null;
                    if (!string.IsNullOrEmpty(expediente) && certificado.HasValue && montosCombinados.ContainsKey(expediente))
                    {
                        // Obtener total de devengados
                        decimal totalDevengado = SIGAFHelper.ObtenerTotalImporteDevengados(expediente);

                        if (totalDevengado > 0)
                        {
                            // Calcular proporción
                            decimal sumaMontos = montosCombinados[expediente].Sum();
                            if (sumaMontos > 0)
                            {
                                sigaf = totalDevengado * certificado.Value / sumaMontos;
                            }
                        }
                    }

                    // Obtener información de SADE
                    var sadeInfo = SADEHelper.ObtenerInfoSADE(expediente);

                    // Determinar el estado del expediente
                    string estado = SIGAFHelper.DeterminarEstadoExpediente(expediente);

                    var legitimoAbono = new Legitimo
                    {
                        Id = (int)datos.Lector["ID"],
                        CodigoAutorizante = datos.Lector["CODIGO_AUTORIZANTE"].ToString(),
                        Expediente = expediente,
                        Estado = estado,
                        Empresa = datos.Lector["EMPRESA"]?.ToString(),
                        InicioEjecucion = datos.Lector["INICIO_EJECUCION"] != DBNull.Value
                            ? (DateTime?)Convert.ToDateTime(datos.Lector["INICIO_EJECUCION"])
                            : null,
                        FinEjecucion = datos.Lector["FIN_EJECUCION"] != DBNull.Value
                            ? (DateTime?)Convert.ToDateTime(datos.Lector["FIN_EJECUCION"])
                            : null,
                        Certificado = certificado,
                        MesAprobacion = datos.Lector["MES_APROBACION"] != DBNull.Value
                            ? (DateTime?)Convert.ToDateTime(datos.Lector["MES_APROBACION"])
                            : null,
                        Obra = new Obra
                        {
                            Descripcion = datos.Lector["OBRA"].ToString()
                        },
                        Sigaf = sigaf,
                        FechaSade = sadeInfo.FechaUltimoPase,
                        BuzonSade = sadeInfo.BuzonDestino
                    };

                    lista.Add(legitimoAbono);
                }

                return lista;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar los legítimos abonos.", ex);
            }
            finally
            {
                datos.cerrarConexion();
            }
        }
        public List<Legitimo> listarFiltro(List<string> linea, List<string> areas, List<string> mesAprobacion, List<string> empresa, List<string> autorizante, List<string> estadoExpediente, string filtro = null)
        {
            var lista = new List<Legitimo>();
            var datos = new AccesoDatos();

            try
            {
                // Consulta simplificada sin los cálculos de SIGAF y ESTADO
                string query = @"
        SELECT 
            L.ID,
            CONCAT(O.DESCRIPCION, ' - ', BA.NOMBRE) AS OBRA,
            L.CODIGO_AUTORIZANTE,
            L.EXPEDIENTE,
            L.INICIO_EJECUCION,
            L.FIN_EJECUCION,
            L.CERTIFICADO,
            L.MES_APROBACION,
            EM.NOMBRE AS EMPRESA,
            A.ID AS AREA_ID,
            A.NOMBRE AS AREA,
            LG.NOMBRE AS LINEA
        FROM LEGITIMOS_ABONOS AS L
        INNER JOIN OBRAS AS O ON L.OBRA = O.ID
        INNER JOIN BARRIOS AS BA ON O.BARRIO = BA.ID
        INNER JOIN EMPRESAS AS EM ON O.EMPRESA = EM.ID
        INNER JOIN AREAS AS A ON O.AREA = A.ID
        LEFT JOIN BD_PROYECTOS BD ON O.ID = BD.ID_BASE
        LEFT JOIN LINEA_DE_GESTION LG ON BD.LINEA_DE_GESTION = LG.ID
        WHERE 1 = 1";

                // Aplicar filtros si están presentes
                if (linea != null && linea.Count > 0)
                {
                    string lineasParam = string.Join(",", linea.Select((e, i) => $"@linea{i}"));
                    query += $" AND LG.NOMBRE IN ({lineasParam})";
                    for (int i = 0; i < linea.Count; i++)
                    {
                        datos.setearParametros($"@linea{i}", linea[i]);
                    }
                }

                if (empresa != null && empresa.Count > 0)
                {
                    string empresasParam = string.Join(",", empresa.Select((e, i) => $"@empresa{i}"));
                    query += $" AND EM.NOMBRE IN ({empresasParam})";
                    for (int i = 0; i < empresa.Count; i++)
                    {
                        datos.setearParametros($"@empresa{i}", empresa[i]);
                    }
                }

                if (areas != null && areas.Count > 0)
                {
                    string areaParam = string.Join(",", areas.Select((e, i) => $"@area{i}"));
                    query += $" AND A.NOMBRE IN ({areaParam})";
                    for (int i = 0; i < areas.Count; i++)
                    {
                        datos.setearParametros($"@area{i}", areas[i]);
                    }
                }

                if (autorizante != null && autorizante.Count > 0)
                {
                    string AutorizanteParam = string.Join(",", autorizante.Select((e, i) => $"@autorizante{i}"));
                    query += $" AND L.CODIGO_AUTORIZANTE IN ({AutorizanteParam})";
                    for (int i = 0; i < autorizante.Count; i++)
                    {
                        datos.setearParametros($"@autorizante{i}", autorizante[i]);
                    }
                }

                if (mesAprobacion != null && mesAprobacion.Count > 0)
                {
                    try
                    {
                        // Dividir y validar el formato de las fechas
                        var mesesAnios = mesAprobacion
                            .Where(ma => ma.Contains("-"))
                            .Select(ma => ma.Split('-')) // Separar "2024-01" en ["2024", "01"]
                            .Select(parts => new { Año = parts[0], Mes = parts[1] });

                        // Construir los filtros dinámicos
                        string filtrosMesAño = string.Join(" OR ", mesesAnios.Select((ma, i) => $"(MONTH(L.MES_APROBACION) = @Mes{i} AND YEAR(L.MES_APROBACION) = @Año{i})"));
                        query += $" AND ({filtrosMesAño})";

                        // Asignar los parámetros
                        int index = 0;
                        foreach (var ma in mesesAnios)
                        {
                            if (int.TryParse(ma.Mes, out int mes) && int.TryParse(ma.Año, out int año))
                            {
                                datos.setearParametros($"@Mes{index}", mes);
                                datos.setearParametros($"@Año{index}", año);
                                index++;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Error al procesar el filtro de fechas.", ex);
                    }
                }

                // Filtro por estado de expediente
                if (estadoExpediente != null && estadoExpediente.Count > 0)
                {
                    var condicionesSql = new Dictionary<EstadoExpediente, string>
                    {
                        [EstadoExpediente.NoIniciado] = "(L.EXPEDIENTE IS NULL OR LTRIM(RTRIM(L.EXPEDIENTE)) = '')",
                        [EstadoExpediente.EnTramite] = @"(L.EXPEDIENTE IS NOT NULL 
                    AND LTRIM(RTRIM(L.EXPEDIENTE)) != '' 
                    AND NOT EXISTS (SELECT 1 FROM DEVENGADOS D 
                    WHERE D.EE_FINANCIERA = L.EXPEDIENTE))",
                        [EstadoExpediente.Devengado] = @"(L.EXPEDIENTE IS NOT NULL 
                    AND LTRIM(RTRIM(L.EXPEDIENTE)) != '' 
                    AND EXISTS (SELECT 1 FROM DEVENGADOS D 
                    WHERE D.EE_FINANCIERA = L.EXPEDIENTE))"
                    };

                    var condiciones = estadoExpediente
                        .Select(e => int.Parse(e))
                        .Where(e => Enum.IsDefined(typeof(EstadoExpediente), e))
                        .Select(e => condicionesSql[(EstadoExpediente)e]);

                    if (condiciones.Any())
                    {
                        query += $" AND ({string.Join(" OR ", condiciones)})";
                    }
                }

                // Filtro de búsqueda general
                if (!string.IsNullOrEmpty(filtro))
                {
                    query += " AND (O.DESCRIPCION LIKE @filtro OR A.NOMBRE LIKE @filtro OR BA.NOMBRE LIKE @filtro OR L.CODIGO_AUTORIZANTE LIKE @filtro OR L.EXPEDIENTE LIKE @filtro OR L.CERTIFICADO LIKE @filtro OR L.MES_APROBACION LIKE @filtro OR EM.NOMBRE LIKE @filtro) ";
                    datos.setearParametros("@filtro", $"%{filtro}%");
                }

                query += " ORDER BY O.DESCRIPCION, L.CODIGO_AUTORIZANTE, L.MES_APROBACION";
                datos.setearConsulta(query);
                datos.ejecutarLectura();

                // Primera pasada: Recopilar todos los expedientes y montos de los legítimos
                Dictionary<string, List<decimal>> legitimosPorExpediente = new Dictionary<string, List<decimal>>();

                while (datos.Lector.Read())
                {
                    string expediente = datos.Lector["EXPEDIENTE"]?.ToString();
                    if (!string.IsNullOrEmpty(expediente) && datos.Lector["CERTIFICADO"] != DBNull.Value)
                    {
                        decimal certificado = Convert.ToDecimal(datos.Lector["CERTIFICADO"]);

                        if (!legitimosPorExpediente.ContainsKey(expediente))
                            legitimosPorExpediente[expediente] = new List<decimal>();

                        legitimosPorExpediente[expediente].Add(certificado);
                    }
                }

                // Obtener los certificados (incluyendo redeterminaciones) para combinar con los legítimos
                Dictionary<string, List<decimal>> certificadosPorExpediente = new Dictionary<string, List<decimal>>();

                // Intentar obtener los datos de certificados desde CalculoRedeterminacionNegocio
                try
                {
                    var calculoRedeterminacionNegocio = new CalculoRedeterminacionNegocio();
                    var certificados = calculoRedeterminacionNegocio.listarCertReliq();

                    // Agrupar los certificados por expediente
                    foreach (var cert in certificados)
                    {
                        if (!string.IsNullOrEmpty(cert.ExpedientePago))
                        {
                            if (!certificadosPorExpediente.ContainsKey(cert.ExpedientePago))
                                certificadosPorExpediente[cert.ExpedientePago] = new List<decimal>();

                            certificadosPorExpediente[cert.ExpedientePago].Add(cert.MontoTotal);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Si hay error al obtener los certificados, continuamos solo con los legítimos
                    System.Diagnostics.Debug.WriteLine($"Error al obtener certificados: {ex.Message}");
                }

                // Combinar los montos de legítimos y certificados
                Dictionary<string, List<decimal>> montosCombinados = new Dictionary<string, List<decimal>>();

                // Primero agregamos todos los legítimos
                foreach (var kvp in legitimosPorExpediente)
                {
                    string expediente = kvp.Key;
                    List<decimal> montos = kvp.Value;

                    montosCombinados[expediente] = new List<decimal>(montos);
                }

                // Luego agregamos los certificados
                foreach (var kvp in certificadosPorExpediente)
                {
                    string expediente = kvp.Key;
                    List<decimal> montos = kvp.Value;

                    if (!montosCombinados.ContainsKey(expediente))
                        montosCombinados[expediente] = new List<decimal>();

                    montosCombinados[expediente].AddRange(montos);
                }

                // Reiniciar la consulta para procesar los objetos legítimos
                datos.cerrarConexion();
                datos = new AccesoDatos();
                datos.setearConsulta(query);

                // Reaplicar todos los parámetros para la segunda consulta
                if (linea != null && linea.Count > 0)
                {
                    for (int i = 0; i < linea.Count; i++)
                    {
                        datos.setearParametros($"@linea{i}", linea[i]);
                    }
                }

                if (empresa != null && empresa.Count > 0)
                {
                    for (int i = 0; i < empresa.Count; i++)
                    {
                        datos.setearParametros($"@empresa{i}", empresa[i]);
                    }
                }

                if (areas != null && areas.Count > 0)
                {
                    for (int i = 0; i < areas.Count; i++)
                    {
                        datos.setearParametros($"@area{i}", areas[i]);
                    }
                }

                if (autorizante != null && autorizante.Count > 0)
                {
                    for (int i = 0; i < autorizante.Count; i++)
                    {
                        datos.setearParametros($"@autorizante{i}", autorizante[i]);
                    }
                }

                if (mesAprobacion != null && mesAprobacion.Count > 0)
                {
                    try
                    {
                        var mesesAnios = mesAprobacion
                            .Where(ma => ma.Contains("-"))
                            .Select(ma => ma.Split('-'))
                            .Select(parts => new { Año = parts[0], Mes = parts[1] });

                        int index = 0;
                        foreach (var ma in mesesAnios)
                        {
                            if (int.TryParse(ma.Mes, out int mes) && int.TryParse(ma.Año, out int año))
                            {
                                datos.setearParametros($"@Mes{index}", mes);
                                datos.setearParametros($"@Año{index}", año);
                                index++;
                            }
                        }
                    }
                    catch { /* Ya manejamos esta excepción antes */ }
                }

                if (!string.IsNullOrEmpty(filtro))
                {
                    datos.setearParametros("@filtro", $"%{filtro}%");
                }

                datos.ejecutarLectura();

                // Procesar los resultados y crear los objetos Legitimo
                while (datos.Lector.Read())
                {
                    string expediente = datos.Lector["EXPEDIENTE"]?.ToString();
                    decimal? certificado = datos.Lector["CERTIFICADO"] != DBNull.Value
                        ? Convert.ToDecimal(datos.Lector["CERTIFICADO"])
                        : (decimal?)null;

                    // Calcular SIGAF usando los montos combinados
                    decimal? sigaf = null;
                    if (!string.IsNullOrEmpty(expediente) && certificado.HasValue && montosCombinados.ContainsKey(expediente))
                    {
                        // Obtener total de devengados
                        decimal totalDevengado = SIGAFHelper.ObtenerTotalImporteDevengados(expediente);

                        if (totalDevengado > 0)
                        {
                            // Calcular proporción
                            decimal sumaMontos = montosCombinados[expediente].Sum();
                            if (sumaMontos > 0)
                            {
                                sigaf = totalDevengado * certificado.Value / sumaMontos;
                            }
                        }
                    }

                    // Obtener información de SADE
                    var sadeInfo = SADEHelper.ObtenerInfoSADE(expediente);

                    // Determinar el estado del expediente
                    string estado = SIGAFHelper.DeterminarEstadoExpediente(expediente);

                    var legitimoAbono = new Legitimo
                    {
                        Id = (int)datos.Lector["ID"],
                        CodigoAutorizante = datos.Lector["CODIGO_AUTORIZANTE"].ToString(),
                        Expediente = expediente,
                        Estado = estado, // Determinado por el helper
                        Empresa = datos.Lector["EMPRESA"]?.ToString(),
                        InicioEjecucion = datos.Lector["INICIO_EJECUCION"] != DBNull.Value
                            ? Convert.ToDateTime(datos.Lector["INICIO_EJECUCION"])
                            : (DateTime?)null,
                        FinEjecucion = datos.Lector["FIN_EJECUCION"] != DBNull.Value
                            ? Convert.ToDateTime(datos.Lector["FIN_EJECUCION"])
                            : (DateTime?)null,
                        Certificado = certificado,
                        MesAprobacion = datos.Lector["MES_APROBACION"] != DBNull.Value
                            ? Convert.ToDateTime(datos.Lector["MES_APROBACION"])
                            : (DateTime?)null,
                        Obra = new Obra
                        {
                            Descripcion = datos.Lector["OBRA"].ToString(),
                            Area = new Area
                            {
                                Id = Convert.ToInt32(datos.Lector["AREA_ID"]),
                                Nombre = datos.Lector["AREA"].ToString()
                            }
                        },
                        Sigaf = sigaf, // Calculado usando montos combinados
                        FechaSade = sadeInfo.FechaUltimoPase, // Obtenido del helper
                        BuzonSade = sadeInfo.BuzonDestino, // Obtenido del helper
                        Linea = datos.Lector["LINEA"]?.ToString()
                    };

                    lista.Add(legitimoAbono);
                }

                return lista;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar los legítimos abonos.", ex);
            }
            finally
            {
                datos.cerrarConexion();
            }
        }
        public bool ActualizarExpediente(int id, string ex)
        {
            var datos = new AccesoDatos();

            try
            {
                datos.setearConsulta(@"
        UPDATE LEGITIMOS_ABONOS 
        SET 
            EXPEDIENTE = @expediente
           WHERE ID = @id");

                datos.agregarParametro("@expediente", ex);
                datos.agregarParametro("@id", id);

                datos.ejecutarAccion();
                return true;
            }
            catch (Exception)
            {
                throw new Exception("Error al modificar el autorizante.");
            }
            finally
            {
                datos.cerrarConexion();
            }
        }
    }
}
