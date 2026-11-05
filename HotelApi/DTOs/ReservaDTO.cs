using System.ComponentModel.DataAnnotations;
using HotelApi.Models;

namespace HotelApi.DTOs;

public class ReservaDetalleCreateDTO
{
    [Required(ErrorMessage = "El ID de la habitación es obligatorio.")]
    public int HabitacionId { get; set; }

    [Required(ErrorMessage = "La fecha de entrada es obligatoria.")]
    public DateTime FechaEntrada { get; set; }

    [Required(ErrorMessage = "La fecha de salida es obligatoria.")]
    public DateTime FechaSalida { get; set; }
}

public class ReservaDetalleUpdateDTO
{
    public int Id { get; set; }

    [Required]
    public DateTime FechaEntrada { get; set; }

    [Required]
    public DateTime FechaSalida { get; set; }
}

public class ReservaServicioCreateDTO
{
    [Required]
    public int ServicioAdicionalId { get; set; }

    [Range(1, 50, ErrorMessage = "La cantidad debe ser entre 1 y 50.")]
    public int Cantidad { get; set; } = 1;
}

public class ReservaCreateDTO
{
    [Required(ErrorMessage = "El ID de usuario es obligatorio.")]
    public int UsuarioId { get; set; }

    [Required(ErrorMessage = "Debe incluir al menos una habitación.")]
    [MinLength(1, ErrorMessage = "La reserva debe tener al menos un detalle de habitación.")]
    public List<ReservaDetalleCreateDTO> Detalles { get; set; } = new();

    public List<ReservaServicioCreateDTO> Servicios { get; set; } = new();
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

public class ReservaLecturaDTO
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public DateTime FechaCreacion { get; set; }
    public string Estado { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public List<ReservaDetalleLecturaDTO> Detalles { get; set; } = new();
    public List<ReservaServicioLecturaDTO> Servicios { get; set; } = new();
}

public class ReservaDetalleLecturaDTO
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

public class ReservaServicioLecturaDTO
{
    public int Id { get; set; }
    public int ServicioAdicionalId { get; set; }
    public string NombreServicio { get; set; } = string.Empty;
    public decimal PrecioUnitario { get; set; }
    public int Cantidad { get; set; }
    public decimal Subtotal { get; set; }
}