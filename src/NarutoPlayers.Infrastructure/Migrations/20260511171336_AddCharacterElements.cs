using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NarutoPlayers.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCharacterElements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Element",
                table: "Characters");

            migrationBuilder.CreateTable(
                name: "CharacterElements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CharacterId = table.Column<Guid>(type: "uuid", nullable: false),
                    Element = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    LearnedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharacterElements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CharacterElements_Characters_CharacterId",
                        column: x => x.CharacterId,
                        principalTable: "Characters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CharacterElements_CharacterId_Element",
                table: "CharacterElements",
                columns: new[] { "CharacterId", "Element" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CharacterElements");

            migrationBuilder.AddColumn<string>(
                name: "Element",
                table: "Characters",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");
        }
    }
}
