using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LaBikeLifeDotNet.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPhotoAndHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MaintenanceRecords_UserMotorcycleId_MaintenanceTaskId",
                table: "MaintenanceRecords");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "UserMotorcycles",
                type: "TEXT",
                maxLength: 600,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRecords_UserMotorcycleId_MaintenanceTaskId",
                table: "MaintenanceRecords",
                columns: new[] { "UserMotorcycleId", "MaintenanceTaskId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MaintenanceRecords_UserMotorcycleId_MaintenanceTaskId",
                table: "MaintenanceRecords");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "UserMotorcycles");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRecords_UserMotorcycleId_MaintenanceTaskId",
                table: "MaintenanceRecords",
                columns: new[] { "UserMotorcycleId", "MaintenanceTaskId" },
                unique: true);
        }
    }
}
