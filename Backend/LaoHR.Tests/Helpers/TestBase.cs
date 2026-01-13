using Microsoft.Extensions.DependencyInjection;
using Xunit;

using LaoHR.Shared.Data;

namespace LaoHR.Tests.Helpers;

public class TestBase : IClassFixture<CustomWebApplicationFactory>
{
    protected readonly CustomWebApplicationFactory _factory;
    protected readonly HttpClient _client;

    public TestBase(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    protected void ReseedDatabase()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<LaoHRDbContext>();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
        }
    }
}
