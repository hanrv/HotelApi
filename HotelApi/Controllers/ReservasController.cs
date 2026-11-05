using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelApi.Data;
using HotelApi.DTOs;
using HotelApi.Models;

namespace HotelApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservasController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ReservasController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/Reservas (Lectura optimizada con AsNoTracking y carga Include)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReservaLecturaDTO>>> GetReservas()
    {
        var reservas = await _context.Reservas
            .AsNoTracking()
            .Include(r => r.ReservaDetalles)
                .ThenInclude(d => d.Habitacion)
            .Include(r => r.ReservaServicios)
                .ThenInclude(rs => rs.ServicioAdicional)
            .Select(r => new ReservaLecturaDTO
            {
                Id = r.Id,
                UsuarioId = r.UsuarioId,
                FechaCreacion = r.FechaCreacion,
                Estado = r.Estado.ToString(),
                Total = r.Total,
                Detalles = r.ReservaDetalles.Select(d => new ReservaDetalleLecturaDTO
                {
                    Id = d.Id,
                    HabitacionId = d.HabitacionId,
                    NumeroHabitacion = d.Habitacion != null ? d.Habitacion.Numero : "N/A",
                    FechaEntrada = d.FechaEntrada,
                    FechaSalida = d.FechaSalida,
                    PrecioUnitarioPorNoche = d.PrecioUnitarioPorNoche,
                    Subtotal = d.Subtotal
                }).ToList(),
                Servicios = r.ReservaServicios.Select(s => new ReservaServicioLecturaDTO
                {
                    Id = s.Id,
                    ServicioAdicionalId = s.ServicioAdicionalId,
                    NombreServicio = s.ServicioAdicional != null ? s.ServicioAdicional.Nombre : "N/A",
                    PrecioUnitario = s.PrecioUnitario,
                    Cantidad = s.Cantidad,
                    Subtotal = s.Subtotal
                }).ToList()
            })
            .ToListAsync();

        return Ok(reservas);
    }

    // GET: api/Reservas/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ReservaLecturaDTO>> GetReservaPorId(int id)
    {
        var reserva = await _context.Reservas
            .AsNoTracking()
            .Include(r => r.ReservaDetalles)
                .ThenInclude(d => d.Habitacion)
            .Include(r => r.ReservaServicios)
                .ThenInclude(rs => rs.ServicioAdicional)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (reserva is null)
            return NotFound(new { Mensaje = $"La reserva con ID {id} no fue encontrada." });

        var dto = new ReservaLecturaDTO
        {
            Id = reserva.Id,
            UsuarioId = reserva.UsuarioId,
            FechaCreacion = reserva.FechaCreacion,
            Estado = reserva.Estado.ToString(),
            Total = reserva.Total,
            Detalles = reserva.ReservaDetalles.Select(d => new ReservaDetalleLecturaDTO
            {
                Id = d.Id,
                HabitacionId = d.HabitacionId,
                NumeroHabitacion = d.Habitacion != null ? d.Habitacion.Numero : "N/A",
                FechaEntrada = d.FechaEntrada,
                FechaSalida = d.FechaSalida,
                PrecioUnitarioPorNoche = d.PrecioUnitarioPorNoche,
                Subtotal = d.Subtotal
            }).ToList(),
            Servicios = reserva.ReservaServicios.Select(s => new ReservaServicioLecturaDTO
            {
                Id = s.Id,
                ServicioAdicionalId = s.ServicioAdicionalId,
                NombreServicio = s.ServicioAdicional != null ? s.ServicioAdicional.Nombre : "N/A",
                PrecioUnitario = s.PrecioUnitario,
                Cantidad = s.Cantidad,
                Subtotal = s.Subtotal
            }).ToList()
        };

        return Ok(dto);
    }

    // POST: api/Reservas (Creación tipo carrito con validación de noches)
    [HttpPost]
    public async Task<ActionResult> CrearReserva([FromBody] ReservaCreateDTO dto)
    {
        var usuarioExiste = await _context.Usuarios.AnyAsync(u => u.Id == dto.UsuarioId);
        if (!usuarioExiste)
            return BadRequest(new { Mensaje = $"El usuario con ID {dto.UsuarioId} no existe." });

        decimal totalCalculado = 0;

        var nuevaReserva = new Reserva
        {
            UsuarioId = dto.UsuarioId,
            FechaCreacion = DateTime.UtcNow,
            Estado = EstadoReserva.Confirmada,
            ReservaDetalles = new List<ReservaDetalle>(),
            ReservaServicios = new List<ReservaServicio>()
        };

        foreach (var detalleDto in dto.Detalles)
        {
            if (detalleDto.FechaSalida <= detalleDto.FechaEntrada)
            {
                return BadRequest(new
                {
                    Mensaje = $"La fecha de salida debe ser posterior a la fecha de entrada para la habitación {detalleDto.HabitacionId}."
                });
            }

            var habitacion = await _context.Habitaciones.FindAsync(detalleDto.HabitacionId);
            if (habitacion is null)
                return BadRequest(new { Mensaje = $"La habitación con ID {detalleDto.HabitacionId} no existe." });

            int noches = (detalleDto.FechaSalida - detalleDto.FechaEntrada).Days;
            if (noches <= 0) noches = 1;

            // Si en Habitacion.cs se llama PrecioPorNoche:
            decimal subtotalHabitacion = habitacion.PrecioPorNoche * noches;
            var detalle = new ReservaDetalle
            {
                HabitacionId = habitacion.Id,
                FechaEntrada = detalleDto.FechaEntrada,
                FechaSalida = detalleDto.FechaSalida,
                PrecioUnitarioPorNoche = habitacion.PrecioPorNoche,
                Subtotal = subtotalHabitacion
            };

            totalCalculado += subtotalHabitacion;
            nuevaReserva.ReservaDetalles.Add(detalle);
        }

        if (dto.Servicios != null && dto.Servicios.Any())
        {
            foreach (var servicioDto in dto.Servicios)
            {
                var servicio = await _context.ServiciosAdicionales.FindAsync(servicioDto.ServicioAdicionalId);
                if (servicio is null)
                    return BadRequest(new { Mensaje = $"El servicio adicional con ID {servicioDto.ServicioAdicionalId} no existe." });

                decimal subtotalServicio = servicio.Precio * servicioDto.Cantidad;

                var reservaServicio = new ReservaServicio
                {
                    ServicioAdicionalId = servicio.Id,
                    PrecioUnitario = servicio.Precio,
                    Cantidad = servicioDto.Cantidad,
                    Subtotal = subtotalServicio
                };

                totalCalculado += subtotalServicio;
                nuevaReserva.ReservaServicios.Add(reservaServicio);
            }
        }

        nuevaReserva.Total = totalCalculado;

        _context.Reservas.Add(nuevaReserva);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetReservaPorId),
            new { id = nuevaReserva.Id },
            new { Mensaje = "Reserva creada exitosamente.", ReservaId = nuevaReserva.Id, Total = totalCalculado }
        );
    }

    // PUT: api/Reservas/5 (Actualización total y recálculo)
    [HttpPut("{id:int}")]
    public async Task<IActionResult> ActualizarReserva(int id, [FromBody] ReservaUpdateDTO dto)
    {
        var reserva = await _context.Reservas
            .Include(r => r.ReservaDetalles)
            .Include(r => r.ReservaServicios)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (reserva is null)
            return NotFound(new { Mensaje = $"Reserva con ID {id} no encontrada." });

        reserva.Estado = dto.Estado;

        if (dto.Detalles != null && dto.Detalles.Any())
        {
            foreach (var detDto in dto.Detalles)
            {
                var detalleExistente = reserva.ReservaDetalles.FirstOrDefault(d => d.Id == detDto.Id);
                if (detalleExistente != null)
                {
                    if (detDto.FechaSalida <= detDto.FechaEntrada)
                        return BadRequest(new { Mensaje = "La fecha de salida debe ser posterior a la fecha de entrada." });

                    detalleExistente.FechaEntrada = detDto.FechaEntrada;
                    detalleExistente.FechaSalida = detDto.FechaSalida;

                    int noches = (detDto.FechaSalida - detDto.FechaEntrada).Days;
                    if (noches <= 0) noches = 1;

                    detalleExistente.Subtotal = detalleExistente.PrecioUnitarioPorNoche * noches;
                }
            }
        }

        decimal totalHabitaciones = reserva.ReservaDetalles.Sum(d => d.Subtotal);
        decimal totalServicios = reserva.ReservaServicios.Sum(s => s.Subtotal);
        reserva.Total = totalHabitaciones + totalServicios;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // PATCH: api/Reservas/5/estado (Actualización parcial de enum)
    [HttpPatch("{id:int}/estado")]
    public async Task<IActionResult> CambiarEstadoReserva(int id, [FromBody] ActualizarEstadoDTO dto)
    {
        var reserva = await _context.Reservas.FindAsync(id);
        if (reserva is null)
            return NotFound(new { Mensaje = $"Reserva con ID {id} no encontrada." });

        reserva.Estado = dto.NuevoEstado;
        await _context.SaveChangesAsync();

        return Ok(new { Mensaje = $"Estado actualizado a {reserva.Estado} exitosamente." });
    }

    // DELETE: api/Reservas/5 (Baja lógica mediante estado)
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> CancelarReserva(int id)
    {
        var reserva = await _context.Reservas.FindAsync(id);
        if (reserva is null)
            return NotFound(new { Mensaje = $"Reserva con ID {id} no encontrada." });

        reserva.Estado = EstadoReserva.Cancelada;
        await _context.SaveChangesAsync();

        return NoContent();
    }
}