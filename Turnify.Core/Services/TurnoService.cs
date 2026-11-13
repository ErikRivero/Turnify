using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Turnify.Core.Entities;
using Turnify.Core.Repositories;
using System.Data;
using Microsoft.EntityFrameworkCore;
using Turnify.Core.Data;

namespace Turnify.Core.Services
{
    public class TurnoService
    {
        private readonly TurnifyDbContext _context;
        private readonly ITurnoRepository _turnoRepository;
        private readonly IClienteRepository _clienteRepository;
        private readonly IBarberoRepository _barberoRepository;
        private readonly IServicioRepository _servicioRepository;

        public TurnoService(
            ITurnoRepository turnoRepository,
            IClienteRepository clienteRepository,
            IBarberoRepository barberoRepository,
            IServicioRepository servicioRepository,
             TurnifyDbContext context)
        {
            _turnoRepository = turnoRepository;
            _clienteRepository = clienteRepository;
            _barberoRepository = barberoRepository;
            _servicioRepository = servicioRepository;
            _context = context;

        }

        public Task<List<Turno>> ObtenerTodosAsync()
        {
            return _turnoRepository.ObtenerTodosAsync();
        }

        public Task<Turno?> ObtenerPorIdAsync(int id)
        {
            return _turnoRepository.ObtenerPorIdAsync(id);
        }

        public async Task<Turno> CrearAsync(
            int clienteId,
            int barberoId,
            int servicioId,
            DateTime fechaHoraInicio)

        {
            await using var transaccion =
    await _context.Database.BeginTransactionAsync(
        IsolationLevel.Serializable);
            if (clienteId <= 0 || barberoId <= 0 || servicioId <= 0)
            {
                throw new ArgumentException(
                    "Los identificadores deben ser mayores que cero.");
            }

            if (fechaHoraInicio <= DateTime.Now)
            {
                throw new ArgumentException(
                    "El turno debe comenzar en una fecha y hora futura.");
            }

            var cliente = await _clienteRepository
                .ObtenerPorIdAsync(clienteId);

            if (cliente is null || !cliente.Activo)
            {
                throw new ArgumentException(
                    "El cliente no existe o está inactivo.");
            }

            var barbero = await _barberoRepository
                .ObtenerPorIdAsync(barberoId);

            if (barbero is null || !barbero.Activo)
            {
                throw new ArgumentException(
                    "El barbero no existe o está inactivo.");
            }

            var servicio = await _servicioRepository
                .ObtenerPorIdAsync(servicioId);

            if (servicio is null || !servicio.Activo)
            {
                throw new ArgumentException(
                    "El servicio no existe o está inactivo.");
            }

            if (servicio.DuracionMinutos <= 0 || servicio.Precio <= 0)
            {
                throw new ArgumentException(
                    "El servicio debe tener duración y precio positivos.");
            }

            DateTime fechaHoraFin;

            try
            {
                fechaHoraFin = fechaHoraInicio
                    .AddMinutes(servicio.DuracionMinutos);
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new ArgumentException(
                    "La fecha del turno está fuera del rango permitido.");
            }

            var existeSuperposicion =
                await _turnoRepository.ExisteSuperposicionAsync(
                    barberoId,
                    fechaHoraInicio,
                    fechaHoraFin);

            if (existeSuperposicion)
            {
                throw new InvalidOperationException(
                    "El barbero ya tiene un turno en ese horario.");
            }

            var turno = new Turno
            {
                ClienteId = clienteId,
                BarberoId = barberoId,
                ServicioId = servicioId,
                FechaHoraInicio = fechaHoraInicio,
                FechaHoraFin = fechaHoraFin,
                PrecioPactado = servicio.Precio,
                Estado = EstadoTurno.Pendiente
            };
            await _turnoRepository.AgregarAsync(turno);
            await transaccion.CommitAsync();

            return turno;
           
        }
        public async Task<bool> ReprogramarAsync(
    int id, DateTime nuevaFechaHoraInicio)
        {
            await using var transaccion =
    await _context.Database.BeginTransactionAsync(
        IsolationLevel.Serializable);
            var turno = await _turnoRepository.ObtenerPorIdAsync(id);

            if (turno is null)
            {
                return false;
            }

            if (turno.Estado != EstadoTurno.Pendiente)
            {
                throw new InvalidOperationException(
                    "Solo se pueden reprogramar turnos pendientes.");
            }

            if (nuevaFechaHoraInicio <= DateTime.Now)
            {
                throw new ArgumentException(
                    "El nuevo horario debe ser futuro.");
            }

            if (!turno.Cliente.Activo ||
                !turno.Barbero.Activo ||
                !turno.Servicio.Activo)
            {
                throw new InvalidOperationException(
                    "El cliente, barbero y servicio deben estar activos.");
            }

            var duracion = turno.FechaHoraFin - turno.FechaHoraInicio;

            if (duracion <= TimeSpan.Zero)
            {
                throw new InvalidOperationException(
                    "El turno tiene una duración inválida.");
            }

            DateTime nuevaFechaHoraFin;

            try
            {
                nuevaFechaHoraFin = nuevaFechaHoraInicio.Add(duracion);
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new ArgumentException(
                    "La nueva fecha está fuera del rango permitido.");
            }

            var existeSuperposicion =
                await _turnoRepository.ExisteSuperposicionAsync(
                    turno.BarberoId,
                    nuevaFechaHoraInicio,
                    nuevaFechaHoraFin,
                    turno.Id);

            if (existeSuperposicion)
            {
                throw new InvalidOperationException(
                    "El barbero ya tiene un turno en ese horario.");
            }

            turno.FechaHoraInicio = nuevaFechaHoraInicio;
            turno.FechaHoraFin = nuevaFechaHoraFin;

            await _turnoRepository.ActualizarAsync(turno);
            await transaccion.CommitAsync();

            return true;
        }
        public async Task<bool> CancelarAsync(int id)
        {
            await using var transaccion =
    await _context.Database.BeginTransactionAsync(
        IsolationLevel.Serializable);
            var turno = await _turnoRepository.ObtenerPorIdAsync(id);

            if (turno is null)
            {
                return false;
            }

            if (turno.Estado == EstadoTurno.Cancelado)
            {
                return true;
            }

            if (turno.Estado != EstadoTurno.Pendiente)
            {
                throw new InvalidOperationException(
                    "Solo se pueden cancelar turnos pendientes.");
            }

            turno.Estado = EstadoTurno.Cancelado;

            await _turnoRepository.ActualizarAsync(turno);
            await transaccion.CommitAsync();

            return true;
        }
        public async Task<bool> CompletarAsync(int id)
        {
            await using var transaccion =
    await _context.Database.BeginTransactionAsync(
        IsolationLevel.Serializable);
            var turno = await _turnoRepository.ObtenerPorIdAsync(id);

            if (turno is null)
            {
                return false;
            }

            if (turno.Estado == EstadoTurno.Completado)
            {
                return true;
            }

            if (turno.Estado != EstadoTurno.Pendiente)
            {
                throw new InvalidOperationException(
                    "Solo se pueden completar turnos pendientes.");
            }

            if (turno.FechaHoraFin > DateTime.Now)
            {
                throw new InvalidOperationException(
                    "No se puede completar un turno antes de su hora de finalización.");
            }

            turno.Estado = EstadoTurno.Completado;
            await _turnoRepository.ActualizarAsync(turno);
            await transaccion.CommitAsync();

            return true;

            
        }
    }

}
