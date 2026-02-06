using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using LaoHR.Shared.Models;
using LaoHR.Tests.Helpers;
using Xunit;

namespace LaoHR.Tests.Integration.Controllers;

public class HolidaysControllerTests : TestBase
{
    public HolidaysControllerTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetHolidays_ReturnsList()
    {
        // Act
        var response = await _client.GetAsync("/api/holidays");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var list = await response.Content.ReadFromJsonAsync<List<Holiday>>();
        list.Should().NotBeNull();
    }
}
