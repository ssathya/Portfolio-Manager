﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using PsqlAccess;

#nullable disable

namespace PsqlAccess.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20221213201014_EarningsCalendarAdded")]
    partial class EarningsCalendarAdded
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseSerialColumns(modelBuilder);

            modelBuilder.Entity("ApplicationModels.EarningsCal.EarningsCalendar", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseSerialColumn(b.Property<int>("Id"));

                    b.Property<bool>("DataObtained")
                        .HasColumnType("boolean");

                    b.Property<DateTime>("EarningsDateYahoo")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("EarningsReadDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("NextRefreshDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Ticker")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("VendorEarningsDate")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("earningsCalendars");
                });

            modelBuilder.Entity("ApplicationModels.Indexes.IndexComponent", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseSerialColumn(b.Property<int>("Id"));

                    b.Property<string>("CompanyName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("ListedInIndex")
                        .HasColumnType("integer");

                    b.Property<string>("Sector")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("SubSector")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Ticker")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("IndexComponents");
                });
#pragma warning restore 612, 618
        }
    }
}
