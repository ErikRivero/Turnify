using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Turnify.API.DTOs;
using Turnify.API.Services;
using Turnify.Core.Services;
namespace Turnify.API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly JwtService _jwtService;

        public AuthController(
            AuthService authService,
            JwtService jwtService)
        {
            _authService = authService;
            _jwtService = jwtService;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginRequest request)
        {
            Response.Headers["Cache-Control"] = "no-store";

            var usuario = await _authService.ValidarCredencialesAsync(
                request.NombreUsuario,
                request.Password);

            if (usuario is null)
            {
                return Unauthorized(new
                {
                    mensaje = "Usuario o contraseña incorrectos."
                });
            }

            var resultado = _jwtService.GenerarToken(usuario);

            return Ok(new
            {
                token = resultado.Token,
                tipo = "Bearer",
                expiraUtc = resultado.ExpiraUtc,
                usuario
            });
        }
    }
}
