using System;
using EsiaUserGenerator.Db;
using EsiaUserGenerator.Db.UoW;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EsiaUserGenerator.Migrations
{
    /// <inheritdoc />
    public partial class OneUserToManyEsiaUserV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            using var db = new ApplicationDbContextFactory().CreateDbContext();

            var invalidUsers = db.EsiaUsers
                .Where(u => u.CreatedRequestId != null &&
                            !db.RequestHistory.Any(r => r.RequestId == u.CreatedRequestId))
                .ToList();

            foreach (var u in invalidUsers)
            {
                u.CreatedRequestId = null;
            }

            db.SaveChanges();
            migrationBuilder.DropForeignKey(
                name: "FK_RequestHistory_EsiaUsers_UserId",
                table: "RequestHistory");

            migrationBuilder.DropTable(
                name: "CreatedHistory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RequestHistory",
                table: "RequestHistory");

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
                principalColumn: "RequestId");
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
