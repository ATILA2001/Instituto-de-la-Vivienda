using Dominio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Negocio
{
    public class ObraNegocio
    {
        public List<Obra> listar(Usuario usuario, List<string> barrios, List<string> empresas, string filtro = null)
        {
            List<Obra> lista = new List<Obra>();
            AccesoDatos datos = new AccesoDatos();

            try
            {

                string query = "SELECT O.ID, A.NOMBRE AS AREA, E.NOMBRE AS EMPRESA, NUMERO, C.NOMBRE AS CONTRATA, AÑO, ETAPA, OBRA, B.NOMBRE AS BARRIO, DESCRIPCION, BD.AUTORIZADO_INICIAL, BD.AUTORIZADO_NUEVO, (SELECT SUM(C.MONTO_TOTAL) FROM CERTIFICADOS AS C INNER JOIN AUTORIZANTES AS A2 ON C.codigo_autorizante = A2.codigo_autorizante WHERE A2.OBRA = O.ID AND YEAR(C.MES_APROBACION) = 2025) AS MONTO_CERTIFICADO, (SELECT SUM(A.MONTO_AUTORIZADO) FROM AUTORIZANTES AS A WHERE A.OBRA = O.ID) AS SUMA_AUTORIZANTE, CASE WHEN BD.AUTORIZADO_NUEVO IS NOT NULL AND BD.AUTORIZADO_NUEVO > 0 THEN ((SELECT SUM(C.MONTO_TOTAL) FROM CERTIFICADOS AS C INNER JOIN AUTORIZANTES AS A2 ON C.codigo_autorizante = A2.codigo_autorizante WHERE A2.OBRA = O.ID AND YEAR(C.MES_APROBACION) = 2025) / BD.AUTORIZADO_NUEVO) * 100 ELSE NULL END AS PORCENTAJE FROM OBRAS AS O INNER JOIN EMPRESAS AS E ON O.EMPRESA = E.ID INNER JOIN AREAS AS A ON O.AREA = A.ID INNER JOIN CONTRATA AS C ON O.CONTRATA = C.ID INNER JOIN BARRIOS AS B ON O.BARRIO = B.ID LEFT JOIN BD_PROYECTOS AS BD ON O.ID = BD.ID_BASE WHERE O.AREA = @area ";
                if (empresas != null && empresas.Count > 0)
                {
                    string empresasParam = string.Join(",", empresas.Select((e, i) => $"@empresa{i}"));
                    query += $" AND E.NOMBRE IN ({empresasParam})";
                    for (int i = 0; i < empresas.Count; i++)
                    {
                        datos.setearParametros($"@empresa{i}", empresas[i]);
                    }
                }

                if (barrios != null && barrios.Count > 0)
                {
                    string barriosParam = string.Join(",", barrios.Select((e, i) => $"@barrio{i}"));
                    query += $" AND B.NOMBRE IN ({barriosParam})";
                    for (int i = 0; i < barrios.Count; i++)
                    {
                        datos.setearParametros($"@barrio{i}", barrios[i]);
                    }
                }
                if (!string.IsNullOrEmpty(filtro))
                {
                    query += " AND (E.NOMBRE LIKE @filtro OR NUMERO LIKE @filtro OR C.NOMBRE LIKE @filtro OR AÑO LIKE @filtro OR ETAPA LIKE @filtro OR OBRA LIKE @filtro OR B.NOMBRE LIKE @filtro OR DESCRIPCION LIKE @filtro) ";
                    datos.setearParametros("@filtro", $"%{filtro}%");
                }


                datos.setearConsulta(query);
                datos.agregarParametro("@area", usuario.Area.Id);
                datos.ejecutarLectura();
                while (datos.Lector.Read())
                {

                    Obra aux = new Obra();
                    aux.Id = (int)datos.Lector["ID"];
                    aux.Descripcion = datos.Lector["DESCRIPCION"] as string;
                    aux.Numero = datos.Lector["NUMERO"] as int?;
                    aux.Año = datos.Lector["AÑO"] as int?;
                    aux.Etapa = datos.Lector["ETAPA"] as int?;
                    aux.ObraNumero = datos.Lector["OBRA"] as int?;
                    aux.AutorizadoInicial = datos.Lector["AUTORIZADO_INICIAL"] as decimal?;
                    aux.AutorizadoNuevo = datos.Lector["AUTORIZADO_NUEVO"] as decimal?;
                    aux.MontoCertificado = datos.Lector["MONTO_CERTIFICADO"] as decimal?;
                    aux.MontoAutorizante = datos.Lector["SUMA_AUTORIZANTE"] as decimal?;
                    aux.Porcentaje = datos.Lector["PORCENTAJE"] as decimal?;


                    aux.Barrio = new Barrio
                    {
                        Id = (int)datos.Lector["ID"],
                        Nombre = datos.Lector["BARRIO"] as string
                    };

                    aux.Area = new Area
                    {
                        Id = (int)datos.Lector["ID"],
                        Nombre = datos.Lector["AREA"] as string
                    };

                    aux.Empresa = new Empresa
                    {
                        Id = (int)datos.Lector["ID"],
                        Nombre = datos.Lector["EMPRESA"] as string
                    };

                    aux.Contrata = new Contrata
                    {
                        Id = (int)datos.Lector["ID"],
                        Nombre = datos.Lector["CONTRATA"] as string
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
        public List<Obra> listar(List<string> barrios, List<string> empresas, List<string> areas, string filtro = null)
        {
            List<Obra> lista = new List<Obra>();
            AccesoDatos datos = new AccesoDatos();

            try
            {
                string query = "SELECT O.ID, A.NOMBRE AS AREA, E.NOMBRE AS EMPRESA, NUMERO, C.NOMBRE AS CONTRATA, AÑO, ETAPA, OBRA, B.NOMBRE AS BARRIO, DESCRIPCION, BD.AUTORIZADO_INICIAL, BD.AUTORIZADO_NUEVO,(SELECT SUM(C.MONTO_TOTAL) FROM CERTIFICADOS AS C INNER JOIN AUTORIZANTES AS A2 ON C.codigo_autorizante = A2.codigo_autorizante WHERE A2.OBRA = O.ID AND YEAR(C.MES_APROBACION) = 2025) AS MONTO_CERTIFICADO, (SELECT SUM(A.MONTO_AUTORIZADO) FROM AUTORIZANTES AS A WHERE A.OBRA = O.ID) AS SUMA_AUTORIZANTE, CASE WHEN BD.AUTORIZADO_NUEVO IS NOT NULL AND BD.AUTORIZADO_NUEVO > 0 THEN ((SELECT SUM(C.MONTO_TOTAL) FROM CERTIFICADOS AS C INNER JOIN AUTORIZANTES AS A2 ON C.codigo_autorizante = A2.codigo_autorizante WHERE A2.OBRA = O.ID AND YEAR(C.MES_APROBACION) = 2025 ) / BD.AUTORIZADO_NUEVO) * 100 ELSE NULL END AS PORCENTAJE FROM OBRAS AS O INNER JOIN EMPRESAS AS E ON O.EMPRESA = E.ID INNER JOIN AREAS AS A ON O.AREA = A.ID INNER JOIN CONTRATA AS C ON O.CONTRATA = C.ID INNER JOIN BARRIOS AS B ON O.BARRIO = B.ID LEFT JOIN BD_PROYECTOS AS BD ON O.ID = BD.ID_BASE where 1=1 ";
                if (empresas != null && empresas.Count > 0)
                {
                    string empresasParam = string.Join(",", empresas.Select((e, i) => $"@empresa{i}"));
                    query += $" AND E.NOMBRE IN ({empresasParam})";
                    for (int i = 0; i < empresas.Count; i++)
                    {
                        datos.setearParametros($"@empresa{i}", empresas[i]);
                    }
                }

                if (barrios != null && barrios.Count > 0)
                {
                    string barriosParam = string.Join(",", barrios.Select((e, i) => $"@barrio{i}"));
                    query += $" AND B.NOMBRE IN ({barriosParam})";
                    for (int i = 0; i < barrios.Count; i++)
                    {
                        datos.setearParametros($"@barrio{i}", barrios[i]);
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


                if (!string.IsNullOrEmpty(filtro))
                {
                    query += " AND (E.NOMBRE LIKE @filtro OR NUMERO LIKE @filtro OR C.NOMBRE LIKE @filtro OR AÑO LIKE @filtro OR ETAPA LIKE @filtro OR OBRA LIKE @filtro OR B.NOMBRE LIKE @filtro OR DESCRIPCION LIKE @filtro) ";
                    datos.setearParametros("@filtro", $"%{filtro}%");
                }

                query += " ORDER BY DESCRIPCION";
                datos.setearConsulta(query);
                datos.ejecutarLectura();
                while (datos.Lector.Read())
                {

                    Obra aux = new Obra();
                    aux.Id = (int)datos.Lector["ID"];
                    aux.Descripcion = datos.Lector["DESCRIPCION"] as string;
                    aux.Numero = datos.Lector["NUMERO"] as int?;
                    aux.Año = datos.Lector["AÑO"] as int?;
                    aux.Etapa = datos.Lector["ETAPA"] as int?;
                    aux.ObraNumero = datos.Lector["OBRA"] as int?;
                    aux.AutorizadoInicial = datos.Lector["AUTORIZADO_INICIAL"] as decimal?;
                    aux.AutorizadoNuevo = datos.Lector["AUTORIZADO_NUEVO"] as decimal?;
                    aux.MontoCertificado = datos.Lector["MONTO_CERTIFICADO"] as decimal?;
                    aux.MontoAutorizante = datos.Lector["SUMA_AUTORIZANTE"] as decimal?;
                    aux.Porcentaje = datos.Lector["PORCENTAJE"] as decimal?;

                    aux.Barrio = new Barrio
                    {
                        Id = (int)datos.Lector["ID"],
                        Nombre = datos.Lector["BARRIO"] as string
                    };

                    aux.Area = new Area
                    {
                        Id = (int)datos.Lector["ID"],
                        Nombre = datos.Lector["AREA"] as string
                    };

                    aux.Empresa = new Empresa
                    {
                        Id = (int)datos.Lector["ID"],
                        Nombre = datos.Lector["EMPRESA"] as string
                    };

                    aux.Contrata = new Contrata
                    {
                        Id = (int)datos.Lector["ID"],
                        Nombre = datos.Lector["CONTRATA"] as string
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
        public bool agregar(Obra nuevaObra)
        {
            AccesoDatos datos = new AccesoDatos();
            try
            {
                // Consulta para insertar la nueva obra
                datos.setearConsulta("INSERT INTO OBRAS (EMPRESA, AREA, CONTRATA, NUMERO, AÑO, ETAPA, OBRA, BARRIO, DESCRIPCION) " +
                                     "VALUES (@EMPRESA, @AREA, @CONTRATA, @NUMERO, @AÑO, @ETAPA, @OBRA, @BARRIO, @DESCRIPCION)");

                // Asignar los parámetros
                datos.agregarParametro("@EMPRESA", nuevaObra.Empresa.Id);
                datos.agregarParametro("@AREA", nuevaObra.Area.Id);
                datos.agregarParametro("@CONTRATA", nuevaObra.Contrata.Id);
                datos.agregarParametro("@NUMERO", nuevaObra.Numero);
                datos.agregarParametro("@AÑO", nuevaObra.Año);
                datos.agregarParametro("@ETAPA", nuevaObra.Etapa);
                datos.agregarParametro("@OBRA", nuevaObra.ObraNumero);
                datos.agregarParametro("@BARRIO", nuevaObra.Barrio.Id);
                datos.agregarParametro("@DESCRIPCION", nuevaObra.Descripcion);

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
        public bool eliminar(int idObra)
        {
            AccesoDatos datos = new AccesoDatos();
            try
            {
                // Consulta para eliminar la obra por su ID
                datos.setearConsulta("DELETE FROM OBRAS WHERE ID = @ID");

                // Asignar el parámetro ID
                datos.agregarParametro("@ID", idObra);

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
        public bool modificar(Obra obraModificada)
        {
            AccesoDatos datos = new AccesoDatos();
            try
            {
                // Consulta para actualizar una obra existente
                datos.setearConsulta("UPDATE OBRAS SET " +
                                     "EMPRESA = @EMPRESA, " +
                                     "CONTRATA = @CONTRATA, " +
                                     "NUMERO = @NUMERO, " +
                                     "AÑO = @AÑO, " +
                                     "ETAPA = @ETAPA, " +
                                     "OBRA = @OBRA, " +
                                     "BARRIO = @BARRIO, " +
                                     "DESCRIPCION = @DESCRIPCION " +
                                     "WHERE ID = @ID");

                // Asignar los parámetros
                datos.agregarParametro("@EMPRESA", obraModificada.Empresa.Id);
                datos.agregarParametro("@CONTRATA", obraModificada.Contrata.Id);
                datos.agregarParametro("@NUMERO", obraModificada.Numero);
                datos.agregarParametro("@AÑO", obraModificada.Año);
                datos.agregarParametro("@ETAPA", obraModificada.Etapa);
                datos.agregarParametro("@OBRA", obraModificada.ObraNumero);
                datos.agregarParametro("@BARRIO", obraModificada.Barrio.Id);
                datos.agregarParametro("@DESCRIPCION", obraModificada.Descripcion);
                datos.agregarParametro("@ID", obraModificada.Id);

                // Ejecutar la actualización
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
                datos.setearConsulta("SELECT O.ID, CONCAT( B.NOMBRE, ' - ' , O.DESCRIPCION,' - ', C.NOMBRE, ' - ', O.NUMERO, '/', O.AÑO) AS NOMBRE " +
                                     "FROM OBRAS AS O " +
                                     "INNER JOIN BARRIOS AS B ON O.BARRIO = B.ID INNER JOIN CONTRATA AS C ON O.CONTRATA = C.ID " +
                                     "WHERE O.AREA = @area order by NOMBRE"); // Se filtra por el área del usuario activo

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
                    row["NOMBRE"] = datos.Lector["NOMBRE"] as string;  // Asignar la descripción de la obra

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


                datos.setearConsulta("SELECT O.ID, CONCAT( B.NOMBRE, ' - ' , O.DESCRIPCION,' - ', C.NOMBRE, ' - ', O.NUMERO, '/', O.AÑO) AS NOMBRE " +
                                     "FROM OBRAS AS O " +
                                     "INNER JOIN BARRIOS AS B ON O.BARRIO = B.ID INNER JOIN CONTRATA AS C ON O.CONTRATA = C.ID order  by NOMBRE "); // Se filtra por el área del usuario activo


                datos.ejecutarLectura();

                // Definir las columnas del DataTable.
                dt.Columns.Add("ID", typeof(int));    // Columna ID (para el valor de la obra)
                dt.Columns.Add("NOMBRE", typeof(string));  // Columna Descripción (para la opción a mostrar)

                while (datos.Lector.Read())
                {
                    // Crear una nueva fila y asignar los valores obtenidos de la base de datos.
                    DataRow row = dt.NewRow();
                    row["ID"] = (int)datos.Lector["ID"];  // Asignar el ID de la obra
                    row["NOMBRE"] = datos.Lector["NOMBRE"] as string;  // Asignar la descripción de la obra

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
        public DataTable listarddlProyecto()
        {
            DataTable dt = new DataTable(); // DataTable donde guardaremos las obras.
            AccesoDatos datos = new AccesoDatos();

            try
            {


                // Consulta que solo devuelve las obras cuyo área coincida con la del usuario activo
                datos.setearConsulta("SELECT O.ID, CONCAT( B.NOMBRE, ' - ' , O.DESCRIPCION,' - ', C.NOMBRE, ' - ', O.NUMERO, '/', O.AÑO) AS NOMBRE FROM OBRAS AS O INNER JOIN BARRIOS AS B ON O.BARRIO = B.ID INNER JOIN CONTRATA AS C ON O.CONTRATA = C.ID WHERE O.ID NOT IN (SELECT ID_BASE FROM BD_PROYECTOS) order by NOMBRE");


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
                    row["NOMBRE"] = datos.Lector["NOMBRE"] as string;  // Asignar la descripción de la obra

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
    }
}
