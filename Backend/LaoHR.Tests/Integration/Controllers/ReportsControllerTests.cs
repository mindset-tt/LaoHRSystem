using System.Net;
using FluentAssertions;
using LaoHR.Tests.Helpers;
using Xunit;

namespace LaoHR.Tests.Integration.Controllers;

public class ReportsControllerTests : TestBase
{
    public ReportsControllerTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task DownloadNssf_InvalidPeriod_ReturnsBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/reports/nssf/999"); // Non-existent period

        // Assert
        // The service throws FileNotFoundException, controller catches Exception -> BadRequest
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
