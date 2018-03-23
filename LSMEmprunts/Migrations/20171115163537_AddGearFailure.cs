using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace LSMEmprunts.Migrations
{
    public partial class AddGearFailure : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Failures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CloseDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ClosingDescription = table.Column<string>(type: "TEXT", nullable: true),
                    GearId = table.Column<int>(type: "INTEGER", nullable: false),
                    OpenDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    OpeningDescription = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Failures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Failures_Gears_GearId",
                        column: x => x.GearId,
                        principalTable: "Gears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Failures_GearId",
                table: "Failures",
                column: "GearId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Failures");
        }
    }
}
