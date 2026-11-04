using System.ComponentModel.DataAnnotations;

namespace HotelApi.Models;

public class Usuario
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre completo es obligatorio.")]
    [MaxLength(100)]
    public string NombreCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato de correo no es válido.")]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "El formato de teléfono no es válido.")]
    [MaxLength(20)]
    public string Telefono { get; set; } = string.Empty;

    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    // Relación 1 a N: Un usuario registra múltiples reservas
    public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}