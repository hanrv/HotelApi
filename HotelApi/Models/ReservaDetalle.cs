using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelApi.Models;

public class ReservaDetalle
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int ReservaId { get; set; }
    public Reserva? Reserva { get; set; }

    [Required]
    public int HabitacionId { get; set; }
    public Habitacion? Habitacion { get; set; }

    [Required]
    public DateTime FechaEntrada { get; set; }

    [Required]
    public DateTime FechaSalida { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal PrecioUnitarioPorNoche { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Subtotal { get; set; }
}