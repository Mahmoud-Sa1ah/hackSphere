using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PentestHub.API.Migrations
{
    /// <inheritdoc />
    public partial class AddGamificationFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropForeignKey(
            //    name: "FK_Reports_ScanHistory_ScanId",
            //    table: "Reports");

            //migrationBuilder.AddColumn<string>(
            //    name: "Bio",
            //    table: "users",
            //    type: "longtext",
            //    nullable: true)
            //    .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "CompletedRooms",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLabSolvedDate",
                table: "Users",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Points",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            //migrationBuilder.AddColumn<string>(
            //    name: "ProfilePhoto",
            //    table: "users",
            //    type: "longtext",
            //    nullable: true)
            //    .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "Streak",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "ScanId",
                table: "Reports",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "Points",
                table: "Labs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Badges",
                columns: table => new
                {
                    BadgeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PointsRequired = table.Column<int>(type: "int", nullable: false),
                    IconUrl = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Badges", x => x.BadgeId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DomainVerifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    DomainName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProofPath = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    VerifiedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    AdminComments = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DomainVerifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DomainVerifications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserBadges",
                columns: table => new
                {
                    UserBadgeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    BadgeId = table.Column<int>(type: "int", nullable: false),
                    DateEarned = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserBadges", x => x.UserBadgeId);
                    table.ForeignKey(
                        name: "FK_UserBadges_Badges_BadgeId",
                        column: x => x.BadgeId,
                        principalTable: "Badges",
                        principalColumn: "BadgeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserBadges_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "Badges",
                columns: new[] { "BadgeId", "Description", "IconUrl", "Name", "PointsRequired" },
                values: new object[,]
                {
                    { 1, "Earned 100 points", "/badges/bronze.png", "Bronze Badge", 100 },
                    { 2, "Earned 500 points", "/badges/silver.png", "Silver Badge", 500 },
                    { 3, "Earned 1000 points", "/badges/gold.png", "Gold Badge", 1000 }
                });

            migrationBuilder.UpdateData(
                table: "Labs",
                keyColumn: "LabId",
                keyValue: 1,
                column: "Points",
                value: 10);

            migrationBuilder.UpdateData(
                table: "Labs",
                keyColumn: "LabId",
                keyValue: 2,
                column: "Points",
                value: 25);

            migrationBuilder.UpdateData(
                table: "Labs",
                keyColumn: "LabId",
                keyValue: 3,
                column: "Points",
                value: 10);

            migrationBuilder.UpdateData(
                table: "Labs",
                keyColumn: "LabId",
                keyValue: 4,
                column: "Points",
                value: 10);

            migrationBuilder.UpdateData(
                table: "Labs",
                keyColumn: "LabId",
                keyValue: 5,
                column: "Points",
                value: 50);

            migrationBuilder.UpdateData(
                table: "Labs",
                keyColumn: "LabId",
                keyValue: 6,
                column: "Points",
                value: 50);

            migrationBuilder.UpdateData(
                table: "Labs",
                keyColumn: "LabId",
                keyValue: 7,
                column: "Points",
                value: 25);

            migrationBuilder.UpdateData(
                table: "Labs",
                keyColumn: "LabId",
                keyValue: 8,
                column: "Points",
                value: 25);

            migrationBuilder.UpdateData(
                table: "Labs",
                keyColumn: "LabId",
                keyValue: 9,
                column: "Points",
                value: 25);

            migrationBuilder.UpdateData(
                table: "Labs",
                keyColumn: "LabId",
                keyValue: 10,
                column: "Points",
                value: 50);

            migrationBuilder.CreateIndex(
                name: "IX_DomainVerifications_UserId",
                table: "DomainVerifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserBadges_BadgeId",
                table: "UserBadges",
                column: "BadgeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserBadges_UserId",
                table: "UserBadges",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_ScanHistory_ScanId",
                table: "Reports",
                column: "ScanId",
                principalTable: "ScanHistory",
                principalColumn: "ScanId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reports_ScanHistory_ScanId",
                table: "Reports");

            migrationBuilder.DropTable(
                name: "DomainVerifications");

            migrationBuilder.DropTable(
                name: "UserBadges");

            migrationBuilder.DropTable(
                name: "Badges");

            migrationBuilder.DropColumn(
                name: "Bio",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CompletedRooms",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastLabSolvedDate",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Points",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ProfilePhoto",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Streak",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Points",
                table: "Labs");

            migrationBuilder.AlterColumn<int>(
                name: "ScanId",
                table: "Reports",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Reports_ScanHistory_ScanId",
                table: "Reports",
                column: "ScanId",
                principalTable: "ScanHistory",
                principalColumn: "ScanId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
