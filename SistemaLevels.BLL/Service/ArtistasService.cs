using SistemaLevels.DAL.Repository;
using SistemaLevels.Models;

namespace SistemaLevels.BLL.Service
{
    public class ArtistasService : IArtistasService
    {
        private readonly IArtistasRepository<Artista> _repo;

        public ArtistasService(IArtistasRepository<Artista> repo)
        {
            _repo = repo;
        }

        public Task<bool> Eliminar(int id) => _repo.Eliminar(id);

        public Task<bool> Actualizar(Artista model) => _repo.Actualizar(model);

        public Task<bool> Insertar(Artista model) => _repo.Insertar(model);

        public Task<Artista> Obtener(int id) => _repo.Obtener(id);

        public Task<IQueryable<Artista>> ObtenerTodos() => _repo.ObtenerTodos();
    }
}
