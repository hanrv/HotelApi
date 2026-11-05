using Microsoft.EntityFrameworkCore;
using HotelApi.DTOs;
using HotelApi.Models;

namespace HotelApi.Data.Repositories;

public class ReservaRepository : IReservaRepository
{
    private readonly ApplicationDbContext _context;

    public ReservaRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ReservaResponseDTO>> ObtenerTodasAsync()
    {
        return await _context.Reservas
            .AsNoTracking()
            .Include(r => r.ReservaDetalles)
                .ThenInclude(d => d.Habitacion)
            .Include(r => r.ReservaServicios)
                .ThenInclude(rs => rs.ServicioAdicional)
            .Select(r => new ReservaResponseDTO
            {
                Id = r.Id,
                UsuarioId = r.UsuarioId,
                FechaCreacion = r.FechaCreacion,
                Estado = r.Estado.ToString(),
                Total = r.Total,
                Habitaciones = r.ReservaDetalles.Select(d => new DetalleHabitacionDTO
                {
                    Id = d.Id,
                    HabitacionId = d.HabitacionId,
                    NumeroHabitacion = d.Habitacion != null ? d.Habitacion.Numero : "N/A",
                    FechaEntrada = d.FechaEntrada,
                    FechaSalida = d.FechaSalida,
                    PrecioUnitarioPorNoche = d.PrecioUnitarioPorNoche,
                    Subtotal = d.Subtotal
                }).ToList(),
                Servicios = r.ReservaServicios.Select(s => new DetalleServicioDTO
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
    }

    public async Task<ReservaResponseDTO?> ObtenerPorIdAsync(int id)
    {
        var r = await _context.Reservas
            .AsNoTracking()
            .Include(res => res.ReservaDetalles)
                .ThenInclude(d => d.Habitacion)
            .Include(res => res.ReservaServicios)
                .ThenInclude(rs => rs.ServicioAdicional)
            .FirstOrDefaultAsync(res => res.Id == id);

        if (r is null)
            return null;

        return new ReservaResponseDTO
        {
            Id = r.Id,
            UsuarioId = r.UsuarioId,
            FechaCreacion = r.FechaCreacion,
            Estado = r.Estado.ToString(),
            Total = r.Total,
            Habitaciones = r.ReservaDetalles.Select(d => new DetalleHabitacionDTO
            {
                Id = d.Id,
                HabitacionId = d.HabitacionId,
                NumeroHabitacion = d.Habitacion != null ? d.Habitacion.Numero : "N/A",
                FechaEntrada = d.FechaEntrada,
                FechaSalida = d.FechaSalida,
                PrecioUnitarioPorNoche = d.PrecioUnitarioPorNoche,
                Subtotal = d.Subtotal
            }).ToList(),
            Servicios = r.ReservaServicios.Select(s => new DetalleServicioDTO
            {
                Id = s.Id,
                ServicioAdicionalId = s.ServicioAdicionalId,
                NombreServicio = s.ServicioAdicional != null ? s.ServicioAdicional.Nombre : "N/A",
                PrecioUnitario = s.PrecioUnitario,
                Cantidad = s.Cantidad,
                Subtotal = s.Subtotal
            }).ToList()
        };
    }

    public async Task<ReservaResponseDTO> CrearReservaAsync(CrearReservaDTO dto)
    {
        var usuarioExiste = await _context.Usuarios.AnyAsync(u => u.Id == dto.UsuarioId);
        if (!usuarioExiste)
            throw new KeyNotFoundException($"El usuario con ID {dto.UsuarioId} no existe.");

        decimal totalCalculado = 0;

        var nuevaReserva = new Reserva
        {
            UsuarioId = dto.UsuarioId,
            FechaCreacion = DateTime.UtcNow,
            Estado = EstadoReserva.Confirmada,
            ReservaDetalles = new List<ReservaDetalle>(),
            ReservaServicios = new List<ReservaServicio>()
        };

        foreach (var item in dto.Habitaciones)
        {
            if (item.FechaSalida <= item.FechaEntrada)
                throw new ArgumentException($"La fecha de salida debe ser mayor a la de entrada para la habitación {item.HabitacionId}.");

            var habitacion = await _context.Habitaciones.FindAsync(item.HabitacionId)
                ?? throw new KeyNotFoundException($"La habitación con ID {item.HabitacionId} no existe.");

            int noches = (item.FechaSalida - item.FechaEntrada).Days;
            if (noches <= 0) noches = 1;

            decimal subtotal = habitacion.PrecioPorNoche * noches;

            nuevaReserva.ReservaDetalles.Add(new ReservaDetalle
            {
                HabitacionId = habitacion.Id,
                FechaEntrada = item.FechaEntrada,
                FechaSalida = item.FechaSalida,
                PrecioUnitarioPorNoche = habitacion.PrecioPorNoche,
                Subtotal = subtotal
            });

            totalCalculado += subtotal;
        }

        if (dto.Servicios != null && dto.Servicios.Any())
        {
            foreach (var srv in dto.Servicios)
            {
                var servicio = await _context.ServiciosAdicionales.FindAsync(srv.ServicioAdicionalId)
                    ?? throw new KeyNotFoundException($"El servicio adicional con ID {srv.ServicioAdicionalId} no existe.");

                decimal subtotal = servicio.Precio * srv.Cantidad;

                nuevaReserva.ReservaServicios.Add(new ReservaServicio
                {
                    ServicioAdicionalId = servicio.Id,
                    PrecioUnitario = servicio.Precio,
                    Cantidad = srv.Cantidad,
                    Subtotal = subtotal
                });

                totalCalculado += subtotal;
            }
        }

        nuevaReserva.Total = totalCalculado;

        _context.Reservas.Add(nuevaReserva);
        await _context.SaveChangesAsync();

        return (await ObtenerPorIdAsync(nuevaReserva.Id))!;
    }
}