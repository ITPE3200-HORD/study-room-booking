using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace study_room_booking.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceUserIdWithStudentId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "UserName",
                table: "Bookings");

            migrationBuilder.AddColumn<int>(
                name: "StudentId",
                table: "Bookings",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_StudentId",
                table: "Bookings",
                column: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Students_StudentId",
                table: "Bookings",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "StudentId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Students_StudentId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_StudentId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "StudentId",
                table: "Bookings");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Bookings",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "Bookings",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }
    }
}
