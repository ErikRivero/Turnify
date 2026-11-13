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
    public class BarberoRepository : IBarberoRepository
    {
        private readonly TurnifyDbContext _context;
        public BarberoRepository(TurnifyDbContext context)
        {
            this._context = context;
        }
        public async Task ActualizarAsync(Barbero barbero)
        {
            _context.Barberos.Update(barbero);
            await _context.SaveChangesAsync();
        }

        public async  Task AgregarAsync(Barbero barbero)
        {
            _context.Barberos.Add(barbero);
            await _context.SaveChangesAsync(); 
        }

        public async Task<Barbero?> ObtenerPorIdAsync(int id)
        {
            return await _context.Barberos.FindAsync(id);
        }

        public async Task<List<Barbero>> ObtenerTodosAsync()
        {
            return await _context.Barberos
           .AsNoTracking()
           .OrderBy(b => b.Apellido)
           .ThenBy(b => b.Nombre)
           .ToListAsync();
        }
    }
}
