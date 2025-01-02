using Dominio;
using System;
using System.Collections.Generic;
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
                // Consulta para insertar el nuevo legítimo
                datos.setearConsulta(@"
            INSERT INTO LEGITIMOS_ABONOS 
            (CODIGO_AUTORIZANTE, OBRA, EXPEDIENTE, INICIO_EJECUCION, FIN_EJECUCION, CERTIFICADO, MES_APROBACION)
            VALUES 
            (@CODIGO_AUTORIZANTE, @OBRA, @EXPEDIENTE, @INICIO_EJECUCION, @FIN_EJECUCION, @CERTIFICADO, @MES_APROBACION)");

                // Asignar los parámetros
                datos.agregarParametro("@CODIGO_AUTORIZANTE", nuevoLegitimo.CodigoAutorizante);
                datos.agregarParametro("@OBRA", nuevoLegitimo.Obra.Id);
                datos.agregarParametro("@EXPEDIENTE", nuevoLegitimo.Expediente);
                datos.agregarParametro("@INICIO_EJECUCION", nuevoLegitimo.InicioEjecucion);
                datos.agregarParametro("@FIN_EJECUCION", nuevoLegitimo.FinEjecucion);
                datos.agregarParametro("@CERTIFICADO", nuevoLegitimo.Certificado);
                datos.agregarParametro("@MES_APROBACION", nuevoLegitimo.MesAprobacion);

                // Ejecutar la consulta
                datos.ejecutarAccion();

                // Retorna true si todo salió bien
                return true;
            }
            catch (Exception ex)
            {
                // Lanza la excepción al nivel superior para manejo personalizado
                throw new Exception("Error al agregar un legítimo.", ex);
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        public bool eliminar(int id)
        {
            throw new NotImplementedException();
        }

        public List<Legitimo> listar(Usuario usuario)
        {
            var lista = new List<Legitimo>();
            var datos = new AccesoDatos();

            try
            {
                datos.setearConsulta(@"
            SELECT 
                O.DESCRIPCION AS OBRA,
                L.CODIGO_AUTORIZANTE,
                L.EXPEDIENTE,
                L.INICIO_EJECUCION,
                L.FIN_EJECUCION,
                L.CERTIFICADO,
                L.MES_APROBACION
            FROM LEGITIMOS_ABONOS AS L
            INNER JOIN OBRAS AS O ON L.OBRA = O.ID WHERE 
                O.AREA = @area");
                datos.agregarParametro("@area", usuario.Area.Id);

                datos.ejecutarLectura();

                while (datos.Lector.Read())
                {
                    var legitimoAbono = new Legitimo
                    {
                        CodigoAutorizante = datos.Lector["CODIGO_AUTORIZANTE"].ToString(),
                        Expediente = datos.Lector["EXPEDIENTE"] as string,
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

    }
}
