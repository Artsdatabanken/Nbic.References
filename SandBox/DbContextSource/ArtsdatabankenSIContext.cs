using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;

namespace SandBox.DbContextSource
{
    public partial class ArtsdatabankenSIContext : DbContext
    {
        private string _connectionstring;

        public ArtsdatabankenSIContext(string connectionstring)
        {
            _connectionstring = connectionstring;
        }

        public ArtsdatabankenSIContext(DbContextOptions<ArtsdatabankenSIContext> options, string connectionstring)
            : base(options)
        {
            _connectionstring = connectionstring;
        }

        public virtual DbSet<RfReference> RfReference { get; set; }
        public virtual DbSet<RfReferenceUsage> RfReferenceUsage { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(_connectionstring);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.3-servicing-35854");

            modelBuilder.Entity<RfReference>(entity =>
            {
                entity.HasKey(e => e.PkReferenceId)
                    .HasName("PK_RF_ReferenceNew");

                entity.ToTable("RF_Reference");

                entity.HasIndex(e => new { e.FkUserId, e.ApplicationId })
                    .HasDatabaseName("NonClusteredIndex-20190323-220323");

                entity.Property(e => e.PkReferenceId)
                    .HasColumnName("PK_ReferenceID")
                    .ValueGeneratedNever();

                entity.Property(e => e.ApplicationId).HasColumnName("Application_ID");

                entity.Property(e => e.Author).IsUnicode(false);

                entity.Property(e => e.Bibliography).IsUnicode(false);

                entity.Property(e => e.EditDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Firstname).IsUnicode(false);

                entity.Property(e => e.FkUserId).HasColumnName("FK_UserID");

                entity.Property(e => e.ImportXml)
                    .HasColumnName("ImportXML")
                    .HasColumnType("xml");

                entity.Property(e => e.Journal).IsUnicode(false);

                entity.Property(e => e.Keywords).IsUnicode(false);

                entity.Property(e => e.Lastname).IsUnicode(false);

                entity.Property(e => e.Middlename).IsUnicode(false);

                entity.Property(e => e.Pages).IsUnicode(false);

                entity.Property(e => e.Summary).IsUnicode(false);

                entity.Property(e => e.Title).IsUnicode(false);

                entity.Property(e => e.Url)
                    .HasColumnName("URL")
                    .IsUnicode(false);

                entity.Property(e => e.Volume).IsUnicode(false);

                entity.Property(e => e.Year).IsUnicode(false);
            });

            modelBuilder.Entity<RfReferenceUsage>(entity =>
            {
                entity.HasKey(e => new { e.FkReferenceId, e.FkApplicationId, e.FkUserId });

                entity.ToTable("RF_Reference_Usage");

                entity.Property(e => e.FkReferenceId).HasColumnName("FK_ReferenceId");

                entity.Property(e => e.FkApplicationId).HasColumnName("FK_Application_ID");

                entity.Property(e => e.FkUserId).HasColumnName("FK_UserID");

                entity.HasOne(d => d.FkReference)
                    .WithMany(p => p.RfReferenceUsage)
                    .HasForeignKey(d => d.FkReferenceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_RF_Reference_Usage_RF_Reference");
            });
        }
    }
}
