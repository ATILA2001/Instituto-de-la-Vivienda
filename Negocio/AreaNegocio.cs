using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dominio;

namespace Negocio
{
    public class AreaNegocio
    {
        public DataTable listarddl()
        {
            DataTable dt = new DataTable(); // DataTable donde guardaremos las empresas.
            AccesoDatos datos = new AccesoDatos();

            try
            {
                datos.setearConsulta("SELECT * FROM AREAS");
                datos.ejecutarLectura();

                // Definir las columnas del DataTable.
                dt.Columns.Add("ID");
                dt.Columns.Add("NOMBRE");

                while (datos.Lector.Read())
                {
                    // Crear una nueva fila y asignar los valores obtenidos.
                    DataRow row = dt.NewRow();
                    row["ID"] = (int)datos.Lector["ID"];
                    row["NOMBRE"] = (string)datos.Lector["NOMBRE"];

                    // Agregar la fila al DataTable.
                    dt.Rows.Add(row);
                }

                return dt;
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
        public List<Area> listar()
        {
            List<Area> lista = new List<Area>();
            AccesoDatos datos = new AccesoDatos();

            try
            {
                datos.setearConsulta("SELECT * FROM AREAS");
                datos.ejecutarLectura();
                while (datos.Lector.Read())
                {

                    Area aux = new Area();
                    aux.Id = (int)datos.Lector["ID"];
                    aux.Nombre = (string)datos.Lector["NOMBRE"];


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
        public void agregar(Area area)
        {
            AccesoDatos datos = new AccesoDatos();
            try
            {
                if (area != null)
                {
                    datos.setearConsulta("INSERT INTO AREAS (NOMBRE) VALUES (@Nombre)");
                    datos.setearParametros("@Nombre", area.Nombre);
                    //datos.setearParametros("@Estado_M", true);
                    datos.ejecutarAccion();
                    datos.cerrarConexion();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        //public void modificar(Area marca)
        //{
        //    AccesoDatos datos = new AccesoDatos();
        //    try
        //    {
        //        if (marca != null)
        //        {
        //            datos.setearProcedimiento("spActualizarMarca");
        //            datos.setearParametros("@Cod_Marca", marca.Cod_Marca);
        //            datos.setearParametros("@Nombre_M", marca.Nombre);
        //            datos.setearParametros("@ImgURL_M", marca.ImagenURL);
        //            datos.setearParametros("@Estado_M", true);
        //            datos.ejecutarAccion();
        //            datos.cerrarConexion();
        //        }
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }
        //}

        //public bool eliminar(string cod)
        //{
        //    AccesoDatos datos = new AccesoDatos();
        //    try
        //    {
        //        datos.setearProcedimiento("spEliminarMarca");
        //        datos.setearParametros("@Cod_Marca", cod);
        //        datos.ejecutarAccion();
        //        return true;
        //    }
        //    catch (Exception)
        //    {
        //        return false;
        //    }
        //    finally { datos.cerrarConexion(); }
        //}

    }
}
