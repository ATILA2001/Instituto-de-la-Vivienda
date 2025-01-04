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
        public List<BdProyecto> Listar()
        {
            List<BdProyecto> lista = new List<BdProyecto>();
            AccesoDatos datos = new AccesoDatos(); // Se usa la clase para manejar la conexión

            try
            {
                // La consulta SQL que une las tablas de la base de datos.
                string query = @"
                SELECT 
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
    }
}

