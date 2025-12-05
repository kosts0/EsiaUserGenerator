using EsiaUserGenerator.Db.Models;
using FluentAssertions;
using Xunit;

namespace EsiaUserGenerator.Tests.Models;

public class RequestHistoryTests
{
    [Fact]
    public void RequestHistory_CanBeCreatedWithDefaultValues()
    {
        // Act
        var history = new RequestHistory();

        // Assert
        history.Should().NotBeNull();
        history.RequestId.Should().Be(Guid.Empty);
        history.Finished.Should().BeFalse();
    }

    [Fact]
    public void RequestHistory_CanSetAndGetRequestId()
    {
        // Arrange
        var history = new RequestHistory();
        var expectedId = Guid.NewGuid();

        // Act
        history.RequestId = expectedId;

        // Assert
        history.RequestId.Should().Be(expectedId);
    }

    [Fact]
    public void RequestHistory_CanSetAndGetJsonRequest()
    {
        // Arrange
        var history = new RequestHistory();
        var expectedJson = "{\"test\":\"data\"}";

        // Act
        history.JsonRequest = expectedJson;

        // Assert
        history.JsonRequest.Should().Be(expectedJson);
    }

    [Fact]
    public void RequestHistory_CanSetAndGetCurrentStatus()
    {
        // Arrange
        var history = new RequestHistory();
        var expectedStatus = "Queued";

        // Act
        history.CurrentStatus = expectedStatus;

        // Assert
        history.CurrentStatus.Should().Be(expectedStatus);
    }

    [Fact]
    public void RequestHistory_CanSetAndGetDateTimeCreated()
    {
        // Arrange
        var history = new RequestHistory();
        var expectedDate = DateTime.UtcNow;

        // Act
        history.DateTimeCreated = expectedDate;

        // Assert
        history.DateTimeCreated.Should().Be(expectedDate);
    }

    [Fact]
    public void RequestHistory_CanSetAndGetLastModified()
    {
        // Arrange
        var history = new RequestHistory();
        var expectedDate = DateTime.UtcNow;

        // Act
        history.LastModified = expectedDate;

        // Assert
        history.LastModified.Should().Be(expectedDate);
    }

    [Fact]
    public void RequestHistory_CanSetAndGetFinished()
    {
        // Arrange
        var history = new RequestHistory();

        // Act
        history.Finished = true;

        // Assert
        history.Finished.Should().BeTrue();
    }

    [Fact]
    public void RequestHistory_HasOneToOneRelationshipWithUser()
    {
        // Arrange
        var history = new RequestHistory
        {
            RequestId = Guid.NewGuid(),
            JsonRequest = "{}",
            CurrentStatus = "Queued"
        };
        var user = new EsiaUser
        {
            Id = Guid.NewGuid(),
            Login = "testuser",
            Password = "password123"
        };

        // Act
        history.User = user;
        user.RequestData = history;

        // Assert
        history.User.Should().Be(user);
        user.RequestData.Should().Be(history);
    }

    [Fact]
    public void RequestHistory_CanStoreComplexJsonData()
    {
        // Arrange
        var history = new RequestHistory();
        var complexJson = @"{
            ""user"": {
                ""name"": ""Test User"",
                ""phone"": ""+79991234567"",
                ""nested"": {
                    ""data"": ""value""
                }
            }
        }";

        // Act
        history.JsonRequest = complexJson;

        // Assert
        history.JsonRequest.Should().Be(complexJson);
    }

    [Theory]
    [InlineData("Queued")]
    [InlineData("Started")]
    [InlineData("PostData")]
    [InlineData("WaitingSms")]
    [InlineData("Completed")]
    public void RequestHistory_CanStoreVariousStatusValues(string status)
    {
        // Arrange
        var history = new RequestHistory();

        // Act
        history.CurrentStatus = status;

        // Assert
        history.CurrentStatus.Should().Be(status);
    }

    [Fact]
    public void RequestHistory_DateTimeCreatedCanBeSetToUtcNow()
    {
        // Arrange
        var history = new RequestHistory();
        var beforeTime = DateTime.UtcNow;

        // Act
        history.DateTimeCreated = DateTime.UtcNow;
        var afterTime = DateTime.UtcNow;

        // Assert
        history.DateTimeCreated.Should().BeOnOrAfter(beforeTime);
        history.DateTimeCreated.Should().BeOnOrBefore(afterTime);
    }

    [Fact]
    public void RequestHistory_LastModifiedCanBeUpdatedMultipleTimes()
    {
        // Arrange
        var history = new RequestHistory
        {
            LastModified = DateTime.UtcNow.AddHours(-1)
        };
        var oldModified = history.LastModified;

        // Act
        System.Threading.Thread.Sleep(10); // Small delay to ensure different timestamp
        history.LastModified = DateTime.UtcNow;

        // Assert
        history.LastModified.Should().BeAfter(oldModified);
    }

    [Fact]
    public void RequestHistory_FinishedDefaultsToFalse()
    {
        // Arrange & Act
        var history = new RequestHistory();

        // Assert
        history.Finished.Should().BeFalse();
    }

    [Fact]
    public void RequestHistory_CanCreateWithAllProperties()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var jsonRequest = "{\"data\":\"test\"}";
        var currentStatus = "Processing";
        var dateCreated = DateTime.UtcNow;
        var lastModified = DateTime.UtcNow;

        // Act
        var history = new RequestHistory
        {
            RequestId = requestId,
            JsonRequest = jsonRequest,
            CurrentStatus = currentStatus,
            DateTimeCreated = dateCreated,
            LastModified = lastModified,
            Finished = true
        };

        // Assert
        history.RequestId.Should().Be(requestId);
        history.JsonRequest.Should().Be(jsonRequest);
        history.CurrentStatus.Should().Be(currentStatus);
        history.DateTimeCreated.Should().Be(dateCreated);
        history.LastModified.Should().Be(lastModified);
        history.Finished.Should().BeTrue();
    }
}