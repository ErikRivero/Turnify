using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Turnify.API.DTOs;
using Turnify.Core.Services;

namespace Turnify.API.Controllers
{
    [Route("api/barberos")]
    [ApiController]
    public class BarberosController : ControllerBase
    {
        private readonly BarberoService _service;

        public BarberosController(BarberoService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerTodos()
        {
            var barberos = await _service.ObtenerTodosAsync();

            return Ok(barberos);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var barbero = await _service.ObtenerPorIdAsync(id);

            if (barbero is null)
            {
                return NotFound(new
                {
                    mensaje = "El barbero no existe."
                });
            }

            return Ok(barbero);
        }

        [HttpPost]
        public async Task<IActionResult> Crear(
            [FromBody] BarberoRequest request)
        {
            try
            {
                var barbero = await _service.CrearAsync(
                    request.Nombre,
                    request.Apellido);

                return CreatedAtAction(
                    nameof(ObtenerPorId),
                    new { id = barbero.Id },
                    barbero);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Actualizar(
            int id,
            [FromBody] BarberoRequest request)
        {
            try
            {
                var actualizado = await _service.ActualizarAsync(
                    id,
                    request.Nombre,
                    request.Apellido);

                if (!actualizado)
                {
                    return NotFound(new
                    {
                        mensaje = "El barbero no existe."
                    });
                }

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DarDeBaja(int id)
        {
            var dadoDeBaja = await _service.DarDeBajaAsync(id);

            if (!dadoDeBaja)
            {
                return NotFound(new
                {
                    mensaje = "El barbero no existe."
                });
            }

            return NoContent();
        }
    }
}
