using System.Security.Claims;
using Demo.Services.DTOs;
using Demo.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace demo_app_crud.Server.Controllers;

/// <summary>User registration, login and identity endpoints.</summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _service;

    public AuthController(IAuthService service)
    {
        _service = service;
    }

    /// <summary>Registers a new user. Anonymous.</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserDto>> Register([FromBody] RegisterDto dto, CancellationToken ct)
    {
        var user = await _service.RegisterAsync(dto, ct);
        return CreatedAtAction(nameof(Me), null, user);
    }

    /// <summary>Authenticates a user and returns a JWT. Anonymous.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResultDto>> Login([FromBody] LoginDto dto, CancellationToken ct)
        => Ok(await _service.LoginAsync(dto, ct));

    /// <summary>Returns the current authenticated user. Requires a valid token.</summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserDto>> Me(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userId, out var id))
            return Unauthorized();

        return Ok(await _service.GetByIdAsync(id, ct));
    }

    /// <summary>Open endpoint that demonstrates an anonymous, unauthenticated route.</summary>
    [HttpGet("public")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Public()
        => Ok(new { message = "This is a public endpoint — no authentication required." });
}
