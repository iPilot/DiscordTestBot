﻿// <auto-generated />
using System;
using FilmsBot.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FaryBotHost.Migrations
{
    [DbContext(typeof(FilmsBotDbContext))]
    [Migration("20220402084400_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("FaryBotHost.Database.Film", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("ID");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<decimal?>("AddedById")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("ADDED_BY_ID");

                    b.Property<decimal?>("GuildId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("GUILD_ID");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("citext")
                        .HasColumnName("NAME");

                    b.Property<int?>("Year")
                        .HasColumnType("integer")
                        .HasColumnName("YEAR");

                    b.HasKey("Id");

                    b.HasIndex("AddedById");

                    b.ToTable("FILMS");
                });

            modelBuilder.Entity("FaryBotHost.Database.FilmRating", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("ID");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<DateTime>("Date")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("DATE");

                    b.Property<decimal>("ParticipantId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("PARTICIPANT_ID");

                    b.Property<double>("Rating")
                        .HasColumnType("double precision")
                        .HasColumnName("RATING");

                    b.HasKey("Id");

                    b.HasIndex("ParticipantId");

                    b.ToTable("RATINGS");
                });

            modelBuilder.Entity("FaryBotHost.Database.FilmVote", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("ID");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<int>("Amount")
                        .HasColumnType("integer")
                        .HasColumnName("AMOUNT");

                    b.Property<long>("FilmId")
                        .HasColumnType("bigint")
                        .HasColumnName("FILM_ID");

                    b.Property<decimal>("ParticipantId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("PARTICIPANT_ID");

                    b.Property<long>("SessionId")
                        .HasColumnType("bigint")
                        .HasColumnName("SESSION_ID");

                    b.HasKey("Id");

                    b.HasIndex("FilmId");

                    b.HasIndex("ParticipantId");

                    b.HasIndex("SessionId");

                    b.ToTable("VOTES");
                });

            modelBuilder.Entity("FaryBotHost.Database.Participant", b =>
                {
                    b.Property<decimal>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("ID");

                    b.Property<DateTime>("JoinedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("JOINED_AT");

                    b.HasKey("Id");

                    b.ToTable("PARTICIPANTS");
                });

            modelBuilder.Entity("FaryBotHost.Database.Session", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("ID");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<DateTime?>("End")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("END");

                    b.Property<long?>("FilmId")
                        .HasColumnType("bigint")
                        .HasColumnName("FILM_ID");

                    b.Property<DateTime>("Start")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("START");

                    b.HasKey("Id");

                    b.HasIndex("FilmId")
                        .IsUnique();

                    b.ToTable("SESSIONS");
                });

            modelBuilder.Entity("FaryBotHost.Database.Film", b =>
                {
                    b.HasOne("FaryBotHost.Database.Participant", "AddedBy")
                        .WithMany("Films")
                        .HasForeignKey("AddedById")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("AddedBy");
                });

            modelBuilder.Entity("FaryBotHost.Database.FilmRating", b =>
                {
                    b.HasOne("FaryBotHost.Database.Participant", "Participant")
                        .WithMany("Ratings")
                        .HasForeignKey("ParticipantId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("FK_FILMS_RATINGS_PARTICIPANT_ID");

                    b.Navigation("Participant");
                });

            modelBuilder.Entity("FaryBotHost.Database.FilmVote", b =>
                {
                    b.HasOne("FaryBotHost.Database.Film", "Film")
                        .WithMany("Votes")
                        .HasForeignKey("FilmId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("FK_FILM_VOTE_FILM_ID");

                    b.HasOne("FaryBotHost.Database.Participant", "Participant")
                        .WithMany("Votes")
                        .HasForeignKey("ParticipantId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("FK_FILM_VOTE_PARTICIPANT_ID");

                    b.HasOne("FaryBotHost.Database.Session", "Session")
                        .WithMany("Votes")
                        .HasForeignKey("SessionId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired()
                        .HasConstraintName("FK_FILM_VOTE_SESSION_ID");

                    b.Navigation("Film");

                    b.Navigation("Participant");

                    b.Navigation("Session");
                });

            modelBuilder.Entity("FaryBotHost.Database.Session", b =>
                {
                    b.HasOne("FaryBotHost.Database.Film", "Film")
                        .WithOne("Session")
                        .HasForeignKey("FaryBotHost.Database.Session", "FilmId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .HasConstraintName("FK_FILM_SESSION_FILM_ID");

                    b.Navigation("Film");
                });

            modelBuilder.Entity("FaryBotHost.Database.Film", b =>
                {
                    b.Navigation("Session");

                    b.Navigation("Votes");
                });

            modelBuilder.Entity("FaryBotHost.Database.Participant", b =>
                {
                    b.Navigation("Films");

                    b.Navigation("Ratings");

                    b.Navigation("Votes");
                });

            modelBuilder.Entity("FaryBotHost.Database.Session", b =>
                {
                    b.Navigation("Votes");
                });
#pragma warning restore 612, 618
        }
    }
}
