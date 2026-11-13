using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Turnify.Core.Models;
using Turnify.Core.Repositories;

namespace Turnify.Core.Services
{
    public class DashboardService
    {
        private readonly IDashboardRepository _repository;

        public DashboardService(IDashboardRepository repository)
        {
            _repository = repository;
        }

        public async Task<DashBoardResumen> ObtenerResumenAsync(
            DateOnly desde,
            DateOnly hasta)
        {
            if (desde > hasta)
            {
                throw new ArgumentException(
                    "La fecha desde no puede ser posterior a la fecha hasta.");
            }

            if (hasta == DateOnly.MaxValue)
            {
                throw new ArgumentException(
                    "La fecha hasta está fuera del rango permitido.");
            }

            var inicio = desde.ToDateTime(TimeOnly.MinValue);

            var finExclusivo = hasta
                .AddDays(1)
                .ToDateTime(TimeOnly.MinValue);

            return await _repository.ObtenerResumenAsync(
                inicio,
                finExclusivo);
        }
    }
}
