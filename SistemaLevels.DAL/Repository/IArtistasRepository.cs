using SistemaLevels.Models;

namespace SistemaLevels.DAL.Repository
{
    public interface IArtistasRepository<TEntityModel> where TEntityModel : class
    {
        Task<bool> Eliminar(int id);
        Task<bool> Actualizar(Artista model);
        Task<bool> Insertar(Artista model);

        Task<Artista?> Obtener(int id);
        Task<IQueryable<Artista>> ObtenerTodos();
    }
}
