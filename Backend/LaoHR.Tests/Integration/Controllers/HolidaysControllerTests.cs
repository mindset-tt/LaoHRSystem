using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using LaoHR.Shared.Models;
using LaoHR.Shared.Pagination;
using LaoHR.Tests.Helpers;
using Xunit;

namespace LaoHR.Tests.Integration.Controllers;

public class HolidaysControllerTests : TestBase
{
    public HolidaysControllerTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetHolidays_ReturnsList()
    {
        // Arrange
        await AuthenticateAsync();

        // Act
        var response = await _client.GetAsync("/api/holidays");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginatedResponse<Holiday>>();
        result.Should().NotBeNull();
    }
}
