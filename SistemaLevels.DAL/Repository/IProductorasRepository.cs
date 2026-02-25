using SistemaLevels.Models;

namespace SistemaLevels.DAL.Repository
{
    public interface IProductorasRepository
    {
        Task<bool> Insertar(Productora model, List<int> clientesIds);

        Task<bool> Actualizar(Productora model, List<int> clientesIds);

        Task<bool> Eliminar(int id);

        Task<Productora?> Obtener(int id);

        Task<IQueryable<Productora>> ObtenerTodos();

        Task<List<int>> ObtenerClientesAsociadosAutomaticos(int idProductora);
    }
}