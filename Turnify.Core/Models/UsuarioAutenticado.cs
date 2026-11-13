using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Turnify.Core.Models
{
    public class UsuarioAutenticado
    {

        public int Id { get; set; }

        public string NombreUsuario { get; set; } = string.Empty;

        public string NombreCompleto { get; set; } = string.Empty;
    }
}
