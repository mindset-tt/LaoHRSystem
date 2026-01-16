using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LaoHR.API.Migrations
{
    /// <inheritdoc />
    public partial class AddPayrollAdjustments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PayrollAdjustments",
                columns: table => new
                {
                    AdjustmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    PeriodId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsTaxable = table.Column<bool>(type: "bit", nullable: false),
                    IsNssfAssessable = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollAdjustments", x => x.AdjustmentId);
                    table.ForeignKey(
                        name: "FK_PayrollAdjustments_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PayrollAdjustments_PayrollPeriods_PeriodId",
                        column: x => x.PeriodId,
                        principalTable: "PayrollPeriods",
                        principalColumn: "PeriodId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 16, 7, 30, 43, 830, DateTimeKind.Utc).AddTicks(6056));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 16, 7, 30, 43, 830, DateTimeKind.Utc).AddTicks(6719));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 16, 7, 30, 43, 830, DateTimeKind.Utc).AddTicks(6721));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 16, 7, 30, 43, 830, DateTimeKind.Utc).AddTicks(6723));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 16, 7, 30, 43, 830, DateTimeKind.Utc).AddTicks(9520));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 16, 7, 30, 43, 831, DateTimeKind.Utc).AddTicks(1617));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 16, 7, 30, 43, 831, DateTimeKind.Utc).AddTicks(1624));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 16, 7, 30, 43, 831, DateTimeKind.Utc).AddTicks(1627));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 16, 7, 30, 43, 831, DateTimeKind.Utc).AddTicks(1630));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 16, 7, 30, 43, 831, DateTimeKind.Utc).AddTicks(1634));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 16, 7, 30, 43, 831, DateTimeKind.Utc).AddTicks(1636));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 16, 7, 30, 43, 831, DateTimeKind.Utc).AddTicks(1639));

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 16, 7, 30, 43, 830, DateTimeKind.Utc).AddTicks(7568), new DateTime(2026, 1, 16, 7, 30, 43, 830, DateTimeKind.Utc).AddTicks(7569) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 16, 7, 30, 43, 830, DateTimeKind.Utc).AddTicks(8615), new DateTime(2026, 1, 16, 7, 30, 43, 830, DateTimeKind.Utc).AddTicks(8615) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 16, 7, 30, 43, 830, DateTimeKind.Utc).AddTicks(8618), new DateTime(2026, 1, 16, 7, 30, 43, 830, DateTimeKind.Utc).AddTicks(8618) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 16, 7, 30, 43, 830, DateTimeKind.Utc).AddTicks(8620), new DateTime(2026, 1, 16, 7, 30, 43, 830, DateTimeKind.Utc).AddTicks(8620) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 16, 7, 30, 43, 830, DateTimeKind.Utc).AddTicks(8622), new DateTime(2026, 1, 16, 7, 30, 43, 830, DateTimeKind.Utc).AddTicks(8622) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 6,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 16, 7, 30, 43, 830, DateTimeKind.Utc).AddTicks(8643), new DateTime(2026, 1, 16, 7, 30, 43, 830, DateTimeKind.Utc).AddTicks(8643) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 7,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 16, 7, 30, 43, 830, DateTimeKind.Utc).AddTicks(8645), new DateTime(2026, 1, 16, 7, 30, 43, 830, DateTimeKind.Utc).AddTicks(8645) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 8,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 16, 7, 30, 43, 830, DateTimeKind.Utc).AddTicks(8646), new DateTime(2026, 1, 16, 7, 30, 43, 830, DateTimeKind.Utc).AddTicks(8647) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 9,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 16, 7, 30, 43, 830, DateTimeKind.Utc).AddTicks(8648), new DateTime(2026, 1, 16, 7, 30, 43, 830, DateTimeKind.Utc).AddTicks(8648) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 10,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 16, 7, 30, 43, 830, DateTimeKind.Utc).AddTicks(8650), new DateTime(2026, 1, 16, 7, 30, 43, 830, DateTimeKind.Utc).AddTicks(8650) });

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 1,
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 16, 7, 30, 43, 829, DateTimeKind.Utc).AddTicks(6699));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 2,
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 16, 7, 30, 43, 829, DateTimeKind.Utc).AddTicks(8207));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 3,
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 16, 7, 30, 43, 829, DateTimeKind.Utc).AddTicks(8463));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 4,
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 16, 7, 30, 43, 829, DateTimeKind.Utc).AddTicks(8465));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 5,
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 16, 7, 30, 43, 829, DateTimeKind.Utc).AddTicks(8466));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 6,
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 16, 7, 30, 43, 829, DateTimeKind.Utc).AddTicks(8467));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "EX_RATE_THB",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 16, 7, 30, 43, 830, DateTimeKind.Utc).AddTicks(5366));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "EX_RATE_USD",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 16, 7, 30, 43, 830, DateTimeKind.Utc).AddTicks(5365));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_CEILING_BASE",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 16, 7, 30, 43, 830, DateTimeKind.Utc).AddTicks(4812));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_EMPLOYEE_RATE",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 16, 7, 30, 43, 830, DateTimeKind.Utc).AddTicks(5360));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_EMPLOYER_RATE",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 16, 7, 30, 43, 830, DateTimeKind.Utc).AddTicks(5362));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "WORK_END_TIME",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 16, 7, 30, 43, 830, DateTimeKind.Utc).AddTicks(5364));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "WORK_START_TIME",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 16, 7, 30, 43, 830, DateTimeKind.Utc).AddTicks(5363));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "ZKTECO_ENABLED",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 16, 7, 30, 43, 830, DateTimeKind.Utc).AddTicks(5367));

            migrationBuilder.CreateIndex(
                name: "IX_PayrollAdjustments_EmployeeId",
                table: "PayrollAdjustments",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollAdjustments_PeriodId",
                table: "PayrollAdjustments",
                column: "PeriodId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PayrollAdjustments");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 16, 7, 24, 36, 153, DateTimeKind.Utc).AddTicks(8586));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 16, 7, 24, 36, 153, DateTimeKind.Utc).AddTicks(9397));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 16, 7, 24, 36, 153, DateTimeKind.Utc).AddTicks(9400));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 16, 7, 24, 36, 153, DateTimeKind.Utc).AddTicks(9402));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 16, 7, 24, 36, 154, DateTimeKind.Utc).AddTicks(2021));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 16, 7, 24, 36, 154, DateTimeKind.Utc).AddTicks(5082));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 16, 7, 24, 36, 154, DateTimeKind.Utc).AddTicks(5090));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 16, 7, 24, 36, 154, DateTimeKind.Utc).AddTicks(5095));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 16, 7, 24, 36, 154, DateTimeKind.Utc).AddTicks(5099));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 16, 7, 24, 36, 154, DateTimeKind.Utc).AddTicks(5102));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 16, 7, 24, 36, 154, DateTimeKind.Utc).AddTicks(5106));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 16, 7, 24, 36, 154, DateTimeKind.Utc).AddTicks(5110));

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 16, 7, 24, 36, 153, DateTimeKind.Utc).AddTicks(9931), new DateTime(2026, 1, 16, 7, 24, 36, 153, DateTimeKind.Utc).AddTicks(9931) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 16, 7, 24, 36, 154, DateTimeKind.Utc).AddTicks(1042), new DateTime(2026, 1, 16, 7, 24, 36, 154, DateTimeKind.Utc).AddTicks(1042) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 16, 7, 24, 36, 154, DateTimeKind.Utc).AddTicks(1046), new DateTime(2026, 1, 16, 7, 24, 36, 154, DateTimeKind.Utc).AddTicks(1046) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 16, 7, 24, 36, 154, DateTimeKind.Utc).AddTicks(1070), new DateTime(2026, 1, 16, 7, 24, 36, 154, DateTimeKind.Utc).AddTicks(1070) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 16, 7, 24, 36, 154, DateTimeKind.Utc).AddTicks(1072), new DateTime(2026, 1, 16, 7, 24, 36, 154, DateTimeKind.Utc).AddTicks(1072) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 6,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 16, 7, 24, 36, 154, DateTimeKind.Utc).AddTicks(1074), new DateTime(2026, 1, 16, 7, 24, 36, 154, DateTimeKind.Utc).AddTicks(1074) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 7,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 16, 7, 24, 36, 154, DateTimeKind.Utc).AddTicks(1076), new DateTime(2026, 1, 16, 7, 24, 36, 154, DateTimeKind.Utc).AddTicks(1077) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 8,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 16, 7, 24, 36, 154, DateTimeKind.Utc).AddTicks(1078), new DateTime(2026, 1, 16, 7, 24, 36, 154, DateTimeKind.Utc).AddTicks(1079) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 9,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 16, 7, 24, 36, 154, DateTimeKind.Utc).AddTicks(1080), new DateTime(2026, 1, 16, 7, 24, 36, 154, DateTimeKind.Utc).AddTicks(1081) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 10,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 16, 7, 24, 36, 154, DateTimeKind.Utc).AddTicks(1082), new DateTime(2026, 1, 16, 7, 24, 36, 154, DateTimeKind.Utc).AddTicks(1083) });

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 1,
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 16, 7, 24, 36, 152, DateTimeKind.Utc).AddTicks(6192));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 2,
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 16, 7, 24, 36, 152, DateTimeKind.Utc).AddTicks(8124));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 3,
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 16, 7, 24, 36, 152, DateTimeKind.Utc).AddTicks(8442));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 4,
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 16, 7, 24, 36, 152, DateTimeKind.Utc).AddTicks(8444));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 5,
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 16, 7, 24, 36, 152, DateTimeKind.Utc).AddTicks(8447));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 6,
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 16, 7, 24, 36, 152, DateTimeKind.Utc).AddTicks(8448));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "EX_RATE_THB",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 16, 7, 24, 36, 153, DateTimeKind.Utc).AddTicks(7691));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "EX_RATE_USD",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 16, 7, 24, 36, 153, DateTimeKind.Utc).AddTicks(7689));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_CEILING_BASE",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 16, 7, 24, 36, 153, DateTimeKind.Utc).AddTicks(7104));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_EMPLOYEE_RATE",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 16, 7, 24, 36, 153, DateTimeKind.Utc).AddTicks(7683));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_EMPLOYER_RATE",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 16, 7, 24, 36, 153, DateTimeKind.Utc).AddTicks(7686));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "WORK_END_TIME",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 16, 7, 24, 36, 153, DateTimeKind.Utc).AddTicks(7688));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "WORK_START_TIME",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 16, 7, 24, 36, 153, DateTimeKind.Utc).AddTicks(7687));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "ZKTECO_ENABLED",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 16, 7, 24, 36, 153, DateTimeKind.Utc).AddTicks(7692));
        }
    }
}
