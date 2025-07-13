using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABAbot.Db.Migrations
{
    /// <inheritdoc />
    public partial class CreateUserGptRequestLogTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserGptRequestLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UserRequest = table.Column<string>(type: "TEXT", nullable: false),
                    GptAnswer = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGptRequestLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserGptRequestLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserGptRequestLogs_UserId",
                table: "UserGptRequestLogs",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserGptRequestLogs");
        }
    }
}
