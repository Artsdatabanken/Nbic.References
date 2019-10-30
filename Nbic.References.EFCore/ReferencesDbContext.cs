using Microsoft.EntityFrameworkCore;
using Nbic.References.Public.Models;

namespace Nbic.References.EFCore
{
    public partial class ReferencesDbContext : DbContext
    {
        public ReferencesDbContext()
        {
        }

        public ReferencesDbContext(DbContextOptions<ReferencesDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Reference> Reference { get; set; }
        public virtual DbSet<ReferenceUsage> ReferenceUsage { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.3-servicing-35854");

            modelBuilder.Entity<Reference>(entity =>
            {
                entity.HasKey(e => e.Id)
                    .HasName("PK_Id");

                entity.ToTable("Reference");

                entity.HasIndex(e => new { e.UserId, e.ApplicationId })
                    .HasName("IX_UserId_ApplicationId");

                entity.Property(e => e.Id)
                    .HasColumnName("Id")
                    .ValueGeneratedNever();

                entity.Property(e => e.ApplicationId).HasColumnName("ApplicationId");

                entity.Property(e => e.Author).IsUnicode(false);

                entity.Property(e => e.Bibliography).IsUnicode(false);
                if (this.Database.IsSqlite())
                {
                    entity.Property(e => e.EditDate)
                        .HasColumnType("datetime")
                        .HasDefaultValueSql("date('now')");
                }
                else
                {
                    entity.Property(e => e.EditDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("GetUtcDate()");
                }

                entity.Property(e => e.Firstname).IsUnicode(false);

                entity.Property(e => e.UserId).HasColumnName("UserId");

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
                    .HasColumnName("Url")
                    .IsUnicode(false);

                entity.Property(e => e.Volume).IsUnicode(false);

                entity.Property(e => e.Year).IsUnicode(false);
            });

            modelBuilder.Entity<ReferenceUsage>(entity =>
            {
                entity.HasKey(e => new { e.ReferenceId, e.ApplicationId, e.UserId });

                entity.ToTable("ReferenceUsage");

                entity.Property(e => e.ReferenceId).HasColumnName("ReferenceId");

                entity.Property(e => e.ApplicationId).HasColumnName("ApplicationId");

                entity.Property(e => e.UserId).HasColumnName("UserId");

                entity.HasOne(d => d.Reference)
                    .WithMany(p => p.ReferenceUsage)
                    .HasForeignKey(d => d.ReferenceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ReferenceUsage_Reference");
            });
        }

    }

    public class SqliteReferencesDbContext : ReferencesDbContext
    {
        private readonly string _dbConnectionString = "DataSource=:memory:";

        public SqliteReferencesDbContext(string dbConnectionString)
        {
            _dbConnectionString = dbConnectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite(_dbConnectionString);
    }
    public class SqlServerReferencesDbContext : ReferencesDbContext
    {
        private readonly string _dbConnectionString;

        public SqlServerReferencesDbContext(string dbConnectionString)
        {
            _dbConnectionString = dbConnectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlServer(_dbConnectionString);
    }
}
