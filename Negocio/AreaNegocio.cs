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
            DataTable dt = new DataTable();
            AccesoDatos datos = new AccesoDatos();

            try
            {
                datos.setearConsulta("SELECT * FROM AREAS ORDER BY NOMBRE");
                datos.ejecutarLectura();

                dt.Columns.Add("ID");
                dt.Columns.Add("NOMBRE");

                while (datos.Lector.Read())
                {
                    DataRow row = dt.NewRow();
                    row["ID"] = (int)datos.Lector["ID"];
                    row["NOMBRE"] = (string)datos.Lector["NOMBRE"];

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
                datos.setearConsulta("SELECT * FROM AREAS  ORDER BY NOMBRE");
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
                    datos.ejecutarAccion();
                    datos.cerrarConexion();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
