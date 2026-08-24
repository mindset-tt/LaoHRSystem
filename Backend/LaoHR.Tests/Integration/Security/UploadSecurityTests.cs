using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using LaoHR.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LaoHR.Tests.Integration.Security;

/// <summary>
/// Phase 4D.1 — upload hardening integration tests. Exercises the full HTTP
/// surface: extension allow-list, content-signature validation, size limit,
/// filename sanitization, protected storage, and authorized download.
/// </summary>
public class UploadSecurityTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public UploadSecurityTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task<HttpClient> LoginAsync(string username, string password)
    {
        var client = _factory.CreateClient();
        var resp = await client.PostAsJsonAsync("/api/auth/login", new { Username = username, Password = password });
        resp.EnsureSuccessStatusCode();
        var token = (await resp.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>()).GetProperty("token").GetString();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private static async Task<int> SeedEmployeeAsync(CustomWebApplicationFactory factory, string code)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LaoHRDbContext>();
        var emp = new Employee { EmployeeCode = code, LaoName = "Upload Test", IsActive = true };
        db.Employees.Add(emp);
        await db.SaveChangesAsync();
        return emp.EmployeeId;
    }

    private static MultipartFormDataContent BuildForm(int employeeId, string fileName, byte[] content)
    {
        var form = new MultipartFormDataContent();
        form.Add(new StringContent(employeeId.ToString()), "employeeId");
        form.Add(new StringContent("CONTRACT"), "documentType");
        var fileContent = new ByteArrayContent(content);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        form.Add(fileContent, "file", fileName);
        return form;
    }

    [Fact]
    public async Task ValidPdf_Upload_Succeeds_And_Downloads_With_Same_Bytes()
    {
        var empId = await SeedEmployeeAsync(_factory, "UPLD1");
        var client = await LoginAsync("admin", "admin123");

        var pdf = "%PDF-1.4 fake body for roundtrip test"u8.ToArray();
        using var form = BuildForm(empId, "contract.pdf", pdf);
        var resp = await client.PostAsync("/api/documents", form);
        resp.StatusCode.Should().Be(HttpStatusCode.Created);

        var doc = await resp.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        var docId = doc.GetProperty("documentId").GetInt32();

        // Download through the AUTHORIZED endpoint and compare bytes exactly
        // (also used by the DR document-restore checksum procedure).
        var download = await client.GetAsync($"/api/documents/{docId}/file");
        download.StatusCode.Should().Be(HttpStatusCode.OK);
        download.Content.Headers.ContentDisposition!.DispositionType.Should().Be("attachment");
        (await download.Content.ReadAsByteArrayAsync()).Should().Equal(pdf);

        // Cleanup row + file
        await client.DeleteAsync($"/api/documents/{docId}");
    }

    [Fact]
    public async Task Exe_Renamed_Pdf_Is_Rejected()
    {
        var empId = await SeedEmployeeAsync(_factory, "UPLD2");
        var client = await LoginAsync("admin", "admin123");

        var exe = new byte[256];
        exe[0] = (byte)'M'; exe[1] = (byte)'Z';
        using var form = BuildForm(empId, "innocent.pdf", exe);
        var resp = await client.PostAsync("/api/documents", form);
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Html_Renamed_Pdf_Is_Rejected()
    {
        var empId = await SeedEmployeeAsync(_factory, "UPLD3");
        var client = await LoginAsync("admin", "admin123");

        var html = "<html><script>alert(1)</script></html>"u8.ToArray();
        using var form = BuildForm(empId, "report.pdf", html);
        var resp = await client.PostAsync("/api/documents", form);
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Double_Extension_Exe_Is_Rejected()
    {
        var empId = await SeedEmployeeAsync(_factory, "UPLD4");
        var client = await LoginAsync("admin", "admin123");

        var exe = new byte[128];
        exe[0] = (byte)'M'; exe[1] = (byte)'Z';
        using var form = BuildForm(empId, "invoice.pdf.exe", exe);
        var resp = await client.PostAsync("/api/documents", form);
        // Last extension .exe is outside the allow-list.
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Path_Traversal_Filename_Is_Sanitized()
    {
        var empId = await SeedEmployeeAsync(_factory, "UPLD5");
        var client = await LoginAsync("admin", "admin123");

        var pdf = "%PDF-1.4 traversal"u8.ToArray();
        using var form = BuildForm(empId, "..\\..\\evil.pdf", pdf);
        var resp = await client.PostAsync("/api/documents", form);
        resp.StatusCode.Should().Be(HttpStatusCode.Created);

        var doc = await resp.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        doc.GetProperty("fileName").GetString().Should().NotContain("..");
        doc.GetProperty("filePath").GetString().Should().NotContain("..");
        doc.GetProperty("filePath").GetString().Should().NotStartWith("/");
    }

    [Fact]
    public async Task Zero_Byte_File_Is_Rejected()
    {
        var empId = await SeedEmployeeAsync(_factory, "UPLD6");
        var client = await LoginAsync("admin", "admin123");

        using var form = BuildForm(empId, "empty.pdf", Array.Empty<byte>());
        var resp = await client.PostAsync("/api/documents", form);
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Oversized_File_Is_Rejected()
    {
        var empId = await SeedEmployeeAsync(_factory, "UPLD7");
        var client = await LoginAsync("admin", "admin123");

        var big = new byte[10 * 1024 * 1024 + 1];
        "%PDF-"u8.ToArray().CopyTo(big, 0);
        using var form = BuildForm(empId, "big.pdf", big);
        var resp = await client.PostAsync("/api/documents", form);
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Svg_Is_Rejected_By_Allow_List()
    {
        var empId = await SeedEmployeeAsync(_factory, "UPLD8");
        var client = await LoginAsync("admin", "admin123");

        var svg = "<svg xmlns='http://www.w3.org/2000/svg'><script>alert(1)</script></svg>"u8.ToArray();
        using var form = BuildForm(empId, "vector.svg", svg);
        var resp = await client.PostAsync("/api/documents", form);
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Employee_Cannot_Download_Another_Employees_Document()
    {
        // Employee B owns the document; employee A must get 403 on /file.
        var empB = await SeedEmployeeAsync(_factory, "UPLDB");
        int docId;
        {
            var admin = await LoginAsync("admin", "admin123");
            using var form = BuildForm(empB, "b-file.pdf", "%PDF-1.4 b"u8.ToArray());
            var up = await admin.PostAsync("/api/documents", form);
            up.EnsureSuccessStatusCode();
            docId = (await up.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>())
                .GetProperty("documentId").GetInt32();
        }

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<LaoHRDbContext>();
            db.Users.Add(new AppUser { Username = "upld-a", PasswordHash = LaoHR.API.Services.PasswordHasher.HashPassword("pass123"), PasswordHashVersion = 2, Role = "Employee", IsActive = true });
            await db.SaveChangesAsync();
        }
        var userA = await LoginAsync("upld-a", "pass123");
        // User A has no employee link -> scope cannot view B's documents.
        var denied = await userA.GetAsync($"/api/documents/{docId}/file");
        denied.StatusCode.Should().BeOneOf(HttpStatusCode.Forbidden, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Uploaded_Files_Are_Stored_Outside_Web_Root()
    {
        var empId = await SeedEmployeeAsync(_factory, "UPLD9");
        var client = await LoginAsync("admin", "admin123");

        var pdf = "%PDF-1.4 webroot check"u8.ToArray();
        int docId;
        string storageKey;
        using (var form = BuildForm(empId, "stored.pdf", pdf))
        {
            var resp = await client.PostAsync("/api/documents", form);
            resp.EnsureSuccessStatusCode();
            var doc = await resp.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
            docId = doc.GetProperty("documentId").GetInt32();
            storageKey = doc.GetProperty("filePath").GetString()!;
        }

        storageKey.Should().StartWith("documents/").And.NotContain("/uploads/");
        // The legacy anonymous static path must NOT serve the stored file.
        var anon = _factory.CreateClient();
        var staticAttempt = await anon.GetAsync($"/{storageKey}");
        staticAttempt.StatusCode.Should().NotBe(HttpStatusCode.OK);

        await client.DeleteAsync($"/api/documents/{docId}");
    }
}

