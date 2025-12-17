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
            // 1. Чистим битые ссылки CreatedRequestId
            migrationBuilder.Sql("""
                                     UPDATE "EsiaUsers" u
                                     SET "CreatedRequestId" = NULL
                                     WHERE "CreatedRequestId" IS NOT NULL
                                       AND NOT EXISTS (
                                           SELECT 1
                                           FROM "RequestHistory" r
                                           WHERE r."RequestId" = u."CreatedRequestId"
                                       );
                                 """);

            // 2. Удаляем старый FK
            migrationBuilder.DropForeignKey(
                name: "FK_RequestHistory_EsiaUsers_UserId",
                table: "RequestHistory");

            // 3. Удаляем промежуточную таблицу
            migrationBuilder.DropTable(
                name: "CreatedHistory");

            // 4. Меняем PK у RequestHistory
            migrationBuilder.DropPrimaryKey(
                name: "PK_RequestHistory",
                table: "RequestHistory");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RequestHistory",
                table: "RequestHistory",
                column: "RequestId");

            // 5. Индекс для FK
            migrationBuilder.CreateIndex(
                name: "IX_EsiaUsers_CreatedRequestId",
                table: "EsiaUsers",
                column: "CreatedRequestId");

            // 6. Новый FK
            migrationBuilder.AddForeignKey(
                name: "FK_EsiaUsers_RequestHistory_CreatedRequestId",
                table: "EsiaUsers",
                column: "CreatedRequestId",
                principalTable: "RequestHistory",
                principalColumn: "RequestId",
                onDelete: ReferentialAction.SetNull
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EsiaUsers_RequestHistory_CreatedRequestId",
                table: "EsiaUsers");

            migrationBuilder.DropIndex(
                name: "IX_EsiaUsers_CreatedRequestId",
                table: "EsiaUsers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RequestHistory",
                table: "RequestHistory");

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
