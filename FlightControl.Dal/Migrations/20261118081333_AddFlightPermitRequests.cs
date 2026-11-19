using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace FlightControl.Dal.Migrations
{
    /// <inheritdoc />
    public partial class AddFlightPermitRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FlightPermitRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RequesterId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AircraftId = table.Column<Guid>(type: "uuid", nullable: false),
                    LicenseId = table.Column<Guid>(type: "uuid", nullable: false),
                    PilotId = table.Column<Guid>(type: "uuid", nullable: false),
                    FleetId = table.Column<Guid>(type: "uuid", nullable: true),
                    Area = table.Column<Polygon>(type: "geography (Polygon, 4326)", nullable: false),
                    PlannedStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PlannedEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MaxAltitudeMeters = table.Column<double>(type: "double precision", nullable: false),
                    Purpose = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DecidedById = table.Column<Guid>(type: "uuid", nullable: true),
                    DecidedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DecisionNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlightPermitRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FlightPermitRequests_Aircraft_AircraftId",
                        column: x => x.AircraftId,
                        principalTable: "Aircraft",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FlightPermitRequests_PilotLicenses_LicenseId",
                        column: x => x.LicenseId,
                        principalTable: "PilotLicenses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FlightPermitRequests_Users_DecidedById",
                        column: x => x.DecidedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FlightPermitRequests_Users_PilotId",
                        column: x => x.PilotId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FlightPermitRequests_Users_RequesterId",
                        column: x => x.RequesterId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FlightPermitRequests_AircraftId",
                table: "FlightPermitRequests",
                column: "AircraftId");

            migrationBuilder.CreateIndex(
                name: "IX_FlightPermitRequests_Area",
                table: "FlightPermitRequests",
                column: "Area")
                .Annotation("Npgsql:IndexMethod", "gist");

            migrationBuilder.CreateIndex(
                name: "IX_FlightPermitRequests_DecidedById",
                table: "FlightPermitRequests",
                column: "DecidedById");

            migrationBuilder.CreateIndex(
                name: "IX_FlightPermitRequests_LicenseId",
                table: "FlightPermitRequests",
                column: "LicenseId");

            migrationBuilder.CreateIndex(
                name: "IX_FlightPermitRequests_PilotId",
                table: "FlightPermitRequests",
                column: "PilotId");

            migrationBuilder.CreateIndex(
                name: "IX_FlightPermitRequests_RequesterId",
                table: "FlightPermitRequests",
                column: "RequesterId");

            migrationBuilder.CreateIndex(
                name: "IX_FlightPermitRequests_Status_PlannedStart",
                table: "FlightPermitRequests",
                columns: new[] { "Status", "PlannedStart" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FlightPermitRequests");
        }
    }
}
