using Dominio;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negocio
{
    public class LineaGestionNegocio
    {
        public DataTable listarddl()
        {
            DataTable dt = new DataTable(); // DataTable donde guardaremos las empresas.
            AccesoDatos datos = new AccesoDatos();

            try
            {
                datos.setearConsulta("SELECT * FROM LINEA_DE_GESTION");
                datos.ejecutarLectura();

                // Definir las columnas del DataTable.
                dt.Columns.Add("ID");
                dt.Columns.Add("NOMBRE");

                while (datos.Lector.Read())
                {
                    // Crear una nueva fila y asignar los valores obtenidos.
                    DataRow row = dt.NewRow();
                    row["ID"] = (int)datos.Lector["ID"];
                    row["NOMBRE"] = (string)datos.Lector["NOMBRE"];

                    // Agregar la fila al DataTable.
                    dt.Rows.Add(row);
                }

                return dt;
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
        public List<LineaGestion> listar()
        {
            List<LineaGestion> lista = new List<LineaGestion>();
            AccesoDatos datos = new AccesoDatos();

            try
            {
                datos.setearConsulta("SELECT * FROM LINEA_DE_GESTION");
                datos.ejecutarLectura();
                while (datos.Lector.Read())
                {

                    LineaGestion aux = new LineaGestion();
                    aux.Id = datos.Lector["ID"] != DBNull.Value ? (int)datos.Lector["ID"] : 0; // Puede ser 0 o algún valor predeterminado
                    aux.Nombre = datos.Lector["NOMBRE"] != DBNull.Value ? (string)datos.Lector["NOMBRE"] : string.Empty; // Si es nulo, asignamos una cadena vacía
                    aux.Tipo = datos.Lector["TIPO"] != DBNull.Value ? (string)datos.Lector["TIPO"] : string.Empty;
                    aux.Grupo = datos.Lector["GRUPO"] != DBNull.Value ? (string)datos.Lector["GRUPO"] : string.Empty;
                    aux.Reparticion = datos.Lector["REPARTICION"] != DBNull.Value ? (string)datos.Lector["REPARTICION"] : string.Empty;


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
        public void agregar(LineaGestion linea)
        {
            AccesoDatos datos = new AccesoDatos();

            try
            {
                // Comando para insertar una nueva LineaGestion
                string consulta = "INSERT INTO LINEA_DE_GESTION (NOMBRE, TIPO, GRUPO, REPARTICION) " +
                                  "VALUES (@NOMBRE, @TIPO, @GRUPO, @REPARTICION)";

                // Configuración de la consulta con parámetros para evitar inyecciones SQL
                datos.setearConsulta(consulta);
                datos.agregarParametro("@NOMBRE", linea.Nombre);
                datos.agregarParametro("@TIPO", linea.Tipo);
                datos.agregarParametro("@GRUPO", linea.Grupo);
                datos.agregarParametro("@REPARTICION", linea.Reparticion);

                // Ejecutar la inserción
                datos.ejecutarAccion();
            }
            catch (Exception ex)
            {
                throw ex; // Lanza la excepción si ocurre algún error
            }
            finally
            {
                datos.cerrarConexion(); // Cerrar la conexión siempre
            }
        }
    }
}
