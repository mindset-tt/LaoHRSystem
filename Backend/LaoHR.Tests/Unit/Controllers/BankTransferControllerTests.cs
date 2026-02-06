using FluentAssertions;
using LaoHR.API.Controllers;
using LaoHR.API.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace LaoHR.Tests.Unit.Controllers;

public class BankTransferControllerTests
{
    [Fact]
    public async Task DownloadBcelFile_Success_ReturnsFile()
    {
        // Arrange
        var mockService = new Mock<IBankTransferService>();
        mockService.Setup(x => x.GenerateBcelTransferFile(It.IsAny<int>()))
                   .ReturnsAsync(new byte[] { 1, 2, 3 });
        
        var controller = new BankTransferController(mockService.Object);
        
        // Act
        var result = await controller.DownloadBcelFile(1);
        
        // Assert
        var fileResult = result.Should().BeOfType<FileContentResult>().Subject;
        fileResult.ContentType.Should().Be("text/plain");
        fileResult.FileContents.Should().HaveCount(3);
    }
    
    [Fact]
    public async Task DownloadBcelFile_Error_ReturnsBadRequest()
    {
        // Arrange
        var mockService = new Mock<IBankTransferService>();
        mockService.Setup(x => x.GenerateBcelTransferFile(It.IsAny<int>()))
                   .ThrowsAsync(new Exception("Fail"));
        
        var controller = new BankTransferController(mockService.Object);
        
        // Act
        var result = await controller.DownloadBcelFile(1);
        
        // Assert
        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.Value.Should().Be("Fail");
    }

    [Fact]
    public async Task DownloadLdbFile_Success_ReturnsFile()
    {
        var mockService = new Mock<IBankTransferService>();
        mockService.Setup(x => x.GenerateLdbTransferFile(It.IsAny<int>()))
                   .ReturnsAsync(new byte[] { 4, 5, 6 });
        
        var controller = new BankTransferController(mockService.Object);
        
        var result = await controller.DownloadLdbFile(1);
        
        var fileResult = result.Should().BeOfType<FileContentResult>().Subject;
        fileResult.ContentType.Should().Be("text/csv");
    }
    
    [Fact]
    public async Task DownloadLdbFile_Error_ReturnsBadRequest()
    {
        var mockService = new Mock<IBankTransferService>();
        mockService.Setup(x => x.GenerateLdbTransferFile(It.IsAny<int>()))
                   .ThrowsAsync(new Exception("Fail LDB"));
        
        var controller = new BankTransferController(mockService.Object);
        
        var result = await controller.DownloadLdbFile(1);
        
        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.Value.Should().Be("Fail LDB");
    }
}
