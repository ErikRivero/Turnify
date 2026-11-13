using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Turnify.Core.Entities;
using Turnify.Core.Repositories;


namespace Turnify.Core.Services
{
    public class ClienteService
    {
        private readonly IClienteRepository _repository;

        public ClienteService(IClienteRepository repository)
        {
            _repository = repository;
        }

        public Task<List<Cliente>> ObtenerTodosAsync()
        {
            return _repository.ObtenerTodosAsync();
        }

        public Task<Cliente?> ObtenerPorIdAsync(int id)
        {
            return _repository.ObtenerPorIdAsync(id);
        }

        public async Task<Cliente> CrearAsync(
            string nombre, string apellido, string telefono)
        {
            Validar(nombre, apellido, telefono);

            var cliente = new Cliente
            {
                Nombre = nombre.Trim(),
                Apellido = apellido.Trim(),
                Telefono = telefono.Trim(),
                Activo = true
            };

            await _repository.AgregarAsync(cliente);

            return cliente;
        }

        public async Task<bool> ActualizarAsync(
            int id, string nombre, string apellido, string telefono)
        {
            Validar(nombre, apellido, telefono);

            var cliente = await _repository.ObtenerPorIdAsync(id);

            if (cliente is null)
            {
                return false;
            }

            cliente.Nombre = nombre.Trim();
            cliente.Apellido = apellido.Trim();
            cliente.Telefono = telefono.Trim();

            await _repository.ActualizarAsync(cliente);

            return true;
        }

        public async Task<bool> DarDeBajaAsync(int id)
        {
            var cliente = await _repository.ObtenerPorIdAsync(id);

            if (cliente is null)
            {
                return false;
            }

            cliente.Activo = false;

            await _repository.ActualizarAsync(cliente);

            return true;
        }

        private static void Validar(
            string nombre, string apellido, string telefono)
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

            if (string.IsNullOrWhiteSpace(telefono))
            {
                throw new ArgumentException(
                    "El teléfono es obligatorio.");
            }

            if (telefono.Trim().Length > 30)
            {
                throw new ArgumentException(
                    "El teléfono no puede superar los 30 caracteres.");
            }
        }

    }
}
