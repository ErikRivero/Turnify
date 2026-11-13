using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Turnify.Core.Services;

namespace Turnify.API.Controllers
{
    [Route("api/dashboard")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly DashboardService _service;

        public DashboardController(DashboardService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerResumen(
            [FromQuery] DateOnly? desde,
            [FromQuery] DateOnly? hasta)
        {
            if (desde.HasValue != hasta.HasValue)
            {
                return BadRequest(new
                {
                    mensaje = "Debés indicar ambas fechas o ninguna."
                });
            }

            var hoy = DateOnly.FromDateTime(DateTime.Now);

            var fechaDesde = desde ?? hoy;
            var fechaHasta = hasta ?? hoy;

            try
            {
                var resumen = await _service.ObtenerResumenAsync(
                    fechaDesde,
                    fechaHasta);

                return Ok(new
                {
                    desde = fechaDesde,
                    hasta = fechaHasta,
                    resumen
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }
    }
}
