using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Turnify.API.DTOs;
using Turnify.Core.Services;

namespace Turnify.API.Controllers
{
    [Route("api/servicios")]
    [ApiController]
    public class ServiciosController : ControllerBase
    {
        private readonly ServicioService _service;
        public ServiciosController(ServicioService service)
        {
            this._service = service;
        }
        [HttpGet]
        public async Task<IActionResult> ObtenerTodos()
        {
            var servicios = await _service.ObtenerTodosAsync();

            return Ok(servicios);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var servicio = await _service.ObtenerPorIdAsync(id);

            if (servicio is null)
            {
                return NotFound(new
                {
                    mensaje = "El servicio no existe."
                });
            }

            return Ok(servicio);
        }

        [HttpPost]
        public async Task<IActionResult> Crear(
            [FromBody] ServicioRequest request)
        {
            try
            {
                var servicio = await _service.CrearAsync(
                    request.Nombre,
                    request.Precio,
                    request.DuracionMinutos);

                return CreatedAtAction(
                    nameof(ObtenerPorId),
                    new { id = servicio.Id },
                    servicio);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Actualizar(
            int id,
            [FromBody] ServicioRequest request)
        {
            try
            {
                var actualizado = await _service.ActualizarAsync(
                    id,
                    request.Nombre,
                    request.Precio,
                    request.DuracionMinutos);

                if (!actualizado)
                {
                    return NotFound(new
                    {
                        mensaje = "El servicio no existe."
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
                    mensaje = "El servicio no existe."
                });
            }

            return NoContent();
        }
    }
}
