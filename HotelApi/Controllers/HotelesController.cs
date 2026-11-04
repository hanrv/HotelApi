using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelApi.Data;
using HotelApi.DTOs;
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

    // GET: api/Hoteles?ciudad=Lima
    [HttpGet]
    public async Task<ActionResult<IEnumerable<HotelResponseDTO>>> GetAll([FromQuery] string? ciudad)
    {
        var query = _context.Hoteles
            .AsNoTracking()
            .Where(h => h.Activo);

        if (!string.IsNullOrWhiteSpace(ciudad))
        {
            query = query.Where(h => h.Ciudad.ToLower() == ciudad.ToLower());
        }

        var hoteles = await query.Select(h => new HotelResponseDTO(
            h.Id,
            h.Nombre,
            h.Direccion,
            h.Ciudad,
            h.Estrellas,
            h.Activo,
            h.Habitaciones.Count
        )).ToListAsync();

        return Ok(hoteles);
    }

    // GET: api/Hoteles/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<HotelResponseDTO>> GetById(int id)
    {
        var hotel = await _context.Hoteles
            .AsNoTracking()
            .Where(h => h.Id == id && h.Activo)
            .Select(h => new HotelResponseDTO(
                h.Id,
                h.Nombre,
                h.Direccion,
                h.Ciudad,
                h.Estrellas,
                h.Activo,
                h.Habitaciones.Count
            ))
            .FirstOrDefaultAsync();

        if (hotel == null)
            return NotFound(new { mensaje = $"El hotel con ID {id} no existe." });

        return Ok(hotel);
    }

    // POST: api/Hoteles
    [HttpPost]
    public async Task<ActionResult<HotelResponseDTO>> Create([FromBody] HotelCreateUpdateDTO dto)
    {
        var hotel = new Hotel
        {
            Nombre = dto.Nombre,
            Direccion = dto.Direccion,
            Ciudad = dto.Ciudad,
            Estrellas = dto.Estrellas,
            Activo = true
        };

        _context.Hoteles.Add(hotel);
        await _context.SaveChangesAsync();

        var response = new HotelResponseDTO(hotel.Id, hotel.Nombre, hotel.Direccion, hotel.Ciudad, hotel.Estrellas, true, 0);
        return CreatedAtAction(nameof(GetById), new { id = hotel.Id }, response);
    }

    // PUT: api/Hoteles/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] HotelCreateUpdateDTO dto)
    {
        var hotel = await _context.Hoteles.FindAsync(id);
        if (hotel == null || !hotel.Activo)
            return NotFound(new { mensaje = $"El hotel con ID {id} no fue encontrado." });

        hotel.Nombre = dto.Nombre;
        hotel.Direccion = dto.Direccion;
        hotel.Ciudad = dto.Ciudad;
        hotel.Estrellas = dto.Estrellas;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: api/Hoteles/5 (Borrado Lógico)
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var hotel = await _context.Hoteles.FindAsync(id);
        if (hotel == null)
            return NotFound(new { mensaje = $"El hotel con ID {id} no existe." });

        hotel.Activo = false;
        await _context.SaveChangesAsync();
        return NoContent();
    }
}