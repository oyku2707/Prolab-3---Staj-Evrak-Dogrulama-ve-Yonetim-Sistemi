using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StajSistemi.Migrations
{
    /// <inheritdoc />
    public partial class EnteredDataVeOpsiyonelProfilBagi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentProfiles_Users_StudentId",
                table: "StudentProfiles");

            migrationBuilder.AlterColumn<int>(
                name: "StudentId",
                table: "StudentProfiles",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "EnteredData_AcademicYear",
                table: "Documents",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "EnteredData_Address",
                table: "Documents",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "EnteredData_AdvisorApprovalFilled",
                table: "Documents",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EnteredData_BirthDate",
                table: "Documents",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EnteredData_CompanyAddress",
                table: "Documents",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "EnteredData_CompanyEmail",
                table: "Documents",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "EnteredData_CompanyFax",
                table: "Documents",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "EnteredData_CompanyName",
                table: "Documents",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "EnteredData_CompanyPhone",
                table: "Documents",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "EnteredData_CompanyWeb",
                table: "Documents",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "EnteredData_Department",
                table: "Documents",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "EnteredData_EmployeeCount",
                table: "Documents",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EnteredData_EmployerEmail",
                table: "Documents",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "EnteredData_EmployerFullName",
                table: "Documents",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "EnteredData_EmployerHasSignature",
                table: "Documents",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EnteredData_EmployerHasStamp",
                table: "Documents",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EnteredData_EmployerPhone",
                table: "Documents",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "EnteredData_EmployerTitle",
                table: "Documents",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "EnteredData_EndDate",
                table: "Documents",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EnteredData_EngineerCount",
                table: "Documents",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EnteredData_HealthAidStatus",
                table: "Documents",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EnteredData_InternshipDays",
                table: "Documents",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EnteredData_IsAbroad",
                table: "Documents",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EnteredData_IsPaid",
                table: "Documents",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EnteredData_Nationality",
                table: "Documents",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EnteredData_PreviousInternshipDays",
                table: "Documents",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EnteredData_ServiceArea",
                table: "Documents",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "EnteredData_StartDate",
                table: "Documents",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EnteredData_StudentApprovalFilled",
                table: "Documents",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EnteredData_StudentEmail",
                table: "Documents",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "EnteredData_StudentFullName",
                table: "Documents",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "EnteredData_StudentNo",
                table: "Documents",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "EnteredData_StudentPhone",
                table: "Documents",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "EnteredData_TcNo",
                table: "Documents",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "EnteredData_WorksOnWeekends",
                table: "Documents",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentProfiles_Users_StudentId",
                table: "StudentProfiles",
                column: "StudentId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentProfiles_Users_StudentId",
                table: "StudentProfiles");

            migrationBuilder.DropColumn(
                name: "EnteredData_AcademicYear",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "EnteredData_Address",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "EnteredData_AdvisorApprovalFilled",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "EnteredData_BirthDate",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "EnteredData_CompanyAddress",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "EnteredData_CompanyEmail",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "EnteredData_CompanyFax",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "EnteredData_CompanyName",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "EnteredData_CompanyPhone",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "EnteredData_CompanyWeb",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "EnteredData_Department",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "EnteredData_EmployeeCount",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "EnteredData_EmployerEmail",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "EnteredData_EmployerFullName",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "EnteredData_EmployerHasSignature",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "EnteredData_EmployerHasStamp",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "EnteredData_EmployerPhone",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "EnteredData_EmployerTitle",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "EnteredData_EndDate",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "EnteredData_EngineerCount",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "EnteredData_HealthAidStatus",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "EnteredData_InternshipDays",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "EnteredData_IsAbroad",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "EnteredData_IsPaid",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "EnteredData_Nationality",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "EnteredData_PreviousInternshipDays",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "EnteredData_ServiceArea",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "EnteredData_StartDate",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "EnteredData_StudentApprovalFilled",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "EnteredData_StudentEmail",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "EnteredData_StudentFullName",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "EnteredData_StudentNo",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "EnteredData_StudentPhone",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "EnteredData_TcNo",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "EnteredData_WorksOnWeekends",
                table: "Documents");

            migrationBuilder.AlterColumn<int>(
                name: "StudentId",
                table: "StudentProfiles",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentProfiles_Users_StudentId",
                table: "StudentProfiles",
                column: "StudentId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
