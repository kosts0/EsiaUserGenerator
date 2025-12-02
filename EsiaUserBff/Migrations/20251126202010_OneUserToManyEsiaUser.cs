using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EsiaUserGenerator.Migrations
{
    /// <inheritdoc />
    public partial class OneUserToManyEsiaUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RequestHistory_EsiaUsers_UserId",
                table: "RequestHistory");

            migrationBuilder.DropTable(
                name: "CreatedHistory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RequestHistory",
                table: "RequestHistory");

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatedRequestId",
                table: "EsiaUsers",
                type: "uuid",
                nullable: true,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_RequestHistory",
                table: "RequestHistory",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_EsiaUsers_CreatedRequestId",
                table: "EsiaUsers",
                column: "CreatedRequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_EsiaUsers_RequestHistory_CreatedRequestId",
                table: "EsiaUsers",
                column: "CreatedRequestId",
                principalTable: "RequestHistory",
                principalColumn: "RequestId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EsiaUsers_RequestHistory_CreatedRequestId",
                table: "EsiaUsers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RequestHistory",
                table: "RequestHistory");

            migrationBuilder.DropIndex(
                name: "IX_EsiaUsers_CreatedRequestId",
                table: "EsiaUsers");

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatedRequestId",
                table: "EsiaUsers",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RequestHistory",
                table: "RequestHistory",
                columns: new[] { "UserId", "RequestId" });

            migrationBuilder.CreateTable(
                name: "CreatedHistory",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedRequestId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreatedHistory", x => new { x.UserId, x.CreatedRequestId });
                    table.ForeignKey(
                        name: "FK_CreatedHistory_EsiaUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "EsiaUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_RequestHistory_EsiaUsers_UserId",
                table: "RequestHistory",
                column: "UserId",
                principalTable: "EsiaUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
