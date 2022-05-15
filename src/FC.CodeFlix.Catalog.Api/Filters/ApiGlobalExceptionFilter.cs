using FC.CodeFlix.Catalog.Application.Exceptions;
using FC.CodeFlix.Catalog.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FC.CodeFlix.Catalog.Api.Filters;

public class ApiGlobalExceptionFilter : IExceptionFilter
{
    private readonly IHostEnvironment _env;

    public ApiGlobalExceptionFilter(IHostEnvironment env)
        => _env = env;
    
    public void OnException(ExceptionContext context)
    {
        var details = new ProblemDetails();
        var expection = context.Exception;

        if (_env.IsDevelopment())
        {
            details.Extensions.Add("StackTrace", expection.StackTrace);
        }

        if(expection is EntityValidationException)
        {
            
            details.Title = "One or more validation errors ocurred";
            details.Status = StatusCodes.Status422UnprocessableEntity;
            details.Type = "UnprocessableEntity";
            details.Detail = expection!.Message;
            
        }else if(expection is NotFoundException)
        {
            
            details.Title = "Not Found";
            details.Status = StatusCodes.Status404NotFound;
            details.Type = "NotFound";
            details.Detail = expection!.Message;

        }
        else
        {
            
            details.Title = "An unespected error ocurred";
            details.Status = StatusCodes.Status422UnprocessableEntity;
            details.Type = "UnexpectedError";
            details.Detail = expection.Message;
            
        }

        context.HttpContext.Response.StatusCode = (int)details.Status;
        context.Result = new ObjectResult(details);
        context.ExceptionHandled = true;
    }
}
