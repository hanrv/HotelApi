using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelApi.Data;
using HotelApi.Models;

namespace HotelApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HotelesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public HotelesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/Hoteles (Optimizado con AsNoTracking e Include)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Hotel>>> GetHoteles()
    {
        var hoteles = await _context.Hoteles
            .AsNoTracking()
            .Include(h => h.Habitaciones)
            .ToListAsync();

        return Ok(hoteles);
    }

    // GET: api/Hoteles/5 (Optimizado con AsNoTracking)
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Hotel>> GetHotel(int id)
    {
        var hotel = await _context.Hoteles
            .AsNoTracking()
            .Include(h => h.Habitaciones)
            .FirstOrDefaultAsync(h => h.Id == id);

        if (hotel is null)
            return NotFound(new { Mensaje = $"El hotel con ID {id} no existe." });

        return Ok(hotel);
    }

    // POST: api/Hoteles
    [HttpPost]
    public async Task<ActionResult<Hotel>> CrearHotel([FromBody] Hotel hotel)
    {
        _context.Hoteles.Add(hotel);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetHotel), new { id = hotel.Id }, hotel);
    }

    // PUT: api/Hoteles/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> ActualizarHotel(int id, [FromBody] Hotel hotel)
    {
        if (id != hotel.Id)
            return BadRequest(new { Mensaje = "El ID de la ruta no coincide con el cuerpo de la petición." });

        var existe = await _context.Hoteles.AnyAsync(h => h.Id == id);
        if (!existe)
            return NotFound(new { Mensaje = $"Hotel con ID {id} no encontrado." });

        _context.Entry(hotel).State = EntityState.Modified;

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

    // DELETE: api/Hoteles/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> EliminarHotel(int id)
    {
        var hotel = await _context.Hoteles.FindAsync(id);
        if (hotel is null)
            return NotFound(new { Mensaje = $"Hotel con ID {id} no encontrado." });

        _context.Hoteles.Remove(hotel);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}