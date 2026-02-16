using SistemaLevels.Models;

namespace SistemaLevels.DAL.Repository
{
    public interface IProductorasRepository<TEntityModel> where TEntityModel : class
    {
        Task<bool> Eliminar(int id);
        Task<bool> Actualizar(Productora model);
        Task<bool> Insertar(Productora model);

        Task<Productora?> Obtener(int id);
        Task<IQueryable<Productora>> ObtenerTodos();
    }
}
