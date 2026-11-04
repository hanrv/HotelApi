using System.ComponentModel.DataAnnotations;
using HotelApi.Models;

namespace HotelApi.DTOs;

public record ItemCarritoHabitacionDTO(
    [Required] int HabitacionId,
    [Required] DateTime FechaEntrada,
    [Required] DateTime FechaSalida
);

public record ItemCarritoServicioDTO(
    [Required] int ServicioAdicionalId,
    [Range(1, 50, ErrorMessage = "La cantidad debe ser entre 1 y 50.")] int Cantidad
);

public record CrearReservaDTO(
    [Required] int UsuarioId,
    [Required, MinLength(1, ErrorMessage = "Debe agregar al menos una habitación a la reserva.")]
    List<ItemCarritoHabitacionDTO> Habitaciones,
    List<ItemCarritoServicioDTO>? Servicios
);

public record DetalleHabitacionDTO(
    int HabitacionId,
    string NumeroHabitacion,
    string Tipo,
    DateTime FechaEntrada,
    DateTime FechaSalida,
    decimal PrecioPorNoche,
    decimal Subtotal
);

public record DetalleServicioDTO(
    int ServicioAdicionalId,
    string NombreServicio,
    int Cantidad,
    decimal PrecioUnitario,
    decimal Subtotal
);

public record ReservaResponseDTO(
    int Id,
    int UsuarioId,
    string NombreUsuario,
    DateTime FechaCreacion,
    string Estado,
    decimal Total,
    List<DetalleHabitacionDTO> Habitaciones,
    List<DetalleServicioDTO> Servicios
);

public record ActualizarEstadoReservaDTO(
    [Required] EstadoReserva NuevoEstado
);