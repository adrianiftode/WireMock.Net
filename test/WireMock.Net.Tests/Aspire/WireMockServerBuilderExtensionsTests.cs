#if NET8_0_OR_GREATER
using System;
using System.Linq;
using System.Net.Sockets;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using FluentAssertions;
using Moq;
using Xunit;

namespace WireMock.Net.Tests.Aspire;

public class WireMockServerBuilderExtensionsTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void AddWireMock_WithInvalidName_ShouldThrowException(string name)
    {
        // Arrange
        var builder = Mock.Of<IDistributedApplicationBuilder>();

        // Act
        Action act = () => builder.AddWireMock(name, 12345);

        // Assert
        act.Should().Throw<Exception>();
    }

    [Fact]
    public void AddWireMock_WithInvalidPort_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        const int invalidPort = -1;
        var builder = Mock.Of<IDistributedApplicationBuilder>();

        // Act
        Action act = () => builder.AddWireMock("ValidName", invalidPort);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>().WithMessage("Specified argument was out of the range of valid values. (Parameter 'port')");
    }

    [Fact]
    public void AddWireMock()
    {
        // Arrange
        var name = $"apiservice{Guid.NewGuid()}";
        const int port = 12345;
        const string username = "admin";
        const string password = "test";
        var builder = DistributedApplication.CreateBuilder();

        // Act
        var wiremock = builder
            .AddWireMock(name, port)
            .WithAdminUserNameAndPassword(username, password)
            .WithReadStaticMappings();

        // Assert
        wiremock.Resource.Should().NotBeNull();
        wiremock.Resource.Name.Should().Be(name);
        wiremock.Resource.Arguments.Should().BeEquivalentTo(new WireMockServerArguments
        {
            AdminPassword = password,
            AdminUsername = username,
            ReadStaticMappings = true,
            WithWatchStaticMappings = false,
            MappingsPath = null,
            Port = port
        });
        wiremock.Resource.Annotations.Should().HaveCount(3);

        var containerImageAnnotation = wiremock.Resource.Annotations.OfType<ContainerImageAnnotation>().FirstOrDefault();
        containerImageAnnotation.Should().BeEquivalentTo(new ContainerImageAnnotation
        {
            Image = "sheyenrath/wiremock.net",
            Registry = null,
            Tag = "latest"
        });

        var endpointAnnotation = wiremock.Resource.Annotations.OfType<EndpointAnnotation>().FirstOrDefault();
        endpointAnnotation.Should().BeEquivalentTo(new EndpointAnnotation(
            protocol: ProtocolType.Tcp,
            uriScheme: "http",
            transport: null,
            name: null,
            port: port,
            targetPort: 80,
            isExternal: null,
            isProxied: true
        ));

        wiremock.Resource.Annotations.OfType<CommandLineArgsCallbackAnnotation>().FirstOrDefault().Should().NotBeNull();
    }
}
#endif