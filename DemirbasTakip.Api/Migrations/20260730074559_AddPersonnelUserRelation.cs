using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DemirbasTakip.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonnelUserRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Personnel",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Personnel_UserId",
                table: "Personnel",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Personnel_Users_UserId",
                table: "Personnel",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Personnel_Users_UserId",
                table: "Personnel");

            migrationBuilder.DropIndex(
                name: "IX_Personnel_UserId",
                table: "Personnel");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Personnel");
        }
    }
}
