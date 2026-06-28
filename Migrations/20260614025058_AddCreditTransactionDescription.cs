using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillifyAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddCreditTransactionDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "CreditTransactions",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "CreditTransactions");
        }
    }
}
