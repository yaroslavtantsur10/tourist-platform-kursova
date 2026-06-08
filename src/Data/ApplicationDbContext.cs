using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TourRecommenderPlatform.Models;

namespace TourRecommenderPlatform.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser> {
  public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
      : base(options) {
  }

  public DbSet<Country> Countries { get; set; }

  public DbSet<Hotel> Hotels { get; set; }

  public DbSet<Tour> Tours { get; set; }

  public DbSet<Booking> Bookings { get; set; }

  public DbSet<Review> Reviews { get; set; }

  public DbSet<FavoriteTour> FavoriteTours { get; set; }

  public DbSet<ModelTrainingLog> ModelTrainingLogs { get; set; }

  protected override void OnModelCreating(ModelBuilder builder) {
    base.OnModelCreating(builder);

    ConfigureApplicationUser(builder);
    ConfigureCountry(builder);
    ConfigureHotel(builder);
    ConfigureTour(builder);
    ConfigureBooking(builder);
    ConfigureReview(builder);
    ConfigureFavoriteTour(builder);
    ConfigureModelTrainingLog(builder);
  }

  private static void ConfigureApplicationUser(ModelBuilder builder) {
    builder.Entity<ApplicationUser>(entity =>
    {
      entity.Property(u => u.FullName)
          .HasMaxLength(150);

      entity.Property(u => u.RegisteredAt)
          .HasDefaultValueSql("GETDATE()");

      entity.Property(u => u.IsBlocked)
          .HasDefaultValue(false);
    });
  }

  private static void ConfigureCountry(ModelBuilder builder) {
    builder.Entity<Country>(entity =>
    {
      entity.ToTable("Countries");
      entity.HasKey(c => c.Id);
      entity.Property(c => c.Name)
          .IsRequired()
          .HasMaxLength(100);
      entity.Property(c => c.Description)
          .HasMaxLength(1000);
      entity.HasIndex(c => c.Name)
          .IsUnique();
    });
  }

  private static void ConfigureHotel(ModelBuilder builder) {
    builder.Entity<Hotel>(entity =>
    {
      entity.ToTable("Hotels");

      entity.HasKey(h => h.Id);

      entity.Property(h => h.Name)
          .IsRequired()
          .HasMaxLength(150);

      entity.Property(h => h.Rating)
          .IsRequired();

      entity.Property(h => h.Description)
          .HasMaxLength(1000);

      entity.HasOne(h => h.Country)
          .WithMany(c => c.Hotels)
          .HasForeignKey(h => h.CountryId)
          .OnDelete(DeleteBehavior.Restrict);
    });
  }

  private static void ConfigureTour(ModelBuilder builder) {
    builder.Entity<Tour>(entity =>
    {
      entity.ToTable("Tours");

      entity.HasKey(t => t.Id);

      entity.Property(t => t.Title)
          .IsRequired()
          .HasMaxLength(200);

      entity.Property(t => t.Description)
          .IsRequired()
          .HasMaxLength(3000);

      entity.Property(t => t.Price)
          .HasColumnType("decimal(18,2)")
          .IsRequired();

      entity.Property(t => t.PhotoPath)
          .HasMaxLength(500);

      entity.Property(t => t.IsActive)
          .HasDefaultValue(true);

      entity.Property(t => t.CreatedAt)
          .HasDefaultValueSql("GETDATE()");

      entity.HasOne(t => t.Country)
          .WithMany(c => c.Tours)
          .HasForeignKey(t => t.CountryId)
          .OnDelete(DeleteBehavior.Restrict);

      entity.HasOne(t => t.Hotel)
          .WithMany(h => h.Tours)
          .HasForeignKey(t => t.HotelId)
          .OnDelete(DeleteBehavior.Restrict);
    });
  }

  private static void ConfigureBooking(ModelBuilder builder) {
    builder.Entity<Booking>(entity =>
    {
      entity.ToTable("Bookings");

      entity.HasKey(b => b.Id);

      entity.Property(b => b.UserId)
          .IsRequired();

      entity.Property(b => b.BookingDate)
          .HasDefaultValueSql("GETDATE()");

      entity.Property(b => b.Status)
          .IsRequired()
          .HasMaxLength(50);

      entity.HasOne(b => b.User)
          .WithMany(u => u.Bookings)
          .HasForeignKey(b => b.UserId)
          .OnDelete(DeleteBehavior.Restrict);

      entity.HasOne(b => b.Tour)
          .WithMany(t => t.Bookings)
          .HasForeignKey(b => b.TourId)
          .OnDelete(DeleteBehavior.Restrict);
    });
  }

  private static void ConfigureReview(ModelBuilder builder) {
    builder.Entity<Review>(entity => {
      entity.ToTable("Reviews");

      entity.HasKey(r => r.Id);

      entity.Property(r => r.UserId)
          .IsRequired();

      entity.Property(r => r.Rating)
          .IsRequired();

      entity.Property(r => r.Comment)
          .HasMaxLength(2000);

      entity.Property(r => r.CreatedAt)
          .HasDefaultValueSql("GETDATE()");

      entity.HasOne(r => r.User)
          .WithMany(u => u.Reviews)
          .HasForeignKey(r => r.UserId)
          .OnDelete(DeleteBehavior.Restrict);

      entity.HasOne(r => r.Tour)
          .WithMany(t => t.Reviews)
          .HasForeignKey(r => r.TourId)
          .OnDelete(DeleteBehavior.Restrict);

      entity.HasIndex(r => new { r.UserId, r.TourId })
          .IsUnique();
    });
  }

  private static void ConfigureFavoriteTour(ModelBuilder builder) {
    builder.Entity<FavoriteTour>(entity => {
      entity.ToTable("FavoriteTours");

      entity.HasKey(f => f.Id);

      entity.Property(f => f.UserId)
          .IsRequired();

      entity.Property(f => f.AddedAt)
          .HasDefaultValueSql("GETDATE()");

      entity.HasOne(f => f.User)
          .WithMany(u => u.FavoriteTours)
          .HasForeignKey(f => f.UserId)
          .OnDelete(DeleteBehavior.Restrict);

      entity.HasOne(f => f.Tour)
          .WithMany(t => t.FavoriteTours)
          .HasForeignKey(f => f.TourId)
          .OnDelete(DeleteBehavior.Restrict);

      entity.HasIndex(f => new { f.UserId, f.TourId })
          .IsUnique();
    });
  }

  private static void ConfigureModelTrainingLog(ModelBuilder builder) {
    builder.Entity<ModelTrainingLog>(entity =>
    {
      entity.ToTable("ModelTrainingLogs");

      entity.HasKey(m => m.Id);

      entity.Property(m => m.TrainingDate)
          .HasDefaultValueSql("GETDATE()");

      entity.Property(m => m.ModelPath)
          .HasMaxLength(500);

      entity.Property(m => m.Status)
          .IsRequired()
          .HasMaxLength(50);

      entity.Property(m => m.Message)
          .HasMaxLength(2000);

      entity.HasOne(m => m.User)
          .WithMany(u => u.ModelTrainingLogs)
          .HasForeignKey(m => m.UserId)
          .OnDelete(DeleteBehavior.Restrict);
    });
  }
}