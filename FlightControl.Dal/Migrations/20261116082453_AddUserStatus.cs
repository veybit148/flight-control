using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlightControl.Dal.Migrations
{
    /// <inheritdoc />
    public partial class AddUserStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Users",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                // Existing users become Active. New rows always get an explicit status from the code.
                defaultValue: "Active");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Users");
        }
    }
}
