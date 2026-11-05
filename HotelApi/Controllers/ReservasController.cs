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

    // =========================================================================
    // 1. GET: api/Reservas (Lectura masiva optimizada con AsNoTracking e Include)
    // =========================================================================
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReservaLecturaDTO>>> GetReservas()
    {
        // Optimización ORM: .AsNoTracking() evita la sobrecarga del Change Tracker
        var reservas = await _context.Reservas
            .AsNoTracking()
            .Include(r => r.Detalles)
                .ThenInclude(d => d.Habitacion)
            .Include(r => r.ReservaServicios)
                .ThenInclude(rs => rs.ServicioAdicional)
            .Select(r => new ReservaLecturaDTO
            {
                Id = r.Id,
                UsuarioId = r.UsuarioId,
                FechaReserva = r.FechaReserva,
                FechaInicio = r.FechaInicio,
                FechaFin = r.FechaFin,
                Estado = r.Estado,
                Total = r.Total,
                Detalles = r.Detalles.Select(d => new ReservaDetalleLecturaDTO
                {
                    Id = d.Id,
                    HabitacionId = d.HabitacionId,
                    NumeroHabitacion = d.Habitacion != null ? d.Habitacion.Numero : "N/A",
                    PrecioNoche = d.PrecioUnitario,
                    CantidadNoches = (r.FechaFin - r.FechaInicio).Days > 0 ? (r.FechaFin - r.FechaInicio).Days : 1
                }).ToList(),
                Servicios = r.ReservaServicios.Select(s => new ReservaServicioLecturaDTO
                {
                    Id = s.Id,
                    ServicioAdicionalId = s.ServicioAdicionalId,
                    NombreServicio = s.ServicioAdicional != null ? s.ServicioAdicional.Nombre : "N/A",
                    PrecioUnitario = s.PrecioUnitario,
                    Cantidad = s.Cantidad
                }).ToList()
            })
            .ToListAsync();

        return Ok(reservas);
    }

    // =========================================================================
    // 2. GET: api/Reservas/{id} (Lectura puntual optimizada)
    // =========================================================================
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ReservaLecturaDTO>> GetReservaPorId(int id)
    {
        var reserva = await _context.Reservas
            .AsNoTracking()
            .Include(r => r.Detalles)
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
            FechaReserva = reserva.FechaReserva,
            FechaInicio = reserva.FechaInicio,
            FechaFin = reserva.FechaFin,
            Estado = reserva.Estado,
            Total = reserva.Total,
            Detalles = reserva.Detalles.Select(d => new ReservaDetalleLecturaDTO
            {
                Id = d.Id,
                HabitacionId = d.HabitacionId,
                NumeroHabitacion = d.Habitacion != null ? d.Habitacion.Numero : "N/A",
                PrecioNoche = d.PrecioUnitario,
                CantidadNoches = (reserva.FechaFin - reserva.FechaInicio).Days > 0 ? (reserva.FechaFin - reserva.FechaInicio).Days : 1
            }).ToList(),
            Servicios = reserva.ReservaServicios.Select(s => new ReservaServicioLecturaDTO
            {
                Id = s.Id,
                ServicioAdicionalId = s.ServicioAdicionalId,
                NombreServicio = s.ServicioAdicional != null ? s.ServicioAdicional.Nombre : "N/A",
                PrecioUnitario = s.PrecioUnitario,
                Cantidad = s.Cantidad
            }).ToList()
        };

        return Ok(dto);
    }

    // =========================================================================
    // 3. POST: api/Reservas (Creación con detalles como carrito de compras)
    // =========================================================================
    [HttpPost]
    public async Task<ActionResult<ReservaLecturaDTO>> CrearReserva([FromBody] ReservaCreateDTO dto)
    {
        if (dto.FechaFin <= dto.FechaInicio)
            return BadRequest(new { Mensaje = "La fecha de fin debe ser posterior a la fecha de inicio." });

        // Validar que el usuario exista
        var usuarioExiste = await _context.Usuarios.AnyAsync(u => u.Id == dto.UsuarioId);
        if (!usuarioExiste)
            return BadRequest(new { Mensaje = $"El usuario con ID {dto.UsuarioId} no existe." });

        int noches = (dto.FechaFin - dto.FechaInicio).Days;
        decimal totalCalculado = 0;

        var nuevaReserva = new Reserva
        {
            UsuarioId = dto.UsuarioId,
            FechaReserva = DateTime.UtcNow,
            FechaInicio = dto.FechaInicio,
            FechaFin = dto.FechaFin,
            Estado = "CONFIRMADA",
            Detalles = new List<ReservaDetalle>(),
            ReservaServicios = new List<ReservaServicio>()
        };

        // Procesar lista de habitaciones (Detalles)
        foreach (var detalleDto in dto.Detalles)
        {
            var habitacion = await _context.Habitaciones.FindAsync(detalleDto.HabitacionId);
            if (habitacion is null)
                return BadRequest(new { Mensaje = $"La habitación {detalleDto.HabitacionId} no existe." });

            var detalle = new ReservaDetalle
            {
                HabitacionId = habitacion.Id,
                PrecioUnitario = habitacion.PrecioNoche,
                Subtotal = habitacion.PrecioNoche * noches
            };

            totalCalculado += detalle.Subtotal;
            nuevaReserva.Detalles.Add(detalle);
        }

        // Procesar lista de servicios opcionales
        if (dto.Servicios != null && dto.Servicios.Any())
        {
            foreach (var servicioDto in dto.Servicios)
            {
                var servicio = await _context.ServiciosAdicionales.FindAsync(servicioDto.ServicioAdicionalId);
                if (servicio is null)
                    return BadRequest(new { Mensaje = $"El servicio {servicioDto.ServicioAdicionalId} no existe." });

                var subtotalServicio = servicio.Precio * servicioDto.Cantidad;
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

        return CreatedAtAction(nameof(GetReservaPorId), new { id = nuevaReserva.Id }, new { Mensaje = "Reserva creada con éxito.", ReservaId = nuevaReserva.Id, Total = totalCalculado });
    }

    // =========================================================================
    // 4. PUT: api/Reservas/{id} (Actualización completa)
    // =========================================================================
    [HttpPut("{id:int}")]
    public async Task<IActionResult> ActualizarReserva(int id, [FromBody] ReservaUpdateDTO dto)
    {
        var reserva = await _context.Reservas
            .Include(r => r.Detalles)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (reserva is null)
            return NotFound(new { Mensaje = $"No existe la reserva con ID {id}." });

        if (dto.FechaFin <= dto.FechaInicio)
            return BadRequest(new { Mensaje = "La fecha de fin debe ser posterior a la fecha de inicio." });

        reserva.FechaInicio = dto.FechaInicio;
        reserva.FechaFin = dto.FechaFin;
        reserva.Estado = dto.Estado;

        // Recalcular subtotales de habitaciones si cambiaron las noches
        int noches = (dto.FechaFin - dto.FechaInicio).Days;
        foreach (var detalle in reserva.Detalles)
        {
            detalle.Subtotal = detalle.PrecioUnitario * noches;
        }

        reserva.Total = reserva.Detalles.Sum(d => d.Subtotal);

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // =========================================================================
    // 5. PATCH: api/Reservas/{id}/estado (Actualización parcial de estado)
    // =========================================================================
    [HttpPatch("{id:int}/estado")]
    public async Task<IActionResult> CambiarEstadoReserva(int id, [FromBody] ActualizarEstadoDTO dto)
    {
        var reserva = await _context.Reservas.FindAsync(id);
        if (reserva is null)
            return NotFound(new { Mensaje = $"Reserva con ID {id} no encontrada." });

        reserva.Estado = dto.NuevoEstado.ToUpper();
        await _context.SaveChangesAsync();

        return Ok(new { Mensaje = $"Estado actualizado a {reserva.Estado} exitosamente." });
    }

    // =========================================================================
    // 6. DELETE: api/Reservas/{id} (Cancelación / Eliminación lógica)
    // =========================================================================
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> CancelarReserva(int id)
    {
        var reserva = await _context.Reservas.FindAsync(id);
        if (reserva is null)
            return NotFound(new { Mensaje = $"Reserva con ID {id} no encontrada." });

        // Eliminación lógica cambiando el estado
        reserva.Estado = "CANCELADA";
        await _context.SaveChangesAsync();

        return NoContent();
    }
}