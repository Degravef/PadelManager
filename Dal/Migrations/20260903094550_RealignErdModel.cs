using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Dal.Migrations
{
    /// <inheritdoc />
    public partial class RealignErdModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "APaye",
                table: "Participations");

            migrationBuilder.DropColumn(
                name: "DateFinPenalite",
                table: "Membres");

            migrationBuilder.DropColumn(
                name: "SoldeDu",
                table: "Membres");

            // NOT a rename: the old TypeMembre column held the (now-removed) enum's Global/Site/Libre
            // values. It's replaced by the TypeMembreId FK below; Role is an unrelated new column
            // (Joueur/AdminSite/AdminGlobal) that happens to share the same "varchar(20)" shape, which
            // is what made EF's migration scaffolding misdetect this as a rename.
            migrationBuilder.DropColumn(
                name: "TypeMembre",
                table: "Membres");

            migrationBuilder.RenameColumn(
                name: "DatePaiement",
                table: "Participations",
                newName: "DateValidation");

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "Membres",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Joueur");

            migrationBuilder.AddColumn<bool>(
                name: "Actif",
                table: "Terrains",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Couvert",
                table: "Terrains",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Numero",
                table: "Terrains",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TypeSurface",
                table: "Terrains",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Actif",
                table: "Sites",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Sites",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Sites",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Sites",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                table: "Sites",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "MembreId",
                table: "Participations",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "NumeroPlace",
                table: "Participations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "Participations",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Statut",
                table: "Participations",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "Actif",
                table: "Membres",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateOnly>(
                name: "DateInscription",
                table: "Membres",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Membres",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotDePasseHash",
                table: "Membres",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Telephone",
                table: "Membres",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TypeMembreId",
                table: "Membres",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CreneauId",
                table: "Matches",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateBasculePublic",
                table: "Matches",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateCreation",
                table: "Matches",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateOnly>(
                name: "DateLimite",
                table: "Matches",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<TimeOnly>(
                name: "EndTime",
                table: "Matches",
                type: "time without time zone",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.AddColumn<decimal>(
                name: "MontantPaye",
                table: "Matches",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "HorairesSites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SiteId = table.Column<int>(type: "integer", nullable: false),
                    Annee = table.Column<int>(type: "integer", nullable: false),
                    HeurePremiereReservation = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    HeureDerniereReservation = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    DureeMatchMinutes = table.Column<int>(type: "integer", nullable: false),
                    PauseMinutes = table.Column<int>(type: "integer", nullable: false),
                    NbJoueursRequis = table.Column<int>(type: "integer", nullable: false),
                    PrixMatch = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HorairesSites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HorairesSites_Sites_SiteId",
                        column: x => x.SiteId,
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JoursFermeture",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SiteId = table.Column<int>(type: "integer", nullable: true),
                    DateFermeture = table.Column<DateOnly>(type: "date", nullable: false),
                    Motif = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JoursFermeture", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JoursFermeture_Sites_SiteId",
                        column: x => x.SiteId,
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Penalites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MembreId = table.Column<int>(type: "integer", nullable: false),
                    MatchId = table.Column<int>(type: "integer", nullable: true),
                    Motif = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    DateDebut = table.Column<DateOnly>(type: "date", nullable: false),
                    DateFin = table.Column<DateOnly>(type: "date", nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Penalites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Penalites_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Penalites_Membres_MembreId",
                        column: x => x.MembreId,
                        principalTable: "Membres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SoldesDus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MembreId = table.Column<int>(type: "integer", nullable: false),
                    MatchId = table.Column<int>(type: "integer", nullable: false),
                    Montant = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Statut = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateReglement = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoldesDus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SoldesDus_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SoldesDus_Membres_MembreId",
                        column: x => x.MembreId,
                        principalTable: "Membres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TypeMembres",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Libelle = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PrefixeMatricule = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    DelaiReservationJours = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TypeMembres", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Creneaux",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    HoraireSiteId = table.Column<int>(type: "integer", nullable: false),
                    Ordre = table.Column<int>(type: "integer", nullable: false),
                    HeureDebut = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    HeureFin = table.Column<TimeOnly>(type: "time without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Creneaux", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Creneaux_HorairesSites_HoraireSiteId",
                        column: x => x.HoraireSiteId,
                        principalTable: "HorairesSites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Paiements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MembreId = table.Column<int>(type: "integer", nullable: false),
                    ParticipationId = table.Column<int>(type: "integer", nullable: true),
                    SoldeDuId = table.Column<int>(type: "integer", nullable: true),
                    Montant = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    DatePaiement = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MoyenPaiement = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Statut = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ReferenceTransaction = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Paiements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Paiements_Membres_MembreId",
                        column: x => x.MembreId,
                        principalTable: "Membres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Paiements_Participations_ParticipationId",
                        column: x => x.ParticipationId,
                        principalTable: "Participations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Paiements_SoldesDus_SoldeDuId",
                        column: x => x.SoldeDuId,
                        principalTable: "SoldesDus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "TypeMembres",
                columns: new[] { "Id", "Code", "DelaiReservationJours", "Libelle", "PrefixeMatricule" },
                values: new object[,]
                {
                    { 1, "GLOBAL", 21, "Membre global", "G" },
                    { 2, "SITE", 14, "Membre de site", "S" },
                    { 3, "LIBRE", 5, "Membre libre", "L" }
                });

            migrationBuilder.CreateIndex(
                name: "UQ_Terrains_SiteId_Numero",
                table: "Terrains",
                columns: new[] { "SiteId", "Numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Participations_MatchId_NumeroPlace",
                table: "Participations",
                columns: new[] { "MatchId", "NumeroPlace" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Membres_TypeMembreId",
                table: "Membres",
                column: "TypeMembreId");

            migrationBuilder.CreateIndex(
                name: "UQ_Membres_Email",
                table: "Membres",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Matches_CreneauId",
                table: "Matches",
                column: "CreneauId");

            migrationBuilder.CreateIndex(
                name: "UQ_Creneaux_HoraireSiteId_Ordre",
                table: "Creneaux",
                columns: new[] { "HoraireSiteId", "Ordre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_HorairesSites_SiteId_Annee",
                table: "HorairesSites",
                columns: new[] { "SiteId", "Annee" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JoursFermeture_DateFermeture",
                table: "JoursFermeture",
                column: "DateFermeture");

            migrationBuilder.CreateIndex(
                name: "IX_JoursFermeture_SiteId",
                table: "JoursFermeture",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "IX_Paiements_MembreId",
                table: "Paiements",
                column: "MembreId");

            migrationBuilder.CreateIndex(
                name: "IX_Paiements_ParticipationId",
                table: "Paiements",
                column: "ParticipationId");

            migrationBuilder.CreateIndex(
                name: "IX_Paiements_SoldeDuId",
                table: "Paiements",
                column: "SoldeDuId");

            migrationBuilder.CreateIndex(
                name: "UQ_Paiements_ReferenceTransaction",
                table: "Paiements",
                column: "ReferenceTransaction",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Penalites_MatchId",
                table: "Penalites",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_Penalites_MembreId",
                table: "Penalites",
                column: "MembreId");

            migrationBuilder.CreateIndex(
                name: "IX_SoldesDus_MatchId",
                table: "SoldesDus",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_SoldesDus_MembreId",
                table: "SoldesDus",
                column: "MembreId");

            migrationBuilder.CreateIndex(
                name: "UQ_TypeMembres_Code",
                table: "TypeMembres",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Matches_Creneaux_CreneauId",
                table: "Matches",
                column: "CreneauId",
                principalTable: "Creneaux",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Membres_TypeMembres_TypeMembreId",
                table: "Membres",
                column: "TypeMembreId",
                principalTable: "TypeMembres",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Matches_Creneaux_CreneauId",
                table: "Matches");

            migrationBuilder.DropForeignKey(
                name: "FK_Membres_TypeMembres_TypeMembreId",
                table: "Membres");

            migrationBuilder.DropTable(
                name: "Creneaux");

            migrationBuilder.DropTable(
                name: "JoursFermeture");

            migrationBuilder.DropTable(
                name: "Paiements");

            migrationBuilder.DropTable(
                name: "Penalites");

            migrationBuilder.DropTable(
                name: "TypeMembres");

            migrationBuilder.DropTable(
                name: "HorairesSites");

            migrationBuilder.DropTable(
                name: "SoldesDus");

            migrationBuilder.DropIndex(
                name: "UQ_Terrains_SiteId_Numero",
                table: "Terrains");

            migrationBuilder.DropIndex(
                name: "UQ_Participations_MatchId_NumeroPlace",
                table: "Participations");

            migrationBuilder.DropIndex(
                name: "IX_Membres_TypeMembreId",
                table: "Membres");

            migrationBuilder.DropIndex(
                name: "UQ_Membres_Email",
                table: "Membres");

            migrationBuilder.DropIndex(
                name: "IX_Matches_CreneauId",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "Actif",
                table: "Terrains");

            migrationBuilder.DropColumn(
                name: "Couvert",
                table: "Terrains");

            migrationBuilder.DropColumn(
                name: "Numero",
                table: "Terrains");

            migrationBuilder.DropColumn(
                name: "TypeSurface",
                table: "Terrains");

            migrationBuilder.DropColumn(
                name: "Actif",
                table: "Sites");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Sites");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Sites");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Sites");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                table: "Sites");

            migrationBuilder.DropColumn(
                name: "NumeroPlace",
                table: "Participations");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "Participations");

            migrationBuilder.DropColumn(
                name: "Statut",
                table: "Participations");

            migrationBuilder.DropColumn(
                name: "Actif",
                table: "Membres");

            migrationBuilder.DropColumn(
                name: "DateInscription",
                table: "Membres");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Membres");

            migrationBuilder.DropColumn(
                name: "MotDePasseHash",
                table: "Membres");

            migrationBuilder.DropColumn(
                name: "Telephone",
                table: "Membres");

            migrationBuilder.DropColumn(
                name: "TypeMembreId",
                table: "Membres");

            migrationBuilder.DropColumn(
                name: "CreneauId",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "DateBasculePublic",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "DateCreation",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "DateLimite",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "MontantPaye",
                table: "Matches");

            migrationBuilder.RenameColumn(
                name: "DateValidation",
                table: "Participations",
                newName: "DatePaiement");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "Membres");

            migrationBuilder.AddColumn<string>(
                name: "TypeMembre",
                table: "Membres",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "MembreId",
                table: "Participations",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "APaye",
                table: "Participations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateFinPenalite",
                table: "Membres",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SoldeDu",
                table: "Membres",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }
    }
}
