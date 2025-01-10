using Dominio;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
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
                // Configurar la consulta para eliminar un registro por ID
                datos.setearConsulta("DELETE FROM AUTORIZANTES WHERE CODIGO_AUTORIZANTE = @ID");

                // Agregar el parámetro para el ID
                datos.agregarParametro("@ID", codigo);

                // Ejecutar la consulta
                datos.ejecutarAccion();

                // Si no hubo excepciones, asumimos que la operación fue exitosa
                return true;
            }
            catch (Exception ex)
            {
                // Propagar la excepción para que se maneje en el nivel superior
                throw new ApplicationException("Hubo un problema al intentar eliminar el autorizante.", ex);
            }
            finally
            {
                // Cerrar la conexión en el bloque finally
                datos.cerrarConexion();
            }
        }

        public List<Autorizante> listar(Usuario usuario)
        {
            List<Autorizante> lista = new List<Autorizante>();
            AccesoDatos datos = new AccesoDatos();

            try
            {
                datos.setearConsulta(@"
            SELECT 
                CONCAT(C.NOMBRE, ' ', O.NUMERO, '/', O.AÑO) AS CONTRATA, 
                O.DESCRIPCION AS OBRA, 
                A.CODIGO_AUTORIZANTE, 
                A.DETALLE, 
                A.CONCEPTO, 
                E.NOMBRE AS ESTADO, 
                E.ID AS ESTADO_ID, 
                A.EXPEDIENTE, 
                A.MONTO_AUTORIZADO,
                A.MES,
                A.AUTORIZACION_GG, 
                AR.NOMBRE AS AREA, 
                AR.ID AS AREA_ID, 
                C.ID AS CONTRATA_ID
            FROM 
                AUTORIZANTES AS A
            INNER JOIN 
                OBRAS AS O ON A.OBRA = O.ID
            INNER JOIN 
                ESTADOS_AUTORIZANTES AS E ON A.ESTADO = E.ID
            INNER JOIN 
                CONTRATA AS C ON O.CONTRATA = C.ID
            LEFT JOIN 
                BD_PROYECTOS AS B ON O.ID = B.ID_BASE
            INNER JOIN 
                AREAS AS AR ON O.AREA = AR.ID
            WHERE 
                O.AREA = @area");
                datos.agregarParametro("@area", usuario.Area.Id);
                datos.ejecutarLectura();
                while (datos.Lector.Read())
                {

                    Autorizante aux = new Autorizante();
                    aux.CodigoAutorizante = datos.Lector["CODIGO_AUTORIZANTE"] as string;
                    aux.Detalle = datos.Lector["DETALLE"] as string;
                    aux.Concepto = datos.Lector["CONCEPTO"] as string;
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
                        Descripcion = datos.Lector["OBRA"] as string,
                        Contrata = new Contrata
                        {
                            Id = (int)datos.Lector["CONTRATA_ID"],  // Asignamos el ID de la Contrata
                            Nombre = datos.Lector["CONTRATA"] as string  // Asignamos el nombre de la Contrata
                        },

                        // Asignamos el Area a la propiedad Obra
                        Area = new Area
                        {
                            Id = (int)datos.Lector["AREA_ID"],  // Asignamos el ID del Area
                            Nombre = datos.Lector["AREA"] as string  // Asignamos el nombre del Area
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

        public List<Autorizante> listar()
        {
            List<Autorizante> lista = new List<Autorizante>();
            AccesoDatos datos = new AccesoDatos();

            try
            {
                datos.setearConsulta(@"
            SELECT 
                CONCAT(C.NOMBRE, ' ', O.NUMERO, '/', O.AÑO) AS CONTRATA, 
                O.DESCRIPCION AS OBRA, 
                A.CODIGO_AUTORIZANTE, 
                A.DETALLE, 
                A.CONCEPTO, 
                E.NOMBRE AS ESTADO, 
                E.ID AS ESTADO_ID, 
                A.EXPEDIENTE, 
                A.MONTO_AUTORIZADO,
A.MES,
                A.AUTORIZACION_GG, 
                AR.NOMBRE AS AREA, 
                AR.ID AS AREA_ID, 
                C.ID AS CONTRATA_ID
            FROM 
                AUTORIZANTES AS A
            INNER JOIN 
                OBRAS AS O ON A.OBRA = O.ID
            INNER JOIN 
                ESTADOS_AUTORIZANTES AS E ON A.ESTADO = E.ID
            INNER JOIN 
                CONTRATA AS C ON O.CONTRATA = C.ID
            LEFT JOIN 
                BD_PROYECTOS AS B ON O.ID = B.ID_BASE
            INNER JOIN 
                AREAS AS AR ON O.AREA = AR.ID");
                datos.ejecutarLectura();
                while (datos.Lector.Read())
                {

                    Autorizante aux = new Autorizante();
                    aux.CodigoAutorizante = datos.Lector["CODIGO_AUTORIZANTE"] as string;
                    aux.Detalle = datos.Lector["DETALLE"] as string;
                    aux.Concepto = datos.Lector["CONCEPTO"] as string;
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
                        Descripcion = datos.Lector["OBRA"] as string,
                        Contrata = new Contrata
                        {
                            Id = (int)datos.Lector["CONTRATA_ID"],  // Asignamos el ID de la Contrata
                            Nombre = datos.Lector["CONTRATA"] as string  // Asignamos el nombre de la Contrata
                        },

                        // Asignamos el Area a la propiedad Obra
                        Area = new Area
                        {
                            Id = (int)datos.Lector["AREA_ID"],  // Asignamos el ID del Area
                            Nombre = datos.Lector["AREA"] as string  // Asignamos el nombre del Area
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
                // Consulta para insertar el nuevo autorizante
                datos.setearConsulta(@"
            INSERT INTO AUTORIZANTES 
            (OBRA, ESTADO, CONCEPTO, DETALLE, EXPEDIENTE, MONTO_AUTORIZADO,MES)
            VALUES 
            (@OBRA, @ESTADO, @CONCEPTO, @DETALLE, @EXPEDIENTE, @MONTO_AUTORIZADO, @MES)");

                // Asignar los parámetros
                datos.agregarParametro("@OBRA", nuevoAutorizante.Obra.Id);
                datos.agregarParametro("@ESTADO", nuevoAutorizante.Estado.Id);
                datos.agregarParametro("@CONCEPTO", nuevoAutorizante.Concepto);
                datos.agregarParametro("@DETALLE", nuevoAutorizante.Detalle);
                datos.agregarParametro("@EXPEDIENTE", nuevoAutorizante.Expediente);
                datos.agregarParametro("@MONTO_AUTORIZADO", nuevoAutorizante.MontoAutorizado);
                datos.agregarParametro("@MES", nuevoAutorizante.Fecha);

                // Ejecutar la inserción
                datos.ejecutarAccion();

                // Si todo fue bien, devolvemos true
                return true;
            }
            catch (Exception ex)
            {
                // En caso de error, lanzamos la excepción para que se maneje donde se llame el método
                throw ex;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }
        public DataTable listarddl(Usuario usuario)
        {
            DataTable dt = new DataTable(); // DataTable donde guardaremos las obras.
            AccesoDatos datos = new AccesoDatos();

            try
            {
                // Verificar si el usuario o el área del usuario son null
                if (usuario == null || usuario.Area == null)
                {
                    throw new ArgumentNullException("El usuario o su área no pueden ser nulos.");
                }

                // Consulta que solo devuelve las obras cuyo área coincida con la del usuario activo
                datos.setearConsulta("SELECT A.ID, codigo_autorizante FROM AUTORIZANTES AS A INNER JOIN OBRAS AS O ON A.OBRA = O.ID WHERE O.AREA = @area and AUTORIZACION_GG = 1");

                // Asignar el parámetro del área del usuario.
                datos.agregarParametro("@area", usuario.Area.Id);

                // Ejecutar la consulta.
                datos.ejecutarLectura();

                // Definir las columnas del DataTable.
                dt.Columns.Add("ID", typeof(int));    // Columna ID (para el valor de la obra)
                dt.Columns.Add("NOMBRE", typeof(string));  // Columna Descripción (para la opción a mostrar)

                while (datos.Lector.Read())
                {
                    // Crear una nueva fila y asignar los valores obtenidos de la base de datos.
                    DataRow row = dt.NewRow();
                    row["ID"] = (int)datos.Lector["ID"];  // Asignar el ID de la obra
                    row["NOMBRE"] = datos.Lector["codigo_autorizante"] as string;  // Asignar la descripción de la obra

                    // Agregar la fila al DataTable.
                    dt.Rows.Add(row);
                }

                return dt;  // Devolvemos el DataTable con los datos
            }
            catch (Exception ex)
            {
                // Proveer información más detallada en el caso de un error
                throw new ApplicationException("Hubo un problema al obtener las obras para el usuario", ex);
            }
            finally
            {
                // Asegurarnos de cerrar la conexión después de usarla.
                datos.cerrarConexion();
            }
        }
        public DataTable listarddl()
        {
            DataTable dt = new DataTable(); // DataTable donde guardaremos las obras.
            AccesoDatos datos = new AccesoDatos();

            try
            {
          

                // Consulta que solo devuelve las obras cuyo área coincida con la del usuario activo
                datos.setearConsulta("SELECT A.ID, codigo_autorizante FROM AUTORIZANTES AS A INNER JOIN OBRAS AS O ON A.OBRA = O.ID");

          
                // Ejecutar la consulta.
                datos.ejecutarLectura();

                // Definir las columnas del DataTable.
                dt.Columns.Add("ID", typeof(int));    // Columna ID (para el valor de la obra)
                dt.Columns.Add("NOMBRE", typeof(string));  // Columna Descripción (para la opción a mostrar)

                while (datos.Lector.Read())
                {
                    // Crear una nueva fila y asignar los valores obtenidos de la base de datos.
                    DataRow row = dt.NewRow();
                    row["ID"] = (int)datos.Lector["ID"];  // Asignar el ID de la obra
                    row["NOMBRE"] = datos.Lector["codigo_autorizante"] as string;  // Asignar la descripción de la obra

                    // Agregar la fila al DataTable.
                    dt.Rows.Add(row);
                }

                return dt;  // Devolvemos el DataTable con los datos
            }
            catch (Exception ex)
            {
                // Proveer información más detallada en el caso de un error
                throw new ApplicationException("Hubo un problema al obtener las obras para el usuario", ex);
            }
            finally
            {
                // Asegurarnos de cerrar la conexión después de usarla.
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
            ESTADO = @estado, 
            CONCEPTO = @concepto, 
            DETALLE = @detalle, 
            EXPEDIENTE = @expediente, 
            MONTO_AUTORIZADO = @montoAutorizado, 
            MES = @mes
        WHERE CODIGO_AUTORIZANTE = @codigoAutorizante");

                // Asignar parámetros
                datos.agregarParametro("@obra", autorizante.Obra.Id);
                datos.agregarParametro("@estado", autorizante.Estado.Id);
                datos.agregarParametro("@concepto", autorizante.Concepto);
                datos.agregarParametro("@detalle", autorizante.Detalle);
                datos.agregarParametro("@expediente", (object)autorizante.Expediente ?? DBNull.Value);
                datos.agregarParametro("@montoAutorizado", autorizante.MontoAutorizado);
                datos.agregarParametro("@mes", (object)autorizante.Fecha ?? DBNull.Value);
                datos.agregarParametro("@codigoAutorizante", autorizante.CodigoAutorizante);

                // Ejecutar la actualización
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
        public bool modificarAdmin(Autorizante autorizante)
        {
            var datos = new AccesoDatos();

            try
            {
                datos.setearConsulta(@"
        UPDATE AUTORIZANTES 
        SET 
            OBRA = @obra, 
            ESTADO = @estado, 
            CONCEPTO = @concepto, 
            DETALLE = @detalle, 
            EXPEDIENTE = @expediente, 
            MONTO_AUTORIZADO = @montoAutorizado, 
            MES = @mes,
AUTORIZACION_GG = @aut
        WHERE CODIGO_AUTORIZANTE = @codigoAutorizante");

                // Asignar parámetros
                datos.agregarParametro("@obra", autorizante.Obra.Id);
                datos.agregarParametro("@estado", autorizante.Estado.Id);
                datos.agregarParametro("@concepto", autorizante.Concepto);
                datos.agregarParametro("@detalle", autorizante.Detalle);
                datos.agregarParametro("@expediente", (object)autorizante.Expediente ?? DBNull.Value);
                datos.agregarParametro("@montoAutorizado", autorizante.MontoAutorizado);
                datos.agregarParametro("@mes", (object)autorizante.Fecha ?? DBNull.Value);
                datos.agregarParametro("@codigoAutorizante", autorizante.CodigoAutorizante);
                datos.agregarParametro("@aut", autorizante.AutorizacionGG);

                // Ejecutar la actualización
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
