using Dominio;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negocio
{
    public class LegitimoNegocio
    {
        public bool agregar(Legitimo nuevoLegitimo)
        {
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta(@"
            INSERT INTO LEGITIMOS_ABONOS 
            (CODIGO_AUTORIZANTE, OBRA, EXPEDIENTE, INICIO_EJECUCION, FIN_EJECUCION, CERTIFICADO, MES_APROBACION)
            VALUES 
            (@CODIGO_AUTORIZANTE, @OBRA, @EXPEDIENTE, @INICIO_EJECUCION, @FIN_EJECUCION, @CERTIFICADO, @MES_APROBACION)");

                datos.agregarParametro("@CODIGO_AUTORIZANTE", nuevoLegitimo.CodigoAutorizante);
                datos.agregarParametro("@OBRA", nuevoLegitimo.Obra.Id);
                datos.agregarParametro("@EXPEDIENTE", nuevoLegitimo.Expediente);
                datos.agregarParametro("@INICIO_EJECUCION", nuevoLegitimo.InicioEjecucion);
                datos.agregarParametro("@FIN_EJECUCION", nuevoLegitimo.FinEjecucion);
                datos.agregarParametro("@CERTIFICADO", nuevoLegitimo.Certificado);
                datos.agregarParametro("@MES_APROBACION", nuevoLegitimo.MesAprobacion);

                datos.ejecutarAccion();

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al agregar un legítimo.", ex);
            }
            finally
            {
                datos.cerrarConexion();
            }
        }
        public bool eliminar(string codigo)
        {
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("DELETE FROM LEGITIMOS_ABONOS WHERE ID = @ID");

                datos.agregarParametro("@ID", codigo);

                datos.ejecutarAccion();

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al eliminar el legítimo.", ex);
            }
            finally
            {
                datos.cerrarConexion();
            }
        }
        public DataTable listarddl(Usuario usuario)
        {
            DataTable dt = new DataTable();
            AccesoDatos datos = new AccesoDatos();

            try
            {
                // Validación: usuario y su área no pueden ser nulos
                if (usuario == null || usuario.Area == null)
                {
                    throw new ArgumentNullException("El usuario o su área no pueden ser nulos.");
                }

                // Configurar la consulta
                datos.setearConsulta(@"
            SELECT  ROW_NUMBER() OVER (ORDER BY L.CODIGO_AUTORIZANTE) AS ID, L.CODIGO_AUTORIZANTE FROM LEGITIMOS_ABONOS AS L INNER JOIN OBRAS AS O ON L.OBRA = O.ID WHERE O.AREA = @area AND L.CODIGO_AUTORIZANTE IS NOT NULL GROUP BY L.CODIGO_AUTORIZANTE ");
                datos.agregarParametro("@area", usuario.Area.Id);

                // Ejecutar la lectura
                datos.ejecutarLectura();

                // Crear columnas para el DataTable
                dt.Columns.Add("ID", typeof(int));
                dt.Columns.Add("NOMBRE", typeof(string));

                // Poblar el DataTable con los datos leídos
                while (datos.Lector.Read())
                {
                    DataRow row = dt.NewRow();
                    row["ID"] = Convert.ToInt32(datos.Lector["ID"]);
                    row["NOMBRE"] = datos.Lector["CODIGO_AUTORIZANTE"] as string;
                    dt.Rows.Add(row);
                }

                return dt; // Devolver el DataTable
            }
            catch (Exception ex)
            {
                // Manejo de errores
                throw new ApplicationException("Hubo un problema al obtener los códigos autorizantes.", ex);
            }
            finally
            {
                // Asegurarse de cerrar la conexión
                datos.cerrarConexion();
            }
        }
        public List<Legitimo> listarFiltro(Usuario usuario, List<string> mesAprobacion, List<string> empresa, List<string> autorizante)
        {
            var lista = new List<Legitimo>();
            var datos = new AccesoDatos();

            try
            {
                string query = " SELECT L.ID,CONCAT(O.DESCRIPCION, ' - ', BA.NOMBRE ) AS OBRA, L.CODIGO_AUTORIZANTE, L.EXPEDIENTE, L.INICIO_EJECUCION, L.FIN_EJECUCION, L.CERTIFICADO, L.MES_APROBACION, EM.NOMBRE AS EMPRESA, CASE WHEN COUNT(L.EXPEDIENTE) OVER (PARTITION BY L.EXPEDIENTE) = 1 THEN (SELECT SUM(D.IMPORTE_PP) FROM DEVENGADOS D WHERE D.EE_FINANCIERA = L.EXPEDIENTE) ELSE (SELECT SUM(D.IMPORTE_PP) FROM DEVENGADOS D WHERE D.EE_FINANCIERA = L.EXPEDIENTE) * L.CERTIFICADO / (SELECT SUM(L2.CERTIFICADO) FROM LEGITIMOS_ABONOS L2 WHERE L2.EXPEDIENTE = L.EXPEDIENTE) END AS SIGAF, PS.[BUZON DESTINO], PS.[FECHA ULTIMO PASE] FROM LEGITIMOS_ABONOS AS L INNER JOIN OBRAS AS O ON L.OBRA = O.ID INNER JOIN BARRIOS AS BA ON O.BARRIO = BA.ID INNER JOIN EMPRESAS AS EM ON O.EMPRESA = EM.ID LEFT JOIN PASES_SADE PS ON L.EXPEDIENTE = PS.EXPEDIENTE COLLATE Modern_Spanish_CI_AS WHERE O.AREA = @area ";

                if (empresa != null && empresa.Count > 0)
                {
                    string empresasParam = string.Join(",", empresa.Select((e, i) => $"@empresa{i}"));
                    query += $" AND EM.NOMBRE IN ({empresasParam})";
                    for (int i = 0; i < empresa.Count; i++)
                    {
                        datos.setearParametros($"@empresa{i}", empresa[i]);
                    }
                }
                if (autorizante != null && autorizante.Count > 0)
                {
                    string AutorizanteParam = string.Join(",", autorizante.Select((e, i) => $"@autorizante{i}"));
                    query += $" AND L.CODIGO_AUTORIZANTE IN ({AutorizanteParam})";
                    for (int i = 0; i < autorizante.Count; i++)
                    {
                        datos.setearParametros($"@autorizante{i}", autorizante[i]);
                    }
                }

                if (mesAprobacion != null && mesAprobacion.Count > 0)
                {
                    try
                    {
                        // Dividir y validar el formato de las fechas
                        var mesesAnios = mesAprobacion
                            .Where(ma => ma.Contains("-"))
                            .Select(ma => ma.Split('-')) // Separar "2024-01" en ["2024", "01"]
                            .Select(parts => new { Año = parts[0], Mes = parts[1] });

                        // Construir los filtros dinámicos
                        string filtrosMesAño = string.Join(" OR ", mesesAnios.Select((ma, i) => $"(MONTH(L.MES_APROBACION) = @Mes{i} AND YEAR(L.MES_APROBACION) = @Año{i})"));
                        query += $" AND ({filtrosMesAño})";

                        // Asignar los parámetros
                        int index = 0;
                        foreach (var ma in mesesAnios)
                        {
                            if (int.TryParse(ma.Mes, out int mes) && int.TryParse(ma.Año, out int año))
                            {
                                datos.setearParametros($"@Mes{index}", mes);
                                datos.setearParametros($"@Año{index}", año);
                                index++;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Error al procesar el filtro de fechas.", ex);
                    }
                }
                datos.setearConsulta(query);

                datos.agregarParametro("@area", usuario.Area.Id);

                datos.ejecutarLectura();

                while (datos.Lector.Read())
                {
                    var legitimoAbono = new Legitimo
                    {
                        Id = (int)datos.Lector["ID"],
                        CodigoAutorizante = datos.Lector["CODIGO_AUTORIZANTE"].ToString(),
                        Expediente = datos.Lector["EXPEDIENTE"] as string,
                        Empresa = datos.Lector["EMPRESA"]?.ToString(),
                        InicioEjecucion = datos.Lector["INICIO_EJECUCION"] != DBNull.Value
                            ? (DateTime?)Convert.ToDateTime(datos.Lector["INICIO_EJECUCION"])
                            : null,
                        FinEjecucion = datos.Lector["FIN_EJECUCION"] != DBNull.Value
                            ? (DateTime?)Convert.ToDateTime(datos.Lector["FIN_EJECUCION"])
                            : null,
                        Certificado = datos.Lector["CERTIFICADO"] != DBNull.Value
                            ? (decimal?)Convert.ToDecimal(datos.Lector["CERTIFICADO"])
                            : null,
                        MesAprobacion = datos.Lector["MES_APROBACION"] != DBNull.Value
                            ? (DateTime?)Convert.ToDateTime(datos.Lector["MES_APROBACION"])
                            : null,
                        Obra = new Obra
                        {
                            Descripcion = datos.Lector["OBRA"].ToString()
                        },
                        Sigaf = datos.Lector["SIGAF"] != DBNull.Value ? Convert.ToDecimal(datos.Lector["SIGAF"]) : (decimal?)null,
                        FechaSade = datos.Lector["FECHA ULTIMO PASE"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(datos.Lector["FECHA ULTIMO PASE"]) : null,
                        BuzonSade = datos.Lector["BUZON DESTINO"]?.ToString()
                    };

                    lista.Add(legitimoAbono);
                }

                return lista;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar los legítimos abonos.", ex);
            }
            finally
            {
                datos.cerrarConexion();
            }
        }
        public List<Legitimo> listar()
        {
            var lista = new List<Legitimo>();
            var datos = new AccesoDatos();

            try
            {
                datos.setearConsulta(@"
            SELECT 
                O.DESCRIPCION AS OBRA,
                O.AREA AS AREA_ID,
                A.NOMBRE AS AREA,
                L.CODIGO_AUTORIZANTE,
                L.EXPEDIENTE,
                L.INICIO_EJECUCION,
                L.FIN_EJECUCION,
                L.CERTIFICADO,
                L.MES_APROBACION
            FROM LEGITIMOS_ABONOS AS L
            INNER JOIN OBRAS AS O ON L.OBRA = O.ID
            INNER JOIN AREAS AS A ON O.AREA = A.ID");

                datos.ejecutarLectura();

                while (datos.Lector.Read())
                {
                    var legitimoAbono = new Legitimo
                    {
                        CodigoAutorizante = datos.Lector["CODIGO_AUTORIZANTE"].ToString(),
                        Expediente = datos.Lector["EXPEDIENTE"] as string,
                        InicioEjecucion = datos.Lector["INICIO_EJECUCION"] != DBNull.Value
                            ? Convert.ToDateTime(datos.Lector["INICIO_EJECUCION"])
                            : (DateTime?)null,
                        FinEjecucion = datos.Lector["FIN_EJECUCION"] != DBNull.Value
                            ? Convert.ToDateTime(datos.Lector["FIN_EJECUCION"])
                            : (DateTime?)null,
                        Certificado = datos.Lector["CERTIFICADO"] != DBNull.Value
                            ? Convert.ToDecimal(datos.Lector["CERTIFICADO"])
                            : (decimal?)null,
                        MesAprobacion = datos.Lector["MES_APROBACION"] != DBNull.Value
                            ? Convert.ToDateTime(datos.Lector["MES_APROBACION"])
                            : (DateTime?)null,
                        Obra = new Obra
                        {
                            Descripcion = datos.Lector["OBRA"].ToString(),
                            Area = new Area
                            {
                                Id = Convert.ToInt32(datos.Lector["AREA_ID"]),
                                Nombre = datos.Lector["AREA"].ToString()
                            }
                        }
                    };

                    lista.Add(legitimoAbono);
                }

                return lista;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar los legítimos abonos.", ex);
            }
            finally
            {
                datos.cerrarConexion();
            }
        }
        public bool ActualizarExpediente(int id, string ex)
        {
            var datos = new AccesoDatos();

            try
            {
                datos.setearConsulta(@"
        UPDATE LEGITIMOS_ABONOS 
        SET 
            EXPEDIENTE = @expediente
           WHERE ID = @id");

                datos.agregarParametro("@expediente", ex);
                datos.agregarParametro("@id", id);

                datos.ejecutarAccion();
                return true;
            }
            catch (Exception)
            {
                throw new Exception("Error al modificar el autorizante.");
            }
            finally
            {
                datos.cerrarConexion();
            }
        }
    }
}
