using Microsoft.EntityFrameworkCore;
using systemforstudent.Models;

namespace systemforstudent.Persistence;

public class UniversityDbContext : DbContext
{
    public DbSet<Student> Students { get; set; } = null!;
    public DbSet<Teacher> Teachers { get; set; } = null!;
    public DbSet<Subject> Subjects { get; set; } = null!;
    public DbSet<Grade> Grades { get; set; } = null!;
    public DbSet<Department> Departments { get; set; } = null!;
    public DbSet<AcademicGroup> AcademicGroups { get; set; } = null!;
    public DbSet<AccountLink> AccountLinks { get; set; } = null!;

    private readonly string _connectionString;

    public UniversityDbContext(string connectionString = "Data Source=university.db")
    {
        _connectionString = connectionString;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite(_connectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<AccountLink>().HasKey(a => a.Username);
        modelBuilder.Entity<AccountLink>().Property(a => a.Username).UseCollation("NOCASE");
        modelBuilder.Entity<AcademicGroup>().HasIndex(g => g.Name).IsUnique();

        // Зв'язки many-to-many
        modelBuilder.Entity<Student>()
            .HasMany(s => s.Subjects)
            .WithMany();

        modelBuilder.Entity<Teacher>()
            .HasMany(t => t.Subjects)
            .WithMany();

        // Кафедра -> викладачі
        modelBuilder.Entity<Department>()
            .HasMany(d => d.Teachers)
            .WithOne();

        // Оцінка -> Студент, Предмет, Викладач
        modelBuilder.Entity<Grade>()
            .HasOne(g => g.Student)
            .WithMany();

        modelBuilder.Entity<Grade>()
            .HasOne(g => g.Subject)
            .WithMany();

        modelBuilder.Entity<Grade>()
            .HasOne(g => g.Teacher)
            .WithMany();
    }
}
