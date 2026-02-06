using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using LaoHR.API.Data;
using LaoHR.Shared.Data;
using LaoHR.API.Services;
using Moq;

namespace LaoHR.Tests.Helpers;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove real IEmailService
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IEmailService));
            if (descriptor != null) services.Remove(descriptor);

            // Add Mock IEmailService
            var mockEmail = new Mock<IEmailService>();
            mockEmail.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                     .Returns(Task.CompletedTask);
            services.AddSingleton(mockEmail.Object);

            var sp = services.BuildServiceProvider();

            using (var scope = sp.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<LaoHRDbContext>();
                
                db.Database.EnsureCreated();
                
                try
                {
                    SeedData(db);
                }
                catch (Exception ex)
                {
                    // Logging
                }
            }
        });
    }

    private void SeedData(LaoHRDbContext db)
    {
        // Add users/employees if needed
    }
}
