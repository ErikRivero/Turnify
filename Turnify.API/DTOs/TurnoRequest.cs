using System.ComponentModel.DataAnnotations;

namespace Turnify.API.DTOs
{
    public class TurnoRequest
    {
        [Range(1, int.MaxValue)]
        public int ClienteId { get; set; }

        [Range(1, int.MaxValue)]
        public int BarberoId { get; set; }

        [Range(1, int.MaxValue)]
        public int ServicioId { get; set; }

        public DateTime FechaHoraInicio { get; set; }
    }
}
