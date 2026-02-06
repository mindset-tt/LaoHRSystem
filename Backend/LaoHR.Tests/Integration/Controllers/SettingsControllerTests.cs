using System.Net;
using FluentAssertions;
using LaoHR.Tests.Helpers;
using Xunit;

namespace LaoHR.Tests.Integration.Controllers;

public class SettingsControllerTests : TestBase
{
    public SettingsControllerTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetSettings_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/settings");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
