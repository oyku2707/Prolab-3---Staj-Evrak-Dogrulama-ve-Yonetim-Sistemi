using Microsoft.EntityFrameworkCore;
using StajSistemi.Domain.Entities;

namespace StajSistemi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Admin> Admins => Set<Admin>();
    public DbSet<StudentProfile> StudentProfiles => Set<StudentProfile>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<ValidationResultRecord> ValidationResults => Set<ValidationResultRecord>();
    public DbSet<LoginAttempt> LoginAttempts => Set<LoginAttempt>();
    public DbSet<InternshipDaySelection> InternshipDaySelections => Set<InternshipDaySelection>();
    public DbSet<Holiday> Holidays => Set<Holiday>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Student ile StudentProfile: bire bir ilişki (StudentProfile.StudentId yabancı anahtar)
        modelBuilder.Entity<Student>()
            .HasOne(s => s.Profile)
            .WithOne()
            .HasForeignKey<StudentProfile>(p => p.StudentId);

        // Belgenin sahibi öğrenci: bir öğrencinin birden çok belgesi olabilir
        modelBuilder.Entity<Document>()
            .HasOne<Student>()
            .WithMany()
            .HasForeignKey(d => d.StudentId);

        // Ek16Data ayrı tablo olmasın, Documents tablosunun sütunları olarak gömülsün
        modelBuilder.Entity<Document>()
            .OwnsOne(d => d.ExtractedData);
         modelBuilder.Entity<Document>()
            .OwnsOne(d => d.EnteredData);
    }
}