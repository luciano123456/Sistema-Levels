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

        public async Task<bool> Insertar(Cliente model, List<int> productorasIds)
            => await _repo.Insertar(model, productorasIds);

        public async Task<bool> Actualizar(Cliente model, List<int> productorasIds)
            => await _repo.Actualizar(model, productorasIds);

        public async Task<bool> Eliminar(int id)
            => await _repo.Eliminar(id);

        public async Task<Cliente?> Obtener(int id)
            => await _repo.Obtener(id);

        public async Task<IQueryable<Cliente>> ObtenerTodos()
            => await _repo.ObtenerTodos();
    }
}