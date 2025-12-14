using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EsiaUserGenerator.Migrations
{
    /// <inheritdoc />
    public partial class AddEsiaInfoGeneratedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GeneratedUserInfo",
                table: "RequestHistory",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GeneratedUserInfo",
                table: "RequestHistory");
        }
    }
}
