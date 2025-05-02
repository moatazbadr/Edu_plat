using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JWT.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedDeviceRelationwithdoctor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DoctorId",
                table: "userDevice",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_userDevice_DoctorId",
                table: "userDevice",
                column: "DoctorId");

            migrationBuilder.AddForeignKey(
                name: "FK_userDevice_Doctors_DoctorId",
                table: "userDevice",
                column: "DoctorId",
                principalTable: "Doctors",
                principalColumn: "DoctorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_userDevice_Doctors_DoctorId",
                table: "userDevice");

            migrationBuilder.DropIndex(
                name: "IX_userDevice_DoctorId",
                table: "userDevice");

            migrationBuilder.DropColumn(
                name: "DoctorId",
                table: "userDevice");
        }
    }
}
