using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.IHttpModule


public class AuthModule : IHttpModule
{
    public void Init(HttpApplication context)
    {
        // Suscribirse al evento 'AuthenticateRequest', que se dispara en cada petición.
        context.AuthenticateRequest += OnAuthenticateRequest;
    }
    private void OnAuthenticateRequest(object sender, EventArgs e)
    {
        HttpApplication app = (HttpApplication)sender;
        HttpContext context = app.Context;
        string requestedPath = context.Request.Path.ToLower();
        var publicPaths = new List<string>
        {
            "/authentication.aspx",
            "/error.aspx",
        };
        if (publicPaths.Contains(requestedPath))
        {
            return;
        }
        HttpCookie authCookie = context.Request.Cookies["Jwt"];
        controlarQueELUsuarioEsteLogueadoYAutorizado(requestedPath, authCookie);
    } 
    controlarQueELUsuarioEsteAutenticadoYAutorizado(string requestedPath, HttpCookie authCookie)
    {
        controlarQueELUsuarioEsteAutenticado(requestedPath, authCookie);
        controlarQueELUsuarioEsteAutorizado(requestedPath);
    }
    controlarQueELUsuarioEsteAutenticado(string requestedPath, HttpCookie authCookie)
    {
        if (NoHayCookie(authCookie) || NoEsUnTokenValido(authCookie.Value))
        {
            RedirectToLogin();
        }
    }
    controlarQueELUsuarioEsteAutorizado(string requestedPath)
    {
        // TODO
    }
    private bool NoHayCookie(HttpCookie authCookie)
    {
        return authCookie == null || string.IsNullOrEmpty(authCookie.Value);
    }
    private bool NoEsUnTokenValido(string token)
    {
        return !tokenNegocio.EsTokenValido(token);
    }
    private void RedirectToLogin()
    {
        Response.Redirect("~/Authentication.aspx", true);
    }
}
