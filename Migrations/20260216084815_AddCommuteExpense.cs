using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KinmuReport.Migrations
{
    /// <inheritdoc />
    public partial class AddCommuteExpense : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "通勤手当",
                columns: table => new
                {
                    社員番号 = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    対象年月 = table.Column<int>(type: "INTEGER", nullable: false),
                    日付 = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    経路NO = table.Column<int>(type: "INTEGER", nullable: true),
                    金額 = table.Column<decimal>(type: "TEXT", precision: 10, scale: 0, nullable: true),
                    備考 = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("通勤手当_pkey", x => new { x.社員番号, x.対象年月, x.日付 });
                    table.ForeignKey(
                        name: "通勤手当_社員番号_fkey",
                        column: x => x.社員番号,
                        principalTable: "社員",
                        principalColumn: "社員番号");
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "通勤手当");
        }
    }
}
