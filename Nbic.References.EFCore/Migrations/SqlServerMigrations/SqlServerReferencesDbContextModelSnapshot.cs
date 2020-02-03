﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Nbic.References.EFCore;

namespace Nbic.References.EFCore.Migrations.SqlServerMigrations
{
    [DbContext(typeof(SqlServerReferencesDbContext))]
    partial class SqlServerReferencesDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.3-servicing-35854")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Nbic.References.Public.Models.Reference", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnName("Id")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int?>("ApplicationId")
                        .HasColumnName("ApplicationId")
                        .HasColumnType("int");

                    b.Property<string>("Author")
                        .HasColumnType("varchar(max)")
                        .IsUnicode(false);

                    b.Property<string>("Bibliography")
                        .HasColumnType("varchar(max)")
                        .IsUnicode(false);

                    b.Property<DateTime>("EditDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime")
                        .HasDefaultValueSql("GetUtcDate()");

                    b.Property<string>("Firstname")
                        .HasColumnType("varchar(max)")
                        .IsUnicode(false);

                    b.Property<string>("Journal")
                        .HasColumnType("varchar(max)")
                        .IsUnicode(false);

                    b.Property<string>("Keywords")
                        .HasColumnType("varchar(max)")
                        .IsUnicode(false);

                    b.Property<string>("Lastname")
                        .HasColumnType("varchar(max)")
                        .IsUnicode(false);

                    b.Property<string>("Middlename")
                        .HasColumnType("varchar(max)")
                        .IsUnicode(false);

                    b.Property<string>("Pages")
                        .HasColumnType("varchar(max)")
                        .IsUnicode(false);

                    b.Property<string>("ReferenceString")
                        .HasColumnName("ReferenceString")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Summary")
                        .HasColumnType("varchar(max)")
                        .IsUnicode(false);

                    b.Property<string>("Title")
                        .HasColumnType("varchar(max)")
                        .IsUnicode(false);

                    b.Property<string>("Url")
                        .HasColumnName("Url")
                        .HasColumnType("varchar(max)")
                        .IsUnicode(false);

                    b.Property<Guid>("UserId")
                        .HasColumnName("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Volume")
                        .HasColumnType("varchar(max)")
                        .IsUnicode(false);

                    b.Property<string>("Year")
                        .HasColumnType("varchar(max)")
                        .IsUnicode(false);

                    b.HasKey("Id")
                        .HasName("PK_Id");

                    b.HasIndex("UserId", "ApplicationId")
                        .HasName("IX_UserId_ApplicationId");

                    b.ToTable("Reference");
                });

            modelBuilder.Entity("Nbic.References.Public.Models.ReferenceUsage", b =>
                {
                    b.Property<Guid>("ReferenceId")
                        .HasColumnName("ReferenceId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("ApplicationId")
                        .HasColumnName("ApplicationId")
                        .HasColumnType("int");

                    b.Property<Guid>("UserId")
                        .HasColumnName("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("ReferenceId", "ApplicationId", "UserId");

                    b.ToTable("ReferenceUsage");
                });

            modelBuilder.Entity("Nbic.References.Public.Models.ReferenceUsage", b =>
                {
                    b.HasOne("Nbic.References.Public.Models.Reference", "Reference")
                        .WithMany("ReferenceUsage")
                        .HasForeignKey("ReferenceId")
                        .HasConstraintName("FK_ReferenceUsage_Reference")
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
