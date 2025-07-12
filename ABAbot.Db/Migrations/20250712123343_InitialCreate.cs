using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABAbot.Db.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ikigaies",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<long>(type: "INTEGER", nullable: false),
                    WhatYouLove = table.Column<string>(type: "TEXT", nullable: false),
                    WhatYouAreGoodAt = table.Column<string>(type: "TEXT", nullable: false),
                    WhatYouCanBePaidFor = table.Column<string>(type: "TEXT", nullable: false),
                    WhatTheWorldNeeds = table.Column<string>(type: "TEXT", nullable: false),
                    GptAns = table.Column<string>(type: "TEXT", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ikigaies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ikigaies_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ikigaies_UserId",
                table: "Ikigaies",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Ikigaies");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
