using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LaoHR.API.Migrations
{
    /// <inheritdoc />
    public partial class AddAppUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EmployeeId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_Users_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId");
                });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 19, 2, 0, 17, 85, DateTimeKind.Utc).AddTicks(9646));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 19, 2, 0, 17, 86, DateTimeKind.Utc).AddTicks(85));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 19, 2, 0, 17, 86, DateTimeKind.Utc).AddTicks(87));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 19, 2, 0, 17, 86, DateTimeKind.Utc).AddTicks(88));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 19, 2, 0, 17, 86, DateTimeKind.Utc).AddTicks(1703));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 19, 2, 0, 17, 86, DateTimeKind.Utc).AddTicks(3175));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 19, 2, 0, 17, 86, DateTimeKind.Utc).AddTicks(3180));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 19, 2, 0, 17, 86, DateTimeKind.Utc).AddTicks(3182));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 19, 2, 0, 17, 86, DateTimeKind.Utc).AddTicks(3184));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 19, 2, 0, 17, 86, DateTimeKind.Utc).AddTicks(3186));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 19, 2, 0, 17, 86, DateTimeKind.Utc).AddTicks(3188));

            migrationBuilder.UpdateData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 19, 2, 0, 17, 86, DateTimeKind.Utc).AddTicks(3190));

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 19, 2, 0, 17, 86, DateTimeKind.Utc).AddTicks(372), new DateTime(2026, 1, 19, 2, 0, 17, 86, DateTimeKind.Utc).AddTicks(372) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 19, 2, 0, 17, 86, DateTimeKind.Utc).AddTicks(980), new DateTime(2026, 1, 19, 2, 0, 17, 86, DateTimeKind.Utc).AddTicks(980) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 19, 2, 0, 17, 86, DateTimeKind.Utc).AddTicks(981), new DateTime(2026, 1, 19, 2, 0, 17, 86, DateTimeKind.Utc).AddTicks(982) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 19, 2, 0, 17, 86, DateTimeKind.Utc).AddTicks(983), new DateTime(2026, 1, 19, 2, 0, 17, 86, DateTimeKind.Utc).AddTicks(983) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 19, 2, 0, 17, 86, DateTimeKind.Utc).AddTicks(984), new DateTime(2026, 1, 19, 2, 0, 17, 86, DateTimeKind.Utc).AddTicks(984) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 6,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 19, 2, 0, 17, 86, DateTimeKind.Utc).AddTicks(985), new DateTime(2026, 1, 19, 2, 0, 17, 86, DateTimeKind.Utc).AddTicks(985) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 7,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 19, 2, 0, 17, 86, DateTimeKind.Utc).AddTicks(986), new DateTime(2026, 1, 19, 2, 0, 17, 86, DateTimeKind.Utc).AddTicks(987) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 8,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 19, 2, 0, 17, 86, DateTimeKind.Utc).AddTicks(988), new DateTime(2026, 1, 19, 2, 0, 17, 86, DateTimeKind.Utc).AddTicks(988) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 9,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 19, 2, 0, 17, 86, DateTimeKind.Utc).AddTicks(989), new DateTime(2026, 1, 19, 2, 0, 17, 86, DateTimeKind.Utc).AddTicks(989) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 10,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 19, 2, 0, 17, 86, DateTimeKind.Utc).AddTicks(990), new DateTime(2026, 1, 19, 2, 0, 17, 86, DateTimeKind.Utc).AddTicks(990) });

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 1,
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 19, 2, 0, 17, 85, DateTimeKind.Utc).AddTicks(3417));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 2,
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 19, 2, 0, 17, 85, DateTimeKind.Utc).AddTicks(4386));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 3,
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 19, 2, 0, 17, 85, DateTimeKind.Utc).AddTicks(4564));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 4,
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 19, 2, 0, 17, 85, DateTimeKind.Utc).AddTicks(4566));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 5,
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 19, 2, 0, 17, 85, DateTimeKind.Utc).AddTicks(4577));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 6,
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 19, 2, 0, 17, 85, DateTimeKind.Utc).AddTicks(4578));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "EX_RATE_THB",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 19, 2, 0, 17, 85, DateTimeKind.Utc).AddTicks(9210));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "EX_RATE_USD",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 19, 2, 0, 17, 85, DateTimeKind.Utc).AddTicks(9209));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_CEILING_BASE",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 19, 2, 0, 17, 85, DateTimeKind.Utc).AddTicks(8891));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_EMPLOYEE_RATE",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 19, 2, 0, 17, 85, DateTimeKind.Utc).AddTicks(9206));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_EMPLOYER_RATE",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 19, 2, 0, 17, 85, DateTimeKind.Utc).AddTicks(9207));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "WORK_END_TIME",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 19, 2, 0, 17, 85, DateTimeKind.Utc).AddTicks(9209));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "WORK_START_TIME",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 19, 2, 0, 17, 85, DateTimeKind.Utc).AddTicks(9208));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "ZKTECO_ENABLED",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 19, 2, 0, 17, 85, DateTimeKind.Utc).AddTicks(9211));

            migrationBuilder.CreateIndex(
                name: "IX_Users_EmployeeId",
                table: "Users",
                column: "EmployeeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");

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
        }
    }
}
