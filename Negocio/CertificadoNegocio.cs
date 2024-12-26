using Dominio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negocio
{
    public class CertificadoNegocio
    {
        public List<Certificado> listar(Usuario usuario)
        {
            List<Certificado> lista = new List<Certificado>();
            AccesoDatos datos = new AccesoDatos();

            try
            {
                datos.setearConsulta(@"SELECT C.ID, CONCAT(CO.NOMBRE,' ',O.NUMERO,'/',O.AÑO) AS CONTRATA, 
                                      O.DESCRIPCION, 
                                      C.CODIGO_AUTORIZANTE, 
                                      EXPEDIENTE_PAGO, 
                                      T.ID AS TIPO_PAGO, 
                                      T.NOMBRE AS TIPO_PAGO_NOMBRE, 
                                      MONTO_TOTAL, 
                                      MES_APROBACION, 
                                      A.MONTO_AUTORIZADO, 
                                      FORMAT((MONTO_TOTAL / A.MONTO_AUTORIZADO) * 100, 'N2') AS PORCENTAJE, 
                                      B.AUTORIZADO_NUEVO 
                               FROM CERTIFICADOS AS C
                               INNER JOIN TIPO_PAGO AS T ON C.TIPO_PAGO = T.ID
                               INNER JOIN AUTORIZANTES AS A ON C.CODIGO_AUTORIZANTE = A.CODIGO_AUTORIZANTE
                               INNER JOIN OBRAS AS O ON A.OBRA = O.ID
                               INNER JOIN CONTRATA AS CO ON O.CONTRATA = CO.ID
                               LEFT JOIN BD_PROYECTOS AS B ON O.ID = B.ID_BASE 
                               WHERE O.AREA = @area");
                datos.agregarParametro("@area", usuario.Area.Id);
                datos.ejecutarLectura();

                while (datos.Lector.Read())
                {
                    Certificado aux = new Certificado
                    {
                        Id = datos.Lector["ID"] != DBNull.Value ? Convert.ToInt32(datos.Lector["ID"]) : 0,
                        Autorizante = new Autorizante
                        {
                            Id = datos.Lector["AUTORIZANTE_ID"] != DBNull.Value ? Convert.ToInt32(datos.Lector["AUTORIZANTE_ID"]) : 0, Obra = datos.Lector["AUTORIZANTE_OBRA"] != DBNull.Value ? new Obra { Id = Convert.ToInt32(datos.Lector["AUTORIZANTE_OBRA"]) } : null, CodigoAutorizante = datos.Lector["CODIGO_AUTORIZANTE"] as string, Detalle = datos.Lector["AUTORIZANTE_DETALLE"] as string, Concepto = datos.Lector["AUTORIZANTE_CONCEPTO"] as string, Estado = datos.Lector["AUTORIZANTE_ESTADO"] != DBNull.Value ? new EstadoAutorizante { Id = Convert.ToInt32(datos.Lector["AUTORIZANTE_ESTADO"]) } : null, Expediente = datos.Lector["AUTORIZANTE_EXPEDIENTE"] as string, MontoAutorizado = datos.Lector["AUTORIZANTE_MONTO_AUTORIZADO"] != DBNull.Value ? Convert.ToDecimal(datos.Lector["AUTORIZANTE_MONTO_AUTORIZADO"]) : 0M, AutorizacionGG = datos.Lector["AUTORIZANTE_AUTORIZACION_GG"] != DBNull.Value ? Convert.ToBoolean(datos.Lector["AUTORIZANTE_AUTORIZACION_GG"]) : false, Fecha = datos.Lector["AUTORIZANTE_FECHA"] != DBNull.Value ? (DateTime?)datos.Lector["AUTORIZANTE_FECHA"] : null
                        },
                        ExpedientePago = datos.Lector["EXPEDIENTE_PAGO"] != DBNull.Value ? datos.Lector["EXPEDIENTE_PAGO"].ToString() : null,
                        Tipo = new TipoPago
                        {
                            Id = datos.Lector["TIPO_PAGO"] != DBNull.Value ? Convert.ToInt32(datos.Lector["TIPO_PAGO"]) : 0,
                            Nombre = datos.Lector["TIPO_PAGO_NOMBRE"] != DBNull.Value ? datos.Lector["TIPO_PAGO_NOMBRE"].ToString() : null
                        },
                        MontoTotal = datos.Lector["MONTO_TOTAL"] != DBNull.Value ? Convert.ToDecimal(datos.Lector["MONTO_TOTAL"]) : 0M,
                        MesAprobacion = datos.Lector["MES_APROBACION"] != DBNull.Value ? (DateTime?)datos.Lector["MES_APROBACION"] : null
                    };

                    lista.Add(aux);
                }

                return lista;
            }
            catch (Exception ex)
            {
                // Manejar la excepción de manera adecuada
                throw new Exception("Error al listar certificados", ex);
            }
            finally
            {
                datos.cerrarConexion();
            }
        }




        public List<Certificado> listar()
        {
            List<Certificado> lista = new List<Certificado>();
            AccesoDatos datos = new AccesoDatos();

            try
            {
                datos.setearConsulta(@"
                el select para el listar debe ser el siguiente: SELECT CONCAT(CO.NOMBRE,' ',O.NUMERO,'/',O.AÑO) AS CONTRATA, O.DESCRIPCION, C.CODIGO_AUTORIZANTE, EXPEDIENTE_PAGO, T.NOMBRE, MONTO_TOTAL, MES_APROBACION, A.MONTO_AUTORIZADO, FORMAT((MONTO_TOTAL / A.MONTO_AUTORIZADO) * 100, 'N2') AS PORCENTAJE, B.AUTORIZADO_NUEVO FROM CERTIFICADOS AS C
INNER JOIN TIPO_PAGO AS T ON C.TIPO_PAGO = T.ID
INNER JOIN AUTORIZANTES AS A ON C.CODIGO_AUTORIZANTE = A.CODIGO_AUTORIZANTE
INNER JOIN OBRAS AS O ON A.OBRA = O.ID
INNER JOIN CONTRATA AS CO ON O.CONTRATA = CO.ID
left JOIN BD_PROYECTOS AS B ON O.ID = B.ID_BASE");

                datos.ejecutarLectura();

                while (datos.Lector.Read())
                {
                    Certificado aux = new Certificado
                    {
                        Id = (int)datos.Lector["ID"],

                        Autorizante = new Autorizante
                        {
                            Id = (int)datos.Lector["CODIGO_AUTORIZANTE"],

                        },
                        ExpedientePago = datos.Lector["EXPEDIENTE_PAGO"] as string,
                        Tipo = new TipoPago
                        {
                            Id = (int)datos.Lector["TIPO_PAGO"],
                            Nombre = datos.Lector["TIPO_PAGO_DESCRIPCION"] as string
                        },
                        MontoTotal = datos.Lector["MONTO_TOTAL"] != DBNull.Value ? (decimal)datos.Lector["MONTO_TOTAL"] : 0M,
                        MesAprobacion = datos.Lector["MES_APROBACION"] != DBNull.Value ? (DateTime)datos.Lector["MES_APROBACION"] : (DateTime?)null
                    };

                    lista.Add(aux);
                }

                return lista;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        public bool agregar(Certificado nuevoCertificado)
        {
            AccesoDatos datos = new AccesoDatos();

            try
            {
                datos.setearConsulta(@"
                INSERT INTO CERTIFICADOS 
                (CODIGO_AUTORIZANTE, EXPEDIENTE_PAGO, TIPO_PAGO, MONTO_TOTAL, MES_APROBACION)
                VALUES 
                (@CODIGO_AUTORIZANTE, @EXPEDIENTE_PAGO, @TIPO_PAGO, @MONTO_TOTAL, @MES_APROBACION)");

                datos.agregarParametro("@CODIGO_AUTORIZANTE", nuevoCertificado.Autorizante.CodigoAutorizante);
                datos.agregarParametro("@EXPEDIENTE_PAGO", nuevoCertificado.ExpedientePago ?? string.Empty);
                datos.agregarParametro("@TIPO_PAGO", nuevoCertificado.Tipo.Id);
                datos.agregarParametro("@MONTO_TOTAL", nuevoCertificado.MontoTotal);
                datos.agregarParametro("@MES_APROBACION", nuevoCertificado.MesAprobacion);

                datos.ejecutarAccion();

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }
    }
}
