using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Turnify.Core.Data;
using Turnify.Core.Services;

namespace Turnify.Core.Repositories
{
    public class ServicioRepository : IServicioRepository
    {
        private readonly TurnifyDbContext _context;
        public ServicioRepository(TurnifyDbContext context)
        {
            this._context = context;
        }
        public async Task ActualizarAsync(Service servicio)
        {
            _context.Servicios.Update(servicio);
            await _context.SaveChangesAsync();
        }

        public async Task AgregarAsync(Service servicio)
        {
            _context.Servicios.Add(servicio);
            await _context.SaveChangesAsync();
        }

        public async Task<Service?> ObtenerPorIdAsync(int id)
        {
            return await _context.Servicios.FindAsync(id);

        }

        public async Task<List<Service>> ObtenerTodosAsync()
        {
            return await _context.Servicios
            .AsNoTracking()
            .OrderBy(s => s.Nombre)
            .ToListAsync();
        }
    }
}
