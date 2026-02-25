using SistemaLevels.DAL.Repository;
using SistemaLevels.Models;

namespace SistemaLevels.BLL.Service
{
    public class ProductorasService : IProductorasService
    {
        private readonly IProductorasRepository _repo;

        public ProductorasService(IProductorasRepository repo)
        {
            _repo = repo;
        }

        public Task<bool> Insertar(Productora model, List<int> clientesIds)
            => _repo.Insertar(model, clientesIds);

        public Task<bool> Actualizar(Productora model, List<int> clientesIds)
            => _repo.Actualizar(model, clientesIds);

        public Task<bool> Eliminar(int id)
            => _repo.Eliminar(id);

        public Task<Productora?> Obtener(int id)
            => _repo.Obtener(id);

        public Task<IQueryable<Productora>> ObtenerTodos()
            => _repo.ObtenerTodos();

        public Task<List<int>> ObtenerClientesAsociadosAutomaticos(int idProductora)
            => _repo.ObtenerClientesAsociadosAutomaticos(idProductora);
    }
}