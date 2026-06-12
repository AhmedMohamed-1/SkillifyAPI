using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillifyAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddAgoraFieldsToSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AgoraChannelName",
                table: "Sessions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HangfireCloseJobId",
                table: "Sessions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HangfireOpenJobId",
                table: "Sessions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "HelperAgoraUid",
                table: "Sessions",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "RequesterAgoraUid",
                table: "Sessions",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AgoraChannelName",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "HangfireCloseJobId",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "HangfireOpenJobId",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "HelperAgoraUid",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "RequesterAgoraUid",
                table: "Sessions");
        }
    }
}
