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

        // 1. Validar colisiones entre los elementos dentro del mismo carrito
        var gruposHabitaciones = dto.Habitaciones.GroupBy(h => h.HabitacionId);
        foreach (var grupo in gruposHabitaciones)
        {
            var lista = grupo.OrderBy(g => g.FechaEntrada).ToList();
            for (int i = 0; i < lista.Count - 1; i++)
            {
                if (lista[i].FechaSalida.Date > lista[i + 1].FechaEntrada.Date)
                    throw new InvalidOperationException($"El carrito contiene rangos de fechas superpuestos para la misma habitación (ID {grupo.Key}).");
            }
        }

        using var transaction = await _context.Database.BeginTransactionAsync();

        var reserva = new Reserva
        {
            UsuarioId = dto.UsuarioId,
            FechaCreacion = DateTime.UtcNow,
            Estado = EstadoReserva.Pendiente
        };

        decimal totalAcumulado = 0;

        var habitacionIds = dto.Habitaciones.Select(h => h.HabitacionId).Distinct().ToList();
        var habitaciones = await _context.Habitaciones
            .Where(h => habitacionIds.Contains(h.Id) && h.Disponible)
            .ToDictionaryAsync(h => h.Id);

        // 2. Procesar habitaciones
        foreach (var item in dto.Habitaciones)
        {
            if (!habitaciones.TryGetValue(item.HabitacionId, out var hab))
                throw new InvalidOperationException($"La habitación {item.HabitacionId} no existe o no está disponible.");

            if (item.FechaSalida.Date <= item.FechaEntrada.Date)
                throw new ArgumentException($"La estancia en la habitación {hab.Numero} debe ser de al menos 1 noche (Fecha salida posterior a entrada).");

            bool enConflicto = await _context.ReservaDetalles
                .AnyAsync(rd => rd.HabitacionId == item.HabitacionId &&
                                rd.Reserva!.Estado != EstadoReserva.Cancelada &&
                                item.FechaEntrada < rd.FechaSalida &&
                                item.FechaSalida > rd.FechaEntrada);

            if (enConflicto)
                throw new InvalidOperationException($"La habitación {hab.Numero} ya se encuentra ocupada en esas fechas.");

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

        // 3. Procesar servicios adicionales
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