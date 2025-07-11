﻿using Dominio;
using Dominio.Enums;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Negocio
{
    public class CertificadoNegocio
    {

        public bool ActualizarExpediente(int id, string ex)
        {
            var datos = new AccesoDatos();

            try
            {
                datos.setearConsulta(@"
        UPDATE CERTIFICADOS 
        SET 
            EXPEDIENTE_PAGO = @expediente
           WHERE ID = @id");

                datos.agregarParametro("@expediente", ex);
                datos.agregarParametro("@id", id);

                datos.ejecutarAccion();
                return true;
            }
            catch (Exception)
            {
                throw new Exception("Error al modificar el certificado.");
            }
            finally
            {
                datos.cerrarConexion();
            }
        }
        public List<Certificado> listarFiltro(Usuario usuario, List<string> autorizante, List<string> tipo, List<string> mesAprobacion, List<string> empresa, List<string> estadoExpediente ,string filtro = null)
        {
            var lista = new List<Certificado>();
            var datos = new AccesoDatos();

            try
            {
                string query = @"SELECT A.ID as ID_AUTORIZANTE,
                    A.DETALLE,
                    C.ID,
                    CONCAT(CO.NOMBRE, ' ', O.NUMERO, '/', O.AÑO) AS CONTRATA,
                    CONCAT(O.DESCRIPCION, ' - ', BA.NOMBRE) AS OBRA, 
                    O.ID AS OBRA_ID,
                    O.DESCRIPCION,
                    EM.ID AS EMPRESA_ID,
                    EM.NOMBRE AS EMPRESA, 
                    C.CODIGO_AUTORIZANTE, 
                    C.EXPEDIENTE_PAGO,
                    T.ID AS TIPO_PAGO, 
                    T.NOMBRE AS TIPO_PAGO_NOMBRE,
                    C.MONTO_TOTAL, 
                    C.MES_APROBACION, 
                    A.MONTO_AUTORIZADO, 
                    O.AREA AS AREAS_ID,
                    AR.NOMBRE AS AREAS_NOMBRE, 
                    A.ESTADO AS ESTADO_ID, 
                    E.NOMBRE AS ESTADO_NOMBRE, 
                    FORMAT((C.MONTO_TOTAL / A.MONTO_AUTORIZADO) * 100,'N2') AS PORCENTAJE,
                    B.ID AS PROYECTO_ID,
                    B.PROYECTO AS PROYECTO_NOMBRE,
                    BA.ID AS BARRIO_ID,
                    BA.NOMBRE AS BARRIO_NOMBRE,
                    B.AUTORIZADO_NUEVO,
                    CASE WHEN COUNT(C.ID) OVER (PARTITION BY C.EXPEDIENTE_PAGO) = 1 
                    THEN (SELECT SUM(D.IMPORTE_PP) FROM DEVENGADOS D 
                    WHERE D.EE_FINANCIERA = C.EXPEDIENTE_PAGO) 
                    ELSE (SELECT SUM(D.IMPORTE_PP) FROM DEVENGADOS D WHERE
                    D.EE_FINANCIERA = C.EXPEDIENTE_PAGO) * C.MONTO_TOTAL / (SELECT SUM(C2.MONTO_TOTAL) 
                    FROM CERTIFICADOS C2 
                    WHERE C2.EXPEDIENTE_PAGO = C.EXPEDIENTE_PAGO) END AS SIGAF,
                    CASE WHEN C.EXPEDIENTE_PAGO IS NULL
                    OR LTRIM(RTRIM(C.EXPEDIENTE_PAGO)) = '' 
                    THEN 'NO INICIADO' 
                    WHEN (CASE WHEN COUNT(C.ID) 
                    OVER (PARTITION BY C.EXPEDIENTE_PAGO) = 1 
                    THEN (SELECT SUM(D.IMPORTE_PP) 
                    FROM DEVENGADOS D 
                    WHERE D.EE_FINANCIERA = C.EXPEDIENTE_PAGO) 
                    ELSE (SELECT SUM(D.IMPORTE_PP) 
                    FROM DEVENGADOS D 
                    WHERE D.EE_FINANCIERA = C.EXPEDIENTE_PAGO) * C.MONTO_TOTAL / (SELECT SUM(C2.MONTO_TOTAL) 
                    FROM CERTIFICADOS C2 
                    WHERE C2.EXPEDIENTE_PAGO = C.EXPEDIENTE_PAGO) END) 
                    IS NOT NULL THEN 'DEVENGADO' 
                    ELSE 'EN TRAMITE' END AS ESTADO, 
                    PS.[BUZON DESTINO], 
                    PS.[FECHA ULTIMO PASE] 
                    FROM CERTIFICADOS C
                    INNER JOIN TIPO_PAGO T ON C.TIPO_PAGO = T.ID 
                    INNER JOIN AUTORIZANTES A ON C.CODIGO_AUTORIZANTE = A.CODIGO_AUTORIZANTE
                    INNER JOIN OBRAS O ON A.OBRA = O.ID 
                    INNER JOIN AREAS AR ON O.AREA = AR.ID 
                    INNER JOIN ESTADOS_AUTORIZANTES E ON A.ESTADO = E.ID 
                    INNER JOIN CONTRATA CO ON O.CONTRATA = CO.ID 
                    LEFT JOIN BD_PROYECTOS B ON O.ID = B.ID_BASE 
                    LEFT JOIN PASES_SADE PS ON C.EXPEDIENTE_PAGO = PS.EXPEDIENTE COLLATE Modern_Spanish_CI_AS 
                    INNER JOIN EMPRESAS EM ON O.EMPRESA = EM.ID
                    INNER JOIN BARRIOS AS BA ON O.BARRIO = BA.ID 
                    WHERE O.AREA = @area ";
                
                


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
                    query += $" AND C.CODIGO_AUTORIZANTE IN ({AutorizanteParam})";
                    for (int i = 0; i < autorizante.Count; i++)
                    {
                        datos.setearParametros($"@autorizante{i}", autorizante[i]);
                    }
                }
                if (tipo != null && tipo.Count > 0)
                {
                    string TipoParam = string.Join(",", tipo.Select((e, i) => $"@tipo{i}"));
                    query += $" AND T.NOMBRE IN ({TipoParam})";
                    for (int i = 0; i < tipo.Count; i++)
                    {
                        datos.setearParametros($"@tipo{i}", tipo[i]);
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
                        string filtrosMesAño = string.Join(" OR ", mesesAnios.Select((ma, i) => $"(MONTH(C.MES_APROBACION) = @Mes{i} AND YEAR(C.MES_APROBACION) = @Año{i})"));
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


                if (estadoExpediente != null && estadoExpediente.Count > 0)
                {
                    var condicionesSql = new Dictionary<EstadoExpediente, string>
                    {
                        [EstadoExpediente.NoIniciado] = "(C.EXPEDIENTE_PAGO IS NULL OR LTRIM(RTRIM(C.EXPEDIENTE_PAGO)) = '')",
                        [EstadoExpediente.EnTramite] = @"(C.EXPEDIENTE_PAGO IS NOT NULL 
                            AND LTRIM(RTRIM(C.EXPEDIENTE_PAGO)) != '' 
                            AND NOT EXISTS (SELECT 1 FROM DEVENGADOS D 
                            WHERE D.EE_FINANCIERA = C.EXPEDIENTE_PAGO))",
                        [EstadoExpediente.Devengado] = @"(C.EXPEDIENTE_PAGO IS NOT NULL 
                            AND LTRIM(RTRIM(C.EXPEDIENTE_PAGO)) != '' 
                            AND EXISTS (SELECT 1 FROM DEVENGADOS D 
                            WHERE D.EE_FINANCIERA = C.EXPEDIENTE_PAGO))"
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


                if (!string.IsNullOrEmpty(filtro))
                {
                    query += " AND (A.DETALLE LIKE @filtro OR CO.NOMBRE LIKE @filtro OR O.NUMERO LIKE @filtro OR O.DESCRIPCION LIKE @filtro OR BA.NOMBRE LIKE @filtro OR EM.NOMBRE LIKE @filtro OR C.CODIGO_AUTORIZANTE LIKE @filtro OR C.EXPEDIENTE_PAGO LIKE @filtro OR T.NOMBRE LIKE @filtro OR C.MONTO_TOTAL LIKE @filtro OR C.MES_APROBACION LIKE @filtro OR A.MONTO_AUTORIZADO LIKE @filtro OR E.NOMBRE LIKE @filtro) ";
                    datos.setearParametros("@filtro", $"%{filtro}%");
                }


                query += " ORDER BY O.DESCRIPCION,C.CODIGO_AUTORIZANTE, C.MES_APROBACION";
                datos.setearConsulta(query);

                datos.agregarParametro("@area", usuario.Area.Id);
                datos.ejecutarLectura();

                while (datos.Lector.Read())
                {
                    var certificado = new Certificado
                    {
                        Id = datos.Lector["ID"] != DBNull.Value ? Convert.ToInt32(datos.Lector["ID"]) : 0,
                        ExpedientePago = datos.Lector["EXPEDIENTE_PAGO"]?.ToString(),
                        Empresa = datos.Lector["EMPRESA"]?.ToString(),
                        MontoTotal = datos.Lector["MONTO_TOTAL"] != DBNull.Value ? Convert.ToDecimal(datos.Lector["MONTO_TOTAL"]) : 0M,
                        MesAprobacion = datos.Lector["MES_APROBACION"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(datos.Lector["MES_APROBACION"]) : null,
                        Porcentaje = datos.Lector["PORCENTAJE"] != DBNull.Value ? Convert.ToDecimal(datos.Lector["PORCENTAJE"]) :0,
                        Sigaf = datos.Lector["SIGAF"] != DBNull.Value ? Convert.ToDecimal(datos.Lector["SIGAF"]) : (decimal?)null,
                        FechaSade = datos.Lector["FECHA ULTIMO PASE"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(datos.Lector["FECHA ULTIMO PASE"]) : null,
                        BuzonSade = datos.Lector["BUZON DESTINO"]?.ToString(),
                        Estado = datos.Lector["ESTADO"]?.ToString(),

                        Tipo = new TipoPago
                        {
                            Id = datos.Lector["TIPO_PAGO"] != DBNull.Value ? Convert.ToInt32(datos.Lector["TIPO_PAGO"]) : 0,
                            Nombre = datos.Lector["TIPO_PAGO_NOMBRE"]?.ToString()
                        },
                        Autorizante = new Autorizante
                        {
                            Id = (int)datos.Lector["ID_AUTORIZANTE"],
                            Detalle = datos.Lector["DETALLE"]?.ToString(),
                            CodigoAutorizante = datos.Lector["CODIGO_AUTORIZANTE"]?.ToString(),
                            MontoAutorizado = datos.Lector["MONTO_AUTORIZADO"] != DBNull.Value ? Convert.ToDecimal(datos.Lector["MONTO_AUTORIZADO"]) : 0M,
                            Estado = new EstadoAutorizante
                            {
                                Id = datos.Lector["ESTADO_ID"] != DBNull.Value ? Convert.ToInt32(datos.Lector["ESTADO_ID"]) : 0,
                                Nombre = datos.Lector["ESTADO_NOMBRE"]?.ToString()
                            },
                            Obra = new Obra
                            {
                                Id = datos.Lector["OBRA_ID"] != DBNull.Value ? Convert.ToInt32(datos.Lector["OBRA_ID"]) : 0,

                                Descripcion = datos.Lector["OBRA"]?.ToString(),
                                
                                Area = new Area
                                {
                                    Id = datos.Lector["AREAS_ID"] != DBNull.Value ? Convert.ToInt32(datos.Lector["AREAS_ID"]) : 0,
                                    Nombre = datos.Lector["AREAS_NOMBRE"]?.ToString()
                                },
                                Contrata = new Contrata
                                {
                                    Nombre = datos.Lector["CONTRATA"]?.ToString()
                                }
                            }
                        }
                    };

                    lista.Add(certificado);
                }

                return lista;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar certificados.", ex);
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        public List<Certificado> listarFiltroAdmin()
        {
            var lista = new List<Certificado>();
            var datos = new AccesoDatos();

            try
            {
                //string query = "SELECT A.ID as ID_AUTORIZANTE,BA.NOMBRE as NOMBRE_BARRIO,B.PROYECTO, C.ID, CONCAT(CO.NOMBRE, ' ', O.NUMERO, '/', O.AÑO) AS CONTRATA, O.DESCRIPCION, EM.NOMBRE AS EMPRESA, C.CODIGO_AUTORIZANTE, C.EXPEDIENTE_PAGO, T.ID AS TIPO_PAGO, T.NOMBRE AS TIPO_PAGO_NOMBRE, C.MONTO_TOTAL, C.MES_APROBACION, A.MONTO_AUTORIZADO, O.AREA AS AREAS_ID, AR.NOMBRE AS AREAS_NOMBRE, A.ESTADO AS ESTADO_ID, E.NOMBRE AS ESTADO_NOMBRE, FORMAT((C.MONTO_TOTAL / A.MONTO_AUTORIZADO) * 100, 'N2') AS PORCENTAJE, B.AUTORIZADO_NUEVO, CASE WHEN COUNT(C.ID) OVER (PARTITION BY C.EXPEDIENTE_PAGO) = 1 THEN (SELECT SUM(D.IMPORTE_PP) FROM DEVENGADOS D WHERE D.EE_FINANCIERA = C.EXPEDIENTE_PAGO) ELSE (SELECT SUM(D.IMPORTE_PP) FROM DEVENGADOS D WHERE D.EE_FINANCIERA = C.EXPEDIENTE_PAGO) * C.MONTO_TOTAL / (SELECT SUM(C2.MONTO_TOTAL) FROM CERTIFICADOS C2 WHERE C2.EXPEDIENTE_PAGO = C.EXPEDIENTE_PAGO) END AS SIGAF, CASE WHEN C.EXPEDIENTE_PAGO IS NULL OR LTRIM(RTRIM(C.EXPEDIENTE_PAGO)) = '' THEN 'NO INICIADO' WHEN (CASE WHEN COUNT(C.ID) OVER (PARTITION BY C.EXPEDIENTE_PAGO) = 1 THEN (SELECT SUM(D.IMPORTE_PP) FROM DEVENGADOS D WHERE D.EE_FINANCIERA = C.EXPEDIENTE_PAGO) ELSE (SELECT SUM(D.IMPORTE_PP) FROM DEVENGADOS D WHERE D.EE_FINANCIERA = C.EXPEDIENTE_PAGO) * C.MONTO_TOTAL / (SELECT SUM(C2.MONTO_TOTAL) FROM CERTIFICADOS C2 WHERE C2.EXPEDIENTE_PAGO = C.EXPEDIENTE_PAGO) END) IS NOT NULL THEN 'DEVENGADO' ELSE 'EN TRAMITE' END AS ESTADO, PS.[BUZON DESTINO], PS.[FECHA ULTIMO PASE] FROM CERTIFICADOS C INNER JOIN TIPO_PAGO T ON C.TIPO_PAGO = T.ID INNER JOIN AUTORIZANTES A ON C.CODIGO_AUTORIZANTE = A.CODIGO_AUTORIZANTE INNER JOIN OBRAS O ON A.OBRA = O.ID INNER JOIN AREAS AR ON O.AREA = AR.ID INNER JOIN ESTADOS_AUTORIZANTES E ON A.ESTADO = E.ID INNER JOIN CONTRATA CO ON O.CONTRATA = CO.ID LEFT JOIN BD_PROYECTOS B ON O.ID = B.ID_BASE LEFT JOIN PASES_SADE PS ON C.EXPEDIENTE_PAGO = PS.EXPEDIENTE COLLATE Modern_Spanish_CI_AS INNER JOIN EMPRESAS EM ON O.EMPRESA = EM.ID INNER JOIN BARRIOS BA ON O.BARRIO = BA.ID ";
                // Replace the query string initialization with:
                string query = @"SELECT 
    A.ID as ID_AUTORIZANTE,
    BA.ID AS BARRIO_ID,
    BA.NOMBRE as NOMBRE_BARRIO,
    B.PROYECTO AS NOMBRE_PROYECTO,
    B.ID AS PROYECTO_ID,
    EM.ID AS EMPRESA_ID, 
    C.ID, 
    CONCAT(CO.NOMBRE, ' ', O.NUMERO, '/', O.AÑO) AS CONTRATA, 
    CONCAT(O.DESCRIPCION, ' - ', BA.NOMBRE) AS OBRA,
    O.DESCRIPCION, 
    O.ID AS OBRA_ID,
    EM.NOMBRE AS EMPRESA, 
    C.CODIGO_AUTORIZANTE, 
    C.EXPEDIENTE_PAGO, 
    T.ID AS TIPO_PAGO, 
    T.NOMBRE AS TIPO_PAGO_NOMBRE, 
    C.MONTO_TOTAL, 
    C.MES_APROBACION, 
    A.MONTO_AUTORIZADO, 
    O.AREA AS AREAS_ID, 
    AR.NOMBRE AS AREAS_NOMBRE, 
    A.ESTADO AS ESTADO_ID, 
    E.NOMBRE AS ESTADO_NOMBRE, 
    FORMAT((C.MONTO_TOTAL / A.MONTO_AUTORIZADO) * 100, 'N2') AS PORCENTAJE, 
    B.AUTORIZADO_NUEVO, 
    CASE 
        WHEN COUNT(C.ID) OVER (PARTITION BY C.EXPEDIENTE_PAGO) = 1 
        THEN (SELECT SUM(D.IMPORTE_PP) FROM DEVENGADOS D WHERE D.EE_FINANCIERA = C.EXPEDIENTE_PAGO) 
        ELSE (SELECT SUM(D.IMPORTE_PP) FROM DEVENGADOS D WHERE D.EE_FINANCIERA = C.EXPEDIENTE_PAGO) * C.MONTO_TOTAL / 
            (SELECT SUM(C2.MONTO_TOTAL) FROM CERTIFICADOS C2 WHERE C2.EXPEDIENTE_PAGO = C.EXPEDIENTE_PAGO) 
    END AS SIGAF, 
    CASE 
        WHEN C.EXPEDIENTE_PAGO IS NULL OR LTRIM(RTRIM(C.EXPEDIENTE_PAGO)) = '' THEN 'NO INICIADO' 
        WHEN (CASE 
            WHEN COUNT(C.ID) OVER (PARTITION BY C.EXPEDIENTE_PAGO) = 1 
            THEN (SELECT SUM(D.IMPORTE_PP) FROM DEVENGADOS D WHERE D.EE_FINANCIERA = C.EXPEDIENTE_PAGO) 
            ELSE (SELECT SUM(D.IMPORTE_PP) FROM DEVENGADOS D WHERE D.EE_FINANCIERA = C.EXPEDIENTE_PAGO) * C.MONTO_TOTAL / 
                (SELECT SUM(C2.MONTO_TOTAL) FROM CERTIFICADOS C2 WHERE C2.EXPEDIENTE_PAGO = C.EXPEDIENTE_PAGO) 
        END) IS NOT NULL THEN 'DEVENGADO' 
        ELSE 'EN TRAMITE' 
    END AS ESTADO, 
    PS.[BUZON DESTINO], 
    PS.[FECHA ULTIMO PASE],
    ISNULL(LDG.NOMBRE, 'Sin Línea') AS LINEA_GESTION_NOMBRE,
    LDG.ID AS LINEA_GESTION_ID
FROM CERTIFICADOS C 
INNER JOIN TIPO_PAGO T ON C.TIPO_PAGO = T.ID 
INNER JOIN AUTORIZANTES A ON C.CODIGO_AUTORIZANTE = A.CODIGO_AUTORIZANTE 
INNER JOIN OBRAS O ON A.OBRA = O.ID 
INNER JOIN AREAS AR ON O.AREA = AR.ID 
INNER JOIN ESTADOS_AUTORIZANTES E ON A.ESTADO = E.ID 
INNER JOIN CONTRATA CO ON O.CONTRATA = CO.ID 
INNER JOIN EMPRESAS EM ON O.EMPRESA = EM.ID 
INNER JOIN BARRIOS BA ON O.BARRIO = BA.ID 
LEFT JOIN BD_PROYECTOS B ON O.ID = B.ID_BASE 
LEFT JOIN LINEA_DE_GESTION LDG ON B.LINEA_DE_GESTION = LDG.ID
LEFT JOIN PASES_SADE PS ON C.EXPEDIENTE_PAGO = PS.EXPEDIENTE COLLATE Modern_Spanish_CI_AS 
WHERE 1=1";
       query += " ORDER BY O.DESCRIPCION,C.CODIGO_AUTORIZANTE, C.MES_APROBACION";
                datos.setearConsulta(query);
                datos.ejecutarLectura();

                while (datos.Lector.Read())
                {
                    var certificado = new Certificado
                    {
                        Id = datos.Lector["ID"] != DBNull.Value ? Convert.ToInt32(datos.Lector["ID"]) : 0,
                        ExpedientePago = datos.Lector["EXPEDIENTE_PAGO"]?.ToString(),
                        Empresa = datos.Lector["EMPRESA"]?.ToString(),
                        MontoTotal = datos.Lector["MONTO_TOTAL"] != DBNull.Value ? Convert.ToDecimal(datos.Lector["MONTO_TOTAL"]) : 0M,
                        MesAprobacion = datos.Lector["MES_APROBACION"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(datos.Lector["MES_APROBACION"]) : null,
                        Porcentaje = datos.Lector["PORCENTAJE"] != DBNull.Value ? Convert.ToDecimal(datos.Lector["PORCENTAJE"]) : 0,
                        Sigaf = datos.Lector["SIGAF"] != DBNull.Value ? Convert.ToDecimal(datos.Lector["SIGAF"]) : (decimal?)null,
                        FechaSade = datos.Lector["FECHA ULTIMO PASE"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(datos.Lector["FECHA ULTIMO PASE"]) : null,
                        BuzonSade = datos.Lector["BUZON DESTINO"]?.ToString(),
                        Estado = datos.Lector["ESTADO"]?.ToString(),

                        Tipo = new TipoPago
                        {
                            Id = datos.Lector["TIPO_PAGO"] != DBNull.Value ? Convert.ToInt32(datos.Lector["TIPO_PAGO"]) : 0,
                            Nombre = datos.Lector["TIPO_PAGO_NOMBRE"]?.ToString()
                        },
                        Autorizante = new Autorizante
                        {
                            Id = (int)datos.Lector["ID_AUTORIZANTE"],

                            CodigoAutorizante = datos.Lector["CODIGO_AUTORIZANTE"]?.ToString(),
                            MontoAutorizado = datos.Lector["MONTO_AUTORIZADO"] != DBNull.Value ? Convert.ToDecimal(datos.Lector["MONTO_AUTORIZADO"]) : 0M,
                            Estado = new EstadoAutorizante
                            {
                                Id = datos.Lector["ESTADO_ID"] != DBNull.Value ? Convert.ToInt32(datos.Lector["ESTADO_ID"]) : 0,
                                Nombre = datos.Lector["ESTADO_NOMBRE"]?.ToString()
                            },
                            Obra = new Obra
                            {
                                Id = datos.Lector["OBRA_ID"] != DBNull.Value ? Convert.ToInt32(datos.Lector["OBRA_ID"]) : 0,
                                Descripcion = datos.Lector["DESCRIPCION"]?.ToString(),
                                Proyecto = datos.Lector["PROYECTO_ID"] != DBNull.Value ?
                                    new BdProyecto
                                    {
                                        Id = Convert.ToInt32(datos.Lector["PROYECTO_ID"]),
                                        Proyecto = datos.Lector["NOMBRE_PROYECTO"]?.ToString()
                                    } : null,
                                LineaGestion = datos.Lector["LINEA_GESTION_ID"] != DBNull.Value ?
                                    new LineaGestion
                                    {
                                        Id = Convert.ToInt32(datos.Lector["LINEA_GESTION_ID"]),
                                        Nombre = datos.Lector["LINEA_GESTION_NOMBRE"]?.ToString()
                                    } : null,
                                Area = new Area
                                {
                                    Id = datos.Lector["AREAS_ID"] != DBNull.Value ? Convert.ToInt32(datos.Lector["AREAS_ID"]) : 0,
                                    Nombre = datos.Lector["AREAS_NOMBRE"]?.ToString()
                                },
                                Contrata = new Contrata
                                {
                                    Nombre = datos.Lector["CONTRATA"]?.ToString()
                                },
                                Barrio = new Barrio
                                {
                                    Id = datos.Lector["BARRIO_ID"] != DBNull.Value ? Convert.ToInt32(datos.Lector["BARRIO_ID"]) : 0,
                                    Nombre = datos.Lector["NOMBRE_BARRIO"]?.ToString()
                                },
                                Empresa = new Empresa
                                {
                                    Id = datos.Lector["EMPRESA_ID"] != DBNull.Value ? Convert.ToInt32(datos.Lector["EMPRESA_ID"]) : 0,
                                    Nombre = datos.Lector["EMPRESA"]?.ToString()
                                }
                            }
                        }
                    };

                    lista.Add(certificado);
                }

                return lista;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar certificados." + ex.Message, ex);
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        /*Remplazado por listarFiltroAdmin sin parámetros
                public List<Certificado> listarFiltroAdmin(List<string> areas, List<string> barrios, List<string> proyectos, List<string> autorizante, List<string> tipo, List<string> mesAprobacion, List<string> empresa, List<string> estadoExpediente, List<string> lineaGestion, string filtro = null)
        {
            var lista = new List<Certificado>();
            var datos = new AccesoDatos();

            try
            {
                // Consulta base sin incluir los cálculos de SIGAF y el ESTADO en el SQL
                string query = @"SELECT 
            A.ID as ID_AUTORIZANTE,
            BA.ID AS BARRIO_ID,
            BA.NOMBRE as NOMBRE_BARRIO,
            EM.ID AS EMPRESA_ID,
            B.PROYECTO,
            B.ID AS PROYECTO_ID,
            C.ID, 
            CONCAT(CO.NOMBRE, ' ', O.NUMERO, '/', O.AÑO) AS CONTRATA, 
            CONCAT(O.DESCRIPCION, ' - ', BA.NOMBRE) AS OBRA,
            O.DESCRIPCION,
            O.ID AS OBRA_ID,
            EM.NOMBRE AS EMPRESA, 
            C.CODIGO_AUTORIZANTE, 
            C.EXPEDIENTE_PAGO, 
            T.ID AS TIPO_PAGO, 
            T.NOMBRE AS TIPO_PAGO_NOMBRE, 
            C.MONTO_TOTAL, 
            C.MES_APROBACION, 
            A.MONTO_AUTORIZADO, 
            O.AREA AS AREAS_ID, 
            AR.NOMBRE AS AREAS_NOMBRE, 
            A.ESTADO AS ESTADO_ID, 
            E.NOMBRE AS ESTADO_NOMBRE, 
            FORMAT((C.MONTO_TOTAL / A.MONTO_AUTORIZADO) * 100, 'N2') AS PORCENTAJE, 
            B.AUTORIZADO_NUEVO, 
            ISNULL(LDG.NOMBRE, 'Sin Línea') AS LINEA_GESTION_NOMBRE,
            LDG.ID AS LINEA_GESTION_ID
        FROM CERTIFICADOS C 
        INNER JOIN TIPO_PAGO T ON C.TIPO_PAGO = T.ID 
        INNER JOIN AUTORIZANTES A ON C.CODIGO_AUTORIZANTE = A.CODIGO_AUTORIZANTE 
        INNER JOIN OBRAS O ON A.OBRA = O.ID 
        INNER JOIN AREAS AR ON O.AREA = AR.ID 
        INNER JOIN ESTADOS_AUTORIZANTES E ON A.ESTADO = E.ID 
        INNER JOIN CONTRATA CO ON O.CONTRATA = CO.ID 
        INNER JOIN EMPRESAS EM ON O.EMPRESA = EM.ID 
        INNER JOIN BARRIOS BA ON O.BARRIO = BA.ID 
        LEFT JOIN BD_PROYECTOS B ON O.ID = B.ID_BASE 
        LEFT JOIN LINEA_DE_GESTION LDG ON B.LINEA_DE_GESTION = LDG.ID
        WHERE 1=1";


                if (areas != null && areas.Count > 0)
                {
                    string areaParam = string.Join(",", areas.Select((e, i) => $"@area{i}"));
                    query += $" AND AR.NOMBRE IN ({areaParam})";
                    for (int i = 0; i < areas.Count; i++)
                    {
                        datos.setearParametros($"@area{i}", areas[i]);
                    }
                }
                if (barrios != null && barrios.Count > 0)
                {
                    string barrioParam = string.Join(",", barrios.Select((e, i) => $"@barrio{i}"));
                    query += $" AND BA.NOMBRE IN ({barrioParam})";
                    for (int i = 0; i < barrios.Count; i++)
                    {
                        datos.setearParametros($"@barrio{i}", barrios[i]);
                    }
                }

                if (proyectos != null && proyectos.Count > 0)
                {
                    string proyectoParam = string.Join(",", proyectos.Select((e, i) => $"@proyecto{i}"));
                    query += $" AND B.PROYECTO IN ({proyectoParam})";
                    for (int i = 0; i < proyectos.Count; i++)
                    {
                        datos.setearParametros($"@proyecto{i}", proyectos[i]);
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
                if (autorizante != null && autorizante.Count > 0)
                {
                    string AutorizanteParam = string.Join(",", autorizante.Select((e, i) => $"@autorizante{i}"));
                    query += $" AND C.CODIGO_AUTORIZANTE IN ({AutorizanteParam})";
                    for (int i = 0; i < autorizante.Count; i++)
                    {
                        datos.setearParametros($"@autorizante{i}", autorizante[i]);
                    }
                }
                if (tipo != null && tipo.Count > 0)
                {
                    string TipoParam = string.Join(",", tipo.Select((e, i) => $"@tipo{i}"));
                    query += $" AND T.NOMBRE IN ({TipoParam})";
                    for (int i = 0; i < tipo.Count; i++)
                    {
                        datos.setearParametros($"@tipo{i}", tipo[i]);
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
                        string filtrosMesAño = string.Join(" OR ", mesesAnios.Select((ma, i) => $"(MONTH(C.MES_APROBACION) = @Mes{i} AND YEAR(C.MES_APROBACION) = @Año{i})"));
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

                if (estadoExpediente != null && estadoExpediente.Count > 0)
                {
                    var condicionesSql = new Dictionary<EstadoExpediente, string>
                    {
                        [EstadoExpediente.NoIniciado] = "(C.EXPEDIENTE_PAGO IS NULL OR LTRIM(RTRIM(C.EXPEDIENTE_PAGO)) = '')",
                        [EstadoExpediente.EnTramite] = @"(C.EXPEDIENTE_PAGO IS NOT NULL 
                                    AND LTRIM(RTRIM(C.EXPEDIENTE_PAGO)) != '' 
                                    AND NOT EXISTS (SELECT 1 FROM DEVENGADOS D 
                                    WHERE D.EE_FINANCIERA = C.EXPEDIENTE_PAGO))",
                        [EstadoExpediente.Devengado] = @"(C.EXPEDIENTE_PAGO IS NOT NULL 
                                    AND LTRIM(RTRIM(C.EXPEDIENTE_PAGO)) != '' 
                                    AND EXISTS (SELECT 1 FROM DEVENGADOS D 
                                    WHERE D.EE_FINANCIERA = C.EXPEDIENTE_PAGO))"
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

                if (lineaGestion != null && lineaGestion.Count > 0)
                {
                    string lineaGestionParam = string.Join(",", lineaGestion.Select((e, i) => $"@lineaGestion{i}"));
                    query += $" AND LDG.NOMBRE IN ({lineaGestionParam})";  // Cambiado LG por LDG para mantener consistencia
                    for (int i = 0; i < lineaGestion.Count; i++)
                    {
                        datos.setearParametros($"@lineaGestion{i}", lineaGestion[i]);
                    }
                }

                if (!string.IsNullOrEmpty(filtro))
                {
                    query += " AND (A.DETALLE LIKE @filtro OR CO.NOMBRE LIKE @filtro OR O.NUMERO LIKE @filtro OR O.DESCRIPCION LIKE @filtro  OR EM.NOMBRE LIKE @filtro OR C.CODIGO_AUTORIZANTE LIKE @filtro OR C.EXPEDIENTE_PAGO LIKE @filtro OR T.NOMBRE LIKE @filtro OR C.MONTO_TOTAL LIKE @filtro OR C.MES_APROBACION LIKE @filtro OR A.MONTO_AUTORIZADO LIKE @filtro OR E.NOMBRE LIKE @filtro) ";
                    datos.setearParametros("@filtro", $"%{filtro}%");
                }
                query += " ORDER BY O.DESCRIPCION,C.CODIGO_AUTORIZANTE, C.MES_APROBACION";
                datos.setearConsulta(query);
                datos.ejecutarLectura();


                // Primero recopilamos todos los expedientes y sus montos asociados
                Dictionary<string, List<decimal>> montosExpediente = new Dictionary<string, List<decimal>>();

                // Primera pasada para recolectar los montos por expediente
                while (datos.Lector.Read())
                {
                    string expediente = datos.Lector["EXPEDIENTE_PAGO"]?.ToString();
                    if (!string.IsNullOrEmpty(expediente))
                    {
                        decimal monto = datos.Lector["MONTO_TOTAL"] != DBNull.Value ?
                            Convert.ToDecimal(datos.Lector["MONTO_TOTAL"]) : 0M;

                        if (!montosExpediente.ContainsKey(expediente))
                            montosExpediente[expediente] = new List<decimal>();

                        montosExpediente[expediente].Add(monto);
                    }
                }

                // Reiniciar la consulta para la creación de los objetos
                datos.cerrarConexion();
                datos = new AccesoDatos();
                datos.setearConsulta(query);
                if (areas != null && areas.Count > 0)
                {
                    string areaParam = string.Join(",", areas.Select((e, i) => $"@area{i}"));
                    query += $" AND AR.NOMBRE IN ({areaParam})";
                    for (int i = 0; i < areas.Count; i++)
                    {
                        datos.setearParametros($"@area{i}", areas[i]);
                    }
                }
                if (barrios != null && barrios.Count > 0)
                {
                    string barrioParam = string.Join(",", barrios.Select((e, i) => $"@barrio{i}"));
                    query += $" AND BA.NOMBRE IN ({barrioParam})";
                    for (int i = 0; i < barrios.Count; i++)
                    {
                        datos.setearParametros($"@barrio{i}", barrios[i]);
                    }
                }

                if (proyectos != null && proyectos.Count > 0)
                {
                    string proyectoParam = string.Join(",", proyectos.Select((e, i) => $"@proyecto{i}"));
                    query += $" AND B.PROYECTO IN ({proyectoParam})";
                    for (int i = 0; i < proyectos.Count; i++)
                    {
                        datos.setearParametros($"@proyecto{i}", proyectos[i]);
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
                if (autorizante != null && autorizante.Count > 0)
                {
                    string AutorizanteParam = string.Join(",", autorizante.Select((e, i) => $"@autorizante{i}"));
                    query += $" AND C.CODIGO_AUTORIZANTE IN ({AutorizanteParam})";
                    for (int i = 0; i < autorizante.Count; i++)
                    {
                        datos.setearParametros($"@autorizante{i}", autorizante[i]);
                    }
                }
                if (tipo != null && tipo.Count > 0)
                {
                    string TipoParam = string.Join(",", tipo.Select((e, i) => $"@tipo{i}"));
                    query += $" AND T.NOMBRE IN ({TipoParam})";
                    for (int i = 0; i < tipo.Count; i++)
                    {
                        datos.setearParametros($"@tipo{i}", tipo[i]);
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
                        string filtrosMesAño = string.Join(" OR ", mesesAnios.Select((ma, i) => $"(MONTH(C.MES_APROBACION) = @Mes{i} AND YEAR(C.MES_APROBACION) = @Año{i})"));
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

                if (estadoExpediente != null && estadoExpediente.Count > 0)
                {
                    var condicionesSql = new Dictionary<EstadoExpediente, string>
                    {
                        [EstadoExpediente.NoIniciado] = "(C.EXPEDIENTE_PAGO IS NULL OR LTRIM(RTRIM(C.EXPEDIENTE_PAGO)) = '')",
                        [EstadoExpediente.EnTramite] = @"(C.EXPEDIENTE_PAGO IS NOT NULL 
                                    AND LTRIM(RTRIM(C.EXPEDIENTE_PAGO)) != '' 
                                    AND NOT EXISTS (SELECT 1 FROM DEVENGADOS D 
                                    WHERE D.EE_FINANCIERA = C.EXPEDIENTE_PAGO))",
                        [EstadoExpediente.Devengado] = @"(C.EXPEDIENTE_PAGO IS NOT NULL 
                                    AND LTRIM(RTRIM(C.EXPEDIENTE_PAGO)) != '' 
                                    AND EXISTS (SELECT 1 FROM DEVENGADOS D 
                                    WHERE D.EE_FINANCIERA = C.EXPEDIENTE_PAGO))"
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

                if (lineaGestion != null && lineaGestion.Count > 0)
                {
                    string lineaGestionParam = string.Join(",", lineaGestion.Select((e, i) => $"@lineaGestion{i}"));
                    query += $" AND LDG.NOMBRE IN ({lineaGestionParam})";  // Cambiado LG por LDG para mantener consistencia
                    for (int i = 0; i < lineaGestion.Count; i++)
                    {
                        datos.setearParametros($"@lineaGestion{i}", lineaGestion[i]);
                    }
                }

                if (!string.IsNullOrEmpty(filtro))
                {
                    query += " AND (A.DETALLE LIKE @filtro OR CO.NOMBRE LIKE @filtro OR O.NUMERO LIKE @filtro OR O.DESCRIPCION LIKE @filtro  OR EM.NOMBRE LIKE @filtro OR C.CODIGO_AUTORIZANTE LIKE @filtro OR C.EXPEDIENTE_PAGO LIKE @filtro OR T.NOMBRE LIKE @filtro OR C.MONTO_TOTAL LIKE @filtro OR C.MES_APROBACION LIKE @filtro OR A.MONTO_AUTORIZADO LIKE @filtro OR E.NOMBRE LIKE @filtro) ";
                    datos.setearParametros("@filtro", $"%{filtro}%");
                }
                datos.ejecutarLectura();
                // Ahora procesamos los resultados con nuestros helpers
                while (datos.Lector.Read())
                {
                    string expediente = datos.Lector["EXPEDIENTE_PAGO"]?.ToString();
                    decimal monto = datos.Lector["MONTO_TOTAL"] != DBNull.Value ?
                        Convert.ToDecimal(datos.Lector["MONTO_TOTAL"]) : 0M;

                    // Usar los helpers para calcular SIGAF y obtener info de SADE
                    decimal? sigaf = null;
                    if (!string.IsNullOrEmpty(expediente) && montosExpediente.ContainsKey(expediente))
                    {
                        sigaf = SIGAFHelper.CalcularSIGAF(expediente, monto, montosExpediente[expediente]);
                    }

                    // Obtener información de SADE
                    var sadeInfo = SADEHelper.ObtenerInfoSADE(expediente);

                    // Determinar el estado del expediente
                    string estado = SIGAFHelper.DeterminarEstadoExpediente(expediente);

                    var certificado = new Certificado
                    {
                        Id = datos.Lector["ID"] != DBNull.Value ? Convert.ToInt32(datos.Lector["ID"]) : 0,
                        ExpedientePago = expediente,
                        Empresa = datos.Lector["EMPRESA"]?.ToString(),
                        MontoTotal = monto,
                        MesAprobacion = datos.Lector["MES_APROBACION"] != DBNull.Value ?
                            (DateTime?)Convert.ToDateTime(datos.Lector["MES_APROBACION"]) : null,
                        Porcentaje = datos.Lector["PORCENTAJE"] != DBNull.Value ?
                            Convert.ToDecimal(datos.Lector["PORCENTAJE"]) : 0,
                        Sigaf = sigaf, // Calculado por el helper
                        FechaSade = sadeInfo.FechaUltimoPase, // Obtenido del helper
                        BuzonSade = sadeInfo.BuzonDestino, // Obtenido del helper
                        Estado = estado, // Determinado por el helper

                        Tipo = new TipoPago
                        {
                            Id = datos.Lector["TIPO_PAGO"] != DBNull.Value ? Convert.ToInt32(datos.Lector["TIPO_PAGO"]) : 0,
                            Nombre = datos.Lector["TIPO_PAGO_NOMBRE"]?.ToString()
                        },
                        Autorizante = new Autorizante
                        {
                            Id = (int)datos.Lector["ID_AUTORIZANTE"],
                            CodigoAutorizante = datos.Lector["CODIGO_AUTORIZANTE"]?.ToString(),
                            MontoAutorizado = datos.Lector["MONTO_AUTORIZADO"] != DBNull.Value ?
                                Convert.ToDecimal(datos.Lector["MONTO_AUTORIZADO"]) : 0M,
                            Estado = new EstadoAutorizante
                            {
                                Id = datos.Lector["ESTADO_ID"] != DBNull.Value ? Convert.ToInt32(datos.Lector["ESTADO_ID"]) : 0,
                                Nombre = datos.Lector["ESTADO_NOMBRE"]?.ToString()
                            },
                            Obra = new Obra
                            {
                                Descripcion = datos.Lector["DESCRIPCION"]?.ToString(),
                                Proyecto = datos.Lector["PROYECTO_ID"] != DBNull.Value ?
                                    new BdProyecto
                                    {
                                        Id = Convert.ToInt32(datos.Lector["PROYECTO_ID"]),
                                        Proyecto = datos.Lector["NOMBRE_PROYECTO"]?.ToString()
                                    } : null,
                                LineaGestion = datos.Lector["LINEA_GESTION_ID"] != DBNull.Value ?
                                    new LineaGestion
                                    {
                                        Id = Convert.ToInt32(datos.Lector["LINEA_GESTION_ID"]),
                                        Nombre = datos.Lector["LINEA_GESTION_NOMBRE"]?.ToString()
                                    } : null,
                                Area = new Area
                                {
                                    Id = datos.Lector["AREAS_ID"] != DBNull.Value ? Convert.ToInt32(datos.Lector["AREAS_ID"]) : 0,
                                    Nombre = datos.Lector["AREAS_NOMBRE"]?.ToString()
                                },
                                Contrata = new Contrata
                                {
                                    Nombre = datos.Lector["CONTRATA"]?.ToString()
                                },
                                Barrio = new Barrio
                                {
                                    Id = datos.Lector["BARRIO_ID"] != DBNull.Value ? Convert.ToInt32(datos.Lector["BARRIO_ID"]) : 0,
                                    Nombre = datos.Lector["NOMBRE_BARRIO"]?.ToString()
                                },
                                Empresa = new Empresa
                                {
                                    Id = datos.Lector["EMPRESA_ID"] != DBNull.Value ? Convert.ToInt32(datos.Lector["EMPRESA_ID"]) : 0,
                                    Nombre = datos.Lector["EMPRESA"]?.ToString()
                                }
                            }
                        }
                    };

                    lista.Add(certificado);
                }

                return lista;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar certificados: " + ex.Message, ex);
            }
            finally
            {
                datos.cerrarConexion();
            }
        }*/
        public bool agregar(Certificado certificado)
        {
            var datos = new AccesoDatos();

            try
            {
                datos.setearConsulta(@"
            INSERT INTO CERTIFICADOS (CODIGO_AUTORIZANTE, EXPEDIENTE_PAGO, TIPO_PAGO, MONTO_TOTAL, MES_APROBACION) 
            VALUES (@codigoAutorizante, @expedientePago, @tipoPago, @montoTotal, @mesAprobacion)");

                datos.agregarParametro("@codigoAutorizante", certificado.Autorizante.CodigoAutorizante);
                datos.agregarParametro("@expedientePago", (object)certificado.ExpedientePago ?? DBNull.Value);
                datos.agregarParametro("@tipoPago", certificado.Tipo.Id);
                datos.agregarParametro("@montoTotal", certificado.MontoTotal);
                datos.agregarParametro("@mesAprobacion", (object)certificado.MesAprobacion ?? DBNull.Value);

                datos.ejecutarAccion();
                return true;
            }
            catch (SqlException sqlEx)
            {
                // Agregar detalles específicos de SQL al mensaje
                throw new Exception($"Error al agregar el certificado. {sqlEx.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception("Error al agregar el certificado.", ex);
            }
            finally
            {
                datos.cerrarConexion();
            }
        }
        public bool eliminar(int id)
        {
            var datos = new AccesoDatos();

            try
            {
                datos.setearConsulta("DELETE FROM CERTIFICADOS WHERE ID = @id");
                datos.agregarParametro("@id", id);
                datos.ejecutarAccion();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al eliminar el certificado.", ex);
            }
            finally
            {
                datos.cerrarConexion();
            }
        }
        public bool modificar(Certificado certificado)
        {
            var datos = new AccesoDatos();

            try
            {
                datos.setearConsulta(@"
        UPDATE CERTIFICADOS 
        SET 
            CODIGO_AUTORIZANTE = @codigoAutorizante, 
            EXPEDIENTE_PAGO = @expedientePago, 
            TIPO_PAGO = @tipoPago, 
            MONTO_TOTAL = @montoTotal, 
            MES_APROBACION = @mesAprobacion
        WHERE ID = @id");

                datos.agregarParametro("@codigoAutorizante", certificado.Autorizante.CodigoAutorizante);
                datos.agregarParametro("@expedientePago", (object)certificado.ExpedientePago ?? DBNull.Value);
                datos.agregarParametro("@tipoPago", certificado.Tipo.Id);
                datos.agregarParametro("@montoTotal", certificado.MontoTotal);
                datos.agregarParametro("@mesAprobacion", (object)certificado.MesAprobacion ?? DBNull.Value);
                datos.agregarParametro("@id", certificado.Id);

                datos.ejecutarAccion();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al modificar el certificado.", ex);
            }
            finally
            {
                datos.cerrarConexion();
            }
        }
    }

}
