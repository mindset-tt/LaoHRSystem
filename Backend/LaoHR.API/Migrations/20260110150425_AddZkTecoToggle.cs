using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LaoHR.API.Migrations
{
    /// <inheritdoc />
    public partial class AddZkTecoToggle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 10, 15, 4, 24, 592, DateTimeKind.Utc).AddTicks(2415));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 10, 15, 4, 24, 592, DateTimeKind.Utc).AddTicks(3454));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 10, 15, 4, 24, 592, DateTimeKind.Utc).AddTicks(3457));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 10, 15, 4, 24, 592, DateTimeKind.Utc).AddTicks(3459));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "EX_RATE_THB",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 10, 15, 4, 24, 592, DateTimeKind.Utc).AddTicks(1718));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "EX_RATE_USD",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 10, 15, 4, 24, 592, DateTimeKind.Utc).AddTicks(1717));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_CEILING_BASE",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 10, 15, 4, 24, 592, DateTimeKind.Utc).AddTicks(1118));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_EMPLOYEE_RATE",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 10, 15, 4, 24, 592, DateTimeKind.Utc).AddTicks(1713));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_EMPLOYER_RATE",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 10, 15, 4, 24, 592, DateTimeKind.Utc).AddTicks(1714));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "WORK_END_TIME",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 10, 15, 4, 24, 592, DateTimeKind.Utc).AddTicks(1716));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "WORK_START_TIME",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 10, 15, 4, 24, 592, DateTimeKind.Utc).AddTicks(1715));

            migrationBuilder.InsertData(
                table: "SystemSettings",
                columns: new[] { "SettingKey", "Description", "SettingValue", "UpdatedAt" },
                values: new object[] { "ZKTECO_ENABLED", "Global Switch for ZKTeco Integration", "false", new DateTime(2026, 1, 10, 15, 4, 24, 592, DateTimeKind.Utc).AddTicks(1719) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "ZKTECO_ENABLED");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 10, 14, 47, 14, 366, DateTimeKind.Utc).AddTicks(7070));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 10, 14, 47, 14, 366, DateTimeKind.Utc).AddTicks(7835));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 10, 14, 47, 14, 366, DateTimeKind.Utc).AddTicks(7837));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 10, 14, 47, 14, 366, DateTimeKind.Utc).AddTicks(7839));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "EX_RATE_THB",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 10, 14, 47, 14, 366, DateTimeKind.Utc).AddTicks(6434));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "EX_RATE_USD",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 10, 14, 47, 14, 366, DateTimeKind.Utc).AddTicks(6433));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_CEILING_BASE",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 10, 14, 47, 14, 366, DateTimeKind.Utc).AddTicks(5840));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_EMPLOYEE_RATE",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 10, 14, 47, 14, 366, DateTimeKind.Utc).AddTicks(6428));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_EMPLOYER_RATE",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 10, 14, 47, 14, 366, DateTimeKind.Utc).AddTicks(6430));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "WORK_END_TIME",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 10, 14, 47, 14, 366, DateTimeKind.Utc).AddTicks(6432));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "WORK_START_TIME",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 10, 14, 47, 14, 366, DateTimeKind.Utc).AddTicks(6431));
        }
    }
}
