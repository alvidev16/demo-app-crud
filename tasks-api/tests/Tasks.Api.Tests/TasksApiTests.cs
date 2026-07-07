using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Tasks.Domain.Entities;
using Tasks.Services.DTOs;

namespace Tasks.Api.Tests;

public class TasksApiTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private static readonly DateOnly Future = DateOnly.FromDateTime(DateTime.UtcNow.Date).AddDays(30);

    // Match the API's contract: enums are serialized/read as their names.
    private static readonly JsonSerializerOptions Json =
        new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    private HttpClient ClientFor(Guid userId)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", factory.TokenFor(userId));
        return client;
    }

    private static Task<HttpResponseMessage> CreateTask(HttpClient client, string title, DateOnly due, string? desc = null)
        => client.PostAsJsonAsync("/api/tasks", new CreateTaskDto(title, desc, due), Json);

    private static async Task<TaskDto> ReadTask(HttpResponseMessage response)
        => (await response.Content.ReadFromJsonAsync<TaskDto>(Json))!;

    // ---- Authentication (AC-7) ----

    [Fact]
    public async Task GetTasks_WithoutToken_Returns401()
    {
        var response = await factory.CreateClient().GetAsync("/api/tasks");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ---- CRUD happy path ----

    [Fact]
    public async Task Create_Then_GetById_RoundTrips()
    {
        var client = ClientFor(Guid.NewGuid());

        var create = await CreateTask(client, "Write the report", Future, "Q1 numbers");
        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await ReadTask(create);

        var fetched = await client.GetFromJsonAsync<TaskDto>($"/api/tasks/{created.Id}", Json);
        fetched!.Title.Should().Be("Write the report");
        fetched.Status.Should().Be(TaskState.Todo);
    }

    [Fact]
    public async Task Create_WithPastDueDate_Returns400()
    {
        var client = ClientFor(Guid.NewGuid());
        var past = DateOnly.FromDateTime(DateTime.UtcNow.Date).AddDays(-1);

        var response = await CreateTask(client, "Valid title", past);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ChangeStatus_TodoDirectlyToDone_Returns400()
    {
        var client = ClientFor(Guid.NewGuid());
        var created = await ReadTask(await CreateTask(client, "A task", Future));

        var response = await client.PatchAsJsonAsync($"/api/tasks/{created.Id}/status",
            new ChangeStatusDto(TaskState.Done), Json);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ChangeStatus_ValidTransition_Returns200()
    {
        var client = ClientFor(Guid.NewGuid());
        var created = await ReadTask(await CreateTask(client, "A task", Future));

        var response = await client.PatchAsJsonAsync($"/api/tasks/{created.Id}/status",
            new ChangeStatusDto(TaskState.InProgress), Json);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await ReadTask(response)).Status.Should().Be(TaskState.InProgress);
    }

    [Fact] // regression: the API contract must accept the status as a NAME, not an integer
    public async Task ChangeStatus_AcceptsStatusAsStringName_Returns200()
    {
        var client = ClientFor(Guid.NewGuid());
        var created = await ReadTask(await CreateTask(client, "A task", Future));

        // Raw JSON with a string status, exactly as a real client would send it.
        var content = new StringContent("""{"status":"InProgress"}""", Encoding.UTF8, "application/json");
        var response = await client.PatchAsync($"/api/tasks/{created.Id}/status", content);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync()).Should().Contain("InProgress"); // echoed as a name
    }

    [Fact]
    public async Task Delete_Then_Get_Returns404()
    {
        var client = ClientFor(Guid.NewGuid());
        var created = await ReadTask(await CreateTask(client, "Temp task", Future));

        (await client.DeleteAsync($"/api/tasks/{created.Id}")).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await client.GetAsync($"/api/tasks/{created.Id}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ---- Ownership isolation (AC-6 / FR-7) ----

    [Fact]
    public async Task GetById_AnotherUsersTask_Returns404()
    {
        var alice = ClientFor(Guid.NewGuid());
        var bob = ClientFor(Guid.NewGuid());

        var aliceTask = await ReadTask(await CreateTask(alice, "Alice's private task", Future));

        // Bob must not be able to see Alice's task — reported as not found, not forbidden.
        var response = await bob.GetAsync($"/api/tasks/{aliceTask.Id}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetMine_ReturnsOnlyCallersTasks()
    {
        var alice = ClientFor(Guid.NewGuid());
        var bob = ClientFor(Guid.NewGuid());
        await CreateTask(alice, "Alice task", Future);
        await CreateTask(bob, "Bob task 1", Future);
        await CreateTask(bob, "Bob task 2", Future);

        var bobTasks = await bob.GetFromJsonAsync<List<TaskDto>>("/api/tasks", Json);

        bobTasks!.Should().HaveCount(2);
        bobTasks.Should().OnlyContain(t => t.Title.StartsWith("Bob"));
    }
}
