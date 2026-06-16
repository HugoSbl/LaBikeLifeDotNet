using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LaBikeLifeDotNet.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGarageAndMaintenance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MaintenanceTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    IntervalKm = table.Column<int>(type: "INTEGER", nullable: true),
                    IntervalMonths = table.Column<int>(type: "INTEGER", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceTasks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserMotorcycles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    Make = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Model = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    Vin = table.Column<string>(type: "TEXT", maxLength: 17, nullable: true),
                    DisplacementCc = table.Column<int>(type: "INTEGER", nullable: true),
                    Cylinders = table.Column<int>(type: "INTEGER", nullable: true),
                    FuelType = table.Column<string>(type: "TEXT", maxLength: 60, nullable: true),
                    BrakeSystem = table.Column<string>(type: "TEXT", maxLength: 120, nullable: true),
                    BodyClass = table.Column<string>(type: "TEXT", maxLength: 120, nullable: true),
                    CurrentMileageKm = table.Column<int>(type: "INTEGER", nullable: false),
                    MileageUpdatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMotorcycles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserMotorcycles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "MaintenanceTasks",
                columns: new[] { "Id", "Category", "IntervalKm", "IntervalMonths", "Name", "Notes" },
                values: new object[,]
                {
                    { 1, "Moteur", 6000, 12, "Vidange huile + filtre", "Plus souvent en usage intensif ou huile minérale." },
                    { 2, "Moteur", 12000, null, "Contrôle/réglage des soupapes", "Varie selon le modèle (sportives plus fréquent)." },
                    { 3, "Transmission", 1000, null, "Nettoyage/graissage de la chaîne", "Et après chaque sortie sous la pluie." },
                    { 4, "Freinage", 10000, null, "Contrôle des plaquettes de frein", "Remplacer si épaisseur < 2-3 mm." },
                    { 5, "Freinage", null, 24, "Remplacement du liquide de frein", "Tous les 2 ans, indépendamment du kilométrage." },
                    { 6, "Moteur", 24000, 24, "Liquide de refroidissement", "Motos à refroidissement liquide." },
                    { 7, "Moteur", 12000, null, "Bougies d'allumage", "Selon le type (iridium plus durable)." },
                    { 8, "Moteur", 12000, null, "Filtre à air", "Plus souvent en milieu poussiéreux." },
                    { 9, "Pneumatiques", 5000, null, "Contrôle d'usure des pneus", "Vérifier aussi la pression régulièrement." },
                    { 10, "Général", 6000, 12, "Inspection générale", "Visserie, câbles, niveaux, éclairage." }
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserMotorcycles_UserId",
                table: "UserMotorcycles",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MaintenanceTasks");

            migrationBuilder.DropTable(
                name: "UserMotorcycles");
        }
    }
}
