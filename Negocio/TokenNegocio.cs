using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Dominio;

public class TokenNegocio{
    private static readonly Lazy<TokenNegocio> _instance = new Lazy<TokenNegocio>(() => new TokenNegocio());
    
    public static TokenNegocio Instance => _instance.Value;

    private readonly string claveSecreta;
    private readonly SymmetricSecurityKey claveSeguridad;
    private readonly SigningCredentials credenciales;
    private readonly JwtSecurityTokenHandler tokenHandler;

    private TokenNegocio()
    {
        claveSecreta = Environment.GetEnvironmentVariable("TOKEN_KEY"); // setear en entorno
        claveSeguridad = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(claveSecreta));
        credenciales = new SigningCredentials(claveSeguridad, SecurityAlgorithms.HmacSha256);
        tokenHandler = new JwtSecurityTokenHandler();
    }

    public string GenerarToken(UsuarioEF usuario)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Nombre),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            claims: claims,
            signingCredentials: credenciales);

        return tokenHandler.WriteToken(token);
    }

    public bool EsTokenValido(string token)
    {
        ClaimsPrincipal claims = ObtenerClaimsDesdeToken(token);
        if (claims == null)
        {
            return false;
        }
        else
        {
            return true;
        }   
    }
    public ClaimsPrincipal ObtenerClaimsDesdeToken(string token)
    {
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = claveSeguridad,
            ValidateLifetime = false,
            ValidateAudience = false,
            ValidateIssuer = false,
            ClockSkew = TimeSpan.Zero,
        };

        SecurityToken validatedToken;
        var principal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);
        return principal;
    }
}