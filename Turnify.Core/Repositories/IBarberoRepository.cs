using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Turnify.Core.Entities;

namespace Turnify.Core.Repositories
{
    public interface IBarberoRepository
    {
        Task<List<Barbero>> ObtenerTodosAsync();

        Task<Barbero?> ObtenerPorIdAsync(int id);

        Task AgregarAsync(Barbero barbero);

        Task ActualizarAsync(Barbero barbero);
    }
}
