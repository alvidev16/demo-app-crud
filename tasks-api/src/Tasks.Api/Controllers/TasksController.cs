using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tasks.Services.DTOs;
using Tasks.Services.Interfaces;

namespace Tasks.Api.Controllers;

/// <summary>CRUD + status API for tasks. All endpoints require authentication (FR-8).</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController(ITaskService service) : ControllerBase
{
    // FR-7: identity comes from the token, never the request body.
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TaskDto>>> GetMine(CancellationToken ct)
        => Ok(await service.GetMineAsync(UserId, ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TaskDto>> GetById(Guid id, CancellationToken ct)
        => Ok(await service.GetByIdAsync(UserId, id, ct));

    [HttpPost]
    public async Task<ActionResult<TaskDto>> Create([FromBody] CreateTaskDto dto, CancellationToken ct)
    {
        var created = await service.CreateAsync(UserId, dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TaskDto>> Update(Guid id, [FromBody] UpdateTaskDto dto, CancellationToken ct)
        => Ok(await service.UpdateAsync(UserId, id, dto, ct));

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<TaskDto>> ChangeStatus(Guid id, [FromBody] ChangeStatusDto dto, CancellationToken ct)
        => Ok(await service.ChangeStatusAsync(UserId, id, dto.Status, ct));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await service.DeleteAsync(UserId, id, ct);
        return NoContent();
    }
}
