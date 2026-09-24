using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillifyAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddingGiftSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastGiftCreditAt",
                table: "Users",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastGiftCreditAt",
                table: "Users");
        }
    }
}
