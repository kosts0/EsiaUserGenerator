using EsiaUserGenerator.Db.Models;
using EsiaUserGenerator.Db.UoW;
using EsiaUserGenerator.Dto.Enum;
using EsiaUserGenerator.Service;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EsiaUserGenerator.Tests.Service;

public class DbUserProgressTrackerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IRequestHistoryRepository> _mockRequestHistoryRepository;
    private readonly Mock<ILogger<DbUserProgressTracker>> _mockLogger;
    private readonly DbUserProgressTracker _tracker;

    public DbUserProgressTrackerTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockRequestHistoryRepository = new Mock<IRequestHistoryRepository>();
        _mockLogger = new Mock<ILogger<DbUserProgressTracker>>();
        
        _mockUnitOfWork.Setup(u => u.RequestHistory).Returns(_mockRequestHistoryRepository.Object);
        
        _tracker = new DbUserProgressTracker(_mockUnitOfWork.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task SetStepAsync_WithValidRequest_UpdatesStatusAndCompletesTransaction()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var step = UserCreationFlow.PostData;
        var requestHistory = new RequestHistory
        {
            RequestId = requestId,
            JsonRequest = "{}",
            CurrentStatus = "Queued"
        };

        _mockRequestHistoryRepository
            .Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync(requestHistory);

        // Act
        await _tracker.SetStepAsync(requestId, step);

        // Assert
        requestHistory.CurrentStatus.Should().Be(step.ToString());
        _mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.Once);
    }

    [Fact]
    public async Task SetStepAsync_WithNonExistentRequest_LogsErrorAndReturnsWithoutThrow()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var step = UserCreationFlow.Started;
        
        _mockRequestHistoryRepository
            .Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync((RequestHistory?)null);

        // Act
        await _tracker.SetStepAsync(requestId, step);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Request entity {requestId} not found")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        
        _mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.Never);
    }

    [Theory]
    [InlineData(UserCreationFlow.Queued)]
    [InlineData(UserCreationFlow.Started)]
    [InlineData(UserCreationFlow.PostData)]
    [InlineData(UserCreationFlow.WaitingSms)]
    [InlineData(UserCreationFlow.ConfirmSms)]
    [InlineData(UserCreationFlow.PasswordSet)]
    [InlineData(UserCreationFlow.Authorization)]
    [InlineData(UserCreationFlow.PersonDataUpdate)]
    [InlineData(UserCreationFlow.PostmailWaiting)]
    [InlineData(UserCreationFlow.PostmailConfirmation)]
    [InlineData(UserCreationFlow.Completed)]
    public async Task SetStepAsync_WithAllFlowSteps_UpdatesStatusCorrectly(UserCreationFlow step)
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var requestHistory = new RequestHistory
        {
            RequestId = requestId,
            JsonRequest = "{}",
            CurrentStatus = "Initial"
        };

        _mockRequestHistoryRepository
            .Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync(requestHistory);

        // Act
        await _tracker.SetStepAsync(requestId, step);

        // Assert
        requestHistory.CurrentStatus.Should().Be(step.ToString());
    }

    [Fact]
    public async Task SetStepAsync_LogsInformationMessage()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var step = UserCreationFlow.Queued;
        var requestHistory = new RequestHistory
        {
            RequestId = requestId,
            JsonRequest = "{}",
            CurrentStatus = "Initial"
        };

        _mockRequestHistoryRepository
            .Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync(requestHistory);

        // Act
        await _tracker.SetStepAsync(requestId, step);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Setting user progress {step} for {requestId}")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SetStepAsync_WhenCompleteAsyncThrows_LogsErrorAndRethrows()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var step = UserCreationFlow.PostData;
        var requestHistory = new RequestHistory
        {
            RequestId = requestId,
            JsonRequest = "{}",
            CurrentStatus = "Queued"
        };

        _mockRequestHistoryRepository
            .Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync(requestHistory);

        var expectedException = new InvalidOperationException("Database error");
        _mockUnitOfWork
            .Setup(u => u.CompleteAsync())
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _tracker.SetStepAsync(requestId, step));

        exception.Should().Be(expectedException);
        
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to set status")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SetStepAsync_WhenGetByIdThrows_LogsErrorAndRethrows()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var step = UserCreationFlow.Started;
        var expectedException = new InvalidOperationException("Repository error");

        _mockRequestHistoryRepository
            .Setup(r => r.GetByIdAsync(requestId))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _tracker.SetStepAsync(requestId, step));

        exception.Should().Be(expectedException);
        
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task SetStepAsync_WithEmptyGuid_TriesToFindRequest()
    {
        // Arrange
        var requestId = Guid.Empty;
        var step = UserCreationFlow.Queued;
        
        _mockRequestHistoryRepository
            .Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync((RequestHistory?)null);

        // Act
        await _tracker.SetStepAsync(requestId, step);

        // Assert
        _mockRequestHistoryRepository.Verify(
            r => r.GetByIdAsync(requestId),
            Times.Once);
    }

    [Fact]
    public async Task SetStepAsync_CallsRepositoryBeforeCompleteAsync()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var step = UserCreationFlow.Authorization;
        var requestHistory = new RequestHistory
        {
            RequestId = requestId,
            JsonRequest = "{}",
            CurrentStatus = "PasswordSet"
        };

        var callSequence = new List<string>();
        
        _mockRequestHistoryRepository
            .Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync(requestHistory)
            .Callback(() => callSequence.Add("GetById"));

        _mockUnitOfWork
            .Setup(u => u.CompleteAsync())
            .Callback(() => callSequence.Add("Complete"))
            .ReturnsAsync(1);

        // Act
        await _tracker.SetStepAsync(requestId, step);

        // Assert
        callSequence.Should().ContainInOrder("GetById", "Complete");
    }

    [Fact]
    public async Task SetStepAsync_UpdatesStatusToStringRepresentation()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var step = UserCreationFlow.PersonDataUpdate;
        var requestHistory = new RequestHistory
        {
            RequestId = requestId,
            JsonRequest = "{}",
            CurrentStatus = "OldStatus"
        };

        _mockRequestHistoryRepository
            .Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync(requestHistory);

        // Act
        await _tracker.SetStepAsync(requestId, step);

        // Assert
        requestHistory.CurrentStatus.Should().Be("PersonDataUpdate");
    }
}