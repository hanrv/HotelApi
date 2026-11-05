using HotelApi.DTOs;

namespace HotelApi.Data.Repositories;

public interface IReservaRepository
{
    Task<IEnumerable<ReservaResponseDTO>> ObtenerTodasAsync();
    Task<ReservaResponseDTO?> ObtenerPorIdAsync(int id);
    Task<ReservaResponseDTO> CrearReservaAsync(CrearReservaDTO dto);
}