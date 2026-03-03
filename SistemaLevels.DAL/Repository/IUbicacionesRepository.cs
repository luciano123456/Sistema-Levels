using SistemaLevels.Models;

namespace SistemaLevels.DAL.Repository
{
    public interface IUbicacionesRepository<T> where T : class
    {
        Task<bool> Insertar(T model);

        Task<bool> Actualizar(T model);

        Task<bool> Eliminar(int id);

        Task<T?> Obtener(int id);

        Task<IQueryable<T>> ObtenerTodos();

        Task<T?> BuscarDuplicado(int? idExcluir, string descripcion);
    }
}