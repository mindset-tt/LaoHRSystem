using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LaoHR.API.Migrations
{
    /// <inheritdoc />
    public partial class WorkScheduleAndHolidayUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NameEn",
                table: "Holidays",
                newName: "NameLao");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Holidays",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Holidays",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Holidays",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Holidays",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "WorkSchedules",
                columns: table => new
                {
                    WorkScheduleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Monday = table.Column<bool>(type: "bit", nullable: false),
                    Tuesday = table.Column<bool>(type: "bit", nullable: false),
                    Wednesday = table.Column<bool>(type: "bit", nullable: false),
                    Thursday = table.Column<bool>(type: "bit", nullable: false),
                    Friday = table.Column<bool>(type: "bit", nullable: false),
                    Saturday = table.Column<bool>(type: "bit", nullable: false),
                    Sunday = table.Column<bool>(type: "bit", nullable: false),
                    WorkStartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    WorkEndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    BreakStartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    BreakEndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    LateThresholdMinutes = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkSchedules", x => x.WorkScheduleId);
                });

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
                columns: new[] { "CreatedAt", "Description", "IsActive", "Name", "NameLao", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 15, 4, 13, 21, 53, DateTimeKind.Utc).AddTicks(3141), null, true, "International New Year", "ປີໃໝ່ສາກົນ", new DateTime(2026, 1, 15, 4, 13, 21, 53, DateTimeKind.Utc).AddTicks(3142) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 2,
                columns: new[] { "CreatedAt", "Description", "IsActive", "Name", "NameLao", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 15, 4, 13, 21, 53, DateTimeKind.Utc).AddTicks(3831), null, true, "International Women's Day", "ວັນແມ່ຍິງສາກົນ", new DateTime(2026, 1, 15, 4, 13, 21, 53, DateTimeKind.Utc).AddTicks(3832) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 3,
                columns: new[] { "CreatedAt", "Description", "IsActive", "Name", "NameLao", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 15, 4, 13, 21, 53, DateTimeKind.Utc).AddTicks(3834), null, true, "Lao New Year (Day 1)", "ວັນປີໃໝ່ລາວ", new DateTime(2026, 1, 15, 4, 13, 21, 53, DateTimeKind.Utc).AddTicks(3834) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 4,
                columns: new[] { "CreatedAt", "Description", "IsActive", "Name", "NameLao", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 15, 4, 13, 21, 53, DateTimeKind.Utc).AddTicks(3835), null, true, "Lao New Year (Day 2)", "ວັນປີໃໝ່ລາວ", new DateTime(2026, 1, 15, 4, 13, 21, 53, DateTimeKind.Utc).AddTicks(3835) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 5,
                columns: new[] { "CreatedAt", "Description", "IsActive", "Name", "NameLao", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 15, 4, 13, 21, 53, DateTimeKind.Utc).AddTicks(3836), null, true, "Lao New Year (Day 3)", "ວັນປີໃໝ່ລາວ", new DateTime(2026, 1, 15, 4, 13, 21, 53, DateTimeKind.Utc).AddTicks(3837) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 6,
                columns: new[] { "CreatedAt", "Description", "IsActive", "Name", "NameLao", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 15, 4, 13, 21, 53, DateTimeKind.Utc).AddTicks(3838), null, true, "International Labour Day", "ວັນກຳມະກອນສາກົນ", new DateTime(2026, 1, 15, 4, 13, 21, 53, DateTimeKind.Utc).AddTicks(3838) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 7,
                columns: new[] { "CreatedAt", "Description", "IsActive", "Name", "NameLao", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 15, 4, 13, 21, 53, DateTimeKind.Utc).AddTicks(3839), null, true, "International Children's Day", "ວັນເດັກນ້ອຍສາກົນ", new DateTime(2026, 1, 15, 4, 13, 21, 53, DateTimeKind.Utc).AddTicks(3839) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 8,
                columns: new[] { "CreatedAt", "Description", "IsActive", "Name", "NameLao", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 15, 4, 13, 21, 53, DateTimeKind.Utc).AddTicks(3840), null, true, "Lao Women's Union Day", "ວັນແມ່ຍິງລາວ", new DateTime(2026, 1, 15, 4, 13, 21, 53, DateTimeKind.Utc).AddTicks(3840) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 9,
                columns: new[] { "CreatedAt", "Description", "IsActive", "Name", "NameLao", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 15, 4, 13, 21, 53, DateTimeKind.Utc).AddTicks(3841), null, true, "National Teacher's Day", "ວັນຄູແຫ່ງຊາດ", new DateTime(2026, 1, 15, 4, 13, 21, 53, DateTimeKind.Utc).AddTicks(3842) });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 10,
                columns: new[] { "CreatedAt", "Description", "IsActive", "Name", "NameLao", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 1, 15, 4, 13, 21, 53, DateTimeKind.Utc).AddTicks(3843), null, true, "National Day", "ວັນຊາດ", new DateTime(2026, 1, 15, 4, 13, 21, 53, DateTimeKind.Utc).AddTicks(3843) });

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkSchedules");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Holidays");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Holidays");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Holidays");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Holidays");

            migrationBuilder.RenameColumn(
                name: "NameLao",
                table: "Holidays",
                newName: "NameEn");

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

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 1,
                columns: new[] { "Name", "NameEn" },
                values: new object[] { "ປີໃໝ່ສາກົນ", "International New Year" });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 2,
                columns: new[] { "Name", "NameEn" },
                values: new object[] { "ວັນແມ່ຍິງສາກົນ", "International Women's Day" });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 3,
                columns: new[] { "Name", "NameEn" },
                values: new object[] { "ວັນປີໃໝ່ລາວ", "Lao New Year (Day 1)" });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 4,
                columns: new[] { "Name", "NameEn" },
                values: new object[] { "ວັນປີໃໝ່ລາວ", "Lao New Year (Day 2)" });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 5,
                columns: new[] { "Name", "NameEn" },
                values: new object[] { "ວັນປີໃໝ່ລາວ", "Lao New Year (Day 3)" });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 6,
                columns: new[] { "Name", "NameEn" },
                values: new object[] { "ວັນກຳມະກອນສາກົນ", "International Labour Day" });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 7,
                columns: new[] { "Name", "NameEn" },
                values: new object[] { "ວັນເດັກນ້ອຍສາກົນ", "International Children's Day" });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 8,
                columns: new[] { "Name", "NameEn" },
                values: new object[] { "ວັນແມ່ຍິງລາວ", "Lao Women's Union Day" });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 9,
                columns: new[] { "Name", "NameEn" },
                values: new object[] { "ວັນຄູແຫ່ງຊາດ", "National Teacher's Day" });

            migrationBuilder.UpdateData(
                table: "Holidays",
                keyColumn: "HolidayId",
                keyValue: 10,
                columns: new[] { "Name", "NameEn" },
                values: new object[] { "ວັນຊາດ", "National Day" });

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 1,
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 14, 7, 15, 49, 460, DateTimeKind.Utc).AddTicks(2906));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 2,
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 14, 7, 15, 49, 460, DateTimeKind.Utc).AddTicks(4089));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 3,
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 14, 7, 15, 49, 460, DateTimeKind.Utc).AddTicks(4279));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 4,
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 14, 7, 15, 49, 460, DateTimeKind.Utc).AddTicks(4280));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 5,
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 14, 7, 15, 49, 460, DateTimeKind.Utc).AddTicks(4282));

            migrationBuilder.UpdateData(
                table: "LeavePolicies",
                keyColumn: "LeavePolicyId",
                keyValue: 6,
                column: "UpdatedAt",
                value: new DateTime(2026, 1, 14, 7, 15, 49, 460, DateTimeKind.Utc).AddTicks(4283));

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
        }
    }
}
