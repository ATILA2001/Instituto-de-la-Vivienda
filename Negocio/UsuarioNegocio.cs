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
                datos.setearConsulta("SELECT * FROM USUARIOS WHERE CORREO = @correo  AND CONTRASENIA = @pass");
                datos.setearParametros("@correo", usuario.Correo);
                datos.setearParametros("@pass", usuario.Contrasenia);
                datos.ejecutarLectura();
                while (datos.Lector.Read())
                {
                    usuario.Correo = datos.Lector["CORREO"].ToString();
                    usuario.Nombre = datos.Lector["NOMBRE"].ToString();
                    usuario.Tipo = Convert.ToBoolean(datos.Lector["TIPO"]);
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
        /*
        public string registrarUsuario(Usuario usuario)
        {
            AccesoDatos datos = new AccesoDatos();
            try
            {
                datos.setearProcedimiento("spRegister");
                datos.setearParametros("@Correo_U", usuario.Correo);
                datos.setearParametros("@Contrasenia_U", usuario.Contrasenia);
                datos.setearParametros("@Nombre_U", usuario.Nombre);
                datos.setearParametros("@Apellido_U", usuario.Apellido);
                datos.setearParametros("@TipoUser_U", 1);
                datos.setearParametros("@Telefono_U", usuario.Telefono);
                datos.setearParametros("Estado_U", true);
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


        public bool ModificarUsuario(Usuario usuario)
        {
            AccesoDatos datos = new AccesoDatos();
            try
            {
                if (usuario != null)
                {
                    datos.setearConsulta("UPDATE Usuarios SET NombreUsuario_U = @NombreUsuario_U, Nombre_U = @Nombre_U, Apellido_U = @Apellido_U, Correo_U = @Correo_U, Contrasenia_U = @Contrasenia_U, ImgURL_U = @ImgURL_U, Estado_U = @Estado_U, TipoUser_U = @TipoUser_U, Telefono_U = @Telefono_U WHERE Cod_Usuario = @Cod_Usuario");
                    datos.setearParametros("@Cod_Usuario", usuario.Cod_Usuario);
                    datos.setearParametros("@NombreUsuario_U", usuario.NombreUsuario);
                    datos.setearParametros("@Nombre_U", usuario.Nombre);
                    datos.setearParametros("@Apellido_U", usuario.Apellido);
                    datos.setearParametros("@Correo_U", usuario.Correo);
                    datos.setearParametros("@Contrasenia_U", usuario.Contrasenia);
                    // datos.setearParametros("@Cod_Localidad_U", usuario.Localidad.Id);
                    datos.setearParametros("@ImgURL_U", usuario.ImagenURL);
                    datos.setearParametros("@Estado_U", true);
                    datos.setearParametros("@Telefono_U", usuario.Telefono);
                    if (usuario.TipoUsuario == TipoUsuario.ADMIN)
                    {
                        datos.setearParametros("@TipoUser_U", 2);
                    }
                    else
                    {
                        datos.setearParametros("@TipoUser_U", 1);
                    }
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
        */
    }
}
