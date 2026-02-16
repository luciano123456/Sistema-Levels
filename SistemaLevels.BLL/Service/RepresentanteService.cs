using SistemaLevels.DAL.Repository;
using SistemaLevels.Models;

namespace SistemaLevels.BLL.Service
{
    public class RepresentantesService : IRepresentantesService
    {
        private readonly IRepresentantesRepository<Representante> _repo;

        public RepresentantesService(IRepresentantesRepository<Representante> repo)
        {
            _repo = repo;
        }

        public async Task<bool> Actualizar(Representante model)
            => await _repo.Actualizar(model);

        public async Task<bool> Eliminar(int id)
            => await _repo.Eliminar(id);

        public async Task<bool> Insertar(Representante model)
            => await _repo.Insertar(model);

        public async Task<Representante> Obtener(int id)
            => await _repo.Obtener(id);

        public async Task<IQueryable<Representante>> ObtenerTodos()
            => await _repo.ObtenerTodos();
    }
}
