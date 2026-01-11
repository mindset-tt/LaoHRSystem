using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LaoHR.API.Migrations
{
    /// <inheritdoc />
    public partial class SeedExchangeRates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 10, 14, 37, 20, 386, DateTimeKind.Utc).AddTicks(3633));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 10, 14, 37, 20, 386, DateTimeKind.Utc).AddTicks(4538));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 10, 14, 37, 20, 386, DateTimeKind.Utc).AddTicks(4541));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 10, 14, 37, 20, 386, DateTimeKind.Utc).AddTicks(4542));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_CEILING_BASE",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 10, 14, 37, 20, 386, DateTimeKind.Utc).AddTicks(2091));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_EMPLOYEE_RATE",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 10, 14, 37, 20, 386, DateTimeKind.Utc).AddTicks(2769));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_EMPLOYER_RATE",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 10, 14, 37, 20, 386, DateTimeKind.Utc).AddTicks(2771));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "WORK_END_TIME",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 10, 14, 37, 20, 386, DateTimeKind.Utc).AddTicks(2773));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "WORK_START_TIME",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 10, 14, 37, 20, 386, DateTimeKind.Utc).AddTicks(2772));

            migrationBuilder.InsertData(
                table: "SystemSettings",
                columns: new[] { "SettingKey", "Description", "SettingValue", "UpdatedAt" },
                values: new object[,]
                {
                    { "EX_RATE_THB", "THB to LAK Exchange Rate", "650", new DateTime(2026, 1, 10, 14, 37, 20, 386, DateTimeKind.Utc).AddTicks(2774) },
                    { "EX_RATE_USD", "USD to LAK Exchange Rate", "22000", new DateTime(2026, 1, 10, 14, 37, 20, 386, DateTimeKind.Utc).AddTicks(2773) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "EX_RATE_THB");

            migrationBuilder.DeleteData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "EX_RATE_USD");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 10, 14, 36, 43, 275, DateTimeKind.Utc).AddTicks(3966));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 10, 14, 36, 43, 275, DateTimeKind.Utc).AddTicks(4623));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 10, 14, 36, 43, 275, DateTimeKind.Utc).AddTicks(4625));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 10, 14, 36, 43, 275, DateTimeKind.Utc).AddTicks(4626));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_CEILING_BASE",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 10, 14, 36, 43, 275, DateTimeKind.Utc).AddTicks(2847));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_EMPLOYEE_RATE",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 10, 14, 36, 43, 275, DateTimeKind.Utc).AddTicks(3407));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_EMPLOYER_RATE",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 10, 14, 36, 43, 275, DateTimeKind.Utc).AddTicks(3408));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "WORK_END_TIME",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 10, 14, 36, 43, 275, DateTimeKind.Utc).AddTicks(3410));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "WORK_START_TIME",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 10, 14, 36, 43, 275, DateTimeKind.Utc).AddTicks(3409));
        }
    }
}
