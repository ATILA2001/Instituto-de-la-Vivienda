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
                datos.setearConsulta("DELETE FROM REDETERMINACIONES WHERE ID = @ID");
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


                if (etapa != null && etapa.Count > 0)
                {
                    // E.ID es la columna correcta para el ID de la etapa en la tabla ESTADOS_REDET
                    string etapaParam = string.Join(",", etapa.Select((id, i) => $"@etapaId{i}"));
                    query += $" AND E.ID IN ({etapaParam})";
                    for (int i = 0; i < etapa.Count; i++)
                    {
                        // Los IDs ya son strings, la BD los convertirá si la columna es numérica.
                        datos.agregarParametro($"@etapaId{i}", etapa[i]);
                    }
                }

                if (codigoAutorizante != null && codigoAutorizante.Count > 0)
                {
                    // A.ID es la columna correcta para el ID del autorizante en la tabla AUTORIZANTES
                    string autorizanteParam = string.Join(",", codigoAutorizante.Select((id, i) => $"@autorizanteId{i}"));
                    query += $" AND A.ID IN ({autorizanteParam})";
                    for (int i = 0; i < codigoAutorizante.Count; i++)
                    {
                        datos.agregarParametro($"@autorizanteId{i}", codigoAutorizante[i]);
                    }
                }

                if (obra != null && obra.Count > 0)
                {
                    // O.ID es la columna correcta para el ID de la obra en la tabla OBRAS
                    string obraParam = string.Join(",", obra.Select((id, i) => $"@obraId{i}"));
                    query += $" AND O.ID IN ({obraParam})";
                    for (int i = 0; i < obra.Count; i++)
                    {
                        datos.agregarParametro($"@obraId{i}", obra[i]);
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
                            CodigoAutorizante = datos.Lector["CODIGO_AUTORIZANTE"] as string,
                            Obra = new Obra
                            {
                                Descripcion = datos.Lector["DESCRIPCION"] as string,
                                Id = (int)datos.Lector["OBRA_ID"]
                            }
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
                        Area = datos.Lector["AREA"] as string,
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
                    AR.ID AS AREA_ID,
                    AR.NOMBRE AS AREA,
                    CONCAT(C.NOMBRE, ' ', O.NUMERO, '/', O.AÑO) AS CONTRATA_NOMBRE,
                    C.ID AS CONTRATA_ID,
                    R.CODIGO_REDET,
                    E.ID AS ETAPA_ID,
                    E.NOMBRE AS ETAPA_NOMBRE,
                    R.PORCENTAJE_PONDERACION AS PORCENTAJE,
                    PS.[BUZON DESTINO], 
                    PS.[FECHA ULTIMO PASE],
                    R.ID_USUARIO,
                U.NOMBRE AS USUARIO_NOMBRE
                FROM 
                    REDETERMINACIONES AS R
                INNER JOIN 
                    ESTADOS_REDET AS E ON R.ETAPA = E.ID
                inner join AUTORIZANTES A ON R.CODIGO_AUTORIZANTE = A.CODIGO_AUTORIZANTE
                inner join OBRAS O on A.OBRA = O.ID
                INNER JOIN EMPRESAS EM ON O.EMPRESA = EM.ID
                INNER JOIN AREAS AS AR ON O.AREA = AR.ID
                LEFT JOIN CONTRATA AS C ON O.CONTRATA = C.ID
                LEFT JOIN PASES_SADE PS ON R.EXPEDIENTE = PS.EXPEDIENTE COLLATE Modern_Spanish_CI_AS
                LEFT JOIN USUARIOS U ON R.ID_USUARIO = U.ID
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
                            CodigoAutorizante = datos.Lector["CODIGO_AUTORIZANTE"] as string,
                            Obra = new Obra
                            {
                                Descripcion = datos.Lector["DESCRIPCION"] as string,
                                Id = (int)datos.Lector["OBRA_ID"],
                                Area = new Area { Id = (int)datos.Lector["AREA_ID"], Nombre = datos.Lector["AREA"] as string },
                                Contrata = datos.Lector["CONTRATA_ID"] != DBNull.Value ?
                                new Contrata
                                {
                                    Id = (int)datos.Lector["CONTRATA_ID"],
                                    Nombre = datos.Lector["CONTRATA_NOMBRE"] as string
                                } : null
                            }
                        },
                        Expediente = datos.Lector["EXPEDIENTE"] as string,
                        Salto = datos.Lector["SALTO"] != DBNull.Value ? (DateTime)datos.Lector["SALTO"] : (DateTime?)null,
                        Nro = datos.Lector["NRO"] != DBNull.Value ? (int)datos.Lector["NRO"] : (int?)null,
                        Tipo = datos.Lector["TIPO"] as string,
                        Observaciones = datos.Lector["OBSERVACIONES"] as string,
                        CodigoRedet = datos.Lector["CODIGO_REDET"] as string,
                        Porcentaje = datos.Lector["PORCENTAJE"] != DBNull.Value ? (decimal)datos.Lector["PORCENTAJE"] : (decimal?)null,

                        FechaSade = datos.Lector["FECHA ULTIMO PASE"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(datos.Lector["FECHA ULTIMO PASE"]) : null,
                        BuzonSade = datos.Lector["BUZON DESTINO"]?.ToString(),
                        Empresa = datos.Lector["EMPRESA"] as string,
                        Area = datos.Lector["AREA"] as string,
                        Etapa = new EstadoRedet
                        {
                            Id = (int)datos.Lector["ETAPA_ID"],
                            Nombre = datos.Lector["ETAPA_NOMBRE"] as string
                        },
                        Usuario = datos.Lector["ID_USUARIO"] != DBNull.Value ?
                    new Usuario
                    {
                        Id = (int)datos.Lector["ID_USUARIO"],
                        Nombre = datos.Lector["USUARIO_NOMBRE"] as string
                    } : null
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

        public bool ActualizarExpediente(int id, string expediente)
        {
            var datos = new AccesoDatos();

            try
            {
                datos.setearConsulta(@"
        UPDATE REDETERMINACIONES 
        SET 
            EXPEDIENTE = @expediente
           WHERE ID = @id");

                datos.agregarParametro("@expediente", expediente);
                datos.agregarParametro("@id", id);

                datos.ejecutarAccion();
                return true;
            }
            catch (Exception)
            {
                throw new Exception("Error al modificar el expediente de la redeterminación.");
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
        public bool modificar(Redeterminacion redeterminacion)
        {
            AccesoDatos datos = new AccesoDatos();
            try
            {
                string query = @"
            UPDATE REDETERMINACIONES 
            SET 
                EXPEDIENTE = @EXPEDIENTE,
                SALTO = @SALTO,
                NRO = @NRO,
                TIPO = @TIPO,
                ETAPA = @ETAPA,
                OBSERVACIONES = @OBSERVACIONES,
                PORCENTAJE_PONDERACION = @PORCENTAJE
            WHERE 
                ID = @ID";

                datos.setearConsulta(query);

                // Agregamos los parámetros necesarios
                datos.agregarParametro("@ID", redeterminacion.Id);
                datos.agregarParametro("@EXPEDIENTE", redeterminacion.Expediente);
                datos.agregarParametro("@SALTO", redeterminacion.Salto.HasValue ? (object)redeterminacion.Salto.Value : DBNull.Value);
                datos.agregarParametro("@NRO", redeterminacion.Nro.HasValue ? (object)redeterminacion.Nro.Value : DBNull.Value);
                datos.agregarParametro("@TIPO", redeterminacion.Tipo);
                datos.agregarParametro("@ETAPA", redeterminacion.Etapa.Id);
                datos.agregarParametro("@OBSERVACIONES", redeterminacion.Observaciones ?? (object)DBNull.Value);
                datos.agregarParametro("@PORCENTAJE", redeterminacion.Porcentaje.HasValue ? (decimal)redeterminacion.Porcentaje.Value : 0);

                // Ejecutamos la acción
                datos.ejecutarAccion();
                return true;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Hubo un problema al intentar modificar la redeterminación.", ex);
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        public bool ActualizarUsuario(Redeterminacion redeterminacion)
        {
            var datos = new AccesoDatos();

            try
            {
                datos.setearConsulta(@"
        UPDATE REDETERMINACIONES 
        SET 
            ID_USUARIO = @usuario 
           WHERE ID = @id");

                datos.agregarParametro("@usuario", redeterminacion.Usuario.Id);
                datos.agregarParametro("@id", redeterminacion.Id);

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
