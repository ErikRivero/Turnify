using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Turnify.Core.Data;
using Turnify.Core.Entities;

namespace Turnify.Core.Repositories
{
    public class TurnoRepository : ITurnoRepository
    {
        private readonly TurnifyDbContext _context;

        public TurnoRepository(TurnifyDbContext context)
        {
            _context = context;
        }
        public async Task ActualizarAsync(Turno turno)
        {

            // Se recibe un turno obtenido con este mismo contexto.
            // EF detecta los cambios realizados sobre sus propiedades.
            await _context.SaveChangesAsync();
        }

        public async Task AgregarAsync(Turno turno)
        {
            _context.Turnos.Add(turno);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExisteSuperposicionAsync(int barberoId, DateTime inicio, DateTime fin, int? turnoIdExcluir = null)
        {
            return await _context.Turnos.AnyAsync(t =>
           t.BarberoId == barberoId &&
           t.Estado != EstadoTurno.Cancelado &&
           (!turnoIdExcluir.HasValue ||
               t.Id != turnoIdExcluir.Value) &&
           t.FechaHoraInicio < fin &&
           t.FechaHoraFin > inicio);
        }

        public async Task<Turno?> ObtenerPorIdAsync(int id)
        {
            return await _context.Turnos
            .Include(t => t.Cliente)
            .Include(t => t.Barbero)
            .Include(t => t.Servicio)
            .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<List<Turno>> ObtenerTodosAsync()
        {
            return await _context.Turnos
             .AsNoTracking()
             .Include(t => t.Cliente)
             .Include(t => t.Barbero)
             .Include(t => t.Servicio)
             .OrderBy(t => t.FechaHoraInicio)
             .ToListAsync();
        }
    }
}
