using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KinmuReport.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "グループ",
                columns: table => new
                {
                    グループコード = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    グループ名 = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("グループ_pkey", x => x.グループコード);
                });

            migrationBuilder.CreateTable(
                name: "社員",
                columns: table => new
                {
                    社員番号 = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    社員名 = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ログインid = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    パスワードハッシュ = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    adオブジェクトid = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    グループコード = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    権限 = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("社員_pkey", x => x.社員番号);
                    table.ForeignKey(
                        name: "社員_グループコード_fkey",
                        column: x => x.グループコード,
                        principalTable: "グループ",
                        principalColumn: "グループコード");
                });

            migrationBuilder.CreateTable(
                name: "アップロード履歴",
                columns: table => new
                {
                    社員番号 = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    対象年月 = table.Column<int>(type: "INTEGER", nullable: false),
                    アップロード日時 = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    sp版数 = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("アップロード履歴_pkey", x => new { x.社員番号, x.対象年月 });
                    table.ForeignKey(
                        name: "アップロード履歴_社員番号_fkey",
                        column: x => x.社員番号,
                        principalTable: "社員",
                        principalColumn: "社員番号");
                });

            migrationBuilder.CreateTable(
                name: "ロック",
                columns: table => new
                {
                    社員番号 = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    対象年月 = table.Column<int>(type: "INTEGER", nullable: false),
                    ロック者番号 = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ロック日時 = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("ロック_pkey", x => new { x.社員番号, x.対象年月 });
                    table.ForeignKey(
                        name: "ロック_ロック者番号_fkey",
                        column: x => x.ロック者番号,
                        principalTable: "社員",
                        principalColumn: "社員番号");
                    table.ForeignKey(
                        name: "ロック_社員番号_fkey",
                        column: x => x.社員番号,
                        principalTable: "社員",
                        principalColumn: "社員番号");
                });

            migrationBuilder.CreateTable(
                name: "勤怠",
                columns: table => new
                {
                    社員番号 = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    対象年月 = table.Column<int>(type: "INTEGER", nullable: false),
                    勤務日 = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    出勤日時 = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    退勤日時 = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    所定内時間 = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: true),
                    残業時間 = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: true),
                    深夜残業時間 = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: true),
                    合計時間 = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: true),
                    勤怠区分 = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    備考 = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("勤怠_pkey", x => new { x.社員番号, x.対象年月, x.勤務日 });
                    table.ForeignKey(
                        name: "勤怠_社員番号_fkey",
                        column: x => x.社員番号,
                        principalTable: "社員",
                        principalColumn: "社員番号");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ロック_ロック者番号",
                table: "ロック",
                column: "ロック者番号");

            migrationBuilder.CreateIndex(
                name: "IX_社員_グループコード",
                table: "社員",
                column: "グループコード");

            migrationBuilder.CreateIndex(
                name: "社員_ログインid_key",
                table: "社員",
                column: "ログインid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "アップロード履歴");

            migrationBuilder.DropTable(
                name: "ロック");

            migrationBuilder.DropTable(
                name: "勤怠");

            migrationBuilder.DropTable(
                name: "社員");

            migrationBuilder.DropTable(
                name: "グループ");
        }
    }
}
