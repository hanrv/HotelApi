using System.ComponentModel.DataAnnotations;

namespace HotelApi.DTOs;

// DTO para registrar un detalle de habitación (similar al carrito de compras)
public class ReservaDetalleCreateDTO
{
    [Required(ErrorMessage = "El ID de la habitación es obligatorio.")]
    public int HabitacionId { get; set; }

    [Required]
    [Range(1, 365, ErrorMessage = "La cantidad de noches debe ser al menos 1.")]
    public int CantidadNoches { get; set; }
}

// DTO para servicios adicionales opcionales
public class ReservaServicioCreateDTO
{
    [Required]
    public int ServicioAdicionalId { get; set; }

    [Range(1, 50)]
    public int Cantidad { get; set; } = 1;
}

// DTO de entrada para crear una reserva
public class ReservaCreateDTO
{
    [Required(ErrorMessage = "El ID de usuario es obligatorio.")]
    public int UsuarioId { get; set; }

    [Required]
    public DateTime FechaInicio { get; set; }

    [Required]
    public DateTime FechaFin { get; set; }

    [Required(ErrorMessage = "Debe incluir al menos una habitación.")]
    [MinLength(1, ErrorMessage = "La reserva debe tener al menos un detalle de habitación.")]
    public List<ReservaDetalleCreateDTO> Detalles { get; set; } = new();

    public List<ReservaServicioCreateDTO> Servicios { get; set; } = new();
}

// DTO para actualización completa (PUT)
public class ReservaUpdateDTO
{
    [Required]
    public DateTime FechaInicio { get; set; }

    [Required]
    public DateTime FechaFin { get; set; }

    [Required]
    [RegularExpression("^(PENDIENTE|CONFIRMADA|CANCELADA|FINALIZADA)$", ErrorMessage = "Estado no válido.")]
    public string Estado { get; set; } = "PENDIENTE";
}

// DTO para actualización parcial de estado (PATCH)
public class ActualizarEstadoDTO
{
    [Required]
    [RegularExpression("^(PENDIENTE|CONFIRMADA|CANCELADA|FINALIZADA)$", ErrorMessage = "Estado no válido.")]
    public string NuevoEstado { get; set; } = string.Empty;
}

// DTO de lectura
public class ReservaLecturaDTO
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public DateTime FechaReserva { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
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
    public decimal PrecioNoche { get; set; }
    public int CantidadNoches { get; set; }
    public decimal Subtotal => PrecioNoche * CantidadNoches;
}

public class ReservaServicioLecturaDTO
{
    public int Id { get; set; }
    public int ServicioAdicionalId { get; set; }
    public string NombreServicio { get; set; } = string.Empty;
    public decimal PrecioUnitario { get; set; }
    public int Cantidad { get; set; }
    public decimal Subtotal => PrecioUnitario * Cantidad;
}