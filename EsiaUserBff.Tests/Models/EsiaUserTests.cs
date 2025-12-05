using EsiaUserGenerator.Db.Models;
using FluentAssertions;
using Xunit;

namespace EsiaUserGenerator.Tests.Models;

public class EsiaUserTests
{
    [Fact]
    public void EsiaUser_CanBeCreatedWithDefaultValues()
    {
        // Act
        var user = new EsiaUser();

        // Assert
        user.Should().NotBeNull();
        user.Id.Should().Be(Guid.Empty);
    }

    [Fact]
    public void EsiaUser_CanSetAndGetId()
    {
        // Arrange
        var user = new EsiaUser();
        var expectedId = Guid.NewGuid();

        // Act
        user.Id = expectedId;

        // Assert
        user.Id.Should().Be(expectedId);
    }

    [Fact]
    public void EsiaUser_CanSetAndGetOid()
    {
        // Arrange
        var user = new EsiaUser();
        var expectedOid = "123456789";

        // Act
        user.Oid = expectedOid;

        // Assert
        user.Oid.Should().Be(expectedOid);
    }

    [Fact]
    public void EsiaUser_OidCanBeNull()
    {
        // Arrange
        var user = new EsiaUser();

        // Act
        user.Oid = null;

        // Assert
        user.Oid.Should().BeNull();
    }

    [Fact]
    public void EsiaUser_CanSetAndGetLogin()
    {
        // Arrange
        var user = new EsiaUser();
        var expectedLogin = "+79991234567";

        // Act
        user.Login = expectedLogin;

        // Assert
        user.Login.Should().Be(expectedLogin);
    }

    [Fact]
    public void EsiaUser_CanSetAndGetPassword()
    {
        // Arrange
        var user = new EsiaUser();
        var expectedPassword = "SecurePassword123!";

        // Act
        user.Password = expectedPassword;

        // Assert
        user.Password.Should().Be(expectedPassword);
    }

    [Fact]
    public void EsiaUser_CanSetAndGetCreatedRequestId()
    {
        // Arrange
        var user = new EsiaUser();
        var expectedRequestId = Guid.NewGuid();

        // Act
        user.CreatedRequestId = expectedRequestId;

        // Assert
        user.CreatedRequestId.Should().Be(expectedRequestId);
    }

    [Fact]
    public void EsiaUser_CreatedRequestIdCanBeNull()
    {
        // Arrange
        var user = new EsiaUser();

        // Act
        user.CreatedRequestId = null;

        // Assert
        user.CreatedRequestId.Should().BeNull();
    }

    [Fact]
    public void EsiaUser_HasOneToOneRelationshipWithRequestData()
    {
        // Arrange
        var user = new EsiaUser
        {
            Id = Guid.NewGuid(),
            Login = "testuser",
            Password = "password123"
        };
        var requestData = new RequestHistory
        {
            RequestId = Guid.NewGuid(),
            JsonRequest = "{}",
            CurrentStatus = "Queued"
        };

        // Act
        user.RequestData = requestData;
        requestData.User = user;

        // Assert
        user.RequestData.Should().Be(requestData);
        requestData.User.Should().Be(user);
    }

    [Fact]
    public void EsiaUser_CanCreateWithAllRequiredProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var login = "+79991234567";
        var password = "TestPassword123!";

        // Act
        var user = new EsiaUser
        {
            Id = id,
            Login = login,
            Password = password
        };

        // Assert
        user.Id.Should().Be(id);
        user.Login.Should().Be(login);
        user.Password.Should().Be(password);
    }

    [Fact]
    public void EsiaUser_CanCreateWithAllProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var oid = "987654321";
        var login = "+79991234567";
        var password = "TestPassword123!";
        var requestId = Guid.NewGuid();

        // Act
        var user = new EsiaUser
        {
            Id = id,
            Oid = oid,
            Login = login,
            Password = password,
            CreatedRequestId = requestId
        };

        // Assert
        user.Id.Should().Be(id);
        user.Oid.Should().Be(oid);
        user.Login.Should().Be(login);
        user.Password.Should().Be(password);
        user.CreatedRequestId.Should().Be(requestId);
    }

    [Theory]
    [InlineData("+79991234567")]
    [InlineData("+7(999)123-45-67")]
    [InlineData("testuser@example.com")]
    [InlineData("username123")]
    public void EsiaUser_AcceptsVariousLoginFormats(string login)
    {
        // Arrange
        var user = new EsiaUser();

        // Act
        user.Login = login;

        // Assert
        user.Login.Should().Be(login);
    }

    [Fact]
    public void EsiaUser_LoginCanBeNull()
    {
        // Arrange
        var user = new EsiaUser();

        // Act
        user.Login = null;

        // Assert
        user.Login.Should().BeNull();
    }

    [Fact]
    public void EsiaUser_PasswordCanBeNull()
    {
        // Arrange
        var user = new EsiaUser();

        // Act
        user.Password = null;

        // Assert
        user.Password.Should().BeNull();
    }

    [Fact]
    public void EsiaUser_CanLinkToRequestHistory()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var requestId = Guid.NewGuid();
        
        var user = new EsiaUser
        {
            Id = userId,
            Login = "test",
            Password = "password",
            CreatedRequestId = requestId
        };
        
        var requestHistory = new RequestHistory
        {
            RequestId = requestId,
            JsonRequest = "{}",
            CurrentStatus = "Queued"
        };

        // Act
        user.RequestData = requestHistory;

        // Assert
        user.RequestData.Should().NotBeNull();
        user.RequestData.RequestId.Should().Be(requestId);
    }
}