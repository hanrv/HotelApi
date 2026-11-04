using System.ComponentModel.DataAnnotations;

namespace HotelApi.Models;

public class Hotel
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre del hotel es obligatorio.")]
    [MaxLength(150, ErrorMessage = "El nombre no puede superar los 150 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "La dirección es obligatoria.")]
    [MaxLength(250)]
    public string Direccion { get; set; } = string.Empty;

    [Required(ErrorMessage = "La ciudad es obligatoria.")]
    [MaxLength(100)]
    public string Ciudad { get; set; } = string.Empty;

    [Range(1, 5, ErrorMessage = "La calificación debe estar entre 1 y 5 estrellas.")]
    public int Estrellas { get; set; }

    public bool Activo { get; set; } = true;

    // Relación 1 a N: Un hotel posee múltiples habitaciones
    public ICollection<Habitacion> Habitaciones { get; set; } = new List<Habitacion>();
}