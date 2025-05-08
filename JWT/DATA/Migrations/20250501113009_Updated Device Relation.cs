using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JWT.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedDeviceRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StudentId",
                table: "userDevice",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_userDevice_StudentId",
                table: "userDevice",
                column: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_userDevice_Students_StudentId",
                table: "userDevice",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_userDevice_Students_StudentId",
                table: "userDevice");

            migrationBuilder.DropIndex(
                name: "IX_userDevice_StudentId",
                table: "userDevice");

            migrationBuilder.DropColumn(
                name: "StudentId",
                table: "userDevice");
        }
    }
}
