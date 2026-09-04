using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dal.Migrations
{
    /// <inheritdoc />
    public partial class FixLegacyEnumStringValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Rows written before the French->English identifier rename still hold the OLD French
            // enum member names as their HasConversion<string>() value (schema/columns were renamed,
            // but stored data wasn't touched by that migration). Translate them to the new English
            // enum member names so EF Core's enum<->string conversion can parse them again.
            migrationBuilder.Sql("UPDATE \"Members\" SET \"Role\" = 'Player' WHERE \"Role\" = 'Joueur';");

            migrationBuilder.Sql("UPDATE \"Participations\" SET \"Role\" = 'Organizer' WHERE \"Role\" = 'Organisateur';");
            migrationBuilder.Sql("UPDATE \"Participations\" SET \"Role\" = 'Player' WHERE \"Role\" = 'Joueur';");
            migrationBuilder.Sql("UPDATE \"Participations\" SET \"Status\" = 'Available' WHERE \"Status\" = 'Libre';");
            migrationBuilder.Sql("UPDATE \"Participations\" SET \"Status\" = 'Reserved' WHERE \"Status\" = 'Reservee';");
            migrationBuilder.Sql("UPDATE \"Participations\" SET \"Status\" = 'Paid' WHERE \"Status\" = 'Payee';");
            migrationBuilder.Sql("UPDATE \"Participations\" SET \"Status\" = 'Cancelled' WHERE \"Status\" = 'Annulee';");

            migrationBuilder.Sql("UPDATE \"Payments\" SET \"Status\" = 'Pending' WHERE \"Status\" = 'EnAttente';");
            migrationBuilder.Sql("UPDATE \"Payments\" SET \"Status\" = 'Validated' WHERE \"Status\" = 'Valide';");
            migrationBuilder.Sql("UPDATE \"Payments\" SET \"Status\" = 'Refunded' WHERE \"Status\" = 'Rembourse';");

            migrationBuilder.Sql("UPDATE \"BalancesDue\" SET \"Status\" = 'Due' WHERE \"Status\" = 'Du';");
            migrationBuilder.Sql("UPDATE \"BalancesDue\" SET \"Status\" = 'Paid' WHERE \"Status\" = 'Paye';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE \"Members\" SET \"Role\" = 'Joueur' WHERE \"Role\" = 'Player';");

            migrationBuilder.Sql("UPDATE \"Participations\" SET \"Role\" = 'Organisateur' WHERE \"Role\" = 'Organizer';");
            migrationBuilder.Sql("UPDATE \"Participations\" SET \"Role\" = 'Joueur' WHERE \"Role\" = 'Player';");
            migrationBuilder.Sql("UPDATE \"Participations\" SET \"Status\" = 'Libre' WHERE \"Status\" = 'Available';");
            migrationBuilder.Sql("UPDATE \"Participations\" SET \"Status\" = 'Reservee' WHERE \"Status\" = 'Reserved';");
            migrationBuilder.Sql("UPDATE \"Participations\" SET \"Status\" = 'Payee' WHERE \"Status\" = 'Paid';");
            migrationBuilder.Sql("UPDATE \"Participations\" SET \"Status\" = 'Annulee' WHERE \"Status\" = 'Cancelled';");

            migrationBuilder.Sql("UPDATE \"Payments\" SET \"Status\" = 'EnAttente' WHERE \"Status\" = 'Pending';");
            migrationBuilder.Sql("UPDATE \"Payments\" SET \"Status\" = 'Valide' WHERE \"Status\" = 'Validated';");
            migrationBuilder.Sql("UPDATE \"Payments\" SET \"Status\" = 'Rembourse' WHERE \"Status\" = 'Refunded';");

            migrationBuilder.Sql("UPDATE \"BalancesDue\" SET \"Status\" = 'Du' WHERE \"Status\" = 'Due';");
            migrationBuilder.Sql("UPDATE \"BalancesDue\" SET \"Status\" = 'Paye' WHERE \"Status\" = 'Paid';");
        }
    }
}
