using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LaoHR.API.Migrations
{
    /// <inheritdoc />
    public partial class AddFinanceClosure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PostingPurpose",
                table: "JournalEntries",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AccountId",
                table: "ExpenseCategories",
                type: "integer",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 24, 4, 7, 56, 300, DateTimeKind.Utc).AddTicks(7666));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 24, 4, 7, 56, 300, DateTimeKind.Utc).AddTicks(8526));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 24, 4, 7, 56, 300, DateTimeKind.Utc).AddTicks(8529));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 24, 4, 7, 56, 300, DateTimeKind.Utc).AddTicks(8530));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 24, 4, 7, 56, 301, DateTimeKind.Utc).AddTicks(1115));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 24, 4, 7, 56, 301, DateTimeKind.Utc).AddTicks(3861));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 24, 4, 7, 56, 301, DateTimeKind.Utc).AddTicks(3867));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 24, 4, 7, 56, 301, DateTimeKind.Utc).AddTicks(3871));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 24, 4, 7, 56, 301, DateTimeKind.Utc).AddTicks(3879));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 24, 4, 7, 56, 301, DateTimeKind.Utc).AddTicks(3881));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 24, 4, 7, 56, 301, DateTimeKind.Utc).AddTicks(3896));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 24, 4, 7, 56, 301, DateTimeKind.Utc).AddTicks(3898));

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "ExpenseCategoryId",
                keyValue: 1,
                column: "AccountId",
                value: null);

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "ExpenseCategoryId",
                keyValue: 2,
                column: "AccountId",
                value: null);

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "ExpenseCategoryId",
                keyValue: 3,
                column: "AccountId",
                value: null);

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "ExpenseCategoryId",
                keyValue: 4,
                column: "AccountId",
                value: null);

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "ExpenseCategoryId",
                keyValue: 5,
                column: "AccountId",
                value: null);

            migrationBuilder.UpdateData(
                table: "ExpenseCategories",
                keyColumn: "ExpenseCategoryId",
                keyValue: 6,
                column: "AccountId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 24, 4, 7, 56, 300, DateTimeKind.Utc).AddTicks(9058), new DateTime(2026, 8, 24, 4, 7, 56, 300, DateTimeKind.Utc).AddTicks(9059) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 24, 4, 7, 56, 301, DateTimeKind.Utc).AddTicks(218), new DateTime(2026, 8, 24, 4, 7, 56, 301, DateTimeKind.Utc).AddTicks(218) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 24, 4, 7, 56, 301, DateTimeKind.Utc).AddTicks(220), new DateTime(2026, 8, 24, 4, 7, 56, 301, DateTimeKind.Utc).AddTicks(220) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 24, 4, 7, 56, 301, DateTimeKind.Utc).AddTicks(222), new DateTime(2026, 8, 24, 4, 7, 56, 301, DateTimeKind.Utc).AddTicks(223) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 24, 4, 7, 56, 301, DateTimeKind.Utc).AddTicks(224), new DateTime(2026, 8, 24, 4, 7, 56, 301, DateTimeKind.Utc).AddTicks(224) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 6,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 24, 4, 7, 56, 301, DateTimeKind.Utc).AddTicks(225), new DateTime(2026, 8, 24, 4, 7, 56, 301, DateTimeKind.Utc).AddTicks(225) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 7,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 24, 4, 7, 56, 301, DateTimeKind.Utc).AddTicks(226), new DateTime(2026, 8, 24, 4, 7, 56, 301, DateTimeKind.Utc).AddTicks(227) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 8,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 24, 4, 7, 56, 301, DateTimeKind.Utc).AddTicks(228), new DateTime(2026, 8, 24, 4, 7, 56, 301, DateTimeKind.Utc).AddTicks(228) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 9,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 24, 4, 7, 56, 301, DateTimeKind.Utc).AddTicks(229), new DateTime(2026, 8, 24, 4, 7, 56, 301, DateTimeKind.Utc).AddTicks(229) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 10,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 24, 4, 7, 56, 301, DateTimeKind.Utc).AddTicks(230), new DateTime(2026, 8, 24, 4, 7, 56, 301, DateTimeKind.Utc).AddTicks(231) });

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 1,
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 4, 7, 56, 300, DateTimeKind.Utc).AddTicks(2261));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 2,
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 4, 7, 56, 300, DateTimeKind.Utc).AddTicks(3738));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 3,
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 4, 7, 56, 300, DateTimeKind.Utc).AddTicks(4079));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 4,
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 4, 7, 56, 300, DateTimeKind.Utc).AddTicks(4080));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 5,
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 4, 7, 56, 300, DateTimeKind.Utc).AddTicks(4082));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 6,
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 4, 7, 56, 300, DateTimeKind.Utc).AddTicks(4083));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "EX_RATE_THB",
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 4, 7, 56, 300, DateTimeKind.Utc).AddTicks(6811));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "EX_RATE_USD",
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 4, 7, 56, 300, DateTimeKind.Utc).AddTicks(6811));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_CEILING_BASE",
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 4, 7, 56, 300, DateTimeKind.Utc).AddTicks(6135));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_EMPLOYEE_RATE",
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 4, 7, 56, 300, DateTimeKind.Utc).AddTicks(6807));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_EMPLOYER_RATE",
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 4, 7, 56, 300, DateTimeKind.Utc).AddTicks(6808));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "WORK_END_TIME",
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 4, 7, 56, 300, DateTimeKind.Utc).AddTicks(6810));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "WORK_START_TIME",
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 4, 7, 56, 300, DateTimeKind.Utc).AddTicks(6809));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "ZKTECO_ENABLED",
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 4, 7, 56, 300, DateTimeKind.Utc).AddTicks(6812));

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseCategories_AccountId",
                table: "ExpenseCategories",
                column: "AccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExpenseCategories_Accounts_AccountId",
                table: "ExpenseCategories",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "AccountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExpenseCategories_Accounts_AccountId",
                table: "ExpenseCategories");

            migrationBuilder.DropIndex(
                name: "IX_ExpenseCategories_AccountId",
                table: "ExpenseCategories");

            migrationBuilder.DropColumn(
                name: "PostingPurpose",
                table: "JournalEntries");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "ExpenseCategories");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 24, 3, 38, 46, 318, DateTimeKind.Utc).AddTicks(906));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 24, 3, 38, 46, 318, DateTimeKind.Utc).AddTicks(1910));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 24, 3, 38, 46, 318, DateTimeKind.Utc).AddTicks(1913));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 24, 3, 38, 46, 318, DateTimeKind.Utc).AddTicks(1915));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 24, 3, 38, 46, 318, DateTimeKind.Utc).AddTicks(4694));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 24, 3, 38, 46, 318, DateTimeKind.Utc).AddTicks(7510));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 24, 3, 38, 46, 318, DateTimeKind.Utc).AddTicks(7516));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 24, 3, 38, 46, 318, DateTimeKind.Utc).AddTicks(7519));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 24, 3, 38, 46, 318, DateTimeKind.Utc).AddTicks(7522));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 24, 3, 38, 46, 318, DateTimeKind.Utc).AddTicks(7524));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 24, 3, 38, 46, 318, DateTimeKind.Utc).AddTicks(7527));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 24, 3, 38, 46, 318, DateTimeKind.Utc).AddTicks(7529));

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 24, 3, 38, 46, 318, DateTimeKind.Utc).AddTicks(2559), new DateTime(2026, 8, 24, 3, 38, 46, 318, DateTimeKind.Utc).AddTicks(2559) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 24, 3, 38, 46, 318, DateTimeKind.Utc).AddTicks(3773), new DateTime(2026, 8, 24, 3, 38, 46, 318, DateTimeKind.Utc).AddTicks(3773) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 24, 3, 38, 46, 318, DateTimeKind.Utc).AddTicks(3775), new DateTime(2026, 8, 24, 3, 38, 46, 318, DateTimeKind.Utc).AddTicks(3776) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 24, 3, 38, 46, 318, DateTimeKind.Utc).AddTicks(3777), new DateTime(2026, 8, 24, 3, 38, 46, 318, DateTimeKind.Utc).AddTicks(3777) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 24, 3, 38, 46, 318, DateTimeKind.Utc).AddTicks(3778), new DateTime(2026, 8, 24, 3, 38, 46, 318, DateTimeKind.Utc).AddTicks(3779) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 6,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 24, 3, 38, 46, 318, DateTimeKind.Utc).AddTicks(3780), new DateTime(2026, 8, 24, 3, 38, 46, 318, DateTimeKind.Utc).AddTicks(3780) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 7,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 24, 3, 38, 46, 318, DateTimeKind.Utc).AddTicks(3781), new DateTime(2026, 8, 24, 3, 38, 46, 318, DateTimeKind.Utc).AddTicks(3781) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 8,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 24, 3, 38, 46, 318, DateTimeKind.Utc).AddTicks(3783), new DateTime(2026, 8, 24, 3, 38, 46, 318, DateTimeKind.Utc).AddTicks(3783) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 9,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 24, 3, 38, 46, 318, DateTimeKind.Utc).AddTicks(3784), new DateTime(2026, 8, 24, 3, 38, 46, 318, DateTimeKind.Utc).AddTicks(3784) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 10,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 24, 3, 38, 46, 318, DateTimeKind.Utc).AddTicks(3785), new DateTime(2026, 8, 24, 3, 38, 46, 318, DateTimeKind.Utc).AddTicks(3786) });

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 1,
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 3, 38, 46, 317, DateTimeKind.Utc).AddTicks(5466));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 2,
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 3, 38, 46, 317, DateTimeKind.Utc).AddTicks(6984));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 3,
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 3, 38, 46, 317, DateTimeKind.Utc).AddTicks(7355));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 4,
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 3, 38, 46, 317, DateTimeKind.Utc).AddTicks(7356));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 5,
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 3, 38, 46, 317, DateTimeKind.Utc).AddTicks(7358));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 6,
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 3, 38, 46, 317, DateTimeKind.Utc).AddTicks(7359));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "EX_RATE_THB",
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 3, 38, 46, 318, DateTimeKind.Utc).AddTicks(74));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "EX_RATE_USD",
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 3, 38, 46, 318, DateTimeKind.Utc).AddTicks(74));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_CEILING_BASE",
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 3, 38, 46, 317, DateTimeKind.Utc).AddTicks(9377));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_EMPLOYEE_RATE",
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 3, 38, 46, 318, DateTimeKind.Utc).AddTicks(70));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_EMPLOYER_RATE",
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 3, 38, 46, 318, DateTimeKind.Utc).AddTicks(71));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "WORK_END_TIME",
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 3, 38, 46, 318, DateTimeKind.Utc).AddTicks(73));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "WORK_START_TIME",
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 3, 38, 46, 318, DateTimeKind.Utc).AddTicks(72));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "ZKTECO_ENABLED",
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 3, 38, 46, 318, DateTimeKind.Utc).AddTicks(75));
        }
    }
}
