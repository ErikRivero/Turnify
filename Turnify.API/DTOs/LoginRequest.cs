using System.ComponentModel.DataAnnotations;

namespace Turnify.API.DTOs
{
    public class LoginRequest
    {

        [Required]
        [StringLength(50)]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
