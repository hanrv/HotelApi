using Microsoft.EntityFrameworkCore;
using HotelApi.Models;

namespace HotelApi.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Hotel> Hoteles => Set<Hotel>();
    public DbSet<Habitacion> Habitaciones => Set<Habitacion>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Reserva> Reservas => Set<Reserva>();
    public DbSet<ReservaDetalle> ReservaDetalles => Set<ReservaDetalle>();
    public DbSet<ServicioAdicional> ServiciosAdicionales => Set<ServicioAdicional>();
    public DbSet<ReservaServicio> ReservaServicios => Set<ReservaServicio>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Hotel
        modelBuilder.Entity<Hotel>(entity =>
        {
            entity.HasKey(h => h.Id);
            entity.Property(h => h.Nombre).IsRequired().HasMaxLength(150);
            entity.Property(h => h.Direccion).IsRequired().HasMaxLength(250);
            entity.Property(h => h.Ciudad).IsRequired().HasMaxLength(100);
            entity.Property(h => h.Activo).HasDefaultValue(true);

            entity.HasMany(h => h.Habitaciones)
                  .WithOne(hab => hab.Hotel)
                  .HasForeignKey(hab => hab.HotelId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Habitacion
        modelBuilder.Entity<Habitacion>(entity =>
        {
            entity.HasKey(h => h.Id);
            entity.Property(h => h.Numero).IsRequired().HasMaxLength(20);
            entity.Property(h => h.Tipo).IsRequired().HasMaxLength(50);
            entity.Property(h => h.PrecioPorNoche).HasPrecision(18, 2);
            entity.Property(h => h.Disponible).HasDefaultValue(true);

            // Índice compuesto único para evitar números de habitación repetidos en el mismo hotel
            entity.HasIndex(h => new { h.HotelId, h.Numero }).IsUnique();
        });

        // Usuario
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.NombreCompleto).IsRequired().HasMaxLength(100);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(150);
            entity.HasIndex(u => u.Email).IsUnique();

            entity.HasMany(u => u.Reservas)
                  .WithOne(r => r.Usuario)
                  .HasForeignKey(r => r.UsuarioId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Reserva
        modelBuilder.Entity<Reserva>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Total).HasPrecision(18, 2);
            entity.Property(r => r.Estado).HasConversion<string>().HasMaxLength(30);

            // Borrado en cascada para ítems dependientes de la reserva
            entity.HasMany(r => r.ReservaDetalles)
                  .WithOne(rd => rd.Reserva)
                  .HasForeignKey(rd => rd.ReservaId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(r => r.ReservaServicios)
                  .WithOne(rs => rs.Reserva)
                  .HasForeignKey(rs => rs.ReservaId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ReservaDetalle
        modelBuilder.Entity<ReservaDetalle>(entity =>
        {
            entity.HasKey(rd => rd.Id);
            entity.Property(rd => rd.PrecioUnitarioPorNoche).HasPrecision(18, 2);
            entity.Property(rd => rd.Subtotal).HasPrecision(18, 2);

            entity.HasOne(rd => rd.Habitacion)
                  .WithMany(h => h.ReservaDetalles)
                  .HasForeignKey(rd => rd.HabitacionId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ServicioAdicional
        modelBuilder.Entity<ServicioAdicional>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Nombre).IsRequired().HasMaxLength(100);
            entity.Property(s => s.Precio).HasPrecision(18, 2);
        });

        // ReservaServicio
        modelBuilder.Entity<ReservaServicio>(entity =>
        {
            entity.HasKey(rs => rs.Id);
            entity.Property(rs => rs.PrecioUnitario).HasPrecision(18, 2);
            entity.Property(rs => rs.Subtotal).HasPrecision(18, 2);

            entity.HasOne(rs => rs.ServicioAdicional)
                  .WithMany(s => s.ReservaServicios)
                  .HasForeignKey(rs => rs.ServicioAdicionalId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}