using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tasks.Api.Auth;

namespace Tasks.Api.Controllers;

/// <summary>
/// DEMO STAND-IN for the host application's existing authentication. The spec assumes a
/// User model and auth already exist; this endpoint simply mints a token for a given user
/// id so the API can be exercised. In a real deployment this is replaced by the host's login.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController(JwtTokenService tokens) : ControllerBase
{
    public record TokenRequest(Guid UserId);

    [HttpPost("token")]
    [AllowAnonymous]
    public ActionResult<object> Token([FromBody] TokenRequest request)
    {
        var userId = request.UserId == Guid.Empty ? Guid.NewGuid() : request.UserId;
        return Ok(new { token = tokens.CreateToken(userId), userId });
    }
}
