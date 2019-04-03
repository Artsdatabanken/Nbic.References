﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Nbic.References.EFCore;

namespace Nbic.References.EFCore.Migrations.SqlServerMigrations
{
    [DbContext(typeof(SqlServerReferencesDbContext))]
    [Migration("20190324220236_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.3-servicing-35854")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Nbic.References.Public.Models.RfReference", b =>
                {
                    b.Property<Guid>("PkReferenceId")
                        .HasColumnName("PK_ReferenceID");

                    b.Property<int?>("ApplicationId")
                        .HasColumnName("Application_ID");

                    b.Property<string>("Author")
                        .IsUnicode(false);

                    b.Property<string>("Bibliography")
                        .IsUnicode(false);

                    b.Property<DateTime>("EditDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime")
                        .HasDefaultValueSql("(getdate())");

                    b.Property<string>("Firstname")
                        .IsUnicode(false);

                    b.Property<int?>("FkUserId")
                        .HasColumnName("FK_UserID");

                    b.Property<string>("ImportXml")
                        .HasColumnName("ImportXML")
                        .HasColumnType("xml");

                    b.Property<string>("Journal")
                        .IsUnicode(false);

                    b.Property<string>("Keywords")
                        .IsUnicode(false);

                    b.Property<string>("Lastname")
                        .IsUnicode(false);

                    b.Property<string>("Middlename")
                        .IsUnicode(false);

                    b.Property<string>("Pages")
                        .IsUnicode(false);

                    b.Property<string>("Summary")
                        .IsUnicode(false);

                    b.Property<string>("Title")
                        .IsUnicode(false);

                    b.Property<string>("Url")
                        .HasColumnName("URL")
                        .IsUnicode(false);

                    b.Property<string>("Volume")
                        .IsUnicode(false);

                    b.Property<string>("Year")
                        .IsUnicode(false);

                    b.HasKey("PkReferenceId")
                        .HasName("PK_RF_ReferenceNew");

                    b.HasIndex("FkUserId", "ApplicationId")
                        .HasName("NonClusteredIndex-20190323-220323");

                    b.ToTable("RF_Reference");
                });

            modelBuilder.Entity("Nbic.References.Public.Models.RfReferenceUsage", b =>
                {
                    b.Property<Guid>("FkReferenceId")
                        .HasColumnName("FK_ReferenceId");

                    b.Property<int>("FkApplicationId")
                        .HasColumnName("FK_Application_ID");

                    b.Property<int>("FkUserId")
                        .HasColumnName("FK_UserID");

                    b.HasKey("FkReferenceId", "FkApplicationId", "FkUserId");

                    b.ToTable("RF_Reference_Usage");
                });

            modelBuilder.Entity("Nbic.References.Public.Models.RfReferenceUsage", b =>
                {
                    b.HasOne("Nbic.References.Public.Models.RfReference", "FkReference")
                        .WithMany("RfReferenceUsage")
                        .HasForeignKey("FkReferenceId")
                        .HasConstraintName("FK_RF_Reference_Usage_RF_Reference");
                });
#pragma warning restore 612, 618
        }
    }
}
