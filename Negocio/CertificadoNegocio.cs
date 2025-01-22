using Dominio;
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
        public List<Certificado> listarFiltro(Usuario usuario, string autorizante, string tipo, DateTime? mesAprobacion,string empresa )
        {
            var lista = new List<Certificado>();
            var datos = new AccesoDatos();

            try
            {
                string query = "SELECT A.ID as ID_AUTORIZANTE, C.ID, CONCAT(CO.NOMBRE, ' ', O.NUMERO, '/', O.AÑO) AS CONTRATA, CONCAT( O.DESCRIPCION, ' - ', BA.NOMBRE ) AS OBRA,EM.NOMBRE AS EMPRESA, C.CODIGO_AUTORIZANTE, C.EXPEDIENTE_PAGO, T.ID AS TIPO_PAGO, T.NOMBRE AS TIPO_PAGO_NOMBRE, C.MONTO_TOTAL, C.MES_APROBACION, A.MONTO_AUTORIZADO, O.AREA AS AREAS_ID, AR.NOMBRE AS AREAS_NOMBRE, A.ESTADO AS ESTADO_ID, E.NOMBRE AS ESTADO_NOMBRE, FORMAT((C.MONTO_TOTAL / A.MONTO_AUTORIZADO) * 100, 'N2') + '%' AS PORCENTAJE, B.AUTORIZADO_NUEVO, CASE WHEN COUNT(C.ID) OVER (PARTITION BY C.EXPEDIENTE_PAGO) = 1 THEN (SELECT SUM(D.IMPORTE_PP) FROM DEVENGADOS D WHERE D.EE_FINANCIERA = C.EXPEDIENTE_PAGO) ELSE (SELECT SUM(D.IMPORTE_PP) FROM DEVENGADOS D WHERE D.EE_FINANCIERA = C.EXPEDIENTE_PAGO) * C.MONTO_TOTAL / (SELECT SUM(C2.MONTO_TOTAL) FROM CERTIFICADOS C2 WHERE C2.EXPEDIENTE_PAGO = C.EXPEDIENTE_PAGO) END AS SIGAF, PS.[BUZON DESTINO], PS.[FECHA ULTIMO PASE] FROM CERTIFICADOS C INNER JOIN TIPO_PAGO T ON C.TIPO_PAGO = T.ID INNER JOIN AUTORIZANTES A ON C.CODIGO_AUTORIZANTE = A.CODIGO_AUTORIZANTE INNER JOIN OBRAS O ON A.OBRA = O.ID INNER JOIN AREAS AR ON O.AREA = AR.ID INNER JOIN ESTADOS_AUTORIZANTES E ON A.ESTADO = E.ID INNER JOIN CONTRATA CO ON O.CONTRATA = CO.ID LEFT JOIN BD_PROYECTOS B ON O.ID = B.ID_BASE LEFT JOIN PASES_SADE PS ON C.EXPEDIENTE_PAGO = PS.EXPEDIENTE COLLATE Modern_Spanish_CI_AS INNER JOIN EMPRESAS EM ON O.EMPRESA=EM.ID INNER JOIN BARRIOS AS BA ON O.BARRIO = BA.ID WHERE O.AREA = @area ";
                if (!string.IsNullOrEmpty(autorizante))
                {
                    query += " AND C.CODIGO_AUTORIZANTE = @Autorizante";
                    datos.setearParametros("@Autorizante", autorizante);
                }
                if (!string.IsNullOrEmpty(empresa))
                {
                    query += " AND EM.NOMBRE = @Empresa";
                    datos.setearParametros("@Empresa", empresa);
                }
                if (!string.IsNullOrEmpty(tipo))
                {
                    query += " AND T.NOMBRE = @Tipo";
                    datos.setearParametros("@Tipo", tipo);
                }
                if (mesAprobacion.HasValue)
                {
                    query += " AND MONTH(C.MES_APROBACION) = @Mes AND YEAR(C.MES_APROBACION) = @Año";
                    datos.setearParametros("@Mes", mesAprobacion.Value.Month);
                    datos.setearParametros("@Año", mesAprobacion.Value.Year);
                }

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
                        Porcentaje = datos.Lector["PORCENTAJE"]?.ToString(),
                        Sigaf = datos.Lector["SIGAF"] != DBNull.Value ? Convert.ToDecimal(datos.Lector["SIGAF"]) : (decimal?)null,
                        FechaSade = datos.Lector["FECHA ULTIMO PASE"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(datos.Lector["FECHA ULTIMO PASE"]) : null,
                        BuzonSade = datos.Lector["BUZON DESTINO"]?.ToString(),


                        Tipo = new TipoPago
                        {
                            Id = datos.Lector["TIPO_PAGO"] != DBNull.Value ? Convert.ToInt32(datos.Lector["TIPO_PAGO"]) : 0,
                            Nombre = datos.Lector["TIPO_PAGO_NOMBRE"]?.ToString()
                        },
                        Autorizante = new Autorizante
                        {
                            Id= (int)datos.Lector["ID_AUTORIZANTE"],
                            CodigoAutorizante = datos.Lector["CODIGO_AUTORIZANTE"]?.ToString(),
                            MontoAutorizado = datos.Lector["MONTO_AUTORIZADO"] != DBNull.Value ? Convert.ToDecimal(datos.Lector["MONTO_AUTORIZADO"]) : 0M,
                            Estado = new EstadoAutorizante
                            {
                                Id = datos.Lector["ESTADO_ID"] != DBNull.Value ? Convert.ToInt32(datos.Lector["ESTADO_ID"]) : 0,
                                Nombre = datos.Lector["ESTADO_NOMBRE"]?.ToString()
                            },
                            Obra = new Obra
                            {
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
        public List<Certificado> listarFiltroAdmin(string autorizante, string tipo, DateTime? mesAprobacion, string empresa)
        {
            var lista = new List<Certificado>();
            var datos = new AccesoDatos();

            try
            {
                string query = "SELECT A.ID as ID_AUTORIZANTE,C.ID, CONCAT(CO.NOMBRE, ' ', O.NUMERO, '/', O.AÑO) AS CONTRATA, O.DESCRIPCION,EM.NOMBRE AS EMPRESA, C.CODIGO_AUTORIZANTE, C.EXPEDIENTE_PAGO, T.ID AS TIPO_PAGO, T.NOMBRE AS TIPO_PAGO_NOMBRE, C.MONTO_TOTAL, C.MES_APROBACION, A.MONTO_AUTORIZADO, O.AREA AS AREAS_ID, AR.NOMBRE AS AREAS_NOMBRE, A.ESTADO AS ESTADO_ID, E.NOMBRE AS ESTADO_NOMBRE, FORMAT((C.MONTO_TOTAL / A.MONTO_AUTORIZADO) * 100, 'N2') AS PORCENTAJE, B.AUTORIZADO_NUEVO, CASE WHEN COUNT(C.ID) OVER (PARTITION BY C.EXPEDIENTE_PAGO) = 1 THEN (SELECT SUM(D.IMPORTE_PP) FROM DEVENGADOS D WHERE D.EE_FINANCIERA = C.EXPEDIENTE_PAGO) ELSE (SELECT SUM(D.IMPORTE_PP) FROM DEVENGADOS D WHERE D.EE_FINANCIERA = C.EXPEDIENTE_PAGO) * C.MONTO_TOTAL / (SELECT SUM(C2.MONTO_TOTAL) FROM CERTIFICADOS C2 WHERE C2.EXPEDIENTE_PAGO = C.EXPEDIENTE_PAGO) END AS SIGAF, PS.[BUZON DESTINO], PS.[FECHA ULTIMO PASE] FROM CERTIFICADOS C INNER JOIN TIPO_PAGO T ON C.TIPO_PAGO = T.ID INNER JOIN AUTORIZANTES A ON C.CODIGO_AUTORIZANTE = A.CODIGO_AUTORIZANTE INNER JOIN OBRAS O ON A.OBRA = O.ID INNER JOIN AREAS AR ON O.AREA = AR.ID INNER JOIN ESTADOS_AUTORIZANTES E ON A.ESTADO = E.ID INNER JOIN CONTRATA CO ON O.CONTRATA = CO.ID LEFT JOIN BD_PROYECTOS B ON O.ID = B.ID_BASE LEFT JOIN PASES_SADE PS ON C.EXPEDIENTE_PAGO = PS.EXPEDIENTE COLLATE Modern_Spanish_CI_AS INNER JOIN EMPRESAS EM ON O.EMPRESA=EM.ID";
                if (!string.IsNullOrEmpty(autorizante))
                {
                    query += " AND C.CODIGO_AUTORIZANTE = @Autorizante";
                    datos.setearParametros("@Autorizante", autorizante);
                }
                if (!string.IsNullOrEmpty(empresa))
                {
                    query += " AND EM.NOMBRE = @Empresa";
                    datos.setearParametros("@Empresa", empresa);
                }
                if (!string.IsNullOrEmpty(tipo))
                {
                    query += " AND T.NOMBRE = @Tipo";
                    datos.setearParametros("@Tipo", tipo);
                }
                if (mesAprobacion.HasValue)
                {
                    query += " AND MONTH(C.MES_APROBACION) = @Mes AND YEAR(C.MES_APROBACION) = @Año";
                    datos.setearParametros("@Mes", mesAprobacion.Value.Month);
                    datos.setearParametros("@Año", mesAprobacion.Value.Year);
                }

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
                        Porcentaje = datos.Lector["PORCENTAJE"]?.ToString(),
                        Sigaf = datos.Lector["SIGAF"] != DBNull.Value ? Convert.ToDecimal(datos.Lector["SIGAF"]) : (decimal?)null,
                        FechaSade = datos.Lector["FECHA ULTIMO PASE"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(datos.Lector["FECHA ULTIMO PASE"]) : null,
                        BuzonSade = datos.Lector["BUZON DESTINO"]?.ToString(),


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
                                Descripcion = datos.Lector["DESCRIPCION"]?.ToString(),
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
        public void agregar(Certificado certificado)
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
