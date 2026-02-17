using SistemaLevels.DAL.Repository;
using SistemaLevels.Models;

namespace SistemaLevels.BLL.Service
{
    public class PersonalService : IPersonalService
    {
        private readonly IPersonalRepository _repo;

        public PersonalService(IPersonalRepository repo)
        {
            _repo = repo;
        }

        public Task<bool> Eliminar(int id) => _repo.Eliminar(id);

        public Task<bool> Insertar(Personal model, List<int> rolesIds, List<int> artistasIds)
            => _repo.Insertar(model, rolesIds, artistasIds);

        public Task<bool> Actualizar(Personal model, List<int> rolesIds, List<int> artistasIds)
            => _repo.Actualizar(model, rolesIds, artistasIds);

        public Task<Personal?> Obtener(int id) => _repo.Obtener(id);

        public Task<IQueryable<Personal>> ObtenerTodos() => _repo.ObtenerTodos();

        public Task<List<int>> ObtenerRolesIds(int idPersonal) => _repo.ObtenerRolesIds(idPersonal);

        public Task<List<int>> ObtenerArtistasIds(int idPersonal) => _repo.ObtenerArtistasIds(idPersonal);
    }
}
