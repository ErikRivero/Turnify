using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Turnify.Core.Services;

namespace Turnify.Core.Repositories
{
    public interface IServicioRepository
    {
        Task<List<Service>> ObtenerTodosAsync();

        Task<Service?> ObtenerPorIdAsync(int id);

        Task AgregarAsync(Service servicio);

        Task ActualizarAsync(Service servicio);
    }
}
