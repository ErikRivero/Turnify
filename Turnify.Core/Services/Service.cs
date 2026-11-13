using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Turnify.Core.Services
{
    public class Service
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public decimal Precio { get; set; }

        public int DuracionMinutos { get; set; }

        public bool Activo { get; set; } = true;
    }
}
