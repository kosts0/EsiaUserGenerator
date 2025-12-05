using EsiaUserGenerator.Dto.Enum;
using FluentAssertions;
using Xunit;

namespace EsiaUserGenerator.Tests.Dto;

public class UserCreationFlowTests
{
    [Fact]
    public void UserCreationFlow_HasExpectedValues()
    {
        // Assert
        Enum.GetValues<UserCreationFlow>().Should().HaveCount(11);
    }

    [Theory]
    [InlineData(UserCreationFlow.Queued, 0)]
    [InlineData(UserCreationFlow.Started, 1)]
    [InlineData(UserCreationFlow.PostData, 2)]
    [InlineData(UserCreationFlow.WaitingSms, 3)]
    [InlineData(UserCreationFlow.ConfirmSms, 4)]
    [InlineData(UserCreationFlow.PasswordSet, 5)]
    [InlineData(UserCreationFlow.Authorization, 6)]
    [InlineData(UserCreationFlow.PersonDataUpdate, 7)]
    [InlineData(UserCreationFlow.PostmailWaiting, 8)]
    [InlineData(UserCreationFlow.PostmailConfirmation, 9)]
    [InlineData(UserCreationFlow.Completed, 10)]
    public void UserCreationFlow_HasCorrectEnumValue(UserCreationFlow flow, int expectedValue)
    {
        // Assert
        ((int)flow).Should().Be(expectedValue);
    }

    [Theory]
    [InlineData(UserCreationFlow.Queued, "Queued")]
    [InlineData(UserCreationFlow.Started, "Started")]
    [InlineData(UserCreationFlow.PostData, "PostData")]
    [InlineData(UserCreationFlow.WaitingSms, "WaitingSms")]
    [InlineData(UserCreationFlow.ConfirmSms, "ConfirmSms")]
    [InlineData(UserCreationFlow.PasswordSet, "PasswordSet")]
    [InlineData(UserCreationFlow.Authorization, "Authorization")]
    [InlineData(UserCreationFlow.PersonDataUpdate, "PersonDataUpdate")]
    [InlineData(UserCreationFlow.PostmailWaiting, "PostmailWaiting")]
    [InlineData(UserCreationFlow.PostmailConfirmation, "PostmailConfirmation")]
    [InlineData(UserCreationFlow.Completed, "Completed")]
    public void UserCreationFlow_ToStringReturnsCorrectName(UserCreationFlow flow, string expectedName)
    {
        // Act
        var result = flow.ToString();

        // Assert
        result.Should().Be(expectedName);
    }

    [Fact]
    public void UserCreationFlow_CanParseFromString()
    {
        // Arrange
        var flowName = "Authorization";

        // Act
        var result = Enum.Parse<UserCreationFlow>(flowName);

        // Assert
        result.Should().Be(UserCreationFlow.Authorization);
    }

    [Fact]
    public void UserCreationFlow_CanConvertToInt()
    {
        // Arrange
        var flow = UserCreationFlow.ConfirmSms;

        // Act
        int intValue = (int)flow;

        // Assert
        intValue.Should().Be(4);
    }

    [Fact]
    public void UserCreationFlow_AllValuesAreUnique()
    {
        // Arrange
        var allValues = Enum.GetValues<UserCreationFlow>();

        // Act
        var uniqueValues = allValues.Distinct();

        // Assert
        uniqueValues.Should().HaveCount(allValues.Length);
    }

    [Fact]
    public void UserCreationFlow_AllNamesAreUnique()
    {
        // Arrange
        var allNames = Enum.GetNames<UserCreationFlow>();

        // Act
        var uniqueNames = allNames.Distinct();

        // Assert
        uniqueNames.Should().HaveCount(allNames.Length);
    }

    [Fact]
    public void UserCreationFlow_FirstStepIsQueued()
    {
        // Arrange
        var firstValue = Enum.GetValues<UserCreationFlow>().First();

        // Assert
        firstValue.Should().Be(UserCreationFlow.Queued);
    }

    [Fact]
    public void UserCreationFlow_LastStepIsCompleted()
    {
        // Arrange
        var lastValue = Enum.GetValues<UserCreationFlow>().Last();

        // Assert
        lastValue.Should().Be(UserCreationFlow.Completed);
    }

    [Fact]
    public void UserCreationFlow_CanBeUsedInSwitch()
    {
        // Arrange
        var flow = UserCreationFlow.PostData;
        var result = string.Empty;

        // Act
        switch (flow)
        {
            case UserCreationFlow.Queued:
                result = "Queued";
                break;
            case UserCreationFlow.PostData:
                result = "PostData";
                break;
            default:
                result = "Other";
                break;
        }

        // Assert
        result.Should().Be("PostData");
    }

    [Fact]
    public void UserCreationFlow_IsDefined_ReturnsTrueForValidValue()
    {
        // Act
        var isDefined = Enum.IsDefined(typeof(UserCreationFlow), UserCreationFlow.WaitingSms);

        // Assert
        isDefined.Should().BeTrue();
    }

    [Fact]
    public void UserCreationFlow_IsDefined_ReturnsFalseForInvalidValue()
    {
        // Act
        var isDefined = Enum.IsDefined(typeof(UserCreationFlow), 999);

        // Assert
        isDefined.Should().BeFalse();
    }
}