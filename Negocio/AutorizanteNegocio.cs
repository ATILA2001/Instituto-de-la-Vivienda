using Dominio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Negocio
{
    public class AutorizanteNegocio
    {

        public bool eliminar(string codigo)
        {
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("DELETE FROM AUTORIZANTES WHERE CODIGO_AUTORIZANTE = @ID");

                datos.agregarParametro("@ID", codigo);

                datos.ejecutarAccion();

                return true;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Hubo un problema al intentar eliminar el autorizante.", ex);
            }
            finally
            {
                datos.cerrarConexion();
            }
        }
        public List<Autorizante> listar(Usuario usuario, string estado, string empresa, int obra)
        {
            List<Autorizante> lista = new List<Autorizante>();
            AccesoDatos datos = new AccesoDatos();

            try
            {
                string query = "SELECT    CONCAT(C.NOMBRE, ' ', O.NUMERO, '/', O.AÑO) AS CONTRATA, O.ID AS OBRA_ID,   CONCAT(O.DESCRIPCION, ' - ', BA.NOMBRE ) AS OBRA,    EM.NOMBRE AS EMPRESA,    A.CODIGO_AUTORIZANTE,     A.DETALLE,     CO.NOMBRE AS CONCEPTO,CO.ID AS CONCEPTO_ID,     E.NOMBRE AS ESTADO,     E.ID AS ESTADO_ID,     A.EXPEDIENTE,     A.MONTO_AUTORIZADO,    A.MES,    A.AUTORIZACION_GG,     AR.NOMBRE AS AREA,     AR.ID AS AREA_ID,     C.ID AS CONTRATA_ID FROM     AUTORIZANTES AS A  INNER JOIN     OBRAS AS O ON A.OBRA = O.ID INNER JOIN     ESTADOS_AUTORIZANTES AS E ON A.ESTADO = E.ID INNER JOIN     CONTRATA AS C ON O.CONTRATA = C.ID LEFT JOIN     BD_PROYECTOS AS B ON O.ID = B.ID_BASE INNER JOIN     AREAS AS AR ON O.AREA = AR.ID     INNER JOIN EMPRESAS AS EM ON O.EMPRESA = EM.ID INNER JOIN BARRIOS AS BA ON O.BARRIO = BA.ID INNER JOIN CONCEPTOS AS CO ON A.CONCEPTO = CO.ID WHERE O.AREA = @area";

                if (!string.IsNullOrEmpty(estado))
                {
                    query += " AND E.NOMBRE = @estado";
                    datos.setearParametros("@estado", estado);
                }
                if (!string.IsNullOrEmpty(empresa))
                {
                    query += " AND EM.NOMBRE = @empresa";
                    datos.setearParametros("@empresa", empresa);
                }
                if (obra != 0)
                {
                    query += " AND O.ID = @obra";
                    datos.setearParametros("@obra", obra);
                }

                datos.setearConsulta(query);
                datos.agregarParametro("@area", usuario.Area.Id);

                datos.ejecutarLectura();
                while (datos.Lector.Read())
                {

                    Autorizante aux = new Autorizante();
                    aux.CodigoAutorizante = datos.Lector["CODIGO_AUTORIZANTE"] as string;
                    aux.Detalle = datos.Lector["DETALLE"] as string;
                    aux.Concepto = new Concepto
                    {
                        Nombre = datos.Lector["CONCEPTO"] as string,
                        Id = (int)datos.Lector["CONCEPTO_ID"]
                    };
                    aux.Empresa = datos.Lector["EMPRESA"] as string;
                    aux.Estado = new EstadoAutorizante
                    {
                        Nombre = datos.Lector["ESTADO"] as string,
                        Id = (int)datos.Lector["ESTADO_ID"]
                    };

                    aux.Expediente = datos.Lector["EXPEDIENTE"] as string;
                    aux.MontoAutorizado = datos.Lector["MONTO_AUTORIZADO"] != DBNull.Value ? (decimal)datos.Lector["MONTO_AUTORIZADO"] : 0M;
                    aux.AutorizacionGG = (bool)datos.Lector["AUTORIZACION_GG"];
                    aux.Fecha = datos.Lector["MES"] != DBNull.Value ? (DateTime)datos.Lector["MES"] : (DateTime?)null;


                    aux.Obra = new Obra
                    {
                        Id = (int)datos.Lector["OBRA_ID"],

                        Descripcion = datos.Lector["OBRA"] as string,
                        Contrata = new Contrata
                        {
                            Id = (int)datos.Lector["CONTRATA_ID"],
                            Nombre = datos.Lector["CONTRATA"] as string
                        },
                        Area = new Area
                        {
                            Id = (int)datos.Lector["AREA_ID"],
                            Nombre = datos.Lector["AREA"] as string
                        }
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
        public List<Autorizante> listar(string estado, string empresa, int obra, string area)
        {
            List<Autorizante> lista = new List<Autorizante>();
            AccesoDatos datos = new AccesoDatos();

            try
            {
                string query = "SELECT    CONCAT(C.NOMBRE, ' ', O.NUMERO, '/', O.AÑO) AS CONTRATA,    CONCAT(O.DESCRIPCION, ' - ', BA.NOMBRE ) AS OBRA, O.ID AS OBRA_ID,   EM.NOMBRE AS EMPRESA,    A.CODIGO_AUTORIZANTE,     A.DETALLE, CO.NOMBRE AS CONCEPTO,CO.ID AS CONCEPTO_ID,     E.NOMBRE AS ESTADO,     E.ID AS ESTADO_ID,     A.EXPEDIENTE,     A.MONTO_AUTORIZADO,    A.MES,    A.AUTORIZACION_GG,     AR.NOMBRE AS AREA,     AR.ID AS AREA_ID,     C.ID AS CONTRATA_ID FROM     AUTORIZANTES AS A  INNER JOIN     OBRAS AS O ON A.OBRA = O.ID INNER JOIN     ESTADOS_AUTORIZANTES AS E ON A.ESTADO = E.ID INNER JOIN     CONTRATA AS C ON O.CONTRATA = C.ID LEFT JOIN     BD_PROYECTOS AS B ON O.ID = B.ID_BASE INNER JOIN     AREAS AS AR ON O.AREA = AR.ID     INNER JOIN EMPRESAS AS EM ON O.EMPRESA = EM.ID INNER JOIN BARRIOS AS BA ON O.BARRIO = BA.ID INNER JOIN CONCEPTOS AS CO ON A.CONCEPTO = CO.ID ";
                if (!string.IsNullOrEmpty(estado))
                {
                    query += " AND E.NOMBRE = @estado";
                    datos.setearParametros("@estado", estado);
                }
                if (!string.IsNullOrEmpty(empresa))
                {
                    query += " AND EM.NOMBRE = @empresa";
                    datos.setearParametros("@empresa", empresa);
                }
                if (!string.IsNullOrEmpty(area))
                {
                    query += " AND AR.NOMBRE = @area";
                    datos.setearParametros("@area", area);
                }
                if (obra != 0)
                {
                    query += " AND O.ID = @obra";
                    datos.setearParametros("@obra", obra);
                }
                datos.setearConsulta(query);
                datos.ejecutarLectura();
                while (datos.Lector.Read())
                {

                    Autorizante aux = new Autorizante();
                    aux.CodigoAutorizante = datos.Lector["CODIGO_AUTORIZANTE"] as string;
                    aux.Detalle = datos.Lector["DETALLE"] as string;
                    aux.Concepto = new Concepto
                    {
                        Nombre = datos.Lector["CONCEPTO"] as string,
                        Id = (int)datos.Lector["CONCEPTO_ID"]
                    };
                    aux.Empresa = datos.Lector["EMPRESA"] as string;
                    aux.Estado = new EstadoAutorizante
                    {
                        Nombre = datos.Lector["ESTADO"] as string,
                        Id = (int)datos.Lector["ESTADO_ID"]
                    };

                    aux.Expediente = datos.Lector["EXPEDIENTE"] as string;
                    aux.MontoAutorizado = datos.Lector["MONTO_AUTORIZADO"] != DBNull.Value ? (decimal)datos.Lector["MONTO_AUTORIZADO"] : 0M;
                    aux.AutorizacionGG = (bool)datos.Lector["AUTORIZACION_GG"];
                    aux.Fecha = datos.Lector["MES"] != DBNull.Value ? (DateTime)datos.Lector["MES"] : (DateTime?)null;

                    aux.Obra = new Obra
                    {
                        Id = (int)datos.Lector["OBRA_ID"],
                        Descripcion = datos.Lector["OBRA"] as string,
                        Contrata = new Contrata
                        {
                            Id = (int)datos.Lector["CONTRATA_ID"],
                            Nombre = datos.Lector["CONTRATA"] as string
                        },

                        Area = new Area
                        {
                            Id = (int)datos.Lector["AREA_ID"],
                            Nombre = datos.Lector["AREA"] as string
                        }
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
        public List<Autorizante> listarPendientes(string estado, string empresa, int obra, string area)
        {
            List<Autorizante> lista = new List<Autorizante>();
            AccesoDatos datos = new AccesoDatos();

            try
            {
                string query = "SELECT    CONCAT(C.NOMBRE, ' ', O.NUMERO, '/', O.AÑO) AS CONTRATA,O.ID AS OBRA_ID,    CONCAT(O.DESCRIPCION, ' - ', BA.NOMBRE ) AS OBRA,    EM.NOMBRE AS EMPRESA,    A.CODIGO_AUTORIZANTE,     A.DETALLE,  CO.NOMBRE AS CONCEPTO,CO.ID AS CONCEPTO_ID,     E.NOMBRE AS ESTADO,     E.ID AS ESTADO_ID,     A.EXPEDIENTE,     A.MONTO_AUTORIZADO,    A.MES,    A.AUTORIZACION_GG,     AR.NOMBRE AS AREA,     AR.ID AS AREA_ID,     C.ID AS CONTRATA_ID FROM     AUTORIZANTES AS A  INNER JOIN     OBRAS AS O ON A.OBRA = O.ID INNER JOIN     ESTADOS_AUTORIZANTES AS E ON A.ESTADO = E.ID INNER JOIN     CONTRATA AS C ON O.CONTRATA = C.ID LEFT JOIN     BD_PROYECTOS AS B ON O.ID = B.ID_BASE INNER JOIN     AREAS AS AR ON O.AREA = AR.ID     INNER JOIN EMPRESAS AS EM ON O.EMPRESA = EM.ID INNER JOIN BARRIOS AS BA ON O.BARRIO = BA.ID INNER JOIN CONCEPTOS AS CO ON A.CONCEPTO = CO.ID where A.AUTORIZACION_GG = 0 ";
                if (!string.IsNullOrEmpty(estado))
                {
                    query += " AND E.NOMBRE = @estado";
                    datos.setearParametros("@estado", estado);
                }
                if (!string.IsNullOrEmpty(empresa))
                {
                    query += " AND EM.NOMBRE = @empresa";
                    datos.setearParametros("@empresa", empresa);
                }
                if (!string.IsNullOrEmpty(area))
                {
                    query += " AND AR.NOMBRE = @area";
                    datos.setearParametros("@area", area);
                }
                if (obra != 0)
                {
                    query += " AND O.ID = @obra";
                    datos.setearParametros("@obra", obra);
                }
                datos.setearConsulta(query);
                datos.ejecutarLectura();
                while (datos.Lector.Read())
                {

                    Autorizante aux = new Autorizante();
                    aux.CodigoAutorizante = datos.Lector["CODIGO_AUTORIZANTE"] as string;
                    aux.Detalle = datos.Lector["DETALLE"] as string;
                    aux.Concepto = new Concepto
                    {
                        Nombre = datos.Lector["CONCEPTO"] as string,
                        Id = (int)datos.Lector["CONCEPTO_ID"]
                    };
                    aux.Empresa = datos.Lector["EMPRESA"] as string;
                    aux.Estado = new EstadoAutorizante
                    {
                        Nombre = datos.Lector["ESTADO"] as string,
                        Id = (int)datos.Lector["ESTADO_ID"]
                    };

                    aux.Expediente = datos.Lector["EXPEDIENTE"] as string;
                    aux.MontoAutorizado = datos.Lector["MONTO_AUTORIZADO"] != DBNull.Value ? (decimal)datos.Lector["MONTO_AUTORIZADO"] : 0M;
                    aux.AutorizacionGG = (bool)datos.Lector["AUTORIZACION_GG"];
                    aux.Fecha = datos.Lector["MES"] != DBNull.Value ? (DateTime)datos.Lector["MES"] : (DateTime?)null;

                    aux.Obra = new Obra
                    {
                        Id = (int)datos.Lector["OBRA_ID"],

                        Descripcion = datos.Lector["OBRA"] as string,
                        Contrata = new Contrata
                        {
                            Id = (int)datos.Lector["CONTRATA_ID"],
                            Nombre = datos.Lector["CONTRATA"] as string
                        },

                        Area = new Area
                        {
                            Id = (int)datos.Lector["AREA_ID"],
                            Nombre = datos.Lector["AREA"] as string
                        }
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
        public bool agregar(Autorizante nuevoAutorizante)
        {
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta(@"
            INSERT INTO AUTORIZANTES 
            (OBRA, ESTADO, CONCEPTO, DETALLE, EXPEDIENTE, MONTO_AUTORIZADO,MES)
            VALUES 
            (@OBRA, @ESTADO, @CONCEPTO, @DETALLE, @EXPEDIENTE, @MONTO_AUTORIZADO, @MES)");

                datos.agregarParametro("@OBRA", nuevoAutorizante.Obra.Id);
                datos.agregarParametro("@ESTADO", nuevoAutorizante.Estado.Id);
                datos.agregarParametro("@CONCEPTO", nuevoAutorizante.Concepto.Id);
                datos.agregarParametro("@DETALLE", nuevoAutorizante.Detalle);
                datos.agregarParametro("@EXPEDIENTE", nuevoAutorizante.Expediente);
                datos.agregarParametro("@MONTO_AUTORIZADO", nuevoAutorizante.MontoAutorizado);
                datos.agregarParametro("@MES", nuevoAutorizante.Fecha);

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
        public DataTable listarddl(Usuario usuario)
        {
            DataTable dt = new DataTable();
            AccesoDatos datos = new AccesoDatos();

            try
            {
                if (usuario == null || usuario.Area == null)
                {
                    throw new ArgumentNullException("El usuario o su área no pueden ser nulos.");
                }

                datos.setearConsulta("SELECT A.ID, codigo_autorizante FROM AUTORIZANTES AS A INNER JOIN OBRAS AS O ON A.OBRA = O.ID WHERE O.AREA = @area and AUTORIZACION_GG = 1");
                datos.agregarParametro("@area", usuario.Area.Id);

                datos.ejecutarLectura();

                dt.Columns.Add("ID", typeof(int));
                dt.Columns.Add("NOMBRE", typeof(string));
                while (datos.Lector.Read())
                {
                    DataRow row = dt.NewRow();
                    row["ID"] = (int)datos.Lector["ID"];
                    row["NOMBRE"] = datos.Lector["codigo_autorizante"] as string;
                    dt.Rows.Add(row);
                }

                return dt;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Hubo un problema al obtener las obras para el usuario", ex);
            }
            finally
            {
                datos.cerrarConexion();
            }
        }
        public DataTable listarddl()
        {
            DataTable dt = new DataTable();
            AccesoDatos datos = new AccesoDatos();

            try
            {

                datos.setearConsulta("SELECT A.ID, codigo_autorizante FROM AUTORIZANTES AS A INNER JOIN OBRAS AS O ON A.OBRA = O.ID");


                datos.ejecutarLectura();
                dt.Columns.Add("ID", typeof(int));
                dt.Columns.Add("NOMBRE", typeof(string));

                while (datos.Lector.Read())
                {
                    DataRow row = dt.NewRow();
                    row["ID"] = (int)datos.Lector["ID"];
                    row["NOMBRE"] = datos.Lector["codigo_autorizante"] as string;

                    dt.Rows.Add(row);
                }

                return dt;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Hubo un problema al obtener las obras para el usuario", ex);
            }
            finally
            {
                datos.cerrarConexion();
            }
        }
        public bool modificar(Autorizante autorizante)
        {
            var datos = new AccesoDatos();

            try
            {
                datos.setearConsulta(@"
        UPDATE AUTORIZANTES 
        SET 
            OBRA = @obra, 
            CONCEPTO = @concepto, 
            DETALLE = @detalle, 
            MONTO_AUTORIZADO = @montoAutorizado, 
            MES = @mes,
AUTORIZACION_GG = @aut
        WHERE CODIGO_AUTORIZANTE = @codigoAutorizante");

                datos.agregarParametro("@obra", autorizante.Obra.Id);
                datos.agregarParametro("@concepto", autorizante.Concepto.Id);
                datos.agregarParametro("@detalle", autorizante.Detalle);
                datos.agregarParametro("@montoAutorizado", autorizante.MontoAutorizado);
                datos.agregarParametro("@mes", (object)autorizante.Fecha ?? DBNull.Value);
                datos.agregarParametro("@codigoAutorizante", autorizante.CodigoAutorizante);
                datos.agregarParametro("@aut", 0);

                datos.ejecutarAccion();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al modificar el autorizante.", ex);
            }
            finally
            {
                datos.cerrarConexion();
            }
        }
        public bool ActualizarEstado(Autorizante autorizante)
        {
            var datos = new AccesoDatos();

            try
            {
                datos.setearConsulta(@"
        UPDATE AUTORIZANTES 
        SET 
            ESTADO = @estado 
           WHERE CODIGO_AUTORIZANTE = @codigoAutorizante");

                datos.agregarParametro("@estado", autorizante.Estado.Id);
                datos.agregarParametro("@codigoAutorizante", autorizante.CodigoAutorizante);

                datos.ejecutarAccion();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al modificar el autorizante.", ex);
            }
            finally
            {
                datos.cerrarConexion();
            }
        }
        public bool ActualizarExpediente(string autorizante, string ex)
        {
            var datos = new AccesoDatos();

            try
            {
                datos.setearConsulta(@"
        UPDATE AUTORIZANTES 
        SET 
            EXPEDIENTE = @expediente
           WHERE CODIGO_AUTORIZANTE = @codigoAutorizante");

                datos.agregarParametro("@expediente", ex);
                datos.agregarParametro("@codigoAutorizante", autorizante);

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
        public bool modificarAdmin(Autorizante autorizante)
        {
            var datos = new AccesoDatos();

            try
            {
                datos.setearConsulta(@"
        UPDATE AUTORIZANTES 
        SET  
            ESTADO = @estado, 
            CONCEPTO = @concepto, 
            DETALLE = @detalle, 
            EXPEDIENTE = @expediente, 
            MONTO_AUTORIZADO = @montoAutorizado, 
            MES = @mes,
AUTORIZACION_GG = @aut
        WHERE CODIGO_AUTORIZANTE = @codigoAutorizante");

                datos.agregarParametro("@estado", autorizante.Estado.Id);
                datos.agregarParametro("@concepto", autorizante.Concepto);
                datos.agregarParametro("@detalle", autorizante.Detalle);
                datos.agregarParametro("@expediente", (object)autorizante.Expediente ?? DBNull.Value);
                datos.agregarParametro("@montoAutorizado", autorizante.MontoAutorizado);
                datos.agregarParametro("@mes", (object)autorizante.Fecha ?? DBNull.Value);
                datos.agregarParametro("@codigoAutorizante", autorizante.CodigoAutorizante);
                datos.agregarParametro("@aut", autorizante.AutorizacionGG);

                datos.ejecutarAccion();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al modificar el autorizante.", ex);
            }
            finally
            {
                datos.cerrarConexion();
            }
        }
    }
}
