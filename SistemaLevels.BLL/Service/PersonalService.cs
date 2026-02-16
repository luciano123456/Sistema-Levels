using SistemaLevels.DAL.Repository;
using SistemaLevels.Models;

namespace SistemaLevels.BLL.Service
{
    public class PersonalService : IPersonalService
    {
        private readonly IPersonalRepository<Personal> _repo;

        public PersonalService(IPersonalRepository<Personal> repo)
        {
            _repo = repo;
        }

        public async Task<bool> Actualizar(Personal model)
            => await _repo.Actualizar(model);

        public async Task<bool> Eliminar(int id)
            => await _repo.Eliminar(id);

        public async Task<bool> Insertar(Personal model)
            => await _repo.Insertar(model);

        public async Task<Personal> Obtener(int id)
            => await _repo.Obtener(id);

        public async Task<IQueryable<Personal>> ObtenerTodos()
            => await _repo.ObtenerTodos();
    }
}
