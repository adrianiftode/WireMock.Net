#if NET8_0_OR_GREATER
using Aspire.Hosting;
using FluentAssertions;
using Xunit;

namespace WireMock.Net.Tests.Aspire;

public class WireMockServerArgumentsTests
{
    [Fact]
    public void DefaultValues_ShouldBeSetCorrectly()
    {
        // Arrange & Act
        var args = new WireMockServerArguments();

        // Assert
        args.Port.Should().Be(WireMockServerArguments.DefaultPort);
        args.AdminUsername.Should().BeNull();
        args.AdminPassword.Should().BeNull();
        args.ReadStaticMappings.Should().BeFalse();
        args.WithWatchStaticMappings.Should().BeFalse();
        args.MappingsPath.Should().BeNull();
    }

    [Fact]
    public void HasBasicAuthentication_ShouldReturnTrue_WhenUsernameAndPasswordAreProvided()
    {
        // Arrange
        var args = new WireMockServerArguments
        {
            AdminUsername = "admin",
            AdminPassword = "password"
        };

        // Act & Assert
        args.HasBasicAuthentication.Should().BeTrue();
    }

    [Fact]
    public void HasBasicAuthentication_ShouldReturnFalse_WhenEitherUsernameOrPasswordIsNotProvided()
    {
        // Arrange
        var argsWithUsernameOnly = new WireMockServerArguments { AdminUsername = "admin" };
        var argsWithPasswordOnly = new WireMockServerArguments { AdminPassword = "password" };

        // Act & Assert
        argsWithUsernameOnly.HasBasicAuthentication.Should().BeFalse();
        argsWithPasswordOnly.HasBasicAuthentication.Should().BeFalse();
    }

    [Fact]
    public void GetArgs_WhenReadStaticMappingsIsTrue_ShouldContainReadStaticMappingsTrue()
    {
        // Arrange
        var args = new WireMockServerArguments
        {
            ReadStaticMappings = true
        };

        // Act
        var commandLineArgs = args.GetArgs();

        // Assert
        commandLineArgs.Should().ContainInOrder("--ReadStaticMappings", "true");
    }

    [Fact]
    public void GetArgs_WhenReadStaticMappingsIsFalse_ShouldNotContainReadStaticMappingsTrue()
    {
        // Arrange
        var args = new WireMockServerArguments
        {
            ReadStaticMappings = false
        };

        // Act
        var commandLineArgs = args.GetArgs();

        // Assert
        commandLineArgs.Should().NotContain("--ReadStaticMappings", "true");
    }

    [Fact]
    public void GetArgs_WhenWithWatchStaticMappingsIsTrue_ShouldContainWatchStaticMappingsTrue()
    {
        // Arrange
        var args = new WireMockServerArguments
        {
            WithWatchStaticMappings = true,
            ReadStaticMappings = false  // This needs to be false to check the independent effect of WithWatchStaticMappings
        };

        // Act
        var commandLineArgs = args.GetArgs();

        // Assert
        commandLineArgs.Should().ContainInOrder("--ReadStaticMappings", "true", "--WatchStaticMappings", "true", "--WatchStaticMappingsInSubdirectories", "true");
    }

    [Fact]
    public void GetArgs_WhenWithWatchStaticMappingsIsFalse_ShouldNotContainWatchStaticMappingsTrue()
    {
        // Arrange
        var args = new WireMockServerArguments
        {
            WithWatchStaticMappings = false
        };

        // Act
        var commandLineArgs = args.GetArgs();

        // Assert
        commandLineArgs.Should().NotContain("--WatchStaticMappings", "true").And.NotContain("--WatchStaticMappingsInSubdirectories", "true");
    }

    [Fact]
    public void GetArgs_ShouldIncludeAuthenticationDetails_WhenAuthenticationIsRequired()
    {
        // Arrange
        var args = new WireMockServerArguments
        {
            AdminUsername = "admin",
            AdminPassword = "password"
        };

        // Act
        var commandLineArgs = args.GetArgs();

        // Assert
        commandLineArgs.Should().Contain("--AdminUserName", "admin");
        commandLineArgs.Should().Contain("--AdminPassword", "password");
    }
}
#endif