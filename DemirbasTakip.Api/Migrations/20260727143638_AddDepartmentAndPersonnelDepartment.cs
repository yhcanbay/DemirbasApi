using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DemirbasTakip.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddDepartmentAndPersonnelDepartment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Department",
                table: "Personnel");

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DepartmentName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PersonnelDepartments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PersonnelId = table.Column<int>(type: "int", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonnelDepartments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonnelDepartments_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PersonnelDepartments_Personnel_PersonnelId",
                        column: x => x.PersonnelId,
                        principalTable: "Personnel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PersonnelDepartments_DepartmentId",
                table: "PersonnelDepartments",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonnelDepartments_PersonnelId",
                table: "PersonnelDepartments",
                column: "PersonnelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PersonnelDepartments");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "Personnel",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
