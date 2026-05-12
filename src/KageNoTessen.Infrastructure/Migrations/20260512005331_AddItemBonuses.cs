using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KageNoTessen.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddItemBonuses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AgilityBonus",
                table: "Items",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AttackBonus",
                table: "Items",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ChakraBonus",
                table: "Items",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DefenseBonus",
                table: "Items",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "IntelligenceBonus",
                table: "Items",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LuckBonus",
                table: "Items",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "VitalityBonus",
                table: "Items",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AgilityBonus",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "AttackBonus",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "ChakraBonus",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "DefenseBonus",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "IntelligenceBonus",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "LuckBonus",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "VitalityBonus",
                table: "Items");
        }
    }
}
