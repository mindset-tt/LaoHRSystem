using FluentAssertions;
using LaoHR.Shared.Models;
using Xunit;

namespace LaoHR.Tests.Unit.Models;

public class EmployeeDocumentTests
{
    [Fact]
    public void EmployeeDocument_Properties_Work()
    {
        var doc = new EmployeeDocument
        {
            DocumentId = 1,
            EmployeeId = 2,
            DocumentType = "Passport",
            FilePath = "/tmp/pass.pdf",
            UploadedAt = DateTime.MinValue
        };
        
        doc.DocumentId.Should().Be(1);
        doc.EmployeeId.Should().Be(2);
        doc.DocumentType.Should().Be("Passport");
    }
}
