using Dominio;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negocio
{
    public class CertificadoNegocio
    {
        public List<Certificado> listar(Usuario usuario)
        {
            var lista = new List<Certificado>();
            var datos = new AccesoDatos();

            try
            {
                datos.setearConsulta(@"
    SELECT 
        C.ID, 
        CONCAT(CO.NOMBRE, ' ', O.NUMERO, '/', O.AÑO) AS CONTRATA, 
        O.DESCRIPCION, 
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
        B.AUTORIZADO_NUEVO 
    FROM CERTIFICADOS C
    INNER JOIN TIPO_PAGO T ON C.TIPO_PAGO = T.ID
    INNER JOIN AUTORIZANTES A ON C.CODIGO_AUTORIZANTE = A.CODIGO_AUTORIZANTE
    INNER JOIN OBRAS O ON A.OBRA = O.ID
    INNER JOIN AREAS AR ON O.AREA = AR.ID
    INNER JOIN ESTADOS_AUTORIZANTES E ON A.ESTADO = E.ID
    INNER JOIN CONTRATA CO ON O.CONTRATA = CO.ID
    LEFT JOIN BD_PROYECTOS B ON O.ID = B.ID_BASE 
    WHERE O.AREA = @area");

                datos.agregarParametro("@area", usuario.Area.Id);
                datos.ejecutarLectura();

                while (datos.Lector.Read())
                {
                    var certificado = new Certificado
                    {
                        Id = datos.Lector["ID"] != DBNull.Value ? Convert.ToInt32(datos.Lector["ID"]) : 0,
                        ExpedientePago = datos.Lector["EXPEDIENTE_PAGO"]?.ToString(),
                        MontoTotal = datos.Lector["MONTO_TOTAL"] != DBNull.Value ? Convert.ToDecimal(datos.Lector["MONTO_TOTAL"]) : 0M,
                        MesAprobacion = datos.Lector["MES_APROBACION"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(datos.Lector["MES_APROBACION"]) : null,
                        Porcentaje = datos.Lector["PORCENTAJE"] != DBNull.Value ? Convert.ToDecimal(datos.Lector["PORCENTAJE"]) / 100 : 0M,
                        Tipo = new TipoPago
                        {
                            Id = datos.Lector["TIPO_PAGO"] != DBNull.Value ? Convert.ToInt32(datos.Lector["TIPO_PAGO"]) : 0,
                            Nombre = datos.Lector["TIPO_PAGO_NOMBRE"]?.ToString()
                        },
                        Autorizante = new Autorizante
                        {
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





        public List<Certificado> listar()
        {
            var lista = new List<Certificado>();
            var datos = new AccesoDatos();

            try
            {
                datos.setearConsulta(@"
    SELECT 
        C.ID, 
        CONCAT(CO.NOMBRE, ' ', O.NUMERO, '/', O.AÑO) AS CONTRATA, 
        O.DESCRIPCION, 
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
        B.AUTORIZADO_NUEVO 
    FROM CERTIFICADOS C
    INNER JOIN TIPO_PAGO T ON C.TIPO_PAGO = T.ID
    INNER JOIN AUTORIZANTES A ON C.CODIGO_AUTORIZANTE = A.CODIGO_AUTORIZANTE
    INNER JOIN OBRAS O ON A.OBRA = O.ID
    INNER JOIN AREAS AR ON O.AREA = AR.ID
    INNER JOIN ESTADOS_AUTORIZANTES E ON A.ESTADO = E.ID
    INNER JOIN CONTRATA CO ON O.CONTRATA = CO.ID
    LEFT JOIN BD_PROYECTOS B ON O.ID = B.ID_BASE");

                datos.ejecutarLectura();

                while (datos.Lector.Read())
                {
                    var certificado = new Certificado
                    {
                        Id = datos.Lector["ID"] != DBNull.Value ? Convert.ToInt32(datos.Lector["ID"]) : 0,
                        ExpedientePago = datos.Lector["EXPEDIENTE_PAGO"]?.ToString(),
                        MontoTotal = datos.Lector["MONTO_TOTAL"] != DBNull.Value ? Convert.ToDecimal(datos.Lector["MONTO_TOTAL"]) : 0M,
                        MesAprobacion = datos.Lector["MES_APROBACION"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(datos.Lector["MES_APROBACION"]) : null,
                        Porcentaje = datos.Lector["PORCENTAJE"] != DBNull.Value ? Convert.ToDecimal(datos.Lector["PORCENTAJE"]) / 100 : 0M,
                        Tipo = new TipoPago
                        {
                            Id = datos.Lector["TIPO_PAGO"] != DBNull.Value ? Convert.ToInt32(datos.Lector["TIPO_PAGO"]) : 0,
                            Nombre = datos.Lector["TIPO_PAGO_NOMBRE"]?.ToString()
                        },
                        Autorizante = new Autorizante
                        {
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
