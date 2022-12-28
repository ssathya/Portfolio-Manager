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
    [Migration("20221228124950_TechnicalAnalysis1")]
    partial class TechnicalAnalysis1
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseSerialColumns(modelBuilder);

            modelBuilder.Entity("ApplicationModels.Compute.Momentum", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseSerialColumn(b.Property<int>("Id"));

                    b.Property<decimal>("MomentumValue")
                        .HasColumnType("numeric");

                    b.Property<DateTime>("ReportingDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Ticker")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("Ticker");

                    b.ToTable("Momentums");
                });

            modelBuilder.Entity("ApplicationModels.EarningsCal.EarningsCalExceptions", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseSerialColumn(b.Property<int>("Id"));

                    b.Property<string>("AdditionalNotes")
                        .HasColumnType("text");

                    b.Property<int>("Exception")
                        .HasColumnType("integer");

                    b.Property<DateTime>("ReportingDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Ticker")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("EarningsCalExceptions");
                });

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

                    b.Property<DateTime>("RemoveDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Ticker")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("VendorEarningsDate")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("EarningsCalendars");
                });

            modelBuilder.Entity("ApplicationModels.FinancialStatement.FinStatements", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseSerialColumn(b.Property<int>("Id"));

                    b.Property<float?>("Assets")
                        .HasColumnType("real");

                    b.Property<float?>("Cash")
                        .HasColumnType("real");

                    b.Property<long?>("CommonStockSharesOutstanding")
                        .HasColumnType("bigint");

                    b.Property<float?>("CurrentAssets")
                        .HasColumnType("real");

                    b.Property<float?>("CurrentLiabilities")
                        .HasColumnType("real");

                    b.Property<float?>("DividendsPaid")
                        .HasColumnType("real");

                    b.Property<float?>("Equity")
                        .HasColumnType("real");

                    b.Property<string>("FilingType")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<float?>("FinancingCashFlows")
                        .HasColumnType("real");

                    b.Property<float?>("InvestingCashFlows")
                        .HasColumnType("real");

                    b.Property<float?>("Liabilities")
                        .HasColumnType("real");

                    b.Property<float?>("NetIncome")
                        .HasColumnType("real");

                    b.Property<float?>("OperatingCashFlows")
                        .HasColumnType("real");

                    b.Property<float?>("OperatingIncome")
                        .HasColumnType("real");

                    b.Property<float?>("PaymentsOfDebt")
                        .HasColumnType("real");

                    b.Property<DateTime?>("PeriodEnd")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("PeriodStart")
                        .HasColumnType("timestamp with time zone");

                    b.Property<float?>("ProceedsFromIssuanceOfDebt")
                        .HasColumnType("real");

                    b.Property<float?>("ResearchAndDevelopmentExpense")
                        .HasColumnType("real");

                    b.Property<float?>("RetainedEarnings")
                        .HasColumnType("real");

                    b.Property<float?>("Revenue")
                        .HasColumnType("real");

                    b.Property<float?>("SellingGeneralAndAdministrativeExpense")
                        .HasColumnType("real");

                    b.Property<string>("Ticker")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("FinStatements");
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

            modelBuilder.Entity("ApplicationModels.Quotes.YQuotes", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseSerialColumn(b.Property<int>("Id"));

                    b.Property<decimal>("Close")
                        .HasColumnType("numeric");

                    b.Property<DateTime>("Date")
                        .HasColumnType("timestamp with time zone");

                    b.Property<decimal>("High")
                        .HasColumnType("numeric");

                    b.Property<decimal>("Low")
                        .HasColumnType("numeric");

                    b.Property<decimal>("Open")
                        .HasColumnType("numeric");

                    b.Property<string>("Ticker")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<long>("Volume")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("Ticker");

                    b.ToTable("YQuotes");
                });
#pragma warning restore 612, 618
        }
    }
}
