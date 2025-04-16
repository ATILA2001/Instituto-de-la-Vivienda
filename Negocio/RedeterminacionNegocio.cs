using Dominio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negocio
{
    public class RedeterminacionNegocio
    {

        public bool agregar(Redeterminacion nuevaRedet)
        {
            AccesoDatos datos = new AccesoDatos();
            try
            {
                string query = @"
            INSERT INTO REDETERMINACIONES 
            (CODIGO_AUTORIZANTE, EXPEDIENTE, SALTO, NRO, TIPO, ETAPA, OBSERVACIONES,PORCENTAJE_PONDERACION) 
            VALUES 
            (@CODIGO_AUTORIZANTE, @EXPEDIENTE, @SALTO, @NRO, @TIPO, @ETAPA, @OBSERVACIONES,@PORCENTAJE)";

                datos.setearConsulta(query);

                datos.agregarParametro("@CODIGO_AUTORIZANTE", nuevaRedet.Autorizante.CodigoAutorizante);
                datos.agregarParametro("@EXPEDIENTE", nuevaRedet.Expediente);
                datos.agregarParametro("@SALTO", nuevaRedet.Salto.HasValue ? (object)nuevaRedet.Salto.Value : DBNull.Value);
                datos.agregarParametro("@NRO", nuevaRedet.Nro.HasValue ? (object)nuevaRedet.Nro.Value : DBNull.Value);
                datos.agregarParametro("@TIPO", nuevaRedet.Tipo);
                datos.agregarParametro("@ETAPA", nuevaRedet.Etapa.Id);
                datos.agregarParametro("@OBSERVACIONES", nuevaRedet.Observaciones ?? (object)DBNull.Value);
                datos.agregarParametro("@PORCENTAJE", nuevaRedet.Porcentaje ?? (object)DBNull.Value);
                datos.ejecutarAccion();
                return true;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Hubo un problema al intentar agregar la redeterminación.", ex);
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        public bool eliminar(int codigo)
        {
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("DELETE FROM REDETERMINACIONES WHERE CODIGO_REDET = @ID");
                datos.agregarParametro("@ID", codigo);
                datos.ejecutarAccion();
                return true;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Hubo un problema al intentar eliminar la redeterminación.", ex);
            }
            finally
            {
                datos.cerrarConexion();
            }
        }
        public List<Redeterminacion> listar(List<string> etapa, List<string> codigoAutorizante, List<string> obra, string filtro = null)
        {
            List<Redeterminacion> lista = new List<Redeterminacion>();
            AccesoDatos datos = new AccesoDatos();

            try
            {
                string query = @"SELECT 
                            R.ID,
                            O.ID AS OBRA_ID,
							O.DESCRIPCION,
                            R.CODIGO_AUTORIZANTE,
                            R.EXPEDIENTE,
                            R.SALTO,
                            R.NRO,
                            R.TIPO,
                            R.ETAPA,
                            R.OBSERVACIONES,
							EM.NOMBRE AS EMPRESA,
							AR.NOMBRE AS AREA,
                            R.CODIGO_REDET,
                            E.ID AS ETAPA_ID,
                            E.NOMBRE AS ETAPA_NOMBRE,
                            R.PORCENTAJE_PONDERACION AS PORCENTAJE,
							PS.[BUZON DESTINO], PS.[FECHA ULTIMO PASE]
                        FROM 
                            REDETERMINACIONES AS R
                        INNER JOIN 
                            ESTADOS_REDET AS E ON R.ETAPA = E.ID
							inner join AUTORIZANTES A ON R.CODIGO_AUTORIZANTE = A.CODIGO_AUTORIZANTE
							inner join OBRAS O on A.OBRA = O.ID
							INNER JOIN EMPRESAS EM ON O.EMPRESA = EM.ID
							INNER JOIN AREAS AS AR ON O.AREA = AR.ID
							LEFT JOIN PASES_SADE PS ON R.EXPEDIENTE = PS.EXPEDIENTE COLLATE Modern_Spanish_CI_AS
                        WHERE 1=1";
                query += " ORDER BY R.CODIGO_AUTORIZANTE,R.NRO ";
                if (etapa != null && etapa.Count > 0)
                {
                    string etapaParam = string.Join(",", etapa.Select((e, i) => $"@etapa{i}"));
                    query += $" AND E.NOMBRE IN ({etapaParam})";
                    for (int i = 0; i < etapa.Count; i++)
                    {
                        datos.agregarParametro($"@etapa{i}", etapa[i]);
                    }
                }

                if (codigoAutorizante != null && codigoAutorizante.Count > 0)
                {
                    string autorizanteParam = string.Join(",", codigoAutorizante.Select((e, i) => $"@autorizante{i}"));
                    query += $" AND R.CODIGO_AUTORIZANTE IN ({autorizanteParam})";
                    for (int i = 0; i < codigoAutorizante.Count; i++)
                    {
                        datos.agregarParametro($"@autorizante{i}", codigoAutorizante[i]);
                    }
                }

                if (obra != null && obra.Count > 0)
                {
                    string obraParam = string.Join(",", obra.Select((e, i) => $"@obra{i}"));
                    query += $" AND O.DESCRIPCION IN ({obraParam})";
                    for (int i = 0; i < obra.Count; i++)
                    {
                        datos.agregarParametro($"@obra{i}", obra[i]);
                    }
                }

                if (!string.IsNullOrEmpty(filtro))
                {
                    query += @" AND (R.CODIGO_REDET LIKE @filtro 
                        OR R.EXPEDIENTE LIKE @filtro 
                        OR R.OBSERVACIONES LIKE @filtro
                        OR O.DESCRIPCION LIKE @filtro
                        OR EM.NOMBRE LIKE @filtro)";
                    datos.agregarParametro("@filtro", $"%{filtro}%");
                }

                datos.setearConsulta(query);
                datos.ejecutarLectura();

                while (datos.Lector.Read())
                {
                    Redeterminacion aux = new Redeterminacion
                    {
                        Id = (int)datos.Lector["ID"],
                        Autorizante = new Autorizante
                        {
                            CodigoAutorizante = datos.Lector["CODIGO_AUTORIZANTE"] as string
                        },
                        Expediente = datos.Lector["EXPEDIENTE"] as string,
                        Salto = datos.Lector["SALTO"] != DBNull.Value ? (DateTime)datos.Lector["SALTO"] : (DateTime?)null,
                        Nro = datos.Lector["NRO"] != DBNull.Value ? (int)datos.Lector["NRO"] : (int?)null,
                        Tipo = datos.Lector["TIPO"] as string,
                        Observaciones = datos.Lector["OBSERVACIONES"] as string,
                        CodigoRedet = datos.Lector["CODIGO_REDET"] as string,
                        Porcentaje = datos.Lector["PORCENTAJE"] != DBNull.Value ? (decimal)datos.Lector["PORCENTAJE"] : (decimal?)null,
                        Obra = new Obra {Descripcion = datos.Lector["DESCRIPCION"] as string, Id = (int)datos.Lector["OBRA_ID"] },
                        FechaSade = datos.Lector["FECHA ULTIMO PASE"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(datos.Lector["FECHA ULTIMO PASE"]) : null,
                        BuzonSade = datos.Lector["BUZON DESTINO"]?.ToString(),

                        Empresa = datos.Lector["EMPRESA"] as string,


                        Area = datos.Lector["AREA"] as string

                        ,
                        Etapa = new EstadoRedet
                        {
                            Id = (int)datos.Lector["ETAPA_ID"],
                            Nombre = datos.Lector["ETAPA_NOMBRE"] as string
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


        public List<Redeterminacion> listar()
        {
            List<Redeterminacion> lista = new List<Redeterminacion>();
            AccesoDatos datos = new AccesoDatos();

            try
            {
                string query = @"SELECT 
                            R.ID,
                            O.ID AS OBRA_ID,
							O.DESCRIPCION, 
                            R.CODIGO_AUTORIZANTE,
                            R.EXPEDIENTE,
                            R.SALTO,
                            R.NRO,
                            R.TIPO,
                            R.ETAPA,
                            R.OBSERVACIONES,
							EM.NOMBRE AS EMPRESA,
							AR.NOMBRE AS AREA,
                            R.CODIGO_REDET,
                            E.ID AS ETAPA_ID,
                            E.NOMBRE AS ETAPA_NOMBRE,
                            R.PORCENTAJE_PONDERACION AS PORCENTAJE,
							PS.[BUZON DESTINO], PS.[FECHA ULTIMO PASE]
                        FROM 
                            REDETERMINACIONES AS R
                        INNER JOIN 
                            ESTADOS_REDET AS E ON R.ETAPA = E.ID
							inner join AUTORIZANTES A ON R.CODIGO_AUTORIZANTE = A.CODIGO_AUTORIZANTE
							inner join OBRAS O on A.OBRA = O.ID
							INNER JOIN EMPRESAS EM ON O.EMPRESA = EM.ID
							INNER JOIN AREAS AS AR ON O.AREA = AR.ID
							LEFT JOIN PASES_SADE PS ON R.EXPEDIENTE = PS.EXPEDIENTE COLLATE Modern_Spanish_CI_AS
                        WHERE 1=1 ";
                query += " ORDER BY R.CODIGO_AUTORIZANTE,R.NRO ";
                datos.setearConsulta(query);
                datos.ejecutarLectura();

                while (datos.Lector.Read())
                {
                    Redeterminacion aux = new Redeterminacion
                    {
                        Id = (int)datos.Lector["ID"],
                        Autorizante = new Autorizante
                        {
                            CodigoAutorizante = datos.Lector["CODIGO_AUTORIZANTE"] as string
                        },
                        Expediente = datos.Lector["EXPEDIENTE"] as string,
                        Salto = datos.Lector["SALTO"] != DBNull.Value ? (DateTime)datos.Lector["SALTO"] : (DateTime?)null,
                        Nro = datos.Lector["NRO"] != DBNull.Value ? (int)datos.Lector["NRO"] : (int?)null,
                        Tipo = datos.Lector["TIPO"] as string,
                        Observaciones = datos.Lector["OBSERVACIONES"] as string,
                        CodigoRedet = datos.Lector["CODIGO_REDET"] as string,
                        Porcentaje = datos.Lector["PORCENTAJE"] != DBNull.Value ? (decimal)datos.Lector["PORCENTAJE"] : (decimal?)null,
                        Obra = new Obra { Descripcion = datos.Lector["DESCRIPCION"] as string, Id = (int)datos.Lector["OBRA_ID"] },
                        FechaSade = datos.Lector["FECHA ULTIMO PASE"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(datos.Lector["FECHA ULTIMO PASE"]) : null,
                        BuzonSade = datos.Lector["BUZON DESTINO"]?.ToString(),

                        Empresa = datos.Lector["EMPRESA"] as string,


                        Area = datos.Lector["AREA"] as string

                        ,
                        Etapa = new EstadoRedet
                        {
                            Id = (int)datos.Lector["ETAPA_ID"],
                            Nombre = datos.Lector["ETAPA_NOMBRE"] as string
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

        public bool ActualizarEstado(Redeterminacion autorizante)
        {
            var datos = new AccesoDatos();

            try
            {
                datos.setearConsulta(@"
        UPDATE REDETERMINACIONES 
        SET 
            ETAPA = @estado 
           WHERE ID = @id");

                datos.agregarParametro("@estado", autorizante.Etapa.Id);
                datos.agregarParametro("@id", autorizante.Id);

                datos.ejecutarAccion();
                return true;
            }  
            catch (Exception ex)
            {
                throw new Exception("Error al modificar la redeterminacion.", ex);
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

    }
}
