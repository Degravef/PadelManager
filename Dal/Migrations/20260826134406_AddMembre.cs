using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Dal.Migrations
{
    /// <inheritdoc />
    public partial class AddMembre : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Membres",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Matricule = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TypeMembre = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SiteId = table.Column<int>(type: "integer", nullable: true),
                    SoldeDu = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    DateFinPenalite = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Membres", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Membres_Sites_SiteId",
                        column: x => x.SiteId,
                        principalTable: "Sites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Membres_SiteId",
                table: "Membres",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "UQ_Membres_Matricule",
                table: "Membres",
                column: "Matricule",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Membres");
        }
    }
}
