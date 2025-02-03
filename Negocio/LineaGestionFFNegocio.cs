using Dominio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negocio
{
    public class LineaGestionFFNegocio
    {
        public List<LineaGestionFF> listar()
        {
            List<LineaGestionFF> lista = new List<LineaGestionFF>();
            AccesoDatos datos = new AccesoDatos();

            try
            {
                datos.setearConsulta(@" SELECT lf.ID AS ID_FF, lf.NOMBRE_FF, lf.FUENTE, l.ID AS ID_LINEA, l.NOMBRE AS NOMBRE_LINEA, l.TIPO, l.GRUPO, l.REPARTICION FROM LINEA_DE_GESTION_FF AS lf INNER JOIN LINEA_DE_GESTION AS l ON lf.ID_LINEA_GESTION = l.ID ORDER BY lf.NOMBRE_FF ");
                datos.ejecutarLectura();

                while (datos.Lector.Read())
                {
                    LineaGestionFF aux = new LineaGestionFF
                    {
                        Id = Convert.IsDBNull(datos.Lector["ID_FF"]) ? 0 : (int)datos.Lector["ID_FF"],
                        Nombre = Convert.IsDBNull(datos.Lector["NOMBRE_FF"]) ? string.Empty : (string)datos.Lector["NOMBRE_FF"],
                        Fuente = Convert.IsDBNull(datos.Lector["FUENTE"]) ? string.Empty : (string)datos.Lector["FUENTE"],
                        LineaGestion = new LineaGestion
                        {
                            Id = Convert.IsDBNull(datos.Lector["ID_LINEA"]) ? 0 : (int)datos.Lector["ID_LINEA"],
                            Nombre = Convert.IsDBNull(datos.Lector["NOMBRE_LINEA"]) ? string.Empty : (string)datos.Lector["NOMBRE_LINEA"],
                            Tipo = Convert.IsDBNull(datos.Lector["TIPO"]) ? string.Empty : (string)datos.Lector["TIPO"],
                            Grupo = Convert.IsDBNull(datos.Lector["GRUPO"]) ? string.Empty : (string)datos.Lector["GRUPO"],
                            Reparticion = Convert.IsDBNull(datos.Lector["REPARTICION"]) ? string.Empty : (string)datos.Lector["REPARTICION"]
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
        public void agregar(LineaGestionFF lineaFF)
        {
            AccesoDatos datos = new AccesoDatos();

            try
            {
                // Comando para insertar una nueva LineaGestionFF
                string consulta = "INSERT INTO LINEA_DE_GESTION_FF (ID_LINEA_GESTION, NOMBRE_FF, FUENTE) " +
                                  "VALUES (@ID_LINEA_GESTION, @NOMBRE_FF, @FUENTE)";

                // Configuración de la consulta con parámetros para evitar inyecciones SQL
                datos.setearConsulta(consulta);
                datos.agregarParametro("@ID_LINEA_GESTION", lineaFF.LineaGestion.Id); // Clave foránea
                datos.agregarParametro("@NOMBRE_FF", lineaFF.Nombre);
                datos.agregarParametro("@FUENTE", lineaFF.Fuente);

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
