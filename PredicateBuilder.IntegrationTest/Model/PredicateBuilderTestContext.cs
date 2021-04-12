using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace PredicateBuilder.IntegrationTest.Model
{
    public partial class PredicateBuilderTestContext : DbContext
    {
        public PredicateBuilderTestContext()
        {
        }

        public PredicateBuilderTestContext(DbContextOptions<PredicateBuilderTestContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Address> Addresses { get; set; }
        public virtual DbSet<Person> People { get; set; }
        public virtual DbSet<PhoneNumber> PhoneNumbers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Server=.;Database=PredicateBuilderTest;Trusted_Connection=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<Address>(entity =>
            {
                entity.ToTable("Address");

                entity.Property(e => e.City).HasMaxLength(100);

                entity.Property(e => e.IsocountryCode)
                    .HasMaxLength(5)
                    .HasColumnName("ISOCountryCode");

                entity.Property(e => e.IsocountrySubdivisionLevel2Code)
                    .HasMaxLength(10)
                    .HasColumnName("ISOCountrySubdivisionLevel2Code");

                entity.Property(e => e.PostalCode).HasMaxLength(20);

                entity.Property(e => e.StreetAddress1).HasMaxLength(200);

                entity.Property(e => e.StreetAddress2).HasMaxLength(200);

                entity.HasOne(d => d.Person)
                    .WithMany(p => p.Addresses)
                    .HasForeignKey(d => d.PersonId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<Person>(entity =>
            {
                entity.ToTable("Person");

                entity.Property(e => e.FirstName).HasMaxLength(100);

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<PhoneNumber>(entity =>
            {
                entity.ToTable("PhoneNumber");

                entity.Property(e => e.CountryCallingCode).HasMaxLength(10);

                entity.Property(e => e.Extension).HasMaxLength(10);

                entity.Property(e => e.Number)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.HasOne(d => d.Person)
                    .WithMany(p => p.PhoneNumbers)
                    .HasForeignKey(d => d.PersonId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
