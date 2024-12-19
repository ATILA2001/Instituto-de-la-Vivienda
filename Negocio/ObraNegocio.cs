using Dominio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negocio
{
    public class ObraNegocio
    {
        public List<Obra> listar(Usuario usuario)
        {
            List<Obra> lista = new List<Obra>();
            AccesoDatos datos = new AccesoDatos();

            try
            {
                datos.setearConsulta("SELECT O.ID,A.NOMBRE AS AREA,E.NOMBRE AS EMPRESA,NUMERO,C.NOMBRE AS CONTRATA,AÑO,ETAPA,OBRA,B.NOMBRE AS BARRIO,DESCRIPCION FROM OBRAS AS O INNER JOIN EMPRESAS AS E ON O.EMPRESA = E.ID INNER JOIN AREAS AS A ON O.AREA = A.ID INNER JOIN CONTRATA AS C ON O.CONTRATA = C.ID INNER JOIN BARRIOS AS B ON O.BARRIO = B.ID where O.AREA = @area");
                datos.agregarParametro("@area", usuario.Area.Id);
                datos.ejecutarLectura();
                while (datos.Lector.Read())
                {

                    Obra aux = new Obra();
                    aux.Id = (int)datos.Lector["ID"];
                    aux.Descripcion = datos.Lector["DESCRIPCION"] as string;
                    aux.Numero = datos.Lector["NUMERO"] as int?;
                    aux.Año = datos.Lector["AÑO"] as int?;
                    aux.Etapa = datos.Lector["ETAPA"] as int?;
                    aux.ObraNumero = datos.Lector["OBRA"] as int?;

                    aux.Barrio = new Barrio
                    {
                        Id = (int)datos.Lector["ID"],
                        Nombre = datos.Lector["BARRIO"] as string
                    };

                    aux.Area = new Area
                    {
                        Id = (int)datos.Lector["ID"],
                        Nombre = datos.Lector["AREA"] as string
                    };

                    aux.Empresa = new Empresa
                    {
                        Id = (int)datos.Lector["ID"], 
                        Nombre = datos.Lector["EMPRESA"] as string
                    };

                    aux.Contrata = new Contrata
                    {
                        Id = (int)datos.Lector["ID"], 
                        Nombre = datos.Lector["CONTRATA"] as string
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
        }
    }
}
