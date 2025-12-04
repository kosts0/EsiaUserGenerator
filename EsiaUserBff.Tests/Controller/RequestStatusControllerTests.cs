using EsiaUserGenerator.Controller;
using EsiaUserGenerator.Db.Models;
using EsiaUserGenerator.Db.UoW;
using EsiaUserGenerator.Dto.API;
using EsiaUserGenerator.Service.Interface;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace EsiaUserGenerator.Tests.Controller;

public class RequestStatusControllerTests
{
    private readonly Mock<IRequestStatusStore> _mockStatusStore;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IRequestHistoryRepository> _mockRequestHistoryRepository;
    private readonly RequestStatusController _controller;

    public RequestStatusControllerTests()
    {
        _mockStatusStore = new Mock<IRequestStatusStore>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockRequestHistoryRepository = new Mock<IRequestHistoryRepository>();
        
        _mockUnitOfWork.Setup(u => u.RequestHistory).Returns(_mockRequestHistoryRepository.Object);
        
        _controller = new RequestStatusController(_mockStatusStore.Object, _mockUnitOfWork.Object);
    }

    [Fact]
    public async Task GetStatus_WithValidId_ReturnsOkResultWithStatusData()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var requestHistory = new RequestHistory
        {
            RequestId = requestId,
            JsonRequest = "{\"test\": \"data\"}",
            CurrentStatus = "Queued",
            DateTimeCreated = DateTime.UtcNow,
            LastModified = DateTime.UtcNow,
            Finished = false
        };

        _mockRequestHistoryRepository
            .Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync(requestHistory);

        // Act
        var result = await _controller.GetStatus(requestId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.Value.Should().BeOfType<StatusResponse>();
        
        var response = okResult.Value as StatusResponse;
        response.Should().NotBeNull();
        response!.Code.Should().Be(200);
        response.CodeStatus.Should().Be("Found");
        response.Data.Should().NotBeNull();
        response.Data.RequestId.Should().Be(requestId);
        response.Data.RequestJsonData.Should().Be("{\"test\": \"data\"}");
        response.Data.CurrentStauts.Should().Be("Queued");
    }

    [Fact]
    public async Task GetStatus_WithNonExistentId_ReturnsNotFoundResult()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        
        _mockRequestHistoryRepository
            .Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync((RequestHistory?)null);

        // Act
        var result = await _controller.GetStatus(requestId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult.Should().NotBeNull();
        notFoundResult!.Value.Should().BeOfType<StatusResponse>();
        
        var response = notFoundResult.Value as StatusResponse;
        response.Should().NotBeNull();
        response!.Code.Should().Be(404);
        response.CodeStatus.Should().Be("Status not found");
        response.Data.Should().BeNull();
    }

    [Fact]
    public async Task GetStatus_WithEmptyGuid_ReturnsNotFoundResult()
    {
        // Arrange
        var requestId = Guid.Empty;
        
        _mockRequestHistoryRepository
            .Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync((RequestHistory?)null);

        // Act
        var result = await _controller.GetStatus(requestId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        var response = notFoundResult!.Value as StatusResponse;
        response!.Code.Should().Be(404);
    }

    [Fact]
    public async Task GetStatus_CallsRepositoryWithCorrectId()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var requestHistory = new RequestHistory
        {
            RequestId = requestId,
            JsonRequest = "{}",
            CurrentStatus = "Started"
        };

        _mockRequestHistoryRepository
            .Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync(requestHistory);

        // Act
        await _controller.GetStatus(requestId);

        // Assert
        _mockRequestHistoryRepository.Verify(
            r => r.GetByIdAsync(requestId),
            Times.Once);
    }

    [Fact]
    public async Task GetStatus_WithComplexJsonRequest_ReturnsCorrectData()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var complexJson = "{\"user\":{\"name\":\"test\",\"data\":{\"nested\":\"value\"}}}";
        var requestHistory = new RequestHistory
        {
            RequestId = requestId,
            JsonRequest = complexJson,
            CurrentStatus = "Completed",
            DateTimeCreated = DateTime.UtcNow,
            Finished = true
        };

        _mockRequestHistoryRepository
            .Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync(requestHistory);

        // Act
        var result = await _controller.GetStatus(requestId);

        // Assert
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as StatusResponse;
        response!.Data.RequestJsonData.Should().Be(complexJson);
    }

    [Theory]
    [InlineData("Queued")]
    [InlineData("Started")]
    [InlineData("PostData")]
    [InlineData("WaitingSms")]
    [InlineData("ConfirmSms")]
    [InlineData("PasswordSet")]
    [InlineData("Authorization")]
    [InlineData("PersonDataUpdate")]
    [InlineData("PostmailWaiting")]
    [InlineData("PostmailConfirmation")]
    [InlineData("Completed")]
    public async Task GetStatus_WithVariousStatuses_ReturnsCorrectStatus(string status)
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var requestHistory = new RequestHistory
        {
            RequestId = requestId,
            JsonRequest = "{}",
            CurrentStatus = status
        };

        _mockRequestHistoryRepository
            .Setup(r => r.GetByIdAsync(requestId))
            .ReturnsAsync(requestHistory);

        // Act
        var result = await _controller.GetStatus(requestId);

        // Assert
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as StatusResponse;
        response!.Data.CurrentStauts.Should().Be(status);
    }

    [Fact]
    public async Task GetStatus_WhenRepositoryThrowsException_PropagatesException()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        _mockRequestHistoryRepository
            .Setup(r => r.GetByIdAsync(requestId))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _controller.GetStatus(requestId));
    }
}