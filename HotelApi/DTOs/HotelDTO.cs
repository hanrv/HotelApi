using System.ComponentModel.DataAnnotations;

namespace HotelApi.DTOs;

public record HotelCreateUpdateDTO(
    [Required(ErrorMessage = "El nombre es obligatorio."), MaxLength(150)] string Nombre,
    [Required(ErrorMessage = "La dirección es obligatoria."), MaxLength(250)] string Direccion,
    [Required(ErrorMessage = "La ciudad es obligatoria."), MaxLength(100)] string Ciudad,
    [Range(1, 5, ErrorMessage = "Las estrellas deben ser entre 1 y 5.")] int Estrellas
);

public record HotelResponseDTO(
    int Id,
    string Nombre,
    string Direccion,
    string Ciudad,
    int Estrellas,
    bool Activo,
    int CantidadHabitaciones
);