using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;


namespace Turnify.API.Filters
{
    public class ConflictoConcurrenciaAttribute : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            Exception? error = context.Exception;

            while (error is not null)
            {
                if (error is SqlException sqlError &&
                    sqlError.Number == 1205)
                {
                    context.Result = new ConflictObjectResult(new
                    {
                        mensaje =
                            "Hubo otra operación simultánea. " +
                            "Actualizá los turnos y volvé a intentarlo."
                    });

                    context.ExceptionHandled = true;
                    return;
                }

                error = error.InnerException;
            }
        }

    }
}
