using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Turnify.Core.Entities;

namespace Turnify.Core.Repositories
{
    public interface ITurnoRepository
    {
        Task<List<Turno>> ObtenerTodosAsync();

        Task<Turno?> ObtenerPorIdAsync(int id);

        Task AgregarAsync(Turno turno);

        Task ActualizarAsync(Turno turno);

        Task<bool> ExisteSuperposicionAsync(
            int barberoId,
            DateTime inicio,
            DateTime fin,
            int? turnoIdExcluir = null);
    }
}
