using SistemaLevels.Models;

namespace SistemaLevels.BLL.Service
{
    public interface IArtistasService
    {
        Task<bool> Eliminar(int id);
        Task<bool> Actualizar(Artista model);
        Task<bool> Insertar(Artista model);

        Task<Artista> Obtener(int id);
        Task<IQueryable<Artista>> ObtenerTodos();
    }
}
