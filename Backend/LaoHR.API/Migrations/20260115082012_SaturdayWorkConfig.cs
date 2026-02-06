using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LaoHR.API.Migrations
{
    /// <inheritdoc />
    public partial class SaturdayWorkConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DailyWorkHours",
                table: "WorkSchedules",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "SaturdayEndTime",
                table: "WorkSchedules",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SaturdayHours",
                table: "WorkSchedules",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "SaturdayStartTime",
                table: "WorkSchedules",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SaturdayWeeks",
                table: "WorkSchedules",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SaturdayWorkType",
                table: "WorkSchedules",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "StandardMonthlyHours",
                table: "WorkSchedules",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 15, 8, 20, 11, 755, DateTimeKind.Utc).AddTicks(5544));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 15, 8, 20, 11, 755, DateTimeKind.Utc).AddTicks(5998));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 15, 8, 20, 11, 755, DateTimeKind.Utc).AddTicks(6000));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 15, 8, 20, 11, 755, DateTimeKind.Utc).AddTicks(6001));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 15, 8, 20, 11, 755, DateTimeKind.Utc).AddTicks(7424));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 15, 8, 20, 11, 755, DateTimeKind.Utc).AddTicks(8973));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 15, 8, 20, 11, 755, DateTimeKind.Utc).AddTicks(8979));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 15, 8, 20, 11, 755, DateTimeKind.Utc).AddTicks(8982));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 15, 8, 20, 11, 755, DateTimeKind.Utc).AddTicks(8984));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 15, 8, 20, 11, 755, DateTimeKind.Utc).AddTicks(8986));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 15, 8, 20, 11, 755, DateTimeKind.Utc).AddTicks(8988));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 15, 8, 20, 11, 755, DateTimeKind.Utc).AddTicks(8990));

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 15, 8, 20, 11, 755, DateTimeKind.Utc).AddTicks(6287), new DateTime(2026, 1, 15, 8, 20, 11, 755, DateTimeKind.Utc).AddTicks(6287) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 15, 8, 20, 11, 755, DateTimeKind.Utc).AddTicks(6899), new DateTime(2026, 1, 15, 8, 20, 11, 755, DateTimeKind.Utc).AddTicks(6900) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 15, 8, 20, 11, 755, DateTimeKind.Utc).AddTicks(6901), new DateTime(2026, 1, 15, 8, 20, 11, 755, DateTimeKind.Utc).AddTicks(6902) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 15, 8, 20, 11, 755, DateTimeKind.Utc).AddTicks(6903), new DateTime(2026, 1, 15, 8, 20, 11, 755, DateTimeKind.Utc).AddTicks(6903) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 15, 8, 20, 11, 755, DateTimeKind.Utc).AddTicks(6904), new DateTime(2026, 1, 15, 8, 20, 11, 755, DateTimeKind.Utc).AddTicks(6904) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 6,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 15, 8, 20, 11, 755, DateTimeKind.Utc).AddTicks(6905), new DateTime(2026, 1, 15, 8, 20, 11, 755, DateTimeKind.Utc).AddTicks(6905) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 7,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 15, 8, 20, 11, 755, DateTimeKind.Utc).AddTicks(6906), new DateTime(2026, 1, 15, 8, 20, 11, 755, DateTimeKind.Utc).AddTicks(6906) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 8,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 15, 8, 20, 11, 755, DateTimeKind.Utc).AddTicks(6907), new DateTime(2026, 1, 15, 8, 20, 11, 755, DateTimeKind.Utc).AddTicks(6908) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 9,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 15, 8, 20, 11, 755, DateTimeKind.Utc).AddTicks(6909), new DateTime(2026, 1, 15, 8, 20, 11, 755, DateTimeKind.Utc).AddTicks(6909) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 10,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 15, 8, 20, 11, 755, DateTimeKind.Utc).AddTicks(6920), new DateTime(2026, 1, 15, 8, 20, 11, 755, DateTimeKind.Utc).AddTicks(6920) });

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 1,
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 15, 8, 20, 11, 754, DateTimeKind.Utc).AddTicks(9384));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 2,
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 15, 8, 20, 11, 755, DateTimeKind.Utc).AddTicks(307));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 3,
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 15, 8, 20, 11, 755, DateTimeKind.Utc).AddTicks(489));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 4,
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 15, 8, 20, 11, 755, DateTimeKind.Utc).AddTicks(490));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 5,
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 15, 8, 20, 11, 755, DateTimeKind.Utc).AddTicks(491));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 6,
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 15, 8, 20, 11, 755, DateTimeKind.Utc).AddTicks(492));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "EX_RATE_THB",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 15, 8, 20, 11, 755, DateTimeKind.Utc).AddTicks(5139));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "EX_RATE_USD",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 15, 8, 20, 11, 755, DateTimeKind.Utc).AddTicks(5138));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_CEILING_BASE",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 15, 8, 20, 11, 755, DateTimeKind.Utc).AddTicks(4806));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_EMPLOYEE_RATE",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 15, 8, 20, 11, 755, DateTimeKind.Utc).AddTicks(5123));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_EMPLOYER_RATE",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 15, 8, 20, 11, 755, DateTimeKind.Utc).AddTicks(5136));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "WORK_END_TIME",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 15, 8, 20, 11, 755, DateTimeKind.Utc).AddTicks(5138));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "WORK_START_TIME",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 15, 8, 20, 11, 755, DateTimeKind.Utc).AddTicks(5137));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "ZKTECO_ENABLED",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 15, 8, 20, 11, 755, DateTimeKind.Utc).AddTicks(5140));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DailyWorkHours",
                table: "WorkSchedules");

            migrationBuilder.DropColumn(
                name: "SaturdayEndTime",
                table: "WorkSchedules");

            migrationBuilder.DropColumn(
                name: "SaturdayHours",
                table: "WorkSchedules");

            migrationBuilder.DropColumn(
                name: "SaturdayStartTime",
                table: "WorkSchedules");

            migrationBuilder.DropColumn(
                name: "SaturdayWeeks",
                table: "WorkSchedules");

            migrationBuilder.DropColumn(
                name: "SaturdayWorkType",
                table: "WorkSchedules");

            migrationBuilder.DropColumn(
                name: "StandardMonthlyHours",
                table: "WorkSchedules");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 15, 4, 13, 21, 53, DateTimeKind.Utc).AddTicks(2220));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 15, 4, 13, 21, 53, DateTimeKind.Utc).AddTicks(2753));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 15, 4, 13, 21, 53, DateTimeKind.Utc).AddTicks(2755));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 15, 4, 13, 21, 53, DateTimeKind.Utc).AddTicks(2756));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 15, 4, 13, 21, 53, DateTimeKind.Utc).AddTicks(4432));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 15, 4, 13, 21, 53, DateTimeKind.Utc).AddTicks(6062));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 15, 4, 13, 21, 53, DateTimeKind.Utc).AddTicks(6067));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 15, 4, 13, 21, 53, DateTimeKind.Utc).AddTicks(6130));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 15, 4, 13, 21, 53, DateTimeKind.Utc).AddTicks(6133));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 15, 4, 13, 21, 53, DateTimeKind.Utc).AddTicks(6135));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 15, 4, 13, 21, 53, DateTimeKind.Utc).AddTicks(6137));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 15, 4, 13, 21, 53, DateTimeKind.Utc).AddTicks(6139));

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 15, 4, 13, 21, 53, DateTimeKind.Utc).AddTicks(3141), new DateTime(2026, 1, 15, 4, 13, 21, 53, DateTimeKind.Utc).AddTicks(3142) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 15, 4, 13, 21, 53, DateTimeKind.Utc).AddTicks(3831), new DateTime(2026, 1, 15, 4, 13, 21, 53, DateTimeKind.Utc).AddTicks(3832) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 15, 4, 13, 21, 53, DateTimeKind.Utc).AddTicks(3834), new DateTime(2026, 1, 15, 4, 13, 21, 53, DateTimeKind.Utc).AddTicks(3834) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 15, 4, 13, 21, 53, DateTimeKind.Utc).AddTicks(3835), new DateTime(2026, 1, 15, 4, 13, 21, 53, DateTimeKind.Utc).AddTicks(3835) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 15, 4, 13, 21, 53, DateTimeKind.Utc).AddTicks(3836), new DateTime(2026, 1, 15, 4, 13, 21, 53, DateTimeKind.Utc).AddTicks(3837) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 6,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 15, 4, 13, 21, 53, DateTimeKind.Utc).AddTicks(3838), new DateTime(2026, 1, 15, 4, 13, 21, 53, DateTimeKind.Utc).AddTicks(3838) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 7,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 15, 4, 13, 21, 53, DateTimeKind.Utc).AddTicks(3839), new DateTime(2026, 1, 15, 4, 13, 21, 53, DateTimeKind.Utc).AddTicks(3839) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 8,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 15, 4, 13, 21, 53, DateTimeKind.Utc).AddTicks(3840), new DateTime(2026, 1, 15, 4, 13, 21, 53, DateTimeKind.Utc).AddTicks(3840) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 9,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 15, 4, 13, 21, 53, DateTimeKind.Utc).AddTicks(3841), new DateTime(2026, 1, 15, 4, 13, 21, 53, DateTimeKind.Utc).AddTicks(3842) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 10,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 15, 4, 13, 21, 53, DateTimeKind.Utc).AddTicks(3843), new DateTime(2026, 1, 15, 4, 13, 21, 53, DateTimeKind.Utc).AddTicks(3843) });

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 1,
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 15, 4, 13, 21, 52, DateTimeKind.Utc).AddTicks(4728));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 2,
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 15, 4, 13, 21, 52, DateTimeKind.Utc).AddTicks(5759));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 3,
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 15, 4, 13, 21, 52, DateTimeKind.Utc).AddTicks(5953));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 4,
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 15, 4, 13, 21, 52, DateTimeKind.Utc).AddTicks(5955));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 5,
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 15, 4, 13, 21, 52, DateTimeKind.Utc).AddTicks(5956));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 6,
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 15, 4, 13, 21, 52, DateTimeKind.Utc).AddTicks(5957));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "EX_RATE_THB",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 15, 4, 13, 21, 53, DateTimeKind.Utc).AddTicks(1692));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "EX_RATE_USD",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 15, 4, 13, 21, 53, DateTimeKind.Utc).AddTicks(1691));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_CEILING_BASE",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 15, 4, 13, 21, 53, DateTimeKind.Utc).AddTicks(1334));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_EMPLOYEE_RATE",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 15, 4, 13, 21, 53, DateTimeKind.Utc).AddTicks(1689));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_EMPLOYER_RATE",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 15, 4, 13, 21, 53, DateTimeKind.Utc).AddTicks(1689));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "WORK_END_TIME",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 15, 4, 13, 21, 53, DateTimeKind.Utc).AddTicks(1691));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "WORK_START_TIME",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 15, 4, 13, 21, 53, DateTimeKind.Utc).AddTicks(1690));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "ZKTECO_ENABLED",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 15, 4, 13, 21, 53, DateTimeKind.Utc).AddTicks(1693));
        }
    }
}
