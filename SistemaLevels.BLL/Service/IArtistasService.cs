using SistemaLevels.BLL.Common;
using SistemaLevels.Models;

namespace SistemaLevels.BLL.Service
{
    public interface IArtistasService
    {
        Task<ServiceResult> Insertar(Artista model);
        Task<ServiceResult> Actualizar(Artista model);
        Task<ServiceResult> Eliminar(int id);

        Task<Artista?> Obtener(int id);
        Task<IQueryable<Artista>> ObtenerTodos();
    }
}