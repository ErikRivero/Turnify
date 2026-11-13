using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Turnify.API.DTOs;
using Turnify.Core.Services;

namespace Turnify.API.Controllers
{
    [Route("api/clientes")]
    [ApiController]
    public class ClientesController : ControllerBase
    {
        private readonly ClienteService _service;

        public ClientesController(ClienteService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerTodos()
        {
            var clientes = await _service.ObtenerTodosAsync();

            return Ok(clientes);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var cliente = await _service.ObtenerPorIdAsync(id);

            if (cliente is null)
            {
                return NotFound(new
                {
                    mensaje = "El cliente no existe."
                });
            }

            return Ok(cliente);
        }

        [HttpPost]
        public async Task<IActionResult> Crear(
            [FromBody] ClienteRequest request)
        {
            try
            {
                var cliente = await _service.CrearAsync(
                    request.Nombre,
                    request.Apellido,
                    request.Telefono);

                return CreatedAtAction(
                    nameof(ObtenerPorId),
                    new { id = cliente.Id },
                    cliente);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Actualizar(
            int id,
            [FromBody] ClienteRequest request)
        {
            try
            {
                var actualizado = await _service.ActualizarAsync(
                    id,
                    request.Nombre,
                    request.Apellido,
                    request.Telefono);

                if (!actualizado)
                {
                    return NotFound(new
                    {
                        mensaje = "El cliente no existe."
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
                    mensaje = "El cliente no existe."
                });
            }

            return NoContent();
        }
    }
}
