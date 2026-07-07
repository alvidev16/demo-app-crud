using FluentAssertions;
using Moq;
using Tasks.Domain.Entities;
using Tasks.Domain.Exceptions;
using Tasks.Domain.Interfaces;
using Tasks.Services.DTOs;

namespace Tasks.Services.Tests;

/// <summary>TaskService use cases with a mocked repository (T-07).</summary>
public class TaskServiceTests
{
    private static readonly DateOnly Today = new(2026, 1, 1);
    private static readonly DateOnly Tomorrow = Today.AddDays(1);

    private readonly Mock<ITaskRepository> _repo = new();
    private readonly TaskService _sut;
    private readonly Guid _user = Guid.NewGuid();

    public TaskServiceTests()
    {
        var clock = new FixedTimeProvider(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));
        _sut = new TaskService(_repo.Object, clock);
    }

    private TaskItem OwnedTask() =>
        new TaskItemBuilder().WithToday(Today).DueOn(Tomorrow).OwnedBy(_user).Build();

    [Fact]
    public async Task CreateAsync_PersistsTaskForCaller()
    {
        var result = await _sut.CreateAsync(_user, new CreateTaskDto("Write the report", null, Tomorrow));

        result.OwnerUserId.Should().Be(_user);
        result.Status.Should().Be(TaskState.Todo);
        _repo.Verify(r => r.AddAsync(It.Is<TaskItem>(t => t.OwnerUserId == _user), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithPastDueDate_ThrowsValidation()
    {
        var act = () => _sut.CreateAsync(_user, new CreateTaskDto("Valid title", null, Today.AddDays(-1)));

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task GetByIdAsync_WhenOwned_ReturnsTask()
    {
        var task = OwnedTask();
        _repo.Setup(r => r.GetByIdAsync(task.Id, It.IsAny<CancellationToken>())).ReturnsAsync(task);

        var result = await _sut.GetByIdAsync(_user, task.Id);

        result.Id.Should().Be(task.Id);
    }

    [Fact] // AC-6 / FR-7: another user's task is invisible
    public async Task GetByIdAsync_WhenOwnedByAnotherUser_ThrowsNotFound()
    {
        var othersTask = new TaskItemBuilder().WithToday(Today).DueOn(Tomorrow).OwnedBy(Guid.NewGuid()).Build();
        _repo.Setup(r => r.GetByIdAsync(othersTask.Id, It.IsAny<CancellationToken>())).ReturnsAsync(othersTask);

        var act = () => _sut.GetByIdAsync(_user, othersTask.Id);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetByIdAsync_WhenMissing_ThrowsNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((TaskItem?)null);

        var act = () => _sut.GetByIdAsync(_user, Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetMineAsync_ReturnsOwnerTasks()
    {
        _repo.Setup(r => r.GetByOwnerAsync(_user, It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<TaskItem> { OwnedTask(), OwnedTask() });

        var result = await _sut.GetMineAsync(_user);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task ChangeStatusAsync_ValidTransition_Updates()
    {
        var task = OwnedTask();
        _repo.Setup(r => r.GetByIdAsync(task.Id, It.IsAny<CancellationToken>())).ReturnsAsync(task);

        var result = await _sut.ChangeStatusAsync(_user, task.Id, TaskState.InProgress);

        result.Status.Should().Be(TaskState.InProgress);
        _repo.Verify(r => r.UpdateAsync(task, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact] // AC-4 through the service
    public async Task ChangeStatusAsync_InvalidTransition_ThrowsValidation()
    {
        var task = OwnedTask();
        _repo.Setup(r => r.GetByIdAsync(task.Id, It.IsAny<CancellationToken>())).ReturnsAsync(task);

        var act = () => _sut.ChangeStatusAsync(_user, task.Id, TaskState.Done);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task DeleteAsync_WhenOwned_CallsRepository()
    {
        var task = OwnedTask();
        _repo.Setup(r => r.GetByIdAsync(task.Id, It.IsAny<CancellationToken>())).ReturnsAsync(task);

        await _sut.DeleteAsync(_user, task.Id);

        _repo.Verify(r => r.DeleteAsync(task, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenNotOwned_ThrowsNotFoundAndDoesNotDelete()
    {
        var othersTask = new TaskItemBuilder().WithToday(Today).DueOn(Tomorrow).OwnedBy(Guid.NewGuid()).Build();
        _repo.Setup(r => r.GetByIdAsync(othersTask.Id, It.IsAny<CancellationToken>())).ReturnsAsync(othersTask);

        var act = () => _sut.DeleteAsync(_user, othersTask.Id);

        await act.Should().ThrowAsync<NotFoundException>();
        _repo.Verify(r => r.DeleteAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
