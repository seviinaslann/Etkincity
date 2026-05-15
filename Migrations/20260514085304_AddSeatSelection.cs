using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Etkincity.Migrations
{
    /// <inheritdoc />
    public partial class AddSeatSelection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SelectedSeat",
                table: "Reservations",
                type: "varchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SelectedSeat",
                table: "Reservations");
        }
    }
}
