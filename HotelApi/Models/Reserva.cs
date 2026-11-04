using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelApi.Models;

public enum EstadoReserva
{
    Pendiente = 0,
    Confirmada = 1,
    Cancelada = 2,
    Completada = 3
}

public class Reserva
{
    [Key]
    public int Id { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    [Required]
    public EstadoReserva Estado { get; set; } = EstadoReserva.Pendiente;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Total { get; set; }

    // Clave foránea y navegación de Usuario
    [Required]
    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    // Carrito de compras: Colección de detalles de habitaciones reservadas
    public ICollection<ReservaDetalle> ReservaDetalles { get; set; } = new List<ReservaDetalle>();

    // Colección de servicios adicionales asociados a la reserva
    public ICollection<ReservaServicio> ReservaServicios { get; set; } = new List<ReservaServicio>();
}