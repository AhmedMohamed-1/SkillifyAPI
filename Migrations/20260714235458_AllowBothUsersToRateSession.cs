using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillifyAPI.Migrations
{
    /// <inheritdoc />
    public partial class AllowBothUsersToRateSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Ratings_SessionId",
                table: "Ratings");

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_SessionId_ReviewerId",
                table: "Ratings",
                columns: new[] { "SessionId", "ReviewerId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Ratings_SessionId_ReviewerId",
                table: "Ratings");

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_SessionId",
                table: "Ratings",
                column: "SessionId",
                unique: true);
        }
    }
}
