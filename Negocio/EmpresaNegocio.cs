using Dominio;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negocio
{
    public class EmpresaNegocio
    {
        public DataTable listarddl()
        {
            DataTable dt = new DataTable(); 
            AccesoDatos datos = new AccesoDatos();

            try
            {
                datos.setearConsulta("SELECT * FROM EMPRESAS  ORDER BY NOMBRE");
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

        public List<Empresa> listar()
        {
            List<Empresa> lista = new List<Empresa>();
            AccesoDatos datos = new AccesoDatos();

            try
            {
                datos.setearConsulta("SELECT * FROM EMPRESAS  ORDER BY NOMBRE");
                datos.ejecutarLectura();
                while (datos.Lector.Read())
                {

                    Empresa aux = new Empresa();
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
        public void agregar(Empresa empresa)
        {
            AccesoDatos datos = new AccesoDatos();
            try
            {
                if (empresa != null)
                {
                    datos.setearConsulta("INSERT INTO EMPRESAS (NOMBRE) VALUES (@Nombre)");
                    datos.setearParametros("@Nombre", empresa.Nombre);
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
                datos.setearConsulta("DELETE FROM EMPRESAS WHERE ID = @ID;");
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
        public void modificar(Empresa empresa)
        {
            AccesoDatos datos = new AccesoDatos();
            try
            {
                if (empresa != null)
                {
                    datos.setearConsulta("UPDATE EMPRESAS SET NOMBRE = @NOMBRE WHERE ID = @ID;");
                    datos.setearParametros("@NOMBRE", empresa.Nombre);
                    datos.setearParametros("@ID", empresa.Id);
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
