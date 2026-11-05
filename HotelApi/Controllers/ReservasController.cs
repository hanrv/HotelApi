using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelApi.Data;
using HotelApi.Data.Repositories;
using HotelApi.DTOs;
using HotelApi.Models;

namespace HotelApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservasController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IReservaRepository _reservaRepo;

    public ReservasController(ApplicationDbContext context, IReservaRepository reservaRepo)
    {
        _context = context;
        _reservaRepo = reservaRepo;
    }

    // GET: api/Reservas (Optimizado con AsNoTracking e Include mediante el repositorio)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReservaResponseDTO>>> GetReservas()
    {
        var reservas = await _reservaRepo.ObtenerTodasAsync();
        return Ok(reservas);
    }

    // GET: api/Reservas/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ReservaResponseDTO>> GetReservaPorId(int id)
    {
        var reserva = await _reservaRepo.ObtenerPorIdAsync(id);
        if (reserva is null)
            return NotFound(new { Mensaje = $"La reserva con ID {id} no fue encontrada." });

        return Ok(reserva);
    }

    // POST: api/Reservas (Creación con lista de habitaciones y servicios)
    [HttpPost]
    public async Task<ActionResult<ReservaResponseDTO>> CrearReserva([FromBody] CrearReservaDTO dto)
    {
        try
        {
            var creada = await _reservaRepo.CrearReservaAsync(dto);
            return CreatedAtAction(nameof(GetReservaPorId), new { id = creada.Id }, creada);
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(new { Mensaje = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Mensaje = ex.Message });
        }
    }

    // PUT: api/Reservas/5 (Actualización completa y recálculo de subtotales)
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

    // PATCH: api/Reservas/5/estado (Actualización parcial del estado)
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

    // DELETE: api/Reservas/5 (Cancelación / Baja lógica)
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