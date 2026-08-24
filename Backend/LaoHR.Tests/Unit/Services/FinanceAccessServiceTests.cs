using FluentAssertions;
using LaoHR.API.Services;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace LaoHR.Tests.Unit.Services;

/// <summary>
/// Phase 4B.2 — Finance capability model regression.
///
/// Proves the role → capability mapping:
///   - Finance role → finance capabilities granted, but NOT HR/employee admin.
///   - HR role → NO finance capabilities.
///   - Employee role → NO finance capabilities.
///   - Admin role → finance capabilities (superset).
/// </summary>
public class FinanceAccessServiceTests
{
    private static FinanceAccessService CreateService(string role)
    {
        var mockHttp = new Mock<IHttpContextAccessor>();
        var identity = new System.Security.Claims.ClaimsIdentity(
            new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, role) }, "mock");
        var principal = new System.Security.Claims.ClaimsPrincipal(identity);
        mockHttp.Setup(x => x.HttpContext).Returns(new DefaultHttpContext { User = principal });
        return new FinanceAccessService(mockHttp.Object);
    }

    [Fact]
    public void FinanceRole_HasFinanceCapabilities()
    {
        var svc = CreateService("Finance");
        svc.IsFinance().Should().BeTrue();
        svc.CanViewAp().Should().BeTrue();
        svc.CanCreateAp().Should().BeTrue();
        svc.CanApproveAp().Should().BeTrue();
        svc.CanViewPayments().Should().BeTrue();
        svc.CanApprovePayments().Should().BeTrue();
        svc.CanViewAccounting().Should().BeTrue();
        svc.CanPostAccounting().Should().BeTrue();
        svc.CanViewCoa().Should().BeTrue();
        svc.CanManageCoa().Should().BeTrue();
        svc.CanManagePeriods().Should().BeTrue();
        svc.CanViewBankAccounts().Should().BeTrue();
        svc.CanManageBankAccounts().Should().BeTrue();
        svc.CanViewFinanceReports().Should().BeTrue();
        svc.CanExportFinance().Should().BeTrue();
    }

    [Fact]
    public void HrRole_HasNoFinanceCapabilities()
    {
        var svc = CreateService("HR");
        svc.IsFinance().Should().BeFalse();
        svc.CanViewAp().Should().BeFalse();
        svc.CanViewPayments().Should().BeFalse();
        svc.CanViewAccounting().Should().BeFalse();
        svc.CanExportFinance().Should().BeFalse();
        svc.CanManageBankAccounts().Should().BeFalse();
    }

    [Fact]
    public void EmployeeRole_HasNoFinanceCapabilities()
    {
        var svc = CreateService("Employee");
        svc.IsFinance().Should().BeFalse();
        svc.CanViewAp().Should().BeFalse();
        svc.CanExportFinance().Should().BeFalse();
    }

    [Fact]
    public void AdminRole_HasFinanceCapabilities()
    {
        var svc = CreateService("Admin");
        svc.IsFinance().Should().BeTrue();
        svc.CanExportFinance().Should().BeTrue();
        svc.CanManageBankAccounts().Should().BeTrue();
    }
}
