using System.Net;
using FluentAssertions;
using LaoHR.Tests.Helpers;
using Xunit;

namespace LaoHR.Tests.Integration.Controllers;

public class DocumentsControllerTests : TestBase
{
    public DocumentsControllerTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetDocuments_ValidId_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/documents/employee/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
