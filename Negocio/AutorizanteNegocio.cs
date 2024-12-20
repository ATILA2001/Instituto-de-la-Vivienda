using Dominio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negocio
{
    public class AutorizanteNegocio
    {
        public List<Autorizante> listar(Usuario usuario)
        {
            List<Autorizante> lista = new List<Autorizante>();
            AccesoDatos datos = new AccesoDatos();

            try
            {
                datos.setearConsulta(@"
            SELECT 
                CONCAT(C.NOMBRE, ' ', O.NUMERO, '/', O.AÑO) AS CONTRATA, 
                O.DESCRIPCION AS OBRA, 
                A.CODIGO_AUTORIZANTE, 
                A.DETALLE, 
                A.CONCEPTO, 
                E.NOMBRE AS ESTADO, 
                E.ID AS ESTADO_ID, 
                A.EXPEDIENTE, 
                A.MONTO_AUTORIZADO, 
                A.AUTORIZACION_GG, 
                AR.NOMBRE AS AREA, 
                AR.ID AS AREA_ID, 
                C.ID AS CONTRATA_ID
            FROM 
                AUTORIZANTES AS A
            INNER JOIN 
                OBRAS AS O ON A.OBRA = O.ID
            INNER JOIN 
                ESTADOS_AUTORIZANTES AS E ON A.ESTADO = E.ID
            INNER JOIN 
                CONTRATA AS C ON O.CONTRATA = C.ID
            INNER JOIN 
                BD_PROYECTOS AS B ON O.ID = B.ID_BASE
            INNER JOIN 
                AREAS AS AR ON O.AREA = AR.ID
            WHERE 
                O.AREA = @area");
                datos.agregarParametro("@area", usuario.Area.Id);
                datos.ejecutarLectura();
                while (datos.Lector.Read())
                {

                    Autorizante aux = new Autorizante();
                    aux.CodigoAutorizante = datos.Lector["CODIGO_AUTORIZANTE"] as string;
                    aux.Detalle = datos.Lector["DETALLE"] as string;
                    aux.Concepto = datos.Lector["CONCEPTO"] as string;
                    aux.Estado = new EstadoAutorizante
                    {
                        Nombre = datos.Lector["ESTADO"] as string,
                        Id = (int)datos.Lector["ESTADO_ID"]
                    };

                    aux.Expediente = datos.Lector["EXPEDIENTE"] as string;
                    aux.MontoAutorizado = datos.Lector["MONTO_AUTORIZADO"] != DBNull.Value ? (decimal)datos.Lector["MONTO_AUTORIZADO"] : 0M;
                    aux.AutorizacionGG = (bool)datos.Lector["AUTORIZACION_GG"];


                    aux.Obra = new Obra
                    {
                        Descripcion = datos.Lector["OBRA"] as string,
                        Contrata = new Contrata
                        {
                            Id = (int)datos.Lector["CONTRATA_ID"],  // Asignamos el ID de la Contrata
                            Nombre = datos.Lector["CONTRATA"] as string  // Asignamos el nombre de la Contrata
                        },

                        // Asignamos el Area a la propiedad Obra
                        Area = new Area
                        {
                            Id = (int)datos.Lector["AREA_ID"],  // Asignamos el ID del Area
                            Nombre = datos.Lector["AREA"] as string  // Asignamos el nombre del Area
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

        public List<Autorizante> listar()
        {
            List<Autorizante> lista = new List<Autorizante>();
            AccesoDatos datos = new AccesoDatos();

            try
            {
                datos.setearConsulta(@"
            SELECT 
                CONCAT(C.NOMBRE, ' ', O.NUMERO, '/', O.AÑO) AS CONTRATA, 
                O.DESCRIPCION AS OBRA, 
                A.CODIGO_AUTORIZANTE, 
                A.DETALLE, 
                A.CONCEPTO, 
                E.NOMBRE AS ESTADO, 
                E.ID AS ESTADO_ID, 
                A.EXPEDIENTE, 
                A.MONTO_AUTORIZADO, 
                A.AUTORIZACION_GG, 
                AR.NOMBRE AS AREA, 
                AR.ID AS AREA_ID, 
                C.ID AS CONTRATA_ID
            FROM 
                AUTORIZANTES AS A
            INNER JOIN 
                OBRAS AS O ON A.OBRA = O.ID
            INNER JOIN 
                ESTADOS_AUTORIZANTES AS E ON A.ESTADO = E.ID
            INNER JOIN 
                CONTRATA AS C ON O.CONTRATA = C.ID
            INNER JOIN 
                BD_PROYECTOS AS B ON O.ID = B.ID_BASE
            INNER JOIN 
                AREAS AS AR ON O.AREA = AR.ID");
                datos.ejecutarLectura();
                while (datos.Lector.Read())
                {

                    Autorizante aux = new Autorizante();
                    aux.CodigoAutorizante = datos.Lector["CODIGO_AUTORIZANTE"] as string;
                    aux.Detalle = datos.Lector["DETALLE"] as string;
                    aux.Concepto = datos.Lector["CONCEPTO"] as string;
                    aux.Estado = new EstadoAutorizante
                    {
                        Nombre = datos.Lector["ESTADO"] as string,
                        Id = (int)datos.Lector["ESTADO_ID"]
                    };

                    aux.Expediente = datos.Lector["EXPEDIENTE"] as string;
                    aux.MontoAutorizado = datos.Lector["MONTO_AUTORIZADO"] != DBNull.Value ? (decimal)datos.Lector["MONTO_AUTORIZADO"] : 0M;
                    aux.AutorizacionGG = (bool)datos.Lector["AUTORIZACION_GG"];


                    aux.Obra = new Obra
                    {
                        Descripcion = datos.Lector["OBRA"] as string,
                        Contrata = new Contrata
                        {
                            Id = (int)datos.Lector["CONTRATA_ID"],  // Asignamos el ID de la Contrata
                            Nombre = datos.Lector["CONTRATA"] as string  // Asignamos el nombre de la Contrata
                        },

                        // Asignamos el Area a la propiedad Obra
                        Area = new Area
                        {
                            Id = (int)datos.Lector["AREA_ID"],  // Asignamos el ID del Area
                            Nombre = datos.Lector["AREA"] as string  // Asignamos el nombre del Area
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
        /*
        public bool agregar(Obra nuevaObra)
        {
            AccesoDatos datos = new AccesoDatos();
            try
            {
                // Consulta para insertar la nueva obra
                datos.setearConsulta("INSERT INTO OBRAS (EMPRESA, AREA, CONTRATA, NUMERO, AÑO, ETAPA, OBRA, BARRIO, DESCRIPCION) " +
                                     "VALUES (@EMPRESA, @AREA, @CONTRATA, @NUMERO, @AÑO, @ETAPA, @OBRA, @BARRIO, @DESCRIPCION)");

                // Asignar los parámetros
                datos.agregarParametro("@EMPRESA", nuevaObra.Empresa.Id);
                datos.agregarParametro("@AREA", nuevaObra.Area.Id);
                datos.agregarParametro("@CONTRATA", nuevaObra.Contrata.Id);
                datos.agregarParametro("@NUMERO", nuevaObra.Numero);
                datos.agregarParametro("@AÑO", nuevaObra.Año);
                datos.agregarParametro("@ETAPA", nuevaObra.Etapa);
                datos.agregarParametro("@OBRA", nuevaObra.ObraNumero);
                datos.agregarParametro("@BARRIO", nuevaObra.Barrio.Id);
                datos.agregarParametro("@DESCRIPCION", nuevaObra.Descripcion);

                // Ejecutar la inserción
                datos.ejecutarAccion();

                // Si todo fue bien, devolvemos true
                return true;
            }
            catch (Exception ex)
            {
                // En caso de error, lanzamos la excepción para que se maneje donde se llame el método
                throw ex;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }*/
    }
}
