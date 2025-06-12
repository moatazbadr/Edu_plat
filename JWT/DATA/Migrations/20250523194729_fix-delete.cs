using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JWT.Migrations
{
    /// <inheritdoc />
    public partial class fixdelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_userDevice_Doctors_DoctorId",
                table: "userDevice");

            migrationBuilder.DropForeignKey(
                name: "FK_userDevice_Students_StudentId",
                table: "userDevice");

            migrationBuilder.AddColumn<int>(
                name: "DoctorId1",
                table: "userDevice",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StudentId1",
                table: "userDevice",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_userDevice_DoctorId1",
                table: "userDevice",
                column: "DoctorId1");

            migrationBuilder.CreateIndex(
                name: "IX_userDevice_StudentId1",
                table: "userDevice",
                column: "StudentId1");

            migrationBuilder.AddForeignKey(
                name: "FK_userDevice_Doctors_DoctorId",
                table: "userDevice",
                column: "DoctorId",
                principalTable: "Doctors",
                principalColumn: "DoctorId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_userDevice_Doctors_DoctorId1",
                table: "userDevice",
                column: "DoctorId1",
                principalTable: "Doctors",
                principalColumn: "DoctorId");

            migrationBuilder.AddForeignKey(
                name: "FK_userDevice_Students_StudentId",
                table: "userDevice",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "StudentId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_userDevice_Students_StudentId1",
                table: "userDevice",
                column: "StudentId1",
                principalTable: "Students",
                principalColumn: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_userDevice_Doctors_DoctorId",
                table: "userDevice");

            migrationBuilder.DropForeignKey(
                name: "FK_userDevice_Doctors_DoctorId1",
                table: "userDevice");

            migrationBuilder.DropForeignKey(
                name: "FK_userDevice_Students_StudentId",
                table: "userDevice");

            migrationBuilder.DropForeignKey(
                name: "FK_userDevice_Students_StudentId1",
                table: "userDevice");

            migrationBuilder.DropIndex(
                name: "IX_userDevice_DoctorId1",
                table: "userDevice");

            migrationBuilder.DropIndex(
                name: "IX_userDevice_StudentId1",
                table: "userDevice");

            migrationBuilder.DropColumn(
                name: "DoctorId1",
                table: "userDevice");

            migrationBuilder.DropColumn(
                name: "StudentId1",
                table: "userDevice");

            migrationBuilder.AddForeignKey(
                name: "FK_userDevice_Doctors_DoctorId",
                table: "userDevice",
                column: "DoctorId",
                principalTable: "Doctors",
                principalColumn: "DoctorId");

            migrationBuilder.AddForeignKey(
                name: "FK_userDevice_Students_StudentId",
                table: "userDevice",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "StudentId");
        }
    }
}
