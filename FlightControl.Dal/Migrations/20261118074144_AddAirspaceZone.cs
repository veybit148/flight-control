using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace FlightControl.Dal.Migrations
{
    /// <inheritdoc />
    public partial class AddAirspaceZone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AirspaceZones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Area = table.Column<Polygon>(type: "geography (Polygon, 4326)", nullable: false),
                    MinAltitudeMeters = table.Column<double>(type: "double precision", nullable: false),
                    MaxAltitudeMeters = table.Column<double>(type: "double precision", nullable: false),
                    ActiveFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActiveUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AirspaceZones", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AirspaceZones_Area",
                table: "AirspaceZones",
                column: "Area")
                .Annotation("Npgsql:IndexMethod", "gist");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AirspaceZones");
        }
    }
}
