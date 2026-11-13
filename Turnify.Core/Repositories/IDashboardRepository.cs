using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Turnify.Core.Models;

namespace Turnify.Core.Repositories
{
    public interface IDashboardRepository
    {

        Task<DashBoardResumen> ObtenerResumenAsync(
            DateTime desde,
            DateTime hastaExclusivo);
    }
}
