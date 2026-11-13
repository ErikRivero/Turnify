using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Turnify.Core.Entities;


namespace Turnify.Core.Repositories
{
    public interface IClienteRepository
    {
        Task<List<Cliente>> ObtenerTodosAsync();

        Task<Cliente?> ObtenerPorIdAsync(int id);

        Task AgregarAsync(Cliente cliente);

        Task ActualizarAsync(Cliente cliente);
    }
}
