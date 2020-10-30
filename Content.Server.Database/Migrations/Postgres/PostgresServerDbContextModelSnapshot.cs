﻿// <auto-generated />
using System;
using System.Net;
using Content.Server.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Content.Server.Database.Migrations.Postgres
{
    [DbContext(typeof(PostgresServerDbContext))]
    partial class PostgresServerDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .HasAnnotation("ProductVersion", "3.1.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("Content.Server.Database.Admin", b =>
                {
                    b.Property<Guid>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("user_id")
                        .HasColumnType("uuid");

                    b.Property<int?>("AdminRankId")
                        .HasColumnName("admin_rank_id")
                        .HasColumnType("integer");

                    b.Property<string>("Title")
                        .HasColumnName("title")
                        .HasColumnType("text");

                    b.HasKey("UserId");

                    b.HasIndex("AdminRankId");

                    b.ToTable("admin");
                });

            modelBuilder.Entity("Content.Server.Database.AdminFlag", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("admin_flag_id")
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<Guid>("AdminId")
                        .HasColumnName("admin_id")
                        .HasColumnType("uuid");

                    b.Property<string>("Flag")
                        .IsRequired()
                        .HasColumnName("flag")
                        .HasColumnType("text");

                    b.Property<bool>("Negative")
                        .HasColumnName("negative")
                        .HasColumnType("boolean");

                    b.HasKey("Id");

                    b.HasIndex("AdminId");

                    b.ToTable("admin_flag");
                });

            modelBuilder.Entity("Content.Server.Database.AdminRank", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("admin_rank_id")
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnName("name")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("admin_rank");
                });

            modelBuilder.Entity("Content.Server.Database.AdminRankFlag", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("admin_rank_flag_id")
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<int>("AdminRankId")
                        .HasColumnName("admin_rank_id")
                        .HasColumnType("integer");

                    b.Property<string>("Flag")
                        .IsRequired()
                        .HasColumnName("flag")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("AdminRankId");

                    b.ToTable("admin_rank_flag");
                });

            modelBuilder.Entity("Content.Server.Database.Antag", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("antag_id")
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("AntagName")
                        .IsRequired()
                        .HasColumnName("antag_name")
                        .HasColumnType("text");

                    b.Property<int>("ProfileId")
                        .HasColumnName("profile_id")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("ProfileId", "AntagName")
                        .IsUnique();

                    b.ToTable("antag");
                });

            modelBuilder.Entity("Content.Server.Database.AssignedUserId", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("assigned_user_id_id")
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<Guid>("UserId")
                        .HasColumnName("user_id")
                        .HasColumnType("uuid");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasColumnName("user_name")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("UserId")
                        .IsUnique();

                    b.HasIndex("UserName")
                        .IsUnique();

                    b.ToTable("assigned_user_id");
                });

            modelBuilder.Entity("Content.Server.Database.Job", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("job_id")
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("JobName")
                        .IsRequired()
                        .HasColumnName("job_name")
                        .HasColumnType("text");

                    b.Property<int>("Priority")
                        .HasColumnName("priority")
                        .HasColumnType("integer");

                    b.Property<int>("ProfileId")
                        .HasColumnName("profile_id")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("ProfileId");

                    b.ToTable("job");
                });

            modelBuilder.Entity("Content.Server.Database.PostgresConnectionLog", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("connection_log_id")
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<IPAddress>("Address")
                        .IsRequired()
                        .HasColumnName("address")
                        .HasColumnType("inet");

                    b.Property<DateTime>("Time")
                        .HasColumnName("time")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("UserId")
                        .HasColumnName("user_id")
                        .HasColumnType("uuid");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasColumnName("user_name")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("connection_log");

                    b.HasCheckConstraint("AddressNotIPv6MappedIPv4", "NOT inet '::ffff:0.0.0.0/96' >>= address");
                });

            modelBuilder.Entity("Content.Server.Database.PostgresPlayer", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("player_id")
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<DateTime>("FirstSeenTime")
                        .HasColumnName("first_seen_time")
                        .HasColumnType("timestamp with time zone");

                    b.Property<IPAddress>("LastSeenAddress")
                        .IsRequired()
                        .HasColumnName("last_seen_address")
                        .HasColumnType("inet");

                    b.Property<DateTime>("LastSeenTime")
                        .HasColumnName("last_seen_time")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("LastSeenUserName")
                        .IsRequired()
                        .HasColumnName("last_seen_user_name")
                        .HasColumnType("text");

                    b.Property<Guid>("UserId")
                        .HasColumnName("user_id")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("UserId")
                        .IsUnique();

                    b.ToTable("player");

                    b.HasCheckConstraint("LastSeenAddressNotIPv6MappedIPv4", "NOT inet '::ffff:0.0.0.0/96' >>= last_seen_address");
                });

            modelBuilder.Entity("Content.Server.Database.PostgresServerBan", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("server_ban_id")
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<ValueTuple<IPAddress, int>?>("Address")
                        .HasColumnName("address")
                        .HasColumnType("inet");

                    b.Property<DateTime>("BanTime")
                        .HasColumnName("ban_time")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("BanningAdmin")
                        .HasColumnName("banning_admin")
                        .HasColumnType("uuid");

                    b.Property<DateTime?>("ExpirationTime")
                        .HasColumnName("expiration_time")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Reason")
                        .IsRequired()
                        .HasColumnName("reason")
                        .HasColumnType("text");

                    b.Property<Guid?>("UserId")
                        .HasColumnName("user_id")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("Address");

                    b.HasIndex("UserId");

                    b.ToTable("server_ban");

                    b.HasCheckConstraint("AddressNotIPv6MappedIPv4", "NOT inet '::ffff:0.0.0.0/96' >>= address");

                    b.HasCheckConstraint("HaveEitherAddressOrUserId", "address IS NOT NULL OR user_id IS NOT NULL");
                });

            modelBuilder.Entity("Content.Server.Database.PostgresServerUnban", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("unban_id")
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<int>("BanId")
                        .HasColumnName("ban_id")
                        .HasColumnType("integer");

                    b.Property<DateTime>("UnbanTime")
                        .HasColumnName("unban_time")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("UnbanningAdmin")
                        .HasColumnName("unbanning_admin")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("BanId")
                        .IsUnique();

                    b.ToTable("server_unban");
                });

            modelBuilder.Entity("Content.Server.Database.Preference", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("preference_id")
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<int>("SelectedCharacterSlot")
                        .HasColumnName("selected_character_slot")
                        .HasColumnType("integer");

                    b.Property<Guid>("UserId")
                        .HasColumnName("user_id")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("UserId")
                        .IsUnique();

                    b.ToTable("preference");
                });

            modelBuilder.Entity("Content.Server.Database.Profile", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("profile_id")
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<int>("Age")
                        .HasColumnName("age")
                        .HasColumnType("integer");

                    b.Property<string>("CharacterName")
                        .IsRequired()
                        .HasColumnName("char_name")
                        .HasColumnType("text");

                    b.Property<string>("EyeColor")
                        .IsRequired()
                        .HasColumnName("eye_color")
                        .HasColumnType("text");

                    b.Property<string>("FacialHairColor")
                        .IsRequired()
                        .HasColumnName("facial_hair_color")
                        .HasColumnType("text");

                    b.Property<string>("FacialHairName")
                        .IsRequired()
                        .HasColumnName("facial_hair_name")
                        .HasColumnType("text");

                    b.Property<string>("HairColor")
                        .IsRequired()
                        .HasColumnName("hair_color")
                        .HasColumnType("text");

                    b.Property<string>("HairName")
                        .IsRequired()
                        .HasColumnName("hair_name")
                        .HasColumnType("text");

                    b.Property<int>("PreferenceId")
                        .HasColumnName("preference_id")
                        .HasColumnType("integer");

                    b.Property<int>("PreferenceUnavailable")
                        .HasColumnName("pref_unavailable")
                        .HasColumnType("integer");

                    b.Property<string>("Sex")
                        .IsRequired()
                        .HasColumnName("sex")
                        .HasColumnType("text");

                    b.Property<string>("SkinColor")
                        .IsRequired()
                        .HasColumnName("skin_color")
                        .HasColumnType("text");

                    b.Property<int>("Slot")
                        .HasColumnName("slot")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("PreferenceId");

                    b.HasIndex("Slot", "PreferenceId")
                        .IsUnique();

                    b.ToTable("profile");
                });

            modelBuilder.Entity("Content.Server.Database.Admin", b =>
                {
                    b.HasOne("Content.Server.Database.AdminRank", "AdminRank")
                        .WithMany("Admins")
                        .HasForeignKey("AdminRankId")
                        .OnDelete(DeleteBehavior.SetNull);
                });

            modelBuilder.Entity("Content.Server.Database.AdminFlag", b =>
                {
                    b.HasOne("Content.Server.Database.Admin", "Admin")
                        .WithMany("Flags")
                        .HasForeignKey("AdminId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Content.Server.Database.AdminRankFlag", b =>
                {
                    b.HasOne("Content.Server.Database.AdminRank", "Rank")
                        .WithMany("Flags")
                        .HasForeignKey("AdminRankId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Content.Server.Database.Antag", b =>
                {
                    b.HasOne("Content.Server.Database.Profile", "Profile")
                        .WithMany("Antags")
                        .HasForeignKey("ProfileId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Content.Server.Database.Job", b =>
                {
                    b.HasOne("Content.Server.Database.Profile", "Profile")
                        .WithMany("Jobs")
                        .HasForeignKey("ProfileId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Content.Server.Database.PostgresServerUnban", b =>
                {
                    b.HasOne("Content.Server.Database.PostgresServerBan", "Ban")
                        .WithOne("Unban")
                        .HasForeignKey("Content.Server.Database.PostgresServerUnban", "BanId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Content.Server.Database.Profile", b =>
                {
                    b.HasOne("Content.Server.Database.Preference", "Preference")
                        .WithMany("Profiles")
                        .HasForeignKey("PreferenceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
