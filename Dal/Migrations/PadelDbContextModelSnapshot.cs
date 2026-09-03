
using System;
using Dal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Dal.Migrations
{
    [DbContext(typeof(PadelDbContext))]
    partial class PadelDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "10.0.11")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Core.Domain.Entities.Creneau", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<TimeOnly>("HeureDebut")
                        .HasColumnType("time without time zone");

                    b.Property<TimeOnly>("HeureFin")
                        .HasColumnType("time without time zone");

                    b.Property<int>("HoraireSiteId")
                        .HasColumnType("integer");

                    b.Property<int>("Ordre")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("HoraireSiteId", "Ordre")
                        .IsUnique()
                        .HasDatabaseName("UQ_Creneaux_HoraireSiteId_Ordre");

                    b.ToTable("Creneaux");
                });

            modelBuilder.Entity("Core.Domain.Entities.HoraireSite", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("Annee")
                        .HasColumnType("integer");

                    b.Property<int>("DureeMatchMinutes")
                        .HasColumnType("integer");

                    b.Property<TimeOnly>("HeureDerniereReservation")
                        .HasColumnType("time without time zone");

                    b.Property<TimeOnly>("HeurePremiereReservation")
                        .HasColumnType("time without time zone");

                    b.Property<int>("NbJoueursRequis")
                        .HasColumnType("integer");

                    b.Property<int>("PauseMinutes")
                        .HasColumnType("integer");

                    b.Property<decimal>("PrixMatch")
                        .HasPrecision(10, 2)
                        .HasColumnType("numeric(10,2)");

                    b.Property<int>("SiteId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("SiteId", "Annee")
                        .IsUnique()
                        .HasDatabaseName("UQ_HorairesSites_SiteId_Annee");

                    b.ToTable("HorairesSites");
                });

            modelBuilder.Entity("Core.Domain.Entities.JourFermeture", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateOnly>("DateFermeture")
                        .HasColumnType("date");

                    b.Property<string>("Motif")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<int?>("SiteId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("DateFermeture");

                    b.HasIndex("SiteId");

                    b.ToTable("JoursFermeture");
                });

            modelBuilder.Entity("Core.Domain.Entities.Match", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int?>("CreneauId")
                        .HasColumnType("integer");

                    b.Property<DateOnly>("Date")
                        .HasColumnType("date");

                    b.Property<DateTime?>("DateBasculePublic")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("DateCreation")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateOnly>("DateLimite")
                        .HasColumnType("date");

                    b.Property<TimeOnly>("EndTime")
                        .HasColumnType("time without time zone");

                    b.Property<decimal>("MontantPaye")
                        .HasPrecision(10, 2)
                        .HasColumnType("numeric(10,2)");

                    b.Property<decimal>("MontantTotal")
                        .HasPrecision(10, 2)
                        .HasColumnType("numeric(10,2)");

                    b.Property<int>("OrganisateurId")
                        .HasColumnType("integer");

                    b.Property<TimeOnly>("StartTime")
                        .HasColumnType("time without time zone");

                    b.Property<string>("Statut")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)");

                    b.Property<int>("TerrainId")
                        .HasColumnType("integer");

                    b.Property<string>("TypeMatch")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)");

                    b.HasKey("Id");

                    b.HasIndex("CreneauId");

                    b.HasIndex("OrganisateurId");

                    b.HasIndex("TerrainId", "Date");

                    b.ToTable("Matches");
                });

            modelBuilder.Entity("Core.Domain.Entities.Membre", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<bool>("Actif")
                        .HasColumnType("boolean");

                    b.Property<DateOnly>("DateInscription")
                        .HasColumnType("date");

                    b.Property<string>("Email")
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("Matricule")
                        .IsRequired()
                        .HasMaxLength(6)
                        .HasColumnType("character varying(6)");

                    b.Property<string>("MotDePasseHash")
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)");

                    b.Property<int?>("SiteId")
                        .HasColumnType("integer");

                    b.Property<string>("Telephone")
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<int>("TypeMembreId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("Email")
                        .IsUnique()
                        .HasDatabaseName("UQ_Membres_Email");

                    b.HasIndex("Matricule")
                        .IsUnique()
                        .HasDatabaseName("UQ_Membres_Matricule");

                    b.HasIndex("SiteId");

                    b.HasIndex("TypeMembreId");

                    b.ToTable("Membres");
                });

            modelBuilder.Entity("Core.Domain.Entities.Paiement", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("DatePaiement")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("MembreId")
                        .HasColumnType("integer");

                    b.Property<decimal>("Montant")
                        .HasPrecision(10, 2)
                        .HasColumnType("numeric(10,2)");

                    b.Property<string>("MoyenPaiement")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<int?>("ParticipationId")
                        .HasColumnType("integer");

                    b.Property<string>("ReferenceTransaction")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<int?>("SoldeDuId")
                        .HasColumnType("integer");

                    b.Property<string>("Statut")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)");

                    b.HasKey("Id");

                    b.HasIndex("MembreId");

                    b.HasIndex("ParticipationId");

                    b.HasIndex("ReferenceTransaction")
                        .IsUnique()
                        .HasDatabaseName("UQ_Paiements_ReferenceTransaction");

                    b.HasIndex("SoldeDuId");

                    b.ToTable("Paiements");
                });

            modelBuilder.Entity("Core.Domain.Entities.Participation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("DateInscription")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("DateValidation")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("MatchId")
                        .HasColumnType("integer");

                    b.Property<int?>("MembreId")
                        .HasColumnType("integer");

                    b.Property<decimal>("MontantDu")
                        .HasPrecision(10, 2)
                        .HasColumnType("numeric(10,2)");

                    b.Property<int>("NumeroPlace")
                        .HasColumnType("integer");

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)");

                    b.Property<string>("Statut")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)");

                    b.HasKey("Id");

                    b.HasIndex("MembreId");

                    b.HasIndex("MatchId", "MembreId")
                        .IsUnique()
                        .HasDatabaseName("UQ_Participations_MatchId_MembreId");

                    b.HasIndex("MatchId", "NumeroPlace")
                        .IsUnique()
                        .HasDatabaseName("UQ_Participations_MatchId_NumeroPlace");

                    b.ToTable("Participations");
                });

            modelBuilder.Entity("Core.Domain.Entities.Penalite", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<bool>("Active")
                        .HasColumnType("boolean");

                    b.Property<DateOnly>("DateDebut")
                        .HasColumnType("date");

                    b.Property<DateOnly>("DateFin")
                        .HasColumnType("date");

                    b.Property<int?>("MatchId")
                        .HasColumnType("integer");

                    b.Property<int>("MembreId")
                        .HasColumnType("integer");

                    b.Property<string>("Motif")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.HasKey("Id");

                    b.HasIndex("MatchId");

                    b.HasIndex("MembreId");

                    b.ToTable("Penalites");
                });

            modelBuilder.Entity("Core.Domain.Entities.Site", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<bool>("Actif")
                        .HasColumnType("boolean");

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<int>("AdminId")
                        .HasColumnType("integer");

                    b.Property<string>("City")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("Email")
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("Phone")
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<string>("PostalCode")
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)");

                    b.HasKey("Id");

                    b.HasIndex("AdminId");

                    b.HasIndex("AdminId", "Name")
                        .IsUnique()
                        .HasDatabaseName("UQ_Sites_AdminId_Name");

                    b.ToTable("Sites");
                });

            modelBuilder.Entity("Core.Domain.Entities.SoldeDu", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("DateCreation")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("DateReglement")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("MatchId")
                        .HasColumnType("integer");

                    b.Property<int>("MembreId")
                        .HasColumnType("integer");

                    b.Property<decimal>("Montant")
                        .HasPrecision(10, 2)
                        .HasColumnType("numeric(10,2)");

                    b.Property<string>("Statut")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)");

                    b.HasKey("Id");

                    b.HasIndex("MatchId");

                    b.HasIndex("MembreId");

                    b.ToTable("SoldesDus");
                });

            modelBuilder.Entity("Core.Domain.Entities.Terrain", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<bool>("Actif")
                        .HasColumnType("boolean");

                    b.Property<bool>("Couvert")
                        .HasColumnType("boolean");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("Numero")
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)");

                    b.Property<int>("SiteId")
                        .HasColumnType("integer");

                    b.Property<string>("TypeSurface")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.HasKey("Id");

                    b.HasIndex("SiteId");

                    b.HasIndex("SiteId", "Name")
                        .IsUnique()
                        .HasDatabaseName("UQ_Terrains_SiteId_Name");

                    b.HasIndex("SiteId", "Numero")
                        .IsUnique()
                        .HasDatabaseName("UQ_Terrains_SiteId_Numero");

                    b.ToTable("Terrains");
                });

            modelBuilder.Entity("Core.Domain.Entities.TypeMembre", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("character varying(10)");

                    b.Property<int>("DelaiReservationJours")
                        .HasColumnType("integer");

                    b.Property<string>("Libelle")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<string>("PrefixeMatricule")
                        .IsRequired()
                        .HasMaxLength(1)
                        .HasColumnType("character varying(1)");

                    b.HasKey("Id");

                    b.HasIndex("Code")
                        .IsUnique()
                        .HasDatabaseName("UQ_TypeMembres_Code");

                    b.ToTable("TypeMembres");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Code = "GLOBAL",
                            DelaiReservationJours = 21,
                            Libelle = "Membre global",
                            PrefixeMatricule = "G"
                        },
                        new
                        {
                            Id = 2,
                            Code = "SITE",
                            DelaiReservationJours = 14,
                            Libelle = "Membre de site",
                            PrefixeMatricule = "S"
                        },
                        new
                        {
                            Id = 3,
                            Code = "LIBRE",
                            DelaiReservationJours = 5,
                            Libelle = "Membre libre",
                            PrefixeMatricule = "L"
                        });
                });

            modelBuilder.Entity("Core.Domain.Entities.Creneau", b =>
                {
                    b.HasOne("Core.Domain.Entities.HoraireSite", "HoraireSite")
                        .WithMany("Creneaux")
                        .HasForeignKey("HoraireSiteId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("HoraireSite");
                });

            modelBuilder.Entity("Core.Domain.Entities.HoraireSite", b =>
                {
                    b.HasOne("Core.Domain.Entities.Site", "Site")
                        .WithMany("HorairesSites")
                        .HasForeignKey("SiteId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Site");
                });

            modelBuilder.Entity("Core.Domain.Entities.JourFermeture", b =>
                {
                    b.HasOne("Core.Domain.Entities.Site", "Site")
                        .WithMany("JoursFermeture")
                        .HasForeignKey("SiteId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("Site");
                });

            modelBuilder.Entity("Core.Domain.Entities.Match", b =>
                {
                    b.HasOne("Core.Domain.Entities.Creneau", "Creneau")
                        .WithMany("Matches")
                        .HasForeignKey("CreneauId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("Core.Domain.Entities.Membre", "Organisateur")
                        .WithMany("MatchesOrganises")
                        .HasForeignKey("OrganisateurId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Core.Domain.Entities.Terrain", "Terrain")
                        .WithMany("Matches")
                        .HasForeignKey("TerrainId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Creneau");

                    b.Navigation("Organisateur");

                    b.Navigation("Terrain");
                });

            modelBuilder.Entity("Core.Domain.Entities.Membre", b =>
                {
                    b.HasOne("Core.Domain.Entities.Site", "Site")
                        .WithMany("Membres")
                        .HasForeignKey("SiteId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("Core.Domain.Entities.TypeMembre", "TypeMembre")
                        .WithMany("Membres")
                        .HasForeignKey("TypeMembreId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Site");

                    b.Navigation("TypeMembre");
                });

            modelBuilder.Entity("Core.Domain.Entities.Paiement", b =>
                {
                    b.HasOne("Core.Domain.Entities.Membre", "Membre")
                        .WithMany("Paiements")
                        .HasForeignKey("MembreId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Core.Domain.Entities.Participation", "Participation")
                        .WithMany("Paiements")
                        .HasForeignKey("ParticipationId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("Core.Domain.Entities.SoldeDu", "SoldeDu")
                        .WithMany("Paiements")
                        .HasForeignKey("SoldeDuId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("Membre");

                    b.Navigation("Participation");

                    b.Navigation("SoldeDu");
                });

            modelBuilder.Entity("Core.Domain.Entities.Participation", b =>
                {
                    b.HasOne("Core.Domain.Entities.Match", "Match")
                        .WithMany("Participations")
                        .HasForeignKey("MatchId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Core.Domain.Entities.Membre", "Membre")
                        .WithMany("Participations")
                        .HasForeignKey("MembreId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("Match");

                    b.Navigation("Membre");
                });

            modelBuilder.Entity("Core.Domain.Entities.Penalite", b =>
                {
                    b.HasOne("Core.Domain.Entities.Match", "Match")
                        .WithMany("Penalites")
                        .HasForeignKey("MatchId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("Core.Domain.Entities.Membre", "Membre")
                        .WithMany("Penalites")
                        .HasForeignKey("MembreId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Match");

                    b.Navigation("Membre");
                });

            modelBuilder.Entity("Core.Domain.Entities.SoldeDu", b =>
                {
                    b.HasOne("Core.Domain.Entities.Match", "Match")
                        .WithMany("SoldesDus")
                        .HasForeignKey("MatchId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Core.Domain.Entities.Membre", "Membre")
                        .WithMany("SoldesDus")
                        .HasForeignKey("MembreId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Match");

                    b.Navigation("Membre");
                });

            modelBuilder.Entity("Core.Domain.Entities.Terrain", b =>
                {
                    b.HasOne("Core.Domain.Entities.Site", "Site")
                        .WithMany("Terrains")
                        .HasForeignKey("SiteId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Site");
                });

            modelBuilder.Entity("Core.Domain.Entities.Creneau", b =>
                {
                    b.Navigation("Matches");
                });

            modelBuilder.Entity("Core.Domain.Entities.HoraireSite", b =>
                {
                    b.Navigation("Creneaux");
                });

            modelBuilder.Entity("Core.Domain.Entities.Match", b =>
                {
                    b.Navigation("Participations");

                    b.Navigation("Penalites");

                    b.Navigation("SoldesDus");
                });

            modelBuilder.Entity("Core.Domain.Entities.Membre", b =>
                {
                    b.Navigation("MatchesOrganises");

                    b.Navigation("Paiements");

                    b.Navigation("Participations");

                    b.Navigation("Penalites");

                    b.Navigation("SoldesDus");
                });

            modelBuilder.Entity("Core.Domain.Entities.Participation", b =>
                {
                    b.Navigation("Paiements");
                });

            modelBuilder.Entity("Core.Domain.Entities.Site", b =>
                {
                    b.Navigation("HorairesSites");

                    b.Navigation("JoursFermeture");

                    b.Navigation("Membres");

                    b.Navigation("Terrains");
                });

            modelBuilder.Entity("Core.Domain.Entities.SoldeDu", b =>
                {
                    b.Navigation("Paiements");
                });

            modelBuilder.Entity("Core.Domain.Entities.Terrain", b =>
                {
                    b.Navigation("Matches");
                });

            modelBuilder.Entity("Core.Domain.Entities.TypeMembre", b =>
                {
                    b.Navigation("Membres");
                });
#pragma warning restore 612, 618
        }
    }
}
