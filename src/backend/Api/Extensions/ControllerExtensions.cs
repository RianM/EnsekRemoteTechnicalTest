using Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Api.Extensions;

public static class ControllerExtensions
{
    public static ActionResult<T> BadRequestWithMessage<T>(this ControllerBase controller, string message)
    {
        var errorResponse = new ErrorResponseDto
        {
            Message = message,
            StatusCode = 400
        };

        return controller.BadRequest(errorResponse);
    }
    
    public static ActionResult<T> HandleError<T>(this ControllerBase controller, Exception ex, string? customMessage = null)
    {
        var errorResponse = new ErrorResponseDto
        {
            Message = customMessage ?? "An internal server error occurred",
            Detail = ex.Message,
            StatusCode = 500
        };

        return controller.StatusCode(500, errorResponse);
    }
}       
