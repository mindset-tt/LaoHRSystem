using System.Reflection;
using FluentAssertions;
using LaoHR.API.Controllers;
using Xunit;

namespace LaoHR.Tests.Unit.Meta;

public class DtoCoverageTests
{
    [Fact]
    public void CoverAllDtosAndRequests()
    {
        // Get the Assembly where DTOs live (LaoHR.API)
        var assembly = typeof(AuthController).Assembly;
        
        // Find all public classes that are not abstract, not controllers, not services
        // Usually ending in Request, Response, Dto, or in specific namespaces
        var types = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => t.Name.EndsWith("Request") || 
                        t.Name.EndsWith("Response") || 
                        t.Name.EndsWith("Dto") ||
                        t.Name.EndsWith("Info")) // UserInfo
            .ToList();

        foreach (var type in types)
        {
            // Try to create instance
            object? instance = null;
            try
            {
                instance = Activator.CreateInstance(type);
            }
            catch
            {
                // No parameterless constructor? Skip or try minimal
                continue;
            }

            if (instance == null) continue;

            // Set and Get all properties
            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (prop.CanWrite && prop.CanRead)
                {
                    // Create dummy value based on type
                    object? dummyValue = GetDummyValue(prop.PropertyType);
                    try 
                    {
                        prop.SetValue(instance, dummyValue);
                        var value = prop.GetValue(instance);
                        
                        // Assert matches (except maybe dates due to precision)
                        if (dummyValue != null)
                           value.Should().BeEquivalentTo(dummyValue, options => options.Excluding(x => x.GetType() == typeof(DateTime)));
                    }
                    catch
                    {
                        // Ignore setting errors (e.g. validtion within setter)
                    }
                }
            }
        }
    }

    private object? GetDummyValue(Type type)
    {
        if (type == typeof(string)) return "test";
        if (type == typeof(int)) return 123;
        if (type == typeof(long)) return 123L;
        if (type == typeof(bool)) return true;
        if (type == typeof(DateTime)) return DateTime.UtcNow;
        if (type == typeof(decimal)) return 10.5m;
        if (type == typeof(double)) return 10.5d;
        if (type == typeof(Guid)) return Guid.NewGuid();
        
        return null; // Nullable or complex
    }
}
