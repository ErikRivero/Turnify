using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Turnify.Core.Models;

namespace Turnify.API.Services
{
    public class JwtService
    {
        public const string Emisor = "Turnify.API";
        public const string Audiencia = "Turnify.Web";

        private readonly SymmetricSecurityKey _key;

        public JwtService(SymmetricSecurityKey key)
        {
            _key = key;
        }

        public (string Token, DateTime ExpiraUtc) GenerarToken(
            UsuarioAutenticado usuario)
        {
            var ahora = DateTime.UtcNow;
            var vencimiento = ahora.AddMinutes(30);

            var claims = new[]
            {
            new Claim(
                JwtRegisteredClaimNames.Sub,
                usuario.Id.ToString()),

            new Claim(
                JwtRegisteredClaimNames.UniqueName,
                usuario.NombreUsuario),

            new Claim(
                JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString())
        };

            var credenciales = new SigningCredentials(
                _key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: Emisor,
                audience: Audiencia,
                claims: claims,
                notBefore: ahora,
                expires: vencimiento,
                signingCredentials: credenciales);

            var tokenTexto = new JwtSecurityTokenHandler()
                .WriteToken(token);

            return (tokenTexto, vencimiento);
        }
    }
}
