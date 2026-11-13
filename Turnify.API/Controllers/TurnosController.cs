using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Turnify.API.DTOs;
using Turnify.Core.Services;
using Turnify.API.Filters;

namespace Turnify.API.Controllers
{
    [Route("api/turnos")]
    [ApiController]
    [ConflictoConcurrencia]

    public class TurnosController : ControllerBase
    {
        private readonly TurnoService _service;

        public TurnosController(TurnoService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerTodos()
        {
            var turnos = await _service.ObtenerTodosAsync();

            return Ok(turnos);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObtenerPorId(int id)
        {
            var turno = await _service.ObtenerPorIdAsync(id);

            if (turno is null)
            {
                return NotFound(new { mensaje = "El turno no existe." });
            }

            return Ok(turno);
        }

        [HttpPost]
        public async Task<IActionResult> Crear(
            [FromBody] TurnoRequest request)
        {
            try
            {
                var turno = await _service.CrearAsync(
                    request.ClienteId,
                    request.BarberoId,
                    request.ServicioId,
                    request.FechaHoraInicio);

                // Consultamos para cargar también los datos relacionados.
                var resultado = await _service.ObtenerPorIdAsync(turno.Id);

                return CreatedAtAction(
                    nameof(ObtenerPorId),
                    new { id = turno.Id },
                    resultado);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { mensaje = ex.Message });
            }
        }

        [HttpPut("{id:int}/reprogramar")]
        public async Task<IActionResult> Reprogramar(
            int id,
            [FromBody] ReprogramarTurnoRequest request)
        {
            try
            {
                var actualizado = await _service.ReprogramarAsync(
                    id,
                    request.FechaHoraInicio);

                if (!actualizado)
                {
                    return NotFound(new { mensaje = "El turno no existe." });
                }

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { mensaje = ex.Message });
            }
        }

        [HttpPatch("{id:int}/cancelar")]
        public async Task<IActionResult> Cancelar(int id)
        {
            try
            {
                var cancelado = await _service.CancelarAsync(id);

                if (!cancelado)
                {
                    return NotFound(new { mensaje = "El turno no existe." });
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { mensaje = ex.Message });
            }
        }

        [HttpPatch("{id:int}/completar")]
        public async Task<IActionResult> Completar(int id)
        {
            try
            {
                var completado = await _service.CompletarAsync(id);

                if (!completado)
                {
                    return NotFound(new { mensaje = "El turno no existe." });
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { mensaje = ex.Message });
            }
        }
    }
}
