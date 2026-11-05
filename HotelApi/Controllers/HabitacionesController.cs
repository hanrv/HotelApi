using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelApi.Data;
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

    // GET: api/Habitaciones (Optimizado con AsNoTracking)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Habitacion>>> GetHabitaciones()
    {
        var habitaciones = await _context.Habitaciones
            .AsNoTracking()
            .Include(h => h.Hotel)
            .ToListAsync();

        return Ok(habitaciones);
    }

    // GET: api/Habitaciones/5 (Optimizado con AsNoTracking)
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Habitacion>> GetHabitacion(int id)
    {
        var habitacion = await _context.Habitaciones
            .AsNoTracking()
            .Include(h => h.Hotel)
            .FirstOrDefaultAsync(h => h.Id == id);

        if (habitacion is null)
            return NotFound(new { Mensaje = $"Habitación con ID {id} no encontrada." });

        return Ok(habitacion);
    }

    // POST: api/Habitaciones
    [HttpPost]
    public async Task<ActionResult<Habitacion>> CrearHabitacion([FromBody] Habitacion habitacion)
    {
        var hotelExiste = await _context.Hoteles.AnyAsync(h => h.Id == habitacion.HotelId);
        if (!hotelExiste)
            return BadRequest(new { Mensaje = $"El hotel con ID {habitacion.HotelId} asignado no existe." });

        _context.Habitaciones.Add(habitacion);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetHabitacion), new { id = habitacion.Id }, habitacion);
    }

    // PUT: api/Habitaciones/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> ActualizarHabitacion(int id, [FromBody] Habitacion habitacion)
    {
        if (id != habitacion.Id)
            return BadRequest(new { Mensaje = "El ID no coincide con el cuerpo enviado." });

        var existe = await _context.Habitaciones.AnyAsync(h => h.Id == id);
        if (!existe)
            return NotFound(new { Mensaje = $"Habitación con ID {id} no encontrada." });

        _context.Entry(habitacion).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw;
        }

        return NoContent();
    }

    // DELETE: api/Habitaciones/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> EliminarHabitacion(int id)
    {
        var habitacion = await _context.Habitaciones.FindAsync(id);
        if (habitacion is null)
            return NotFound(new { Mensaje = $"Habitación con ID {id} no encontrada." });

        _context.Habitaciones.Remove(habitacion);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}