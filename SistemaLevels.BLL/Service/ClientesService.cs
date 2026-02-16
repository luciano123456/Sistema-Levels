using SistemaLevels.DAL.Repository;
using SistemaLevels.Models;

namespace SistemaLevels.BLL.Service
{
    public class ClientesService : IClientesService
    {
        private readonly IClientesRepository<Cliente> _repo;

        public ClientesService(IClientesRepository<Cliente> repo)
        {
            _repo = repo;
        }

        public async Task<bool> Actualizar(Cliente model)
            => await _repo.Actualizar(model);

        public async Task<bool> Eliminar(int id)
            => await _repo.Eliminar(id);

        public async Task<bool> Insertar(Cliente model)
            => await _repo.Insertar(model);

        public async Task<Cliente?> Obtener(int id)
            => await _repo.Obtener(id);

        public async Task<IQueryable<Cliente>> ObtenerTodos()
            => await _repo.ObtenerTodos();
    }
}
