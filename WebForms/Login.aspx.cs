﻿using Dominio;
using Negocio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WebForms
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        protected void btnIniciar_Click(object sender, EventArgs e)
        {
            Usuario usuario;
            UsuarioNegocio negocio = new UsuarioNegocio();
            try
            {
                usuario = new Usuario(txtEmail.Text.Trim(), txtPass.Text.Trim());
                if (negocio.Logear(usuario))
                {
                    Session.Add("Usuario", usuario);
                    if (Session["Usuario"] != null && ((Dominio.Usuario)Session["Usuario"]).Tipo == true)
                    {
                        Response.Redirect("BdProyectos.aspx", false);
                    }
                    else
                    {
                        if (((Dominio.Usuario)Session["Usuario"]).Estado == true)
                        {
                            if (((Dominio.Usuario)Session["Usuario"]).Area != null && ((Dominio.Usuario)Session["Usuario"]).Area.Id == 16)
                            {
                                Response.Redirect("Redeterminaciones.aspx", false);
                            }
                            else
                            {
                                Response.Redirect("Obras.aspx", false);
                            }
                        }
                        else
                        {
                            Session.Add("error", "Usuario no habilitado a ingresar, solicitar acceso al area correspondiente.");
                            Response.Redirect("Error.aspx", false);
                        }
                    }
                }
                else
                {
                    Session.Add("error", "Usuario o Contraseña Incorrectos");
                    Response.Redirect("Error.aspx", false);
                }

            }
            catch (Exception ex)
            {

                Session.Add("error", ex.ToString());
                Response.Redirect("Error.aspx");
            }
        }

    }
}