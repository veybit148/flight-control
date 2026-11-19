using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlightControl.Dal.Migrations
{
    /// <inheritdoc />
    public partial class AddPermitCheckResults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PermitCheckResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PermitRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    RuleName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Outcome = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermitCheckResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PermitCheckResults_FlightPermitRequests_PermitRequestId",
                        column: x => x.PermitRequestId,
                        principalTable: "FlightPermitRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PermitCheckResults_PermitRequestId",
                table: "PermitCheckResults",
                column: "PermitRequestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PermitCheckResults");
        }
    }
}
