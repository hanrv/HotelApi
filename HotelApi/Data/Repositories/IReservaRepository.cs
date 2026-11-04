using HotelApi.DTOs;
using HotelApi.Models;

namespace HotelApi.Data.Repositories;

public interface IReservaRepository
{
    Task<ReservaResponseDTO?> ObtenerPorIdDetalladoAsync(int id);
    Task<IEnumerable<ReservaResponseDTO>> ObtenerPorUsuarioAsync(int usuarioId);
    Task<Reserva> CrearReservaCarritoAsync(CrearReservaDTO dto);
    Task<bool> ActualizarEstadoAsync(int id, EstadoReserva nuevoEstado);
}