using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Turnify.Core.Models
{
    public class DashBoardResumen
    {
        public int TotalTurnos { get; set; }

        public int TurnosPendientes { get; set; }

        public int TurnosCompletados { get; set; }

        public int TurnosCancelados { get; set; }

        public decimal ImporteServiciosCompletados { get; set; }
    }
}
