using SistemaLevels.DAL.Repository;
using SistemaLevels.Models;

namespace SistemaLevels.BLL.Service
{
    public class ClientesService : IClientesService
    {
        private readonly IClientesRepository _repo;

        public ClientesService(IClientesRepository repo)
        {
            _repo = repo;
        }

        public Task<bool> Insertar(Cliente model, List<int> productorasIds)
            => _repo.Insertar(model, productorasIds);

        public Task<bool> Actualizar(Cliente model, List<int> productorasIds)
            => _repo.Actualizar(model, productorasIds);

        public Task<bool> Eliminar(int id)
            => _repo.Eliminar(id);

        public Task<Cliente?> Obtener(int id)
            => _repo.Obtener(id);

        public Task<IQueryable<Cliente>> ObtenerTodos()
            => _repo.ObtenerTodos();
    }
}