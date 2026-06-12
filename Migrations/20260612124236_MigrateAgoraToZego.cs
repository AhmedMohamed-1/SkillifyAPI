using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillifyAPI.Migrations
{
    /// <inheritdoc />
    public partial class MigrateAgoraToZego : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HelperAgoraUid",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "RequesterAgoraUid",
                table: "Sessions");

            migrationBuilder.RenameColumn(
                name: "AgoraChannelName",
                table: "Sessions",
                newName: "ZegoRoomId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ZegoRoomId",
                table: "Sessions",
                newName: "AgoraChannelName");

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
    }
}
