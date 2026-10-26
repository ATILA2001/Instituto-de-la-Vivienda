using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace Negocio
{
    public class AccesoDatos : IDisposable
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
            var connectionString = ConfigurationManager.ConnectionStrings["ConnectionStringTest"].ConnectionString; // TEST.
            //var connectionString = ConfigurationManager.ConnectionStrings["ConnectionStringProd"].ConnectionString; // PRODUCCION.

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
            comando.Connection = conexion;
            conexion.Open();
            lector = comando.ExecuteReader();
        }
        /// <summary>
        /// Cierra el lector (si lo hay) y la conexión, haya o no lector abierto.
        /// Antes sólo se cerraba la conexión cuando existía un lector, por lo que
        /// ejecutarAccion/ejecutarAccionScalar dejaban la conexión abierta hasta el GC
        /// y bajo carga se agotaba el pool de conexiones.
        /// </summary>
        public void cerrarConexion()
        {
            if (lector != null)
            {
                lector.Close();
                lector = null;
            }
            if (conexion != null && conexion.State != System.Data.ConnectionState.Closed)
            {
                conexion.Close();
            }
        }

        public void Dispose()
        {
            cerrarConexion();
            comando?.Dispose();
            conexion?.Dispose();
        }
        public void ejecutarAccion()
        {
            comando.Connection = conexion;
            conexion.Open();
            comando.ExecuteNonQuery();
        }
        public string ejecutarAccionScalar()
        {
            comando.Connection = conexion;
            conexion.Open();
            return comando.ExecuteScalar().ToString();
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
