using Dominio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.DirectoryServices.AccountManagement;
using System.Web.Security;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;

namespace Negocio
{
    public class UsuarioNegocio
    {
        public UsuarioNegocio() { }

		// metodo para Traducir correo a cuil, 
		// metodo para Logear con cuil y pass
		// metodo para Logear con correo y pass (que traduce a cuil y pass)

		public void ObtenerCuil(Usuario usuario)
		{
			AccesoDatos datos = new AccesoDatos();
			try
			{
				datos.setearConsulta("SELECT u.ID,u.NOMBRE,TIPO,CORREO,ESTADO,a.NOMBRE as AREA,a.ID as IDAREA, u.USERNAME as USERNAME FROM USUARIOS as u inner join AREAS as a on AREA = a.ID WHERE CORREO = @correo ");
				datos.setearParametros("@correo", usuario.Correo);
				datos.setearParametros("@pass", usuario.Contrasenia);

				// Aca traigo el cuil BEGIN

				// datos.setearParametros("@cuil", usuario.Username);

				// Aca traigo el cuil END
				Debug.Write("@correo?...: " + usuario.Correo);


				datos.ejecutarLectura();

				while (datos.Lector.Read())
				{
					usuario.Correo = datos.Lector["CORREO"] as string ?? string.Empty;
					usuario.Nombre = datos.Lector["NOMBRE"] as string ?? string.Empty;
					usuario.Tipo = datos.Lector["TIPO"] != DBNull.Value && Convert.ToBoolean(datos.Lector["TIPO"]);
					usuario.Estado = datos.Lector["ESTADO"] != DBNull.Value && Convert.ToBoolean(datos.Lector["ESTADO"]);
					usuario.Area = new Area();

					// CUANDO ENTRA POR EL MAIL, VA A OBTENER EL USERNAME Y DSP LO USA PARA EL INICIO DE SESION EN EL DOMINIO
					usuario.Username = datos.Lector["USERNAME"] as string ?? string.Empty;
					//

					usuario.Area.Id = (int)datos.Lector["IDAREA"];
					usuario.Area.Nombre = (string)datos.Lector["AREA"];

					// CON EL CUIL DISPARA EL INICIO DE SESION EN EL DOMINIO
					//return LogearIntegSecur(usuario, usuario.Username);
					//

					return;
				}

				return;
			}
			catch (Exception)
			{
				throw; // You might want to log this or provide more context.
			}
			finally
			{
				datos.cerrarConexion();
			}
		}
		public bool LogearIntegSecur(Usuario usuario, string userName)
        
        {
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("SELECT u.ID,u.NOMBRE,TIPO,CORREO,ESTADO,a.NOMBRE as AREA,a.ID as IDAREA FROM IVC_TEST.DBO.USUARIOS as u inner join IVC_TEST.DBO.AREAS as a on AREA = a.ID WHERE DOMAIN = 'BUENOSAIRES' AND USERNAME = @windowsUserName ");
                datos.setearParametros("@windowsUserName", userName);
                datos.ejecutarLectura();

				Debug.WriteLine("Valor de userName: " + userName);
				// Debug.WriteLine("datos.Lector.Read()!!!!!!!!!!!!!!!: " + datos.Lector.Read());
				// Debug.WriteLine(datos.Lector[0].ToString());
				var contexto = new PrincipalContext(ContextType.Domain, "BUENOSAIRES");


				// Validar las credenciales del usuario
				Debug.Write("¿Credenciales válidas?...: ");
				Debug.Write("¿userName?...: " + userName);
				bool resultado = contexto.ValidateCredentials("BUENOSAIRES\\" + usuario.Username, usuario.Contrasenia);
				if (!resultado)
				{
					return false;
				}

				Debug.Write("-----¿Credenciales válidas?--------------------------: " + resultado);


				while (datos.Lector.Read())
                {
					// Debug.WriteLine(datos.Lector[0].ToString());
					// Debug.WriteLine(datos.Lector.GetName(0));
					usuario.Correo = datos.Lector["CORREO"] as string ?? string.Empty;
                    usuario.Nombre = datos.Lector["NOMBRE"] as string ?? string.Empty;
                    usuario.Tipo = datos.Lector["TIPO"] != DBNull.Value && Convert.ToBoolean(datos.Lector["TIPO"]);
                    usuario.Estado = datos.Lector["ESTADO"] != DBNull.Value && Convert.ToBoolean(datos.Lector["ESTADO"]);
                    usuario.Area = new Area();
                    usuario.Area.Id = (int)datos.Lector["IDAREA"];
                    usuario.Area.Nombre = (string)datos.Lector["AREA"];
                    return true;
                    // && contexto.ValidateCredentials(userName, usuario.Contrasenia);
                }

				Debug.WriteLine("usuario.Correo: " + usuario.Correo);

				return false;
            }
            catch (Exception)
            {
                throw; // You might want to log this or provide more context.
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        
        public bool Logear(Usuario usuario)
        {
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("SELECT u.ID,u.NOMBRE,TIPO,CORREO,ESTADO,a.NOMBRE as AREA,a.ID as IDAREA FROM USUARIOS as u inner join AREAS as a on AREA = a.ID WHERE CORREO = @correo ");
                datos.setearParametros("@correo", usuario.Correo);
                datos.setearParametros("@pass", usuario.Contrasenia);
                datos.ejecutarLectura();

                while (datos.Lector.Read())
                {
                    usuario.Correo = datos.Lector["CORREO"] as string ?? string.Empty;
                    usuario.Nombre = datos.Lector["NOMBRE"] as string ?? string.Empty;
                    usuario.Tipo = datos.Lector["TIPO"] != DBNull.Value && Convert.ToBoolean(datos.Lector["TIPO"]);
                    usuario.Estado = datos.Lector["ESTADO"] != DBNull.Value && Convert.ToBoolean(datos.Lector["ESTADO"]);
                    usuario.Area = new Area();

					// CUANDO ENTRA POR EL MAIL, VA A OBTENER EL USERNAME Y DSP LO USA PARA EL INICIO DE SESION EN EL DOMINIO
					usuario.Username = datos.Lector["USERNAME"] as string ?? string.Empty;
                    //

					usuario.Area.Id = (int)datos.Lector["IDAREA"];
                    usuario.Area.Nombre = (string)datos.Lector["AREA"];

					// CON EL CUIL DISPARA EL INICIO DE SESION EN EL DOMINIO
					return LogearIntegSecur(usuario, usuario.Username);
					//

					//return true;
				}

				return false;
            }
            catch (Exception)
            {
                throw; // You might want to log this or provide more context.
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        public string registrarUsuario(Usuario usuario)
        {
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("INSERT INTO USUARIOS (NOMBRE, CORREO, AREA) OUTPUT INSERTED.NOMBRE VALUES (@nombre,@correo, @area);");
                datos.setearParametros("@correo", usuario.Correo);
                datos.setearParametros("@nombre", usuario.Nombre);
                datos.setearParametros("@area", usuario.Area.Id);
                return datos.ejecutarAccionScalar();
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                datos.cerrarConexion();
            }
        }

        public List<Usuario> listarUsuario()
        {
            List<Usuario> lista = new List<Usuario>();
            AccesoDatos datos = new AccesoDatos();

            try
            {
                datos.setearConsulta(@"SELECT u.ID, u.NOMBRE, u.TIPO, u.CORREO, u.ESTADO, 
                                      a.NOMBRE AS AreaNombre, a.ID AS AreaId 
                               FROM USUARIOS u 
                               INNER JOIN AREAS a ON u.AREA = a.ID");
                datos.ejecutarLectura();

                while (datos.Lector.Read())
                {
                    Usuario aux = new Usuario
                    {
                        Id = Convert.ToInt32(datos.Lector["ID"]),
                        Nombre = datos.Lector["NOMBRE"].ToString(),
                        Tipo = Convert.ToBoolean(datos.Lector["TIPO"]),
                        Correo = datos.Lector["CORREO"].ToString(),
                        Estado = Convert.ToBoolean(datos.Lector["ESTADO"]),
                        Area = new Area
                        {
                            Id = Convert.ToInt32(datos.Lector["AreaId"]),
                            Nombre = datos.Lector["AreaNombre"].ToString()
                        }
                    };

                    lista.Add(aux);
                }

                return lista;
            }
            catch (Exception ex)
            {
                // Puedes implementar un registro de errores aquí en lugar de relanzar la excepción directamente
                throw new Exception("Error al listar usuarios", ex);
            }
            finally
            {
                datos.cerrarConexion();
            }
        }
        public List<Usuario> listarUsuarioPendiente()
        {
            List<Usuario> lista = new List<Usuario>();
            AccesoDatos datos = new AccesoDatos();

            try
            {
                datos.setearConsulta(@"SELECT u.ID, u.NOMBRE, u.TIPO, u.CORREO, u.ESTADO, 
                                      a.NOMBRE AS AreaNombre, a.ID AS AreaId 
                               FROM USUARIOS u 
                               INNER JOIN AREAS a ON u.AREA = a.ID where u.ESTADO = 0");
                datos.ejecutarLectura();

                while (datos.Lector.Read())
                {
                    Usuario aux = new Usuario
                    {
                        Id = Convert.ToInt32(datos.Lector["ID"]),
                        Nombre = datos.Lector["NOMBRE"].ToString(),
                        Tipo = Convert.ToBoolean(datos.Lector["TIPO"]),
                        Correo = datos.Lector["CORREO"].ToString(),
                        Estado = Convert.ToBoolean(datos.Lector["ESTADO"]),
                        Area = new Area
                        {
                            Id = Convert.ToInt32(datos.Lector["AreaId"]),
                            Nombre = datos.Lector["AreaNombre"].ToString()
                        }
                    };

                    lista.Add(aux);
                }

                return lista;
            }
            catch (Exception ex)
            {
                // Puedes implementar un registro de errores aquí en lugar de relanzar la excepción directamente
                throw new Exception("Error al listar usuarios", ex);
            }
            finally
            {
                datos.cerrarConexion();
            }
        }
        public bool eliminar(object codP)
        {
            AccesoDatos datos = new AccesoDatos();

            try
            {
                if (codP != null)
                {
                    datos.setearConsulta("DELETE FROM USUARIOS WHERE ID = @ID");
                    datos.setearParametros("@ID", codP);
                    datos.ejecutarAccion();
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al intentar eliminar el usuario", ex);
            }
            finally
            {
                datos.cerrarConexion();
            }
        }
        public bool ModificarUsuario(Usuario usuario)
        {
            AccesoDatos datos = new AccesoDatos();
            try
            {
                if (usuario != null)
                {
                    datos.setearConsulta("UPDATE USUARIOS SET NOMBRE = @NOMBRE, CORREO = @CORREO, TIPO = @TIPO, ESTADO = @ESTADO WHERE ID = @ID");
                    datos.setearParametros("@NOMBRE", usuario.Nombre);
                    datos.setearParametros("@CORREO", usuario.Correo);
                    datos.setearParametros("@TIPO", usuario.Tipo);
                    datos.setearParametros("@ESTADO", usuario.Estado);
                    datos.setearParametros("@ID", usuario.Id);
                    datos.ejecutarAccion();
                    return true;
                }
                return false;
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                datos.cerrarConexion();
            }

        }

        public DataTable listarDdlRedet()
        {
            DataTable dt = new DataTable();
            AccesoDatos datos = new AccesoDatos();

            try
            {
                datos.setearConsulta(@"SELECT u.ID, u.NOMBRE 
                                       FROM USUARIOS u
                                       INNER JOIN AREAS a ON u.AREA = a.ID 
                                       WHERE u.ESTADO = 1 AND a.ID = 16
                                       ORDER BY u.NOMBRE");
                datos.ejecutarLectura();

                // Definir las columnas del DataTable.
                dt.Columns.Add("ID", typeof(int));
                dt.Columns.Add("NOMBRE", typeof(string));

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
                throw new Exception("Error al listar usuarios para ddl Redet", ex);
            }
            finally
            {
                datos.cerrarConexion();
            }
        }
    }
}
