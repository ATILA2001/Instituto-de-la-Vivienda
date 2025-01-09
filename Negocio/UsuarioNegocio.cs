using Dominio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Negocio
{
    public class UsuarioNegocio
    {
        public UsuarioNegocio() { }
        public bool Logear(Usuario usuario)
        {
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearConsulta("SELECT u.ID,u.NOMBRE,TIPO,CORREO,ESTADO,a.NOMBRE as AREA,a.ID as IDAREA FROM USUARIOS as u inner join AREAS as a on AREA = a.ID WHERE CORREO = @correo AND CONTRASENIA = @pass ");
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
                    usuario.Area.Id = (int)datos.Lector["IDAREA"];
                    usuario.Area.Nombre = (string)datos.Lector["AREA"];
                    return true;
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
                datos.setearConsulta("INSERT INTO USUARIOS (NOMBRE, CORREO, CONTRASENIA, AREA) OUTPUT INSERTED.NOMBRE VALUES (@nombre,@correo, @pass,@area);");
                datos.setearParametros("@correo", usuario.Correo);
                datos.setearParametros("@pass", usuario.Contrasenia);
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

        public bool eliminar(object codP)
        {
            throw new NotImplementedException();
        }


        /*

      public Usuario BuscarUsuario(string cod)
      {
          AccesoDatos datos = new AccesoDatos();
          Usuario usuario = new Usuario();
          try
          {
              datos.setearConsulta("SELECT usu.NombreUsuario_U, usu.Nombre_U, usu.Apellido_U, usu.Correo_U, usu.Contrasenia_U, usu.ImgURL_U, usu.TipoUser_U, usu.Telefono_U " +
                  "FROM USUARIOS AS usu " +
                  "WHERE usu.Cod_Usuario = @Cod_Usuario");
              datos.setearParametros("@Cod_Usuario", cod);
              datos.ejecutarLectura();
              while (datos.Lector.Read())
              {
                  usuario.Cod_Usuario = cod;
                  usuario.NombreUsuario = datos.Lector["NombreUsuario_U"] != DBNull.Value ? (string)datos.Lector["NombreUsuario_U"] : null;
                  usuario.Nombre = datos.Lector["Nombre_U"] != DBNull.Value ? (string)datos.Lector["Nombre_U"] : null;
                  usuario.Apellido = datos.Lector["Apellido_U"] != DBNull.Value ? (string)datos.Lector["Apellido_U"] : null;
                  usuario.Correo = datos.Lector["Correo_U"] != DBNull.Value ? (string)datos.Lector["Correo_U"] : null;
                  usuario.Contrasenia = datos.Lector["Contrasenia_U"] != DBNull.Value ? (string)datos.Lector["Contrasenia_U"] : null;
                  usuario.ImagenURL = datos.Lector["ImgURL_U"] != DBNull.Value ? (string)datos.Lector["ImgURL_U"] : null;
                  usuario.TipoUsuario = datos.Lector["TipoUser_U"] != DBNull.Value ? (TipoUsuario)datos.Lector["TipoUser_U"] : TipoUsuario.NORMAL;
                  usuario.Telefono = datos.Lector["Telefono_U"] != DBNull.Value ? (int)datos.Lector["Telefono_U"] : 0;
                  usuario.Estado = true;
              }
              return usuario;
          }
          catch (Exception ex)
          {
              throw new Exception("Error al buscar usuario: " + ex.Message);
          }
      }
        */

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
      
    }
}
