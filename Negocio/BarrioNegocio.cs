using Dominio;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Negocio
{
    public class BarrioNegocio
    {
        public DataTable listarddl()
        {
            DataTable dt = new DataTable(); 
            AccesoDatos datos = new AccesoDatos();

            try
            {
                datos.setearConsulta("SELECT * FROM BARRIOS  ORDER BY NOMBRE");
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
        public List<Barrio> listar(string filtro = null)
        {
            List<Barrio> lista = new List<Barrio>();
            AccesoDatos datos = new AccesoDatos();

            try
            {
                string query = "SELECT * FROM BARRIOS  where 1=1 ";
                if (!string.IsNullOrEmpty(filtro))
                {
                    query += " AND NOMBRE LIKE @filtro ";
                    datos.setearParametros("@filtro", $"%{filtro}%");
                }
                query += "ORDER BY NOMBRE";
                datos.setearConsulta(query);
                datos.ejecutarLectura();
                while (datos.Lector.Read())
                {

                    Barrio aux = new Barrio();
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
        public void agregar(Barrio barrio)
        {
            AccesoDatos datos = new AccesoDatos();
            try
            {
                if (barrio != null)
                {
                    datos.setearConsulta("INSERT INTO BARRIOS (NOMBRE) VALUES (@Nombre)");
                    datos.setearParametros("@Nombre", barrio.Nombre);
                    datos.ejecutarAccion();
                    datos.cerrarConexion();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        public void modificar(Barrio barrio)
        { 
            AccesoDatos datos = new AccesoDatos();
            try
            {
                if (barrio != null)
                {
                    datos.setearConsulta("UPDATE BARRIOS SET NOMBRE = @NOMBRE WHERE ID = @ID;");
                    datos.setearParametros("@NOMBRE", barrio.Nombre);
                    datos.setearParametros("@ID", barrio.Id);
                    datos.ejecutarAccion();
                    datos.cerrarConexion();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public bool eliminar(int id)
        {
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("DELETE FROM BARRIOS WHERE ID = @ID;");
                datos.setearParametros("@ID", id);
                datos.ejecutarAccion();
                return true;
            }
            catch (Exception)
           {
               return false;
            }
           finally { datos.cerrarConexion(); }
        }
    }
}
