using Dominio;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negocio
{
    public class BdProyectoNegocio
    {
        public DataTable listarddl()
        {
            DataTable dt = new DataTable();
            AccesoDatos datos = new AccesoDatos();

            try
            {
                datos.setearConsulta("SELECT DISTINCT proyecto FROM BD_PROYECTOS");

                datos.ejecutarLectura();
                dt.Columns.Add("ID", typeof(int)); // Generar ID incremental
                dt.Columns.Add("NOMBRE", typeof(string));

                int id = 1; // Inicializamos el contador de ID

                while (datos.Lector.Read())
                {
                    DataRow row = dt.NewRow();
                    row["ID"] = id; // Generar ID único incremental
                    row["NOMBRE"] = datos.Lector["proyecto"] as string;

                    dt.Rows.Add(row);
                    id++; // Incrementar el ID para la siguiente fila
                }

                return dt;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Hubo un problema al obtener los proyectos para el usuario", ex);
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

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

        public List<BdProyecto> Listar(List <string> linea, List<string> proye, List<string> area, string filtro = null)
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
                L.ID as ID_LINEA,
                A.ID AS ID_AREA,
                A.NOMBRE AS AREA
                FROM BD_PROYECTOS AS BD
                INNER JOIN OBRAS AS O ON BD.ID_BASE = O.ID
                INNER JOIN AREAS AS A ON O.AREA = A.ID
                INNER JOIN BARRIOS AS BA ON O.BARRIO = BA.ID
                INNER JOIN LINEA_DE_GESTION AS L ON BD.LINEA_DE_GESTION = L.ID
                INNER JOIN CONTRATA AS C ON O.CONTRATA = C.ID where 1=1";
               
                if (linea != null && linea.Count > 0)
                {
                    string lineasParam = string.Join(",", linea.Select((e, i) => $"@linea{i}"));
                    query += $" AND L.NOMBRE IN ({lineasParam})";
                    for (int i = 0; i < linea.Count; i++)
                    {
                        datos.setearParametros($"@linea{i}", linea[i]);
                    }
                }
                if (area != null && area.Count > 0)
                {
                    string areasParam = string.Join(",", area.Select((e, i) => $"@area{i}"));
                    query += $" AND A.NOMBRE IN ({areasParam})";
                    for (int i = 0; i < area.Count; i++)
                    {
                        datos.setearParametros($"@area{i}", area[i]);
                    }
                }
                if (proye != null && proye.Count > 0)
                {
                    string proyeParam = string.Join(",", proye.Select((e, i) => $"@proye{i}"));
                    query += $" AND PROYECTO IN ({proyeParam})";
                    for (int i = 0; i < proye.Count; i++)
                    {
                        datos.setearParametros($"@proye{i}", proye[i]);
                    }
                }
                
                if (!string.IsNullOrEmpty(filtro))
                {

                    query += " AND (PROYECTO LIKE @filtro OR  L.NOMBRE LIKE @filtro OR SUBPROYECTO LIKE @filtro OR O.DESCRIPCION LIKE @filtro ) ";
                    datos.setearParametros("@filtro", $"%{filtro}%");

                }
                query += " ORDER BY OBRA ";
                datos.setearConsulta(query);
                datos.ejecutarLectura();

                while (datos.Lector.Read())
                {
                    BdProyecto proyecto = new BdProyecto
                    {
                        Obra = new Obra
                        {
                            Id = datos.Lector["ID_OBRA"] != DBNull.Value ? Convert.ToInt32(datos.Lector["ID_OBRA"]) : 0,
                            Descripcion = datos.Lector["OBRA"].ToString(),
                            Contrata = new Contrata
                            {
                                Nombre = datos.Lector["CONTRATA"].ToString()
                            },
                            Area = new Area
                            {
                                Id = datos.Lector["ID_AREA"] != DBNull.Value ? Convert.ToInt32(datos.Lector["ID_AREA"]) : 0,
                                Nombre = datos.Lector["AREA"].ToString()
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
                        AutorizadoNuevo = Convert.ToDecimal(datos.Lector["AUTORIZADO_NUEVO"]),
                        
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

