using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelApi.Data;
using HotelApi.DTOs;
using HotelApi.Models;

namespace HotelApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HabitacionesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public HabitacionesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/Habitaciones/hotel/1
    [HttpGet("hotel/{hotelId:int}")]
    public async Task<ActionResult<IEnumerable<HabitacionResponseDTO>>> GetByHotel(int hotelId)
    {
        var habitaciones = await _context.Habitaciones
            .AsNoTracking()
            .Where(h => h.HotelId == hotelId)
            .Select(h => new HabitacionResponseDTO(
                h.Id,
                h.Numero,
                h.Tipo,
                h.PrecioPorNoche,
                h.CapacidadPersonas,
                h.Disponible,
                h.HotelId,
                h.Hotel!.Nombre
            ))
            .ToListAsync();

        return Ok(habitaciones);
    }

    // POST: api/Habitaciones
    [HttpPost]
    public async Task<ActionResult<HabitacionResponseDTO>> Create([FromBody] HabitacionCreateDTO dto)
    {
        var hotelExiste = await _context.Hoteles.AnyAsync(h => h.Id == dto.HotelId && h.Activo);
        if (!hotelExiste)
            return NotFound(new { mensaje = $"El hotel con ID {dto.HotelId} no existe o no está activo." });

        var habitacion = new Habitacion
        {
            Numero = dto.Numero,
            Tipo = dto.Tipo,
            PrecioPorNoche = dto.PrecioPorNoche,
            CapacidadPersonas = dto.CapacidadPersonas,
            HotelId = dto.HotelId,
            Disponible = true
        };

        _context.Habitaciones.Add(habitacion);
        await _context.SaveChangesAsync();

        return Ok(habitacion);
    }

    // PATCH: api/Habitaciones/5/disponibilidad
    [HttpPatch("{id:int}/disponibilidad")]
    public async Task<IActionResult> ToggleDisponibilidad(int id, [FromBody] bool disponible)
    {
        var habitacion = await _context.Habitaciones.FindAsync(id);
        if (habitacion == null)
            return NotFound(new { mensaje = $"Habitación {id} no encontrada." });

        habitacion.Disponible = disponible;
        await _context.SaveChangesAsync();
        return NoContent();
    }
}