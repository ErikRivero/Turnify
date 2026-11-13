using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Turnify.Core.Entities;

namespace Turnify.Core.Repositories
{
    public interface IUsuarioRepository
    {
        Task<Usuario?> ObtenerPorNombreAsync(string nombreUsuario);

        Task AgregarAsync(Usuario usuario);

        Task ActualizarAsync(Usuario usuario);
    }
}
