using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LaoHR.API.Migrations
{
    /// <inheritdoc />
    public partial class AddHolidays : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Holidays",
                columns: table => new
                {
                    HolidayId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Year = table.Column<int>(type: "int", nullable: false),
                    IsRecurring = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Holidays", x => x.HolidayId);
                });

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 10, 13, 27, 48, 893, DateTimeKind.Utc).AddTicks(7215));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 10, 13, 27, 48, 893, DateTimeKind.Utc).AddTicks(7956));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 10, 13, 27, 48, 893, DateTimeKind.Utc).AddTicks(7959));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 10, 13, 27, 48, 893, DateTimeKind.Utc).AddTicks(7960));

            migrationBuilder.InsertData(
                table: "Holidays",
                columns: new[] { "HolidayId", "Date", "IsRecurring", "Name", "NameEn", "Year" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "ປີໃໝ່ສາກົນ", "International New Year", 2026 },
                    { 2, new DateTime(2026, 3, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "ວັນແມ່ຍິງສາກົນ", "International Women's Day", 2026 },
                    { 3, new DateTime(2026, 4, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "ວັນປີໃໝ່ລາວ", "Lao New Year (Day 1)", 2026 },
                    { 4, new DateTime(2026, 4, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "ວັນປີໃໝ່ລາວ", "Lao New Year (Day 2)", 2026 },
                    { 5, new DateTime(2026, 4, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "ວັນປີໃໝ່ລາວ", "Lao New Year (Day 3)", 2026 },
                    { 6, new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "ວັນກຳມະກອນສາກົນ", "International Labour Day", 2026 },
                    { 7, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "ວັນເດັກນ້ອຍສາກົນ", "International Children's Day", 2026 },
                    { 8, new DateTime(2026, 7, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "ວັນແມ່ຍິງລາວ", "Lao Women's Union Day", 2026 },
                    { 9, new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "ວັນຄູແຫ່ງຊາດ", "National Teacher's Day", 2026 },
                    { 10, new DateTime(2026, 12, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "ວັນຊາດ", "National Day", 2026 }
                });

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_CEILING_BASE",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 10, 13, 27, 48, 893, DateTimeKind.Utc).AddTicks(5996));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_EMPLOYEE_RATE",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 10, 13, 27, 48, 893, DateTimeKind.Utc).AddTicks(6603));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_EMPLOYER_RATE",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 10, 13, 27, 48, 893, DateTimeKind.Utc).AddTicks(6604));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "WORK_END_TIME",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 10, 13, 27, 48, 893, DateTimeKind.Utc).AddTicks(6606));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "WORK_START_TIME",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 10, 13, 27, 48, 893, DateTimeKind.Utc).AddTicks(6605));

            migrationBuilder.CreateIndex(
                name: "IX_Holidays_Date",
                table: "Holidays",
                column: "Date",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Holidays");

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 10, 13, 26, 5, 974, DateTimeKind.Utc).AddTicks(7652));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 10, 13, 26, 5, 974, DateTimeKind.Utc).AddTicks(8502));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 10, 13, 26, 5, 974, DateTimeKind.Utc).AddTicks(8504));

            migrationBuilder.UpdateData(
                table: "Departments",
                keyColumn: "DepartmentId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 1, 10, 13, 26, 5, 974, DateTimeKind.Utc).AddTicks(8505));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_CEILING_BASE",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 10, 13, 26, 5, 974, DateTimeKind.Utc).AddTicks(6061));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_EMPLOYEE_RATE",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 10, 13, 26, 5, 974, DateTimeKind.Utc).AddTicks(6734));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "NSSF_EMPLOYER_RATE",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 10, 13, 26, 5, 974, DateTimeKind.Utc).AddTicks(6736));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "WORK_END_TIME",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 10, 13, 26, 5, 974, DateTimeKind.Utc).AddTicks(6739));

            migrationBuilder.UpdateData(
                table: "SystemSettings",
                keyColumn: "SettingKey",
                keyValue: "WORK_START_TIME",
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 10, 13, 26, 5, 974, DateTimeKind.Utc).AddTicks(6738));
        }
    }
}
