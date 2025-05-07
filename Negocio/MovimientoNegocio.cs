using Dominio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Negocio
{
    public class MovimientoNegocio
    {
        public List<Movimiento> listar(List<string> obras, string filtro = null)
        {
            List<Movimiento> lista = new List<Movimiento>();
            AccesoDatos datos = new AccesoDatos();

            try
            {
                string query = "select M.ID,CONCAT(O.DESCRIPCION, ' - ', BA.NOMBRE) AS OBRA, BD.PROYECTO, BD.SUBPROYECTO,LG.NOMBRE as LINEA, M.MOVIMIENTO,M.FECHA," +
                    "                    BD.AUTORIZADO_INICIAL + ISNULL(" +
                    "    (SELECT SUM(M2.MOVIMIENTO)" +
                    "       FROM MOVIMIENTOS_GESTION M2" +
                    "      WHERE M2.ID_BASE = M.ID_BASE), 0" +
                    ") AS AUTORIZADO_NUEVO" +
                    "                     from MOVIMIENTOS_GESTION M" +
                    "                    INNER JOIN OBRAS O ON M.ID_BASE = O.ID" +
                    "                    inner join BD_PROYECTOS BD on O.ID = BD.ID_BASE" +
                    "                    inner join BARRIOS BA on O.BARRIO = BA.ID                    inner join LINEA_DE_GESTION LG on BD.LINEA_DE_GESTION = LG.ID WHERE 1 = 1 ";

                if (obras != null && obras.Count > 0)
                {
                    string obraParam = string.Join(",", obras.Select((e, i) => $"@obra{i}"));
                    query += $" AND O.DESCRIPCION IN ({obraParam})";
                    for (int i = 0; i < obras.Count; i++)
                    {
                        datos.setearParametros($"@obra{i}", obras[i]);
                    }
                }

                if (!string.IsNullOrEmpty(filtro))
                {
                    query += " AND (E.NOMBRE LIKE @filtro OR NUMERO LIKE @filtro OR C.NOMBRE LIKE @filtro OR AÑO LIKE @filtro OR ETAPA LIKE @filtro OR OBRA LIKE @filtro OR B.NOMBRE LIKE @filtro OR DESCRIPCION LIKE @filtro) ";
                    datos.setearParametros("@filtro", $"%{filtro}%");
                }

                query += " ORDER BY ID";
                datos.setearConsulta(query);
                datos.ejecutarLectura();
                while (datos.Lector.Read())
                {
                    Movimiento aux = new Movimiento();
                    aux.Id = (int)datos.Lector["ID"];
                    aux.Obra = new Obra(); // Initialize the Obra property
                    aux.Obra.Descripcion = datos.Lector["OBRA"] as string;
                    aux.Monto = (decimal)datos.Lector["MOVIMIENTO"];
                    aux.Fecha = (DateTime)datos.Lector["FECHA"];
                    aux.AutorizadoNuevo = (decimal)datos.Lector["AUTORIZADO_NUEVO"];
                    aux.Proyecto = datos.Lector["PROYECTO"] as string;
                    aux.SubProyecto = datos.Lector["SUBPROYECTO"] as string;
                    aux.Linea = datos.Lector["LINEA"] as string;

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
        public bool agregar(Movimiento movimiento)
        {
            AccesoDatos datos = new AccesoDatos();
            try
            {
                // Consulta para insertar la nueva obra
                datos.setearConsulta("INSERT INTO MOVIMIENTOS_GESTION (ID_BASE, MOVIMIENTO, FECHA) " +
                                     "VALUES (@OBRA, @MOVIMIENTO, @FECHA)");

                // Asignar los parámetros
                datos.agregarParametro("@OBRA", movimiento.Obra.Id);
                datos.agregarParametro("@MOVIMIENTO", movimiento.Monto);
                datos.agregarParametro("@FECHA", movimiento.Fecha);
               

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
        public bool eliminar(int idMovimiento)
        {
            AccesoDatos datos = new AccesoDatos();
            try
            {
                // Consulta para eliminar la obra por su ID
                datos.setearConsulta("DELETE FROM MOVIMIENTOS_GESTION WHERE ID = @ID");

                // Asignar el parámetro ID
                datos.agregarParametro("@ID", idMovimiento);

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
        public bool modificar(Movimiento movimientoModificado)
        {
            AccesoDatos datos = new AccesoDatos();
            try
            {
                // Consulta para actualizar un movimiento existente
                datos.setearConsulta("UPDATE MOVIMIENTOS_GESTION SET " +
                                     "ID_BASE = @OBRA, " +
                                     "MOVIMIENTO = @MOVIMIENTO, " +
                                     "FECHA = @FECHA " +
                                     "WHERE ID = @ID");

                // Asignar los parámetros
                datos.agregarParametro("@OBRA", movimientoModificado.Obra.Id);
                datos.agregarParametro("@MOVIMIENTO", movimientoModificado.Monto);
                datos.agregarParametro("@FECHA", movimientoModificado.Fecha);
                datos.agregarParametro("@ID", movimientoModificado.Id);

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
    }
}
