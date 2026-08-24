using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LaoHR.API.Migrations
{
    /// <inheritdoc />
    public partial class AddBudgetEnforcementAndAssetTraceability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BudgetId",
                table: "PurchaseRequests",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BudgetId",
                table: "PurchaseOrders",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ActualAmount",
                table: "Budgets",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CommittedAmount",
                table: "Budgets",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ReservedAmount",
                table: "Budgets",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "GoodsReceiptItemId",
                table: "Assets",
                type: "integer",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 24, 3, 2, 59, 585, DateTimeKind.Utc).AddTicks(2190));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 24, 3, 2, 59, 585, DateTimeKind.Utc).AddTicks(4358));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 24, 3, 2, 59, 585, DateTimeKind.Utc).AddTicks(4376));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 24, 3, 2, 59, 585, DateTimeKind.Utc).AddTicks(4378));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 24, 3, 2, 59, 586, DateTimeKind.Utc).AddTicks(502));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 24, 3, 2, 59, 586, DateTimeKind.Utc).AddTicks(6298));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 24, 3, 2, 59, 586, DateTimeKind.Utc).AddTicks(6322));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 24, 3, 2, 59, 586, DateTimeKind.Utc).AddTicks(6327));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 24, 3, 2, 59, 586, DateTimeKind.Utc).AddTicks(6330));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 24, 3, 2, 59, 586, DateTimeKind.Utc).AddTicks(6335));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 24, 3, 2, 59, 586, DateTimeKind.Utc).AddTicks(6339));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 24, 3, 2, 59, 586, DateTimeKind.Utc).AddTicks(6342));

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 24, 3, 2, 59, 585, DateTimeKind.Utc).AddTicks(5753), new DateTime(2026, 8, 24, 3, 2, 59, 585, DateTimeKind.Utc).AddTicks(5754) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 24, 3, 2, 59, 585, DateTimeKind.Utc).AddTicks(8337), new DateTime(2026, 8, 24, 3, 2, 59, 585, DateTimeKind.Utc).AddTicks(8339) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 24, 3, 2, 59, 585, DateTimeKind.Utc).AddTicks(8342), new DateTime(2026, 8, 24, 3, 2, 59, 585, DateTimeKind.Utc).AddTicks(8342) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 24, 3, 2, 59, 585, DateTimeKind.Utc).AddTicks(8345), new DateTime(2026, 8, 24, 3, 2, 59, 585, DateTimeKind.Utc).AddTicks(8345) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 24, 3, 2, 59, 585, DateTimeKind.Utc).AddTicks(8378), new DateTime(2026, 8, 24, 3, 2, 59, 585, DateTimeKind.Utc).AddTicks(8378) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 6,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 24, 3, 2, 59, 585, DateTimeKind.Utc).AddTicks(8380), new DateTime(2026, 8, 24, 3, 2, 59, 585, DateTimeKind.Utc).AddTicks(8381) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 7,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 24, 3, 2, 59, 585, DateTimeKind.Utc).AddTicks(8382), new DateTime(2026, 8, 24, 3, 2, 59, 585, DateTimeKind.Utc).AddTicks(8383) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 8,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 24, 3, 2, 59, 585, DateTimeKind.Utc).AddTicks(8385), new DateTime(2026, 8, 24, 3, 2, 59, 585, DateTimeKind.Utc).AddTicks(8385) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 9,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 24, 3, 2, 59, 585, DateTimeKind.Utc).AddTicks(8387), new DateTime(2026, 8, 24, 3, 2, 59, 585, DateTimeKind.Utc).AddTicks(8387) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 10,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 24, 3, 2, 59, 585, DateTimeKind.Utc).AddTicks(8389), new DateTime(2026, 8, 24, 3, 2, 59, 585, DateTimeKind.Utc).AddTicks(8389) });

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 1,
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 3, 2, 59, 584, DateTimeKind.Utc).AddTicks(4185));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 2,
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 3, 2, 59, 584, DateTimeKind.Utc).AddTicks(6311));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 3,
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 3, 2, 59, 584, DateTimeKind.Utc).AddTicks(6974));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 4,
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 3, 2, 59, 584, DateTimeKind.Utc).AddTicks(6977));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 5,
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 3, 2, 59, 584, DateTimeKind.Utc).AddTicks(6978));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 6,
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 3, 2, 59, 584, DateTimeKind.Utc).AddTicks(6980));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "EX_RATE_THB",
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 3, 2, 59, 585, DateTimeKind.Utc).AddTicks(932));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "EX_RATE_USD",
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 3, 2, 59, 585, DateTimeKind.Utc).AddTicks(931));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_CEILING_BASE",
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 3, 2, 59, 585, DateTimeKind.Utc).AddTicks(83));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_EMPLOYEE_RATE",
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 3, 2, 59, 585, DateTimeKind.Utc).AddTicks(905));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_EMPLOYER_RATE",
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 3, 2, 59, 585, DateTimeKind.Utc).AddTicks(928));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "WORK_END_TIME",
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 3, 2, 59, 585, DateTimeKind.Utc).AddTicks(930));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "WORK_START_TIME",
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 3, 2, 59, 585, DateTimeKind.Utc).AddTicks(929));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "ZKTECO_ENABLED",
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 3, 2, 59, 585, DateTimeKind.Utc).AddTicks(933));

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseRequests_BudgetId",
                table: "PurchaseRequests",
                column: "BudgetId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_BudgetId",
                table: "PurchaseOrders",
                column: "BudgetId");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_GoodsReceiptItemId",
                table: "Assets",
                column: "GoodsReceiptItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_GoodsReceiptItems_GoodsReceiptItemId",
                table: "Assets",
                column: "GoodsReceiptItemId",
                principalTable: "GoodsReceiptItems",
                principalColumn: "GoodsReceiptItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrders_Budgets_BudgetId",
                table: "PurchaseOrders",
                column: "BudgetId",
                principalTable: "Budgets",
                principalColumn: "BudgetId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseRequests_Budgets_BudgetId",
                table: "PurchaseRequests",
                column: "BudgetId",
                principalTable: "Budgets",
                principalColumn: "BudgetId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assets_GoodsReceiptItems_GoodsReceiptItemId",
                table: "Assets");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrders_Budgets_BudgetId",
                table: "PurchaseOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseRequests_Budgets_BudgetId",
                table: "PurchaseRequests");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseRequests_BudgetId",
                table: "PurchaseRequests");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrders_BudgetId",
                table: "PurchaseOrders");

            migrationBuilder.DropIndex(
                name: "IX_Assets_GoodsReceiptItemId",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "BudgetId",
                table: "PurchaseRequests");

            migrationBuilder.DropColumn(
                name: "BudgetId",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "ActualAmount",
                table: "Budgets");

            migrationBuilder.DropColumn(
                name: "CommittedAmount",
                table: "Budgets");

            migrationBuilder.DropColumn(
                name: "ReservedAmount",
                table: "Budgets");

            migrationBuilder.DropColumn(
                name: "GoodsReceiptItemId",
                table: "Assets");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 24, 2, 9, 32, 797, DateTimeKind.Utc).AddTicks(1719));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 24, 2, 9, 32, 797, DateTimeKind.Utc).AddTicks(2944));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 24, 2, 9, 32, 797, DateTimeKind.Utc).AddTicks(2947));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 24, 2, 9, 32, 797, DateTimeKind.Utc).AddTicks(2948));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 24, 2, 9, 32, 797, DateTimeKind.Utc).AddTicks(6972));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 24, 2, 9, 32, 798, DateTimeKind.Utc).AddTicks(1069));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 24, 2, 9, 32, 798, DateTimeKind.Utc).AddTicks(1076));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 24, 2, 9, 32, 798, DateTimeKind.Utc).AddTicks(1080));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 24, 2, 9, 32, 798, DateTimeKind.Utc).AddTicks(1083));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 24, 2, 9, 32, 798, DateTimeKind.Utc).AddTicks(1086));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 24, 2, 9, 32, 798, DateTimeKind.Utc).AddTicks(1105));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 24, 2, 9, 32, 798, DateTimeKind.Utc).AddTicks(1108));

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 24, 2, 9, 32, 797, DateTimeKind.Utc).AddTicks(3641), new DateTime(2026, 8, 24, 2, 9, 32, 797, DateTimeKind.Utc).AddTicks(3642) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 24, 2, 9, 32, 797, DateTimeKind.Utc).AddTicks(5501), new DateTime(2026, 8, 24, 2, 9, 32, 797, DateTimeKind.Utc).AddTicks(5501) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 24, 2, 9, 32, 797, DateTimeKind.Utc).AddTicks(5504), new DateTime(2026, 8, 24, 2, 9, 32, 797, DateTimeKind.Utc).AddTicks(5505) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 24, 2, 9, 32, 797, DateTimeKind.Utc).AddTicks(5506), new DateTime(2026, 8, 24, 2, 9, 32, 797, DateTimeKind.Utc).AddTicks(5507) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 24, 2, 9, 32, 797, DateTimeKind.Utc).AddTicks(5508), new DateTime(2026, 8, 24, 2, 9, 32, 797, DateTimeKind.Utc).AddTicks(5509) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 6,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 24, 2, 9, 32, 797, DateTimeKind.Utc).AddTicks(5510), new DateTime(2026, 8, 24, 2, 9, 32, 797, DateTimeKind.Utc).AddTicks(5511) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 7,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 24, 2, 9, 32, 797, DateTimeKind.Utc).AddTicks(5512), new DateTime(2026, 8, 24, 2, 9, 32, 797, DateTimeKind.Utc).AddTicks(5512) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 8,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 24, 2, 9, 32, 797, DateTimeKind.Utc).AddTicks(5514), new DateTime(2026, 8, 24, 2, 9, 32, 797, DateTimeKind.Utc).AddTicks(5514) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 9,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 24, 2, 9, 32, 797, DateTimeKind.Utc).AddTicks(5515), new DateTime(2026, 8, 24, 2, 9, 32, 797, DateTimeKind.Utc).AddTicks(5516) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 10,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 24, 2, 9, 32, 797, DateTimeKind.Utc).AddTicks(5541), new DateTime(2026, 8, 24, 2, 9, 32, 797, DateTimeKind.Utc).AddTicks(5541) });

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 1,
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 2, 9, 32, 796, DateTimeKind.Utc).AddTicks(5121));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 2,
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 2, 9, 32, 796, DateTimeKind.Utc).AddTicks(6920));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 3,
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 2, 9, 32, 796, DateTimeKind.Utc).AddTicks(7369));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 4,
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 2, 9, 32, 796, DateTimeKind.Utc).AddTicks(7371));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 5,
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 2, 9, 32, 796, DateTimeKind.Utc).AddTicks(7373));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 6,
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 2, 9, 32, 796, DateTimeKind.Utc).AddTicks(7374));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "EX_RATE_THB",
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 2, 9, 32, 797, DateTimeKind.Utc).AddTicks(680));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "EX_RATE_USD",
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 2, 9, 32, 797, DateTimeKind.Utc).AddTicks(679));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_CEILING_BASE",
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 2, 9, 32, 796, DateTimeKind.Utc).AddTicks(9905));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_EMPLOYEE_RATE",
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 2, 9, 32, 797, DateTimeKind.Utc).AddTicks(651));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_EMPLOYER_RATE",
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 2, 9, 32, 797, DateTimeKind.Utc).AddTicks(676));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "WORK_END_TIME",
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 2, 9, 32, 797, DateTimeKind.Utc).AddTicks(678));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "WORK_START_TIME",
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 2, 9, 32, 797, DateTimeKind.Utc).AddTicks(677));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "ZKTECO_ENABLED",
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 2, 9, 32, 797, DateTimeKind.Utc).AddTicks(681));
        }
    }
}
