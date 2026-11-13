using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Turnify.Core.Entities;
using Turnify.Core.Models;
using Turnify.Core.Repositories;
using Turnify.Core.Services;
namespace Turnify.Core.Services
{
    public class AuthService
    {
        private readonly IUsuarioRepository _repository;
        private readonly IPasswordHasher<Usuario> _passwordHasher;

        public AuthService(
            IUsuarioRepository repository,
            IPasswordHasher<Usuario> passwordHasher)
        {
            _repository = repository;
            _passwordHasher = passwordHasher;
        }

        public async Task<UsuarioAutenticado?> ValidarCredencialesAsync(
            string nombreUsuario,
            string password)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario) ||
                string.IsNullOrEmpty(password))
            {
                return null;
            }

            var usuario = await _repository.ObtenerPorNombreAsync(
                nombreUsuario.Trim());

            if (usuario is null || !usuario.Activo)
            {
                return null;
            }

            var resultado = _passwordHasher.VerifyHashedPassword(
                usuario,
                usuario.PasswordHash,
                password);

            if (resultado == PasswordVerificationResult.Failed)
            {
                return null;
            }

            if (resultado == PasswordVerificationResult.SuccessRehashNeeded)
            {
                usuario.PasswordHash = _passwordHasher.HashPassword(
                    usuario,
                    password);

                await _repository.ActualizarAsync(usuario);
            }

            return new UsuarioAutenticado
            {
                Id = usuario.Id,
                NombreUsuario = usuario.NombreUsuario,
                NombreCompleto = usuario.NombreCompleto
            };
        }
    }
}
