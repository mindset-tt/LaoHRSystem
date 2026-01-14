using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LaoHR.API.Migrations
{
    /// <inheritdoc />
    public partial class LeaveEnhancement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "TotalDays",
                table: "LeaveRequests",
                type: "decimal(5,1)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "AttachmentPath",
                table: "LeaveRequests",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HalfDayType",
                table: "LeaveRequests",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsHalfDay",
                table: "LeaveRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "LeaveBalances",
                columns: table => new
                {
                    LeaveBalanceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    LeaveType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    TotalDays = table.Column<decimal>(type: "decimal(5,1)", nullable: false),
                    UsedDays = table.Column<decimal>(type: "decimal(5,1)", nullable: false),
                    CarriedOverDays = table.Column<decimal>(type: "decimal(5,1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveBalances", x => x.LeaveBalanceId);
                    table.ForeignKey(
                        name: "FK_LeaveBalances_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LeavePolicies",
                columns: table => new
                {
                    LeavePolicyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LeaveType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    LeaveTypeLao = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AnnualQuota = table.Column<int>(type: "int", nullable: false),
                    MaxCarryOver = table.Column<int>(type: "int", nullable: false),
                    AccrualPerMonth = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    RequiresAttachment = table.Column<bool>(type: "bit", nullable: false),
                    MinDaysForAttachment = table.Column<int>(type: "int", nullable: false),
                    AllowHalfDay = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeavePolicies", x => x.LeavePolicyId);
                });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 14, 7, 15, 49, 461, DateTimeKind.Utc).AddTicks(990));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 14, 7, 15, 49, 461, DateTimeKind.Utc).AddTicks(1557));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 14, 7, 15, 49, 461, DateTimeKind.Utc).AddTicks(1558));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 14, 7, 15, 49, 461, DateTimeKind.Utc).AddTicks(1559));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 14, 7, 15, 49, 461, DateTimeKind.Utc).AddTicks(3975));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 14, 7, 15, 49, 461, DateTimeKind.Utc).AddTicks(5857));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 14, 7, 15, 49, 461, DateTimeKind.Utc).AddTicks(5862));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 14, 7, 15, 49, 461, DateTimeKind.Utc).AddTicks(5865));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 14, 7, 15, 49, 461, DateTimeKind.Utc).AddTicks(5868));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 14, 7, 15, 49, 461, DateTimeKind.Utc).AddTicks(5870));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 14, 7, 15, 49, 461, DateTimeKind.Utc).AddTicks(5872));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 14, 7, 15, 49, 461, DateTimeKind.Utc).AddTicks(5874));

            migrationBuilder.InsertData(
                table: "LeavePolicies",
                columns: new[] { "LeavePolicyId", "AccrualPerMonth", "AllowHalfDay", "AnnualQuota", "IsActive", "LeaveType", "LeaveTypeLao", "MaxCarryOver", "MinDaysForAttachment", "RequiresAttachment", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 1.25m, true, 15, true, "ANNUAL", "ພັກປະຈຳປີ", 5, 0, false, new DateTime(2026, 1, 14, 7, 15, 49, 460, DateTimeKind.Utc).AddTicks(2906) },
                    { 2, 0m, true, 30, true, "SICK", "ພັກປ່ວຍ", 0, 3, true, new DateTime(2026, 1, 14, 7, 15, 49, 460, DateTimeKind.Utc).AddTicks(4089) },
                    { 3, 0m, true, 3, true, "PERSONAL", "ພັກສ່ວນຕົວ", 0, 0, false, new DateTime(2026, 1, 14, 7, 15, 49, 460, DateTimeKind.Utc).AddTicks(4279) },
                    { 4, 0m, false, 90, true, "MATERNITY", "ພັກເກີດລູກ", 0, 0, false, new DateTime(2026, 1, 14, 7, 15, 49, 460, DateTimeKind.Utc).AddTicks(4280) },
                    { 5, 0m, false, 15, true, "PATERNITY", "ພັກພໍ່ເກີດລູກ", 0, 0, false, new DateTime(2026, 1, 14, 7, 15, 49, 460, DateTimeKind.Utc).AddTicks(4282) },
                    { 6, 0m, false, 365, true, "UNPAID", "ພັກບໍ່ໄດ້ເງິນ", 0, 0, false, new DateTime(2026, 1, 14, 7, 15, 49, 460, DateTimeKind.Utc).AddTicks(4283) }
                });

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "EX_RATE_THB",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 14, 7, 15, 49, 461, DateTimeKind.Utc).AddTicks(355));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "EX_RATE_USD",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 14, 7, 15, 49, 461, DateTimeKind.Utc).AddTicks(354));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_CEILING_BASE",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 14, 7, 15, 49, 460, DateTimeKind.Utc).AddTicks(9997));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_EMPLOYEE_RATE",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 14, 7, 15, 49, 461, DateTimeKind.Utc).AddTicks(351));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_EMPLOYER_RATE",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 14, 7, 15, 49, 461, DateTimeKind.Utc).AddTicks(352));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "WORK_END_TIME",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 14, 7, 15, 49, 461, DateTimeKind.Utc).AddTicks(354));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "WORK_START_TIME",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 14, 7, 15, 49, 461, DateTimeKind.Utc).AddTicks(353));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "ZKTECO_ENABLED",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 14, 7, 15, 49, 461, DateTimeKind.Utc).AddTicks(356));

            migrationBuilder.CreateIndex(
                name: "IX_LeaveBalances_EmployeeId_LeaveType_Year",
                table: "LeaveBalances",
                columns: new[] { "EmployeeId", "LeaveType", "Year" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeavePolicies_LeaveType",
                table: "LeavePolicies",
                column: "LeaveType",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LeaveBalances");

            migrationBuilder.DropTable(
                name: "LeavePolicies");

            migrationBuilder.DropColumn(
                name: "AttachmentPath",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "HalfDayType",
                table: "LeaveRequests");

            migrationBuilder.DropColumn(
                name: "IsHalfDay",
                table: "LeaveRequests");

            migrationBuilder.AlterColumn<int>(
                name: "TotalDays",
                table: "LeaveRequests",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,1)");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 14, 2, 9, 4, 592, DateTimeKind.Utc).AddTicks(2318));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 14, 2, 9, 4, 592, DateTimeKind.Utc).AddTicks(2862));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 14, 2, 9, 4, 592, DateTimeKind.Utc).AddTicks(2864));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 14, 2, 9, 4, 592, DateTimeKind.Utc).AddTicks(2865));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 14, 2, 9, 4, 592, DateTimeKind.Utc).AddTicks(4580));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 14, 2, 9, 4, 592, DateTimeKind.Utc).AddTicks(6293));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 14, 2, 9, 4, 592, DateTimeKind.Utc).AddTicks(6300));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 14, 2, 9, 4, 592, DateTimeKind.Utc).AddTicks(6303));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 14, 2, 9, 4, 592, DateTimeKind.Utc).AddTicks(6305));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 14, 2, 9, 4, 592, DateTimeKind.Utc).AddTicks(6308));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 14, 2, 9, 4, 592, DateTimeKind.Utc).AddTicks(6310));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 14, 2, 9, 4, 592, DateTimeKind.Utc).AddTicks(6313));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "EX_RATE_THB",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 14, 2, 9, 4, 592, DateTimeKind.Utc).AddTicks(1739));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "EX_RATE_USD",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 14, 2, 9, 4, 592, DateTimeKind.Utc).AddTicks(1738));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_CEILING_BASE",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 14, 2, 9, 4, 592, DateTimeKind.Utc).AddTicks(1243));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_EMPLOYEE_RATE",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 14, 2, 9, 4, 592, DateTimeKind.Utc).AddTicks(1734));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_EMPLOYER_RATE",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 14, 2, 9, 4, 592, DateTimeKind.Utc).AddTicks(1736));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "WORK_END_TIME",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 14, 2, 9, 4, 592, DateTimeKind.Utc).AddTicks(1737));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "WORK_START_TIME",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 14, 2, 9, 4, 592, DateTimeKind.Utc).AddTicks(1737));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "ZKTECO_ENABLED",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 14, 2, 9, 4, 592, DateTimeKind.Utc).AddTicks(1739));
        }
    }
}
