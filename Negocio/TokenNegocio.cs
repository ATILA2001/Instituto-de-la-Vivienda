using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Dominio;

public class TokenNegocio{
    private readonly string claveSecreta;
    private readonly SymmetricSecurityKey claveSeguridad;
    private readonly SigningCredentials credenciales;
    private readonly JwtSecurityTokenHandler tokenHandler;
    public TokenNegocio()
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
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, usuario.Area.Nombre)
        };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: credenciales);

        return tokenHandler.WriteToken(token);
    }
    
    public void ValidarToken(string token){
        try{
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = claveSeguridad,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
            };

            SecurityToken validatedToken;
            var principal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);

            var nombreUsuario = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var rol = principal.FindFirst(ClaimTypes.Role)?.Value;
        }
        catch (SecurityTokenExpiredException){
            throw new SecurityTokenExpiredException("Por favor, inicie sesión nuevamente.");
        }
    }
}