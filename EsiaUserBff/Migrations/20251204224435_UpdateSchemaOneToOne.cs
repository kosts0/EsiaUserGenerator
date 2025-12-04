using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EsiaUserGenerator.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSchemaOneToOne : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EsiaUsers_RequestHistory_CreatedRequestId",
                table: "EsiaUsers");

            migrationBuilder.DropIndex(
                name: "IX_EsiaUsers_CreatedRequestId",
                table: "EsiaUsers");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "RequestHistory");

            migrationBuilder.DropColumn(
                name: "DateTimeCreated",
                table: "EsiaUsers");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "EsiaUsers");

            migrationBuilder.AddColumn<string>(
                name: "CurrentStatus",
                table: "RequestHistory",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateTimeCreated",
                table: "RequestHistory",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "Finished",
                table: "RequestHistory",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModified",
                table: "RequestHistory",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_EsiaUsers_CreatedRequestId",
                table: "EsiaUsers",
                column: "CreatedRequestId",
                unique: true);

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

            migrationBuilder.DropIndex(
                name: "IX_EsiaUsers_CreatedRequestId",
                table: "EsiaUsers");

            migrationBuilder.DropColumn(
                name: "CurrentStatus",
                table: "RequestHistory");

            migrationBuilder.DropColumn(
                name: "DateTimeCreated",
                table: "RequestHistory");

            migrationBuilder.DropColumn(
                name: "Finished",
                table: "RequestHistory");

            migrationBuilder.DropColumn(
                name: "LastModified",
                table: "RequestHistory");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "RequestHistory",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "DateTimeCreated",
                table: "EsiaUsers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "EsiaUsers",
                type: "text",
                nullable: true);

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
    }
}
