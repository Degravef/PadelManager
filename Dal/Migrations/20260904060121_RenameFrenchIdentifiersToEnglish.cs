using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dal.Migrations
{
    /// <inheritdoc />
    public partial class RenameFrenchIdentifiersToEnglish : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Rename tables
            migrationBuilder.RenameTable(
                name: "Membres",
                newName: "Members");
            migrationBuilder.RenameTable(
                name: "Terrains",
                newName: "Courts");
            migrationBuilder.RenameTable(
                name: "Paiements",
                newName: "Payments");
            migrationBuilder.RenameTable(
                name: "Penalites",
                newName: "Penalties");
            migrationBuilder.RenameTable(
                name: "SoldesDus",
                newName: "BalancesDue");
            migrationBuilder.RenameTable(
                name: "HorairesSites",
                newName: "SiteSchedules");
            migrationBuilder.RenameTable(
                name: "JoursFermeture",
                newName: "ClosureDays");
            migrationBuilder.RenameTable(
                name: "Creneaux",
                newName: "Slots");
            migrationBuilder.RenameTable(
                name: "TypeMembres",
                newName: "MemberTypes");

            // 2. Rename columns
            migrationBuilder.RenameColumn(
                name: "Actif",
                table: "Members",
                newName: "Active");
            migrationBuilder.RenameColumn(
                name: "DateInscription",
                table: "Members",
                newName: "RegistrationDate");
            migrationBuilder.RenameColumn(
                name: "MotDePasseHash",
                table: "Members",
                newName: "PasswordHash");
            migrationBuilder.RenameColumn(
                name: "Telephone",
                table: "Members",
                newName: "Phone");
            migrationBuilder.RenameColumn(
                name: "TypeMembreId",
                table: "Members",
                newName: "MemberTypeId");
            migrationBuilder.RenameColumn(
                name: "Actif",
                table: "Courts",
                newName: "Active");
            migrationBuilder.RenameColumn(
                name: "Couvert",
                table: "Courts",
                newName: "Covered");
            migrationBuilder.RenameColumn(
                name: "Numero",
                table: "Courts",
                newName: "Number");
            migrationBuilder.RenameColumn(
                name: "TypeSurface",
                table: "Courts",
                newName: "SurfaceType");
            migrationBuilder.RenameColumn(
                name: "MembreId",
                table: "Payments",
                newName: "MemberId");
            migrationBuilder.RenameColumn(
                name: "SoldeDuId",
                table: "Payments",
                newName: "BalanceDueId");
            migrationBuilder.RenameColumn(
                name: "DatePaiement",
                table: "Payments",
                newName: "PaymentDate");
            migrationBuilder.RenameColumn(
                name: "Montant",
                table: "Payments",
                newName: "Amount");
            migrationBuilder.RenameColumn(
                name: "MoyenPaiement",
                table: "Payments",
                newName: "PaymentMethod");
            migrationBuilder.RenameColumn(
                name: "ReferenceTransaction",
                table: "Payments",
                newName: "TransactionReference");
            migrationBuilder.RenameColumn(
                name: "Statut",
                table: "Payments",
                newName: "Status");
            migrationBuilder.RenameColumn(
                name: "MembreId",
                table: "Penalties",
                newName: "MemberId");
            migrationBuilder.RenameColumn(
                name: "DateDebut",
                table: "Penalties",
                newName: "StartDate");
            migrationBuilder.RenameColumn(
                name: "DateFin",
                table: "Penalties",
                newName: "EndDate");
            migrationBuilder.RenameColumn(
                name: "Motif",
                table: "Penalties",
                newName: "Reason");
            migrationBuilder.RenameColumn(
                name: "MembreId",
                table: "BalancesDue",
                newName: "MemberId");
            migrationBuilder.RenameColumn(
                name: "DateCreation",
                table: "BalancesDue",
                newName: "CreatedAt");
            migrationBuilder.RenameColumn(
                name: "DateReglement",
                table: "BalancesDue",
                newName: "SettlementDate");
            migrationBuilder.RenameColumn(
                name: "Montant",
                table: "BalancesDue",
                newName: "Amount");
            migrationBuilder.RenameColumn(
                name: "Statut",
                table: "BalancesDue",
                newName: "Status");
            migrationBuilder.RenameColumn(
                name: "Annee",
                table: "SiteSchedules",
                newName: "Year");
            migrationBuilder.RenameColumn(
                name: "DureeMatchMinutes",
                table: "SiteSchedules",
                newName: "MatchDurationMinutes");
            migrationBuilder.RenameColumn(
                name: "HeureDerniereReservation",
                table: "SiteSchedules",
                newName: "ClosingTime");
            migrationBuilder.RenameColumn(
                name: "HeurePremiereReservation",
                table: "SiteSchedules",
                newName: "OpeningTime");
            migrationBuilder.RenameColumn(
                name: "NbJoueursRequis",
                table: "SiteSchedules",
                newName: "RequiredPlayers");
            migrationBuilder.RenameColumn(
                name: "PauseMinutes",
                table: "SiteSchedules",
                newName: "BreakMinutes");
            migrationBuilder.RenameColumn(
                name: "PrixMatch",
                table: "SiteSchedules",
                newName: "MatchPrice");
            migrationBuilder.RenameColumn(
                name: "DateFermeture",
                table: "ClosureDays",
                newName: "ClosureDate");
            migrationBuilder.RenameColumn(
                name: "Motif",
                table: "ClosureDays",
                newName: "Reason");
            migrationBuilder.RenameColumn(
                name: "HoraireSiteId",
                table: "Slots",
                newName: "SiteScheduleId");
            migrationBuilder.RenameColumn(
                name: "HeureDebut",
                table: "Slots",
                newName: "StartTime");
            migrationBuilder.RenameColumn(
                name: "HeureFin",
                table: "Slots",
                newName: "EndTime");
            migrationBuilder.RenameColumn(
                name: "Ordre",
                table: "Slots",
                newName: "Order");
            migrationBuilder.RenameColumn(
                name: "DelaiReservationJours",
                table: "MemberTypes",
                newName: "ReservationWindowDays");
            migrationBuilder.RenameColumn(
                name: "Libelle",
                table: "MemberTypes",
                newName: "Label");
            migrationBuilder.RenameColumn(
                name: "PrefixeMatricule",
                table: "MemberTypes",
                newName: "MatriculePrefix");
            migrationBuilder.RenameColumn(
                name: "TypeMatch",
                table: "Matches",
                newName: "Type");
            migrationBuilder.RenameColumn(
                name: "TerrainId",
                table: "Matches",
                newName: "CourtId");
            migrationBuilder.RenameColumn(
                name: "Statut",
                table: "Matches",
                newName: "Status");
            migrationBuilder.RenameColumn(
                name: "OrganisateurId",
                table: "Matches",
                newName: "OrganizerId");
            migrationBuilder.RenameColumn(
                name: "MontantTotal",
                table: "Matches",
                newName: "TotalAmount");
            migrationBuilder.RenameColumn(
                name: "MontantPaye",
                table: "Matches",
                newName: "AmountPaid");
            migrationBuilder.RenameColumn(
                name: "DateLimite",
                table: "Matches",
                newName: "PaymentDeadline");
            migrationBuilder.RenameColumn(
                name: "DateCreation",
                table: "Matches",
                newName: "CreatedAt");
            migrationBuilder.RenameColumn(
                name: "DateBasculePublic",
                table: "Matches",
                newName: "PublicSwitchDate");
            migrationBuilder.RenameColumn(
                name: "CreneauId",
                table: "Matches",
                newName: "SlotId");
            migrationBuilder.RenameColumn(
                name: "Statut",
                table: "Participations",
                newName: "Status");
            migrationBuilder.RenameColumn(
                name: "NumeroPlace",
                table: "Participations",
                newName: "SeatNumber");
            migrationBuilder.RenameColumn(
                name: "MontantDu",
                table: "Participations",
                newName: "AmountDue");
            migrationBuilder.RenameColumn(
                name: "MembreId",
                table: "Participations",
                newName: "MemberId");
            migrationBuilder.RenameColumn(
                name: "DateValidation",
                table: "Participations",
                newName: "PaymentDate");
            migrationBuilder.RenameColumn(
                name: "DateInscription",
                table: "Participations",
                newName: "RegistrationDate");
            migrationBuilder.RenameColumn(
                name: "Actif",
                table: "Sites",
                newName: "Active");

            // 3. Rename indexes/unique constraints
            migrationBuilder.RenameIndex(
                name: "UQ_Terrains_SiteId_Name",
                table: "Courts",
                newName: "UQ_Courts_SiteId_Name");
            migrationBuilder.RenameIndex(
                name: "UQ_Terrains_SiteId_Numero",
                table: "Courts",
                newName: "UQ_Courts_SiteId_Number");
            migrationBuilder.RenameIndex(
                name: "IX_Terrains_SiteId",
                table: "Courts",
                newName: "IX_Courts_SiteId");
            migrationBuilder.RenameIndex(
                name: "UQ_Membres_Matricule",
                table: "Members",
                newName: "UQ_Members_Matricule");
            migrationBuilder.RenameIndex(
                name: "UQ_Membres_Email",
                table: "Members",
                newName: "UQ_Members_Email");
            migrationBuilder.RenameIndex(
                name: "IX_Membres_SiteId",
                table: "Members",
                newName: "IX_Members_SiteId");
            migrationBuilder.RenameIndex(
                name: "IX_Membres_TypeMembreId",
                table: "Members",
                newName: "IX_Members_MemberTypeId");
            migrationBuilder.RenameIndex(
                name: "UQ_Participations_MatchId_MembreId",
                table: "Participations",
                newName: "UQ_Participations_MatchId_MemberId");
            migrationBuilder.RenameIndex(
                name: "UQ_Participations_MatchId_NumeroPlace",
                table: "Participations",
                newName: "UQ_Participations_MatchId_SeatNumber");
            migrationBuilder.RenameIndex(
                name: "IX_Participations_MembreId",
                table: "Participations",
                newName: "IX_Participations_MemberId");
            migrationBuilder.RenameIndex(
                name: "UQ_TypeMembres_Code",
                table: "MemberTypes",
                newName: "UQ_MemberTypes_Code");
            migrationBuilder.RenameIndex(
                name: "UQ_HorairesSites_SiteId_Annee",
                table: "SiteSchedules",
                newName: "UQ_SiteSchedules_SiteId_Year");
            migrationBuilder.RenameIndex(
                name: "UQ_Creneaux_HoraireSiteId_Ordre",
                table: "Slots",
                newName: "UQ_Slots_SiteScheduleId_Order");
            migrationBuilder.RenameIndex(
                name: "UQ_Paiements_ReferenceTransaction",
                table: "Payments",
                newName: "UQ_Payments_TransactionReference");
            migrationBuilder.RenameIndex(
                name: "IX_Paiements_MembreId",
                table: "Payments",
                newName: "IX_Payments_MemberId");
            migrationBuilder.RenameIndex(
                name: "IX_Paiements_ParticipationId",
                table: "Payments",
                newName: "IX_Payments_ParticipationId");
            migrationBuilder.RenameIndex(
                name: "IX_Paiements_SoldeDuId",
                table: "Payments",
                newName: "IX_Payments_BalanceDueId");
            migrationBuilder.RenameIndex(
                name: "IX_Penalites_MatchId",
                table: "Penalties",
                newName: "IX_Penalties_MatchId");
            migrationBuilder.RenameIndex(
                name: "IX_Penalites_MembreId",
                table: "Penalties",
                newName: "IX_Penalties_MemberId");
            migrationBuilder.RenameIndex(
                name: "IX_SoldesDus_MatchId",
                table: "BalancesDue",
                newName: "IX_BalancesDue_MatchId");
            migrationBuilder.RenameIndex(
                name: "IX_SoldesDus_MembreId",
                table: "BalancesDue",
                newName: "IX_BalancesDue_MemberId");
            migrationBuilder.RenameIndex(
                name: "IX_JoursFermeture_SiteId",
                table: "ClosureDays",
                newName: "IX_ClosureDays_SiteId");
            migrationBuilder.RenameIndex(
                name: "IX_JoursFermeture_DateFermeture",
                table: "ClosureDays",
                newName: "IX_ClosureDays_ClosureDate");
            migrationBuilder.RenameIndex(
                name: "IX_Matches_TerrainId_Date",
                table: "Matches",
                newName: "IX_Matches_CourtId_Date");
            migrationBuilder.RenameIndex(
                name: "IX_Matches_OrganisateurId",
                table: "Matches",
                newName: "IX_Matches_OrganizerId");
            migrationBuilder.RenameIndex(
                name: "IX_Matches_CreneauId",
                table: "Matches",
                newName: "IX_Matches_SlotId");

            // 4. Rename primary key constraints
            migrationBuilder.Sql("ALTER TABLE \"Members\" RENAME CONSTRAINT \"PK_Membres\" TO \"PK_Members\";");
            migrationBuilder.Sql("ALTER TABLE \"Courts\" RENAME CONSTRAINT \"PK_Terrains\" TO \"PK_Courts\";");
            migrationBuilder.Sql("ALTER TABLE \"Payments\" RENAME CONSTRAINT \"PK_Paiements\" TO \"PK_Payments\";");
            migrationBuilder.Sql("ALTER TABLE \"Penalties\" RENAME CONSTRAINT \"PK_Penalites\" TO \"PK_Penalties\";");
            migrationBuilder.Sql("ALTER TABLE \"BalancesDue\" RENAME CONSTRAINT \"PK_SoldesDus\" TO \"PK_BalancesDue\";");
            migrationBuilder.Sql("ALTER TABLE \"SiteSchedules\" RENAME CONSTRAINT \"PK_HorairesSites\" TO \"PK_SiteSchedules\";");
            migrationBuilder.Sql("ALTER TABLE \"ClosureDays\" RENAME CONSTRAINT \"PK_JoursFermeture\" TO \"PK_ClosureDays\";");
            migrationBuilder.Sql("ALTER TABLE \"Slots\" RENAME CONSTRAINT \"PK_Creneaux\" TO \"PK_Slots\";");
            migrationBuilder.Sql("ALTER TABLE \"MemberTypes\" RENAME CONSTRAINT \"PK_TypeMembres\" TO \"PK_MemberTypes\";");

            // 5. Rename foreign key constraints
            migrationBuilder.Sql("ALTER TABLE \"Matches\" RENAME CONSTRAINT \"FK_Matches_Terrains_TerrainId\" TO \"FK_Matches_Courts_CourtId\";");
            migrationBuilder.Sql("ALTER TABLE \"Matches\" RENAME CONSTRAINT \"FK_Matches_Membres_OrganisateurId\" TO \"FK_Matches_Members_OrganizerId\";");
            migrationBuilder.Sql("ALTER TABLE \"Matches\" RENAME CONSTRAINT \"FK_Matches_Creneaux_CreneauId\" TO \"FK_Matches_Slots_SlotId\";");
            migrationBuilder.Sql("ALTER TABLE \"Participations\" RENAME CONSTRAINT \"FK_Participations_Membres_MembreId\" TO \"FK_Participations_Members_MemberId\";");
            migrationBuilder.Sql("ALTER TABLE \"Members\" RENAME CONSTRAINT \"FK_Membres_Sites_SiteId\" TO \"FK_Members_Sites_SiteId\";");
            migrationBuilder.Sql("ALTER TABLE \"Members\" RENAME CONSTRAINT \"FK_Membres_TypeMembres_TypeMembreId\" TO \"FK_Members_MemberTypes_MemberTypeId\";");
            migrationBuilder.Sql("ALTER TABLE \"Courts\" RENAME CONSTRAINT \"FK_Terrains_Sites_SiteId\" TO \"FK_Courts_Sites_SiteId\";");
            migrationBuilder.Sql("ALTER TABLE \"SiteSchedules\" RENAME CONSTRAINT \"FK_HorairesSites_Sites_SiteId\" TO \"FK_SiteSchedules_Sites_SiteId\";");
            migrationBuilder.Sql("ALTER TABLE \"ClosureDays\" RENAME CONSTRAINT \"FK_JoursFermeture_Sites_SiteId\" TO \"FK_ClosureDays_Sites_SiteId\";");
            migrationBuilder.Sql("ALTER TABLE \"Slots\" RENAME CONSTRAINT \"FK_Creneaux_HorairesSites_HoraireSiteId\" TO \"FK_Slots_SiteSchedules_SiteScheduleId\";");
            migrationBuilder.Sql("ALTER TABLE \"Penalties\" RENAME CONSTRAINT \"FK_Penalites_Matches_MatchId\" TO \"FK_Penalties_Matches_MatchId\";");
            migrationBuilder.Sql("ALTER TABLE \"Penalties\" RENAME CONSTRAINT \"FK_Penalites_Membres_MembreId\" TO \"FK_Penalties_Members_MemberId\";");
            migrationBuilder.Sql("ALTER TABLE \"BalancesDue\" RENAME CONSTRAINT \"FK_SoldesDus_Matches_MatchId\" TO \"FK_BalancesDue_Matches_MatchId\";");
            migrationBuilder.Sql("ALTER TABLE \"BalancesDue\" RENAME CONSTRAINT \"FK_SoldesDus_Membres_MembreId\" TO \"FK_BalancesDue_Members_MemberId\";");
            migrationBuilder.Sql("ALTER TABLE \"Payments\" RENAME CONSTRAINT \"FK_Paiements_Membres_MembreId\" TO \"FK_Payments_Members_MemberId\";");
            migrationBuilder.Sql("ALTER TABLE \"Payments\" RENAME CONSTRAINT \"FK_Paiements_Participations_ParticipationId\" TO \"FK_Payments_Participations_ParticipationId\";");
            migrationBuilder.Sql("ALTER TABLE \"Payments\" RENAME CONSTRAINT \"FK_Paiements_SoldesDus_SoldeDuId\" TO \"FK_Payments_BalancesDue_BalanceDueId\";");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 1. Rename foreign key constraints back
            migrationBuilder.Sql("ALTER TABLE \"Payments\" RENAME CONSTRAINT \"FK_Payments_BalancesDue_BalanceDueId\" TO \"FK_Paiements_SoldesDus_SoldeDuId\";");
            migrationBuilder.Sql("ALTER TABLE \"Payments\" RENAME CONSTRAINT \"FK_Payments_Participations_ParticipationId\" TO \"FK_Paiements_Participations_ParticipationId\";");
            migrationBuilder.Sql("ALTER TABLE \"Payments\" RENAME CONSTRAINT \"FK_Payments_Members_MemberId\" TO \"FK_Paiements_Membres_MembreId\";");
            migrationBuilder.Sql("ALTER TABLE \"BalancesDue\" RENAME CONSTRAINT \"FK_BalancesDue_Members_MemberId\" TO \"FK_SoldesDus_Membres_MembreId\";");
            migrationBuilder.Sql("ALTER TABLE \"BalancesDue\" RENAME CONSTRAINT \"FK_BalancesDue_Matches_MatchId\" TO \"FK_SoldesDus_Matches_MatchId\";");
            migrationBuilder.Sql("ALTER TABLE \"Penalties\" RENAME CONSTRAINT \"FK_Penalties_Members_MemberId\" TO \"FK_Penalites_Membres_MembreId\";");
            migrationBuilder.Sql("ALTER TABLE \"Penalties\" RENAME CONSTRAINT \"FK_Penalties_Matches_MatchId\" TO \"FK_Penalites_Matches_MatchId\";");
            migrationBuilder.Sql("ALTER TABLE \"Slots\" RENAME CONSTRAINT \"FK_Slots_SiteSchedules_SiteScheduleId\" TO \"FK_Creneaux_HorairesSites_HoraireSiteId\";");
            migrationBuilder.Sql("ALTER TABLE \"ClosureDays\" RENAME CONSTRAINT \"FK_ClosureDays_Sites_SiteId\" TO \"FK_JoursFermeture_Sites_SiteId\";");
            migrationBuilder.Sql("ALTER TABLE \"SiteSchedules\" RENAME CONSTRAINT \"FK_SiteSchedules_Sites_SiteId\" TO \"FK_HorairesSites_Sites_SiteId\";");
            migrationBuilder.Sql("ALTER TABLE \"Courts\" RENAME CONSTRAINT \"FK_Courts_Sites_SiteId\" TO \"FK_Terrains_Sites_SiteId\";");
            migrationBuilder.Sql("ALTER TABLE \"Members\" RENAME CONSTRAINT \"FK_Members_MemberTypes_MemberTypeId\" TO \"FK_Membres_TypeMembres_TypeMembreId\";");
            migrationBuilder.Sql("ALTER TABLE \"Members\" RENAME CONSTRAINT \"FK_Members_Sites_SiteId\" TO \"FK_Membres_Sites_SiteId\";");
            migrationBuilder.Sql("ALTER TABLE \"Participations\" RENAME CONSTRAINT \"FK_Participations_Members_MemberId\" TO \"FK_Participations_Membres_MembreId\";");
            migrationBuilder.Sql("ALTER TABLE \"Matches\" RENAME CONSTRAINT \"FK_Matches_Slots_SlotId\" TO \"FK_Matches_Creneaux_CreneauId\";");
            migrationBuilder.Sql("ALTER TABLE \"Matches\" RENAME CONSTRAINT \"FK_Matches_Members_OrganizerId\" TO \"FK_Matches_Membres_OrganisateurId\";");
            migrationBuilder.Sql("ALTER TABLE \"Matches\" RENAME CONSTRAINT \"FK_Matches_Courts_CourtId\" TO \"FK_Matches_Terrains_TerrainId\";");

            // 2. Rename primary key constraints back
            migrationBuilder.Sql("ALTER TABLE \"MemberTypes\" RENAME CONSTRAINT \"PK_MemberTypes\" TO \"PK_TypeMembres\";");
            migrationBuilder.Sql("ALTER TABLE \"Slots\" RENAME CONSTRAINT \"PK_Slots\" TO \"PK_Creneaux\";");
            migrationBuilder.Sql("ALTER TABLE \"ClosureDays\" RENAME CONSTRAINT \"PK_ClosureDays\" TO \"PK_JoursFermeture\";");
            migrationBuilder.Sql("ALTER TABLE \"SiteSchedules\" RENAME CONSTRAINT \"PK_SiteSchedules\" TO \"PK_HorairesSites\";");
            migrationBuilder.Sql("ALTER TABLE \"BalancesDue\" RENAME CONSTRAINT \"PK_BalancesDue\" TO \"PK_SoldesDus\";");
            migrationBuilder.Sql("ALTER TABLE \"Penalties\" RENAME CONSTRAINT \"PK_Penalties\" TO \"PK_Penalites\";");
            migrationBuilder.Sql("ALTER TABLE \"Payments\" RENAME CONSTRAINT \"PK_Payments\" TO \"PK_Paiements\";");
            migrationBuilder.Sql("ALTER TABLE \"Courts\" RENAME CONSTRAINT \"PK_Courts\" TO \"PK_Terrains\";");
            migrationBuilder.Sql("ALTER TABLE \"Members\" RENAME CONSTRAINT \"PK_Members\" TO \"PK_Membres\";");

            // 3. Rename indexes/unique constraints back
            migrationBuilder.RenameIndex(
                name: "IX_Matches_SlotId",
                table: "Matches",
                newName: "IX_Matches_CreneauId");
            migrationBuilder.RenameIndex(
                name: "IX_Matches_OrganizerId",
                table: "Matches",
                newName: "IX_Matches_OrganisateurId");
            migrationBuilder.RenameIndex(
                name: "IX_Matches_CourtId_Date",
                table: "Matches",
                newName: "IX_Matches_TerrainId_Date");
            migrationBuilder.RenameIndex(
                name: "IX_ClosureDays_ClosureDate",
                table: "ClosureDays",
                newName: "IX_JoursFermeture_DateFermeture");
            migrationBuilder.RenameIndex(
                name: "IX_ClosureDays_SiteId",
                table: "ClosureDays",
                newName: "IX_JoursFermeture_SiteId");
            migrationBuilder.RenameIndex(
                name: "IX_BalancesDue_MemberId",
                table: "BalancesDue",
                newName: "IX_SoldesDus_MembreId");
            migrationBuilder.RenameIndex(
                name: "IX_BalancesDue_MatchId",
                table: "BalancesDue",
                newName: "IX_SoldesDus_MatchId");
            migrationBuilder.RenameIndex(
                name: "IX_Penalties_MemberId",
                table: "Penalties",
                newName: "IX_Penalites_MembreId");
            migrationBuilder.RenameIndex(
                name: "IX_Penalties_MatchId",
                table: "Penalties",
                newName: "IX_Penalites_MatchId");
            migrationBuilder.RenameIndex(
                name: "IX_Payments_BalanceDueId",
                table: "Payments",
                newName: "IX_Paiements_SoldeDuId");
            migrationBuilder.RenameIndex(
                name: "IX_Payments_ParticipationId",
                table: "Payments",
                newName: "IX_Paiements_ParticipationId");
            migrationBuilder.RenameIndex(
                name: "IX_Payments_MemberId",
                table: "Payments",
                newName: "IX_Paiements_MembreId");
            migrationBuilder.RenameIndex(
                name: "UQ_Payments_TransactionReference",
                table: "Payments",
                newName: "UQ_Paiements_ReferenceTransaction");
            migrationBuilder.RenameIndex(
                name: "UQ_Slots_SiteScheduleId_Order",
                table: "Slots",
                newName: "UQ_Creneaux_HoraireSiteId_Ordre");
            migrationBuilder.RenameIndex(
                name: "UQ_SiteSchedules_SiteId_Year",
                table: "SiteSchedules",
                newName: "UQ_HorairesSites_SiteId_Annee");
            migrationBuilder.RenameIndex(
                name: "UQ_MemberTypes_Code",
                table: "MemberTypes",
                newName: "UQ_TypeMembres_Code");
            migrationBuilder.RenameIndex(
                name: "IX_Participations_MemberId",
                table: "Participations",
                newName: "IX_Participations_MembreId");
            migrationBuilder.RenameIndex(
                name: "UQ_Participations_MatchId_SeatNumber",
                table: "Participations",
                newName: "UQ_Participations_MatchId_NumeroPlace");
            migrationBuilder.RenameIndex(
                name: "UQ_Participations_MatchId_MemberId",
                table: "Participations",
                newName: "UQ_Participations_MatchId_MembreId");
            migrationBuilder.RenameIndex(
                name: "IX_Members_MemberTypeId",
                table: "Members",
                newName: "IX_Membres_TypeMembreId");
            migrationBuilder.RenameIndex(
                name: "IX_Members_SiteId",
                table: "Members",
                newName: "IX_Membres_SiteId");
            migrationBuilder.RenameIndex(
                name: "UQ_Members_Email",
                table: "Members",
                newName: "UQ_Membres_Email");
            migrationBuilder.RenameIndex(
                name: "UQ_Members_Matricule",
                table: "Members",
                newName: "UQ_Membres_Matricule");
            migrationBuilder.RenameIndex(
                name: "IX_Courts_SiteId",
                table: "Courts",
                newName: "IX_Terrains_SiteId");
            migrationBuilder.RenameIndex(
                name: "UQ_Courts_SiteId_Number",
                table: "Courts",
                newName: "UQ_Terrains_SiteId_Numero");
            migrationBuilder.RenameIndex(
                name: "UQ_Courts_SiteId_Name",
                table: "Courts",
                newName: "UQ_Terrains_SiteId_Name");

            // 4. Rename columns back
            migrationBuilder.RenameColumn(
                name: "MemberTypeId",
                table: "Members",
                newName: "TypeMembreId");
            migrationBuilder.RenameColumn(
                name: "Phone",
                table: "Members",
                newName: "Telephone");
            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "Members",
                newName: "MotDePasseHash");
            migrationBuilder.RenameColumn(
                name: "RegistrationDate",
                table: "Members",
                newName: "DateInscription");
            migrationBuilder.RenameColumn(
                name: "Active",
                table: "Members",
                newName: "Actif");
            migrationBuilder.RenameColumn(
                name: "SurfaceType",
                table: "Courts",
                newName: "TypeSurface");
            migrationBuilder.RenameColumn(
                name: "Number",
                table: "Courts",
                newName: "Numero");
            migrationBuilder.RenameColumn(
                name: "Covered",
                table: "Courts",
                newName: "Couvert");
            migrationBuilder.RenameColumn(
                name: "Active",
                table: "Courts",
                newName: "Actif");
            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Payments",
                newName: "Statut");
            migrationBuilder.RenameColumn(
                name: "TransactionReference",
                table: "Payments",
                newName: "ReferenceTransaction");
            migrationBuilder.RenameColumn(
                name: "PaymentMethod",
                table: "Payments",
                newName: "MoyenPaiement");
            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "Payments",
                newName: "Montant");
            migrationBuilder.RenameColumn(
                name: "PaymentDate",
                table: "Payments",
                newName: "DatePaiement");
            migrationBuilder.RenameColumn(
                name: "BalanceDueId",
                table: "Payments",
                newName: "SoldeDuId");
            migrationBuilder.RenameColumn(
                name: "MemberId",
                table: "Payments",
                newName: "MembreId");
            migrationBuilder.RenameColumn(
                name: "Reason",
                table: "Penalties",
                newName: "Motif");
            migrationBuilder.RenameColumn(
                name: "EndDate",
                table: "Penalties",
                newName: "DateFin");
            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "Penalties",
                newName: "DateDebut");
            migrationBuilder.RenameColumn(
                name: "MemberId",
                table: "Penalties",
                newName: "MembreId");
            migrationBuilder.RenameColumn(
                name: "Status",
                table: "BalancesDue",
                newName: "Statut");
            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "BalancesDue",
                newName: "Montant");
            migrationBuilder.RenameColumn(
                name: "SettlementDate",
                table: "BalancesDue",
                newName: "DateReglement");
            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "BalancesDue",
                newName: "DateCreation");
            migrationBuilder.RenameColumn(
                name: "MemberId",
                table: "BalancesDue",
                newName: "MembreId");
            migrationBuilder.RenameColumn(
                name: "MatchPrice",
                table: "SiteSchedules",
                newName: "PrixMatch");
            migrationBuilder.RenameColumn(
                name: "BreakMinutes",
                table: "SiteSchedules",
                newName: "PauseMinutes");
            migrationBuilder.RenameColumn(
                name: "RequiredPlayers",
                table: "SiteSchedules",
                newName: "NbJoueursRequis");
            migrationBuilder.RenameColumn(
                name: "OpeningTime",
                table: "SiteSchedules",
                newName: "HeurePremiereReservation");
            migrationBuilder.RenameColumn(
                name: "ClosingTime",
                table: "SiteSchedules",
                newName: "HeureDerniereReservation");
            migrationBuilder.RenameColumn(
                name: "MatchDurationMinutes",
                table: "SiteSchedules",
                newName: "DureeMatchMinutes");
            migrationBuilder.RenameColumn(
                name: "Year",
                table: "SiteSchedules",
                newName: "Annee");
            migrationBuilder.RenameColumn(
                name: "Reason",
                table: "ClosureDays",
                newName: "Motif");
            migrationBuilder.RenameColumn(
                name: "ClosureDate",
                table: "ClosureDays",
                newName: "DateFermeture");
            migrationBuilder.RenameColumn(
                name: "Order",
                table: "Slots",
                newName: "Ordre");
            migrationBuilder.RenameColumn(
                name: "EndTime",
                table: "Slots",
                newName: "HeureFin");
            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "Slots",
                newName: "HeureDebut");
            migrationBuilder.RenameColumn(
                name: "SiteScheduleId",
                table: "Slots",
                newName: "HoraireSiteId");
            migrationBuilder.RenameColumn(
                name: "MatriculePrefix",
                table: "MemberTypes",
                newName: "PrefixeMatricule");
            migrationBuilder.RenameColumn(
                name: "Label",
                table: "MemberTypes",
                newName: "Libelle");
            migrationBuilder.RenameColumn(
                name: "ReservationWindowDays",
                table: "MemberTypes",
                newName: "DelaiReservationJours");
            migrationBuilder.RenameColumn(
                name: "SlotId",
                table: "Matches",
                newName: "CreneauId");
            migrationBuilder.RenameColumn(
                name: "PublicSwitchDate",
                table: "Matches",
                newName: "DateBasculePublic");
            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Matches",
                newName: "DateCreation");
            migrationBuilder.RenameColumn(
                name: "PaymentDeadline",
                table: "Matches",
                newName: "DateLimite");
            migrationBuilder.RenameColumn(
                name: "AmountPaid",
                table: "Matches",
                newName: "MontantPaye");
            migrationBuilder.RenameColumn(
                name: "TotalAmount",
                table: "Matches",
                newName: "MontantTotal");
            migrationBuilder.RenameColumn(
                name: "OrganizerId",
                table: "Matches",
                newName: "OrganisateurId");
            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Matches",
                newName: "Statut");
            migrationBuilder.RenameColumn(
                name: "CourtId",
                table: "Matches",
                newName: "TerrainId");
            migrationBuilder.RenameColumn(
                name: "Type",
                table: "Matches",
                newName: "TypeMatch");
            migrationBuilder.RenameColumn(
                name: "RegistrationDate",
                table: "Participations",
                newName: "DateInscription");
            migrationBuilder.RenameColumn(
                name: "PaymentDate",
                table: "Participations",
                newName: "DateValidation");
            migrationBuilder.RenameColumn(
                name: "MemberId",
                table: "Participations",
                newName: "MembreId");
            migrationBuilder.RenameColumn(
                name: "AmountDue",
                table: "Participations",
                newName: "MontantDu");
            migrationBuilder.RenameColumn(
                name: "SeatNumber",
                table: "Participations",
                newName: "NumeroPlace");
            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Participations",
                newName: "Statut");
            migrationBuilder.RenameColumn(
                name: "Active",
                table: "Sites",
                newName: "Actif");

            // 5. Rename tables back
            migrationBuilder.RenameTable(
                name: "MemberTypes",
                newName: "TypeMembres");
            migrationBuilder.RenameTable(
                name: "Slots",
                newName: "Creneaux");
            migrationBuilder.RenameTable(
                name: "ClosureDays",
                newName: "JoursFermeture");
            migrationBuilder.RenameTable(
                name: "SiteSchedules",
                newName: "HorairesSites");
            migrationBuilder.RenameTable(
                name: "BalancesDue",
                newName: "SoldesDus");
            migrationBuilder.RenameTable(
                name: "Penalties",
                newName: "Penalites");
            migrationBuilder.RenameTable(
                name: "Payments",
                newName: "Paiements");
            migrationBuilder.RenameTable(
                name: "Courts",
                newName: "Terrains");
            migrationBuilder.RenameTable(
                name: "Members",
                newName: "Membres");
        }
    }
}
