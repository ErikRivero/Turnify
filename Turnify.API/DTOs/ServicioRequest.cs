using System.ComponentModel.DataAnnotations;

namespace Turnify.API.DTOs
{
    public class ServicioRequest
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100,
       ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        public decimal Precio { get; set; }

        public int DuracionMinutos { get; set; }
    }
}
