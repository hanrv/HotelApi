using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelApi.Models;

public class Habitacion
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "El número de habitación es obligatorio.")]
    [MaxLength(20)]
    public string Numero { get; set; } = string.Empty;

    [Required(ErrorMessage = "El tipo de habitación es obligatorio.")]
    [MaxLength(50)]
    public string Tipo { get; set; } = string.Empty; // Simple, Doble, Matrimonial, Suite

    [Required]
    [Range(0.01, 10000.00, ErrorMessage = "El precio por noche debe ser mayor a 0.")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal PrecioPorNoche { get; set; }

    [Range(1, 10, ErrorMessage = "La capacidad debe ser entre 1 y 10 personas.")]
    public int CapacidadPersonas { get; set; }

    public bool Disponible { get; set; } = true;

    // Clave foránea y navegación hacia Hotel
    [Required]
    public int HotelId { get; set; }
    public Hotel? Hotel { get; set; }

    // Relación con el detalle de las reservas
    public ICollection<ReservaDetalle> ReservaDetalles { get; set; } = new List<ReservaDetalle>();
}