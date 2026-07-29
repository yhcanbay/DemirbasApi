using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DemirbasTakip.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddRefreshTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PersonnelDepartments_Departments_DepartmentId",
                table: "PersonnelDepartments");

            migrationBuilder.DropForeignKey(
                name: "FK_PersonnelDepartments_Personnel_PersonnelId",
                table: "PersonnelDepartments");

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_PersonnelDepartments_Departments_DepartmentId",
                table: "PersonnelDepartments",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PersonnelDepartments_Personnel_PersonnelId",
                table: "PersonnelDepartments",
                column: "PersonnelId",
                principalTable: "Personnel",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PersonnelDepartments_Departments_DepartmentId",
                table: "PersonnelDepartments");

            migrationBuilder.DropForeignKey(
                name: "FK_PersonnelDepartments_Personnel_PersonnelId",
                table: "PersonnelDepartments");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.AddForeignKey(
                name: "FK_PersonnelDepartments_Departments_DepartmentId",
                table: "PersonnelDepartments",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PersonnelDepartments_Personnel_PersonnelId",
                table: "PersonnelDepartments",
                column: "PersonnelId",
                principalTable: "Personnel",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
