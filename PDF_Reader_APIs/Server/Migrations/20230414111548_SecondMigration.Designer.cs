﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PDF_Reader_APIs.Server;

#nullable disable

namespace PDF_Reader_APIs.Server.Migrations
{
    [DbContext(typeof(Database))]
    [Migration("20230414111548_SecondMigration")]
    partial class SecondMigration
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("PDF_Reader_APIs.Shared.Entities.PDF", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("id"));

                    b.Property<string>("FileLink")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<double>("FileSize")
                        .HasColumnType("float");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("NumberOfPages")
                        .HasColumnType("int");

                    b.Property<string>("SentencesLinkTxt")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("TimeOfUpload")
                        .HasColumnType("datetime2");

                    b.HasKey("id");

                    b.ToTable("PDFs");
                });

            modelBuilder.Entity("PDF_Reader_APIs.Shared.Entities.Sentences", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("id"));

                    b.Property<int>("PDFid")
                        .HasColumnType("int");

                    b.Property<string>("Sentence")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("id");

                    b.HasIndex("PDFid");

                    b.ToTable("Sentences");
                });

            modelBuilder.Entity("PDF_Reader_APIs.Shared.Entities.Sentences", b =>
                {
                    b.HasOne("PDF_Reader_APIs.Shared.Entities.PDF", "PDF")
                        .WithMany("Sentences")
                        .HasForeignKey("PDFid")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("PDF");
                });

            modelBuilder.Entity("PDF_Reader_APIs.Shared.Entities.PDF", b =>
                {
                    b.Navigation("Sentences");
                });
#pragma warning restore 612, 618
        }
    }
}