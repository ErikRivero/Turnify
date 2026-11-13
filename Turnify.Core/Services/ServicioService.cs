using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Turnify.Core.Entities;
using Turnify.Core.Repositories;

namespace Turnify.Core.Services
{
    public class ServicioService
    {
        private readonly IServicioRepository _repository;
        public ServicioService(IServicioRepository repository)
        {
            this._repository = repository;
        }

        public Task<List<Service>> ObtenerTodosAsync()
        {
            return _repository.ObtenerTodosAsync();
        }

        public Task<Service?> ObtenerPorIdAsync(int id)
        {
            return _repository.ObtenerPorIdAsync(id);
        }

        public async Task<Service> CrearAsync(
            string nombre, decimal precio, int duracionMinutos)
        {
            Validar(nombre, precio, duracionMinutos);

            var servicio = new Service
            {
                Nombre = nombre.Trim(),
                Precio = precio,
                DuracionMinutos = duracionMinutos,
                Activo = true
            };

            await _repository.AgregarAsync(servicio);

            return servicio;
        }

        public async Task<bool> ActualizarAsync(
            int id, string nombre, decimal precio, int duracionMinutos)
        {
            Validar(nombre, precio, duracionMinutos);

            var servicio = await _repository.ObtenerPorIdAsync(id);

            if (servicio is null)
            {
                return false;
            }

            servicio.Nombre = nombre.Trim();
            servicio.Precio = precio;
            servicio.DuracionMinutos = duracionMinutos;

            await _repository.ActualizarAsync(servicio);

            return true;
        }

        public async Task<bool> DarDeBajaAsync(int id)
        {
            var servicio = await _repository.ObtenerPorIdAsync(id);

            if (servicio is null)
            {
                return false;
            }

            servicio.Activo = false;

            await _repository.ActualizarAsync(servicio);

            return true;
        }

        private static void Validar(
            string nombre, decimal precio, int duracionMinutos)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                throw new ArgumentException(
                    "El nombre del servicio es obligatorio.");
            }

            if (nombre.Trim().Length > 100)
            {
                throw new ArgumentException(
                    "El nombre no puede superar los 100 caracteres.");
            }

            if (precio <= 0)
            {
                throw new ArgumentException(
                    "El precio debe ser mayor que cero.");
            }

            if (decimal.Round(precio, 2) != precio)
            {
                throw new ArgumentException(
                    "El precio puede tener como máximo dos decimales.");
            }

            if (duracionMinutos <= 0)
            {
                throw new ArgumentException(
                    "La duración debe ser mayor que cero.");
            }
        }

    }
}
