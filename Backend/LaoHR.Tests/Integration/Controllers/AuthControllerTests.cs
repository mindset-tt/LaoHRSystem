using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using LaoHR.API.Controllers;
using LaoHR.Tests.Helpers;
using Xunit;

namespace LaoHR.Tests.Integration.Controllers;

public class AuthControllerTests : TestBase
{
    public AuthControllerTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsToken()
    {
        // Arrange
        var request = new LoginRequest { Username = "admin", Password = "admin123" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrEmpty();
        result.Role.Should().Be("Admin");
    }

    [Fact]
    public async Task Login_InvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var request = new LoginRequest { Username = "admin", Password = "wrongpassword" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("admin", "admin123", "Admin")]
    [InlineData("hradmin", "hr123", "HR")]
    [InlineData("employee", "emp123", "Employee")]
    public async Task Login_SeededAccounts_AllAuthenticate(string username, string password, string expectedRole)
    {
        // Phase 3C2B — every seeded demo account must pass the login validator
        // and authenticate successfully. This guards against the "hr" (2-char)
        // username defect where a seeded account could never log in.
        var request = new LoginRequest { Username = username, Password = password };

        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        result.Should().NotBeNull();
        result!.Role.Should().Be(expectedRole);
    }
}
