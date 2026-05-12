using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KageNoTessen.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLastPvPAttackedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastPvPAttackedAt",
                table: "Characters",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastPvPAttackedAt",
                table: "Characters");
        }
    }
}
