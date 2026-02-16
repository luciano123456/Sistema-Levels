using SistemaLevels.Models;

namespace SistemaLevels.DAL.Repository
{
    public interface IClientesRepository<TEntityModel> where TEntityModel : class
    {
        Task<bool> Eliminar(int id);
        Task<bool> Actualizar(Cliente model);
        Task<bool> Insertar(Cliente model);

        Task<Cliente?> Obtener(int id);
        Task<IQueryable<Cliente>> ObtenerTodos();
    }
}
