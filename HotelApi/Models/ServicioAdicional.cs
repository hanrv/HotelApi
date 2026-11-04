using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelApi.Models;

public class ServicioAdicional
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre del servicio es obligatorio.")]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty; // Spa, Desayuno Buffet, Transporte Aeropuerto

    [MaxLength(250)]
    public string Descripcion { get; set; } = string.Empty;

    [Range(0.01, 5000.00)]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Precio { get; set; }

    public ICollection<ReservaServicio> ReservaServicios { get; set; } = new List<ReservaServicio>();
}