using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dal.Migrations
{
    /// <inheritdoc />
    public partial class EnforceConcurrencyRulesInDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BalancesDue_MatchId",
                table: "BalancesDue");

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "Participations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "Matches",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "BalancesDue",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Participations_SeatNumber_Range",
                table: "Participations",
                sql: "\"SeatNumber\" BETWEEN 1 AND 4");

            migrationBuilder.CreateIndex(
                name: "UQ_Matches_CourtId_Date_StartTime",
                table: "Matches",
                columns: new[] { "CourtId", "Date", "StartTime" },
                unique: true,
                filter: "\"Status\" <> 'Cancelled'");

            migrationBuilder.CreateIndex(
                name: "UQ_BalancesDue_MatchId",
                table: "BalancesDue",
                column: "MatchId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Participations_SeatNumber_Range",
                table: "Participations");

            migrationBuilder.DropIndex(
                name: "UQ_Matches_CourtId_Date_StartTime",
                table: "Matches");

            migrationBuilder.DropIndex(
                name: "UQ_BalancesDue_MatchId",
                table: "BalancesDue");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Participations");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "BalancesDue");

            migrationBuilder.CreateIndex(
                name: "IX_BalancesDue_MatchId",
                table: "BalancesDue",
                column: "MatchId");
        }
    }
}
