using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LaoHR.API.Migrations
{
    /// <inheritdoc />
    public partial class IncreaseSystemSettingLen : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 11, 2, 20, 21, 607, DateTimeKind.Utc).AddTicks(6297));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 11, 2, 20, 21, 607, DateTimeKind.Utc).AddTicks(7008));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 11, 2, 20, 21, 607, DateTimeKind.Utc).AddTicks(7011));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 11, 2, 20, 21, 607, DateTimeKind.Utc).AddTicks(7012));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "EX_RATE_THB",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 11, 2, 20, 21, 607, DateTimeKind.Utc).AddTicks(5612));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "EX_RATE_USD",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 11, 2, 20, 21, 607, DateTimeKind.Utc).AddTicks(5611));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_CEILING_BASE",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 11, 2, 20, 21, 607, DateTimeKind.Utc).AddTicks(5053));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_EMPLOYEE_RATE",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 11, 2, 20, 21, 607, DateTimeKind.Utc).AddTicks(5606));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_EMPLOYER_RATE",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 11, 2, 20, 21, 607, DateTimeKind.Utc).AddTicks(5608));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "WORK_END_TIME",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 11, 2, 20, 21, 607, DateTimeKind.Utc).AddTicks(5610));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "WORK_START_TIME",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 11, 2, 20, 21, 607, DateTimeKind.Utc).AddTicks(5609));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "ZKTECO_ENABLED",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 11, 2, 20, 21, 607, DateTimeKind.Utc).AddTicks(5613));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 11, 2, 19, 57, 452, DateTimeKind.Utc).AddTicks(6100));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 11, 2, 19, 57, 452, DateTimeKind.Utc).AddTicks(7103));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 11, 2, 19, 57, 452, DateTimeKind.Utc).AddTicks(7106));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 11, 2, 19, 57, 452, DateTimeKind.Utc).AddTicks(7107));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "EX_RATE_THB",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 11, 2, 19, 57, 452, DateTimeKind.Utc).AddTicks(5462));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "EX_RATE_USD",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 11, 2, 19, 57, 452, DateTimeKind.Utc).AddTicks(5461));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_CEILING_BASE",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 11, 2, 19, 57, 452, DateTimeKind.Utc).AddTicks(4887));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_EMPLOYEE_RATE",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 11, 2, 19, 57, 452, DateTimeKind.Utc).AddTicks(5439));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_EMPLOYER_RATE",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 11, 2, 19, 57, 452, DateTimeKind.Utc).AddTicks(5441));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "WORK_END_TIME",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 11, 2, 19, 57, 452, DateTimeKind.Utc).AddTicks(5460));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "WORK_START_TIME",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 11, 2, 19, 57, 452, DateTimeKind.Utc).AddTicks(5459));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "ZKTECO_ENABLED",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 11, 2, 19, 57, 452, DateTimeKind.Utc).AddTicks(5463));
        }
    }
}
