using EsiaUserGenerator.Dto.API;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace EsiaUserGenerator.Tests.Dto;

// Concrete implementation for testing the abstract base class
public class TestResponse : ResponceBase<TestData>
{
}

public class TestData
{
    public string Value { get; set; } = string.Empty;
}

public class ResponceBaseTests
{
    [Fact]
    public void ResponceBase_CanSetAndGetCode()
    {
        // Arrange
        var response = new TestResponse();
        var expectedCode = 200;

        // Act
        response.Code = expectedCode;

        // Assert
        response.Code.Should().Be(expectedCode);
    }

    [Fact]
    public void ResponceBase_CanSetAndGetCodeStatus()
    {
        // Arrange
        var response = new TestResponse();
        var expectedStatus = "Success";

        // Act
        response.CodeStatus = expectedStatus;

        // Assert
        response.CodeStatus.Should().Be(expectedStatus);
    }

    [Fact]
    public void ResponceBase_CanSetAndGetData()
    {
        // Arrange
        var response = new TestResponse();
        var expectedData = new TestData { Value = "test" };

        // Act
        response.Data = expectedData;

        // Assert
        response.Data.Should().Be(expectedData);
        response.Data.Value.Should().Be("test");
    }

    [Fact]
    public void ResponceBase_CanSetAndGetExceptionMessage()
    {
        // Arrange
        var response = new TestResponse();
        var expectedMessage = "An error occurred";

        // Act
        response.ExceptionMessage = expectedMessage;

        // Assert
        response.ExceptionMessage.Should().Be(expectedMessage);
    }

    [Fact]
    public void ResponceBase_CanSetAndGetException()
    {
        // Arrange
        var response = new TestResponse();
        var expectedException = new InvalidOperationException("Test error");

        // Act
        response.Exception = expectedException;

        // Assert
        response.Exception.Should().Be(expectedException);
    }

    [Fact]
    public void ResponceBase_ExceptionIsNotSerializedWithJsonIgnoreAttribute()
    {
        // Arrange
        var response = new TestResponse
        {
            Code = 500,
            CodeStatus = "Error",
            Exception = new InvalidOperationException("Test exception"),
            ExceptionMessage = "Error message"
        };

        // Act
        var json = JsonConvert.SerializeObject(response);

        // Assert
        json.Should().NotContain("\"Exception\"");
        json.Should().Contain("\"ExceptionMessage\"");
        json.Should().Contain("Error message");
    }

    [Fact]
    public void ResponceBase_CanSerializeWithAllProperties()
    {
        // Arrange
        var response = new TestResponse
        {
            Code = 200,
            CodeStatus = "Success",
            Data = new TestData { Value = "test" },
            ExceptionMessage = null
        };

        // Act
        var json = JsonConvert.SerializeObject(response);

        // Assert
        json.Should().Contain("\"Code\":200");
        json.Should().Contain("\"CodeStatus\":\"Success\"");
        json.Should().Contain("\"Value\":\"test\"");
    }

    [Fact]
    public void ResponceBase_CanDeserialize()
    {
        // Arrange
        var json = @"{
            ""Code"": 404,
            ""CodeStatus"": ""Not Found"",
            ""Data"": { ""Value"": ""missing"" },
            ""ExceptionMessage"": ""Resource not found""
        }";

        // Act
        var response = JsonConvert.DeserializeObject<TestResponse>(json);

        // Assert
        response.Should().NotBeNull();
        response!.Code.Should().Be(404);
        response.CodeStatus.Should().Be("Not Found");
        response.Data.Should().NotBeNull();
        response.Data.Value.Should().Be("missing");
        response.ExceptionMessage.Should().Be("Resource not found");
    }

    [Theory]
    [InlineData(200)]
    [InlineData(201)]
    [InlineData(400)]
    [InlineData(404)]
    [InlineData(500)]
    public void ResponceBase_AcceptsVariousStatusCodes(int code)
    {
        // Arrange
        var response = new TestResponse();

        // Act
        response.Code = code;

        // Assert
        response.Code.Should().Be(code);
    }

    [Fact]
    public void ResponceBase_CodeCanBeNull()
    {
        // Arrange
        var response = new TestResponse();

        // Act
        response.Code = null;

        // Assert
        response.Code.Should().BeNull();
    }

    [Fact]
    public void ResponceBase_CodeStatusCanBeNull()
    {
        // Arrange
        var response = new TestResponse();

        // Act
        response.CodeStatus = null;

        // Assert
        response.CodeStatus.Should().BeNull();
    }

    [Fact]
    public void ResponceBase_ExceptionMessageCanBeNull()
    {
        // Arrange
        var response = new TestResponse();

        // Act
        response.ExceptionMessage = null;

        // Assert
        response.ExceptionMessage.Should().BeNull();
    }

    [Fact]
    public void ResponceBase_DataCanBeNull()
    {
        // Arrange
        var response = new TestResponse();

        // Act
        response.Data = null!;

        // Assert
        response.Data.Should().BeNull();
    }

    [Fact]
    public void ResponceBase_CanCreateSuccessResponse()
    {
        // Arrange & Act
        var response = new TestResponse
        {
            Code = 200,
            CodeStatus = "Success",
            Data = new TestData { Value = "result" }
        };

        // Assert
        response.Code.Should().Be(200);
        response.CodeStatus.Should().Be("Success");
        response.Data.Should().NotBeNull();
        response.ExceptionMessage.Should().BeNull();
        response.Exception.Should().BeNull();
    }

    [Fact]
    public void ResponceBase_CanCreateErrorResponse()
    {
        // Arrange & Act
        var exception = new InvalidOperationException("Database error");
        var response = new TestResponse
        {
            Code = 500,
            CodeStatus = "Internal Server Error",
            ExceptionMessage = exception.Message,
            Exception = exception
        };

        // Assert
        response.Code.Should().Be(500);
        response.CodeStatus.Should().Be("Internal Server Error");
        response.ExceptionMessage.Should().Be("Database error");
        response.Exception.Should().Be(exception);
    }
}