using Api.Services;
using Domain.Constants;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(ITokenService tokenService) : ControllerBase
{
    /// <summary>
    /// Generate a JWT token with Energy Company Account Manager role (for testing purposes)
    /// </summary>
    [HttpPost("token/manager")]
    public IActionResult GenerateManagerToken()
    {
        var token = tokenService.GenerateToken("Energy Company Account Manager");
        return Ok(new { 
            Token = token,
            Role = "Energy Company Account Manager",
            Message = ErrorMessages.Authentication.TokenHeaderMessage
        });
    }

    /// <summary>
    /// Generate a JWT token without any role (for testing purposes)
    /// </summary>
    [HttpPost("token/anonymous")]
    public IActionResult GenerateAnonymousToken()
    {
        var token = tokenService.GenerateAnonymousToken();
        return Ok(new { 
            Token = token,
            Role = "None",
            Message = ErrorMessages.Authentication.TokenHeaderMessage
        });
    }
}