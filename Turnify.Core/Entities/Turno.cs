using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Turnify.Core.Services;

namespace Turnify.Core.Entities
{
    public class Turno
    {
        public int Id { get; set; }

        public DateTime FechaHoraInicio { get; set; }

        public DateTime FechaHoraFin { get; set; }

        public decimal PrecioPactado { get; set; }

        public EstadoTurno Estado { get; set; } = EstadoTurno.Pendiente;

        public int ClienteId { get; set; }
        public Cliente Cliente { get; set; } = null!;

        public int BarberoId { get; set; }
        public Barbero Barbero { get; set; } = null!;

        public int ServicioId { get; set; }
        public Service Servicio { get; set; } = null!;
    }
}
