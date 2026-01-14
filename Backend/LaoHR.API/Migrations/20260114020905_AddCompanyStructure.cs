using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LaoHR.API.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Provinces",
                columns: table => new
                {
                    PrId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PrName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PrNameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Provinces", x => x.PrId);
                });

            migrationBuilder.CreateTable(
                name: "Districts",
                columns: table => new
                {
                    DiId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DiName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DiNameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PrId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Districts", x => x.DiId);
                    table.ForeignKey(
                        name: "FK_Districts_Provinces_PrId",
                        column: x => x.PrId,
                        principalTable: "Provinces",
                        principalColumn: "PrId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Villages",
                columns: table => new
                {
                    VillId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VillName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    VillNameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DiId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Villages", x => x.VillId);
                    table.ForeignKey(
                        name: "FK_Villages_Districts_DiId",
                        column: x => x.DiId,
                        principalTable: "Districts",
                        principalColumn: "DiId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompanySettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyNameLao = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CompanyNameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LSSOCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TaxRisId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BankAccountNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BankName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Tel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    VillageId = table.Column<int>(type: "int", nullable: true),
                    DistrictId = table.Column<int>(type: "int", nullable: true),
                    ProvinceId = table.Column<int>(type: "int", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanySettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanySettings_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "Districts",
                        principalColumn: "DiId");
                    table.ForeignKey(
                        name: "FK_CompanySettings_Provinces_ProvinceId",
                        column: x => x.ProvinceId,
                        principalTable: "Provinces",
                        principalColumn: "PrId");
                    table.ForeignKey(
                        name: "FK_CompanySettings_Villages_VillageId",
                        column: x => x.VillageId,
                        principalTable: "Villages",
                        principalColumn: "VillId");
                });

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

            migrationBuilder.InsertData(
                table: "Employees",
                columns: new[] { "EmployeeId", "BankAccount", "BankName", "BaseSalary", "CreatedAt", "DateOfBirth", "DepartmentId", "DependentCount", "Email", "EmployeeCode", "EnglishName", "Gender", "HireDate", "IsActive", "JobTitle", "LaoName", "NssfId", "Phone", "ProfilePath", "SalaryCurrency", "TaxId", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, null, null, 8000000m, new DateTime(2026, 1, 14, 2, 9, 4, 592, DateTimeKind.Utc).AddTicks(4580), null, 3, 0, "somphon@laohr.la", "EMP001", "Somphon Khamsouk", "Male", new DateTime(2024, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "Software Engineer", "ສົມພອນ ຄຳສຸກ", null, "020 5555 1234", null, "LAK", null, null },
                    { 2, null, null, 12000000m, new DateTime(2026, 1, 14, 2, 9, 4, 592, DateTimeKind.Utc).AddTicks(6293), null, 1, 0, "davanh@laohr.la", "EMP002", "Davanh Sisavath", "Female", new DateTime(2023, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "HR Manager", "ດາວັນ ສີສະຫວັດ", null, "020 5555 5678", null, "LAK", null, null },
                    { 3, null, null, 9500000m, new DateTime(2026, 1, 14, 2, 9, 4, 592, DateTimeKind.Utc).AddTicks(6300), null, 2, 0, "manivanh@laohr.la", "EMP003", "Manivanh Souksavan", "Female", new DateTime(2023, 8, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "Accountant", "ມະນີວັນ ສຸກສະຫວັນ", null, "020 5555 9012", null, "LAK", null, null },
                    { 4, null, null, 11000000m, new DateTime(2026, 1, 14, 2, 9, 4, 592, DateTimeKind.Utc).AddTicks(6303), null, 4, 0, "phouvong@laohr.la", "EMP004", "Phouvong Xaiyavong", "Male", new DateTime(2022, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "Sales Manager", "ພູວົງ ໄຊຍະວົງ", null, "020 5555 3456", null, "LAK", null, null },
                    { 5, null, null, 7500000m, new DateTime(2026, 1, 14, 2, 9, 4, 592, DateTimeKind.Utc).AddTicks(6305), null, 4, 0, "bounmi@laohr.la", "EMP005", "Bounmi Vongphachan", "Male", new DateTime(2024, 2, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Marketing Specialist", "ບຸນມີ ວົງພະຈັນ", null, "020 5555 7890", null, "LAK", null, null },
                    { 6, null, null, 6500000m, new DateTime(2026, 1, 14, 2, 9, 4, 592, DateTimeKind.Utc).AddTicks(6308), null, 1, 0, "souphaphon@laohr.la", "EMP006", "Souphaphon Nang", "Female", new DateTime(2023, 11, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "Office Administrator", "ນາງ ສຸພາພອນ", null, "020 5555 2468", null, "LAK", null, null },
                    { 7, null, null, 15000000m, new DateTime(2026, 1, 14, 2, 9, 4, 592, DateTimeKind.Utc).AddTicks(6310), null, 3, 0, "vilayphon@laohr.la", "EMP007", "Vilayphon Keomanee", "Male", new DateTime(2021, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "Senior Developer", "ວິໄລພອນ ແກ້ວມະນີ", null, "020 5555 1357", null, "LAK", null, null },
                    { 8, null, null, 25000000m, new DateTime(2026, 1, 14, 2, 9, 4, 592, DateTimeKind.Utc).AddTicks(6313), null, 2, 0, "chanthala@laohr.la", "EMP008", "Chanthala Phommavong", "Female", new DateTime(2020, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, "CFO", "ຈັນທະລາ ພົມມະວົງ", null, "020 5555 8642", null, "LAK", null, null }
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_CompanySettings_DistrictId",
                table: "CompanySettings",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanySettings_ProvinceId",
                table: "CompanySettings",
                column: "ProvinceId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanySettings_VillageId",
                table: "CompanySettings",
                column: "VillageId");

            migrationBuilder.CreateIndex(
                name: "IX_Districts_PrId",
                table: "Districts",
                column: "PrId");

            migrationBuilder.CreateIndex(
                name: "IX_Villages_DiId",
                table: "Villages",
                column: "DiId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompanySettings");

            migrationBuilder.DropTable(
                name: "Villages");

            migrationBuilder.DropTable(
                name: "Districts");

            migrationBuilder.DropTable(
                name: "Provinces");

            migrationBuilder.DeleteData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Employees",
                keyColumn: "EmployeeId",
                keyValue: 8);

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
    }
}
