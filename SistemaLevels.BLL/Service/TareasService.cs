using SistemaLevels.DAL.Repository;
using SistemaLevels.Models;

namespace SistemaLevels.BLL.Service
{
    public class TareasService : ITareasService
    {
        private readonly ITareasRepository<Tarea> _repo;

        public TareasService(ITareasRepository<Tarea> repo)
        {
            _repo = repo;
        }

        public async Task<bool> Actualizar(Tarea model)
            => await _repo.Actualizar(model);

        public async Task<bool> Eliminar(int id)
            => await _repo.Eliminar(id);

        public async Task<bool> Insertar(Tarea model)
            => await _repo.Insertar(model);

        public async Task<Tarea?> Obtener(int id)
            => await _repo.Obtener(id);

        public async Task<IQueryable<Tarea>> ObtenerTodos()
            => await _repo.ObtenerTodos();
    }
}
