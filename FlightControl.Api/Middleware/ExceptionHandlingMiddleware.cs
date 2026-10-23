using FlightControl.Validation.Core;
using FlightControl.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace FlightControl.Api.Middleware;


public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }

        catch(ValidationException ex)
        {
            var groups = ex.Errors.GroupBy(e => e.Field);
            var errors = groups.ToDictionary(
                g => g.Key,
                g => g.Select(e => e.Message).ToArray()
            );
            var problem = new ValidationProblemDetails(errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation Error",
                Detail = "One or More Validation Errors Occurred"
            };
            await WriteProblemAsync(context, problem);
        }

        catch(ConflictException ex)
        {
            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Conflict Occurred",
                Detail = ex.Message
            };
            await WriteProblemAsync(context, problem);
        }

        catch (NotFoundException ex)
        {
            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Not Found",
                Detail = ex.Message
            };

            await WriteProblemAsync(context, problem);

        }

        catch(Exception ex)
        {
            _logger.LogError(ex, "Error occurred at ExceptionHandlingMiddleware");

            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Something went wrong",
                Detail = "An unexpected error occurred."
            };

            await WriteProblemAsync(context, problem);
        }
    }

    private async Task WriteProblemAsync(HttpContext context, ProblemDetails problem)
    {
        context.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(problem, problem.GetType(), options: null, contentType: "application/problem+json");
    }

}