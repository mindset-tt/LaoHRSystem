using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LaoHR.API.Migrations
{
    /// <inheritdoc />
    public partial class AddJournalSourceUniqueness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 24, 4, 53, 58, 763, DateTimeKind.Utc).AddTicks(3417));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 24, 4, 53, 58, 763, DateTimeKind.Utc).AddTicks(5185));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 24, 4, 53, 58, 763, DateTimeKind.Utc).AddTicks(5189));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 24, 4, 53, 58, 763, DateTimeKind.Utc).AddTicks(5190));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 24, 4, 53, 58, 764, DateTimeKind.Utc).AddTicks(281));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 24, 4, 53, 58, 764, DateTimeKind.Utc).AddTicks(4712));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 24, 4, 53, 58, 764, DateTimeKind.Utc).AddTicks(4721));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 24, 4, 53, 58, 764, DateTimeKind.Utc).AddTicks(4735));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 24, 4, 53, 58, 764, DateTimeKind.Utc).AddTicks(4739));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 24, 4, 53, 58, 764, DateTimeKind.Utc).AddTicks(4741));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 24, 4, 53, 58, 764, DateTimeKind.Utc).AddTicks(4744));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 8, 24, 4, 53, 58, 764, DateTimeKind.Utc).AddTicks(4746));

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 24, 4, 53, 58, 763, DateTimeKind.Utc).AddTicks(6093), new DateTime(2026, 8, 24, 4, 53, 58, 763, DateTimeKind.Utc).AddTicks(6093) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 24, 4, 53, 58, 763, DateTimeKind.Utc).AddTicks(7918), new DateTime(2026, 8, 24, 4, 53, 58, 763, DateTimeKind.Utc).AddTicks(7918) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 24, 4, 53, 58, 763, DateTimeKind.Utc).AddTicks(7920), new DateTime(2026, 8, 24, 4, 53, 58, 763, DateTimeKind.Utc).AddTicks(7921) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 24, 4, 53, 58, 763, DateTimeKind.Utc).AddTicks(7922), new DateTime(2026, 8, 24, 4, 53, 58, 763, DateTimeKind.Utc).AddTicks(7922) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 24, 4, 53, 58, 763, DateTimeKind.Utc).AddTicks(7923), new DateTime(2026, 8, 24, 4, 53, 58, 763, DateTimeKind.Utc).AddTicks(7924) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 6,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 24, 4, 53, 58, 763, DateTimeKind.Utc).AddTicks(7925), new DateTime(2026, 8, 24, 4, 53, 58, 763, DateTimeKind.Utc).AddTicks(7925) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 7,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 24, 4, 53, 58, 763, DateTimeKind.Utc).AddTicks(7926), new DateTime(2026, 8, 24, 4, 53, 58, 763, DateTimeKind.Utc).AddTicks(7927) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 8,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 24, 4, 53, 58, 763, DateTimeKind.Utc).AddTicks(7928), new DateTime(2026, 8, 24, 4, 53, 58, 763, DateTimeKind.Utc).AddTicks(7928) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 9,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 24, 4, 53, 58, 763, DateTimeKind.Utc).AddTicks(7929), new DateTime(2026, 8, 24, 4, 53, 58, 763, DateTimeKind.Utc).AddTicks(7929) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 10,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 8, 24, 4, 53, 58, 763, DateTimeKind.Utc).AddTicks(7931), new DateTime(2026, 8, 24, 4, 53, 58, 763, DateTimeKind.Utc).AddTicks(7931) });

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 1,
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 4, 53, 58, 762, DateTimeKind.Utc).AddTicks(3960));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 2,
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 4, 53, 58, 762, DateTimeKind.Utc).AddTicks(5777));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 3,
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 4, 53, 58, 762, DateTimeKind.Utc).AddTicks(6208));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 4,
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 4, 53, 58, 762, DateTimeKind.Utc).AddTicks(6210));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 5,
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 4, 53, 58, 762, DateTimeKind.Utc).AddTicks(6211));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 6,
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 4, 53, 58, 762, DateTimeKind.Utc).AddTicks(6213));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "EX_RATE_THB",
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 4, 53, 58, 763, DateTimeKind.Utc).AddTicks(1497));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "EX_RATE_USD",
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 4, 53, 58, 763, DateTimeKind.Utc).AddTicks(1497));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_CEILING_BASE",
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 4, 53, 58, 762, DateTimeKind.Utc).AddTicks(9914));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_EMPLOYEE_RATE",
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 4, 53, 58, 763, DateTimeKind.Utc).AddTicks(1459));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_EMPLOYER_RATE",
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 4, 53, 58, 763, DateTimeKind.Utc).AddTicks(1463));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "WORK_END_TIME",
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 4, 53, 58, 763, DateTimeKind.Utc).AddTicks(1465));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "WORK_START_TIME",
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 4, 53, 58, 763, DateTimeKind.Utc).AddTicks(1464));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "ZKTECO_ENABLED",
                column: "UpdatedAt",
                value: new DateTime(2026, 8, 24, 4, 53, 58, 763, DateTimeKind.Utc).AddTicks(1498));

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_SourceType_SourceId_PostingPurpose",
                table: "JournalEntries",
                columns: new[] { "SourceType", "SourceId", "PostingPurpose" },
                unique: true,
                filter: "\"SourceId\" IS NOT NULL AND \"PostingPurpose\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JournalEntries_SourceType_SourceId_PostingPurpose",
                table: "JournalEntries");

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
        }
    }
}
