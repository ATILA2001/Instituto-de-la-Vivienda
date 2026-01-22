using Dominio;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
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
                //datos.setearConsulta("SELECT DISTINCT proyecto FROM BD_PROYECTOS");
                datos.setearConsulta("SELECT DISTINCT proyecto AS NOMBRE FROM BD_PROYECTOS WHERE proyecto IS NOT NULL AND LTRIM(RTRIM(proyecto)) <> '' ORDER BY proyecto");

                datos.ejecutarLectura();

                dt.Load(datos.Lector);
                //dt.Columns.Add("ID", typeof(int)); // Generar ID incremental
                //dt.Columns.Add("NOMBRE", typeof(string));

                //int id = 1; // Inicializamos el contador de ID

                //while (datos.Lector.Read())
                //{
                //    DataRow row = dt.NewRow();
                //    row["ID"] = id; // Generar ID único incremental
                //    row["NOMBRE"] = datos.Lector["proyecto"] as string;

                //    dt.Rows.Add(row);
                //    id++; // Incrementar el ID para la siguiente fila
                //}

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
            AUTORIZADO2025 = @Autorizado2025
        WHERE ID = @id");

                datos.agregarParametro("@subProyecto", proyecto.SubProyecto);
                datos.agregarParametro("@proyecto", proyecto.Proyecto);
                datos.agregarParametro("@lineaGestion", proyecto.LineaGestion.Id);
                datos.agregarParametro("@Autorizado2025", proyecto.Autorizado2025);
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

        public List<BdProyecto> Listar(List<string> linea, List<string> proye, List<string> area, string filtro = null)
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
                    AUTORIZADO2025,
                    AUTORIZADO2026,
                    O.ID as ID_OBRA,
                    L.ID as ID_LINEA,
                    A.ID AS ID_AREA,
                    A.NOMBRE AS AREA,
E.ID AS ID_EMPRESA,
                    E.NOMBRE AS EMPRESA,
                    C.ID AS ID_CONTRATA
                FROM BD_PROYECTOS AS BD
                INNER JOIN OBRAS AS O ON BD.ID_BASE = O.ID
                INNER JOIN AREAS AS A ON O.AREA = A.ID
                INNER JOIN BARRIOS AS BA ON O.BARRIO = BA.ID
                INNER JOIN LINEA_DE_GESTION AS L ON BD.LINEA_DE_GESTION = L.ID
                INNER JOIN CONTRATA AS C ON O.CONTRATA = C.ID
INNER JOIN EMPRESAS AS E ON E.ID = O.EMPRESA                
WHERE 1=1";


                if (linea != null && linea.Count > 0)
                {
                    try
                    {
                        var lineaIds = linea.Select(int.Parse).ToList();
                        if (lineaIds.Any())
                        {
                            string lineasParam = string.Join(",", lineaIds.Select((id, i) => $"@linea{i}"));
                            query += $" AND L.ID IN ({lineasParam})"; // Usa L.ID
                            setearParametrosInt(datos, lineaIds, "linea"); // Llama al helper de INT
                        }
                    }
                    catch (FormatException ex) { Debug.WriteLine("Error al convertir IDs de línea a int: " + ex.Message); }
                }
                if (area != null && area.Count > 0)
                {
                    try
                    {
                        var areaIds = area.Select(int.Parse).ToList();
                        if (areaIds.Any())
                        {
                            string areasParam = string.Join(",", areaIds.Select((id, i) => $"@area{i}"));
                            query += $" AND A.ID IN ({areasParam})"; // Usa A.ID
                            setearParametrosInt(datos, areaIds, "area"); // Llama al helper de INT
                        }
                    }
                    catch (FormatException ex) { Debug.WriteLine("Error al convertir IDs de área a int: " + ex.Message); }
                }
                if (proye != null && proye.Count > 0)
                {
                    string proyeParamNombres = string.Join(",", proye.Select((nombre, i) => $"@proyeNombre{i}"));
                    query += $" AND BD.PROYECTO IN ({proyeParamNombres})";

                    for (int i = 0; i < proye.Count; i++)
                    {
                        datos.setearParametros($"@proyeNombre{i}", proye[i]); // Pasar los nombres como parámetros string.
                    }
                }


                if (!string.IsNullOrEmpty(filtro))
                {
                    // Usar los alias BD, O, A, C, L definidos en la consulta
                    query += " AND (BD.PROYECTO LIKE @filtro OR BD.SUBPROYECTO LIKE @filtro OR O.DESCRIPCION LIKE @filtro OR A.NOMBRE LIKE @filtro OR C.NOMBRE LIKE @filtro OR L.NOMBRE LIKE @filtro)";
                    datos.setearParametros("@filtro", "%" + filtro + "%");
                }

                query += " ORDER BY OBRA ";
                datos.setearConsulta(query);
                datos.ejecutarLectura();

                while (datos.Lector.Read())
                {
                    BdProyecto aux = new BdProyecto();
                    aux.Id = (int)datos.Lector["ID"]; // Usa "ID" del SELECT
                    aux.Proyecto = (string)datos.Lector["PROYECTO"]; // Usa "PROYECTO" del SELECT
                    aux.SubProyecto = datos.Lector["SUBPROYECTO"] != DBNull.Value ? (string)datos.Lector["SUBPROYECTO"] : ""; // Usa "SUBPROYECTO"
                    aux.Autorizado2025 = (decimal)datos.Lector["AUTORIZADO2025"]; // Usa "AUTORIZADO2025"
                    aux.Autorizado2026 = (decimal)datos.Lector["AUTORIZADO2026"]; // Usa "AUTORIZADO2026"

                    aux.Obra = new Obra();
                    aux.Obra.Id = (int)datos.Lector["ID_OBRA"]; // Usa "ID_OBRA"
                    aux.Obra.Descripcion = (string)datos.Lector["OBRA"]; // Usa "OBRA" (el CONCAT)

                    aux.Obra.Area = new Area();
                    aux.Obra.Area.Id = (int)datos.Lector["ID_AREA"]; // Usa "ID_AREA"
                    aux.Obra.Area.Nombre = (string)datos.Lector["AREA"]; // Usa "AREA" (A.NOMBRE AS AREA)

                    aux.Obra.Contrata = new Contrata();
                    aux.Obra.Contrata.Id = (int)datos.Lector["ID_CONTRATA"]; // Usa "ID_CONTRATA" (añadido al SELECT)
                    aux.Obra.Contrata.Nombre = (string)datos.Lector["CONTRATA"]; // Usa "CONTRATA" (el CONCAT)

                    aux.Obra.Empresa = new Empresa();
                    aux.Obra.Empresa.Id = (int)datos.Lector["ID_EMPRESA"];
                    aux.Obra.Empresa.Nombre = (string)datos.Lector["EMPRESA"];

                    aux.LineaGestion = new LineaGestion();
                    aux.LineaGestion.Id = (int)datos.Lector["ID_LINEA"]; // Usa "ID_LINEA"
                    aux.LineaGestion.Nombre = (string)datos.Lector["NombreLineaGestion"]; // Usa "NombreLineaGestion"

                    lista.Add(aux);
                }
                return lista;
            }
            catch (Exception ex)
            {
                // Loggear o manejar la excepción
                System.Diagnostics.Debug.WriteLine("Error en BdProyectoNegocio.Listar: " + ex.ToString());
                throw; // Relanzar para que la capa superior se entere
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        private void setearParametrosInt(AccesoDatos datos, List<int> valores, string prefijo)
        {
            for (int i = 0; i < valores.Count; i++)
            {
                // Pasar el valor INT directamente a setearParametro
                datos.setearParametros($"@{prefijo}{i}", valores[i]);
            }
        }

        // Método auxiliar para parámetros string (sin cambios)
        private void setearParametros(AccesoDatos datos, List<string> valores, string prefijo)
        {
            for (int i = 0; i < valores.Count; i++)
            {
                datos.setearParametros($"@{prefijo}{i}", valores[i]);
            }
        }

        public bool agregar(BdProyecto proyecto)
        {
            AccesoDatos datos = new AccesoDatos();

            try
            {
                string query = @"
                    INSERT INTO BD_PROYECTOS 
                    (ID_BASE, SUBPROYECTO, PROYECTO, LINEA_DE_GESTION, AUTORIZADO2025) 
                    VALUES 
                    (@ID_BASE, @SUBPROYECTO, @PROYECTO, @LINEA_DE_GESTION, @AUTORIZADO2025)";

                datos.setearConsulta(query);

                datos.agregarParametro("@ID_BASE", proyecto.Obra.Id);
                datos.agregarParametro("@SUBPROYECTO", proyecto.SubProyecto);
                datos.agregarParametro("@PROYECTO", proyecto.Proyecto);
                datos.agregarParametro("@LINEA_DE_GESTION", proyecto.LineaGestion.Id);
                datos.agregarParametro("@AUTORIZADO2025", proyecto.Autorizado2025);

                datos.ejecutarAccion();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al agregar el proyecto.", ex);
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

