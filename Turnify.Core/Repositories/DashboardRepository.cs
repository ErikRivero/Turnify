using Microsoft.EntityFrameworkCore;
using Turnify.Core.Data;
using Turnify.Core.Entities;
using Turnify.Core.Models;

namespace Turnify.Core.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly TurnifyDbContext _context;

        public DashboardRepository(TurnifyDbContext context)
        {
            _context = context;
        }
        public async Task<DashBoardResumen> ObtenerResumenAsync(
        DateTime desde,
        DateTime hastaExclusivo)
        {
            var resumen = await _context.Turnos
                .Where(t =>
                    t.FechaHoraInicio >= desde &&
                    t.FechaHoraInicio < hastaExclusivo)
                .GroupBy(t => 1)
                .Select(grupo => new DashBoardResumen
                {
                    TotalTurnos = grupo.Count(),

                    TurnosPendientes = grupo.Count(t =>
                        t.Estado == EstadoTurno.Pendiente),

                    TurnosCompletados = grupo.Count(t =>
                        t.Estado == EstadoTurno.Completado),

                    TurnosCancelados = grupo.Count(t =>
                        t.Estado == EstadoTurno.Cancelado),

                    ImporteServiciosCompletados = grupo.Sum(t =>
                        t.Estado == EstadoTurno.Completado
                            ? t.PrecioPactado
                            : 0m)
                })
                .FirstOrDefaultAsync();

            return resumen ?? new DashBoardResumen();
        }
    }
}
