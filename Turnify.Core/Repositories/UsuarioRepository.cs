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
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly TurnifyDbContext _context;

        public UsuarioRepository(TurnifyDbContext context)
        {
            _context = context;
        }
        public async Task ActualizarAsync(Usuario usuario)
        {
            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();
        }

        public async Task AgregarAsync(Usuario usuario)
        {
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
        }

        public async Task<Usuario?> ObtenerPorNombreAsync(string nombreUsuario)
        {
            return await _context.Usuarios
             .SingleOrDefaultAsync(u =>
                 u.NombreUsuario == nombreUsuario);
        }
    }
}
