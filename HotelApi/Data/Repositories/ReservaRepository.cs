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

    public async Task<ReservaResponseDTO?> ObtenerPorIdDetalladoAsync(int id)
    {
        return await _context.Reservas
            .AsNoTracking()
            .Where(r => r.Id == id)
            .Select(r => new ReservaResponseDTO(
                r.Id,
                r.UsuarioId,
                r.Usuario!.NombreCompleto,
                r.FechaCreacion,
                r.Estado.ToString(),
                r.Total,
                r.ReservaDetalles.Select(d => new DetalleHabitacionDTO(
                    d.HabitacionId,
                    d.Habitacion!.Numero,
                    d.Habitacion.Tipo,
                    d.FechaEntrada,
                    d.FechaSalida,
                    d.PrecioUnitarioPorNoche,
                    d.Subtotal
                )).ToList(),
                r.ReservaServicios.Select(s => new DetalleServicioDTO(
                    s.ServicioAdicionalId,
                    s.ServicioAdicional!.Nombre,
                    s.Cantidad,
                    s.PrecioUnitario,
                    s.Subtotal
                )).ToList()
            ))
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<ReservaResponseDTO>> ObtenerPorUsuarioAsync(int usuarioId)
    {
        return await _context.Reservas
            .AsNoTracking()
            .Where(r => r.UsuarioId == usuarioId)
            .OrderByDescending(r => r.FechaCreacion)
            .Select(r => new ReservaResponseDTO(
                r.Id,
                r.UsuarioId,
                r.Usuario!.NombreCompleto,
                r.FechaCreacion,
                r.Estado.ToString(),
                r.Total,
                r.ReservaDetalles.Select(d => new DetalleHabitacionDTO(
                    d.HabitacionId,
                    d.Habitacion!.Numero,
                    d.Habitacion.Tipo,
                    d.FechaEntrada,
                    d.FechaSalida,
                    d.PrecioUnitarioPorNoche,
                    d.Subtotal
                )).ToList(),
                r.ReservaServicios.Select(s => new DetalleServicioDTO(
                    s.ServicioAdicionalId,
                    s.ServicioAdicional!.Nombre,
                    s.Cantidad,
                    s.PrecioUnitario,
                    s.Subtotal
                )).ToList()
            ))
            .ToListAsync();
    }

    public async Task<Reserva> CrearReservaCarritoAsync(CrearReservaDTO dto)
    {
        var usuarioExiste = await _context.Usuarios.AnyAsync(u => u.Id == dto.UsuarioId);
        if (!usuarioExiste)
            throw new KeyNotFoundException($"El usuario con ID {dto.UsuarioId} no existe.");

        using var transaction = await _context.Database.BeginTransactionAsync();

        var reserva = new Reserva
        {
            UsuarioId = dto.UsuarioId,
            FechaCreacion = DateTime.UtcNow,
            Estado = EstadoReserva.Pendiente
        };

        decimal totalAcumulado = 0;

        // 1. Procesamiento de habitaciones del carrito[cite: 1]
        var habitacionIds = dto.Habitaciones.Select(h => h.HabitacionId).Distinct().ToList();
        var habitaciones = await _context.Habitaciones
            .Where(h => habitacionIds.Contains(h.Id) && h.Disponible)
            .ToDictionaryAsync(h => h.Id);

        foreach (var item in dto.Habitaciones)
        {
            if (!habitaciones.TryGetValue(item.HabitacionId, out var hab))
                throw new InvalidOperationException($"La habitación {item.HabitacionId} no existe o no está disponible.");

            if (item.FechaSalida <= item.FechaEntrada)
                throw new ArgumentException($"La fecha de salida debe ser posterior a la de entrada para la habitación {hab.Numero}.");

            // Control de colisión de fechas
            bool enConflicto = await _context.ReservaDetalles
                .AnyAsync(rd => rd.HabitacionId == item.HabitacionId &&
                                rd.Reserva!.Estado != EstadoReserva.Cancelada &&
                                item.FechaEntrada < rd.FechaSalida &&
                                item.FechaSalida > rd.FechaEntrada);

            if (enConflicto)
                throw new InvalidOperationException($"La habitación {hab.Numero} ya se encuentra reservada en las fechas solicitadas.");

            int totalNoches = (item.FechaSalida.Date - item.FechaEntrada.Date).Days;
            decimal subtotal = hab.PrecioPorNoche * totalNoches;

            reserva.ReservaDetalles.Add(new ReservaDetalle
            {
                HabitacionId = hab.Id,
                FechaEntrada = item.FechaEntrada,
                FechaSalida = item.FechaSalida,
                PrecioUnitarioPorNoche = hab.PrecioPorNoche,
                Subtotal = subtotal
            });

            totalAcumulado += subtotal;
        }

        // 2. Procesamiento de servicios opcionales[cite: 1]
        if (dto.Servicios != null && dto.Servicios.Any())
        {
            var servicioIds = dto.Servicios.Select(s => s.ServicioAdicionalId).Distinct().ToList();
            var servicios = await _context.ServiciosAdicionales
                .Where(s => servicioIds.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id);

            foreach (var srvItem in dto.Servicios)
            {
                if (!servicios.TryGetValue(srvItem.ServicioAdicionalId, out var srv))
                    throw new KeyNotFoundException($"El servicio adicional {srvItem.ServicioAdicionalId} no existe.");

                decimal subtotalServicio = srv.Precio * srvItem.Cantidad;

                reserva.ReservaServicios.Add(new ReservaServicio
                {
                    ServicioAdicionalId = srv.Id,
                    Cantidad = srvItem.Cantidad,
                    PrecioUnitario = srv.Precio,
                    Subtotal = subtotalServicio
                });

                totalAcumulado += subtotalServicio;
            }
        }

        reserva.Total = totalAcumulado;

        _context.Reservas.Add(reserva);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return reserva;
    }

    public async Task<bool> ActualizarEstadoAsync(int id, EstadoReserva nuevoEstado)
    {
        var reserva = await _context.Reservas.FindAsync(id);
        if (reserva == null) return false;

        reserva.Estado = nuevoEstado;
        await _context.SaveChangesAsync();
        return true;
    }
}