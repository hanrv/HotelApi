using System.ComponentModel.DataAnnotations;
using HotelApi.Models;

namespace HotelApi.DTOs;

public class DetalleHabitacionCreateDTO
{
    [Required(ErrorMessage = "El ID de la habitación es obligatorio.")]
    public int HabitacionId { get; set; }

    [Required(ErrorMessage = "La fecha de entrada es obligatoria.")]
    public DateTime FechaEntrada { get; set; }

    [Required(ErrorMessage = "La fecha de salida es obligatoria.")]
    public DateTime FechaSalida { get; set; }
}

public class DetalleServicioCreateDTO
{
    [Required(ErrorMessage = "El ID del servicio adicional es obligatorio.")]
    public int ServicioAdicionalId { get; set; }

    [Range(1, 50, ErrorMessage = "La cantidad debe estar entre 1 y 50.")]
    public int Cantidad { get; set; } = 1;
}

public class CrearReservaDTO
{
    [Required(ErrorMessage = "El ID del usuario es obligatorio.")]
    public int UsuarioId { get; set; }

    [Required(ErrorMessage = "Debe incluir al menos una habitación.")]
    [MinLength(1, ErrorMessage = "La reserva debe tener al menos una habitación.")]
    public List<DetalleHabitacionCreateDTO> Habitaciones { get; set; } = new();

    public List<DetalleServicioCreateDTO> Servicios { get; set; } = new();
}

public class ReservaDetalleUpdateDTO
{
    [Required]
    public int Id { get; set; }

    [Required]
    public DateTime FechaEntrada { get; set; }

    [Required]
    public DateTime FechaSalida { get; set; }
}

public class ReservaUpdateDTO
{
    [Required]
    public EstadoReserva Estado { get; set; }

    public List<ReservaDetalleUpdateDTO> Detalles { get; set; } = new();
}

public class ActualizarEstadoDTO
{
    [Required]
    public EstadoReserva NuevoEstado { get; set; }
}

public class DetalleHabitacionDTO
{
    public int Id { get; set; }
    public int HabitacionId { get; set; }
    public string NumeroHabitacion { get; set; } = string.Empty;
    public DateTime FechaEntrada { get; set; }
    public DateTime FechaSalida { get; set; }
    public int CantidadNoches => (FechaSalida - FechaEntrada).Days > 0 ? (FechaSalida - FechaEntrada).Days : 1;
    public decimal PrecioUnitarioPorNoche { get; set; }
    public decimal Subtotal { get; set; }
}

public class DetalleServicioDTO
{
    public int Id { get; set; }
    public int ServicioAdicionalId { get; set; }
    public string NombreServicio { get; set; } = string.Empty;
    public decimal PrecioUnitario { get; set; }
    public int Cantidad { get; set; }
    public decimal Subtotal { get; set; }
}

public class ReservaResponseDTO
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public DateTime FechaCreacion { get; set; }
    public string Estado { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public List<DetalleHabitacionDTO> Habitaciones { get; set; } = new();
    public List<DetalleServicioDTO> Servicios { get; set; } = new();
}