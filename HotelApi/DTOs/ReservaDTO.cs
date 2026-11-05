using System.ComponentModel.DataAnnotations;
using HotelApi.Models;

namespace HotelApi.DTOs;

// =========================================================================
// 1. DTOs de Entrada (Creación y Modificación)
// =========================================================================

public class CrearReservaHabitacionDTO
{
    [Required]
    public int HabitacionId { get; set; }

    [Required]
    public DateTime FechaEntrada { get; set; }

    [Required]
    public DateTime FechaSalida { get; set; }
}

public class CrearReservaServicioDTO
{
    [Required]
    public int ServicioAdicionalId { get; set; }

    [Range(1, 50)]
    public int Cantidad { get; set; } = 1;
}

// DTO de entrada compatible con el repositorio y el controlador
public class CrearReservaDTO
{
    [Required]
    public int UsuarioId { get; set; }

    // Propiedad 'Habitaciones' requerida por ReservaRepository.cs
    public List<CrearReservaHabitacionDTO> Habitaciones { get; set; } = new();

    // Alias 'Detalles' para compatibilidad con el controlador
    public List<CrearReservaHabitacionDTO> Detalles
    {
        get => Habitaciones;
        set => Habitaciones = value;
    }

    public List<CrearReservaServicioDTO> Servicios { get; set; } = new();
}

public class ReservaDetalleCreateDTO : CrearReservaHabitacionDTO { }
public class ReservaServicioCreateDTO : CrearReservaServicioDTO { }
public class ReservaCreateDTO : CrearReservaDTO { }

public class ReservaDetalleUpdateDTO
{
    public int Id { get; set; }
    public DateTime FechaEntrada { get; set; }
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

// =========================================================================
// 2. DTOs de Salida (Lectura y Proyección)
// =========================================================================

public class DetalleHabitacionDTO
{
    public int Id { get; set; }
    public int HabitacionId { get; set; }
    public string NumeroHabitacion { get; set; } = string.Empty;
    public DateTime FechaEntrada { get; set; }
    public DateTime FechaSalida { get; set; }
    public decimal PrecioUnitarioPorNoche { get; set; }
    public decimal Subtotal { get; set; }

    public DetalleHabitacionDTO() { }

    public DetalleHabitacionDTO(int id, int habitacionId, string numeroHabitacion, DateTime entrada, DateTime salida, decimal precio, decimal subtotal)
    {
        Id = id;
        HabitacionId = habitacionId;
        NumeroHabitacion = numeroHabitacion;
        FechaEntrada = entrada;
        FechaSalida = salida;
        PrecioUnitarioPorNoche = precio;
        Subtotal = subtotal;
    }
}

public class DetalleServicioDTO
{
    public int Id { get; set; }
    public int ServicioAdicionalId { get; set; }
    public string NombreServicio { get; set; } = string.Empty;
    public decimal PrecioUnitario { get; set; }
    public int Cantidad { get; set; }
    public decimal Subtotal { get; set; }

    public DetalleServicioDTO() { }

    public DetalleServicioDTO(int id, int servicioId, string nombre, decimal precio, int cantidad, decimal subtotal)
    {
        Id = id;
        ServicioAdicionalId = servicioId;
        NombreServicio = nombre;
        PrecioUnitario = precio;
        Cantidad = cantidad;
        Subtotal = subtotal;
    }
}

public class ReservaDetalleLecturaDTO : DetalleHabitacionDTO { }
public class ReservaServicioLecturaDTO : DetalleServicioDTO { }

// DTO principal con constructor de 8 parámetros para ReservaRepository.cs
public class ReservaResponseDTO
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string UsuarioNombre { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public EstadoReserva Estado { get; set; }
    public decimal Total { get; set; }
    public List<DetalleHabitacionDTO> Habitaciones { get; set; } = new();
    public List<DetalleServicioDTO> Servicios { get; set; } = new();

    // Alias para el controlador
    public List<DetalleHabitacionDTO> Detalles => Habitaciones;

    public ReservaResponseDTO() { }

    // Constructor de 8 argumentos requerido en las líneas 21 y 54
    public ReservaResponseDTO(
        int id,
        int usuarioId,
        string usuarioNombre,
        DateTime fechaCreacion,
        EstadoReserva estado,
        decimal total,
        List<DetalleHabitacionDTO> habitaciones,
        List<DetalleServicioDTO> servicios)
    {
        Id = id;
        UsuarioId = usuarioId;
        UsuarioNombre = usuarioNombre;
        FechaCreacion = fechaCreacion;
        Estado = estado;
        Total = total;
        Habitaciones = habitaciones;
        Servicios = servicios;
    }
}

public class ReservaLecturaDTO : ReservaResponseDTO { }