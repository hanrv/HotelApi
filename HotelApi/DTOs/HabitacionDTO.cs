using System.ComponentModel.DataAnnotations;

namespace HotelApi.DTOs;

public record HabitacionCreateDTO(
    [Required, MaxLength(20)] string Numero,
    [Required, MaxLength(50)] string Tipo,
    [Range(0.01, 10000.00)] decimal PrecioPorNoche,
    [Range(1, 10)] int CapacidadPersonas,
    [Required] int HotelId
);

public record HabitacionResponseDTO(
    int Id,
    string Numero,
    string Tipo,
    decimal PrecioPorNoche,
    int CapacidadPersonas,
    bool Disponible,
    int HotelId,
    string NombreHotel
);