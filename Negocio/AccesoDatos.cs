using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace Negocio
{
    public class AccesoDatos
    {

        private SqlConnection conexion;
        private SqlCommand comando;
        private SqlDataReader lector;

        public SqlDataReader Lector
        {
            get
            {
                return lector;
            }
        }
        public AccesoDatos()
        {
            // COMENTADO POR CIERRE PLANIFICACION Y MERGE EN RAMA MAIN.
            //var connectionString = ConfigurationManager.ConnectionStrings["ConnectionStringTest"].ConnectionString;
            var connectionString = ConfigurationManager.ConnectionStrings["ConnectionStringProd"].ConnectionString;
            Console.WriteLine(connectionString);
            conexion = new SqlConnection(connectionString);
            comando = new SqlCommand();
        }

        public void setearConsulta(string consulta)
        {
            comando.CommandType = System.Data.CommandType.Text;
            comando.CommandText = consulta;
        }
        public void setearProcedimiento(string procedimiento)
        {
            comando.CommandType = System.Data.CommandType.StoredProcedure;
            comando.CommandText = procedimiento;
        }

        public SqlDataReader cargarControl(string consulta)
        {
            conexion.Open();
            comando = new SqlCommand(consulta, conexion);

            lector = comando.ExecuteReader();
            return lector;
        }

        public void ejecutarLectura()
        {
            try
            {
                comando.Connection = conexion;
                conexion.Open();
                lector = comando.ExecuteReader();
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
        public void cerrarConexion()
        {
            if (lector != null)
            {
                lector.Close();
                conexion.Close();
            }
        }
        public void ejecutarAccion()
        {
            comando.Connection = conexion;
            try
            {
                conexion.Open();
                comando.ExecuteNonQuery();
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
        public string ejecutarAccionScalar()
        {
            comando.Connection = conexion;
            try
            {
                conexion.Open();
                return comando.ExecuteScalar().ToString();
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public void setearParametros(string nombre, object valor)
        {
            comando.Parameters.AddWithValue(nombre, valor);
        }

        public void agregarParametro(string nombreParametro, object valor)
        {
            SqlParameter parametro = new SqlParameter(nombreParametro, valor);
            comando.Parameters.Add(parametro);
        }
    }
}
