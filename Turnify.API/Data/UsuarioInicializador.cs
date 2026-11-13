using Microsoft.AspNetCore.Identity;
using Turnify.Core.Entities;
using Turnify.Core.Repositories;

namespace Turnify.API.Data
{
    public class UsuarioInicializador
    {
        public static async Task CrearAsync(
        IServiceProvider services,
        IConfiguration configuration)
        {
            var nombreUsuario =
                configuration["UsuarioInicial:NombreUsuario"]?.Trim();

            // Sin configuración, no intentamos crear un usuario.
            if (string.IsNullOrWhiteSpace(nombreUsuario))
            {
                return;
            }

            if (nombreUsuario.Length > 50)
            {
                throw new InvalidOperationException(
                    "El nombre de usuario no puede superar los 50 caracteres.");
            }

            var repository =
                services.GetRequiredService<IUsuarioRepository>();

            var existente =
                await repository.ObtenerPorNombreAsync(nombreUsuario);

            if (existente is not null)
            {
                var restablecer = configuration.GetValue<bool>(
                    "UsuarioInicial:RestablecerPassword");

                if (restablecer)
                {
                    var nuevaPassword =
                        configuration["UsuarioInicial:Password"];

                    if (string.IsNullOrWhiteSpace(nuevaPassword) ||
                        nuevaPassword.Length < 12)
                    {
                        throw new InvalidOperationException(
                            "La nueva contraseña debe tener al menos 12 caracteres.");
                    }

                    var hasherRestablecimiento =
                        services.GetRequiredService<IPasswordHasher<Usuario>>();

                    existente.PasswordHash =
                        hasherRestablecimiento.HashPassword(
                            existente,
                            nuevaPassword);

                    await repository.ActualizarAsync(existente);
                }

                return;
            }

            var nombreCompleto =
                configuration["UsuarioInicial:NombreCompleto"]?.Trim();

            var password =
                configuration["UsuarioInicial:Password"];

            if (string.IsNullOrWhiteSpace(nombreCompleto) ||
                nombreCompleto.Length > 150)
            {
                throw new InvalidOperationException(
                    "Configurá un nombre completo de hasta 150 caracteres.");
            }

            if (string.IsNullOrWhiteSpace(password) ||
                password.Length < 12)
            {
                throw new InvalidOperationException(
                    "Configurá una contraseña inicial de al menos 12 caracteres.");
            }

            var passwordHasher =
                services.GetRequiredService<IPasswordHasher<Usuario>>();

            var usuario = new Usuario
            {
                NombreUsuario = nombreUsuario,
                NombreCompleto = nombreCompleto,
                Activo = true
            };

            usuario.PasswordHash =
                passwordHasher.HashPassword(usuario, password);

            await repository.AgregarAsync(usuario);
        }
    }
}
