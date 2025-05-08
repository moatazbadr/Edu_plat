using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JWT.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedDeviceRelationwithbothstudentanddoctor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_userDevice_AspNetUsers_ApplicationUserId",
                table: "userDevice");

            migrationBuilder.DropForeignKey(
                name: "FK_userDevice_Doctors_DoctorId",
                table: "userDevice");

            migrationBuilder.DropForeignKey(
                name: "FK_userDevice_Students_StudentId",
                table: "userDevice");

            migrationBuilder.DropIndex(
                name: "IX_userDevice_ApplicationUserId",
                table: "userDevice");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "userDevice");

            migrationBuilder.AlterColumn<int>(
                name: "StudentId",
                table: "userDevice",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "DoctorId",
                table: "userDevice",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_userDevice_Doctors_DoctorId",
                table: "userDevice",
                column: "DoctorId",
                principalTable: "Doctors",
                principalColumn: "DoctorId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_userDevice_Students_StudentId",
                table: "userDevice",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "StudentId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_userDevice_Doctors_DoctorId",
                table: "userDevice");

            migrationBuilder.DropForeignKey(
                name: "FK_userDevice_Students_StudentId",
                table: "userDevice");

            migrationBuilder.AlterColumn<int>(
                name: "StudentId",
                table: "userDevice",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "DoctorId",
                table: "userDevice",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "userDevice",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_userDevice_ApplicationUserId",
                table: "userDevice",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_userDevice_AspNetUsers_ApplicationUserId",
                table: "userDevice",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

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
