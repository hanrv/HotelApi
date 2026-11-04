using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelApi.Models;

public class ReservaServicio
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int ReservaId { get; set; }
    public Reserva? Reserva { get; set; }

    [Required]
    public int ServicioAdicionalId { get; set; }
    public ServicioAdicional? ServicioAdicional { get; set; }

    [Range(1, 100)]
    public int Cantidad { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal PrecioUnitario { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Subtotal { get; set; }
}