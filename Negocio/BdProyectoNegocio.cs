using Dominio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negocio
{
    public class BdProyectoNegocio
    {

        public bool modificar(BdProyecto proyecto)
        {
            var datos = new AccesoDatos();

            try
            {
                // Consulta SQL para actualizar un proyecto en la base de datos
                datos.setearConsulta(@"
        UPDATE BD_PROYECTOS
        SET 
            ID_BASE = @idBase, 
            SUBPROYECTO = @subProyecto, 
            PROYECTO = @proyecto, 
            LINEA_DE_GESTION = @lineaGestion, 
            AUTORIZADO_INICIAL = @autorizadoInicial
        WHERE ID = @id");

                // Asignar valores a los parámetros de la consulta
                datos.agregarParametro("@idBase", proyecto.Obra.Id);
                datos.agregarParametro("@subProyecto", proyecto.SubProyecto);
                datos.agregarParametro("@proyecto", proyecto.Proyecto);
                datos.agregarParametro("@lineaGestion", proyecto.LineaGestion.Id);
                datos.agregarParametro("@autorizadoInicial", proyecto.AutorizadoInicial);
                datos.agregarParametro("@id", proyecto.Id);

                // Ejecutar la consulta SQL
                datos.ejecutarAccion();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al modificar el proyecto.", ex);
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        public List<BdProyecto> Listar()
        {
            List<BdProyecto> lista = new List<BdProyecto>();
            AccesoDatos datos = new AccesoDatos(); // Se usa la clase para manejar la conexión

            try
            {
                // La consulta SQL que une las tablas de la base de datos.
                string query = @"
                SELECT 
                    BD.ID,
                    CONCAT(C.NOMBRE, ' ', O.NUMERO, '/', O.AÑO) AS CONTRATA,
                    O.DESCRIPCION, 
                    PROYECTO,
                    SUBPROYECTO,
                    L.NOMBRE AS NombreLineaGestion,
                    AUTORIZADO_INICIAL,
                    AUTORIZADO_NUEVO
                FROM BD_PROYECTOS AS BD
                INNER JOIN OBRAS AS O ON BD.ID_BASE = O.ID
                INNER JOIN LINEA_DE_GESTION AS L ON BD.LINEA_DE_GESTION = L.ID
                INNER JOIN CONTRATA AS C ON O.CONTRATA = C.ID";

                // Usamos `setearConsulta` para establecer la consulta
                datos.setearConsulta(query);
                datos.ejecutarLectura(); // Ejecutar la lectura de datos

                // Leer cada registro de los resultados y convertirlo en un objeto `BdProyecto`
                while (datos.Lector.Read())
                {
                    BdProyecto proyecto = new BdProyecto
                    {
                        Obra = new Obra
                        {
                            Descripcion = datos.Lector["DESCRIPCION"].ToString()
                           , Contrata = new Contrata { 
                           Nombre = datos.Lector["CONTRATA"].ToString()
                        }
                        },
                        Id = datos.Lector["ID"] != DBNull.Value ? Convert.ToInt32(datos.Lector["ID"]) : 0,
                        Proyecto = datos.Lector["PROYECTO"].ToString(),
                        SubProyecto = datos.Lector["SUBPROYECTO"].ToString(),
                        LineaGestion = new LineaGestion
                        {
                            Nombre = datos.Lector["NombreLineaGestion"].ToString()
                        },
                        AutorizadoInicial = Convert.ToDecimal(datos.Lector["AUTORIZADO_INICIAL"]),
                        AutorizadoNuevo = Convert.ToDecimal(datos.Lector["AUTORIZADO_NUEVO"])
                    };

                    // Agregar el proyecto a la lista
                    lista.Add(proyecto);
                }

                return lista;
            }
            catch (Exception ex)
            {
                throw ex; // Propagar excepción para manejarla en una capa superior
            }
            finally
            {
                datos.cerrarConexion(); // Cerrar la conexión al final
            }
        }

        public void agregar(BdProyecto proyecto)
        {
            AccesoDatos datos = new AccesoDatos(); // Clase para manejar la conexión a la base de datos

            try
            {
                // Comando SQL para insertar un nuevo proyecto
                string query = @"
            INSERT INTO BD_PROYECTOS 
            (ID_BASE, SUBPROYECTO, PROYECTO, LINEA_DE_GESTION, AUTORIZADO_INICIAL) 
            VALUES 
            (@ID_BASE, @SUBPROYECTO, @PROYECTO, @LINEA_DE_GESTION, @AUTORIZADO_INICIAL)";

                // Establecer la consulta SQL para insertar los datos
                datos.setearConsulta(query);

                // Agregar parámetros para evitar inyecciones SQL
                datos.agregarParametro("@ID_BASE", proyecto.Obra.Id); // El ID de la obra que se relaciona con el proyecto
                datos.agregarParametro("@SUBPROYECTO", proyecto.SubProyecto); // Subproyecto
                datos.agregarParametro("@PROYECTO", proyecto.Proyecto); // Proyecto
                datos.agregarParametro("@LINEA_DE_GESTION", proyecto.LineaGestion.Id); // ID de la línea de gestión
                datos.agregarParametro("@AUTORIZADO_INICIAL", proyecto.AutorizadoInicial); // Monto autorizado inicial

                // Ejecutar la acción SQL de inserción
                datos.ejecutarAccion();
            }
            catch (Exception ex)
            {
                throw ex; // Propaga la excepción para manejarla en un nivel superior
            }
            finally
            {
                datos.cerrarConexion(); // Cierra la conexión con la base de datos
            }
        }
        public bool eliminar(int id)
        {
            var datos = new AccesoDatos(); // Clase para gestionar la conexión con la base de datos

            try
            {
                // Consulta SQL para eliminar el proyecto por su ID
                string query = "DELETE FROM BD_PROYECTOS WHERE ID = @id";

                // Configurar la consulta con el parámetro correspondiente
                datos.setearConsulta(query);
                datos.agregarParametro("@id", id);

                // Ejecutar la acción SQL para eliminar
                datos.ejecutarAccion();

                return true; // Devuelve true si la operación fue exitosa
            }
            catch (Exception ex)
            {
                // Lanzar una excepción específica con información adicional
                throw new Exception("Error al eliminar el proyecto.", ex);
            }
            finally
            {
                datos.cerrarConexion(); // Asegurar que la conexión se cierra
            }
        }
    }
}

