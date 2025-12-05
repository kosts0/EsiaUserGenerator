using EsiaUserGenerator.Dto.API;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace EsiaUserGenerator.Tests.Dto;

public class StatusResponseTests
{
    [Fact]
    public void StatusResponse_InheritsFromResponceBase()
    {
        // Arrange & Act
        var response = new StatusResponse();

        // Assert
        response.Should().BeAssignableTo<ResponceBase<StatusData>>();
    }

    [Fact]
    public void StatusResponse_CanSetAndGetCode()
    {
        // Arrange
        var response = new StatusResponse();
        var expectedCode = 200;

        // Act
        response.Code = expectedCode;

        // Assert
        response.Code.Should().Be(expectedCode);
    }

    [Fact]
    public void StatusResponse_CanSetAndGetCodeStatus()
    {
        // Arrange
        var response = new StatusResponse();
        var expectedStatus = "Found";

        // Act
        response.CodeStatus = expectedStatus;

        // Assert
        response.CodeStatus.Should().Be(expectedStatus);
    }

    [Fact]
    public void StatusResponse_CanSetAndGetData()
    {
        // Arrange
        var response = new StatusResponse();
        var expectedData = new StatusData
        {
            RequestId = Guid.NewGuid(),
            RequestJsonData = "{}",
            CurrentStauts = "Queued"
        };

        // Act
        response.Data = expectedData;

        // Assert
        response.Data.Should().Be(expectedData);
    }

    [Fact]
    public void StatusResponse_CanSerializeToJson()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var response = new StatusResponse
        {
            Code = 200,
            CodeStatus = "Success",
            Data = new StatusData
            {
                RequestId = requestId,
                RequestJsonData = "{\"test\":\"data\"}",
                CurrentStauts = "Completed"
            }
        };

        // Act
        var json = JsonConvert.SerializeObject(response);

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("200");
        json.Should().Contain("Success");
        json.Should().Contain(requestId.ToString());
    }

    [Fact]
    public void StatusResponse_CanDeserializeFromJson()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var json = $@"{{
            ""Code"": 200,
            ""CodeStatus"": ""Found"",
            ""Data"": {{
                ""RequestId"": ""{requestId}"",
                ""RequestJsonData"": ""{{}}"",
                ""CurrentStauts"": ""Queued""
            }}
        }}";

        // Act
        var response = JsonConvert.DeserializeObject<StatusResponse>(json);

        // Assert
        response.Should().NotBeNull();
        response!.Code.Should().Be(200);
        response.CodeStatus.Should().Be("Found");
        response.Data.Should().NotBeNull();
        response.Data.RequestId.Should().Be(requestId);
    }

    [Fact]
    public void StatusResponse_ExceptionIsNotSerializedToJson()
    {
        // Arrange
        var response = new StatusResponse
        {
            Code = 500,
            CodeStatus = "Error",
            Exception = new InvalidOperationException("Test exception")
        };

        // Act
        var json = JsonConvert.SerializeObject(response);

        // Assert
        json.Should().NotContain("Exception");
        json.Should().NotContain("InvalidOperationException");
    }
}

public class StatusDataTests
{
    [Fact]
    public void StatusData_CanSetAndGetRequestId()
    {
        // Arrange
        var data = new StatusData();
        var expectedId = Guid.NewGuid();

        // Act
        data.RequestId = expectedId;

        // Assert
        data.RequestId.Should().Be(expectedId);
    }

    [Fact]
    public void StatusData_CanSetAndGetRequestJsonData()
    {
        // Arrange
        var data = new StatusData();
        var expectedJson = "{\"user\":\"test\"}";

        // Act
        data.RequestJsonData = expectedJson;

        // Assert
        data.RequestJsonData.Should().Be(expectedJson);
    }

    [Fact]
    public void StatusData_CanSetAndGetCurrentStauts()
    {
        // Arrange
        var data = new StatusData();
        var expectedStatus = "WaitingSms";

        // Act
        data.CurrentStauts = expectedStatus;

        // Assert
        data.CurrentStauts.Should().Be(expectedStatus);
    }

    [Fact]
    public void StatusData_RequestIdCanBeNull()
    {
        // Arrange
        var data = new StatusData();

        // Act
        data.RequestId = null;

        // Assert
        data.RequestId.Should().BeNull();
    }

    [Fact]
    public void StatusData_CanCreateWithAllProperties()
    {
        // Arrange
        var requestId = Guid.NewGuid();
        var jsonData = "{\"test\":\"value\"}";
        var status = "Completed";

        // Act
        var data = new StatusData
        {
            RequestId = requestId,
            RequestJsonData = jsonData,
            CurrentStauts = status
        };

        // Assert
        data.RequestId.Should().Be(requestId);
        data.RequestJsonData.Should().Be(jsonData);
        data.CurrentStauts.Should().Be(status);
    }

    [Theory]
    [InlineData("")]
    [InlineData("{}")]
    [InlineData("{\"complex\":{\"nested\":\"data\"}}")]
    [InlineData("[\"array\",\"data\"]")]
    public void StatusData_AcceptsVariousJsonFormats(string jsonData)
    {
        // Arrange
        var data = new StatusData();

        // Act
        data.RequestJsonData = jsonData;

        // Assert
        data.RequestJsonData.Should().Be(jsonData);
    }
}