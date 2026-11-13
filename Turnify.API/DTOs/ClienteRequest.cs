using System.ComponentModel.DataAnnotations;

namespace Turnify.API.DTOs
{
    public class ClienteRequest
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100,
        ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [StringLength(100,
            ErrorMessage = "El apellido no puede superar los 100 caracteres.")]
        public string Apellido { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        [StringLength(30,
            ErrorMessage = "El teléfono no puede superar los 30 caracteres.")]
        public string Telefono { get; set; } = string.Empty;
    }
}
