using Dominio;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negocio
{
    public class ContrataNegocio
    {
        public DataTable listarddl()
        {
            DataTable dt = new DataTable(); 
            AccesoDatos datos = new AccesoDatos();

            try
            {
                datos.setearConsulta("SELECT * FROM CONTRATA");
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
                datos.setearConsulta("SELECT * FROM CONTRATA");
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
    }
}
