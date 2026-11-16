using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlightControl.Dal.Migrations
{
    /// <inheritdoc />
    public partial class AddPilotLicenses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PilotLicenses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PilotId = table.Column<Guid>(type: "uuid", nullable: false),
                    LicenseNumber = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    SuspensionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PilotLicenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PilotLicenses_Users_PilotId",
                        column: x => x.PilotId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LicenseCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LicenseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LicenseCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LicenseCategories_PilotLicenses_LicenseId",
                        column: x => x.LicenseId,
                        principalTable: "PilotLicenses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LicenseCategories_LicenseId_Category",
                table: "LicenseCategories",
                columns: new[] { "LicenseId", "Category" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PilotLicenses_LicenseNumber",
                table: "PilotLicenses",
                column: "LicenseNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PilotLicenses_PilotId",
                table: "PilotLicenses",
                column: "PilotId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LicenseCategories");

            migrationBuilder.DropTable(
                name: "PilotLicenses");
        }
    }
}
