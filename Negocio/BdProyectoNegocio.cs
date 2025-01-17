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

                datos.setearConsulta(@"
        UPDATE BD_PROYECTOS
        SET 
           
            SUBPROYECTO = @subProyecto, 
            PROYECTO = @proyecto, 
            LINEA_DE_GESTION = @lineaGestion, 
            AUTORIZADO_INICIAL = @autorizadoInicial
        WHERE ID = @id");

                datos.agregarParametro("@subProyecto", proyecto.SubProyecto);
                datos.agregarParametro("@proyecto", proyecto.Proyecto);
                datos.agregarParametro("@lineaGestion", proyecto.LineaGestion.Id);
                datos.agregarParametro("@autorizadoInicial", proyecto.AutorizadoInicial);
                datos.agregarParametro("@id", proyecto.Id);

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
            AccesoDatos datos = new AccesoDatos();
            try
            {
                string query = @"
                SELECT 
                    BD.ID,
                    CONCAT(C.NOMBRE, ' ', O.NUMERO, '/', O.AÑO) AS CONTRATA,
                    CONCAT(O.DESCRIPCION, ' - ', BA.NOMBRE ) AS OBRA, 
                    PROYECTO,
                    SUBPROYECTO,
                    L.NOMBRE AS NombreLineaGestion,
                    AUTORIZADO_INICIAL,
                    AUTORIZADO_NUEVO,
O.ID as ID_OBRA,
L.ID as ID_LINEA
                FROM BD_PROYECTOS AS BD
                INNER JOIN OBRAS AS O ON BD.ID_BASE = O.ID
INNER JOIN BARRIOS AS BA ON O.BARRIO = BA.ID
                INNER JOIN LINEA_DE_GESTION AS L ON BD.LINEA_DE_GESTION = L.ID
                INNER JOIN CONTRATA AS C ON O.CONTRATA = C.ID ORDER BY OBRA";

                datos.setearConsulta(query);
                datos.ejecutarLectura();

                while (datos.Lector.Read())
                {
                    BdProyecto proyecto = new BdProyecto
                    {
                        Obra = new Obra
                        {
                            Id = datos.Lector["ID_OBRA"] != DBNull.Value ? Convert.ToInt32(datos.Lector["ID_OBRA"]) : 0,
                            Descripcion = datos.Lector["OBRA"].ToString()
                           ,
                            Contrata = new Contrata
                            {
                                Nombre = datos.Lector["CONTRATA"].ToString()
                            }
                        },
                        Id = datos.Lector["ID"] != DBNull.Value ? Convert.ToInt32(datos.Lector["ID"]) : 0,
                        Proyecto = datos.Lector["PROYECTO"].ToString(),
                        SubProyecto = datos.Lector["SUBPROYECTO"].ToString(),
                        LineaGestion = new LineaGestion
                        {
                            Id = datos.Lector["ID_LINEA"] != DBNull.Value ? Convert.ToInt32(datos.Lector["ID_LINEA"]) : 0,
                            Nombre = datos.Lector["NombreLineaGestion"].ToString()
                        },
                        AutorizadoInicial = Convert.ToDecimal(datos.Lector["AUTORIZADO_INICIAL"]),
                        AutorizadoNuevo = Convert.ToDecimal(datos.Lector["AUTORIZADO_NUEVO"])
                    };

                    lista.Add(proyecto);
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

        public void agregar(BdProyecto proyecto)
        {
            AccesoDatos datos = new AccesoDatos();

            try
            {
                string query = @"
            INSERT INTO BD_PROYECTOS 
            (ID_BASE, SUBPROYECTO, PROYECTO, LINEA_DE_GESTION, AUTORIZADO_INICIAL) 
            VALUES 
            (@ID_BASE, @SUBPROYECTO, @PROYECTO, @LINEA_DE_GESTION, @AUTORIZADO_INICIAL)";

                datos.setearConsulta(query);

                datos.agregarParametro("@ID_BASE", proyecto.Obra.Id);
                datos.agregarParametro("@SUBPROYECTO", proyecto.SubProyecto);
                datos.agregarParametro("@PROYECTO", proyecto.Proyecto);
                datos.agregarParametro("@LINEA_DE_GESTION", proyecto.LineaGestion.Id);
                datos.agregarParametro("@AUTORIZADO_INICIAL", proyecto.AutorizadoInicial);

                datos.ejecutarAccion();
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
        public bool eliminar(int id)
        {
            var datos = new AccesoDatos();

            try
            {
                string query = "DELETE FROM BD_PROYECTOS WHERE ID = @id";

                datos.setearConsulta(query);
                datos.agregarParametro("@id", id);

                datos.ejecutarAccion();

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al eliminar el proyecto.", ex);
            }
            finally
            {
                datos.cerrarConexion();
            }
        }
    }
}

