using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Turnify.Core.Entities;
using Turnify.Core.Repositories;

namespace Turnify.Core.Services
{
    public class BarberoService
    {
        private readonly IBarberoRepository _repository;

        public BarberoService(IBarberoRepository repository)
        {
            _repository = repository;
        }

        public Task<List<Barbero>> ObtenerTodosAsync()
        {
            return _repository.ObtenerTodosAsync();
        }

        public Task<Barbero?> ObtenerPorIdAsync(int id)
        {
            return _repository.ObtenerPorIdAsync(id);
        }

        public async Task<Barbero> CrearAsync(
            string nombre, string apellido)
        {
            Validar(nombre, apellido);

            var barbero = new Barbero
            {
                Nombre = nombre.Trim(),
                Apellido = apellido.Trim(),
                Activo = true
            };

            await _repository.AgregarAsync(barbero);

            return barbero;
        }

        public async Task<bool> ActualizarAsync(
            int id, string nombre, string apellido)
        {
            Validar(nombre, apellido);

            var barbero = await _repository.ObtenerPorIdAsync(id);

            if (barbero is null)
            {
                return false;
            }

            barbero.Nombre = nombre.Trim();
            barbero.Apellido = apellido.Trim();

            await _repository.ActualizarAsync(barbero);

            return true;
        }

        public async Task<bool> DarDeBajaAsync(int id)
        {
            var barbero = await _repository.ObtenerPorIdAsync(id);

            if (barbero is null)
            {
                return false;
            }

            barbero.Activo = false;

            await _repository.ActualizarAsync(barbero);

            return true;
        }

        private static void Validar(string nombre, string apellido)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                throw new ArgumentException(
                    "El nombre es obligatorio.");
            }

            if (nombre.Trim().Length > 100)
            {
                throw new ArgumentException(
                    "El nombre no puede superar los 100 caracteres.");
            }

            if (string.IsNullOrWhiteSpace(apellido))
            {
                throw new ArgumentException(
                    "El apellido es obligatorio.");
            }

            if (apellido.Trim().Length > 100)
            {
                throw new ArgumentException(
                    "El apellido no puede superar los 100 caracteres.");
            }
        }
    }
}
