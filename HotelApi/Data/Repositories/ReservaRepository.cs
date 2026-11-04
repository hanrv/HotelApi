using HotelApi.DTOs;
using HotelApi.Models;
using Microsoft.EntityFrameworkCore;

public async Task<Reserva> CrearReservaCarritoAsync(CrearReservaDTO dto)
{
    var usuarioExiste = await _context.Usuarios.AnyAsync(u => u.Id == dto.UsuarioId);
    if (!usuarioExiste)
        throw new KeyNotFoundException($"El usuario con ID {dto.UsuarioId} no existe.");

    // Validar colisiones entre los elementos del mismo carrito
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

    foreach (var item in dto.Habitaciones)
    {
        if (!habitaciones.TryGetValue(item.HabitacionId, out var hab))
            throw new InvalidOperationException($"La habitación {item.HabitacionId} no existe o no está disponible.");

        // Corrección del cálculo de noches mínimas
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

    // Servicios adicionales
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