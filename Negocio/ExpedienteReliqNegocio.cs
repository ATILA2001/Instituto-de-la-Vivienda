using Dominio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negocio
{
    public class ExpedienteReliqNegocio
    {
        public bool GuardarOActualizar(string codigoRedet, DateTime mesAprobacion, string expediente)
        {
            AccesoDatos datos = new AccesoDatos();
            try
            {
                // Verificar si ya existe una entrada para este código y mes
                string queryVerificar = @"
                    SELECT COUNT(1) FROM EXPEDIENTES_RELIQ 
                    WHERE CODIGO_REDET = @CODIGO_REDET 
                    AND MONTH(MES_APROBACION) = MONTH(@MES_APROBACION)
                    AND YEAR(MES_APROBACION) = YEAR(@MES_APROBACION)";

                datos.setearConsulta(queryVerificar);
                datos.agregarParametro("@CODIGO_REDET", codigoRedet);
                datos.agregarParametro("@MES_APROBACION", mesAprobacion);
                datos.ejecutarLectura();

                bool existeRegistro = false;
                if (datos.Lector.Read())
                {
                    existeRegistro = Convert.ToInt32(datos.Lector[0]) > 0;
                }

                datos.cerrarConexion();

                // Si existe, actualizamos; si no, insertamos
                string query = existeRegistro
                    ? @"UPDATE EXPEDIENTES_RELIQ 
                        SET EXPEDIENTE = @EXPEDIENTE
                        WHERE CODIGO_REDET = @CODIGO_REDET 
                        AND MONTH(MES_APROBACION) = MONTH(@MES_APROBACION)
                        AND YEAR(MES_APROBACION) = YEAR(@MES_APROBACION)"
                    : @"INSERT INTO EXPEDIENTES_RELIQ 
                        (CODIGO_REDET, MES_APROBACION, EXPEDIENTE) 
                        VALUES 
                        (@CODIGO_REDET, @MES_APROBACION, @EXPEDIENTE)";

                datos = new AccesoDatos();
                datos.setearConsulta(query);
                datos.agregarParametro("@CODIGO_REDET", codigoRedet);
                datos.agregarParametro("@MES_APROBACION", mesAprobacion);
                datos.agregarParametro("@EXPEDIENTE", expediente);

                datos.ejecutarAccion();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al guardar/actualizar el expediente de reliquidación", ex);
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        public ExpedienteReliq ObtenerPorCodigoYMes(string codigoRedet, DateTime mesAprobacion)
        {
            AccesoDatos datos = new AccesoDatos();
            try
            {
                string query = @"
                    SELECT ID, CODIGO_REDET, MES_APROBACION, EXPEDIENTE
                    FROM EXPEDIENTES_RELIQ
                    WHERE CODIGO_REDET = @CODIGO_REDET
                    AND MONTH(MES_APROBACION) = MONTH(@MES_APROBACION)
                    AND YEAR(MES_APROBACION) = YEAR(@MES_APROBACION)";

                datos.setearConsulta(query);
                datos.agregarParametro("@CODIGO_REDET", codigoRedet);
                datos.agregarParametro("@MES_APROBACION", mesAprobacion);
                datos.ejecutarLectura();

                ExpedienteReliq expediente = null;
                if (datos.Lector.Read())
                {
                    expediente = new ExpedienteReliq
                    {
                        Id = (int)datos.Lector["ID"],
                        CodigoRedet = datos.Lector["CODIGO_REDET"] as string,
                        MesAprobacion = (DateTime)datos.Lector["MES_APROBACION"],
                        Expediente = datos.Lector["EXPEDIENTE"] as string
                    };
                }

                return expediente;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener el expediente de reliquidación", ex);
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        public List<ExpedienteReliq> Listar()
        {
            List<ExpedienteReliq> lista = new List<ExpedienteReliq>();
            AccesoDatos datos = new AccesoDatos();

            try
            {
                string query = @"
                    SELECT ID, CODIGO_REDET, MES_APROBACION, EXPEDIENTE
                    FROM EXPEDIENTES_RELIQ
                    ORDER BY CODIGO_REDET, MES_APROBACION";

                datos.setearConsulta(query);
                datos.ejecutarLectura();

                while (datos.Lector.Read())
                {
                    ExpedienteReliq expediente = new ExpedienteReliq
                    {
                        Id = (int)datos.Lector["ID"],
                        CodigoRedet = datos.Lector["CODIGO_REDET"] as string,
                        MesAprobacion = (DateTime)datos.Lector["MES_APROBACION"],
                        Expediente = datos.Lector["EXPEDIENTE"] as string
                    };

                    lista.Add(expediente);
                }

                return lista;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar los expedientes de reliquidación", ex);
            }
            finally
            {
                datos.cerrarConexion();
            }
        }
    }
}