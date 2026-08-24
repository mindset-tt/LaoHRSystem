using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using LaoHR.API.Services;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using Xunit;

namespace LaoHR.Tests.Unit.Services;

public class ApprovalServiceTests
{
    private readonly LaoHRDbContext _context;
    private readonly ApprovalService _service;

    public ApprovalServiceTests()
    {
        var options = new DbContextOptionsBuilder<LaoHRDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new LaoHRDbContext(options);
        var hierarchy = new OrganizationHierarchyService(_context);
        _service = new ApprovalService(_context, hierarchy);
    }

    private async Task SeedAsync()
    {
        _context.Employees.AddRange(
            new Employee { EmployeeId = 1, EmployeeCode = "E1", LaoName = "Manager" },
            new Employee { EmployeeId = 2, EmployeeCode = "E2", LaoName = "Employee", ManagerId = 1 });
        await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task CreateRequest_ResolvesDirectManager()
    {
        await SeedAsync();

        var request = await _service.CreateRequestAsync("LEAVE", 100, 2, new List<ApprovalStepDefinition>
        {
            new() { ResolverType = "DIRECT_MANAGER" }
        });

        request.Status.Should().Be("PENDING");
        request.Steps.Should().HaveCount(1);
        request.Steps.First().ApproverEmployeeId.Should().Be(1); // resolved to manager
    }

    [Fact]
    public async Task Approve_ByCorrectApprover_AdvancesToComplete()
    {
        await SeedAsync();

        var request = await _service.CreateRequestAsync("LEAVE", 100, 2, new List<ApprovalStepDefinition>
        {
            new() { ResolverType = "DIRECT_MANAGER" }
        });

        var result = await _service.ApproveAsync(request.ApprovalRequestId, 1, "ok");

        result.Status.Should().Be("APPROVED");
        result.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Approve_ByWrongApprover_Throws()
    {
        await SeedAsync();

        var request = await _service.CreateRequestAsync("LEAVE", 100, 2, new List<ApprovalStepDefinition>
        {
            new() { ResolverType = "DIRECT_MANAGER" }
        });

        // Employee 2 (the requester) is not the approver.
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.ApproveAsync(request.ApprovalRequestId, 2, "self-approve"));
    }

    [Fact]
    public async Task Reject_ByApprover_SetsRejected()
    {
        await SeedAsync();

        var request = await _service.CreateRequestAsync("LEAVE", 100, 2, new List<ApprovalStepDefinition>
        {
            new() { ResolverType = "DIRECT_MANAGER" }
        });

        var result = await _service.RejectAsync(request.ApprovalRequestId, 1, "denied");

        result.Status.Should().Be("REJECTED");
    }

    [Fact]
    public async Task MultiStep_SequentialApproval_AdvancesSteps()
    {
        await SeedAsync();

        var request = await _service.CreateRequestAsync("LEAVE", 100, 2, new List<ApprovalStepDefinition>
        {
            new() { ResolverType = "DIRECT_MANAGER" },   // step 0 → manager (1)
            new() { ResolverType = "EMPLOYEE", ApproverEmployeeId = 1 } // step 1 → same manager for test
        });

        // Approve step 0
        var afterStep0 = await _service.ApproveAsync(request.ApprovalRequestId, 1, "step0");
        afterStep0.Status.Should().Be("PENDING"); // still pending, advanced to step 1
        afterStep0.CurrentStepIndex.Should().Be(1);

        // Approve step 1
        var afterStep1 = await _service.ApproveAsync(request.ApprovalRequestId, 1, "step1");
        afterStep1.Status.Should().Be("APPROVED");
    }

    [Fact]
    public async Task Cancel_ByRequester_SetsCancelled()
    {
        await SeedAsync();

        var request = await _service.CreateRequestAsync("LEAVE", 100, 2, new List<ApprovalStepDefinition>
        {
            new() { ResolverType = "DIRECT_MANAGER" }
        });

        var result = await _service.CancelAsync(request.ApprovalRequestId, 2);

        result.Status.Should().Be("CANCELLED");
    }

    [Fact]
    public async Task Cancel_ByNonRequester_Throws()
    {
        await SeedAsync();

        var request = await _service.CreateRequestAsync("LEAVE", 100, 2, new List<ApprovalStepDefinition>
        {
            new() { ResolverType = "DIRECT_MANAGER" }
        });

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.CancelAsync(request.ApprovalRequestId, 1));
    }

    // ---- Phase 3C2B — approval security regression ----

    [Fact]
    public async Task Approve_AfterTerminalApproval_Throws()
    {
        await SeedAsync();

        var request = await _service.CreateRequestAsync("LEAVE", 100, 2, new List<ApprovalStepDefinition>
        {
            new() { ResolverType = "DIRECT_MANAGER" }
        });

        await _service.ApproveAsync(request.ApprovalRequestId, 1, "ok");

        // A second approval on a terminal (APPROVED) request must be rejected.
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.ApproveAsync(request.ApprovalRequestId, 1, "again"));
    }

    [Fact]
    public async Task Approve_AfterRejection_Throws()
    {
        await SeedAsync();

        var request = await _service.CreateRequestAsync("LEAVE", 100, 2, new List<ApprovalStepDefinition>
        {
            new() { ResolverType = "DIRECT_MANAGER" }
        });

        await _service.RejectAsync(request.ApprovalRequestId, 1, "denied");

        // A rejected (terminal) request cannot be approved afterwards.
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.ApproveAsync(request.ApprovalRequestId, 1, "late"));
    }

    [Fact]
    public async Task Reject_AfterTerminalApproval_Throws()
    {
        await SeedAsync();

        var request = await _service.CreateRequestAsync("LEAVE", 100, 2, new List<ApprovalStepDefinition>
        {
            new() { ResolverType = "DIRECT_MANAGER" }
        });

        await _service.ApproveAsync(request.ApprovalRequestId, 1, "ok");

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.RejectAsync(request.ApprovalRequestId, 1, "late reject"));
    }

    [Fact]
    public async Task ApproverSnapshot_ManagerChange_DoesNotAlterPendingRequest()
    {
        // Seed: employee 2 reports to manager 1.
        await SeedAsync();

        var request = await _service.CreateRequestAsync("LEAVE", 100, 2, new List<ApprovalStepDefinition>
        {
            new() { ResolverType = "DIRECT_MANAGER" }
        });

        // The resolved approver is snapshotted as manager 1.
        request.Steps.First().ApproverEmployeeId.Should().Be(1);

        // Manager changes to a new employee 3 AFTER submission.
        _context.Employees.Add(new Employee { EmployeeId = 3, EmployeeCode = "E3", LaoName = "New Manager" });
        var emp2 = await _context.Employees.FindAsync(2);
        emp2!.ManagerId = 3;
        await _context.SaveChangesAsync();

        // The pending request must still be assigned to the original manager (1).
        var reloaded = await _context.ApprovalRequests
            .Include(r => r.Steps)
            .FirstAsync(r => r.ApprovalRequestId == request.ApprovalRequestId);
        reloaded.Steps.First().ApproverEmployeeId.Should().Be(1);

        // And the original manager (1) can still approve it.
        var result = await _service.ApproveAsync(request.ApprovalRequestId, 1, "ok");
        result.Status.Should().Be("APPROVED");
    }

    [Fact]
    public async Task NewRequest_AfterManagerChange_ResolvesNewManager()
    {
        await SeedAsync();

        // Change manager to 3 before a new request.
        _context.Employees.Add(new Employee { EmployeeId = 3, EmployeeCode = "E3", LaoName = "New Manager" });
        var emp2 = await _context.Employees.FindAsync(2);
        emp2!.ManagerId = 3;
        await _context.SaveChangesAsync();

        var request = await _service.CreateRequestAsync("LEAVE", 200, 2, new List<ApprovalStepDefinition>
        {
            new() { ResolverType = "DIRECT_MANAGER" }
        });

        request.Steps.First().ApproverEmployeeId.Should().Be(3);
    }
}