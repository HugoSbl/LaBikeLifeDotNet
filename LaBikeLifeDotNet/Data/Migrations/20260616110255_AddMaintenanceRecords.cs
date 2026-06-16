using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LaBikeLifeDotNet.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMaintenanceRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MaintenanceRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserMotorcycleId = table.Column<int>(type: "INTEGER", nullable: false),
                    MaintenanceTaskId = table.Column<int>(type: "INTEGER", nullable: false),
                    LastDoneKm = table.Column<int>(type: "INTEGER", nullable: false),
                    LastDoneUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaintenanceRecords_MaintenanceTasks_MaintenanceTaskId",
                        column: x => x.MaintenanceTaskId,
                        principalTable: "MaintenanceTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MaintenanceRecords_UserMotorcycles_UserMotorcycleId",
                        column: x => x.UserMotorcycleId,
                        principalTable: "UserMotorcycles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRecords_MaintenanceTaskId",
                table: "MaintenanceRecords",
                column: "MaintenanceTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRecords_UserMotorcycleId_MaintenanceTaskId",
                table: "MaintenanceRecords",
                columns: new[] { "UserMotorcycleId", "MaintenanceTaskId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MaintenanceRecords");
        }
    }
}
