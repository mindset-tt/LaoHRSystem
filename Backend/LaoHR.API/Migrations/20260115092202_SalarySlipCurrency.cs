using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LaoHR.API.Migrations
{
    /// <inheritdoc />
    public partial class SalarySlipCurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BaseSalaryOriginal",
                table: "SalarySlips",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ContractCurrency",
                table: "SalarySlips",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "ExchangeRateUsed",
                table: "SalarySlips",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "NetSalaryOriginal",
                table: "SalarySlips",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "PaymentCurrency",
                table: "SalarySlips",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 15, 9, 22, 1, 775, DateTimeKind.Utc).AddTicks(5507));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 15, 9, 22, 1, 775, DateTimeKind.Utc).AddTicks(6137));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 15, 9, 22, 1, 775, DateTimeKind.Utc).AddTicks(6140));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 15, 9, 22, 1, 775, DateTimeKind.Utc).AddTicks(6141));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 15, 9, 22, 1, 775, DateTimeKind.Utc).AddTicks(8050));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 15, 9, 22, 1, 776, DateTimeKind.Utc).AddTicks(211));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 15, 9, 22, 1, 776, DateTimeKind.Utc).AddTicks(227));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 15, 9, 22, 1, 776, DateTimeKind.Utc).AddTicks(230));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 15, 9, 22, 1, 776, DateTimeKind.Utc).AddTicks(239));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 15, 9, 22, 1, 776, DateTimeKind.Utc).AddTicks(241));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 15, 9, 22, 1, 776, DateTimeKind.Utc).AddTicks(329));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 15, 9, 22, 1, 776, DateTimeKind.Utc).AddTicks(332));

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 15, 9, 22, 1, 775, DateTimeKind.Utc).AddTicks(6539), new DateTime(2026, 1, 15, 9, 22, 1, 775, DateTimeKind.Utc).AddTicks(6539) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 15, 9, 22, 1, 775, DateTimeKind.Utc).AddTicks(7308), new DateTime(2026, 1, 15, 9, 22, 1, 775, DateTimeKind.Utc).AddTicks(7309) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 15, 9, 22, 1, 775, DateTimeKind.Utc).AddTicks(7311), new DateTime(2026, 1, 15, 9, 22, 1, 775, DateTimeKind.Utc).AddTicks(7311) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 15, 9, 22, 1, 775, DateTimeKind.Utc).AddTicks(7313), new DateTime(2026, 1, 15, 9, 22, 1, 775, DateTimeKind.Utc).AddTicks(7313) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 15, 9, 22, 1, 775, DateTimeKind.Utc).AddTicks(7314), new DateTime(2026, 1, 15, 9, 22, 1, 775, DateTimeKind.Utc).AddTicks(7314) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 6,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 15, 9, 22, 1, 775, DateTimeKind.Utc).AddTicks(7316), new DateTime(2026, 1, 15, 9, 22, 1, 775, DateTimeKind.Utc).AddTicks(7317) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 7,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 15, 9, 22, 1, 775, DateTimeKind.Utc).AddTicks(7318), new DateTime(2026, 1, 15, 9, 22, 1, 775, DateTimeKind.Utc).AddTicks(7318) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 8,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 15, 9, 22, 1, 775, DateTimeKind.Utc).AddTicks(7319), new DateTime(2026, 1, 15, 9, 22, 1, 775, DateTimeKind.Utc).AddTicks(7320) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 9,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 15, 9, 22, 1, 775, DateTimeKind.Utc).AddTicks(7321), new DateTime(2026, 1, 15, 9, 22, 1, 775, DateTimeKind.Utc).AddTicks(7321) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 10,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 15, 9, 22, 1, 775, DateTimeKind.Utc).AddTicks(7323), new DateTime(2026, 1, 15, 9, 22, 1, 775, DateTimeKind.Utc).AddTicks(7323) });

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 1,
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 15, 9, 22, 1, 774, DateTimeKind.Utc).AddTicks(6697));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 2,
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 15, 9, 22, 1, 774, DateTimeKind.Utc).AddTicks(7760));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 3,
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 15, 9, 22, 1, 774, DateTimeKind.Utc).AddTicks(7954));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 4,
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 15, 9, 22, 1, 774, DateTimeKind.Utc).AddTicks(7955));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 5,
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 15, 9, 22, 1, 774, DateTimeKind.Utc).AddTicks(7957));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 6,
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 15, 9, 22, 1, 774, DateTimeKind.Utc).AddTicks(7958));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "EX_RATE_THB",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 15, 9, 22, 1, 775, DateTimeKind.Utc).AddTicks(4863));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "EX_RATE_USD",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 15, 9, 22, 1, 775, DateTimeKind.Utc).AddTicks(4862));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_CEILING_BASE",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 15, 9, 22, 1, 775, DateTimeKind.Utc).AddTicks(4487));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_EMPLOYEE_RATE",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 15, 9, 22, 1, 775, DateTimeKind.Utc).AddTicks(4858));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_EMPLOYER_RATE",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 15, 9, 22, 1, 775, DateTimeKind.Utc).AddTicks(4860));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "WORK_END_TIME",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 15, 9, 22, 1, 775, DateTimeKind.Utc).AddTicks(4861));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "WORK_START_TIME",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 15, 9, 22, 1, 775, DateTimeKind.Utc).AddTicks(4861));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "ZKTECO_ENABLED",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 15, 9, 22, 1, 775, DateTimeKind.Utc).AddTicks(4864));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BaseSalaryOriginal",
                table: "SalarySlips");

            migrationBuilder.DropColumn(
                name: "ContractCurrency",
                table: "SalarySlips");

            migrationBuilder.DropColumn(
                name: "ExchangeRateUsed",
                table: "SalarySlips");

            migrationBuilder.DropColumn(
                name: "NetSalaryOriginal",
                table: "SalarySlips");

            migrationBuilder.DropColumn(
                name: "PaymentCurrency",
                table: "SalarySlips");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 15, 8, 52, 44, 270, DateTimeKind.Utc).AddTicks(8887));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 15, 8, 52, 44, 270, DateTimeKind.Utc).AddTicks(9409));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 15, 8, 52, 44, 270, DateTimeKind.Utc).AddTicks(9411));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 15, 8, 52, 44, 270, DateTimeKind.Utc).AddTicks(9412));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 15, 8, 52, 44, 271, DateTimeKind.Utc).AddTicks(1288));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 15, 8, 52, 44, 271, DateTimeKind.Utc).AddTicks(2899));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 15, 8, 52, 44, 271, DateTimeKind.Utc).AddTicks(2905));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 15, 8, 52, 44, 271, DateTimeKind.Utc).AddTicks(2908));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 15, 8, 52, 44, 271, DateTimeKind.Utc).AddTicks(2911));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 15, 8, 52, 44, 271, DateTimeKind.Utc).AddTicks(2913));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 15, 8, 52, 44, 271, DateTimeKind.Utc).AddTicks(2915));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 15, 8, 52, 44, 271, DateTimeKind.Utc).AddTicks(2923));

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 15, 8, 52, 44, 270, DateTimeKind.Utc).AddTicks(9768), new DateTime(2026, 1, 15, 8, 52, 44, 270, DateTimeKind.Utc).AddTicks(9768) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 15, 8, 52, 44, 271, DateTimeKind.Utc).AddTicks(592), new DateTime(2026, 1, 15, 8, 52, 44, 271, DateTimeKind.Utc).AddTicks(592) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 15, 8, 52, 44, 271, DateTimeKind.Utc).AddTicks(594), new DateTime(2026, 1, 15, 8, 52, 44, 271, DateTimeKind.Utc).AddTicks(595) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 15, 8, 52, 44, 271, DateTimeKind.Utc).AddTicks(596), new DateTime(2026, 1, 15, 8, 52, 44, 271, DateTimeKind.Utc).AddTicks(596) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 15, 8, 52, 44, 271, DateTimeKind.Utc).AddTicks(598), new DateTime(2026, 1, 15, 8, 52, 44, 271, DateTimeKind.Utc).AddTicks(598) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 6,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 15, 8, 52, 44, 271, DateTimeKind.Utc).AddTicks(599), new DateTime(2026, 1, 15, 8, 52, 44, 271, DateTimeKind.Utc).AddTicks(599) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 7,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 15, 8, 52, 44, 271, DateTimeKind.Utc).AddTicks(600), new DateTime(2026, 1, 15, 8, 52, 44, 271, DateTimeKind.Utc).AddTicks(601) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 8,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 15, 8, 52, 44, 271, DateTimeKind.Utc).AddTicks(602), new DateTime(2026, 1, 15, 8, 52, 44, 271, DateTimeKind.Utc).AddTicks(602) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 9,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 15, 8, 52, 44, 271, DateTimeKind.Utc).AddTicks(603), new DateTime(2026, 1, 15, 8, 52, 44, 271, DateTimeKind.Utc).AddTicks(603) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 10,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 15, 8, 52, 44, 271, DateTimeKind.Utc).AddTicks(617), new DateTime(2026, 1, 15, 8, 52, 44, 271, DateTimeKind.Utc).AddTicks(617) });

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 1,
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 15, 8, 52, 44, 270, DateTimeKind.Utc).AddTicks(1837));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 2,
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 15, 8, 52, 44, 270, DateTimeKind.Utc).AddTicks(2903));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 3,
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 15, 8, 52, 44, 270, DateTimeKind.Utc).AddTicks(3100));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 4,
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 15, 8, 52, 44, 270, DateTimeKind.Utc).AddTicks(3102));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 5,
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 15, 8, 52, 44, 270, DateTimeKind.Utc).AddTicks(3104));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 6,
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 15, 8, 52, 44, 270, DateTimeKind.Utc).AddTicks(3105));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "EX_RATE_THB",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 15, 8, 52, 44, 270, DateTimeKind.Utc).AddTicks(8412));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "EX_RATE_USD",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 15, 8, 52, 44, 270, DateTimeKind.Utc).AddTicks(8411));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_CEILING_BASE",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 15, 8, 52, 44, 270, DateTimeKind.Utc).AddTicks(8052));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_EMPLOYEE_RATE",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 15, 8, 52, 44, 270, DateTimeKind.Utc).AddTicks(8393));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_EMPLOYER_RATE",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 15, 8, 52, 44, 270, DateTimeKind.Utc).AddTicks(8408));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "WORK_END_TIME",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 15, 8, 52, 44, 270, DateTimeKind.Utc).AddTicks(8410));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "WORK_START_TIME",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 15, 8, 52, 44, 270, DateTimeKind.Utc).AddTicks(8409));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "ZKTECO_ENABLED",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 15, 8, 52, 44, 270, DateTimeKind.Utc).AddTicks(8412));
        }
    }
}
